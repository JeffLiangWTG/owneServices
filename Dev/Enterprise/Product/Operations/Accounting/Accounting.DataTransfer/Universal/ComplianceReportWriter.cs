using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using DebitCredit = Enterprise.UniversalDataBuss.DataObjects.Accounting.DebitCredit;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalCurrency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalPostingJournal = Enterprise.UniversalDataBuss.DataObjects.Accounting.PostingJournal;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0004: Cast is redundant", Justification = "Failed unit tests: without ZString? casting, its value will be string.Empty instead of null")]
	public class ComplianceReportWriter : TopLevelDataObjectWriter<AccComplianceReport, UniversalTransactionBatch>
	{
		public ComplianceReportWriter(IDataWritingManager manager) : base(manager) { }

		protected override IDataContextManager GetDataContextManager(BusinessObject report)
		{
			return report.GetUniversalDataContextManager();
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ComplianceReport;
		}

		protected override void PopulateDataObject(AccComplianceReport report, UniversalTransactionBatch dataObject)
		{
			dataObject.BatchType = new UniversalCodeDescriptionPair();
			dataObject.BatchType.Code = report.ACR_ReportType;
			dataObject.BatchType.Description = report.ACR_Description;

			dataObject.Periodicity = new UniversalCodeDescriptionPair();
			dataObject.Periodicity.Code = report.ACR_Periodicity;
			dataObject.Periodicity.Description = report.Lookups.PeriodicityList.GetDescriptionFromCode(report.ACR_Periodicity);

			dataObject.DateFrom = report.ACR_DateFrom;
			dataObject.DateTo = report.ACR_DateTo;

			var additionalDataCollector = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ComplianceReportDataCollectionMode.TransactionBatch);

			dataObject.SetOrganizationAddressCollection(() =>
			{
				var result = new List<UniversalOrgAddress>();
				result.Add(additionalDataCollector.OrgAddresses[report.Company.OrgProxy.OH_Code]);
				if (report.Branch != null && report.Branch.OrgProxy != null)
				{
					result.Add(additionalDataCollector.OrgAddresses[report.Branch.OrgProxy.OH_Code]);
				}
				return result;
			});

			if (ComplianceReportAdditionalDataCollector.ShouldAddOpeningGLBalance(report))
			{
				dataObject.TransactionCollection.Add(CreateOpeningGLBalanceLineInfo(report));
			}

			bool shouldPopulateAdditionalHeaderDetails = ComplianceReportAdditionalDataCollector.ShouldPopulateAdditionalHeaderDetails(report);
			var headerPK = ZGuid.Empty;
			UniversalTransaction batchLine = null;
			UniversalPostingJournal postingJournal = null;

			foreach (AccComplianceReportLine line in report.ReportLines)
			{
				if (batchLine == null || line.AH_PK != headerPK)
				{
					batchLine = CreateLineInfo(report, line, additionalDataCollector.OrgAddresses);
					headerPK = line.AH_PK;
					postingJournal = null;

					if (shouldPopulateAdditionalHeaderDetails || report.SupportsDayBook)
					{
						var header = additionalDataCollector.GetTransactionHeader(line.AH_PK) as TransactionHeaderDetails;
						if (line.AH_PK.IsEmpty && report.SupportsDayBook)
						{
							batchLine.Description = AccountingConstants.DefaultDayBookLineDescriptions.WIPAccrual;
						}
						if (header != null)
						{
							PopulateAdditionalHeaderDetails(report, batchLine, header, shouldPopulateAdditionalHeaderDetails);
							if (shouldPopulateAdditionalHeaderDetails)
							{
								PopulateBankAccountIfNecessary(batchLine, line, additionalDataCollector.DebitBankAccounts, header);
							}
						}
					}

					dataObject.TransactionCollection.Add(batchLine);
				}

				PopulatePostingJournal(report, batchLine, line, ref postingJournal, additionalDataCollector);
			}
		}

		UniversalTransaction CreateOpeningGLBalanceLineInfo(AccComplianceReport report)
		{
			var result = new UniversalTransaction(writeManager.WriterStrategy);
			result.PostDate = report.ACR_DateFrom.AddDays(-1);

			result.LocalCurrency = new UniversalCurrency()
			{
				Code = report.Company.LocalCurrency.RX_Code,
				Description = report.Company.LocalCurrency.RX_DescMultilingual
			};

			result.SetPostingJournalCollection(() =>
			{
				var collection = new List<UniversalPostingJournal>();
				var lineSubInfoDR = new UniversalPostingJournal(writeManager.WriterStrategy) { LocalAmount = report.GLOpeningBalanceDR, Sequence = 0 };
				PopulateOpeningBalanceDetails(report, lineSubInfoDR, DebitCredit.Debit);
				var lineSubInfoCR = new UniversalPostingJournal(writeManager.WriterStrategy) { LocalAmount = -report.GLOpeningBalanceCR, Sequence = 0 };
				PopulateOpeningBalanceDetails(report, lineSubInfoCR, DebitCredit.Credit);

				collection.Add(lineSubInfoDR);
				collection.Add(lineSubInfoCR);
				return collection;
			});

			return result;
		}

		void PopulateOpeningBalanceDetails(AccComplianceReport report, UniversalPostingJournal openingBalanceLine, DebitCredit debitCredit)
		{
			if (report.SupportsGLBalanceDetails)
			{
				var debitCreditAsString = new DebitCreditConverter().FromEnumValue(debitCredit);
				foreach (var balanceDetailsLine in report.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>().Where(x => x.AG_DebitCredit == debitCreditAsString))
				{
					var line = new PostingJournalDetail();
					var glAccount = new GLAccount()
					{
						AccountCode = balanceDetailsLine.AG_AccountNum,
						DebitCredit = debitCredit,
						AccountType = balanceDetailsLine.AG_AccountType.IsEmpty ? null : new UniversalCodeDescriptionPair() { Code = balanceDetailsLine.AG_AccountType },
						Description = balanceDetailsLine.AG_Description.IsEmpty ? null : balanceDetailsLine.AG_Description,
						ConsolidationAccountCode = balanceDetailsLine.AG_ConsolidationAccountNum.IsEmpty ? null : balanceDetailsLine.AG_ConsolidationAccountNum,
					};

					switch (debitCredit)
					{
						case DebitCredit.Debit:
							line.DebitGLAccount = glAccount;
							line.PostingAmount = balanceDetailsLine.GeneralLedgerAmountDR;
							break;
						case DebitCredit.Credit:
							line.CreditGLAccount = glAccount;
							line.PostingAmount = balanceDetailsLine.GeneralLedgerAmountCR;
							break;
					}

					openingBalanceLine.PostingJournalDetailCollection.Add(line);
				}
			}
		}

		UniversalTransaction CreateLineInfo(AccComplianceReport report, AccComplianceReportLine line, Dictionary<ZString, UniversalOrgAddress> orgAddresses)
		{
			var result = new UniversalTransaction(writeManager.WriterStrategy);
			result.Ledger = line.AH_Ledger;
			result.TransactionType = line.AH_TransactionType.IsEmpty ? null : new TransactionTypeConverter().ToEnumValue(line.AH_TransactionType);
			result.PostDate = line.PostDate.IsEmpty ? null : line.PostDate;
			result.Number = line.AH_TransactionNum.IsEmpty ? null : (ZString?)line.AH_TransactionNum;
			result.TransactionReference = line.AH_TransactionReference.IsEmpty ? null : (ZString?)line.AH_TransactionReference;
			result.ComplianceSubType = line.AH_ComplianceSubType.IsEmpty ? null : (ZString?)line.AH_ComplianceSubType;
			result.Description = line.ReportSubCode.IsEmpty ? null : (ZString?)line.ReportSubCode;
			result.LocalCurrency = new UniversalCurrency()
			{
				Code = report.Company.LocalCurrency.RX_Code,
				Description = report.Company.LocalCurrency.RX_DescMultilingual
			};

			result.LocalExVATAmount = line.TotalExTaxAmount;
			result.LocalVATAmount = line.TotalTaxAmount;

			if (!line.OH_Code.IsEmpty && orgAddresses.ContainsKey(line.OH_Code))
			{
				result.OrganizationAddress = orgAddresses[line.OH_Code];
				if (result.OrganizationAddress.GovRegNumType == null)
				{
					result.OrganizationAddress.GovRegNumType = new RegistrationNumberType()
					{ Code = report.TaxRegistrationType, Description = report.TaxRegistrationTypeList.GetDescriptionFromCode(report.TaxRegistrationType) };
					result.OrganizationAddress.GovRegNum = line.OK_CustomsRegNo.IsEmpty ? null : (ZString?)line.OK_CustomsRegNo;
				}
			}

			return result;
		}

		void PopulatePostingJournal(AccComplianceReport report, UniversalTransaction batchLine, AccComplianceReportLine line, ref UniversalPostingJournal postingJournal, ComplianceReportAdditionalDataCollector additionalDataCollector)
		{
			if (batchLine.PostingJournalCollection != null || batchLine.SetPostingJournalCollection(() => new List<UniversalPostingJournal>()))
			{
				if (report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBook)
				{
					postingJournal = PopulatePostingJournalAsSubInfo(line, batchLine, report.ReportLineGrouping, additionalDataCollector);
					batchLine.PostingJournalCollection.Add(postingJournal);
				}
				else if (report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping)
				{
					var currentPostingJournal = postingJournal;
					postingJournal = PopulatePostingJournalAsSubInfo(line, batchLine, report.ReportLineGrouping, additionalDataCollector, currentPostingJournal);
					if (postingJournal != currentPostingJournal)
					{
						batchLine.PostingJournalCollection.Add(postingJournal);
					}
				}
				else
				{
					if ((report.GoodsServiceType == Lookups.GoodsServiceTypeCodes.GoodsAndService ||
						report.GoodsServiceType == Lookups.GoodsServiceTypeCodes.GoodsOnly) && line.IsGoods)
					{
						postingJournal = PopulatePostingJournalAsSubInfo(line, batchLine, Lookups.GoodsServiceTypeCodes.GoodsOnly, additionalDataCollector);
						batchLine.PostingJournalCollection.Add(postingJournal);
					}

					if ((report.GoodsServiceType == Lookups.GoodsServiceTypeCodes.GoodsAndService ||
						report.GoodsServiceType == Lookups.GoodsServiceTypeCodes.ServiceOnly) && line.IsService)
					{
						postingJournal = PopulatePostingJournalAsSubInfo(line, batchLine, Lookups.GoodsServiceTypeCodes.ServiceOnly, additionalDataCollector);
						batchLine.PostingJournalCollection.Add(postingJournal);
					}
				}
			}
		}

		void PopulateAdditionalHeaderDetails(AccComplianceReport report, UniversalTransaction batchLine, TransactionHeaderDetails headerData, bool populateAllHeaderDetails)
		{
			if (populateAllHeaderDetails || report.SupportsDayBook)
			{
				batchLine.Description = headerData.Description;
				batchLine.CreateTime = headerData.CreateTime;
				batchLine.CreateUser = new StaffUsingAttributes() { Code = headerData.CreateUserCode, Name = headerData.CreateUserName };
			}

			if (populateAllHeaderDetails)
			{
				if (headerData.TransactionCurrency != report.Company.LocalCurrency.RX_Code)
				{
					batchLine.OSCurrency = new UniversalCurrency() { Code = headerData.TransactionCurrency, Description = headerData.TransactionCurrencyDesc };
					batchLine.ExchangeRate = headerData.ExchangeRate;
				}
				if (!headerData.OriginalTransactionNumber.IsEmpty)
				{
					batchLine.OriginalReference = new OriginalReference() { OriginalTransactionNumber = headerData.OriginalTransactionNumber };
				}
				batchLine.TransactionDate = headerData.InvoiceDate;
				batchLine.OSTotal = headerData.OSTotal;
				batchLine.CheckNumberOrPaymentRef = string.IsNullOrWhiteSpace(headerData.ChequeOrReference) ? null : (ZString?)headerData.ChequeOrReference;
				batchLine.PaymentOrReceiptType = string.IsNullOrWhiteSpace(headerData.ReceiptType) ? null : new PaymentOrReceiptTypeConverter().ToEnumValue(headerData.ReceiptType);
				if (headerData.IsSelfBilling)
				{
					batchLine.PlaceOfIssue = "CustomerSelfBill";
				}
				batchLine.IsCancelled = headerData.IsCancelled;
				batchLine.LastEditTime = headerData.LastEditTime;
				batchLine.LastEditUser = new StaffUsingAttributes() { Code = headerData.LastEditUserCode, Name = headerData.LastEditUserName };
				batchLine.DigitalSignature = string.IsNullOrWhiteSpace(headerData.DigitalSignature) ? null : (ZString?)headerData.DigitalSignature;
			}
		}

		void PopulateBankAccountIfNecessary(UniversalTransaction batchLine, AccComplianceReportLine line, Dictionary<ZGuid, BankAccount> debitBankAccounts, TransactionHeaderDetails headerData)
		{
			if (line.AH_TransactionType == TransactionTypes.Payment && line.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				batchLine.SetBankAccountCollection(() =>
				{
					var result = new List<BankAccount>();
					if (!headerData.DebitBankAccount.IsEmpty)
					{
						BankAccount account;
						if (debitBankAccounts.TryGetValue(headerData.DebitBankAccount, out account))
						{
							result.Add(account);
						}
					}
					if (!string.IsNullOrWhiteSpace(headerData.CreditBankAccount))
					{
						result.Add(new BankAccount()
						{
							AccountNumber = headerData.CreditBankAccount,
							AccountType = BankAccountType.Credit,
							Country = new Country() { Code = headerData.CountryCode, Name = headerData.CountryDesc },
							BankName = headerData.BankName
						});
					}
					return result;
				});
			}
		}

		protected UniversalPostingJournal PopulatePostingJournalAsSubInfo(AccComplianceReportLine line, UniversalTransaction lineInfo, ZString lineTypeCode, ComplianceReportAdditionalDataCollector additionalDataCollector, UniversalPostingJournal currentPostingJournal = null)
		{
			var result = currentPostingJournal;
			if (result == null || result.BatchSequence != line.ACL_ReportSequence)
			{
				result = new UniversalPostingJournal(writeManager.WriterStrategy);
				result.BatchSequence = line.ACL_ReportSequence;

				result.ChargeCurrency = lineInfo.LocalCurrency;

				result.Organization = line.OH_Code.IsEmpty ? null : new OrganizationReference() { Key = line.OH_Code, Type = nameof(DataContextType.Organization) };

				result.VATTaxID = line.AT_Code.IsEmpty ? null : GetNewTaxID(additionalDataCollector.TaxIDs[line.AT_Code], line.AL_TaxRate);
				result.TaxMessageID = line.TaxMessage.IsEmpty ? null : new TaxMessageID() { TaxMessageCode = line.TaxMessage };

				TransactionLineDetails details = additionalDataCollector.GetTransactionLine(line.ACL_ReportSequence);
				if (details != null)
				{
					result.Sequence = details.LineSequence;
					result.GSTVATBasis = new UniversalCodeDescriptionPair() { Code = details.GSTVATBasis };
					result.Description = details.Description;
					result.OSAmount = details.OSExTaxAmount;
					if (!details.WithholdingTaxPK.IsEmpty)
					{
						result.WithholdingTaxID = additionalDataCollector.WithholdingTaxIDs[details.WithholdingTaxPK];
						result.LocalWHTAmount = details.WithholdingTaxAmount;
					}
				}

				result.TransactionCategory = lineTypeCode;
				PopulatePostingJournalLineTypeSpecificFields(result, line, lineInfo, details, additionalDataCollector);
			}
			else
			{
				PopulatePostingJournalLineTypeSpecificFields(result, line, lineInfo, null, additionalDataCollector, journalDetailsOnly: true);
			}

			return result;
		}

		void PopulatePostingJournalLineTypeSpecificFields(UniversalPostingJournal postingJournal, AccComplianceReportLine line, UniversalTransaction lineInfo, TransactionLineDetails details, ComplianceReportAdditionalDataCollector additionalDataCollector, bool journalDetailsOnly = false)
		{
			switch (postingJournal.TransactionCategory)
			{
				case Lookups.GoodsServiceTypeCodes.GoodsOnly:
					postingJournal.LocalAmount = line.GoodsExTaxAmount;
					postingJournal.LocalGSTVATAmount = line.GoodsTaxAmount;
					break;
				case Lookups.GoodsServiceTypeCodes.ServiceOnly:
					postingJournal.LocalAmount = line.ServiceExTaxAmount;
					postingJournal.LocalGSTVATAmount = line.ServiceTaxAmount;
					break;
				case Lookups.ReportLineGroupingListCodes.DayBook:
					postingJournal.LocalAmount = line.GeneralLedgerAmountCR + line.GeneralLedgerAmountDR; // Only one of this amounts will be not 0
					postingJournal.GLAccount = line.AG_AccountNum.IsEmpty ? null : new GLAccount() { AccountCode = line.AG_AccountNum, Description = line.AG_Description };
					postingJournal.GLPostDate = line.PostDate.IsEmpty ? null : line.PostDate;
					postingJournal.Branch = line.GB_Code.IsEmpty ? null : new Branch() { Code = line.GB_Code };
					postingJournal.Department = line.GE_Code.IsEmpty ? null : new Department() { Code = line.GE_Code };
					break;
				case Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping:
					if (!journalDetailsOnly)
					{
						postingJournal.Branch = line.GB_Code.IsEmpty ? null : new Branch() { Code = line.GB_Code };
						postingJournal.Department = line.GE_Code.IsEmpty ? null : new Department() { Code = line.GE_Code };

						if (details != null && !details.ChargeCode.IsEmpty &&
							additionalDataCollector.ChargeCodes.TryGetValue(details.ChargeCode, out var chargeCodeDetails))
						{
							postingJournal.ChargeCode = new ChargeCode() { Code = chargeCodeDetails.ChargeCode.Code, Description = chargeCodeDetails.ChargeCode.Description };
						}

						postingJournal.LocalAmount = line.GoodsExTaxAmount + line.ServiceExTaxAmount;
						postingJournal.LocalGSTVATAmount = line.GoodsTaxAmount + line.ServiceTaxAmount;
					}

					var journalDetail = new PostingJournalDetail();

					journalDetail.PostingCurrency = lineInfo.LocalCurrency;
					journalDetail.PostingAmount = line.GeneralLedgerAmountCR + line.GeneralLedgerAmountDR; // Only one of this amounts will be not 0

					if (!line.AG_AccountNum.IsEmpty)
					{
						if (!line.GeneralLedgerAmountCR.IsEmpty)
						{
							journalDetail.CreditGLAccount = new GLAccount() { AccountCode = line.AG_AccountNum, Description = line.AG_Description };
						}
						if (!line.GeneralLedgerAmountDR.IsEmpty)
						{
							journalDetail.DebitGLAccount = new GLAccount() { AccountCode = line.AG_AccountNum, Description = line.AG_Description };
						}
					}
					journalDetail.PostingDate = line.PostDate.IsEmpty ? null : line.PostDate;

					postingJournal.PostingJournalDetailCollection.Add(journalDetail);
					break;
				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid Compliance Report Line Type Code: {0}.", postingJournal.TransactionCategory));
			}
		}

		internal static TaxID GetNewTaxID(TaxID taxID, ZDecimal rate) => GetNewTaxID(taxID.TaxCode.Value, taxID.Description.Value, taxID.TaxType.Code.Value, rate);

		internal static TaxID GetNewTaxID(ZString code, ZString description, ZString taxType, ZDecimal rate)
			=> new TaxID { TaxCode = code, Description = description, TaxType = new UniversalCodeDescriptionPair { Code = taxType }, TaxRate = rate };
	}
}
