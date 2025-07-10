using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CriticalValidation
{
	#region SuppressResourceStringsCheckRegion
	// Developer only debug message should not be translated

	public static class CriticalValidationInfoExtensions
	{
		public static string GetTransactionMatchLinkGroupInfoAboutBalanceIssue(this TransactionMatchLinkGroup group)
		{
			return group.GetTransactionMatchLinkGroupInfo(
				filterMatchLink: (matchLink => true),
				basicTransactionInfo: FormattableString.Invariant($"Transaction Match Group Balance = {group.GetBalance()}, Number of Match Links = {group.Count}")
			);
		}

		public static string GetTransactionMatchLinkGroupInfoAboutCompanyIssue(this TransactionMatchLinkGroup group, (ZGuid mainCompany, ZGuid[] otherCompany) companiesFocusing)
		{
			return group.GetTransactionMatchLinkGroupInfo(
				filterMatchLink: filterMatchLink,
				basicTransactionInfo: FormattableString.Invariant($"Transaction MainCompany = {companiesFocusing.mainCompany} , Number of Other Companies = {companiesFocusing.otherCompany.Length}")
				);
			bool filterMatchLink(TransactionMatchLink matchLink)
			{
				return matchLink?.TransactionHeader != null
					&& companiesFocusing.otherCompany.Contains(matchLink.TransactionHeader.AH_GC);
			}
		}

		static string GetTransactionMatchLinkGroupInfo(
			this TransactionMatchLinkGroup group
			, Func<TransactionMatchLink, bool> filterMatchLink
			, string basicTransactionInfo)
		{
			var matchLinks = group.Select(
					(matchLink, index) => (matchLink: (TransactionMatchLink)matchLink, index)
				)
				.Where(matchLinkWithIndex => filterMatchLink(matchLinkWithIndex.matchLink));

			var result = new StringBuilder();
			result.AppendLine(basicTransactionInfo);
			result.AppendLine();
			foreach (var (matchLink, index) in matchLinks)
			{
				result.AppendLine(string.Format(CultureInfo.InvariantCulture, "Match Link {0}:", index));
				result.AppendLine(matchLink.GetAllPropertyValues());
				if (matchLink.TransactionHeader != null)
				{
					result.AppendLine(string.Format(CultureInfo.InvariantCulture, "Transaction {0}:", index));
					result.AppendLine(matchLink.TransactionHeader.GetAllPropertyValues());
					result.AppendLine(string.Format(CultureInfo.InvariantCulture, "LocalPartialPaymentAmount: {0}", ((IMatching)matchLink.MatchingTransaction).LocalPartialPaymentAmount));
					result.AppendLine();
				}
				else
				{
					result.AppendLine("Transaction Header is null.");
				}
			}
			return result.ToString();
		}

		public static string GetJobConsolCostInfo(this JobConsolCost consolCost, ApportionSplitCharge[] chargeCollection = null, bool shouldAddChargesStackTraceInfo = false)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine(ZString.Format(
@"Job Consol Cost:
{0}
	Charge Code = {1}, Invoice # = {2}, Invoice Date = {3}, Currency = {4}, OS Cost Amount = {5}, GST is Overridden = {25}, OS GST Amount = {6}, Exchange Rate = {7}, Local Cost Amount = {8}, Creditor = {9}, PPDCLT = {10}, Apportionment Method = {11}, Supplier Cost Reference = {12}, Payment Date = {13}, Payment Type = {14}, Cheque # = {15}, Bank Account = {16}, Cheque Book = {17}, Tax Rate = {18}, Tax Date = {26}, AR Invoice = {19}, AP Invoice = {20}, Is For Collect Invoice = {21}, Consol = {22}, Is Final = {23}, Tax Code = {24}, Supply Type = {27}, Tax Branch = {28}.",
				consolCost.GetBusinessObjectGenericInfo(), consolCost.ChargeCode == null ? ZString.Empty : consolCost.ChargeCode.AC_Code, consolCost.E6_InvoiceNum,
				consolCost.E6_InvoiceDate.ToAUString(), consolCost.E6_RX_NKCurrency, consolCost.E6_OSCostAmount, consolCost.E6_OSGSTAmount, consolCost.E6_ExchangeRate,
				consolCost.E6_LocalCostAmount, consolCost.Creditor == null ? ZString.Empty : consolCost.Creditor.OH_Code, consolCost.E6_PPDCLT,
				consolCost.E6_ApportionmentMethod, consolCost.E6_CostReference, consolCost.E6_PaymentDate.ToAUString(), consolCost.E6_PaymentType, consolCost.E6_ChequeOrReference,
				consolCost.E6_AB_BankAccount, consolCost.E6_AK_ChequeBook, consolCost.E6_AT_TaxRate, consolCost.E6_AH_ARInvoice, consolCost.E6_AH_APInvoice,
				consolCost.E6_IsForCollectInvoice, consolCost.E6_ParentID,
				((bool)consolCost.IsFinal).ToYesNoString(),
				consolCost.TaxRate == null ? ZString.Empty : consolCost.TaxRate.AT_Code,
				((bool)consolCost.E6_IsTaxAmountOverridden).ToYesNoString(),
				consolCost.E6_TaxDate,
				consolCost.E6_SupplyType,
				consolCost.CostTaxBranch?.GB_Code ?? ZString.Empty)
				+ consolCost.GetFieldsWithChangesInfo()
				+ System.Environment.NewLine
				+ consolCost.GetParentCollectionsInfo());

			AddApportionmentChargesInfo();

			if (consolCost.IsPosted)
			{
				TransactionHeaderWithLines transaction = null;
				try
				{
					transaction = consolCost.Factory.Load<TransactionHeader>(consolCost.E6_AH_APInvoice) as TransactionHeaderWithLines;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					transaction = null;
				}
				result.AppendLine(transaction?.GetTransactionHeaderWithLinesInfo() ?? "<Posted transaction is not found>");
			}

			return result.ToString();

			void AddApportionmentChargesInfo()
			{
				var collection = chargeCollection == null || !chargeCollection.Any() ? consolCost.ApportionmentCharges.ToArray() : chargeCollection;

				if (collection.Any())
				{
					result.AppendLine(FormattableString.Invariant($"Apportionment Charges ({collection.Length}):"));
				}

				foreach (ApportionSplitCharge charge in collection)
				{
					result.AppendLine(charge.GetJobChargeInfo());
					if (charge.IsCostPosted)
					{
						result.AppendLine(charge.APLine?.GetTransactionLineInfo() ?? "<Posted line is not found>");
					}
					if (shouldAddChargesStackTraceInfo)
					{
						var jobChargeAmountCallStack = CriticalValidationInfoCollectorService.GetService(charge.Factory)?.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostAmountChanged);
						var apLineChangeCallStack = CriticalValidationInfoCollectorService.GetService(charge.Factory)?.GetInfo(charge.JR_AL_APLine, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenImportInvoice);
						var recordedMessage = FormattableString.Invariant($@"{ZArchitecture.Schema.JobChargeSchema.JR_OSCostAmt.Name} Stack Trace = {jobChargeAmountCallStack}
Relative AP Line construction stack trace = {apLineChangeCallStack}");
						result.AppendLine(recordedMessage);
					}
				}

				if (chargeCollection != null && chargeCollection.Any())
				{
					var chargeCollectionPKIndex = chargeCollection.ToDictionary(x => x.PK);
					var apportionmentChargesPKIndex = consolCost.ApportionmentCharges.Cast<BaseCharge>().ToDictionary(x => x.PK);

					var chargeCollectionNotInApportionmentCharges = chargeCollectionPKIndex.Where(x => !apportionmentChargesPKIndex.ContainsKey(x.Key)).Select(x => x.Value).ToArray();
					if (chargeCollectionNotInApportionmentCharges.Any())
					{
						result.AppendLine("Apportionment charges loaded in chargeCollection but not in Consol Cost collection:");
						chargeCollectionNotInApportionmentCharges.ForEach(x => result.AppendLine(x.GetJobChargeInfo()));
					}

					var apportionmentChargesNotInchargeCollection = apportionmentChargesPKIndex.Where(x => !chargeCollectionPKIndex.ContainsKey(x.Key)).Select(x => x.Value).ToArray();
					if (apportionmentChargesNotInchargeCollection.Any())
					{
						result.AppendLine("Apportionment charges in Consol Cost collection but was not in chargeCollection:");
						apportionmentChargesNotInchargeCollection.ForEach(x => result.AppendLine(x.GetJobChargeInfo()));
					}
				}
			}
		}

		public static string GetTransactionHeaderWithLinesInfo(this TransactionHeaderWithLines header, bool shouldAddStackTraceInfoForOSAmountIncorrect = false)
		{
			StringBuilder result = new StringBuilder();

			AppendHeaderWithLinesInfo(header, result, false);

			if (header.OriginalTransaction != null && header.OriginalTransaction is TransactionHeaderWithLines originalHeaderWithLines)
			{
				AppendHeaderWithLinesInfo(originalHeaderWithLines, result, true);
			}

			return result.ToString();

			void AppendHeaderWithLinesInfo(TransactionHeaderWithLines header, StringBuilder result, bool isOriginalInReversedTransation)
			{
				if (isOriginalInReversedTransation)
				{
					result.AppendLine("Original Transaction:");
				}
				result.AppendLine(header.GetTransactionHeaderInfo());
				if (header.Lines.Count > 0)
				{
					result.AppendLine("Line Details:");
					foreach (DependentTransactionLine line in header.Lines)
					{
						AppendLineInfo(line);
					}
				}
				else
				{
					var cacheOnlyQuery = new ZQuery() { FetchOnlyFromLocalCache = true, MaximumRows = 5 };
					cacheOnlyQuery.AddToFilter(AccTransactionLinesSchema.AL_AH, null);
					cacheOnlyQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, TransactionLineTypes.WIP);
					cacheOnlyQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, TransactionLineTypes.Accrual);

					var lines = header.Factory.Load<AccTransactionLines>(cacheOnlyQuery);
					result.AppendLine("The header has no lines. Top 5 suspicious lines in the factory:");
					foreach (var line in lines)
					{
						AppendLineInfo(line);
					}
				}
			}

			void AppendLineInfo(AccTransactionLines line)
			{
				result.AppendLine(line.GetTransactionLineInfo());

				if (shouldAddStackTraceInfoForOSAmountIncorrect)
				{
					var osAmountIncorrectCallStack = CriticalValidationInfoCollectorService.GetService(line.Factory)?.GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountIncorrectWhenPostingReceivableCharges);
					if (osAmountIncorrectCallStack != null)
					{
						result.AppendLine(osAmountIncorrectCallStack);
					}
				}
			}
		}

		public static string GetTransactionHeaderTaxAmountWithLinesTaxAmountInfo(this TransactionHeaderWithLines header)
		{
			var result = new ZStringBuilder();

			var headerTaxAmountInfo = FormattableString.Invariant($"Header: OSTaxAmount = {header.AH_OSTaxAmount}, LocalTaxAmount = {header.AH_LocalTaxAmount}");
			result.AppendLine(headerTaxAmountInfo);

			if (header.Lines.Count > 0)
			{
				result.AppendLine("Lines:");
				foreach (DependentTransactionLine line in header.Lines)
				{
					var lineInfo = FormattableString.Invariant($"OSExTaxAmount = {line.AL_OSExTaxAmount}, LocalExTaxAmount = {line.AL_LocalExTaxAmount}, OSTaxAmount = {line.AL_OSTaxAmount}, LocalTaxAmount = {line.AL_LocalTaxAmount}, Charge JR_OSSellGSTAmt_Calc = {line.RelatedJobCharge?.JR_OSSellGSTAmt_Calc}, Charge JR_Sell_LocalGSTAmount = {line.RelatedJobCharge?.JR_Sell_LocalGSTAmount}");
					result.AppendLine(lineInfo);
				}
			}

			return result.ToString();
		}

		public static string GetCashBasisVATInfo(this AccCashBasisVAT cashVATRecognitionRecord)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine(string.Format(
				"Cash VAT Recognition Record: PK = {0}, Post Date = {1}, Tax Base Amount = {2}, Tax Amount = {3}, Transaction Line = {4}, Company Code = {5}, Match Group Number = {6}, Is In DB = {7}, Has Changes = {8}.",
				cashVATRecognitionRecord.PK, cashVATRecognitionRecord.YC_PostDate.ToAUString(), cashVATRecognitionRecord.YC_TaxBaseAmount, cashVATRecognitionRecord.YC_TaxAmount, cashVATRecognitionRecord.YC_AL_TransactionLine,
				cashVATRecognitionRecord.Company.GC_Code, cashVATRecognitionRecord.YC_MatchGroupNum, cashVATRecognitionRecord.IsInDatabase.ToYesNoString(), cashVATRecognitionRecord.HasChanges.ToYesNoString())
				+ cashVATRecognitionRecord.GetFieldsWithChangesInfo());

			if (cashVATRecognitionRecord.TransactionLine != null)
			{
				result.AppendLine(cashVATRecognitionRecord.TransactionLine.GetTransactionLineInfo());
			}
			return result.ToString();
		}

		public static string GetExchangeRateOriginalInfo(this ExchangeRate exchangeRate)
		{
			string result;
			if (exchangeRate.IsDeleted && !((IBusinessObjectInternals)exchangeRate).Row.HasVersion(DataRowVersion.Original)) // Case for deleted, not previously saved ExchangeRate 
			{
				result = "Original property values for deleted and not previously saved BusinessObject are not accessible";
			}
			else // Works for not deleted and as well as deleted but previously saved ExchangeRate
			{
				result = FormattableString.Invariant($"ExchangeRate original values: PK = {exchangeRate.PK}, Currency Code = {exchangeRate.JF_RX_NKRateCurrencyInfo.OriginalValue}, Job Header PK = {exchangeRate.JF_JHInfo.OriginalValue}, Base Rate = {exchangeRate.JF_BaseRateInfo.OriginalValue}, CFX Minimum = {exchangeRate.JF_CFXMinimumInfo.OriginalValue}, CFX Percent = {exchangeRate.JF_CFXPercentInfo.OriginalValue}, Is Transformed = {exchangeRate.JF_IsTransformedInfo.OriginalValue}, Org Header PK = {exchangeRate.JF_OH_OrgInfo.OriginalValue}, Org Type = {exchangeRate.JF_OrgTypeInfo.OriginalValue}.");
			}

			return result;
		}

		public static string GetAccComplianceDocumentHeaderInfo(this AccComplianceDocumentHeader complianceDocumentHeader)
		{
			var result = new StringBuilder();
			var addressPKOfOrgCusCode = complianceDocumentHeader.Organisation.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(complianceDocumentHeader.ADH_DocumentType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_OA_PremisesAddress ?? ZGuid.Empty;
			var addressOfOrgCusCode = complianceDocumentHeader.Factory.Load<OrgAddress>(addressPKOfOrgCusCode)?.OA_Code ?? ZString.Empty;
			var addressPKOfTransactionHeaders = complianceDocumentHeader.TransactionHeaders.Cast<AccTransactionHeader>().Select(x => x.AH_OA_InvoiceAddressOverride);
			var addressOfParentTransaction = string.Join(",", complianceDocumentHeader.Factory.Load<OrgAddress>(new ZQuery(OrgAddressSchema.PK, addressPKOfTransactionHeaders)).Select(x => x.OA_Code).ToHashSet());
			var sendingDocumentAddress = complianceDocumentHeader.ADH_Ledger == LedgerTypes.AccountsReceivable ? complianceDocumentHeader.Organisation.AddressForSendingARDocuments?.OA_Code.ToString() : complianceDocumentHeader.Organisation.AddressForSendingAPDocuments?.OA_Code.ToString();

			result.AppendLine(string.Format(
				"Compliance Document Header: PK = {0}, Organization = {1}, Organization for Address = {2}, Address = {3}, Address Of Organization CustomeCode = {4}, Address Of Parent Transactiton = {5}, Sending Documents Address = {6}, Is In DB = {7}, Has Changes = {8}.",
				complianceDocumentHeader.PK,
				complianceDocumentHeader.Organisation.OH_Code,
				complianceDocumentHeader.AddressOverride.Header.OH_Code,
				complianceDocumentHeader.AddressOverride.OA_Code,
				addressOfOrgCusCode,
				addressOfParentTransaction,
				sendingDocumentAddress,
				complianceDocumentHeader.IsInDatabase.ToYesNoString(),
				complianceDocumentHeader.HasChanges.ToYesNoString())
				+ complianceDocumentHeader.GetFieldsWithChangesInfo());

			if (complianceDocumentHeader.ComplianceDocumentLines != null)
			{
				var selectedOnes = complianceDocumentHeader.ComplianceDocumentLines.Cast<AccComplianceDocumentLine>().SelectMany(x => x.TransactionHeaders);
#if NETFRAMEWORK
				var parentTransactionHeaders = selectedOnes.DistinctBy(x => x.PK);
#else
				var parentTransactionHeaders = System.Linq.Enumerable.DistinctBy(selectedOnes, x => x.PK);
#endif

				foreach (var parentTransactionHeader in parentTransactionHeaders)
				{
					result.AppendLine(parentTransactionHeader.GetTransactionHeaderInfo());
				}
			}
			return result.ToString();
		}
	}

	#endregion
}

