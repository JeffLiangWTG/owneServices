using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Xml;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.com.enett991
{
	/*
	We cannot possibly mock functions with >16 arguments
	so we create a DTO to use instead of all the arguments
	*/
	public struct ProcessDirectDebitDTO
	{
		public readonly string integrator;
		public readonly string integratorKey;
		public readonly string version;
		public readonly string sourceDescription;
		public readonly string integratorRef;
		public readonly string fromClient;
		public readonly string fromAccountBSB;
		public readonly string fromAccountNo;
		public readonly string toClient;
		public readonly string toAccountName;
		public readonly string toAccountBSB;
		public readonly string toAccountNo;
		public readonly string invoiceNo;
		public readonly string invoiceID;
		public readonly decimal amount;
		public readonly string currency;
		public readonly bool sendRemittance;
		public readonly string integratorXML;
		public readonly PaymentLineItem[] lineItems;
		public readonly bool authorised;
		public readonly string adviceCCEmail;
		public readonly string createdBy;
		public readonly string notes;
		public readonly DateTime paymentDate;
		public readonly string integrationAuthCode;

		public ProcessDirectDebitDTO(
			string integrator,
			string integratorKey,
			string version,
			string sourceDescription,
			string integratorRef,
			string fromClient,
			string fromAccountBSB,
			string fromAccountNo,
			string toClient,
			string toAccountName,
			string toAccountBSB,
			string toAccountNo,
			string invoiceNo,
			string invoiceID,
			decimal amount,
			string currency,
			bool sendRemittance,
			string integratorXML,
			PaymentLineItem[] lineItems,
			bool authorised,
			string adviceCCEmail,
			string createdBy,
			string notes,
			DateTime paymentDate,
			string integrationAuthCode
		)
		{
			this.integrator = integrator;
			this.integratorKey = integratorKey;
			this.version = version;
			this.sourceDescription = sourceDescription;
			this.integratorRef = integratorRef;
			this.fromClient = fromClient;
			this.fromAccountBSB = fromAccountBSB;
			this.fromAccountNo = fromAccountNo;
			this.toClient = toClient;
			this.toAccountName = toAccountName;
			this.toAccountBSB = toAccountBSB;
			this.toAccountNo = toAccountNo;
			this.invoiceNo = invoiceNo;
			this.invoiceID = invoiceID;
			this.amount = amount;
			this.currency = currency;
			this.sendRemittance = sendRemittance;
			this.integratorXML = integratorXML;
			this.lineItems = lineItems;
			this.authorised = authorised;
			this.adviceCCEmail = adviceCCEmail;
			this.createdBy = createdBy;
			this.notes = notes;
			this.paymentDate = paymentDate;
			this.integrationAuthCode = integrationAuthCode;
		}
	}

	public struct ProcessDirectDebitFxDTO
	{
		public readonly string integrator;
		public readonly string integratorKey;
		public readonly string version;
		public readonly string sourceDescription;
		public readonly string integratorRef;
		public readonly string fromClient;
		public readonly string fromAccountBSB;
		public readonly string fromAccountNo;
		public readonly string toClient;
		public readonly string toAccountName;
		public readonly string toAccountNo;
		public readonly string swiftCode;
		public readonly string invoiceNo;
		public readonly string invoiceID;
		public readonly decimal amount;
		public readonly string currency;
		public readonly bool sendRemittance;
		public readonly string integratorXML;
		public readonly PaymentLineItem[] lineItems;
		public readonly bool authorised;
		public readonly string adviceEmail;
		public readonly string createdBy;
		public readonly string notes;
		public readonly DateTime paymentDate;
		public readonly string integrationAuthCode;
		public readonly int fxQuoteID;
		public readonly decimal localAmount;
		public readonly string localCurrency;
		public readonly decimal fxRate;
		public readonly string fxUsername;
		public readonly string fxPassword;
		public ProcessDirectDebitFxDTO(
			string integrator,
			string integratorKey,
			string version,
			string sourceDescription,
			string integratorRef,
			string fromClient,
			string fromAccountBSB,
			string fromAccountNo,
			string toClient,
			string toAccountName,
			string toAccountNo,
			string swiftCode,
			string invoiceNo,
			string invoiceID,
			decimal amount,
			string currency,
			bool sendRemittance,
			string integratorXML,
			PaymentLineItem[] lineItems,
			bool authorised,
			string adviceEmail,
			string createdBy,
			string notes,
			DateTime paymentDate,
			string integrationAuthCode,
			int fxQuoteID,
			decimal localAmount,
			string localCurrency,
			decimal fxRate,
			string fxUsername,
			string fxPassword
		)
		{
			this.integrator = integrator;
			this.integratorKey = integratorKey;
			this.version = version;
			this.sourceDescription = sourceDescription;
			this.integratorRef = integratorRef;
			this.fromClient = fromClient;
			this.fromAccountBSB = fromAccountBSB;
			this.fromAccountNo = fromAccountNo;
			this.toClient = toClient;
			this.toAccountName = toAccountName;
			this.toAccountNo = toAccountNo;
			this.swiftCode = swiftCode;
			this.invoiceNo = invoiceNo;
			this.invoiceID = invoiceID;
			this.amount = amount;
			this.currency = currency;
			this.sendRemittance = sendRemittance;
			this.integratorXML = integratorXML;
			this.lineItems = lineItems;
			this.authorised = authorised;
			this.adviceEmail = adviceEmail;
			this.createdBy = createdBy;
			this.notes = notes;
			this.paymentDate = paymentDate;
			this.integrationAuthCode = integrationAuthCode;
			this.fxQuoteID = fxQuoteID;
			this.localAmount = localAmount;
			this.localCurrency = localCurrency;
			this.fxRate = fxRate;
			this.fxUsername = fxUsername;
			this.fxPassword = fxPassword;
		}
	}

	public struct ProcessCreditCardDTO
	{
		public readonly string integrator;
		public readonly string integratorKey;
		public readonly string version;
		public readonly string sourceDescription;
		public readonly string integratorRef;
		public readonly string fromClient;
		public readonly string toClient;
		public readonly string invoiceNo;
		public readonly string invoiceID;
		public readonly decimal amount;
		public readonly string currency;
		public readonly string cardNumber;
		public readonly string cardholderName;
		public readonly string cardExpiryDate;
		public readonly string cardSecurityCode;
		public readonly bool sendRemittance;
		public readonly string integratorXML;
		public readonly PaymentLineItem[] lineItems;
		public readonly string adviceCCEmail;
		public readonly string createdBy;
		public readonly string integrationAuthCode;
		public ProcessCreditCardDTO(
			string integrator,
			string integratorKey,
			string version,
			string sourceDescription,
			string integratorRef,
			string fromClient,
			string toClient,
			string invoiceNo,
			string invoiceID,
			decimal amount,
			string currency,
			string cardNumber,
			string cardholderName,
			string cardExpiryDate,
			string cardSecurityCode,
			bool sendRemittance,
			string integratorXML,
			PaymentLineItem[] lineItems,
			string adviceCCEmail,
			string createdBy,
			string integrationAuthCode
		)
		{
			this.integrator = integrator;
			this.integratorKey = integratorKey;
			this.version = version;
			this.sourceDescription = sourceDescription;
			this.integratorRef = integratorRef;
			this.fromClient = fromClient;
			this.toClient = toClient;
			this.invoiceNo = invoiceNo;
			this.invoiceID = invoiceID;
			this.amount = amount;
			this.currency = currency;
			this.cardNumber = cardNumber;
			this.cardholderName = cardholderName;
			this.cardExpiryDate = cardExpiryDate;
			this.cardSecurityCode = cardSecurityCode;
			this.sendRemittance = sendRemittance;
			this.integratorXML = integratorXML;
			this.lineItems = lineItems;
			this.adviceCCEmail = adviceCCEmail;
			this.createdBy = createdBy;
			this.integrationAuthCode = integrationAuthCode;
		}
	}

	public interface IeNettWebServiceClient
	{
		int Timeout { get; set; }
		string Url { get; set; }
		IWebProxy Proxy { get; set; }

		Response_GetNewInvoices GetNewInvoices(
			string integrator,
			string integratorKey,
			string client,
			DateTime lastUpdate,
			int startRecord,
			int maxRecords,
			string integrationAuthCode
		);

		Response_GetNewPayments GetNewPayments(
			string integrator,
			string integratorKey,
			string client,
			DateTime lastUpdate,
			int startRecord,
			int maxRecords,
			string integrationAuthCode
		);

		Response_GetClientList[] DisplayClientList(
			string integrator,
			string integratorKey,
			int eCN,
			string clientName
		);

		Response_GetCancelledInvoices GetCancelledInvoices(
			string integrator,
			string integratorKey,
			string client,
			DateTime lastUpdate,
			int startRecord,
			int maxRecords,
			string integrationAuthCode
		);

		Response_CustomHouseOutboundMessage GetFxQuote(
			string integrator,
			string integratorKey,
			string fromClient,
			string toClient,
			string fxUsername,
			string fxPassword,
			string beneficiaryName,
			string paymentMethod,
			string currency,
			decimal amount,
			string integrationAuthCode
		);

		Response_StorageCalculation StorageCalculation(
			string integrator,
			string integratorKey,
			string port,
			string container,
			DateTime pickupDate
		);

		Response_CreateInvoice CreateInvoice(
			string integrator,
			string integratorKey,
			string version,
			string sourceDescription,
			string integratorRef,
			string fromClient,
			string toClient,
			string broker,
			string invoiceNo,
			string replacingInvoiceNo,
			string invoicePayload,
			string integrationAuthCode
		);

		Response_CancelInvoice CancelInvoice(
			string integrator,
			string integratorKey,
			string version,
			string sourceDescription,
			string integratorRef,
			string fromClient,
			string toClient,
			string invoiceNo,
			string invoiceID,
			string notes,
			string cancelledBy,
			string integrationAuthCode
		);

		Response_OfflinePaymentNotification OfflinePaymentNotification(
			string userID,
			string integrator,
			string integratorKey,
			string version,
			string sourceDescription,
			string integratorRef,
			string fromClient,
			string toClient,
			string invoiceNo,
			string invoiceID,
			string notes,
			bool sendRemittance,
			string integratorXML,
			decimal amount,
			string currency,
			string integrationAuthCode);

		Response_ProcessDirectDebit ProcessDirectDebit(ProcessDirectDebitDTO dto);
		Response_ProcessDirectDebit ProcessDirectDebitFx(ProcessDirectDebitFxDTO dto);
		Response_ProcessCreditCard ProcessCreditCard(ProcessCreditCardDTO dto);
	}

	public partial class IntegrationService : IeNettWebServiceClient
	{
		public Response_ProcessDirectDebit ProcessDirectDebitFx(ProcessDirectDebitFxDTO dto)
		{
			return ProcessDirectDebitFx(
				integrator: dto.integrator,
				integratorKey: dto.integratorKey,
				version: dto.version,
				sourceDescription: dto.sourceDescription,
				integratorRef: dto.integratorRef,
				fromClient: dto.fromClient,
				fromAccountBSB: dto.fromAccountBSB,
				fromAccountNo: dto.fromAccountNo,
				toClient: dto.toClient,
				toAccountName: dto.toAccountName,
				toAccountNo: dto.toAccountNo,
				swiftCode: dto.swiftCode,
				invoiceNo: dto.invoiceNo,
				invoiceID: dto.invoiceID,
				amount: dto.amount,
				currency: dto.currency,
				sendRemittance: dto.sendRemittance,
				integratorXML: dto.integratorXML,
				lineItems: dto.lineItems,
				authorised: dto.authorised,
				adviceEmail: dto.adviceEmail,
				createdBy: dto.createdBy,
				notes: dto.notes,
				paymentDate: dto.paymentDate,
				integrationAuthCode: dto.integrationAuthCode,
				fxQuoteID: dto.fxQuoteID,
				localAmount: dto.localAmount,
				localCurrency: dto.localCurrency,
				fxRate: dto.fxRate,
				fxUsername: dto.fxUsername,
				fxPassword: dto.fxPassword
			);
		}
		public Response_ProcessCreditCard ProcessCreditCard(ProcessCreditCardDTO dto)
		{
			return ProcessCreditCard(
				integrator: dto.integrator,
				integratorKey: dto.integratorKey,
				version: dto.version,
				sourceDescription: dto.sourceDescription,
				integratorRef: dto.integratorRef,
				fromClient: dto.fromClient,
				toClient: dto.toClient,
				invoiceNo: dto.invoiceNo,
				invoiceID: dto.invoiceID,
				amount: dto.amount,
				currency: dto.currency,
				cardNumber: dto.cardNumber,
				cardholderName: dto.cardholderName,
				cardExpiryDate: dto.cardExpiryDate,
				cardSecurityCode: dto.cardSecurityCode,
				sendRemittance: dto.sendRemittance,
				integratorXML: dto.integratorXML,
				lineItems: dto.lineItems,
				adviceCCEmail: dto.adviceCCEmail,
				createdBy: dto.createdBy,
				integrationAuthCode: dto.integrationAuthCode
			);
		}

		public Response_ProcessDirectDebit ProcessDirectDebit(ProcessDirectDebitDTO dto)
		{
			return ProcessDirectDebit(
				integrator: dto.integrator,
				integratorKey: dto.integratorKey,
				version: dto.version,
				sourceDescription: dto.sourceDescription,
				integratorRef: dto.integratorRef,
				fromClient: dto.fromClient,
				fromAccountBSB: dto.fromAccountBSB,
				fromAccountNo: dto.fromAccountNo,
				toClient: dto.toClient,
				toAccountName: dto.toAccountName,
				toAccountBSB: dto.toAccountBSB,
				toAccountNo: dto.toAccountNo,
				invoiceNo: dto.invoiceNo,
				invoiceID: dto.invoiceID,
				amount: dto.amount,
				currency: dto.currency,
				sendRemittance: dto.sendRemittance,
				integratorXML: dto.integratorXML,
				lineItems: dto.lineItems,
				authorised: dto.authorised,
				adviceCCEmail: dto.adviceCCEmail,
				createdBy: dto.createdBy,
				notes: dto.notes,
				paymentDate: dto.paymentDate,
				integrationAuthCode: dto.integrationAuthCode
			);
		}
	}
}

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	internal static class eNettInboundTransactionProcessorExtensions
	{
		internal static DateTime FromUTCToMelbourneLocalTime(this DateTime uTCTime)
		{
			GlbCompany company = GlbCompany.CurrentCompany;

			if (!MelbourneUNLOCOs.ContainsKey(company.PK))
			{
				MelbourneUNLOCOs.Add(company.PK, company.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL"));
			}

			RefUNLOCO melbourneUNLOCO = MelbourneUNLOCOs[company.PK];
			var localtime = melbourneUNLOCO.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(uTCTime);

			if (!AccountingConfigurationRegistry.Instance.IncludeTimeZoneInformationForLastUpdateDate.Value)
			{
				localtime = DateTime.SpecifyKind(localtime, DateTimeKind.Unspecified);
			}

			return localtime;
		}

		static Dictionary<ZGuid, RefUNLOCO> MelbourneUNLOCOs
		{
			get { return fMelbourneUNLOCOs ?? (fMelbourneUNLOCOs = new Dictionary<ZGuid, RefUNLOCO>()); }
		}

		[ThreadStatic]
		static Dictionary<ZGuid, RefUNLOCO> fMelbourneUNLOCOs;
	}

	public class eNettInboundTransactionProcessor : IProcessor
	{
		public eNettInboundTransactionProcessor()
			: this("CARGOWISE")
		{
		}

		internal eNettInboundTransactionProcessor(string integrator)
		{
			this.Integrator = integrator;
		}

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			IeNettWebServiceClient eNettService = GetNewWebService();
			var companies = GetCompanies();

			WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
			eNettService.Proxy = WebRequest.DefaultWebProxy;

			string errorConnectingToENettWebServiceMessage = Res.GetString("ec3aa9e8-f1f6-4d0b-9b2d-05b8a1cf42fe", "Error connecting to eNett web service");

			try
			{
				foreach (GlbCompany company in companies)
				{
					GlbBranch branch = AccountingUtils.GetTopOneActiveBranchOfCompany(company.PK, Factory);
					if (branch != null)
					{
						using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, branch.PK.ToGuid(), Env.CurrentDepartment != null ? Env.CurrentDepartment.PK : Guid.Empty)))
						{
							if (!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty)
							{
								IntegratorKey = AccountingConfigurationRegistry.Instance.ENettIntegratorKey.Value;
								GetNewInvoices(eNettService, token);
								GetNewPayments(eNettService, token);
								GetCancelledInvoices(eNettService, token);
							}
						}
					}
				}
				Factory.Save();
			}
			catch (WebException ex1)
			{
				if (AccountingConfigurationRegistry.Instance.EnableExtraLoggingForENettWebExceptions.Value)
				{
					notifications.AddError(errorConnectingToENettWebServiceMessage + "\r\n" + WebExceptionErrorLogBuilder.BuildErrorLog(ex1));
				}
				else
				{
					notifications.AddError(errorConnectingToENettWebServiceMessage + "\r\n" + ex1.Message);
				}
			}
			catch (SocketException ex1)
			{
				notifications.AddError(errorConnectingToENettWebServiceMessage + "\r\n" + ex1.Message);
			}
			catch (InvalidOperationException ex1)
			{
				notifications.AddError(errorConnectingToENettWebServiceMessage + "\r\n" + ex1.Message);
			}
			catch (SoapException ex1)
			{
				notifications.AddError(errorConnectingToENettWebServiceMessage + "\r\n" + ex1.Message);
			}
			catch (OnSavingCriticalCheckException ex)
			{
				notifications.AddError(errorConnectingToENettWebServiceMessage + "\r\n" + ex.DeveloperErrorMessage);
			}
			catch (Exception ex1) when (!ex1.IsCriticalException())
			{
				ErrorReporter.ReportOnce(Res.GetString("49c3a716-fa87-4989-a0e4-fd28f95f583d", "eNett integration failure"), ex1);
			}
		}

		IEnumerable<GlbCompany> GetCompanies()
		{
			ZDBOnlyQuery companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
			companyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, null);
			ZDBOnlySubQuery registrySubQuery = new ZDBOnlySubQuery(typeof(StmData), StmDataSchema.SD_Owner);
			registrySubQuery.AddToFilter(StmDataSchema.SD_Name, "ENETTREGISTRATION");
			companyQuery.AddSubQuery(registrySubQuery, JoinCondition.And);

			return Factory.Load<GlbCompany>(companyQuery);
		}

		protected virtual IeNettWebServiceClient GetNewWebService()
		{
			IeNettWebServiceClient result = ObjectFactory.Get<IeNettWebServiceClient>();
			result.Url = AccountingConfigurationRegistry.Instance.ENettWebServiceLocation.Value;

			return result;
		}

		#region Properties

		public string Integrator { get; private set; }
		public string IntegratorKey { get; private set; }

		#endregion

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		EDIMessage InsertMessageIntoDatabase(string messageSubType, bool isSuccess, string xml, ZGuid branchPK, ZGuid departmentPK, ZGuid transactionPK)
		{
			EDIMessage message = Factory.New<eNettEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eNett;
			message.EM_MessageType = "ENE";
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = isSuccess ? EDIMessage.Status.Received : EDIMessage.Status.Failed;
			message.EM_MessageText = xml;
			message.EM_GB = branchPK;
			message.EM_GE = departmentPK;
			message.EM_LinkTable = AccTransactionHeader.Schema.TableName;
			message.EM_LinkUniqueID = transactionPK;
			Factory.Save();

			return message;
		}

		#region Get Methods

		void GetNewInvoices(IeNettWebServiceClient eNettService, CancellationToken token)
		{
			Response_GetNewInvoices response = null;
			int currentRecord = 1;
			ZDateTime lastUpdate = AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.Value;
			try
			{
				do
				{
					token.ThrowIfCancellationRequested();
					response = eNettService.GetNewInvoices(
						Integrator,
						IntegratorKey,
						AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
						lastUpdate.ToDateTime().FromUTCToMelbourneLocalTime(),
						currentRecord++,
						maxRecords,
						AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);
					if (response.success && response.updates != null)
					{
						BusinessObjectFactory newFactory = new BusinessObjectFactory();
						XmlInterchange interchange;
						eNettInboundInvoiceDataAdapter adapter = new eNettInboundInvoiceDataAdapter();
						TxnHeader deserializedInvoice = DeserializeXmlInterchangeAndHeader(response.updates as XmlElement, adapter, out interchange);
						NotificationBuffer notify = new NotificationBuffer();
						ValueObjectImportContext context = new ValueObjectImportContext(newFactory, interchange, notify);
						InvoicingBase newInvoice = null;
						if (AccountingConfigurationRegistry.Instance.CreateAPInvoiceOnENettImport.Value)
						{
							newInvoice = ProcessNewAPInvoice(deserializedInvoice, adapter, context, newFactory);
						}
						if (newInvoice == null || !newInvoice.IsInDatabase)
						{
							newFactory = new BusinessObjectFactory();
							deserializedInvoice = DeserializeXmlInterchangeAndHeader(response.updates as XmlElement, adapter, out interchange);
							notify = new NotificationBuffer();
							context = new ValueObjectImportContext(newFactory, interchange, notify);
							newInvoice = ProcessNewInvoice(deserializedInvoice, adapter, context, newFactory);
						}
						EDIMessage message = InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.GetNewInvoices, response.success, response.updates.OuterXml, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, newInvoice.PK);
						SendNotificationEmail(context, deserializedInvoice, Res.GetString("7638234f-4d9e-456e-98a2-18bd995f0e80", "invoice"), message);
					}
				}
				while (response.success && response.additionalPending);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (response != null && response.updates != null)
				{
					InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.GetNewInvoices, false, response.updates.OuterXml, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
				}
				throw;
			}
			finally
			{
				if (response != null)
				{
					AccountingConfigurationRegistry.Instance.ENettGetLastInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, response.receivedDateTime.ToUniversalTime());
				}
			}
			if (!response.success)
			{
				new eNettWebServiceFailedEmail("GetNewInvoices", response.receivedDateTime, response.errorCode, response.errorMessage, true).Send();
			}
		}

		void GetNewPayments(IeNettWebServiceClient eNettService, CancellationToken token)
		{
			Response_GetNewPayments response = null;
			int currentRecord = 1;
			ZDateTime lastUpdate = AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.Value;
			try
			{
				do
				{
					token.ThrowIfCancellationRequested();
					response = eNettService.GetNewPayments(Integrator,
						IntegratorKey,
						AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
						lastUpdate.ToDateTime().FromUTCToMelbourneLocalTime(),
						currentRecord++,
						maxRecords,
						AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);
					if (response.success && response.updates != null)
					{
						BusinessObjectFactory newFactory = new BusinessObjectFactory();
						XmlInterchange interchange;
						eNettInboundPaymentDataAdapter adapter = new eNettInboundPaymentDataAdapter();
						TxnHeader deserializedReceiptHeader = DeserializeXmlInterchangeAndHeader(response.updates as XmlElement, adapter, out interchange);
						NotificationBuffer notify = new NotificationBuffer();
						ValueObjectImportContext context = new ValueObjectImportContext(newFactory, notify);
						ARReceipt newReceipt = ProcessNewPayment(deserializedReceiptHeader, interchange, adapter, context, new ValueObjectExportContext(notify), newFactory);
						EDIMessage message = InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.GetNewPayments, response.success, response.updates.OuterXml, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, newReceipt.PK);
						SendNotificationEmail(context, deserializedReceiptHeader, Res.GetString("b9f05890-4625-4c8a-9c3a-008234b4dcf4", "payment"), message);
					}
				}
				while (response.success && response.additionalPending);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (response != null && response.updates != null)
				{
					InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.GetNewPayments, false, response.updates.OuterXml, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
				}
				throw;
			}
			finally
			{
				if (response != null)
				{
					AccountingConfigurationRegistry.Instance.ENettGetLastPaymentDate.SetValue(
						GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
						response.receivedDateTime.ToUniversalTime());
				}
			}
			if (!response.success)
			{
				new eNettWebServiceFailedEmail("GetNewPayments", response.receivedDateTime, response.errorCode, response.errorMessage, true).Send();
			}
		}

		void GetCancelledInvoices(IeNettWebServiceClient eNettService, CancellationToken token)
		{
			Response_GetCancelledInvoices response = null;
			int currentRecord = 1;
			ZDateTime lastUpdate = AccountingConfigurationRegistry.Instance.ENettGetLastCancelledInvoicesDate.Value;
			try
			{
				do
				{
					token.ThrowIfCancellationRequested();
					response = eNettService.GetCancelledInvoices(Integrator,
									IntegratorKey,
									AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode,
									lastUpdate.ToDateTime().FromUTCToMelbourneLocalTime(),
									currentRecord++,
									maxRecords,
									AccountingConfigurationRegistry.Instance.ENettRegistration.Value.AuthenticationCode);
					if (response != null && response.success && response.updates != null)
					{
						BusinessObjectFactory newFactory = new BusinessObjectFactory();
						XmlInterchange interchange;
						eNettInboundInvoiceDataAdapter adapter = new eNettInboundInvoiceDataAdapter();
						TxnHeader deserializedInvoice = DeserializeXmlInterchangeAndHeader(response.updates as XmlElement, adapter, out interchange);
						NotificationBuffer notify = new NotificationBuffer();
						ValueObjectImportContext context = new ValueObjectImportContext(newFactory, interchange, notify);
						InvoicingBase invoiceToReverse = null;
						invoiceToReverse = GetInvoiceToReverse(deserializedInvoice, adapter, newFactory, interchange, notify);
						if (invoiceToReverse != null)
						{
							ReverseInvoice(invoiceToReverse);
							EDIMessage message = InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.GetNewCancellations, response.success, response.updates.OuterXml, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, invoiceToReverse.PK);
							SendNotificationEmail(context, deserializedInvoice, Res.GetString("7638234f-4d9e-456e-98a2-18bd995f0e80", "invoice"), message);
						}
					}
				}
				while (response != null && response.success && response.additionalPending);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (response != null && response.updates != null)
				{
					InsertMessageIntoDatabase(eNettMessageSubTypeList.Codes.GetNewCancellations, false, response.updates.OuterXml, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, ZGuid.Empty);
				}
				throw;
			}
			finally
			{
				if (response != null)
				{
					AccountingConfigurationRegistry.Instance.ENettGetLastCancelledInvoicesDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, response.receivedDateTime.ToUniversalTime());

					if (!response.success)
					{
						new eNettWebServiceFailedEmail("GetNewInvoiceCancellations", response.receivedDateTime, response.errorCode, response.errorMessage, true).Send();
					}
				}
			}
		}

		InvoicingBase GetInvoiceToReverse(TxnHeader deserializedInvoice, eNettInboundInvoiceDataAdapter adapter, BusinessObjectFactory newFactory, XmlInterchange interchange, NotificationBuffer notify)
		{
			APInvoice tempInvoice = new BusinessObjectFactory().New<APInvoice>();
			adapter.RunExtraValidation = false;
			try
			{
				ValueObjectImportContext context = new ValueObjectImportContext(tempInvoice.Factory, interchange, notify);
				adapter.ImportFromValueObject(tempInvoice, deserializedInvoice, context);
			}
			finally
			{
				adapter.RunExtraValidation = true;
			}

			InvoicingBase result = null;
			if (tempInvoice != null && tempInvoice.AH_OH.IsValid && !tempInvoice.AH_TransactionNum.IsEmpty)
			{
				result = LoadInvoiceToReverse(newFactory, tempInvoice, TransactionTypes.Invoice, LedgerTypes.AccountsPayable);
				if (result == null)
				{
					result = LoadInvoiceToReverse(newFactory, tempInvoice, TransactionTypes.UAInvoice, LedgerTypes.UnapprovedPayableTransactions);
				}
			}
			return result;
		}

		InvoicingBase LoadInvoiceToReverse(BusinessObjectFactory newFactory, InvoicingBase tempInvoice, ZString transactionType, ZString ledger)
		{
			ZQuery invoiceToReverseQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, transactionType);
			invoiceToReverseQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			invoiceToReverseQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, tempInvoice.AH_OH);
			invoiceToReverseQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, tempInvoice.AH_TransactionNum);
			invoiceToReverseQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, tempInvoice.AH_GC);
			return newFactory.LoadTop1<InvoicingBase>(invoiceToReverseQuery);
		}

		void ReverseInvoice(InvoicingBase invoiceToReverse)
		{
			ReversingBase reversing = new ReversingFactory().NewReversing(invoiceToReverse);
			if (reversing != null && reversing.CanReverseTransaction)
			{
				reversing.Reverse();
				if (reversing.ReverseTransaction != null && reversing.ReverseTransaction is TransactionHeader)
				{
					((TransactionHeader)reversing.ReverseTransaction).AH_TransactionNum = invoiceToReverse.AH_TransactionNum;
				}

				if (invoiceToReverse.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					invoiceToReverse.Delete(); //That makes it identical to what happens functionally on the Unapproved Transactions module.
				}

				invoiceToReverse.Factory.Save();
			}
			else
			{
				new eNettGenericNotificationEmail(Res.GetString("5ab6520e-6eb5-4372-99cb-701681d7b4ef", @"There was en error processing {0} {1}.
Message: {2}", invoiceToReverse.HumanReadableName, invoiceToReverse.AH_TransactionNum, Res.GetString("f4ec17d3-28c4-40eb-afd6-702b8ded4922", "Invoice could not be reversed.")));
			}
		}

		#endregion

		#region Process Methods

		APInvoice ProcessNewAPInvoice(TxnHeader deserializedInvoice, eNettInboundInvoiceDataAdapter adapter, ValueObjectImportContext context, BusinessObjectFactory newFactory)
		{
			APInvoice result = newFactory.New<APInvoice>();
			result.IsCreatedByENett = true;
			result.SubmittedFromInvoicingForm = true;
			adapter.ImportFromValueObject(result, deserializedInvoice, context);

			if (!result.IsDeleted)
			{
				result.RunPreSaveValidation();
			}
			else
			{
				return null;
			}
			if (!context.NotificationsHasErrors && !result.HasErrors)
			{
				newFactory.Save();
				result.DocManagerInfo.Save();
			}

			return result;
		}

		UAInvoice ProcessNewInvoice(TxnHeader deserializedInvoice, eNettInboundInvoiceDataAdapter adapter, ValueObjectImportContext context, BusinessObjectFactory newFactory)
		{
			UAInvoice result = newFactory.New<UAInvoice>();
			result.IsCreatedByENett = true;
			adapter.ImportFromValueObject(result, deserializedInvoice, context);

			if (!context.NotificationsHasErrors)
			{
				newFactory.Save();
				result.DocManagerInfo.Save();
			}

			return result;
		}

		[Serializable]
		class eNettInboundTransactionProcessorException : Exception
		{
			public eNettInboundTransactionProcessorException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			public eNettInboundTransactionProcessorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		void ReportError(string message)
		{
			ExceptionReporter.Instance.ReportDeveloperException(message, new eNettInboundTransactionProcessorException(new System.Diagnostics.StackTrace().ToString()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message does not need to be localised")]
		void HandleEnettDefaultReceiptBank(TxnHeader deserializedReceipt, ARReceipt result, eNettInboundPaymentDataAdapter adapter, ValueObjectImportContext context)
		{
			if (deserializedReceipt.OsInvoiceAmtInclTax == null)
			{
				ReportError("DeserializedReceipt.OsInvoiceAmtInclTax is null");
			}

			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, deserializedReceipt.OsInvoiceAmtInclTax.CurrencyCode);

			if (currency == null)
			{
				ReportError("currency is null");
			}

			ENettRegisteredBankAccount enettDefaultReceiptBank = AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value.GetDefaultReceiptBankAccount(currency.PK);
			if (enettDefaultReceiptBank != null)
			{
				result.AH_AB = enettDefaultReceiptBank.BankAccountPK;
				adapter.ImportFromValueObject(result, deserializedReceipt, context);

				if (!result.IsDeleted)
				{
					result.AH_AB = enettDefaultReceiptBank.BankAccountPK;
					result.AH_ChequeOrReference = deserializedReceipt.ChequeOrReference;
				}
			}
			else
			{
				context.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("b057e624-7cef-4f01-bc04-37de172f16c1", "No eNett default bank account setup")));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message does not need to be localised")]
		void HandleEnettRegNumber(Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumber eNettRegNumber, ARReceipt result)
		{
			if (eNettRegNumber != null)
			{
				RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, eNettRegNumber.CountryOfRegistration));

				if (country == null)
				{
					ReportError("country is null");
				}

				ZQuery eNettRegNumberQuery = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, country.Code);
				eNettRegNumberQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, eNettRegNumber.NumberType);
				eNettRegNumberQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eNettRegNumber.Number);
				OrgCusCode eNettCusCode = Factory.LoadTop1<OrgCusCode>(eNettRegNumberQuery);

				if (eNettCusCode != null && eNettCusCode.Header != null)
				{
					result.AH_OH = eNettCusCode.OK_OH;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message does not need to be localised")]
		ARReceipt CreateARReceipt(TxnHeader deserializedReceipt, XmlInterchange interchange, eNettInboundPaymentDataAdapter adapter, ValueObjectImportContext context, BusinessObjectFactory newFactory)
		{
			ARReceipt result = newFactory.New<ARReceipt>();

			if (interchange.InterchangeInfo == null)
			{
				ReportError("Interchange.InterchangeInfo is null");
			}

			if (interchange.InterchangeInfo.EDIOrganisation == null)
			{
				ReportError("Interchange.InterchangeInfo.EDIOrganisation is null");
			}

			if (interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails == null)
			{
				ReportError("Interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails is null");
			}

			RegistrationNumberCollection registrationNumbers = interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails.RegistrationNumbers;

			if (registrationNumbers == null)
			{
				ReportError("registrationNumbers is null");
			}

			Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumber eNettRegNumber = registrationNumbers.FindRegistrationNumber(RegistrationNumberTypes.ENE, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			HandleEnettRegNumber(eNettRegNumber, result);

			if (result.AH_OH.IsEmpty)
			{
				context.Add(ErrorType.Error,
					Res.GetString("35dd9161-e1c8-40ac-85eb-7b5b082ddf14", "Error: Attempt to process ComPay inbound transaction failed because not organization match could be found for ComPay Debtor {0} '{1}', Code '{2}'.",
					interchange.InterchangeInfo.EDIOrganisation.EDICode.IsEmpty ? Res.GetString("717584ce-cd76-439f-bf66-9ab4dc25187e", "Name") : Res.GetString("a6de75f0-6459-4d8e-a9d6-0fa9a1097baf", "ID"),
					interchange.InterchangeInfo.EDIOrganisation.EDICode.IsEmpty ? interchange.InterchangeInfo.EDIOrganisation.OrganisationDetails.Name : interchange.InterchangeInfo.EDIOrganisation.EDICode,
					eNettRegNumber != null ? eNettRegNumber.Number.ToString() : Res.GetString("508fc032-0a80-44ab-bb49-9775b326cbbb", "Not specified")));
			}

			result.AH_ReceiptType = ReceiptTypes.eNettDirectCredit;

			HandleEnettDefaultReceiptBank(deserializedReceipt, result, adapter, context);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message does not need to be localised")]
		ARReceipt ProcessNewPayment(TxnHeader deserializedReceipt, XmlInterchange interchange, eNettInboundPaymentDataAdapter adapter, ValueObjectImportContext context, IValueObjectExportContext exportContext, BusinessObjectFactory newFactory)
		{
			Argument.NotNull(deserializedReceipt, "DeserializedReceipt", "DeserializedReceipt is null");
			Argument.NotNull(interchange, "Interchange", "Interchange is null");
			Argument.NotNull(adapter, "Adapter", "Adapter is null");
			Argument.NotNull(context, "Context", "Context is null");
			Argument.NotNull(exportContext, "ExportContext", "ExportContext is null");
			Argument.NotNull(newFactory, "NewFactory", "NewFactory is null");

			if (context.Interchange == null)
			{
				ReportError("Context.Interchange is null");
			}

			if (((XmlInterchange)context.Interchange).InterchangeInfo == null)
			{
				ReportError("Context.Interchange.InterchangeInfo is null");
			}

			if (((XmlInterchange)context.Interchange).InterchangeInfo.Source == null)
			{
				ReportError("Context.Interchange.InterchangeInfo.Source is null");
			} ((XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;
			((XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(GlbCompany.CurrentCompany.OrgProxy, exportContext);

			ARReceipt result = ChequeOrReferenceIsAlreadyUsed(deserializedReceipt.ChequeOrReference, newFactory);

			if (result != null)
			{
				context.Add(ErrorType.Error,
					Res.GetString("a86be8ed-4336-4961-b323-c67f60c3cd98", "Receipt with Company ID '{0}' already exists.", deserializedReceipt.ChequeOrReference));
			}
			else
			{
				result = CreateARReceipt(deserializedReceipt, interchange, adapter, context, newFactory);
			}

			if (result != null && !result.IsDeleted)
			{
				result.RunPreSaveValidation();
			}

			if (result != null)
			{
				if (!result.HasErrors)
				{
					if (!context.NotificationsHasErrors)
					{
						newFactory.Save();
					}
				}
				else
				{
					foreach (INotification notification in result.Notifications)
					{
						context.Add(ErrorType.Error, notification.Message);
					}
				}
			}
			return result;
		}

		ARReceipt ChequeOrReferenceIsAlreadyUsed(ZString chequeOrReference, BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Receipt);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, chequeOrReference);
			return factory.LoadTop1<ARReceipt>(filter);
		}

		#endregion

		#region Deserialize Methods

		TxnHeader DeserializeXmlInterchangeAndHeader(XmlElement xmlDocument, IValueObjectDataAdapter adapter, out XmlInterchange xmlInterchange)
		{
			using (var reader = xmlDocument.CreateReader())
			{
				return (TxnHeader)XmlInterchange.DeserializeInterchangeAndPayload(reader, new XmlValueObjectSerializer(adapter.ValueObjectType), out xmlInterchange, true);
			}
		}

		#endregion

		bool HasNotificationWarningStartsWith(ValueObjectImportContext context, string message)
		{
			var result = false;
			var notificationBuffer = context.Notifications as NotificationBuffer ?? new NotificationBuffer(context.Notifications);

			if (notificationBuffer.HasWarnings)
			{
				foreach (var notification in notificationBuffer.GetEventsByType(ErrorType.Warning))
				{
					if (notification.Message.StartsWith(message))
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		void SendNotificationEmail(ValueObjectImportContext context, TxnHeader deserializedTransaction, string transactionHumanReadableName, EDIMessage message)
		{
			if (HasNotificationWarningStartsWith(context, Res.GetString("aa8e7b72-00b7-4b15-b697-5d5158c4bc26", "Matching failed for Receipt")))
			{
				NotificationBuffer notificationBuffer = context.Notifications as NotificationBuffer ?? new NotificationBuffer(context.Notifications);
				new eNettGenericNotificationEmail(Res.GetString("d4c93ac6-27ed-4e61-aad1-8c7ff6895b7f", @"There was en error processing {0}{1}{2}.
Message: {3}
See <a href=""{4}"">{4}</a> for more details.",
					transactionHumanReadableName, deserializedTransaction.TxnNumber.IsEmpty ? "" : " ", deserializedTransaction.TxnNumber,
					notificationBuffer.Events.ToUniqueMessageListString().Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"),
					GetLinkToMessage(message))).Send();
			}
			else if (context.NotificationsHasErrors)
			{
				NotificationBuffer notificationBuffer = context.Notifications as NotificationBuffer ?? new NotificationBuffer(context.Notifications);
				new eNettGenericNotificationEmail(Res.GetString("d4c93ac6-27ed-4e61-aad1-8c7ff6895b7f", @"There was en error processing {0}{1}{2}.
Message: {3}
See <a href=""{4}"">{4}</a> for more details.",
					transactionHumanReadableName, deserializedTransaction.TxnNumber.IsEmpty ? "" : " ", deserializedTransaction.TxnNumber,
					notificationBuffer.GetEventsByType(ErrorType.Error).ToUniqueMessageListString().Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"),
					GetLinkToMessage(message))).Send();
			}
		}

		string GetLinkToMessage(EDIMessage message)
		{
			return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.LinkedeNettEDIMessage, message.PK.ToGuid());
		}

		const int maxRecords = 1;

		#endregion
	}
}
