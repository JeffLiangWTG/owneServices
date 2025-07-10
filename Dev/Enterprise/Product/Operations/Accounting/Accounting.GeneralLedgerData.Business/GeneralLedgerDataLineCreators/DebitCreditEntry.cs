using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class DebitCreditEntryItem
	{
		public ZGuid AccountPK { get; set; }
		public ZDecimal LocalAmount { get; set; }
		public ZDecimal OSAmount { get; set; }
		public ZDateTime JournalDate { get; set; }
		public int Period { get; set; }
		public string GLDAccountType { get; set; }
		public string GLDType { get; set; }
		public DebitCredit DRCRSign { get; set; }
		public Guid BranchPK { get; set; }
		public Guid DepartmentPK { get; set; }
	}

	public class DebitCreditEntry
	{
		public DebitCreditEntryItem[] EntryItems { get; set; }
		public Guid TransactionHeaderPK { get; set; }
		public Guid TransactionLinePK { get; set; }
		public Guid CashBasisVatPK { get; set; }
		public Guid TaxGLMovementPK { get; set; }
		public string Currency { get; set; }
		public decimal ExchangeRate { get; set; }
		public Guid CompanyPK { get; set; }
		public Guid BranchPK { get; set; }
		public Guid TaxBranchPK { get; set; }
		public Guid DepartmentPK { get; set; }
	}

	public static class GLDAccountTypes
	{
		public const string ARControlAccount = "ARC";
		public const string APControlAccount = "APC";
		public const string ARSuspenseControlAccount = "ARS";
		public const string APSuspenseControlAccount = "APS";
		public const string ARControlAccountForGST = "RGT";
		public const string APControlAccountForGST = "PGT";
		public const string GSTOutputControlAccount = "GTO";
		public const string GSTInputControlAccount = "GTI";
		public const string PendingGSTOutputControlAccount = "PGO";
		public const string PendingGSTInputControlAccount = "PGI";
		public const string PendingGSTRecoverableOutputControlAccount = "PRO";
		public const string PendingGSTRecoverableInputControlAccount = "PRI";
		public const string GSTRecoverableControlAccount = "GRC";
		public const string GSTRecoverableTransactionLineGLAccount = "GRT";
		public const string TransactionBankGLAccountForGST = "TBG";
		public const string TransactionHeaderGLAccount = "THG";
		public const string TransactionLineGLAccount = "TLG";
		public const string TransactionLineGLAccountForReverse = "TGR";
		public const string TransactionBankGLAccount = "TBA";
		public const string FromTRFBankControlAccount = "FTB";
		public const string ToTRFBankControlAccount = "TTB";
		public const string ARFromTRFControlAccount = "RFT";
		public const string APFromTRFControlAccount = "PFT";
		public const string ARToTRFControlAccount = "RTT";
		public const string APToTRFControlAccount = "PTT";
		public const string CFXGLAccount = "CFX";
		public const string JobRevenueJournalControlAccount = "JRJ";
		public const string AccruedRevenueControlAccount = "ARA";
		public const string AccruedCostControlAccount = "ACA";
		public const string GLMovementDebitAccount = "MDA";
		public const string GLMovementCreditAccount = "MCA";
	}
}
