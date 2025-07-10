using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Enterprise.ZArchitecture.Core
{
	public static class AccountTypeComboBoxConstants
	{
		public const string BalanceSheetAccount = "BSH";
		public const string ProfitAndLossAccount = "P&L"; // Non-semantic text
		public const string Total = "TTL";
		public const string Header = "HDR";
		public const string OpeningBalance = "OBA";
		public const string ClosingBalance = "CBA";
		public const string Consolidation = "CLN";
		public const string Alternate = "ALT";
		public const string CarriedForwardAccount = "CFW";
		public const string Note = "NTE";
	}

	public static class TransactionTypes
	{
		public const string Contra = "CTR";
		public const string Invoice = "INV";
		public const string CreditNote = "CRD";
		public const string AdjustmentNote = "ADJ";
		public const string Journal = "JNL";
		public const string JobRevenueJournal = "JRJ";
		public const string GLAutoJournal = "AJL";
		public const string GLStandardJournal = "GJL";
		public const string GLReversingJournal = "RJL";
		public const string WIPAccrualJournal = "WAJ";
		public const string Payment = "PAY";
		public const string Receipt = "REC";
		public const string Transfer = "TRF";
		public const string Discount = "DSC";
		public const string DirectReceipt = "DRC";
		public const string DirectPayment = "DPY";
		public const string Overpayment = "OVP";
		public const string ExchangeDifference = "EXX";
		public const string OpeningPayment = "OPY";
		public const string OpeningReceipt = "ORC";
		public const string ReceiptBatch = "RCB";
		public const string DDRBatch = "DDB";
		public const string InvoiceBatch = "INB";
		public const string UACreditNote = "UAC";
		public const string UAInvoice = "UAI";
		public const string InvoicePendingAllocation = "IPA";
		public const string CreditNotePendingAllocation = "CPA";
		public const string IncompleteInvoice = "INI";
		public const string IncompleteCreditNote = "INC";
		public const string IncompleteAdjustmentNote = "INA";
		public const string GLNoteJournal = "NJL";
	}

	public static class TransactionLineTypes
	{
		public const string Cost = "CST";
		public const string Revenue = "REV";
		public const string Accrual = "ACR";
		public const string WIP = "WIP";
		public const string UnapprovedCost = "UCT";
	}

	public static class LedgerTypes
	{
		public const string AccountsPayable = "AP";
		public const string AccountsReceivable = "AR";
		public const string General = "GL";
		public const string CashBook = "CB";
		public const string JobCosting = "JC";
		public const string UnapprovedPayableTransactions = "UA";
		public const string TransactionsPendingAllocation = "PA";
		public const string IncompleteTransactions = "IN";
	}

	public static class ExportTransactionTypes
	{
		public const string ARInvoice = "ARI";
		public const string APInvoice = "API";
		public const string ARCreditNote = "ARC";
		public const string APCreditNote = "APC";
		public const string ARAdjustmentNote = "ARA";
		public const string APAdjustmentNote = "APA";
		public const string WIPPosting = "WIP";
		public const string AccrualPosting = "ACP";
		public const string WIPReversal = "WIR";
		public const string AccrualReversal = "ACR";
		public const string UnallocatedAPInvoices = "UPI";
		public const string UnallocatedAPCreditNotes = "UPC";
	}

	public static class ReceiptTypes
	{
		public const string Cheque = "CHQ";
		public const string CreditCard = "CCD";
		public const string Cash = "CSH";
		public const string DirectDebit = "DDR";
		public const string DirectCredit = "DCR";
		public const string DirectDebitLine = "DDL";
		public const string NonRolledUpBatch = "NRB";
		public const string EFT = "EFT";
		public const string ScheduledEFT = "SFT";
		public const string CollectionRequest = "CRQ";
		public const string AccountMaintenanceFee = "AMF";
		public const string BankDebitTax = "BDT";
		public const string BankDepositFee = "BDP";
		public const string InterestPaid = "INT";
		public const string InterestReceived = "INR";
		public const string PeriodicPayment = "PPY";
		public const string StampDuty = "STD";
		public const string MiscellaneousReceipt = "MSR";
		public const string MiscellaneousFees = "MSF";
		public const string eNettDirectDebit = "END";
		public const string eNettDirectDebitForeignCurrency = "EDF";
		public const string eNettDirectCredit = "ENC";
		public const string eNettCreditCard = "ECC";
		public const string ForeignCurrencyBalance = "FCB";
		public const string EPayment = "EPA";
	}

	public static class ChequeTransactionTypes
	{
		public const string ChequeEntryTransaction  = "CET";
		public const string ChequeOutToCreditor = "COC";
		public const string ChequeOutToBankForCollection = "CBC";
		public const string ChequeOutToBankAsGuarantee  = "CBG";
		public const string ChequeCollectedAtBank = "CCB";
		public const string BadChequeAtBank = "BAB";
		public const string ChequeReturnFromBank = "CRB";
		public const string ChequeReturnToTheDebtor = "CRR";
		public const string ChequeCollectedInPortfolio = "CCP";
		public const string BadChequeInPortfolio = "BCP";
		public const string CollectionOfEndorsedCheque = "CEC";
		public const string ChequeReturnFromCreditor = "CRC";
		public const string ReturnedBadChequeFromCreditor = "BCR";
		public const string UncollectibleCheques = "UCC";
		public const string ChequeInJudicialProcess = "CJP";

		public static readonly ImmutableArray<string> BatchTypesForFilledCurrency
			= new[]
			{
				ChequeEntryTransaction,
				ChequeOutToCreditor,
				ChequeOutToBankForCollection,
				ChequeOutToBankAsGuarantee,
				ChequeCollectedAtBank,
				BadChequeAtBank,
				ChequeReturnFromBank,
				ChequeReturnToTheDebtor,
				ChequeCollectedInPortfolio,
				BadChequeInPortfolio,
				CollectionOfEndorsedCheque,
				ChequeReturnFromCreditor,
				ReturnedBadChequeFromCreditor,
				UncollectibleCheques,
				ChequeInJudicialProcess
			}.ToImmutableArray();
	}

	public static class CASSAutoCreateClaim
	{
		public const string OverBilled = "OVR";
		public const string BothOverUnderBilled = "BTH";
		public const string NotCreated = "NOT";
	}

	public static class ActualOrMaxIndicator
	{
		public const string Actual = "ACT";
		public const string Max = "MAX";
	}

	public static class AllocationMethod
	{
		public const string Manual = "MAN";
		public const string Shipment = "SHP";
		public const string Revenue = "REV";
		public const string ChargeableUnits = "CHG";
		public const string GrossWeight = "GWT";
		public const string GrossVolume = "GVT";
		public const string ContainerCount = "CNT";
		public const string OuterPackTotal = "OPT";
		public const string TwentyFootEquivalentUnit = "TEU";
		public const string CapacityPerContainer = "CAP";
		public const string FreeSpaceContribution = "FSC";
	}

	public static class PaymentApprovalStatus
	{
		public const string AwaitingApproval = "AWA";
		public const string FullyApproved = "APP";
		public const string Posted = "PST";
		public const string Rejected = "REJ";
		public const string Cancelled = "CAN";
		public const string Draft = "DFT";

		public static IEnumerable<string> StatusesAllowingExchangeRateQuote => new List<string> { AwaitingApproval, FullyApproved, Rejected, Draft };
	}

	public static class ExportInvoiceTypesInPDFFormat
	{
		public const string ConsolInvoice = "COI";
		public const string ShipmentInvoice = "SHI";
	}

	public static class NettingTransactionApprovalStatus
	{
		public const string Approved = "APP";
		public const string Disputed = "DIS";
		public const string Added = "ADD";
		public const string Matched = "MAT";
		public const string Setteled = "SET";
		public const string Reversed = "REV";
		public const string SettledOutOfNetting = "SON";
	}

	public static class NettingExchangeRateType
	{
		public const string Indicative = "IND";
		public const string Execution = "NET";
	}

	public static class EPaymentMethods
	{
		public const string EPaymentViaOFX = "EPO";
	}

	public static class AccDraftInvoiceProcessingErrorCodes
	{
		public const string CreditorNotProvided = "CRD";
		public const string TransactionAmountNotProvided = "AMT";
		public const string NoJobsFound = "JOB";
		public const string NoReferencesProvided = "REF";
		public const string TransactionNumberNotProvided = "NUM";
		public const string CurrencyNotProvided = "CUR";
		public const string NoAccrualsFound = "NAC";
		public const string NoMatchingAccrualFfound = "MCH";
		public const string MultipleMatchingAccrualsFound = "MUL";
		public const string SubtotalsDoNotMatchInvoiceTotal = "SBT";
		public const string LineAmountsDoNotMatchInvoiceTotal = "LNT";
		public const string UnableToPost = "POS";
		public const string SystemExceptionOccurred = "SYS";
		public const string CriticalValidationError = "CRV";
		public const string UnsupportedFileTypeForParsing = "FIL";
		public const string DuplicateInvoiceNumber = "DUP";
		public const string UnexpectedDueDate = "DUE";

		public static IEnumerable<string> GetAllErrorCodes()
		{
			return typeof(AccDraftInvoiceProcessingErrorCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(p => (string)p.GetValue(null));
		}
	}
}
