using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCountry = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using UniversalCurrency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;
using UniversalUNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	public class CollectionBatchWriter : TopLevelDataObjectWriter<AccCollectionBatch, UniversalTransactionBatch>
	{
		public CollectionBatchWriter(IDataWritingManager manager) : base(manager) { }

		protected override IDataContextManager GetDataContextManager(BusinessObject batch)
		{
			return batch.GetUniversalDataContextManager();
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CollectionBatch;
		}

		SettlementMethod SetSettlementMethodDetailFromBatch(AccCollectionBatch batch, AccCollectionOrder order)
		{
			var settlementMethod = new SettlementMethod();
			if (batch != null)
			{
				settlementMethod.Method = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair();
				settlementMethod.Method.Code = batch.ACB_CollectionFileFormat;
				CollectionFileFormatList fileFormatList = new CollectionFileFormatList();
				settlementMethod.Method.Description = fileFormatList.GetDescriptionFromCode(settlementMethod.Method.Code);
			}
			if (order != null)
			{
				settlementMethod.AuthorizationReference = order.CollectionRequestUMRReference;
				settlementMethod.AuthorizationExpiryDate = order.CollectionRequestUMRSignedDate;
			}
			return settlementMethod;
		}

		BankAccount SetBankAccountDetailFromBatch(AccCollectionBatch batch)
		{
			var bankAccount = new BankAccount();
			if (batch != null && batch.BankAccount != null)
			{
				bankAccount.BankName = batch.BankAccount.AB_BankName.IsEmpty ? null : batch.BankAccount.AB_BankName;
				bankAccount.AccountName = batch.BankAccount.AB_BankAccountName.IsEmpty ? null : batch.BankAccount.AB_BankAccountName;
				bankAccount.AccountNumber = batch.BankAccount.AB_AccountNum.IsEmpty ? null : batch.BankAccount.AB_AccountNum;
				bankAccount.BankBranch = batch.BankAccount.AB_BSB.IsEmpty ? null : batch.BankAccount.AB_BSB;
				bankAccount.BankSwift = batch.BankAccount.AB_SWIFT.IsEmpty ? null : batch.BankAccount.AB_SWIFT;
				bankAccount.Currency = batch.BankAccount.AB_RX_NKAccountCurrency.IsEmpty ? null : batch.BankAccount.AB_RX_NKAccountCurrency;
				bankAccount.IsDefaultAccount = batch.BankAccount.AB_IsDefaultReceiptBankAccount.IsEmpty ? ZBool.False : batch.BankAccount.AB_IsDefaultReceiptBankAccount;
				bankAccount.Country = null;
				RefCountry country = batch.BankAccount.BankAccountCountry;
				if (country != null)
				{
					bankAccount.Country = new UniversalCountry();
					bankAccount.Country.Code = country.Code.IsEmpty ? null : country.Code;
					bankAccount.Country.Name = country.Description.IsEmpty ? null : country.Description;
				}
				bankAccount.IBANNumber = batch.BankAccount.IBAN.IsEmpty ? null : batch.BankAccount.IBAN;
				bankAccount.EFTUserId = batch.BankAccount.AB_AccountEFTUserID.IsEmpty ? null : batch.BankAccount.AB_AccountEFTUserID;
				bankAccount.AccountType = BankAccountType.Credit;
				if (!batch.BankAccount.AB_AutoDDRFormat.IsEmpty)
				{
					bankAccount.AutoDDRFormat = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair();
					bankAccount.AutoDDRFormat.Code = batch.BankAccount.AB_AutoDDRFormat;
					BankDDRFormatList bankDDRFormatList = new BankDDRFormatList();
					bankAccount.AutoDDRFormat.Description = bankDDRFormatList.GetDescriptionFromCode(bankAccount.AutoDDRFormat.Code);
				}
			}
			return bankAccount;
		}

		BankAccount SetBankAccountDetailFromOrder(AccCollectionOrder order)
		{
			var bankAccount = new BankAccount();
			if (order != null)
			{
				bankAccount.BankName = order.CollectionRequestBankName.IsEmpty ? null : order.CollectionRequestBankName;
				bankAccount.BankBranchName = order.CollectionRequestBankBranchName.IsEmpty ? null : order.CollectionRequestBankBranchName;
				bankAccount.AccountName = order.CollectionRequestAccountName.IsEmpty ? null : order.CollectionRequestAccountName;
				bankAccount.AccountNumber = order.CollectionRequestAccountNumber.IsEmpty ? null : order.CollectionRequestAccountNumber;
				bankAccount.BankBranch = order.CollectionRequestBankBsb.IsEmpty ? null : order.CollectionRequestBankBsb;
				bankAccount.BankSwift = order.CollectionRequestBankSwift.IsEmpty ? null : order.CollectionRequestBankSwift;
				bankAccount.Currency = order.CollectionRequestAccountCurrency.IsEmpty ? null : order.CollectionRequestAccountCurrency;
				bankAccount.IsDefaultAccount = order.CollectionRequestBankIsDefault.IsEmpty ? ZBool.False : order.CollectionRequestBankIsDefault;
				bankAccount.IBANNumber = order.CollectionRequestIBANNumber.IsEmpty ? null : order.CollectionRequestIBANNumber;
				bankAccount.AccountType = BankAccountType.Debit;
				if (order.CollectionRequestBankAccountDetail != null)
				{
					RefCountry country = order.CollectionRequestBankAccountDetail.Country;
					if (country != null)
					{
						bankAccount.Country = new UniversalCountry();
						bankAccount.Country.Code = country.Code.IsEmpty ? null : country.Code;
						bankAccount.Country.Name = country.Description.IsEmpty ? null : country.Description;
					}
				}
			}
			return bankAccount;
		}

		protected override void PopulateDataObject(AccCollectionBatch batch, UniversalTransactionBatch dataObject)
		{
			var batchBankAccount = SetBankAccountDetailFromBatch(batch);
			var orgAddresses = GetOrgAddresses(batch);

			dataObject.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<UniversalOrgAddress>();
				var batchCompanyProxy = batch.Company.GC_OH_OrgProxy;
				if (!batchCompanyProxy.IsEmpty && orgAddresses.ContainsKey(batchCompanyProxy))
				{
					addresses.Add(orgAddresses[batchCompanyProxy]); // store orgAddress for the batch company's org proxy
				}
				return addresses;
			});

			var fileFormat = batch.ACB_CollectionFileFormat;
			var isRibaOrSepaFileFormat = fileFormat == CollectionFileFormatList.Codes.ribaFormat || fileFormat == CollectionFileFormatList.Codes.sepaFormat;
			var transactionNumberOption = AccountingConfigurationRegistry.Instance.TransactionNumberOptionForCollectionBatchExportFile.GetValueWithoutFallback(batch.ACB_GC.ToGuid(), Guid.Empty, Guid.Empty);
			var transactionTypeConverter = new TransactionTypeConverter();
			foreach (var order in batch.CollectionOrders)
			{
				if (order.IncludeInBatch && order.ACO_Amount != 0 && !order.IsCancelled) // do we need to include rejected order?
				{
					var settlementMethod = SetSettlementMethodDetailFromBatch(batch, order);
					foreach (var orderLine in order.CollectionOrderLines)
					{
						if (orderLine.IncludeInOrder && orderLine.CollectionAmount != 0)
						{
							var orderlineInfo = new UniversalTransaction(writeManager.WriterStrategy);
							orderlineInfo.TransactionReference = order.ACO_OrderNumber;

							if (isRibaOrSepaFileFormat && transactionNumberOption == AccountingMasterFilesConstants.TransactionNumberCodes.ComplianceOrInvoiceNr)
							{
								var transactionHeader = orderLine.TransactionHeader;
								var complianceSubType = transactionHeader.AH_ComplianceSubType;
								var transactionReference = transactionHeader.AH_TransactionReference;
								orderlineInfo.ComplianceSubType = complianceSubType.IsEmpty ? null : complianceSubType;
								orderlineInfo.Number = complianceSubType.IsEmpty || transactionReference.IsEmpty ? orderLine.TransactionNumber : transactionReference;
							}
							else
							{
								orderlineInfo.Number = orderLine.TransactionNumber;
							}

							orderlineInfo.OrderCollectionDate = order.ACO_CollectionDate;
							orderlineInfo.SettlementMethod = settlementMethod;

							var orderBankAccount = SetBankAccountDetailFromOrder(order);
							orderlineInfo.SetBankAccountCollection(() =>
							{
								var accounts = new List<BankAccount>();
								accounts.Add(orderBankAccount);
								accounts.Add(batchBankAccount);
								return accounts;
							});

							orderlineInfo.SetPostingJournalCollection(() => new List<PostingJournal> { new PostingJournal(writeManager.WriterStrategy) });

							orderlineInfo.IsCancelled = order.ACO_IsCancelled;

							var debtor = order.ACO_OH_Debtor;
							if (!debtor.IsEmpty && orgAddresses.ContainsKey(debtor))
							{
								orderlineInfo.OrganizationAddress = orgAddresses[debtor];
							}

							orderlineInfo.TransactionType = orderLine.TransactionType.IsEmpty ? null : transactionTypeConverter.ToEnumValue(orderLine.TransactionType);
							orderlineInfo.OutstandingAmount = orderLine.CollectionAmount;
							orderlineInfo.JobInvoiceNumber = orderLine.JobInvoicingNumber;
							orderlineInfo.PostDate = orderLine.PostDate.IsEmpty ? null : orderLine.PostDate;
							orderlineInfo.DueDate = orderLine.DueDate.IsEmpty ? null : orderLine.DueDate;
							orderlineInfo.TransactionDate = orderLine.InvoiceDate.IsEmpty ? null : orderLine.InvoiceDate;

							orderlineInfo.LocalCurrency = new UniversalCurrency();
							orderlineInfo.LocalCurrency.Code = orderLine.LocalCurrency;
							orderlineInfo.LocalTotal = orderLine.LocalInvoiceAmount;

							orderlineInfo.OSCurrency = new UniversalCurrency();
							orderlineInfo.OSCurrency.Code = orderLine.OSCurrency;
							orderlineInfo.OSTotal = orderLine.OSInvoiceAmount;

							dataObject.TransactionCollection.Add(orderlineInfo);
						}
					}
				}
			}
			if (isRibaOrSepaFileFormat && !dataObject.TransactionCollection.Any())
			{
				throw new DataObjectValidationException(Res.GetString("E49902B2-44F4-4CBF-A918-A6E9AF1BC749", "Batch number {0} without valid transactions, {1} file cannot be created.",
					batch.ACB_BatchNumber, fileFormat + "A"));
			}
		}

		Dictionary<ZGuid, UniversalOrgAddress> GetOrgAddresses(AccCollectionBatch batch)
		{
			var result = new Dictionary<ZGuid, UniversalOrgAddress>();

			var orgPKs = new List<ZGuid>();

			orgPKs.Add(GlbCompany.CurrentCompany.GC_OH_OrgProxy); // add login company's org proxy
			orgPKs.AddRange(batch.CollectionOrders.Where(x => !x.ACO_OH_Debtor.IsEmpty).Select(x => x.ACO_OH_Debtor).Distinct()); // add each order's debtor

			var orgQuery = new ZQuery(OrgHeaderSchema.PK, orgPKs);
			var orgHeaders = batch.Factory.Load<OrgHeader>(orgQuery);
			foreach (var orgHeader in orgHeaders)
			{
				var orgAddress = new UniversalOrgAddress(writeManager.WriterStrategy);
				var calculatedAddress = orgHeader.Addresses.DefaultAddressOfType(OrgAddressType.Receivables);
				if (calculatedAddress == null)
				{
					calculatedAddress = orgHeader.Addresses.DefaultAddressOfType(OrgAddressType.Office);
					orgAddress.AddressType = OrgAddressType.Office.ToString();
				}
				else
				{
					orgAddress.AddressType = OrgAddressType.Receivables.ToString();
				}
				orgAddress.AddressOverride = ZBool.False;
				orgAddress.OrganizationCode = orgHeader.OH_Code;

				if (!calculatedAddress.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					orgAddress.Port = new UniversalUNLOCO();
					orgAddress.Port.Code = calculatedAddress.OA_RL_NKRelatedPortCode;
					orgAddress.Port.Name = calculatedAddress.PortName;
				}

				ZString? companyNameOverride = calculatedAddress.OA_CompanyNameOverride.IsEmpty ? null : calculatedAddress.OA_CompanyNameOverride;
				orgAddress.CompanyName = (companyNameOverride.HasValue && !companyNameOverride.Value.IsEmpty) ? companyNameOverride : orgHeader.OH_FullName;
				if (!calculatedAddress.OA_RN_NKCountryCode.IsEmpty)
				{
					orgAddress.Country = new UniversalCountry();
					orgAddress.Country.Code = calculatedAddress.OA_RN_NKCountryCode;
					orgAddress.Country.Name = calculatedAddress.Country != null ? calculatedAddress.Country.RN_DescMultilingual : null;
				}

				if (!orgHeader.OH_ScreeningStatus.IsEmpty)
				{
					orgAddress.ScreeningStatus = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair();
					orgAddress.ScreeningStatus.Code = orgHeader.OH_ScreeningStatus;
					ScreeningStatusesList screeningStatusesList = new ScreeningStatusesList();
					orgAddress.ScreeningStatus.Description = screeningStatusesList.GetDescriptionFromCode(orgAddress.ScreeningStatus.Code);
				}

				orgAddress.AddressShortCode = calculatedAddress.OA_Code;
				orgAddress.Address1 = calculatedAddress.OA_Address1.IsEmpty ? null : calculatedAddress.OA_Address1;
				orgAddress.Address2 = calculatedAddress.OA_Address2.IsEmpty ? null : calculatedAddress.OA_Address2;
				orgAddress.City = calculatedAddress.OA_City.IsEmpty ? null : calculatedAddress.OA_City;
				orgAddress.Postcode = calculatedAddress.OA_PostCode.IsEmpty ? null : calculatedAddress.OA_PostCode;
				orgAddress.State = calculatedAddress.OA_State.IsEmpty ? null : calculatedAddress.OA_State;

				orgAddress.Email = calculatedAddress.OA_Email.IsEmpty ? null : calculatedAddress.OA_Email;
				orgAddress.Fax = calculatedAddress.OA_Fax.IsEmpty ? null : calculatedAddress.OA_Fax;
				orgAddress.Phone = calculatedAddress.OA_Phone.IsEmpty ? null : calculatedAddress.OA_Phone;

				if (orgHeader.CustomsCodes.Any())
				{
					orgAddress.SetRegistrationNumberCollection(() =>
					{
						var collection = new List<UniversalRegistrationNumber>();
						foreach (OrgCusCode regNo in orgHeader.CustomsCodes)
						{
							collection.Add(OrganizationAddressHelper.CreateRegistrationNumber(regNo));
						}
						return collection;
					});
				}

				result[orgHeader.PK] = orgAddress;
			}

			return result;
		}
	}
}
