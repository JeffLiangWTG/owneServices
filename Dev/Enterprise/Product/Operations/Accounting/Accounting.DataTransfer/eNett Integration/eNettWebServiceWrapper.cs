using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.eNett;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	public class eNettWebServiceWrapper : IeNettWebServiceWrapper
	{
		static readonly TimeSpan DefaultWebServiceTimeout = TimeSpan.FromSeconds(300); // hard code for now

		public eNettWebServiceWrapper()
			: this(CreateNewWebService(), true)
		{
		}

		public eNettWebServiceWrapper(IeNettWebServiceClient service, bool isAutoFactorySave, BusinessObjectFactory factory = null)
		{
			if (factory == null && !isAutoFactorySave)
			{
				throw new ArgumentException($"It is invalid to set '{nameof(isAutoFactorySave)}' false when no external Factory has been passed to '{nameof(factory)}' as it will lead to losing unsaved data");
			}
			if (isAutoFactorySave && factory != null)
			{
				throw new ArgumentException($"'{nameof(factory)}' must be null when '{nameof(isAutoFactorySave)}' is true as it will be ignored.");
			}
			IsAutoFactorySave = isAutoFactorySave;
			Factory = factory ?? new BusinessObjectFactory();
			this.Service = service;
		}

		#region Properties

#if DEBUG
		public bool IsAutoFactorySave_ForTest => IsAutoFactorySave;
		public BusinessObjectFactory Factory_ForTest => Factory;
#endif

		bool IsAutoFactorySave { get; }
		BusinessObjectFactory Factory { get; }
		readonly IeNettWebServiceClient Service;
		readonly string Integrator = "CARGOWISE";
		readonly string IntegratorKey = AccountingConfigurationRegistry.Instance.ENettIntegratorKey.Value;

		#endregion

		public static IeNettWebServiceClient CreateNewWebService()
		{
			var result =
#if DEBUG
 Globals.IsTest && !UseRealWebService_ForTesting ? MockENettWebService.Instance.WebService :
#endif
 new IntegrationService();
#if DEBUG
			if (!Globals.IsTest || UseRealWebService_ForTesting)
#endif
			{
				result.Url = AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.Value;
			}
			result.Timeout = (int)DefaultWebServiceTimeout.TotalMilliseconds;

			return result;
		}

		#region Web Methods Wrappers

		public bool ProcessOfflinePayment(AccTransactionMatchLink matchLink, AccTransactionHeader header)
		{
			return ProcessOfflinePayment(matchLink.AP_Amount, header);
		}

		public bool ProcessOfflinePayment(ZDecimal amount, AccTransactionHeader header)
		{
			bool result = false;
			string responseTextForEdiMessage = string.Empty;

			try
			{
				Response_OfflinePaymentNotification response = Service.OfflinePaymentNotification(
					"",
					Integrator,
					IntegratorKey,
					"1",
					header.AH_Desc,
					header.AH_TransactionNum,
					AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
					header.Header.ENettRegistrationNumber,
					header.AH_TransactionNum,
					header.AH_TransactionNum,
					"",
					false,
					null,
					amount,
					GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency,
					AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);
				result = response.success;

				using (MemoryStream stream = new MemoryStream())
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(response.GetType());
					serializer.Serialize(stream, response);
					stream.Flush();
					responseTextForEdiMessage = StreamConverter.StreamToString(stream);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.OfflinePayment, result, string.Empty, header.AH_GB, header.AH_GE, header.PK);
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.Response, result, responseTextForEdiMessage, header.AH_GB, header.AH_GE, header.PK);
			}

			return result;
		}

		public eNettWebServiceResult ProcessDirectDebitFx(IeNettTransaction header, int quoteID)
		{
			eNettWebServiceResult result = new eNettWebServiceResult();
			result.success = false;

			string payeeAccountNumber = string.Empty;
			string payeeBSB = string.Empty;
			string payeeSwift = string.Empty;

			AccAPAccountDetails apDetails = header.Header.CompanyData.AccountDetailsCollection.GetAutoDirectDebitAccount(header.Currency.RX_Code);
			if (apDetails != null && header.Header.ENettRegistrationNumber.IsEmpty)
			{
				payeeAccountNumber = apDetails.A1_AccountName;
				payeeBSB = apDetails.A1_BankBsb;
				payeeSwift = apDetails.A1_BankSwift;
			}

			string xml = string.Empty;
			try
			{
				xml = SerializePayment(header);
				Response_ProcessDirectDebit response = Service.ProcessDirectDebitFx(
					new ProcessDirectDebitFxDTO(
						integrator: Integrator,
						integratorKey: IntegratorKey,
						version: "1",
						sourceDescription: header.AH_Desc,
						integratorRef: header.AH_TransactionNum,
						fromClient: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
						fromAccountBSB: header.BankAccount.AB_BSB,
						fromAccountNo: header.BankAccount.AB_AccountNum,
						toClient: header.Header.ENettRegistrationNumber,
						toAccountName: payeeAccountNumber,
						toAccountNo: payeeBSB,
						swiftCode: payeeSwift,
						invoiceNo: string.Empty,
						invoiceID: string.Empty,
						amount: header.AH_OSTotalAmount,
						currency: header.Currency.RX_Code,
						sendRemittance: false, // sendRemittance?
						integratorXML: xml,
						lineItems: null, // lineItems
						authorised: true, // autorised?
						adviceEmail: string.Empty, // adviceEmail?
						createdBy: header.Logs.CreatedByUserName,
						notes: string.Empty, // notes?
						paymentDate: header.PostDate.ToDateTime(),
						integrationAuthCode: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode,
						fxQuoteID: quoteID,
						localAmount: header.LocalExTaxAmount + header.LocalTaxAmount,
						localCurrency: GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency,
						fxRate: header.ExchangeRate,
						fxUsername: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.CustomHouseUsername,
						fxPassword: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.CustomHousePassword
					));
				result.success = response.success;
				result.errorCode = response.errorCode;
				result.errorMessage = response.errorMessage;
			}
#if DEBUG
			catch (Exception ex) when (Enterprise.Accounting.Integration.Testing.AccountingTestHelper.IsAssertionFailedError(ex))
			{
				throw;
			}
#endif
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.success = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.ProcessDirectDebit, result.success, xml, header.AH_GB, header.AH_GE, header.PK);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant for ComPay")]
		public GetFxQuoteResult GetFxQuote(OrgHeader toClient, ZString paymentMethod, ZString currency, ZDecimal amount)
		{
			try
			{
				Response_CustomHouseOutboundMessage quoteResult = Service.GetFxQuote(
					Integrator,
					IntegratorKey,
					AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
					toClient.ENettRegistrationNumber,
					AccountingConfigurationRegistry.Instance.ENettRegistration.Value.CustomHouseUsername,
					AccountingConfigurationRegistry.Instance.ENettRegistration.Value.CustomHousePassword,
					"beneficiary",
					"Wire",
					currency,
					amount,
					AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);

				ZDecimal localAmount = quoteResult.amount;
				ZDecimal rate = Env.CurrentCompany.ExchangeRate.GetRate(localAmount, amount);

				GetFxQuoteResult result = new GetFxQuoteResult();
				result.Success = quoteResult.success;
				result.QuoteID = quoteResult.quoteID;
				result.LocalAmount = localAmount;
				result.Rate = rate;
				result.Message = quoteResult.result;
				return result;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return new GetFxQuoteResult { Success = false };
			}
		}

#if DEBUG

		public GetFxQuoteResult GetFxQuote(OrgHeader toClient, ZString paymentMethod, ZString currency, ZDecimal amount, ZDecimal testModifier)
		{
			GetFxQuoteResult result = GetFxQuote(toClient, paymentMethod, currency, amount);
			result.Rate += testModifier;
			result.LocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(amount, result.Rate);
			return result;
		}

#endif

		public eNettResponseWithMessages GetContainerStorageFee(BusinessObject storageFeeInvoice)
		{
			StorageFeeInvoicePayment invoice = (StorageFeeInvoicePayment)storageFeeInvoice;
			return GetContainerStorageFee(Service, invoice);
		}

		public eNettResponseWithMessages GetContainerStorageFee(IeNettWebServiceClient eNettService, StorageFeeInvoicePayment invoice)
		{
			Argument.NotNull(invoice, nameof(invoice));
			Argument.NotNullOrEmpty(invoice.ContainerNumber, $"{nameof(invoice)}.{nameof(invoice.ContainerNumber)}");
			Argument.NotNullOrEmpty(invoice.PortCode, $"{nameof(invoice)}.{nameof(invoice.PortCode)}");
			if (!invoice.PickupDate.IsValid)
			{
				throw new ArgumentException("Invoice.PickupDate cannot be invalid", $"{nameof(invoice)}.{nameof(invoice.PickupDate)}");
			}

			if (invoice.PickupDate.IsEmpty)
			{
				throw new ArgumentException("Invoice.PickupDate cannot be empty", $"{nameof(invoice)}.{nameof(invoice.PickupDate)}");
			}

			bool success = false;
			List<string> messages = new List<string>();

			try
			{
				Response_StorageCalculation webServiceResponse = eNettService.StorageCalculation(Integrator,
					IntegratorKey, invoice.PortCode, invoice.ContainerNumber, invoice.PickupDate.ToDateTime());

				invoice.StorageCharges = webServiceResponse.amount;
				success = webServiceResponse.success;

				if (webServiceResponse.amount == 0)
				{
					messages.Add(Res.GetString("5ae55e6d-0c20-4dc5-86e1-7db042fe0162", "Unable to retrieve storage charges for this container."));
				}

				if (!string.IsNullOrEmpty(webServiceResponse.errorMessage))
				{
					messages.Add(webServiceResponse.errorMessage.Replace(@"\n", System.Environment.NewLine));
				}
			}
			catch (NullReferenceException) { messages.Add(Res.GetString("c40788b7-2bed-4ced-a91e-f943cf2ae895", "An error occurred when connecting to the eNett Web Service")); }
			catch (WebException) { messages.Add(Res.GetString("c40788b7-2bed-4ced-a91e-f943cf2ae895", "An error occurred when connecting to the eNett Web Service")); }
			catch (SocketException) { messages.Add(Res.GetString("c40788b7-2bed-4ced-a91e-f943cf2ae895", "An error occurred when connecting to the eNett Web Service")); }
			catch (InvalidOperationException) { messages.Add(Res.GetString("c40788b7-2bed-4ced-a91e-f943cf2ae895", "An error occurred when connecting to the eNett Web Service")); }

			return new eNettResponseWithMessages(success, messages.ToArray());
		}

		public ZString DisplayClientList(BusinessObjectCollection comPayRegisteredOrganisations)
		{
			return DisplayClientList(Service, comPayRegisteredOrganisations);
		}

		public ZString DisplayClientList(IeNettWebServiceClient eNettService, BusinessObjectCollection comPayRegisteredOrganisations)
		{
			comPayRegisteredOrganisations.RemoveAndDeleteAll();

			bool result = false;
			Response_GetClientList[] response = null;
			ZString errorMessageResult = ZString.Empty;

			if (AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty || !AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsNumbersOnlyOrEmpty)
			{
				errorMessageResult = Res.GetString("6c5ee5e2-da84-4a33-adc7-6d3ef30c6576", "ComPay Registration Code is invalid.");
			}
			else
			{
				try
				{
					response = eNettService.DisplayClientList(Integrator, IntegratorKey, Convert.ToInt32(AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode), "");
					result = true;
				}
				catch (WebException) { }
				catch (SocketException) { }
				catch (InvalidOperationException) { }

				if (!result)
				{
					errorMessageResult = Res.GetString("a969c542-aa53-4e7b-b486-b025d00ce1c2", "There was a problem connecting to the ComPay web service. Please try again later.");
				}
				else
				{
					if (response != null && response.Length > 0)
					{
						DynamicBusinessObjectCollection headers = new DynamicBusinessObjectCollection(Factory);
						ZSqlParameterCollection @params = new ZSqlParameterCollection();
						@params.Add(ZSqlParameter.New("@AUCode", Core.Constants.CountryCodes.Australia, RefCountrySchema.RN_Code));
						@params.Add(ZSqlParameter.New("@ENECode", OrgCusCode.CodeTypes.eNettRegistrationNumber, OrgCusCodeSchema.OK_CodeType));

						headers.Load(
							"SELECT " + OrgHeaderSchema.Constants.PK + ", " + OrgHeaderSchema.Constants.OH_Code + ", " + OrgCusCodeSchema.Constants.OK_CustomsRegNo +
							" FROM " + OrgCusCodeSchema.Constants.SqlSchemaName + "." + OrgCusCodeSchema.Constants.TableName +
							" INNER JOIN " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ON " + OrgCusCodeSchema.Constants.OK_OH + " = " + OrgHeaderSchema.Constants.PK +
							" WHERE " + OrgCusCodeSchema.Constants.OK_CodeType + " = @ENECode AND " + OrgCusCodeSchema.Constants.OK_RN_NKCodeCountry + " = @AUCode" +
							" ORDER BY " + OrgHeaderSchema.Constants.OH_Code
						, @params);

						foreach (Response_GetClientList client in response)
						{
							if (client.ECN != 0 && !string.IsNullOrEmpty(client.TerminalCode))
							{
								ComPayRegisteredOrganisation line = (ComPayRegisteredOrganisation)comPayRegisteredOrganisations.AddNew();
								line.ClientName = client.ClientName;
								line.ABN = client.ABN;
								line.ECN = client.ECN;
								line.RegistrationDate = client.RegistrationDate;
								line.Address1 = client.Address1;
								line.Address2 = client.Address2;
								line.Suburb = client.Suburb;
								line.State = client.State;
								line.Postcode = client.Postcode;
								line.Country = client.Country;
								line.Phone = client.Phone;
								line.Fax = client.Fax;
								line.TerminalCode = client.TerminalCode;

								var relatedOrganisationsBuilder = new ZStringBuilder();
								const string separator = ", ";
								const string ellipsis = "...";

								foreach (DynamicBusinessObject header in headers)
								{
									if ((ZString)header[OrgCusCodeSchema.Constants.OK_CustomsRegNo] == client.ECN.ToString())
									{
										line.OrgHeaderPKs.Add((ZGuid)header[OrgHeaderSchema.Constants.PK]);
										relatedOrganisationsBuilder.Append((ZString)header[OrgHeaderSchema.Constants.OH_Code]);
									}
								}

								var relatedOrganisations = relatedOrganisationsBuilder.ToStringWithDelimiterBetweenAppends(separator);

								if (relatedOrganisations.Length > line.RelatedOrganisationsInfo.MaxLength)
								{
									relatedOrganisations = relatedOrganisations.Substring(0, line.RelatedOrganisationsInfo.MaxLength - ellipsis.Length) + ellipsis;
								}

								line.RelatedOrganisations = relatedOrganisations;
							}
						}
					}
				}
			}

			return errorMessageResult;
		}

		#endregion

		#region Implementation

		void InsertMessageIntoDatabase(string messageSubType, bool isSuccess, string xml, ZGuid branchPK, ZGuid departmentPK, ZGuid transactionPK)
		{
			EDIMessage message = Factory.New<eNettEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eNett;
			message.EM_MessageType = "ENE";
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = isSuccess ? EDIMessage.Status.Sent : EDIMessage.Status.Failed;
			message.EM_MessageText = xml;
			message.EM_GB = branchPK;
			message.EM_GE = departmentPK;
			message.EM_LinkTable = AccTransactionHeader.Schema.TableName;
			message.EM_LinkUniqueID = transactionPK;

			if (IsAutoFactorySave)
			{
				Factory.Save();
			}
		}

		#region Test

#if DEBUG

		public static bool UseRealWebService_ForTesting
		{
			get { return useRealWebService_ForTesting; }
			set { useRealWebService_ForTesting = value; }
		}

		[ThreadStatic]
		static bool useRealWebService_ForTesting;

#endif

		#endregion

		#endregion

		public bool ProcessDirectDebit(TransactionHeader header)
		{
			return ProcessDirectDebit(Service, header);
		}

		bool ProcessDirectDebit(IeNettWebServiceClient eNettService, TransactionHeader header)
		{
			bool result = false;
			string responseTextForEdiMessage = string.Empty;

			string payLoad = SerializePayment(header);

			Response_ProcessDirectDebit response = null;
			try
			{
				if (eNettHelper.IsOrganisationeNettRegistered(header.Header))
				{
					response = eNettService.ProcessDirectDebit(
						new ProcessDirectDebitDTO(
							integrator: Integrator,
							integratorKey: IntegratorKey,
							version: "1",
							sourceDescription: header.AH_Desc,
							integratorRef: header.AH_TransactionNum,
							fromClient: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
							fromAccountBSB: header.BankAccount.AB_BSB,
							fromAccountNo: header.BankAccount.AB_AccountNum,
							toClient: header.Header.ENettRegistrationNumber,
							toAccountName: "",
							toAccountBSB: "",
							toAccountNo: "",
							invoiceNo: "",
							invoiceID: "",
							amount: header.AH_OSTotalAmount,
							currency: header.AH_RX_NKTransactionCurrency,
							sendRemittance: true,
							integratorXML: payLoad,
							lineItems: null,
							authorised: true,
							adviceCCEmail: "",
							createdBy: "",
							notes: "",
							paymentDate: header.AH_InvoiceDate.ToDateTime(),
							integrationAuthCode: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode
						)
					);
					result = response.success;
				}
				else if (eNettHelper.DoesOrgHaveeNettDDRAccount((header.Header)))
				{
					AccAPAccountDetails accountDetails = eNettHelper.GetEnettBankAccountDetails(header.Header);
					response = eNettService.ProcessDirectDebit(
						new ProcessDirectDebitDTO(
							integrator: Integrator,
							integratorKey: IntegratorKey,
							version: "1",
							sourceDescription: header.AH_Desc,
							integratorRef: header.AH_TransactionNum,
							fromClient: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
							fromAccountBSB: header.BankAccount.AB_BSB,
							fromAccountNo: header.BankAccount.AB_AccountNum,
							toClient: "",
							toAccountName: accountDetails.A1_BankName,
							toAccountBSB: accountDetails.A1_BankBsb,
							toAccountNo: accountDetails.A1_BankAccount,
							invoiceNo: "",
							invoiceID: "",
							amount: header.AH_OSTotalAmount,
							currency: header.AH_RX_NKTransactionCurrency,
							sendRemittance: true,
							integratorXML: payLoad,
							lineItems: null,
							authorised: true,
							adviceCCEmail: "",
							createdBy: "",
							notes: "",
							paymentDate: header.AH_InvoiceDate.ToDateTime(),
							integrationAuthCode: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode
						)
					);
					result = response.success;
				}

				using (MemoryStream stream = new MemoryStream())
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(response.GetType());
					serializer.Serialize(stream, response);
					stream.Flush();
					responseTextForEdiMessage = StreamConverter.StreamToString(stream);
				}

				if (response.success && header.AH_ReceiptBatchNo.IsEmpty && response.batchDate != null)
				{
					Payment headerAsPayment = header as Payment;
					eNettPaymentBatchManager manager = new eNettPaymentBatchManager(header.Factory);
					manager.AddToExistingBatchOrCreateNewBatch(headerAsPayment, response.batchDate, true);
				}

				if (!result)
				{
					new eNettWebServiceFailedEmail("ProcessDirectDebit", response.processedDateTime, response.errorCode, response.errorMessage, IsAutoFactorySave, IsAutoFactorySave ? null : Factory).Send();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.ProcessDirectDebit, result, payLoad, header.AH_GB, header.AH_GE, header.PK);
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.Response, result, responseTextForEdiMessage, header.AH_GB, header.AH_GE, header.PK);
			}

			return result;
		}

		public eNettWebServiceResult ProcessCreditCard(IeNettTransaction header, string cardSecurityCode)
		{
			eNettWebServiceResult result = new eNettWebServiceResult();
			result.success = false;
			string responseTextForEdiMessage = string.Empty;

			string payLoad = SerializePayment(header);

			Response_ProcessCreditCard response = null;
			try
			{
				TwoWayEncoder encoder = new TwoWayEncoder(header.BankAccount.PK.ToGuid());
				string creditCardNumber = encoder.Decrypt(header.BankAccount.AB_DebitCreditCardNumber);
				response = Service.ProcessCreditCard(
					new ProcessCreditCardDTO(
						integrator: Integrator,
						integratorKey: IntegratorKey,
						version: "1",
						sourceDescription: header.AH_Desc,
						integratorRef: header.AH_TransactionNum,
						fromClient: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
						toClient: header.Header.ENettRegistrationNumber,
						invoiceNo: string.Empty,
						invoiceID: string.Empty,
						amount: header.AH_OSTotalAmount,
						currency: header.Currency.RX_Code,
						cardNumber: creditCardNumber,
						cardholderName: header.BankAccount.AB_DebitCreditCardName,
						cardExpiryDate: header.BankAccount.AB_DebitCreditCardExpiry,
						cardSecurityCode: cardSecurityCode,
						sendRemittance: true,
						integratorXML: payLoad,
						lineItems: null,
						adviceCCEmail: string.Empty,
						createdBy: header.Logs.CreatedByUserName,
						integrationAuthCode: AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode
					)
				);
				result.success = response.success;

				using (MemoryStream stream = new MemoryStream())
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(response.GetType());
					serializer.Serialize(stream, response);
					stream.Flush();
					responseTextForEdiMessage = StreamConverter.StreamToString(stream);
				}

				if (!result.success)
				{
					new eNettWebServiceFailedEmail("ProcessCreditCard", response.processedDateTime, response.errorCode, response.errorMessage, IsAutoFactorySave, IsAutoFactorySave ? null : Factory).Send();
					result.errorCode = response.errorCode;
					result.errorMessage = response.errorMessage;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.success = false;
			}
			finally
			{
				InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.ProcessCreditCard, result.success, string.Empty, header.AH_GB, header.AH_GE, header.PK);
			}
			return result;
		}

		string SerializePayment(TransactionHeader header)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ARAPPaymentDataAdapter dataAdapter = new ARAPPaymentDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

				serializer.ExportXmlData(stream, dataAdapter, new BusinessObject[] { header }, new ValueObjectExportContext(new NotificationBuffer()));

				stream.Flush();

				string result = StreamConverter.StreamToString(stream);
#if DEBUG
				result = result.Replace(User.SupportUserName, "username");
#endif
				return result;
			}
		}

		string SerializePayment(IeNettTransaction header)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				ARAPPaymentDataAdapter dataAdapter = new ARAPPaymentDataAdapter();
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);

				serializer.ExportXmlData(stream, dataAdapter, new BusinessObject[] { (BusinessObject)header }, new ValueObjectExportContext(new NotificationBuffer()));

				stream.Flush();

				string result = StreamConverter.StreamToString(stream);
#if DEBUG
				result = result.Replace(User.SupportUserName, "username");
#endif
				return result;
			}
		}
	}
}
