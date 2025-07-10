using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AccGLAggregate = Enterprise.MasterFiles.Business.AccGLAggregate;

namespace Enterprise.Accounting.Business.Aggregator
{
#if DEBUG

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
	public class BatchTestHelper
	{
		public BatchTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
			this.fMaxGLAccountCount = 40;
			this.fMaxOrgHeaderCount = 5;
		}

		#region Public Members
		public AccTransactionHeaderCollection TestDataSet;

		public Guid WIPChargeCode;
		public Guid WIPChargeCode2;
		public Guid CostChargeCode;
		public Guid RevenueChargeCode;
		public Guid AccrualChargeCode;
		public Guid AccrualChargeCode2;

		public Guid WIPAccounts;
		public Guid WIPAccounts2;
		public Guid CostAccounts;
		public Guid ReveneueAccount;
		public Guid AccrualAccounts;
		public Guid AccrualAccounts2;

		public Guid Branch1;
		public Guid Department1;

		public ZDateTime PostDate1;

		public ZDateTime PostDate200301 = new ZDateTime(2003, 1, 15);
		public ZDateTime PostDate200302 = new ZDateTime(2003, 2, 15);
		public ZDateTime PostDate200303 = new ZDateTime(2003, 3, 15);

		public AccountingPeriodCalculator PeriodCalc;
		public Guid CFXAccount;
		public Guid JobRevenueJournalControlAccount;
		public Guid ARControlAccount;
		public Guid APControlAccount;
		public Guid ARSuspenseControlAccount;
		public Guid APSuspenseControlAccount;
		public Guid ExchangeDifference;
		public Guid Overpayment;
		public Guid Discount;
		public Guid GSTIn;
		public Guid GSTOut;
		public Guid PendingGSTIn;
		public Guid PendingGSTOut;

		public Guid WIPControl;
		public Guid ACRControl;

		#endregion

		public void UpdateChargeAccounts()
		{
			SetWIPSACRAccounts();

			ReveneueAccount = GLHeaders[30].PK.ToGuid();
			WIPAccounts = GLHeaders[31].PK.ToGuid();
			CostAccounts = GLHeaders[32].PK.ToGuid();
			AccrualAccounts = GLHeaders[33].PK.ToGuid();

			WIPAccounts2 = GLHeaders[34].PK.ToGuid();
			AccrualAccounts2 = GLHeaders[35].PK.ToGuid();

			ZQuery query = new ZQuery(AccChargeCodeSchema.PK, RevenueChargeCode);
			query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.PK, CostChargeCode);
			query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.PK, WIPChargeCode);
			query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.PK, AccrualChargeCode);

			AccChargeCode[] chargeCodeWIP1s = Factory.Load<AccChargeCode>(query);
			foreach (AccChargeCode code in chargeCodeWIP1s)
			{
				code.AC_AG_RevenueAccount = ReveneueAccount;
				code.AC_AG_WIPAccount = WIPAccounts;
				code.AC_AG_CostAccount = CostAccounts;
				code.AC_AG_AccrualAccount = AccrualAccounts;
			}

			query = new ZQuery(AccChargeCodeSchema.PK, WIPChargeCode2);
			query.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.PK, AccrualChargeCode2);

			AccChargeCode[] chargeCodeWIP2s = Factory.Load<AccChargeCode>(query);
			foreach (AccChargeCode code in chargeCodeWIP2s)
			{
				code.AC_AG_RevenueAccount = ReveneueAccount;
				code.AC_AG_WIPAccount = WIPAccounts2;
				code.AC_AG_CostAccount = CostAccounts;
				code.AC_AG_AccrualAccount = AccrualAccounts2;
			}

			Factory.Save();
		}

		public void SetUpPeriods()
		{
			SetUpPeriods(GlbCompany.CurrentCompany.PK);
		}

		public void SetUpPeriods(ZGuid companyPK)
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();

			periodHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 1, 1, 0, 0, 0), new ZDateTime(2003, 1, 31, 23, 59, 0), companyPK);
			periodHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1, 0, 0, 0), new ZDateTime(2003, 2, 28, 23, 59, 0), companyPK);
			periodHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1, 0, 0, 0), new ZDateTime(2003, 3, 31, 23, 59, 0), companyPK);
			periodHelper.SetupSinglePeriod(200304, new ZDateTime(2003, 4, 1, 0, 0, 0), new ZDateTime(2003, 4, 30, 23, 59, 0), companyPK);
		}

		public void InsertBanks()
		{
			AccBankAccountCollection referenceBanksToEnsureCreation = Banks;
			Factory.Save();
		}

		internal AccBankAccountCollection Banks
		{
			get
			{
				if (banks == null)
				{
					banks = new AccBankAccountCollection(Factory);
					banks.Add(TestObjectCreator.CreateBankAccount("BNK1", "BANK 1 DESCRIPTION", GlbCompany.CurrentCompany.LocalCurrency, GLHeaders[35]));
					banks.Add(TestObjectCreator.CreateBankAccount("BNK2", "BANK 2 DESCRIPTION", GlbCompany.CurrentCompany.LocalCurrency, GLHeaders[36]));
					banks.Add(TestObjectCreator.CreateBankAccount("BNK3", "BANK 3 DESCRIPTION", GlbCompany.CurrentCompany.LocalCurrency, GLHeaders[37]));
				}
				return banks;
			}
		}
		AccBankAccountCollection banks;

		public TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		internal OrgHeaderCollection Organisations
		{
			get
			{
				if (organisations == null)
				{
					ZQuery query = new ZQuery();
					query.MaximumRows = fMaxOrgHeaderCount;
					organisations = new OrgHeaderCollection(Factory);
					organisations.LoadWithMoreFiltering(query);
				}
				return organisations;
			}
		}
		OrgHeaderCollection organisations;

		public void SetAPARJournal(AccTransactionHeaderCollection testDataSet, string ledgerType, Guid accountPK, decimal amount, ZDateTime postDate)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));

			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_AG = accountPK;
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = TransactionTypes.Journal;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_PostDate = postDate.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = ledgerType + "Journal" + index;

			transaction.AH_PostToGL = "N";
		}

		public void SetAPARContra(AccTransactionHeaderCollection testDataSet, string ledgerType, decimal amount, ZDateTime postDate)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = postDate.ToDateTime();
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = TransactionTypes.Contra;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_PostDate = postDate.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = ledgerType + "Contra" + index;

			transaction.AH_PostToGL = "N";
		}

		public void SetHeaderOnlyLines(AccTransactionHeaderCollection testDataSet, string ledgerType, string transactionType, decimal amount, ZDateTime postDate)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_PostDate = postDate.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = transactionType + index;

			transaction.AH_PostToGL = "N";

			bool miscLedgerAndType =  (ledgerType == LedgerTypes.AccountsReceivable || ledgerType == LedgerTypes.AccountsPayable) &&
				(transactionType == TransactionTypes.ExchangeDifference || transactionType == TransactionTypes.Discount || transactionType == TransactionTypes.Overpayment);

			if (miscLedgerAndType) // Need to create a Journal to match it because we cannot save miscellaneous transaction with a non zero outstanding amount
			{
				AccTransactionHeader journalToMatch = Factory.New<AccTransactionHeader>();

				journalToMatch.AH_InvoiceDate = DateTime.Now;
				journalToMatch.AH_Ledger = ledgerType;
				journalToMatch.AH_TransactionType = TransactionTypes.Journal;
				journalToMatch.AH_InvoiceAmount = -amount;
				journalToMatch.AH_OutstandingAmount = -amount;
				journalToMatch.AH_PostDate = postDate.ToDateTime();
				journalToMatch.AH_GB = Branch1;
				journalToMatch.AH_GE = Department1;
				journalToMatch.AH_TransactionNum = TransactionTypes.Journal + index;
				journalToMatch.AH_PostToGL = "Y";

				AccTransactionMatchLink link1 = Factory.New<AccTransactionMatchLink>();
				AccTransactionMatchLink link2 = Factory.New<AccTransactionMatchLink>();

				link1.AP_AH = transaction.PK;
				link1.AP_Amount = transaction.AH_OutstandingAmount;
				link2.AP_AH = journalToMatch.PK;
				link2.AP_Amount = journalToMatch.AH_OutstandingAmount;
				link1.AP_MatchDate = link2.AP_MatchDate = DateTime.Now;
				link1.AP_MatchGroupNum = link1.AP_MatchGroupNum = "1000" + index.ToString();

				TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
				matchlinks.Add(link1);
				matchlinks.Add(link2);
				transaction.AH_OutstandingAmount = 0m;
				journalToMatch.AH_OutstandingAmount = 0m;
			}
		}

		public void SetDirectTransaction(string transactionType, AccTransactionHeaderCollection testDataSet, Guid lineAccount, Guid bankAccount, decimal lineAmount, decimal gST, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			AccTransactionLines line = Factory.New<AccTransactionLines>();

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = "DirectReceipt" + index;
			transaction.AH_InvoiceAmount = lineAmount;
			transaction.AH_GSTAmount = gST;
			transaction.AH_AB = bankAccount;

			line.AL_AH = transaction.PK;
			line.AL_LineType = transactionType;
			line.AL_LineAmount = lineAmount;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = gST;
			line.AL_PostDate = PostDate1.ToDateTime();
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			line.AL_AG = lineAccount;

			transaction.AH_PostToGL = "N";
		}

		public void SetInvoiceCreditAdjustmentREV(AccTransactionHeaderCollection testDataSet)
		{
			SetInvoiceCreditAdjustmentREV(testDataSet, PostDate1);
		}

		public AccTransactionLines SetInvoiceCreditAdjustmentREV(AccTransactionHeaderCollection testDataSet, ZDateTime profitRecognitionDate, bool isTaxCashBasis = false, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			int lineIndex = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = "InvCrdAdjREV1";
			transaction.AH_GE = Department1;

			line.AL_AH = transaction.PK;
			line.AL_LineAmount = -100.0M;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = -10.0M;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_PostDate = PostDate1.ToDateTime();
			line.AL_ReverseDate = profitRecognitionDate.ToDateTime();
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			line.AL_AC = RevenueChargeCode;
			line.AL_AG = Factory.Load<AccChargeCode>(RevenueChargeCode).AC_AG_RevenueAccount;
			if (isTaxCashBasis)
			{
				line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			}

			transaction.AH_PostToGL = "N";

			return line;
		}

		public void SetJobRevenueJournal(AccTransactionHeaderCollection testDataSet)
		{
			SetJobRevenueJournal(testDataSet, PostDate1);
		}

		public void SetJobRevenueJournal(AccTransactionHeaderCollection testDataSet, ZDateTime profitRecognitionDate)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));

			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			AccTransactionLines line2 = Factory.New<AccTransactionLines>();

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.JobCosting;
			transaction.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = "JobRevenueJournal1";
			transaction.AH_GE = Department1;

			line1.AL_AH = transaction.PK;
			line1.AL_LineAmount = -100.0M;
			line1.AL_LineType = TransactionLineTypes.Revenue;
			line1.AL_PostDate = PostDate1.ToDateTime();
			line1.AL_ReverseDate = profitRecognitionDate.ToDateTime();
			line1.AL_GB = Branch1;
			line1.AL_GE = Department1;
			line1.AL_AC = RevenueChargeCode;
			line1.AL_AG = Factory.Load<AccChargeCode>(RevenueChargeCode).AC_AG_RevenueAccount;

			line2.AL_AH = transaction.PK;
			line2.AL_LineAmount = 50.0M;
			line2.AL_LineType = TransactionLineTypes.Revenue;
			line2.AL_PostDate = PostDate1.ToDateTime();
			line2.AL_ReverseDate = profitRecognitionDate.ToDateTime();
			line2.AL_GB = Branch1;
			line2.AL_GE = Department1;
			line2.AL_AC = RevenueChargeCode;
			line2.AL_AG = Factory.Load<AccChargeCode>(RevenueChargeCode).AC_AG_RevenueAccount;

			transaction.AH_PostToGL = "N";
		}

		public void SetInvoiceCreditAdjustmentREVWithoutChargeCode(AccTransactionHeaderCollection testDataSet, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			int lineIndex = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = "InvCrdAdjREV1";
			transaction.AH_GE = Department1;

			line.AL_AH = transaction.PK;
			line.AL_LineAmount = -100.0M;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = -10.0M;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_PostDate = PostDate1.ToDateTime();
			line.AL_ReverseDate = PostDate1.ToDateTime();
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			line.AL_AG = GLHeaders[0].PK;

			transaction.AH_PostToGL = "N";
		}

		public void SetInvoiceCreditAdjustmentCST(AccTransactionHeaderCollection testDataSet)
		{
			SetInvoiceCreditAdjustmentCST(testDataSet, PostDate1);
		}

		public AccTransactionLines SetInvoiceCreditAdjustmentCST(AccTransactionHeaderCollection testDataSet, ZDateTime profitRecognitionDate, bool isTaxCashBasis = false, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			int lineIndex = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = "InvCrdAdjCST1";
			transaction.AH_GE = Department1;

			line.AL_AH = transaction.PK;
			line.AL_LineAmount = 100.0M;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = 10.0M;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_PostDate = PostDate1.ToDateTime();
			line.AL_ReverseDate = profitRecognitionDate.ToDateTime();
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			line.AL_AC = CostChargeCode;
			line.AL_AG = Factory.Load<AccChargeCode>(CostChargeCode).AC_AG_CostAccount;
			if (isTaxCashBasis)
			{
				line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			}

			transaction.AH_PostToGL = "N";

			return line;
		}

		public void SetInvoiceCreditAdjustmentCSTWithoutChargeCode(AccTransactionHeaderCollection testDataSet, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			int lineIndex = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.CreditNote;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = "InvCrdAdjCST1";
			transaction.AH_GE = Department1;

			line.AL_AH = transaction.PK;
			line.AL_LineAmount = 100.0M;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = 10.0M;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_PostDate = PostDate1.ToDateTime();
			line.AL_ReverseDate = PostDate1.ToDateTime();
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			line.AL_AG = GLHeaders[1].PK;

			transaction.AH_PostToGL = "N";
		}

		public void SetGSTCashBasis(bool value)
		{
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsGSTCashBasis = value;
			Factory.Save();
		}

		public AccTransactionLines SetInvoiceCreditAdjustment(AccTransactionHeaderCollection testDataSet, string ledger, string transactionType, string transactionLineType, Guid aL_AG, Guid aL_AC, decimal lineAmount, decimal gST, ZDateTime profitRecognitionDate, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			AccTransactionLines line = Factory.New<AccTransactionLines>();
			int lineIndex = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = ledger + transactionType + index;
			transaction.AH_GE = Department1;
			transaction.AH_InvoiceAmount = lineAmount;
			transaction.AH_GSTAmount = gST;
			transaction.AH_OutstandingAmount = lineAmount + gST;

			line.AL_AH = transaction.PK;
			line.AL_LineAmount = lineAmount;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = gST;
			line.AL_LineType = transactionLineType;
			line.AL_PostDate = PostDate1.ToDateTime();
			line.AL_ReverseDate = profitRecognitionDate.ToDateTime();
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			if (aL_AC != Guid.Empty)
			{
				line.AL_AC = aL_AC;
			}
			if (aL_AG != Guid.Empty)
			{
				line.AL_AG = aL_AG;
			}

			transaction.AH_PostToGL = "N";

			return line;
		}

		public void SetInvoiceCreditAdjustment(AccTransactionHeaderCollection testDataSet, string ledger, string transactionType, string transactionLineType, Guid aL_AG, Guid aL_AC, decimal lineAmount, decimal gST)
		{
			SetInvoiceCreditAdjustment(testDataSet, ledger, transactionType, transactionLineType, aL_AG, aL_AC, lineAmount, gST, PostDate1);
		}

		public Guid SetWIPAccrualLinesOnly(string transactionLineType, Guid aL_AG, Guid aL_AC, decimal lineAmount, decimal gST, ZDateTime postDate, ZDateTime reverseDate)
		{
			return SetWIPAccrualLinesOnly(transactionLineType, aL_AG, aL_AC, lineAmount, gST, postDate, reverseDate, "N", "N");
		}

		public Guid SetWIPAccrualLinesOnly(string transactionLineType, Guid aL_AG, Guid aL_AC, decimal lineAmount, decimal gST, ZDateTime postDate, ZDateTime reverseDate, string postFlag, string reverseFlag, decimal inputGSTVATRecoverable = 1)
		{
			AccTransactionLines line = Factory.New<AccTransactionLines>();

			line.AL_LineAmount = lineAmount;
			line.AL_InputGSTVATRecoverable = inputGSTVATRecoverable;
			line.AL_GSTVAT = gST;
			line.AL_LineType = transactionLineType;
			line.AL_PostDate = postDate.ToDateTime();
			if (reverseDate != new ZDateTime(1900, 1, 1) && !reverseDate.IsEmpty)
			{
				line.AL_ReverseDate = reverseDate.ToDateTime();
			}
			line.AL_GB = Branch1;
			line.AL_GE = Department1;
			if (aL_AC != Guid.Empty)
			{
				line.AL_AC = aL_AC;
			}
			if (aL_AG != Guid.Empty)
			{
				line.AL_AG = aL_AG;
			}

			line.AL_PostToGL = postFlag;
			line.AL_ReverseToGL = reverseFlag;
			return line.PK.ToGuid();
		}

		public void SetPayment(AccTransactionHeaderCollection testDataSet, string ledgerType, decimal amount)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = TransactionTypes.Payment;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = "Payment" + index;
			transaction.AH_AB = Banks[0].PK;

			transaction.AH_PostToGL = "N";
		}

		public void SetReceiptPayment(AccTransactionHeaderCollection testDataSet, string ledgerType, string transactionType, Guid aH_AB, decimal amount)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = transactionType + index;
			transaction.AH_AB = aH_AB;

			transaction.AH_PostToGL = "N";
		}

		public void SetReceipt(AccTransactionHeaderCollection testDataSet, string ledgerType, decimal amount)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = TransactionTypes.Receipt;
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_GE = Department1;
			transaction.AH_TransactionNum = "Receipt" + index;
			transaction.AH_AB = Banks[1].PK;

			transaction.AH_PostToGL = "N";
		}

		public void SetCashBookDataSet(AccTransactionHeaderCollection testDataSet, string transactionType, decimal amount, Guid bankPK, byte? transactionCount = null, ZGuid groupId = default(ZGuid))
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = LedgerTypes.CashBook;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = "CashBook-" + transactionType + (index + 1);
			transaction.AH_GE = Department1;
			transaction.AH_TransactionCount = transactionCount.GetValueOrDefault((byte)(index + 1));
			transaction.AH_AB = Utilities.GetGuidFromObject(bankPK);
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_TransactionBelongsToGroup = groupId;

			transaction.AH_PostToGL = "N";
		}

		public void SetARAPTransferDataSet(AccTransactionHeaderCollection testDataSet, string ledgerType, decimal amount)
		{
			AccTransactionHeader transaction = testDataSet.AddNew(typeof(AccTransactionHeader));
			int index = testDataSet.Count - 1;

			transaction.AH_InvoiceDate = DateTime.Now;
			transaction.AH_Ledger = ledgerType;
			transaction.AH_TransactionType = TransactionTypes.Transfer;
			transaction.AH_PostDate = PostDate1.ToDateTime();
			transaction.AH_GB = Branch1;
			transaction.AH_TransactionNum = ledgerType + TransactionTypes.Transfer + (index + 1);
			transaction.AH_GE = Department1;
			transaction.AH_TransactionCount = (byte)(index + 1);
			transaction.AH_OH = Organisations[index % 5].PK.ToGuid();
			transaction.AH_InvoiceAmount = amount;
			transaction.AH_OutstandingAmount = amount;

			transaction.AH_PostToGL = "N";
		}

		public void SetControlAccounts()
		{
			JobRevenueJournalControlAccount = GLHeaders[14].PK.ToGuid();
			ARControlAccount = GLHeaders[15].PK.ToGuid();
			APControlAccount = GLHeaders[16].PK.ToGuid();
			ExchangeDifference = GLHeaders[17].PK.ToGuid();
			Overpayment = GLHeaders[18].PK.ToGuid();
			Discount = GLHeaders[19].PK.ToGuid();
			GSTIn = GLHeaders[20].PK.ToGuid();
			GSTOut = GLHeaders[21].PK.ToGuid();
			WIPControl = GLHeaders[22].PK.ToGuid();
			ACRControl = GLHeaders[23].PK.ToGuid();
			PendingGSTIn = GLHeaders[24].PK.ToGuid();
			PendingGSTOut = GLHeaders[25].PK.ToGuid();
			ARSuspenseControlAccount = GLHeaders[26].PK.ToGuid();
			APSuspenseControlAccount = GLHeaders[27].PK.ToGuid();
			CFXAccount = GLHeaders[14].PK.ToGuid();
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		public void SetWIPSACRAccounts()
		{
			ZQuery bondClaimQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "BOND");
			bondClaimQuery.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_Code, "CLAIM");
			AccChargeCodeCollection charges = new AccChargeCodeCollection(Factory, bondClaimQuery);
			charges.Load();
			foreach (AccChargeCode code in charges)
			{
				code.AC_AG_CostAccount = GLHeaders[26].PK;
			}
			Factory.Save();

			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_AG_AccrualAccount, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_AG_CostAccount, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_AG_RevenueAccount, SQLComparisonOperator.NotEqual, null);
			filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_AG_WIPAccount, SQLComparisonOperator.NotEqual, null);
			AccChargeCodeCollection chargeCodes = new AccChargeCodeCollection(Factory, filter);

			chargeCodes.Load();
			WIPChargeCode = chargeCodes[0].PK.ToGuid();
			RevenueChargeCode = chargeCodes[1].PK.ToGuid();
			CostChargeCode = chargeCodes[2].PK.ToGuid();
			AccrualChargeCode = chargeCodes[3].PK.ToGuid();
			WIPChargeCode2 = chargeCodes[4].PK.ToGuid();
			AccrualChargeCode2 = chargeCodes[5].PK.ToGuid();
		}

		//public void SetGLAccountList()
		//{
		//    GLAccountDescription = new ArrayList();
		//    GLAccountNumber = new ArrayList();
		//    GLAccountType = new ArrayList();

		//    foreach (AccGLHeader GLHeader in GLHeaders)
		//    {
		//        GLAccountDescription.Add(GLHeader.AG_Description);
		//        GLAccountNumber.Add(GLHeader.AG_AccountNum);
		//        GLAccountType.Add(GLHeader.AG_AccountType);
		//    }
		//}

		public AccGLHeaderCollection GLHeaders
		{
			get
			{
				if (fGLHeaders == null || fGLHeaders.Count == 0)
				{
					ZQuery query = new ZQuery();
					query.MaximumRows = fMaxGLAccountCount;
					query.OrderBy = "AG_AccountNum";
					fGLHeaders = new AccGLHeaderCollection(Factory);
					fGLHeaders.LoadWithMoreFiltering(query);
				}
				return fGLHeaders;
			}
		}
		AccGLHeaderCollection fGLHeaders;

		public Guid InsertTestAggregateRow(BusinessObjectFactory factory, ZGuid gLAccount, decimal amount, int period)
		{
			AccGLAggregate aggregateRow1 = factory.New(typeof(AccGLAggregate)) as AccGLAggregate;
			aggregateRow1.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregateRow1.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregateRow1.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregateRow1.AA_AG = gLAccount;
			aggregateRow1.AA_Amount = amount;
			aggregateRow1.AA_Period = period;
			factory.Save();

			return aggregateRow1.PK.ToGuid();
		}

		#region Implementation

		protected int fMaxGLAccountCount;
		protected int fMaxOrgHeaderCount;

		#endregion
	}
#endif
}
