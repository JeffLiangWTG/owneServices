using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public static class AccountingConstants
	{
		public static class ApprovalCredentialOption
		{
			public const string SingleLogin = "ONE";
			public const string DoubleLogin = "TWO";
			public const string SequentialLogin = "SEQ";
		}

		public static class RevenueRecognitionDateConstants
		{
			public static ZDateTime Immediate { get { return ZDateTime.MinSmallDateTimeValue; } }

			public static ZDateTime MinSpecialDate { get { return ZDateTime.MaxSmallDateTime.AddMonths(-1); } }

			public static ZDateTime DateAfterLastPeriod { get { return DateTime.MaxValue; } }
			public static ZDateTime JobClosure { get { return ZDateTime.MaxSmallDateTime; } }

			// New Date Constants must be specified like MinSpecialDate.AddMinutes(XX)
			public static ZDateTime CustomsClearanceDate { get { return MinSpecialDate.AddMinutes(10); } }
			public static ZDateTime PostDateOfFirstARTransaction { get { return MinSpecialDate.AddMinutes(20); } }
		}

		public static class CreateColletionOrdersBatchOption
		{
			public const string GroupByDebtorAndDueDate = "GDD";
			public const string GroupByDebtor = "GDB";
			public const string GroupAllSelected = "ALL";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded filter type")]
		public static class DateFilterTypes
		{
			public const string None = "";
			public const string ETD = "ETD";
			public const string ETA = "ETA";
			public const string JobOpen = "Job Open";
			public const string JobClose = "Job Close";
			public const string PostingDate = "Posting Date";
			public const string DocumentDate = "Document Date";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded filter type")]
		public static class AmountFilterTypes
		{
			public const string None = "None";
			public const string ExTaxAmount = "Ex Tax Amount";
			public const string TaxAmount = "Tax Amount";
			public const string TotalAmount = "Total Amount";
		}

		public static class AmountComparisons
		{
			public const string EqualTo = "=";
			public const string GreaterThan = ">";
			public const string GreaterOrEqual = ">=";
			public const string LessThan = "<";
			public const string LessOrEqual = "<=";
		}

		public static class IssueStatementPackType
		{
			public const string Default = "DEF";
			public const string StatementOnly = "STA";
			public const string StatementAndInvoices = "ATT";
		}

		public static class ChequeLabelConstants
		{
			public static string ChequeAutoPrintedLabel
			{
				get { return Res.GetString("cf7786dd-ec11-4915-9546-b196eee0f17f", "Check Auto Printed"); }
			}
			public static string ChequeNumberIsAutoAllocatedLabel
			{
				get { return Res.GetString("34d7fed9-f455-465a-98cc-5ca208096d24", "Check Number Auto Allocated on Posting"); }
			}
		}

		public static class ChequeNumberAllocationErrorMessages
		{
			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
			public const string ChequeBookIsFullExceptionMessage = "Could not allocate check number using selected Check Book. The Check Book is full.\nPlease select another checkbook or change the LastNo for the current chquebook.";
			public static string ChequeBookIsBusyExceptionMessage
			{
				get { return Res.GetString("1458700d-1650-4765-b353-1cc0789d8c46", "The Check Book record is currently busy. Please try again."); }
			}
			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer only string")]
			public const string ChequeBookIsFullValidationMessage = "The Check Book is full.\r\nSelect another checkbook or change the Last Number for the current checkbook.";
		}

		public static class ENettErrorMessages
		{
			public static string ProcessCreditCardExceptionMessage
			{
				get { return Res.GetString("3c2629c8-1241-470a-8a04-cd7ae8b770ec", "Error making Credit Card payment over ComPay.  Please try again."); }
			}

			public static string OrganisationNotRegisteredForENett
			{
				get { return Res.GetString("06eb2894-cea7-4c5d-b7dd-99379d2cd708", "This organization is not registered for ComPay.  To setup the organization for ComPay, edit the organization under Details -> Config -> Registration Numbers / Codes."); }
			}
		}

		public static class AllocateErrorMessage
		{
			public static string LastExternalStorageExceptionMessage
			{
				get { return Res.GetString("DE5D8647-1737-46A8-AC53-A6F4F60CAF3C", "Editing or posting this request requires retrieval of eDocs from external storage. The attached eDocs are temporarily unavailable. Please wait approximately 1 minute and try again."); }
			}
		}

		public static class ReopenClosedJobSecurityMessages
		{
			public static string ErrorMessage
			{
				get
				{
					return Res.GetString("04cadad1-1224-40a9-8dfe-d734aa2e40d9", "As you do not have the access right to re-open the Job, you will require authorization to proceed upon posting this transaction.");
				}
			}

			public static string WarningMessage
			{
				get
				{
					return Res.GetString("5ba87d0d-a6b0-4bba-a9c4-95815e76f121", "The job will be reopened after posting this transaction.");
				}
			}
		}

		public static class ProfitShareErrorMessages
		{
			public static string InvalidRegistry(IRegistryItemInternals registryItem)
			{
				return Res.GetString("7c960712-19eb-4429-b158-42815815ac58", "Incorrect Registry Item value.\r\nPlease set up a correct value to the Registry Item: '{0}'.", registryItem.Location);
			}

			public static string UnsavedChanges
			{
				get
				{
					return Res.GetString("ecad9780-5b78-4a78-9da0-6633f15f8835", "Please save this job before creating Profit Share charges.");
				}
			}

			public static string InvalidDebtor(OrgHeader profitShareParty, string partyType)
			{
				return Res.GetString("3940C9E0-A5E4-4CB3-BAE3-AB6BE0F81451", "In order to create profit share charges for {0} party, {1} has to be marked as {2}", partyType, profitShareParty.OH_Code, profitShareParty.OH_IsDebtorInfo.HumanReadableName);
			}

			public static string InvalidCreditor(ZString orgCode)
			{
				return Res.GetString("19f09c1f-ec30-4d58-af6b-2daea7cce85d", "Organization {0} is not a valid creditor", orgCode);
			}
		}

		public static class PrefixTypes
		{
			public const string HouseBillNumberPrefix = "H:";
			public const string MasterBillNumberPrefix = "M:";
			public const string CarrierPrincipalPrefix = "P:";
			public const string ConsolNumberPrefix = "C:";
			public const string CustomsEntryNumberPrefix = "E:";
			public const string FlightVoyageNumberPrefix = "V:";
			public const string OrderNumberPrefix = "O:";
			public const string BookingReferenceNumberPrefix = "B:";
			public const string CoLoadMasterBillNumberPrefix = "L:";
			public const string ContainerNumberPrefix = "T:";
			public const string ShipmentNumberPrefix = "S:";
		}

		public static class CostConfirmationDocumentSettingsCodes
		{
			public const string Summary = "SMY";
			public const string Detail = "DTL";
			public const string Both = "BTH";
		}

		public static CodeDescriptionPairList CostConfirmationDocumentSettingList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(CostConfirmationDocumentSettingsCodes.Summary, ResString.GetMultilingualString("CE1E20A5-9058-4bb5-878C-E1591CE62FE2", "Summary"));
				lookUpList.AddPair(CostConfirmationDocumentSettingsCodes.Detail, ResString.GetMultilingualString("B3FD7BDE-F88F-4b3f-BFAA-5C9FE403EF88", "Detail"));
				lookUpList.AddPair(CostConfirmationDocumentSettingsCodes.Both, ResString.GetMultilingualString("26A4829C-99A8-44f1-BBF3-5882C10E396B", "Both"));
				return lookUpList;
			}
		}

		public static class GLDTypeCodes
		{
			public const string Posting = "PST";
			public const string Recognition = "REC";
			public const string RealizeCashBasisVAT = "CBV";
			public const string RealizeTaxGLMovement = "TGM";
		}

		public static CodeDescriptionPairList GLDTypeList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(GLDTypeCodes.Posting, ResString.GetMultilingualString("0cefd097-b6b6-4a07-b5c3-0a65d03246e0", "Posting"));
				lookUpList.AddPair(GLDTypeCodes.Recognition, ResString.GetMultilingualString("ddee3284-a54f-4dd3-ad93-ec7f8140117c", "Recognition"));
				lookUpList.AddPair(GLDTypeCodes.RealizeCashBasisVAT, ResString.GetMultilingualString("a9ca25a2-c0ea-4678-818c-0daa52e56fc9", "Realize Cash Basis VAT"));
				lookUpList.AddPair(GLDTypeCodes.RealizeTaxGLMovement, ResString.GetMultilingualString("d2a98140-5cdc-4477-b70e-fe30f11bd600", "Realize Tax GL Movement"));
				return lookUpList;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code Lookup Prefix Does Not Need To Be Localised")]
		public static class VietnamEInvoicingReceivingFileTypeCodes
		{
			public const string PDF = "pdf";
			public const string JSON = "json";
			public const string XML = "xml";
		}

		public static CodeDescriptionPairList VietnamEInvoicingReceivingFileTypeList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(VietnamEInvoicingReceivingFileTypeCodes.PDF, ResString.GetMultilingualString("71d16aec-0e80-4ffa-84ff-c8a2111f9446", "PDF file"));
				lookUpList.AddPair(VietnamEInvoicingReceivingFileTypeCodes.JSON, ResString.GetMultilingualString("4779d896-5b6d-4ddc-bf28-5512e219e15e", "JSON file"));
				lookUpList.AddPair(VietnamEInvoicingReceivingFileTypeCodes.XML, ResString.GetMultilingualString("2ccf9851-112b-4250-a076-f1c72937a386", "XML file"));
				return lookUpList;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
		public enum InvoiceRoundCurrencyUnit
		{
			FiveMinor = 5,
			TenMinor = 10,
			TwentyMinor = 20,
			FiftyMinor = 50,
			OneMajor = 100
		}

		public static class RoundToCurrencyUnits
		{
			public const string FiveMinorUnits = "0.05";
			public const string TenMinorUnits = "0.10";
			public const string TwentyMinorUnits = "0.20";
			public const string FiftyMinorUnits = "0.50";
			public const string OneMajorUnit = "1.00";
		}

		public static CodeDescriptionPairList RoundToCurrencyUnitsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(RoundToCurrencyUnits.FiveMinorUnits, ResString.GetMultilingualString("b7e64bd1-9f14-4a9c-a859-8456f2633e1e", "Nearest 5 minor units"));
				result.AddPair(RoundToCurrencyUnits.TenMinorUnits, ResString.GetMultilingualString("1c03c273-03db-410b-a069-201e762536f2", "Nearest 10 minor units"));
				result.AddPair(RoundToCurrencyUnits.TwentyMinorUnits, ResString.GetMultilingualString("c1b7145e-a2e7-44f0-ad43-3990d6ad1aa3", "Nearest 20 minor units"));
				result.AddPair(RoundToCurrencyUnits.FiftyMinorUnits, ResString.GetMultilingualString("39aa5cd0-2cc9-4eda-86f3-6c930fed7a5d", "Nearest 50 minor units"));
				result.AddPair(RoundToCurrencyUnits.OneMajorUnit, ResString.GetMultilingualString("396a5550-a9b9-4982-af2c-4d28c9cef5ba", "Nearest 1 major unit"));
				return result;
			}
		}

		public enum InvoiceRoundingOption
		{
			ARU,
			ARD,
			RBM
		}

		public static class RoundingOptionsCodes
		{
			public const string AlwaysRoundUp = "ARU";
			public const string AlwaysRoundDown = "ARD";
			public const string RoundBasedOnMidpoint = "RBM";
		}

		public static CodeDescriptionPairList RoundingOptionsList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(RoundingOptionsCodes.AlwaysRoundUp, ResString.GetMultilingualString("bbfbb117-8923-4651-96e1-d439649502c6", "Always Round Up"));
				result.AddPair(RoundingOptionsCodes.AlwaysRoundDown, ResString.GetMultilingualString("1439caa1-60b3-47d8-ac54-9c21b5d966a3", "Always Round Down"));
				result.AddPair(RoundingOptionsCodes.RoundBasedOnMidpoint, ResString.GetMultilingualString("65935635-005e-4bad-a6ec-91c1318bde91", "Round Based on a Midpoint"));
				return result;
			}
		}

		public static CodeDescriptionPairList ZeroAmountTaxTypesDescriptionList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(AccTaxRate.Types.Rated, ResString.GetMultilingualString("168471f6-1da5-439c-bfb7-f7fd0175cd92", "Zero Rated"));
				lookUpList.AddPair(AccTaxRate.Types.Exempt, ResString.GetMultilingualString("da01d0e2-cd30-4374-8e78-b9da177fcfcd", "Exempt"));
				lookUpList.AddPair(AccTaxRate.Types.NotReportable, ResString.GetMultilingualString("31cf63c9-4ce1-457d-a21e-f8b71b80bed0", "Not Applicable"));
				lookUpList.AddPair(AccTaxRate.Types.ReverseRated, ResString.GetMultilingualString("c5b0c91e-3af2-404f-baa9-e4c047c0d381", "Reverse Charge"));
				lookUpList.AddPair(AccTaxRate.Types.Suspended, ResString.GetMultilingualString("daf380c6-8a54-4c28-8a6a-97c52d11ff1e", "Suspended"));
				lookUpList.AddPair(AccTaxRate.Types.ReportableUnderBusinessTax, ResString.GetMultilingualString("915a7e30-787f-48d1-87db-f0b8887ede59", "Reportable Under Business Tax"));
				lookUpList.AddPair(AccTaxRate.Types.ExcludedFromTheTaxBase, ResString.GetMultilingualString("8353851f-9e11-428b-95ac-a2be6872b1b1", "Excluded"));
				lookUpList.AddPair(AccTaxRate.Types.CapitalRated, ResString.GetMultilingualString("5337d599-a0e2-45e2-abc0-0e0c7560cbe8", "Zero Rated"));
				return lookUpList;
			}
		}

		public static class VoucherAppointedPartiesCode
		{
			public const string VoucherEnter = "REC";
			public const string VoucherPost = "PST";
			public const string VoucherCasher = "PAY";
			public const string VoucherReviewer = "CHK";
		}
		public static class VoucherItemRegistryCode
		{
			public const string BadDebtWriteOff = "REVBD";
			public const string ReversalRelated = "REVRT";
			public const string JobARCreditNote = "REVCN";
			public const string ConsolARCreditNote = "RECCN";
			public const string JobARInvoice = "REVJB";
			public const string ConsolARInvoice = "REVCS";
			public const string ARPeriodicInvoice = "REVPE";
			public const string ARPeriodicCreditNote = "REPCN";
			public const string OverpaymentRelated = "RODRU";
			public const string BankFeeJournal = "RBDRU";
			public const string ExchangeDiff = "REDRU";
			public const string DiscountRelated = "RDDRU";

			public const string UnOverpaymentRelated = "UNROD";
			public const string UnBankFeeJournal = "UNRBD";
			public const string UnExchangeDiff = "UNRED";
			public const string UnDiscountRelated = "UNRDD";

			public const string UNPaymentTransactionType = "UNA";

			public const string CASS = "CASSD";
			public const string FinanceCharge = "FCFBT";
			public const string JCCFX = "JCCFX";
			public const string BJGDM = "BJGDM";
			public const string ARCASH = "ARCAR";
			public const string APCASH = "APCAR";
			public const string JobAPInvoice = "APJIN";

			public const string OutstandingBalanceCurrencyAdjustmentJournal = "OBCAJ";
		}

		public static class DisbursementShortfallSurplusCode
		{
			public const string DisbursementShortfallSurplus = "DSSJC";
		}

		public static class MatchingDefaultDesc
		{
			public static string DiscountDesc
			{
				get
				{
					return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.DiscountRelated,
							 Res.GetString("51e023a3-81e4-46c9-a81f-9657144f6e6b", "DISCOUNT RELATING TO MATCH NO"));
				}
			}
			public static string OverpaymentDesc
			{
				get
				{
					return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.OverpaymentRelated,
														 Res.GetString("5176e2a8-c98e-4d74-afef-7a1b49531768", "OVERPAYMENT RELATING TO MATCH NO"));
				}
			}
			public static string ExchangeDifferenceDesc
			{
				get
				{
					return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ExchangeDiff,
														 Res.GetString("92cb9f56-62bf-46c2-8e06-04c3c166ea94", "EXCHANGE DIFF RELATING TO MATCH NO"));
				}
			}

			public static string BankFeeDesc
			{
				get
				{
					return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.BankFeeJournal,
														 Res.GetString("6e08f7e2-1a0a-4fa8-80f8-429d12b0964a", "BANK FEE JOURNAL RELATING TO MATCH NO"));
				}
			}
		}

		public class ClearingJournalConfigurationTypes : CodeDescriptionPairList
		{
			public static CodeDescriptionPair Standard
			{
				get { return new CodeDescriptionPair("STD", ResString.GetMultilingualString("50DD1BA6-0FFE-49CA-A6FB-24A33E7A7090", "Standard Contra and Transfer behaviors")); }
			}
			public static CodeDescriptionPair HeaderBranch
			{
				get { return new CodeDescriptionPair("HBR", ResString.GetMultilingualString("168B1FF8-9F26-49CD-B24F-F871DFE8CBFE", "Header Level Clearing Journals")); }
			}
			public static CodeDescriptionPair LineBranchPerHeader
			{
				get { return new CodeDescriptionPair("LBR", ResString.GetMultilingualString("2941B633-B959-452C-B338-E898237A3735", "Line Level Clearing Journals")); }
			}
			public static CodeDescriptionPair LineBranchPerMatching
			{
				get { return new CodeDescriptionPair("LBX", ResString.GetMultilingualString("248EE153-02B9-4240-8C1D-688BE683B947", "Line Level Clearing Journals when different Branch/Ledger/Organizations are being matched")); }
			}

			public ClearingJournalConfigurationTypes()
			{
				Add(Standard);
				Add(HeaderBranch);
				Add(LineBranchPerHeader);
				Add(LineBranchPerMatching);
			}
		}

		public enum DebtorCreditorTerms { DebtorTerms, CreditorTerms }

		public static class ReversalDueDateCalculation
		{
			public const string DebtorsTerms = "DEB";
			public const string CreditorsTerms = "CRD";
			public const string OriginalInvoiceDueDate = "OID";
		}

		public class ReversalDueDateCalculationTypes : CodeDescriptionPairList
		{
			public static CodeDescriptionPair DebtorsTerms
			{
				get { return new CodeDescriptionPair(ReversalDueDateCalculation.DebtorsTerms, ResString.GetMultilingualString("c71ab7e4-07b4-49fb-b38f-a16937b2fa7f", "Default according to debtor's terms")); }
			}

			public static CodeDescriptionPair CreditorsTerms
			{
				get { return new CodeDescriptionPair(ReversalDueDateCalculation.CreditorsTerms, ResString.GetMultilingualString("d80e965b-d7e3-4e77-a2b1-79c4a5675558", "Default according to creditor's terms")); }
			}

			public static CodeDescriptionPair OriginalInvoiceDueDate
			{
				get { return new CodeDescriptionPair(ReversalDueDateCalculation.OriginalInvoiceDueDate, ResString.GetMultilingualString("443cbc8a-b71a-4573-a254-d4449a7e3e73", "Default to original invoice's due date")); }
			}

			public ReversalDueDateCalculationTypes(DebtorCreditorTerms terms)
			{
				if (terms == DebtorCreditorTerms.DebtorTerms)
				{
					Add(DebtorsTerms);
				}
				else if (terms == DebtorCreditorTerms.CreditorTerms)
				{
					Add(CreditorsTerms);
				}
				Add(OriginalInvoiceDueDate);
			}
		}

		public static class PaymentReceiptXUBTypes
		{
			public static readonly ZString AccountingReceipt = "AccountingReceipt";
			public static readonly ZString AccountingPayment = "AccountingPayment";
			public static readonly ZString AccountingMatching = "AccountingMatching";

			public static ZString[] GetTypes() => new[] { AccountingReceipt, AccountingPayment, AccountingMatching };
		}

		public static class ReversalDefaultFromOriginalTransactionDate
		{
			public const string TodaysDate = "TOD";
			public const string DefaultFromOriginalTransactionInvoiceDateOrCurrentDate = "OCD";
			public const string DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod = "OFD";
			public const string DefaultFromOriginalTransactionPostDateOrCurrentDate = "OPD";
			public const string DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod = "OPF";
		}

		public class ReversalDefaultFromOriginalInvoiceDateTypes : CodeDescriptionPairList
		{
			public static CodeDescriptionPair TodaysDate
			{
				get { return new CodeDescriptionPair(ReversalDefaultFromOriginalTransactionDate.TodaysDate, ResString.GetMultilingualString("855b93ed-1c8f-4de4-b61c-bf9acfa1d7ec", "Default to today's date")); }
			}

			public static CodeDescriptionPair DefaultFromOriginalTransactionInvoiceDateOrCurrentDate
			{
				get { return new CodeDescriptionPair(ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrCurrentDate, ResString.GetMultilingualString("13f7713d-8497-499d-ab40-fe1560d230e2", "Default from Original Transaction Invoice Date or current date if original date is in closed period")); }
			}

			public static CodeDescriptionPair DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod
			{
				get { return new CodeDescriptionPair(ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod, ResString.GetMultilingualString("05a06df3-7eef-4b02-b03f-488711bbb2a5", "Default from Original Transaction Invoice Date or first day of first open period if original date is in closed period")); }
			}

			public static CodeDescriptionPair DefaultFromOriginalTransactionPostDateOrCurrentDate
			{
				get { return new CodeDescriptionPair(ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrCurrentDate, ResString.GetMultilingualString("49cd37ee-167b-466f-a393-fb64b5977f85", "Default from Original Transaction Post Date or current date if original date is in closed period")); }
			}

			public static CodeDescriptionPair DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod
			{
				get { return new CodeDescriptionPair(ReversalDefaultFromOriginalTransactionDate.DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod, ResString.GetMultilingualString("4429b8cb-54b1-4c93-8606-316e97223275", "Default from Original Transaction Post Date or first day of first open period if original date is in closed period")); }
			}

			public ReversalDefaultFromOriginalInvoiceDateTypes()
			{
				Add(TodaysDate);
				Add(DefaultFromOriginalTransactionInvoiceDateOrCurrentDate);
				Add(DefaultFromOriginalTransactionInvoiceDateOrFirstDayOfFirstOpenPeriod);
				Add(DefaultFromOriginalTransactionPostDateOrCurrentDate);
				Add(DefaultFromOriginalTransactionPostDateOrFirstDayOfFirstOpenPeriod);
			}
		}

		public sealed class PartPaymentTaxRealizationRuleTypes : CodeDescriptionPairList
		{
			public PartPaymentTaxRealizationRuleTypes()
			{
				Add(Proportionally);
				Add(TaxFirst);
			}

			public static CodeDescriptionPair Proportionally { get { return new CodeDescriptionPair("PRP", ResString.GetMultilingualString("2C70FFEF-9C49-467E-8239-A168AF154279", "Tax is reportable proportionally")); } }
			public static CodeDescriptionPair TaxFirst { get { return new CodeDescriptionPair("1ST", ResString.GetMultilingualString("C7164318-347A-4808-A53A-9D0B5638A43C", "Part Payments Realize Tax First")); } }
		}

		public sealed class DescriptionInDocumentsForTaxAmountsRuleTypes : CodeDescriptionPairList
		{
			public DescriptionInDocumentsForTaxAmountsRuleTypes()
			{
				Add(Both);
				Add(TaxRate);
				Add(TaxAmount);
			}

			public static CodeDescriptionPair Both { get { return new CodeDescriptionPair("BTH", ResString.GetMultilingualString("5694538e-9171-4768-b092-8d51e06bb66b", "Print Both Tax Rate and Tax Amount")); } }
			public static CodeDescriptionPair TaxRate { get { return new CodeDescriptionPair("RAT", ResString.GetMultilingualString("8f2b7839-063f-4d52-8883-b8d42c5f2376", "Only Print Tax Rate")); } }
			public static CodeDescriptionPair TaxAmount { get { return new CodeDescriptionPair("AMT", ResString.GetMultilingualString("d34bfe0c-460d-4165-96f4-a21eb01bd852", "Only Print Tax Amount")); } }
		}

		public sealed class ItalyTaxRegimeIdTypes : CodeDescriptionPairList
		{
			public ItalyTaxRegimeIdTypes()
			{
				Add(RF01);
				Add(RF02);
				// No RF03
				Add(RF04);
				Add(RF05);
				Add(RF06);
				Add(RF07);
				Add(RF08);
				Add(RF09);
				Add(RF10);
				Add(RF11);
				Add(RF12);
				Add(RF13);
				Add(RF14);
				Add(RF15);
				Add(RF16);
				Add(RF17);
				Add(RF18);
				Add(RF19);
			}

			public static CodeDescriptionPair RF01 { get { return new CodeDescriptionPair("RF01", (NoResString)"Ordinario"); } }
			public static CodeDescriptionPair RF02 { get { return new CodeDescriptionPair("RF02", (NoResString)"Contribuenti minimi (art.1, c.96-117, L. 244/07)"); } }
			public static CodeDescriptionPair RF04 { get { return new CodeDescriptionPair("RF04", (NoResString)"Agricoltura e attività connesse e pesca (artt.34 e 34-bis, DPR 633/72)"); } }
			public static CodeDescriptionPair RF05 { get { return new CodeDescriptionPair("RF05", (NoResString)"Vendita sali e tabacchi (art.74, c.1, DPR. 633/72)"); } }
			public static CodeDescriptionPair RF06 { get { return new CodeDescriptionPair("RF06", (NoResString)"Commercio fiammiferi (art.74, c.1, DPR  633/72)"); } }
			public static CodeDescriptionPair RF07 { get { return new CodeDescriptionPair("RF07", (NoResString)"Editoria (art.74, c.1, DPR  633/72)"); } }
			public static CodeDescriptionPair RF08 { get { return new CodeDescriptionPair("RF08", (NoResString)"Gestione servizi telefonia pubblica (art.74, c.1, DPR 633/72)"); } }
			public static CodeDescriptionPair RF09 { get { return new CodeDescriptionPair("RF09", (NoResString)"Rivendita documenti di trasporto pubblico e di sosta (art.74, c.1, DPR  633/72)"); } }
			public static CodeDescriptionPair RF10 { get { return new CodeDescriptionPair("RF10", (NoResString)"Intrattenimenti, giochi e altre attività di cui alla tariffa allegata al DPR 640/72 (art.74, c.6, DPR 633/72)"); } }
			public static CodeDescriptionPair RF11 { get { return new CodeDescriptionPair("RF11", (NoResString)"Agenzie viaggi e turismo (art.74-ter, DPR 633/72)"); } }
			public static CodeDescriptionPair RF12 { get { return new CodeDescriptionPair("RF12", (NoResString)"Agriturismo (art.5, c.2, L. 413/91)"); } }
			public static CodeDescriptionPair RF13 { get { return new CodeDescriptionPair("RF13", (NoResString)"Vendite a domicilio (art.25-bis, c.6, DPR  600/73)"); } }
			public static CodeDescriptionPair RF14 { get { return new CodeDescriptionPair("RF14", (NoResString)"Rivendita beni usati, oggetti d’arte, d’antiquariato o da collezione (art.36, DL 41/95)"); } }
			public static CodeDescriptionPair RF15 { get { return new CodeDescriptionPair("RF15", (NoResString)"Agenzie di vendite all’asta di oggetti d’arte, antiquariato o da collezione (art.40-bis, DL 41/95)"); } }
			public static CodeDescriptionPair RF16 { get { return new CodeDescriptionPair("RF16", (NoResString)"IVA per cassa P.A. (art.6, c.5, DPR 633/72)"); } }
			public static CodeDescriptionPair RF17 { get { return new CodeDescriptionPair("RF17", (NoResString)"IVA per cassa (art. 32-bis, DL 83/2012)"); } }
			public static CodeDescriptionPair RF18 { get { return new CodeDescriptionPair("RF18", (NoResString)"Altro"); } }
			public static CodeDescriptionPair RF19 { get { return new CodeDescriptionPair("RF19", (NoResString)"Regime forfettario (art.1, c.54-89, L. 190/2014)"); } }
		}

		public class InvoicePostingExchangeRateOption : CodeDescriptionPairList
		{
			public InvoicePostingExchangeRateOption()
				: base()
			{
				Add(Default);
				Add(TodayExchangeRate);
				Add(ExchangeRateBasedOnInvoiceDate);
				Add(ExchangeRateBasedOnPostDate);
				Add(EarliestOfInvoiceOrTaxDate);
			}

			public static CodeDescriptionPair Default { get { return new CodeDescriptionPair("DEF", ResString.GetMultilingualString("93c6ebcc-ce82-407b-97ef-2b4072b05eb7", "Default")); } }
			public static CodeDescriptionPair TodayExchangeRate { get { return new CodeDescriptionPair("TOD", ResString.GetMultilingualString("c5d5e9ef-bdc9-4f13-ab17-c5cd5b230e31", "Today Exchange Rate")); } }
			public static CodeDescriptionPair ExchangeRateBasedOnInvoiceDate { get { return new CodeDescriptionPair("INV", ResString.GetMultilingualString("7937ad9f-53f1-4bdd-b701-34d9c6d7598e", "Exchange Rate based on Invoice Date")); } }
			public static CodeDescriptionPair ExchangeRateBasedOnPostDate { get { return new CodeDescriptionPair("PST", ResString.GetMultilingualString("745560c2-b4f4-47e0-b5f7-99fe2f8b4efe", "Exchange Rate based on Post Date")); } }
			public static CodeDescriptionPair EarliestOfInvoiceOrTaxDate { get { return new CodeDescriptionPair("EIT", ResString.GetMultilingualString("3b5eac5f-33ae-4e43-986f-8f04e61191f6", "Earliest of Invoice Date and Tax Date")); } }

			public static CodeDescriptionPairList CodeList
			{
				get
				{
					return new InvoicePostingExchangeRateOption();
				}
			}
		}

		public class PrintTaxDateInARInvoiceDocumentOption : CodeDescriptionPairList
		{
			public PrintTaxDateInARInvoiceDocumentOption()
				: base()
			{
				Add(DoNotPrintTaxDate);
				Add(PrintTaxDateInBody);
				Add(PrintTaxDateInHeaderEarliest);
				Add(PrintTaxDateInHeaderLatest);
			}

			public static CodeDescriptionPair DoNotPrintTaxDate { get { return new CodeDescriptionPair("NOT", ResString.GetMultilingualString("ebadcbc0-e13e-40ad-9cd1-f668598fbc67", "Do not print tax date")); } }
			public static CodeDescriptionPair PrintTaxDateInBody { get { return new CodeDescriptionPair("BOD", ResString.GetMultilingualString("8882c34e-26f8-4e71-a23c-b5a9d7bf0864", "Print tax date on each charge line")); } }
			public static CodeDescriptionPair PrintTaxDateInHeaderEarliest { get { return new CodeDescriptionPair("HDR", ResString.GetMultilingualString("609e7c9a-300c-41b0-ae60-79420bd2095c", "Print tax date in header (earliest)")); } }
			public static CodeDescriptionPair PrintTaxDateInHeaderLatest { get { return new CodeDescriptionPair("HDT", ResString.GetMultilingualString("e7047120-e1be-4b21-a3ce-3cb70a6ccb0e", "Print tax date in header (latest)")); } }

			public static CodeDescriptionPairList CodeList => new PrintTaxDateInARInvoiceDocumentOption();
		}

		public sealed class TaxMessageMandatoryOptions : CodeDescriptionPairList
		{
			TaxMessageMandatoryOptions()
				: base()
			{
				Add(new CodeDescriptionPair(Constants.TaxMessageMandatoryOptionConstants.NotRequired, ResString.GetMultilingualString("162CFFE1-43F7-49C4-B84E-514FD0307A14", "Not Required")));
				Add(new CodeDescriptionPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero, ResString.GetMultilingualString("007305E4-363D-4009-803A-031947AD4DAC", "Required When Tax is Zero")));
				Add(new CodeDescriptionPair(Constants.TaxMessageMandatoryOptionConstants.RequiredAlways, ResString.GetMultilingualString("8FD3167B-646E-4C8E-BF91-B4F5DDA9DAB1", "Required Always")));
				Add(new CodeDescriptionPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero, ResString.GetMultilingualString("02A13E6A-3D7F-4BFC-9335-73FC9FE96435", "Required When Extra Tax Is Not Zero")));
				Add(new CodeDescriptionPair(Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax, ResString.GetMultilingualString("C2E4CFC1-6478-4883-9DCE-C6B53D7B02CE", "Required When the Tax ID Has an Extra Tax Element or Required When Tax Is Zero")));
			}

			public static CodeDescriptionPairList CodeList => new TaxMessageMandatoryOptions();
		}

		public sealed class CommentChargeLineARInvoiceWarningOptions : CodeDescriptionPairList
		{
			CommentChargeLineARInvoiceWarningOptions()
				: base()
			{
				Add(new CodeDescriptionPair(NoAction, (NoResString)"No Action"));
				Add(new CodeDescriptionPair(WarningValidation, (NoResString)"Warning Validation"));
				Add(new CodeDescriptionPair(ErrorValidation, (NoResString)"Error Validation"));
			}

			public const string NoAction = "NON";
			public const string WarningValidation = "WRN";
			public const string ErrorValidation = "ERR";

			public static CodeDescriptionPairList CodeList => new CommentChargeLineARInvoiceWarningOptions();
		}

		public sealed class AgingOptions : CodeDescriptionPairList
		{
			AgingOptions()
				: base()
			{
				Add(new CodeDescriptionPair(InvoiceDate, ResString.GetMultilingualString("4E6E6743-9F4F-481D-A92E-93DF2590BC71", "Transactions will be aged by Invoice Date")));
				Add(new CodeDescriptionPair(DueDate, ResString.GetMultilingualString("2DB8C010-B4C5-4F76-9B27-4D828A729D5D", "Transactions will be aged by Due Date")));
				Add(new CodeDescriptionPair(PostDate, ResString.GetMultilingualString("31C9E362-16FC-4F69-9F67-1ED733BC6917", "Transactions will be aged by Post Date")));
			}

			public const string InvoiceDate = "INV";
			public const string DueDate = "DUE";
			public const string PostDate = "PST";

			public static CodeDescriptionPairList CodeList => new AgingOptions();
		}

		public class NettingModeOption : CodeDescriptionPairList
		{
			public NettingModeOption()
				: base()
			{
				Add(CompanyLevel);
				Add(OrganizationLevel);
			}

			public static CodeDescriptionPair CompanyLevel { get { return new CodeDescriptionPair("COM", ResString.GetMultilingualString("4289214a-dfd0-4719-ae89-4a6b1eeb2047", "Company Level")); } }
			public static CodeDescriptionPair OrganizationLevel { get { return new CodeDescriptionPair("ORG", ResString.GetMultilingualString("36eb64c3-1f2a-4e66-b074-ecbf45d553a2", "Organization Level")); } }

			public static CodeDescriptionPairList CodeList => new NettingModeOption();
		}

		public class EnforceZeroBalanceDisbursementsOption : CodeDescriptionPairList
		{
			public EnforceZeroBalanceDisbursementsOption()
				: base()
			{
				Add(Default);
				Add(OSAmount);
				Add(LocalAmount);
				Add(Either);
				Add(Both);
			}

			public static CodeDescriptionPair Default { get { return new CodeDescriptionPair("DEF", ResString.GetMultilingualString("6AB39ABF-88AE-420C-9483-8FD7F1FF8098", "Default")); } }
			public static CodeDescriptionPair OSAmount { get { return new CodeDescriptionPair("CUR", ResString.GetMultilingualString("DB82DF26-0AD3-452B-8E82-61C298A7FF67", "Validate OS Currency and Amounts are Equal")); } }
			public static CodeDescriptionPair LocalAmount { get { return new CodeDescriptionPair("LOC", ResString.GetMultilingualString("8B9D3A22-52AA-41AE-94C7-FFB2DFB3D2E7", "Validate Local Amounts are Equal")); } }
			public static CodeDescriptionPair Either { get { return new CodeDescriptionPair("EIT", ResString.GetMultilingualString("2C37F5B2-D2E0-402D-BF67-6492D888AC58", "Validate Either OS Currency and Amounts or Local Amounts are Equal")); } }
			public static CodeDescriptionPair Both { get { return new CodeDescriptionPair("BTH", ResString.GetMultilingualString("7A801920-00B9-47B3-AE41-19F00D8E744E", "Validate Both OS Currency and Amounts and Local Amounts are Equal")); } }

			public static CodeDescriptionPairList CodeList
			{
				get
				{
					return new EnforceZeroBalanceDisbursementsOption();
				}
			}
		}

		public static class TransactionReferenceTypes
		{
			public const string ConsolNumber = "CON";
			public const string VesselVoyage = "VSV";
			public const string ConsolContainerNumber = "CCN";
			public const string MasterBill = "MBL";
			public const string HouseBill = "HBL";
			public const string ShipmentNumber = "SHN";
			public const string CarrierBookingReference = "CBR";
			public const string AgentReference = "AGR";
			public const string PackedContainer = "PAC";
			public const string OrderReferences = "ORD";
			public const string InvoiceTransactionNumber = "INV";
			public const string JobInvoiceNumber = "JOB";
		}

		public static class RemittanceFileRowTypes
		{
			public const string Receipt = "REC";
			public const string Payment = "PAY";
			public const string MatchHeader = "MHR";
			public const string PaidTransaction = "PTR";
			public const string NettingClearingLine = "NCL";
		}

		public sealed class InvAndPstDateDefaultingRuleTypes : CodeDescriptionPairList
		{
			public InvAndPstDateDefaultingRuleTypes()
			{
				Add(Default);
				Add(MonthEndSuspension);
			}

			public static CodeDescriptionPair Default { get { return new CodeDescriptionPair("DEF", ResString.GetMultilingualString("abff6b9e-c3b9-4c88-8b5a-080339d1d395", "Default")); } }
			public static CodeDescriptionPair MonthEndSuspension { get { return new CodeDescriptionPair("MTH", ResString.GetMultilingualString("c4bf230a-9c7e-4a5c-ad4b-bbfd2f26116f", "Month End Suspension")); } }
		}

		public sealed class JobRevenueJournalGLAccountDefaultingRuleTypes : CodeDescriptionPairList
		{
			public JobRevenueJournalGLAccountDefaultingRuleTypes()
			{
				Add(Cost);
				Add(Revenue);
				Add(Both);
			}

			public static CodeDescriptionPair Cost { get { return new CodeDescriptionPair("CST", ResString.GetMultilingualString("6d93fb02-6b64-45dc-a60b-e5e1680bbb60", "Debit and Credit Cost GL Account")); } }
			public static CodeDescriptionPair Revenue { get { return new CodeDescriptionPair("REV", ResString.GetMultilingualString("2d23f3a0-d602-47f4-8e62-76522e3b8afb", "Debit and Credit Revenue GL Account")); } }
			public static CodeDescriptionPair Both { get { return new CodeDescriptionPair("BTH", ResString.GetMultilingualString("39b07bb8-a47c-4db6-b2f1-12f28cd2094e", "Debit Cost GL Account, Credit Revenue GL Account")); } }
		}

		public sealed class ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes : CodeDescriptionPairList
		{
			public ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes()
			{
				Add(Default);
				Add(Increased);
				Add(BothIncreasedOrDecreased);
			}

			public static CodeDescriptionPair Default { get { return new CodeDescriptionPair("DEF", ResString.GetMultilingualString("59964BD3-5C48-4601-A8DF-9EFF9E8EF2BE", "No comparison made (this is the registry default)")); } }
			public static CodeDescriptionPair Increased { get { return new CodeDescriptionPair("INC", ResString.GetMultilingualString("B74A8228-BFD0-4584-B2F2-BF7FF49981FE", "Create new credit control request only when sell values have increased")); } }
			public static CodeDescriptionPair BothIncreasedOrDecreased { get { return new CodeDescriptionPair("BTH", ResString.GetMultilingualString("2767483D-F905-4B75-80CD-FBA33AEF0705", "Create new credit control request when sell values have both increased or decreased")); } }
		}

		public sealed class MatchStatusTypes : CodeDescriptionPairList
		{
			public MatchStatusTypes()
			{
				Add(Unallocated);
			}

			public static CodeDescriptionPair Unallocated { get { return new CodeDescriptionPair("UAC", ResString.GetMultilingualString("fb18f9c9-7734-4d5e-95c2-ebe4fe3881ab", "Unallocated")); } }
		}

		public sealed class MatchStatusReasonCodeTypes : CodeDescriptionPairList
		{
			public MatchStatusReasonCodeTypes()
			{
				Add(InAdvance);
			}

			public static CodeDescriptionPair InAdvance { get { return new CodeDescriptionPair("ADV", ResString.GetMultilingualString("89025674-948d-43b0-811d-551c689f097d", "Receipt/Payment in advance")); } }
		}

		public static class VolumeEqualizationMessages
		{
			public static string DiscountResults => (NoResString)"Volume Equalization Discount results:";
		}

		public static class RegistryDefaultValues
		{
			public const string ForeignCurrencyGLBalanceAdjustmentAccount = "2020.20.00";
			public const string PLAppropriationAccount = "4900.00.00";
		}

		public static class IndiaCompanyEmptyStateErrorMessages
		{
			public static string EmptyStateErrorMessageForLineBranch
			{
				get { return Res.GetString("008666A3-445F-47AF-828C-DB7438A75DC3", @"System could not identify the STATE of the Line Branch. Please confirm Main Office address of Line Branch Organization Proxy do have STATE recorded. If there is no Organization Proxy for Line Branch then please check Main office address of Company Organization Proxy."); }
			}

			public static string EmptyStateWarningMessageForOrigin
			{
				get { return Res.GetString("B31EB7C8-7296-472D-BD19-7D78B2C33770", @"System could not identify the STATE of the Origin. Please confirm the UNLOCO of Origin do have STATE recorded."); }
			}

			public static string EmptyStateWarningMessageForDestination
			{
				get { return Res.GetString("CB2E4F18-FBC3-47FF-B9C2-93516B257795", @"System could not identify the STATE of the Destination. Please confirm the UNLOCO of Destination do have STATE recorded."); }
			}

			public static string EmptyStateWarningMessageForFixedPlaceOfSupply
			{
				get { return Res.GetString("0F66CA92-6FFD-407A-BCF4-BBF1E8FCDBC1", @"System could not identify the STATE of the Customs Port Of Clearance In Place. Please confirm the UNLOCO of Customs Port Of Clearance In Place do have STATE recorded."); }
			}

			public static string EmptyStateWarningMessageForOrganization
			{
				get { return Res.GetString("A695B043-4C6A-4235-827B-CE307F15EAF4", @"System could not identify the STATE of the Organization. Please confirm Main Office address of Organization do have STATE recorded."); }
			}

			public static string EmptyStateErrorMessageForChargeBranch
			{
				get { return Res.GetString("434626B7-F602-434D-A72A-4CFA3142E59B", @"System could not identify the STATE of the Line Branch. Please confirm Main Office address of Charge Branch Organization Proxy do have STATE recorded. If there is no Organization Proxy for Charge Branch then please check Main office address of Company Organization Proxy."); }
			}

			public static string EmptyStateWarningMessageForSellAccount
			{
				get { return Res.GetString("6E298D37-4A23-45DB-9FC6-8A301A658537", @"System could not identify the STATE of the Debtor. Please confirm Main Office address of Debtor do have STATE recorded."); }
			}

			public static string EmptyStateWarningMessageForCostAccount
			{
				get { return Res.GetString("15B3FC7D-B0D4-47DD-94DB-4AA4A9FCBCEA", @"System could not identify the STATE of the Creditor. Please confirm Main Office address of Creditor do have STATE recorded."); }
			}
		}

		public static string DisallowToUpdateComplianceSubTypeAndNumber => Res.GetString("DE98D93D-6436-4E8F-9015-838BBB62CD44", "You cannot manually update Compliance Sub Type and/or Number as E-Invoicing functionality has been enabled.");

		public static string ComplianceSubTypeAndNumberEligibilityWarning
			=> Res.GetString("d88c2590-da75-4bee-9632-2946c081f188", "Note that changing the Compliance Number or Compliance Sub-Type will not re-evaluate the transaction for E-Invoicing.");

		public static class VietnamEInvoicingMessage
		{
			public static string AllTransactionsNotSatisfyToAllocate => AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value
				? AllTransactionsNotSatisfyToAllocate_EnableEInvoicingAdjustment
				: AllTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment;

			static string AllTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment => Res.GetString("BD233908-D7FA-405C-BDEE-EBE796C79441", @"Compliance Number cannot be allocated to the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number to CRD and ADJ transactions is not supported.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

No transaction will be updated.");

			static string AllTransactionsNotSatisfyToAllocate_EnableEInvoicingAdjustment => Res.GetString("63923594-68B5-4B02-8A46-6CB726A86439", @"Compliance Number cannot be allocated to the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number is only allowed for INV and CRD posted via 'Amend with Credit Note' function.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

No transaction will be updated.");

			public static string SomeTransactionsNotSatisfyToAllocate => AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value
				? SomeTransactionsNotSatisfyToAllocate_EnableEInvoicingAdjustment
				: SomeTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment;

			static string SomeTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment => Res.GetString("0455F7B3-4830-45D6-A070-329D88D505D1", @"Compliance Number cannot be allocated to some of the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number to CRD and ADJ transactions is not supported.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

Only INV transactions with a compliance sub type will be updated.");

			static string SomeTransactionsNotSatisfyToAllocate_EnableEInvoicingAdjustment => Res.GetString("A978078B-6664-40D2-B22C-FDACCBDE32D3", @"Compliance Number cannot be allocated to some of the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number is only allowed for INV and CRD posted via 'Amend with Credit Note' function.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

Only INV transactions with a compliance sub type will be updated.");

			public static string AllTransactionsNotSatisfyToPrint => Res.GetString("96F90889-CAB1-4AC2-9CA7-54705696D0FF", @"The selected transactions cannot be printed due to one of the following reasons:
1. No matching compliance invoice book found.
2. Selected INV transactions do not have a compliance sub type assigned.
3. Printing of compliance document for CRD and ADJ transactions is not supported.
4. INV transactions have been reversed.
5. Selected transactions contains comment charge lines (Charge Type: CMT) only.

No document will be printed.");

			public static string SomeTransactionsNotSatisfyToPrint => Res.GetString("BE2461C6-A472-41E5-B208-520F89346B83", @"Some of the selected transactions cannot be printed due to one of the following reasons:
1. No matching compliance invoice book found.
2. Selected INV transactions do not have a compliance sub type assigned.
3. Printing of compliance document for CRD and ADJ transactions is not supported.
4. INV transactions have been reversed.
5. Selected transactions contains comment charge lines (Charge Type: CMT) only.

Only INV transactions with compliance sub type and compliance number assigned will be printed.");
		}

		public static class AccountingMatchStatusReasonCodeErrorMessage
		{
			public static string MatchStatusReasonCodeShouldNotSpecified => Res.GetString("A0AC4270-94CC-4E94-967C-3C8681492DD9", "If a match status is not specified, a reason must not be specified.");
		}

		public static class AccountingSupportingDocumentNumberErrorMessage
		{
			public static string EmptySupportingDocumentNumber => Res.GetString("91F907BA-2386-4C96-8BB5-09AF59DFC4E9", "A supporting document number is required for reversing invoice/amending invoice with credit note. Please fill with a support document number.");
		}

		public static class AccountingInvoiceEventParentFinderErrorMessages
		{
			public static string NoMatchedInvoiceFound(ZString ledger, ZString transactionType, ZString invoiceNumber, ZString creditorCode, ZString invoiceDate)
			{
				return Res.GetString("0C4F45C5-3895-48F3-8DE7-BE1D00D82EC3", "Unable to find a matched invoice #{0} {1} {2} for creditor '{3}' with invoice date [{4}] in the system.", ledger, transactionType, invoiceNumber, creditorCode, invoiceDate);
			}

			public static string TooManyMatchedInvoiceFound(ZString ledger, ZString transactionType, ZString invoiceNumber, ZString creditorCode, ZString invoiceDate)
			{
				return Res.GetString("2FA4EC26-1C6A-43BE-9253-E26E2FE426E9", "More than one matched invoice #{0} {1} {2} for creditor '{3}' with invoice date [{4}] found in the system.", ledger, transactionType, invoiceNumber, creditorCode, invoiceDate);
			}

			public static string TaxRegNumberTooShort(ZString taxRegNumber)
			{
				return Res.GetString("33A6DC45-2B72-4D35-B779-1BB42EC3D4EF", "<Tax Reg Number> is too short, it needs to have at least 6 characters: '{0}'.", taxRegNumber);
			}

			public static string NoMatchedOrgCusCodeFound(ZString taxRegCountry, ZString taxRegCode, ZString taxRegNumber)
			{
				return Res.GetString("45BE0260-1D87-43E0-9212-BA7532B900F9", "Unable to find an unique creditor due to registration number '{0} {1} {2}' not found in the system.", taxRegCountry, taxRegCode, taxRegNumber);
			}

			public static string TooManyMatchedOrgCusCodeFound(ZString taxRegCountry, ZString taxRegCode, ZString taxRegNumber)
			{
				return Res.GetString("37FFA378-D54D-4886-A127-A6B52CCDE280", "Unable to find an unique creditor due to duplicate registration number '{0} {1} {2}' detected in the system.", taxRegCountry, taxRegCode, taxRegNumber);
			}

			public static string NoMatchedOrganizationFound(ZString orgCode)
			{
				return Res.GetString("1F5DE78A-D489-48B0-830C-A78549F3575F", "Unable to find an unique Creditor / Debtor due to code '{0}' not found in the system.", orgCode);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer only string")]
		public static class InvoiceAdditionalReference
		{
			public const string Separator = "|";
			public const string Posted = "Posted";
			public const string FullyMatched = "Fully Matched";
			public const string UndoFullyMatched = "Undo Fully Matched";
			public const string PostedAndFullyMatched = "Posted and Fully Matched";
			public const string Reversed = "Reversed";
		}

		public static class AuditAndCashActionText
		{
			public static MultilingualString AuditTransactionText => ResString.GetMultilingualString("5ECFCA5E-8773-46BC-BEE9-12DFBAB73D68", "Audit Transaction");

			public static MultilingualString UndoAuditTransactionText => ResString.GetMultilingualString("073E7953-F020-4C9C-A97E-CA8708A7753D", "Undo Audit Transaction");

			public static MultilingualString RecordCashierText => ResString.GetMultilingualString("CA980655-5B9B-4766-8A2B-A7F7D76E0A54", "Record Cashier");

			public static MultilingualString ClearCashierText => ResString.GetMultilingualString("B83D5282-71ED-4ACA-A984-B00408209B89", "Clear Cashier");
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer only string")]
		public static class InvoiceForNettingInfoList
		{
			public const string Number = "TransactionNum";
			public const string InternalReferenceNumber = "InternalReferenceNum";
			public const string Status = "MatchingStatus";
			public const string Ledger = "Ledger";
			public const string TransactionType = "TransactionType";
			public const string InitiatingEHubId = "InitiatingEHubId";
			public const string ReceivingEHubId = "ReceivingEHubId";
		}

		public static class FuturePostingErrorMessages
		{
			public static MultilingualString RegistryIsNotEnabled => ResString.GetMultilingualString("8EF06BA9-96C9-4ff2-92E6-8B4BA44AF847", "The post date cannot be in the future.\r\nAbility to future post cashbook transactions and matching is controlled by the Allow Future Posting of Cash Book Transactions registry, and Allow Future Posting security.");

			public static MultilingualString UserHasNoSecurity => ResString.GetMultilingualString("3CCA81E3-A500-42c5-B5E0-223B8DB5B824", "The post date cannot be in the future.\r\nIf you need to set a future date, it requires the following security permission: Manage -> Cash Book -> Cashbook Transactions -> Allow Future Posting");
		}

		public static class ChargeHashCalculatorInfo
		{
			public const byte ChargeCostHashVersion = 0;
			public const byte ChargeSellHashVersion = 0;
		}

		public static class ComplianceReportTypes
		{
			public const string PurchaseAndSalesVATForTW = "TXT";
			public const string ZeroRatedSalesVATForTW = "T02";
			public const string MakeTaxDigitalReportType = "MTD";
			public const string TaxablePaymentsAnnualReportType = "TPR";
			public const string PaymentTimesSmallBusinessReportType = "PTR";
			public const string PaymentTimesAllPaymentsReportType = "PTA";
			public const string PaymentTimesSmallBusiness2024ReportType = "PTS";
			public const string PaymentTimesAllPayments2024ReportType = "TCP";
			public const string ZusammenfassendeMeldungGermanyReportType = "ZMD";
		}

		public static class DefaultDayBookLineDescriptions
		{
			public static MultilingualString WIP { get { return ResString.GetMultilingualString("99d33d32-f7bc-47f5-828e-7496d165adda", "Aggregated Daily WIP Movement"); } }
			public static MultilingualString Accrual { get { return ResString.GetMultilingualString("664965c7-5823-472c-9297-fa4f9eb1bb3d", "Aggregated Daily Accrual Movement"); } }
			public static MultilingualString WIPAccrual { get { return ResString.GetMultilingualString("389295a9-6aac-4565-8870-959a62e84cfc", "Aggregated Daily WIP and Accrual Movement"); } }
		}

		public static string TaxBranchConflictErrorMessage => Res.GetString("40e25723-435a-489d-8182-0b095172d389", @"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag");

		public static string TaxBranchConflictErrorMessageWithSuggestion => TaxBranchConflictErrorMessage + "\r\n" + Res.GetString("96f7d1b0-e9f5-498e-8284-52c39ed31c69", "One possible way to resolve this is to go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.");

		public static MultilingualString WriteOffAsBadDebtText => ResString.GetMultilingualString("53396be8-a3f2-4a63-b8d8-39e6cf9dc9b9", "Write Off As Bad Debt");

		public static string OrderAlreadyCompletedErrorMessage => Res.GetString("49f13e09-97d1-4de2-9d40-62b6e3140117", "The collection order has been completed with a deposited date saved. No further changes are allowed.");

		public static string NegativeComplianceCaption => Res.GetString("B704149E-96FF-4798-ACFA-97CB2D562778", "Negative Compliance Records");

		public static ResourceString ComplianceDocumentNegativeLinesMessage => ResString.GetMultilingualString("BA82254D-DCB6-480D-B741-F69A80AF4C67", "Compliance Document records could not be created as negative compliance document lines are not allowed.");

		public static ResourceString ComplianceDocumentNegativeHeadersMessage => ResString.GetMultilingualString("C2A9EF38-DA39-495D-8E01-20C9E5E4B5BE", "Compliance Document records could not be created as negative compliance documents are not allowed.");

		public static ResourceString GetComplianceDocumentNegativeMessageWithInfo(string ledger, string transactionType, string transactionNum)
		{
			return ResString.GetMultilingualString("48A99186-5B02-4BC1-9867-17ED8B65CB13", "Transaction {0} {1} {2} is posted successfully. However, {3}", ledger, transactionType, transactionNum, AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.Value ? ComplianceDocumentNegativeHeadersMessage : ComplianceDocumentNegativeLinesMessage);
		}

		public static string GetComplianceDocumentNegativeMessage() => AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.Value
			? ComplianceDocumentNegativeHeadersMessage
			: ComplianceDocumentNegativeLinesMessage;

		public static string GetComplianceDocumentNegativeMessageWithTransNum(string invoicesStr) => AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.Value
			? Res.GetString("17BC0B86-4E10-4F93-B1F6-ADE1176E2F6E", @"Compliance Document records could not be created as negative compliance documents are not allowed.
Details of relevant transactions are listed below:
{0}", invoicesStr)
			: Res.GetString("C02766DC-792D-44C3-BA7C-FEB3CF48E706", @"Compliance Document records could not be created as negative compliance document lines are not allowed.
Details of relevant transactions are listed below:
{0}", invoicesStr);

		#region AccCollectionOrder

		public static class AccCollectionOrderMenuNames
		{
			public static MultilingualString AddTransactionsToOrderMenuItemName => ResString.GetMultilingualString("498a2fc9-c225-460c-b23b-6148ee790863", "Add Transactions To Order");

			public static MultilingualString RejectOrderMenuItemName => ResString.GetMultilingualString("AccCollectionBatchForm|b7a09ac7-4415-4ba2-8b24-3ba86d72ff04", "Reject Order");

			public static MultilingualString CreateReceiptMenuItemName => ResString.GetMultilingualString("cd822b06-ea0e-4d2e-814c-6b902dd2f834", "Create Receipts and Individual Deposit Batch");
		}

		#endregion

		public static string CommentChargeLineValidationMessage
		=> Res.GetString("6d248b9d-e6ef-4761-bfe1-2f6bb0f3685b", @"Comment type charge lines will not be included in tax invoices issued to receivables organizations. 
				If extra notes related to the charge need to be included on a tax invoice, it is recommended that the extra note text be included in the charge description field.");

		public static string ReverseConfirmationCaptionText => Res.GetString("661d0aa4-188e-437c-9bcd-cdad2483f5ac", "Reverse Confirmation");

		public static string ReverseConfirmComplianceSubTypeMessage => Res.GetString("0E86F557-775C-4AAD-ADA4-05578A95524A", "A compliance sub type and number has been assigned to this transaction. Do you want to reverse the transaction?");

		public static string ReprintingInvoiceMessage => Res.GetString("bda0ad28-0300-45b6-af3a-2a8ab86ddb46", @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, re-printing is not allowed through this module.
Please go to the eDocs tab of the transaction and re-print the first invoice version stored there.");

		#region Job Is Ready For Financial Closure

		public static MultilingualString JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage => ResString.GetMultilingualString("6119B40E-9FCC-4128-B0DE-B34ECBB7F847", "You cannot inserted / updated / deleted charges on this job because it has Ready For Financial Closure status.");

		public static string JobIsReadyForFinancialClosureWithoutPostSecurityErrorMessage => Res.GetString("6cb502ac-487a-4f39-b069-1b0829b36626", "You cannot post charges on this job because it has Ready For Financial Closure status.");

		#endregion

		public static class DsbJobBatchStatus
		{
			public const string Open = "OPN";
			public const string RequireApproval = "REQ";
			public const string Approve = "APP";
			public const string Close = "CLS";
			public const string Cancel = "CAN";
		}

		public static class CurrencyDefaultValues
		{
			public const int NoteJournalCurrencyDecimal = 2;
		}

		public static string GetIsDiscrepancyWithPaymentBatchErrorMessage(string propertyDescription)
		{
			return Res.GetString("75056127-D82F-426A-BE43-A10CC6BA2132", "Payment approval detail does not match payment batch header - please reconcile {0} with payment batch header manually or by re-entering header details.", propertyDescription);
		}

		public static string PaymentApprovalIsPostedOrCancelledErrorMessage => Res.GetString("71329242-C723-4B93-9698-E51E53922AEB", "Payment approval has status CAN-Canceled/PST-Posted, please remove this payment approval via Right Click > Remove in order to save Payment Batch record");

		public static string AmountCannotBeSetErrorMessage => Res.GetString("92e08fff-3060-47f2-a777-7d341a46121d", "Line amount cannot be set if there is a comment charge entered");

		public static string CloseButtonText => Res.GetString("7DE7DB39-B1EC-4DC4-AC11-A79D0360CEBE", "Close");

		public static string InvoiceDateIsInTheFutureErrorMessage => Res.GetString("7E71B4EF-3348-4B16-96B5-BED69F0D77C0", "The invoice date cannot be in the future because the registry 'Accounting > Receivable > Default Settings > Disallow Posting Invoices With A Future Invoice Date' is set to Yes.");

		public static string GetAmountShouldBeZeroForCMTLineErrorMessage(string transactionNum, string transactionType)
		{
			return Res.GetString("FF2180FA-1636-432C-AB53-9BE735ECE65A", "Amount of lines with CMT Charge Code should be zero for Transaction Num: {0} with Transaction Type: {1}", transactionNum, transactionType);
		}

		public static string GetGLAccountShouldNotBeEmptyErrorMessage(string transactionNum, string transactionType)
		{
			return Res.GetString("254CC6FA-6F3D-4839-A385-FC4688C52D1D", "GL Account of line should not be empty for Transaction Num: {0} with Transaction Type: {1}", transactionNum, transactionType);
		}

		public static string ChargeOrConsolCostIsNotNullForSameInvoiceWithUnequalColumnErrorMessage => Res.GetString("B211A06D-8E5F-4c36-BD43-FD3261A45A60", @"This cost will be posted on a local currency Payables Invoice.
A mix of Cost Currencies have recorded for this Creditor and Invoice Number. Because of this, they will be posted as a local currency payables transaction.");

		public static string GetIsNotLocalCurrencyAndExRateOptionIsApplicableErrorMessage(string invoicePostingExchangeRateRegistryItemCaption, string exchangeRateOption)
		{
			return Res.GetString("70b37859-2760-47dc-8ebe-608a65608684",
@"This estimated cost amount has been calculated based on current job exchange rate.
This will be converted based on ""{0}"" registry value ""{1}"" on posting as actual cost.", invoicePostingExchangeRateRegistryItemCaption, exchangeRateOption);
		}

		public static string CancelRequestsErrorMessage => Res.GetString("2ca7b027-60d3-4a32-a91c-445aee2fe062",
					@"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied.");

		public static class JournalEntriesNumberCustomisationNumberRule
		{
			public const string ALL = "ALL";

			public const string GRP = "GRP";

			public const string TRN = "TRN";
		}

		public static class JournalEntriesNumberCustomisationAllocationOption
		{
			public const string NON = "NON";

			public const string GEN = "GEN";
		}

		public static class JournalEntriesNumberCustomisationSequenceResetOption
		{
			public const string YEAR = "YR";

			public const string MONTH = "MTH";
		}

		public static class JournalEntriesNumberFountainPoolConstants
		{
			public const string JournalEntriesNumberPoolName = "JournalEntriesNumberPool";
		}

		public static class ShipmentTypeDescriptions
		{
			public static MultilingualString All { get { return ResString.GetMultilingualString("62C51B76-F2EB-4F12-86E6-931A8E70ECE5", "Any Shipment Type"); } }
		}

		public static class ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes
		{
			public const string InSameCountryRegionAsCurrentCompany = "SCC";
			public const string NotSameCountryRegionAsCurrentCompany = "NSC";
			public const string SameUnlocoAsJobBranch = "SUJ";
			public const string NotSameUnlocoAsJobBranch = "NSJ";
		}

		public static class ElectronicProcessingChargeDescriptionOverrideOriginDestinationDescriptions
		{
			public static MultilingualString InSameCountryRegionAsCurrentCompany { get { return ResString.GetMultilingualString("642310A9-9A4E-407D-BA43-6CC19AC415DD", "In the same country/region as current login company"); } }
			public static MultilingualString NotSameCountryRegionAsCurrentCompany { get { return ResString.GetMultilingualString("ABFBD43D-E9C0-4B44-8020-9E77584CD943", "Not in the same country/region as current login company"); } }
			public static MultilingualString SameUnlocoAsJobBranch { get { return ResString.GetMultilingualString("3ABB8835-E28C-4952-AAE9-409C14D28701", "In the same UNLOCO as the job header branch"); } }
			public static MultilingualString NotSameUnlocoAsJobBranch { get { return ResString.GetMultilingualString("CF58F7D3-FB87-4E46-A3EC-8FB7CF1FC85F", "Not in the same UNLOCO as the job header branch"); } }
		}

		public static CodeDescriptionPairList ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.InSameCountryRegionAsCurrentCompany, ElectronicProcessingChargeDescriptionOverrideOriginDestinationDescriptions.InSameCountryRegionAsCurrentCompany);
				lookUpList.AddPair(ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.NotSameCountryRegionAsCurrentCompany, ElectronicProcessingChargeDescriptionOverrideOriginDestinationDescriptions.NotSameCountryRegionAsCurrentCompany);
				return lookUpList;
			}
		}

		public static CodeDescriptionPairList ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.SameUnlocoAsJobBranch, ElectronicProcessingChargeDescriptionOverrideOriginDestinationDescriptions.SameUnlocoAsJobBranch);
				lookUpList.AddPair(ElectronicProcessingChargeDescriptionOverrideOriginDestinationCodes.NotSameUnlocoAsJobBranch, ElectronicProcessingChargeDescriptionOverrideOriginDestinationDescriptions.NotSameUnlocoAsJobBranch);
				return lookUpList;
			}
		}

		public static CodeDescriptionPairList ElectronicProcessingChargeDescriptionOverrideOriginDestinationList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddRange(ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList);
				lookUpList.AddRange(ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList);
				return lookUpList;
			}
		}

		public static class ElectronicProcessingChargeDescriptionOverridePrefixSuffixCodes
		{
			public const string Prefix = "PRE";
			public const string Suffix = "SUF";
		}

		public static class ElectronicProcessingChargeDescriptionOverridePrefixSuffixDescriptions
		{
			public static MultilingualString Prefix { get { return ResString.GetMultilingualString("300F7E7B-1CF9-49A3-B61F-D56108DDBDA4", "Prefix"); } }
			public static MultilingualString Suffix { get { return ResString.GetMultilingualString("031EFD18-E8CE-4999-8FAA-CCA4CEB54488", "Suffix"); } }
		}

		public static CodeDescriptionPairList ElectronicProcessingChargeDescriptionOverridePrefixSuffixList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(ElectronicProcessingChargeDescriptionOverridePrefixSuffixCodes.Prefix, ElectronicProcessingChargeDescriptionOverridePrefixSuffixDescriptions.Prefix);
				lookUpList.AddPair(ElectronicProcessingChargeDescriptionOverridePrefixSuffixCodes.Suffix, ElectronicProcessingChargeDescriptionOverridePrefixSuffixDescriptions.Suffix);
				return lookUpList;
			}
		}

		public static CodeDescriptionPairList AccDraftInvoiceHeaderStatusList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.AwaitingApproval, ResString.GetMultilingualString("E304C15E-2638-4952-BC9B-775F081D8590", "Awaiting Approval"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.Analyzing, ResString.GetMultilingualString("2B5ABABF-2919-48D0-9854-2F1BC678F029", "Analyzing"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.ApprovedForPosting, ResString.GetMultilingualString("23EF406A-1B50-41EA-9F83-ED0565D89BFE", "Approved For Posting"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.Discarded, ResString.GetMultilingualString("CE80718A-AD9D-495F-B39E-CC4CD92CAA94", "Discarded"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.Draft, ResString.GetMultilingualString("3966F61A-CE69-4E65-A46E-A1DD5F81FF5B", "Draft"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.InDispute, ResString.GetMultilingualString("0B77FDE4-D6B6-4D7E-9464-91EF31C49CD1", "In Dispute"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.Processed, ResString.GetMultilingualString("57F7449D-1D48-40AA-8372-91700F0707B5", "Processed"));
				lookUpList.AddPair(AccDraftInvoiceHeaderStatus.InReview, ResString.GetMultilingualString("4D3ABA60-CDCB-4506-BB6C-E3DF37F84EFD", "In Review"));
				return lookUpList;
			}
		}
	}
}
