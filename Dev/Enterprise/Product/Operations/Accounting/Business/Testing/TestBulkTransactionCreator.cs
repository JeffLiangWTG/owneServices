#if DEBUG
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class TestBulkTransactionCreator
	{
		public TestBulkTransactionCreator(BusinessObjectFactory factory, TestObjectCreator testObjectCreator, bool postEverythingInOneYear = false)
		{
			Factory = factory;
			TestObjectCreator = testObjectCreator;
			PostEverythingInOneYear = postEverythingInOneYear;
		}

		readonly BusinessObjectFactory Factory;
		readonly TestObjectCreator TestObjectCreator;
		readonly bool PostEverythingInOneYear;

		public OrgHeader Org1
		{
			get;
			set;
		}

		public OrgHeader Org2
		{
			get;
			set;
		}

		public OrgHeader Org3
		{
			get;
			set;
		}

		public OrgHeader Org4
		{
			get;
			set;
		}

		public ZDateTime June2008
		{
			get { return new ZDateTime(2008, 06, 15); }
		}

		public ZDateTime June2009
		{
			get { return PostEverythingInOneYear ? June2008 : new ZDateTime(2009, 06, 15); }
		}

		public ZDateTime June2010
		{
			get { return PostEverythingInOneYear ? June2008 : new ZDateTime(2010, 06, 15); }
		}

		public ZDateTime June2011
		{
			get { return PostEverythingInOneYear ? June2008 : new ZDateTime(2011, 06, 15); }
		}

		public ZString ARControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value).AccountNum; }
		}

		public ZString APControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value).AccountNum; }
		}

		public ZString ARSuspenseControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value).AccountNum; }
		}

		public ZString APSuspenseControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value).AccountNum; }
		}

		public ZString JobRevenueJournalControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value).AccountNum; }
		}

		public ZString GSTInputControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value).AccountNum; }
		}

		public ZString GSTOutputControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value).AccountNum; }
		}

		public ZString WIPControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value).AccountNum; }
		}

		public ZString AccrualControlAccount
		{
			get { return Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value).AccountNum; }
		}

		public void Setup()
		{
			SetupPeriods();
			SetupCommonData();
			SetupControlAccounts();
		}

		void SetupCommonData()
		{
			TestObjectCreator.AUDBankAccount.GLHeader.AG_AccountNum = "AUDAcc";
			TestObjectCreator.USDBankAccount.GLHeader.AG_AccountNum = "USDAcc";

			Org1 = TestObjectCreator.CreateOrgHeader("Org1", true, true, true, true, true, true);
			Org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true, true, true, true, true);
			Org3 = TestObjectCreator.CreateOrgHeader("Org3", true, true, true, true, true, true);
			Org4 = TestObjectCreator.CreateOrgHeader("Org4", true, true, true, true, true, true);
		}

		void SetupPeriods()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2008, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2011, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void SetupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = TestObjectCreator.CreateCFXAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
		}

		public AccBankAccount BankAccountAUD
		{
			get { return TestObjectCreator.AUDBankAccount; }
		}

		public AccBankAccount BankAccountUSD
		{
			get { return TestObjectCreator.USDBankAccount; }
		}

		public ZString Branch
		{
			get { return GlbBranch.CurrentBranch.GB_Code; }
		}

		public ZString Department
		{
			get { return GlbDepartment.CurrentDepartment.GE_Code; }
		}

		public ZString Company
		{
			get { return GlbCompany.CurrentCompany.GC_Code; }
		}

		public ZString AUD
		{
			get { return TestObjectCreator.AUD.RX_Code; }
		}

		#region Create All Transactions

		public void CreateTransactions()
		{
			CreatePayments();
			CreateReceipts();
			CreateDiscounts();
			CreateOverpayments();
			CreateARAPJournals();
			CreateGLJournals();
			CreateARAPTransfers();
			CreateARAPExchangeDifferences();
			CreateContras();
			CreateInvoices();
			CreateCreditNotes();
			CreateAdjustmentNotes();
			CreateCBTransfers();
			CreateCBExchangeDifferences();
			CreateCBDirectPayments();
			CreateCBDirectReceipts();
			CreateJobCostingJournals();
			CreateJobRevenueJournals();
			CreateWIPsAndAccruals();
		}

		#endregion

		#region Payments

		public void CreatePayments()
		{
			TestObjectCreator.CreateARPayment(1.0m, 1m, June2008, June2008, Org1.PK, BankAccountAUD.PK);
			TestObjectCreator.CreateARPayment(1.0m, -10m, June2009, June2009, Org2.PK, BankAccountAUD.PK);
			TestObjectCreator.CreateAPPayment(1.0m, 100m, June2010, June2010, Org3.PK, BankAccountAUD.PK);
			TestObjectCreator.CreateAPPayment(1.0m, -1000m, June2011, June2011, Org4.PK, BankAccountAUD.PK);
			Factory.Save();
		}

		#endregion

		#region Receipts

		public void CreateReceipts()
		{
			TestObjectCreator.CreateARReceipt(1.0m, 1m, June2008, June2008, Org1.PK, BankAccountAUD.PK);
			TestObjectCreator.CreateARReceipt(1.0m, -10m, June2009, June2009, Org2.PK, BankAccountAUD.PK);
			TestObjectCreator.CreateAPReceipt(1.0m, 100m, June2010, June2010, Org3.PK, BankAccountAUD.PK);
			TestObjectCreator.CreateAPReceipt(1.0m, -1000m, June2011, June2011, Org4.PK, BankAccountAUD.PK);
			Factory.Save();
		}

		#endregion

		#region Discounts

		public void CreateDiscounts()
		{
			TestObjectCreator.CreateAPDiscount(1m, June2008, Org1.PK);
			TestObjectCreator.CreateAPDiscount(-10m, June2009, Org2.PK);
			TestObjectCreator.CreateARDiscount(100m, June2010, Org3.PK);
			TestObjectCreator.CreateARDiscount(-1000m, June2011, Org4.PK);
			Factory.Save();
		}

		#endregion

		#region Overpayments

		public void CreateOverpayments()
		{
			TestObjectCreator.CreateOverpayment<AROverpayment>(1m, June2008, Org1.PK);
			TestObjectCreator.CreateOverpayment<AROverpayment>(-10m, June2009, Org2.PK);
			TestObjectCreator.CreateOverpayment<AROverpayment>(100m, June2010, Org3.PK);
			TestObjectCreator.CreateOverpayment<AROverpayment>(-1000m, June2011, Org4.PK);
			Factory.Save();
		}

		#endregion

		#region Journals

		public void CreateARAPJournals()
		{
			ARJournal arJournalPositive = TestObjectCreator.CreateJournal<ARJournal>(1m, June2008, Org1.PK);
			ARJournal arJournalNegative = TestObjectCreator.CreateJournal<ARJournal>(-10m, June2009, Org2.PK);
			APJournal apJournalPositive = TestObjectCreator.CreateJournal<APJournal>(100m, June2010, Org3.PK);
			APJournal apJournalNegative = TestObjectCreator.CreateJournal<APJournal>(-1000m, June2011, Org4.PK);
			Factory.Save();
		}

		#endregion

		#region GL Journals

		public void CreateGLJournals()
		{
			var glStandardJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, new ZDateTime(2008, 2, 15), new ZDateTime(2008, 2, 28, 23, 59, 0), null);
			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 50m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 300m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glStandardJournal.Balance();
			BusinessObjectFactory.SaveTogether(Factory, new AggregateWrapper(glStandardJournal, glStandardJournal));

			var glAutoJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLAutoJournal, new ZDateTime(2009, 02, 15), new ZDateTime(2009, 2, 28, 23, 59, 0), new ZDateTime(2009, 6, 30, 23, 59, 0));
			TestObjectCreator.CreateGLJournalLine(glAutoJournal, 200m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glAutoJournal, 1000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glAutoJournal.Balance();
			BusinessObjectFactory.SaveTogether(Factory, new AggregateWrapper(glAutoJournal, glAutoJournal));

			var glReversingJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLReversingJournal, new ZDateTime(2010, 02, 15), new ZDateTime(2010, 2, 28, 23, 59, 0), new ZDateTime(2010, 6, 1));
			TestObjectCreator.CreateGLJournalLine(glReversingJournal, 10000m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glReversingJournal, 3000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glReversingJournal.Balance();
			BusinessObjectFactory.SaveTogether(Factory, new AggregateWrapper(glReversingJournal, glReversingJournal));
		}

		#endregion

		#region Elimination GL Journals

		public void CreateEliminationGLJournals(ZString eliminationCategory)
		{
			var glStandardJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, new ZDateTime(2008, 2, 15), new ZDateTime(2008, 2, 28, 23, 59, 0), null);
			glStandardJournal.AH_TransactionCategory = eliminationCategory;
			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 5m, DebitCredit.CR, TestObjectCreator.GLHeader1.PK).AL_OH = Org1.PK;
			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 5m, DebitCredit.DR, TestObjectCreator.GLHeader2.PK).AL_OH = Org1.PK;
			BusinessObjectFactory.SaveTogether(Factory, new AggregateWrapper(glStandardJournal, glStandardJournal));
		}

		#endregion

		#region ARAP Transfers

		public void CreateARAPTransfers()
		{
			TestObjectCreator.CreateTransfer<ARTransfer>(1m, June2008, Org1.PK, Org2.PK);
			TestObjectCreator.CreateTransfer<ARTransfer>(-10m, June2009, Org2.PK, Org3.PK);
			TestObjectCreator.CreateTransfer<APTransfer>(100m, June2010, Org3.PK, Org4.PK);
			TestObjectCreator.CreateTransfer<APTransfer>(-1000m, June2011, Org4.PK, Org1.PK);
			Factory.Save();
		}

		#endregion

		#region Exchange Differences

		public void CreateARAPExchangeDifferences()
		{
			TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(1m, June2008, Org1.PK);
			TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(-10m, June2009, Org2.PK);
			TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(100m, June2010, Org3.PK);
			TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(-1000m, June2011, Org4.PK);
			Factory.Save();
		}

		#endregion

		#region Contras

		public void CreateContras()
		{
			TestObjectCreator.CreateContra(1m, June2008, Org1.PK, Org2.PK);
			TestObjectCreator.CreateContra(-10m, June2009, Org2.PK, Org3.PK);
			TestObjectCreator.CreateContra(100m, June2010, Org3.PK, Org4.PK);
			TestObjectCreator.CreateContra(-1000m, June2011, Org4.PK, Org1.PK);
			Factory.Save();
		}

		#endregion

		#region Invoices

		public void CreateInvoices()
		{
			var apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, Org1);
			apInvoicePositive.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoicePositive.AH_PostDate = June2008;
			apInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -10m);
			apInvoicePositive.Lines[1].AL_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			APInvoice apInvoiceNegative = TestObjectCreator.CreateAPInvoice<APInvoice>("112", TestObjectCreator.AUD, 1.0m, -100m, -10m, 0m, -100m, -10m, 0m, Org2);
			apInvoiceNegative.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoiceNegative.AH_PostDate = June2009;
			apInvoiceNegative.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
			TestObjectCreator.CreateAPInvoiceLine(apInvoiceNegative, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 10m);
			apInvoiceNegative.Lines[1].AL_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			ARInvoice arInvoicePositive = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, Org3);
			arInvoicePositive.AH_PostDate = June2010;
			arInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			InvoicingLineBase line2 = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, -50m, -5m, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			Factory.Save();

			ARInvoice arInvoiceNegative = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, Org4);
			arInvoiceNegative.AH_PostDate = June2011;
			arInvoiceNegative.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
			line = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, TestObjectCreator.AUD, 1.0m, -100m, -10m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			line2 = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, TestObjectCreator.AUD, 1.0m, 50m, 5m, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			Factory.Save();
		}

		#endregion

		#region Credit Notes

		public void CreateCreditNotes()
		{
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
			APCreditNote apCreditNotePositive = TestObjectCreator.CreateAPCreditNoteWithLine("001", Org1, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 100.0m, June2008, false);
			TestObjectCreator.CreateAPCreditNoteLine(apCreditNotePositive, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -5m);
			Factory.Save();

			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001002"));
			APCreditNote apCreditNoteNegative = TestObjectCreator.CreateAPCreditNoteWithLine("002", Org2, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, -100.0m, June2009, false);
			TestObjectCreator.CreateAPCreditNoteLine(apCreditNoteNegative, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 5m);
			Factory.Save();

			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001003"));
			ARCreditNote arCreditNotePositive = TestObjectCreator.CreateARCreditNoteWithLine("003", Org3, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 1000.00m, June2010, false);
			TestObjectCreator.CreateARCreditNoteLine(arCreditNotePositive, job, TestObjectCreator.CC1, -100.00m, TestObjectCreator.AUD, 1.0m, "Desc");
			arCreditNotePositive.AH_PostDate = June2010;
			arCreditNotePositive.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNotePositive.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			Factory.Save();

			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001004"));
			ARCreditNote arCreditNoteNegative = TestObjectCreator.CreateARCreditNoteWithLine("004", Org4, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, -1000.00m, June2011, false);
			TestObjectCreator.CreateARCreditNoteLine(arCreditNoteNegative, job, TestObjectCreator.CC1, 100.00m, TestObjectCreator.AUD, 1.0m, "Desc");
			arCreditNoteNegative.AH_PostDate = June2011;
			arCreditNoteNegative.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNoteNegative.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			Factory.Save();
		}

		#endregion

		#region Adjusment Notes

		public void CreateAdjustmentNotes()
		{
			ARAdjustmentNote positiveARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 90.00m, 5m, new ZDateTime(2008, 06, 15), Org1.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC1.PK, 100.00m, 10m);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC2.PK, -10.00m, -5m);
			Factory.Save();

			ARAdjustmentNote negativeARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", -90.00m, -5m, new ZDateTime(2009, 06, 15), Org2.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, TestObjectCreator.CC1.PK, -100.00m, -10m);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, TestObjectCreator.CC2.PK, 10.00m, 5m);
			Factory.Save();

			APAdjustmentNote positiveAPAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("003", 90.00m, 5m, new ZDateTime(2010, 06, 15), Org3.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, TestObjectCreator.CC1.PK, 100.00m, 10m);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, TestObjectCreator.CC2.PK, -10.00m, -5m);
			Factory.Save();

			APAdjustmentNote negativeAPAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("004", -90.00m, -5m, new ZDateTime(2011, 06, 15), Org4.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, TestObjectCreator.CC1.PK, -100.00m, -10m);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, TestObjectCreator.CC2.PK, 10.00m, 5m);
			Factory.Save();
		}

		#endregion

		#region CB Transfers

		public void CreateCBTransfers()
		{
			BankTransfer positiveTransfer = TestObjectCreator.CreateBankTransfer(June2008, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 1000m, 0.90m);
			BankTransfer negativeTransfer = TestObjectCreator.CreateBankTransfer(June2009, TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, -5000m, 0.90m);
			Factory.Save();
		}

		#endregion

		#region CB Exchange Differences

		public void CreateCBExchangeDifferences()
		{
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			var exchangeDifference = Factory.New<CashbookExchangeDiff>();
			exchangeDifference.AH_InvoiceDate = June2008;
			exchangeDifference.AH_PostDate = June2008;
			exchangeDifference.AH_AB = BankAccountUSD.PK;
			exchangeDifference.AH_Desc = "test bank currency adjustment";
			exchangeDifference.AH_ExchangeRate = 1.5m;
			exchangeDifference.AH_LocalExTaxAmount = 100m;
			exchangeDifference.AH_LocalTaxAmount = 0m;
			Factory.Save();
		}

		#endregion

		#region CB Direct Payment

		public void CreateCBDirectPayments()
		{
			TestObjectCreator.CreateDirectPayment(June2008, 100m, 10m, 4000m, 400m);
			TestObjectCreator.CreateDirectPayment(June2009, -5000m, -500m, -10000m, -1000m);
			Factory.Save();
		}

		#endregion

		#region CB Direct Receipt

		public void CreateCBDirectReceipts()
		{
			TestObjectCreator.CreateDirectReceipt(June2010, 100m, 10m, 4000m, 400m);
			TestObjectCreator.CreateDirectReceipt(June2011, -5000m, -500m, -10000m, -1000m);
			Factory.Save();
		}

		#endregion

		#region Job Costing Journals

		public void CreateJobCostingJournals()
		{
			JCJournalHeader journal = TestObjectCreator.CreateJCJournalHeader(June2008, 0m);
			JCJournalLine line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, June2008, 50m);
			line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, June2008, 120m);
			Factory.Save();

			journal = TestObjectCreator.CreateJCJournalHeader(June2009, -180m);
			line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, June2009, -80m);
			line = TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, June2009, -100m);
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			line.AL_GE = department.PK;
			Factory.Save();
		}

		#endregion

		#region Job Revenue Journals

		public void CreateJobRevenueJournals()
		{
			Job job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 250m);
			((JobRevenueJournalLine)journal.Lines[0]).OSUnsignedLineAmount = 150m;	//purposely break JRJ so that we can see a result in the GL Aggregation
			Factory.Save();
		}

		#endregion

		#region WIPs and Accruals

		public void CreateWIPsAndAccruals()
		{
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001006"));
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_LocalSellAmt = 25m;
			charge.JR_OSSellAmt = 25m;
			charge.JR_LocalCostAmt = 50m;
			charge.JR_OSCostAmt = 50m;
			charge.JR_OH_SellAccount = Org1.PK;
			charge.JR_OH_CostAccount = Org2.PK;
			Factory.Save();

			charge.ARLine.AL_PostDate = June2008;
			charge.APLine.AL_PostDate = June2009;
			Factory.Save();

			charge.JR_LocalSellAmt = 400m;
			charge.JR_OSSellAmt = 400m;
			charge.JR_LocalCostAmt = 700m;
			charge.JR_OSCostAmt = 700m;
			Factory.Save();

			charge.ARLine.AL_PostDate = June2010;
			charge.APLine.AL_PostDate = June2011;
			Factory.Save();
		}

		#endregion
	}
}
#endif