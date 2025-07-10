using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	public class EliminationJournalCreatorTest : TestCaseWithFactory
	{
		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestEliminationJournalCreation_NoEliminationPresentationCategorySet_InRegistry()
		{
			CreateTransactions();

			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA1";
			category.Description = (NoResString)"Category 1";
			category.Bool = true; // active
			category.Bool2 = false; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(1, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			Factory.Save();

			var glPresentationJournalCaterogyList = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AssertNull("Precondition: no elimination category set up in presentation category registry", glPresentationJournalCaterogyList.EliminationCategory);

			new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();

			var batches = Factory.Load<AccConsolidationBatch>(new ZQuery());

			foreach (var batch in batches)
			{
				var consolidationBatchPkList = new List<ZGuid>();
				consolidationBatchPkList.Add(batch.PK);
				var consolidationBatchRows = new ConsolidationBatchExporter(consolidationBatchPkList, false).Export();

				var consolidationBatch = Factory.LoadTop1<AccConsolidationBatch>(new ZQuery(AccConsolidationBatchSchema.PK, SQLComparisonOperator.Equal, batch.PK));

				if (consolidationBatchRows.Any() && consolidationBatchRows.Any(x => !string.IsNullOrEmpty(x.TransactionOrganisationCode)))
				{
					AssertNull("No elimination journal has been created as no elimination category set in registry.", consolidationBatch.EliminationJournal);
				}
			}
		}

		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestEliminationJournalCreation_NoJournalCreated()
		{
			var creator = new EliminationJournalCreatorDebug();
			creator.NoTransactionFound = NoTransactionFoundHandlerForTest;
			Assert(!NoTransactionFoundRaised);
			creator.Create(new List<ZGuid>());
			Assert("No Transaction found error shoud be raised", NoTransactionFoundRaised);
		}

		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestEliminationJournalCreationFor_AllTransactions()
		{
			CreateTransactions();

			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA2";
			category.Description = (NoResString)"Category 2";
			category.Bool = true; // active
			category.Bool2 = true; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(1, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			Factory.Save();

			new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();

			var batches = Factory.Load<AccConsolidationBatch>(new ZQuery());

			foreach (var batch in batches)
			{
				var consolidationBatchPkList = new List<ZGuid>();
				consolidationBatchPkList.Add(batch.PK);
				var consolidationBatchRows = new ConsolidationBatchExporter(consolidationBatchPkList, false).Export();

				var consolidationBatch = Factory.LoadTop1<AccConsolidationBatch>(new ZQuery(AccConsolidationBatchSchema.PK, SQLComparisonOperator.Equal, batch.PK));

				if (consolidationBatchRows.Any() && consolidationBatchRows.Any(x => !string.IsNullOrEmpty(x.TransactionOrganisationCode)))
				{
					AssertNotNull(consolidationBatch.EliminationJournal);
					var journal = Factory.LoadTop1<GLJournals.GLJournal>(new ZQuery(AccTransactionHeaderSchema.PK, consolidationBatch.EliminationJournal.PK));

					AssertEquals(TransactionTypes.GLStandardJournal, journal.AH_TransactionType);
					AssertEquals("CA2", journal.AH_TransactionCategory);
					AssertEquals("Elimination General Ledger Journal", journal.AH_Desc);
					AssertEquals(consolidationBatch.YB_GC_Company, journal.AH_GC);
					AssertEquals(consolidationBatch.Period.AM_Period, journal.PostPeriod);

					Assert(journal.IsBalanced);

					foreach (var line in journal.Lines.Cast<GLJournalLine>())
					{
						AssertEquals(TransactionTypes.GLStandardJournal, line.AL_LineType);
						AssertEquals(journal.AH_GC, line.AL_GC);
						AssertEquals("Elimination General Ledger Journal", line.AL_Desc);

						var consolidationBatchRow = consolidationBatchRows.FirstOrDefault(x => x.BranchCode == line.Branch.GB_Code
							&& x.DepartmentCode.Trim() == line.Department.GE_Code
							&& x.GLAccount.Trim() == line.GLHeader.AG_AccountNum
							&& x.TransactionOrganisationCode.Trim() == Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, line.AL_OH)).OH_Code
							&& Math.Abs(x.AmountInTransactionCurrency) == line.UnsignedOSLineAmount);

						AssertNotNull(consolidationBatchRow);
					}
				}
			}
		}

		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestEliminationJournalCreation_Create_NoActiveBranch()
		{
			CreateTransactions();

			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA2";
			category.Description = (NoResString)"Category 2";
			category.Bool = true; // active
			category.Bool2 = true; // elimination
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(1, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);
			Factory.Save();

			new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();
			var branches = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));

			foreach (var branch in branches)
			{
				branch.GB_IsActive = false;
			}

			Factory.Save();
			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);

			var creator = new EliminationJournalCreatorDebug();
			creator.ShowError = ShowErrorHandlerForTest;

			AssertNoExceptionThrown(delegate
			{ creator.Create(batchPKs); });
		}

		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestEliminationJournalLineSkipSubAccountValidation()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			var consolidationGroup = Factory.New<AccConsolidationGroup>();
			consolidationGroup.YR_Code = "OCEANIA";
			consolidationGroup.YR_Description = "OCEANIA GROUP";
			consolidationGroup.GroupMembers.AddNew().YM_GC_Company = GlbCompany.CurrentCompany.PK;
			consolidationGroup.GroupMembers.AddNew().YM_OH_Organisation = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			invoice1.AH_PostDate = Env.Time.CurrentLocalDate.AddDays(-2);
			invoice1.AH_OSExTaxAmount = 100m;
			invoice1.AH_LocalExTaxAmount = 100m;
			TestObjectCreator.CreateInvoiceLine(invoice1, 100m, setTaxes: false);

			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			invoice2.AH_PostDate = Env.Time.CurrentLocalDate.AddDays(-1);
			invoice2.AH_OSExTaxAmount = 200m;
			invoice2.AH_LocalExTaxAmount = 200m;
			TestObjectCreator.CreateInvoiceLine(invoice2, 200m, setTaxes: false);

			Factory.Save();

			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA2";
			category.Description = (NoResString)"Category 2";
			category.Bool = true; // active
			category.Bool2 = true; // elimination
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(1, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);
			Factory.Save();

			new ConsolidationBatchCreator(consolidationGroup.PK).ReadDataAndCreateBatches();
			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);
			var creator = new EliminationJournalCreatorDebug();
			creator.Create(batchPKs);

			AssertEquals("Eliminal journal should has 1 row.", 1, creator.EliminationJournals_TestOnly.Count);
			Assert("Elimination journal should not has errors.", !creator.EliminationJournals_TestOnly[0].HasErrors);
			foreach (var line in creator.EliminationJournals_TestOnly[0].Lines.Cast<GLJournalLine>())
			{
				Assert("Journal line should not has errors.", !line.HasErrors);
				Assert("GL journal line should contain context AutoEliminationJournal.", line.HasContext(BusinessContext.AutoEliminationJournal));
			}
		}

		void ShowErrorHandlerForTest(string message, string caption)
		{
			return;
		}

		void NoTransactionFoundHandlerForTest(string message, string caption)
		{
			NoTransactionFoundRaised = true;
		}

		bool NoTransactionFoundRaised;

		#region SetUp

		TestObjectCreator TestObjectCreator { get; set; }
		AccConsolidationGroup group1;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);

			SetupPeriods();
			SetupCommonData();
			SetupControlAccounts();

			group1 = Factory.New<AccConsolidationGroup>();
			group1.YR_Code = "OCEANIA";
			group1.YR_Description = "OCEANIA GROUP";
			group1.GroupMembers.AddNew().YM_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}

		void SetupCommonData()
		{
			TestObjectCreator.AUDBankAccount.GLHeader.AG_AccountNum = "AUDAcc";
			TestObjectCreator.USDBankAccount.GLHeader.AG_AccountNum = "USDAcc";
			TestObjectCreator.GLHeader1.AG_AccountNum = "GLHeader1";
			TestObjectCreator.GLHeader2.AG_AccountNum = "GLHeader2";
		}

		void SetupPeriods()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2008, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2011, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void SetupControlAccounts()
		{
			var aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			var aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			var jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			var cFXAccount = TestObjectCreator.CreateCFXAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
		}

		#endregion

		AccBankAccount BankAccountAUD
		{
			get { return TestObjectCreator.AUDBankAccount; }
		}

		#region Create All Transactions

		void CreateTransactions()
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

		void CreatePayments()
		{
			CreateARPaymentPositive();
			CreateARPaymentNegative();
			CreateAPPaymentPositive();
			CreateAPPaymentNegative();
		}

		void CreateARPaymentPositive()
		{
			var date = new ZDateTime(2008, 06, 15);
			TestObjectCreator.CreateARPayment(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
		}

		void CreateARPaymentNegative()
		{
			var date = new ZDateTime(2009, 06, 15);
			TestObjectCreator.CreateARPayment(1.0m, -10m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
		}

		void CreateAPPaymentPositive()
		{
			var date = new ZDateTime(2010, 06, 15);
			TestObjectCreator.CreateAPPayment(1.0m, 10000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
		}

		void CreateAPPaymentNegative()
		{
			var date = new ZDateTime(2011, 06, 15);
			TestObjectCreator.CreateAPPayment(1.0m, -1000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
		}

		#endregion

		#region Receipts

		void CreateReceipts()
		{
			var date = new ZDateTime(2009, 06, 15);
			TestObjectCreator.CreateARReceipt(1.0m, -10m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			date = new ZDateTime(2010, 06, 15);
			TestObjectCreator.CreateAPReceipt(1.0m, 100m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);

			date = new ZDateTime(2011, 06, 15);
			TestObjectCreator.CreateAPReceipt(1.0m, -1000m, date, date, TestObjectCreator.ABIGAS.PK, BankAccountAUD.PK);
		}

		#endregion

		#region Discounts

		void CreateDiscounts()
		{
			var date = new ZDateTime(2008, 06, 15);
			TestObjectCreator.CreateAPDiscount(10m, date, TestObjectCreator.ABIGAS.PK);

			date = new ZDateTime(2009, 06, 15);
			TestObjectCreator.CreateAPDiscount(-10m, date, TestObjectCreator.ABIGAS.PK);

			date = new ZDateTime(2010, 06, 15);
			TestObjectCreator.CreateARDiscount(10m, date, TestObjectCreator.ABIGAS.PK);

			date = new ZDateTime(2011, 06, 15);
			TestObjectCreator.CreateARDiscount(-10m, date, TestObjectCreator.ABIGAS.PK);
		}

		#endregion

		#region Overpayments

		void CreateOverpayments()
		{
			var date = new ZDateTime(2008, 06, 15);
			TestObjectCreator.CreateOverpayment<AROverpayment>(150m, date, TestObjectCreator.ABIGAS.PK);

			date = new ZDateTime(2009, 06, 15);
			TestObjectCreator.CreateOverpayment<APOverpayment>(20m, date, TestObjectCreator.ABIGAS.PK);

			date = new ZDateTime(2010, 06, 15);
			TestObjectCreator.CreateOverpayment<AROverpayment>(-150m, date, TestObjectCreator.ABIGAS.PK);

			date = new ZDateTime(2011, 06, 15);
			TestObjectCreator.CreateOverpayment<APOverpayment>(-20m, date, TestObjectCreator.ABIGAS.PK);
		}

		#endregion

		#region Journals

		void CreateARAPJournals()
		{
			TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2008, 6, 15), TestObjectCreator.AALSHI.PK);

			TestObjectCreator.CreateJournal<ARJournal>(-50m, new ZDateTime(2009, 6, 15), TestObjectCreator.AALSHI.PK);

			TestObjectCreator.CreateJournal<APJournal>(10000m, new ZDateTime(2010, 6, 15), TestObjectCreator.ABIGAS.PK);

			TestObjectCreator.CreateJournal<APJournal>(-5000m, new ZDateTime(2011, 6, 15), TestObjectCreator.ABIGAS.PK);
		}

		#endregion

		#region GL Journals

		void CreateGLJournals()
		{
			var glStandardJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2008, 2, 15), new ZDateTime(2008, 2, 28, 23, 59, 0), null);

			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 50m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 300m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glStandardJournal.Balance();

			var glAutoJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2009, 02, 15), new ZDateTime(2009, 2, 28, 23, 59, 0), new ZDateTime(2009, 6, 30, 23, 59, 0));
			TestObjectCreator.CreateGLJournalLine(glAutoJournal, 200m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glAutoJournal, 1000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glAutoJournal.Balance();

			var glReversingJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLReversingJournal, new ZDateTime(2010, 02, 15), new ZDateTime(2010, 2, 28, 23, 59, 0), new ZDateTime(2010, 6, 1));
			TestObjectCreator.CreateGLJournalLine(glReversingJournal, 10000m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glReversingJournal, 3000m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			glReversingJournal.Balance();
		}

		#endregion

		#region ARAP Transfers

		void CreateARAPTransfers()
		{
			TestObjectCreator.CreateTransfer<ARTransfer>(100m, new ZDateTime(2008, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK); //arTransferPositive

			TestObjectCreator.CreateTransfer<ARTransfer>(-10m, new ZDateTime(2009, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK); //arTransferNegative

			TestObjectCreator.CreateTransfer<APTransfer>(10000m, new ZDateTime(2010, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK); //apTransferPositive

			TestObjectCreator.CreateTransfer<APTransfer>(-1000m, new ZDateTime(2011, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK); //apTransferNegative
		}

		#endregion

		#region Exchange Differences

		void CreateARAPExchangeDifferences()
		{
			CreateARExchangeDifference(new ZDateTime(2008, 6, 15), 20m);

			CreateAPExchangeDifference(new ZDateTime(2009, 6, 15), 30m);

			CreateARExchangeDifference(new ZDateTime(2010, 6, 15), -30m);

			CreateAPExchangeDifference(new ZDateTime(2011, 6, 15), -20m);
		}

		void CreateARExchangeDifference(ZDateTime postDate, decimal exchangeDifferenceAmount)
		{
			TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(-exchangeDifferenceAmount, postDate, TestObjectCreator.ABIGAS.PK);
		}

		void CreateAPExchangeDifference(ZDateTime postDate, decimal exchangeDifferenceAmount)
		{
			TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(-exchangeDifferenceAmount, postDate, TestObjectCreator.ABIGAS.PK);
		}

		#endregion

		#region Contras

		void CreateContras()
		{
			TestObjectCreator.CreateContra(1000m, new ZDateTime(2008, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK); //contraPositive

			TestObjectCreator.CreateContra(-500m, new ZDateTime(2009, 6, 15), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK); //contraNegative
		}

		#endregion

		#region Invoices

		void CreateInvoices()
		{
			var apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			apInvoicePositive.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoicePositive.AH_PostDate = new ZDateTime(2008, 06, 15);
			apInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -10m);
			apInvoicePositive.Lines[1].AL_AC = TestObjectCreator.CC1.PK;

			var apInvoiceNegative = TestObjectCreator.CreateAPInvoice<APInvoice>("112", TestObjectCreator.AUD, 1.0m, -100m, -10m, 0m, -100m, -10m, 0m, TestObjectCreator.AALSHI);
			apInvoiceNegative.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoiceNegative.AH_PostDate = new ZDateTime(2008, 12, 15);
			apInvoiceNegative.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
			TestObjectCreator.CreateAPInvoiceLine(apInvoiceNegative, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 10m);
			apInvoiceNegative.Lines[1].AL_AC = TestObjectCreator.CC1.PK;

			var arInvoicePositive = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			arInvoicePositive.AH_PostDate = new ZDateTime(2009, 06, 15);
			arInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
			var line = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(arInvoicePositive, TestObjectCreator.AUD, 1.0m, -50m, -5m, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;

			var arInvoiceNegative = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			arInvoiceNegative.AH_PostDate = new ZDateTime(2009, 12, 15);
			arInvoiceNegative.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TCD;
			line = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, TestObjectCreator.AUD, 1.0m, -100m, -10m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			line2 = TestObjectCreator.CreateInvoiceLine(arInvoiceNegative, TestObjectCreator.AUD, 1.0m, 50m, 5m, 0m);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
		}

		#endregion

		#region Credit Notes

		void CreateCreditNotes()
		{
			var date = new ZDateTime(2008, 6, 15);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
			var apCreditNotePositive = TestObjectCreator.CreateAPCreditNoteWithLine("001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 100.0m, date, postToGL: false);
			TestObjectCreator.CreateAPCreditNoteLine(apCreditNotePositive, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -5m);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_APLine = apCreditNotePositive.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostGSTAmt_Calc = -apCreditNotePositive.Lines[0].AL_OSGSTAmount;
			charge.JR_OSSellAmt = 0M;
			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_APLine = apCreditNotePositive.Lines[1].PK;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSCostGSTAmt_Calc = -apCreditNotePositive.Lines[1].AL_OSGSTAmount;
			charge2.JR_OSSellAmt = 0M;

			date = new ZDateTime(2008, 12, 15);
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001002"));
			var apCreditNoteNegative = TestObjectCreator.CreateAPCreditNoteWithLine("002", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, -100.0m, date, postToGL: false);
			TestObjectCreator.CreateAPCreditNoteLine(apCreditNoteNegative, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 5m);
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_APLine = apCreditNoteNegative.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostGSTAmt_Calc = -apCreditNoteNegative.Lines[0].AL_OSGSTAmount;
			charge.JR_OSSellAmt = 0M;
			charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_APLine = apCreditNoteNegative.Lines[1].PK;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSCostGSTAmt_Calc = -apCreditNoteNegative.Lines[1].AL_OSGSTAmount;
			charge2.JR_OSSellAmt = 0M;

			date = new ZDateTime(2009, 06, 15);
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001003"));
			var arCreditNotePositive = TestObjectCreator.CreateARCreditNoteWithLine("003", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 1000.00m, date, postToGL: false);
			TestObjectCreator.CreateARCreditNoteLine(arCreditNotePositive, job, TestObjectCreator.CC1, -100.00m, TestObjectCreator.AUD, 1.0m, "Desc");
			arCreditNotePositive.AH_PostDate = date;
			arCreditNotePositive.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNotePositive.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_ARLine = arCreditNotePositive.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostAmt = 0M;
			charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_ARLine = arCreditNotePositive.Lines[1].PK;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSCostAmt = 0M;

			date = new ZDateTime(2009, 12, 15);
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001004"));
			var arCreditNoteNegative = TestObjectCreator.CreateARCreditNoteWithLine("004", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, -1000.00m, date, postToGL: false);
			TestObjectCreator.CreateARCreditNoteLine(arCreditNoteNegative, job, TestObjectCreator.CC1, 100.00m, TestObjectCreator.AUD, 1.0m, "Desc");
			arCreditNoteNegative.AH_PostDate = date;
			arCreditNoteNegative.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNoteNegative.Lines[1].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_AL_ARLine = arCreditNoteNegative.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			charge.JR_OSCostAmt = 0M;
			charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_AL_ARLine = arCreditNoteNegative.Lines[1].PK;
			charge2.SetAmountsFromLinkedLinesForTests();
			charge2.JR_OSCostAmt = 0M;
		}

		#endregion

		#region Adjusment Notes

		void CreateAdjustmentNotes()
		{
			var positiveARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 90.00m, 5m, new ZDateTime(2008, 06, 15), TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC1.PK, 100.00m, 10m);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC2.PK, -10.00m, -5m);

			var negativeARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("002", -90.00m, -5m, new ZDateTime(2009, 06, 15), TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, TestObjectCreator.CC1.PK, -100.00m, -10m);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeARAdjustmentNote, TestObjectCreator.CC2.PK, 10.00m, 5m);

			var positiveAPAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("003", 90.00m, 5m, new ZDateTime(2010, 06, 15), TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, TestObjectCreator.CC1.PK, 100.00m, 10m);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveAPAdjustmentNote, TestObjectCreator.CC2.PK, -10.00m, -5m);

			var negativeAPAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<APAdjustmentNote>("004", -90.00m, -5m, new ZDateTime(2011, 06, 15), TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, TestObjectCreator.CC1.PK, -100.00m, -10m);
			TestObjectCreator.CreateAdjusmentNoteLine(negativeAPAdjustmentNote, TestObjectCreator.CC2.PK, 10.00m, 5m);
		}

		#endregion

		#region CB Transfers

		void CreateCBTransfers()
		{
			TestObjectCreator.CreateBankTransfer(new ZDateTime(2008, 6, 15), TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, 1000m, 0.90m); //positiveTransfer

			TestObjectCreator.CreateBankTransfer(new ZDateTime(2009, 6, 15), TestObjectCreator.AUDBankAccount.PK, TestObjectCreator.USDBankAccount.PK, -5000m, 0.90m); //negativeTransfer
		}

		#endregion

		#region CB Exchange Differences

		void CreateCBExchangeDifferences()
		{
			var exchangeRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_SellRate = 1.234m;
			exchangeRate.RE_RX_NKExCurrency = "JPY";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			TestObjectCreator.CreateCashbookExchangeDifference(new ZDateTime(2008, 6, 15), 100m, TestObjectCreator.AUDBankAccount, saveCashBook: false);
		}

		#endregion

		#region CB Direct Payment

		void CreateCBDirectPayments()
		{
			TestObjectCreator.CreateDirectPayment(new ZDateTime(2008, 6, 15), 100m, 10m, 4000m, 400m);

			TestObjectCreator.CreateDirectPayment(new ZDateTime(2009, 6, 15), -5000m, -500m, -10000m, -1000m);
		}

		#endregion

		#region CB Direct Receipt

		void CreateCBDirectReceipts()
		{
			TestObjectCreator.CreateDirectReceipt(new ZDateTime(2010, 6, 15), 100m, 10m, 4000m, 400m);

			TestObjectCreator.CreateDirectReceipt(new ZDateTime(2011, 6, 15), -5000m, -500m, -10000m, -1000m);
		}

		#endregion

		#region Job Costing Journals

		void CreateJobCostingJournals()
		{
			var date = new ZDateTime(2008, 6, 15);
			var journal = TestObjectCreator.CreateJCJournalHeader(date, 0m);
			TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC1, null, date, 50m);
			TestObjectCreator.CreateJCJournalLine(journal, TestObjectCreator.CC2, null, date, 120m);
		}

		#endregion

		#region Job Revenue Journals

		void CreateJobRevenueJournals()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 250m);
			((JobRevenueJournalLine)journal.Lines[0]).OSUnsignedLineAmount = 150m;  //purposely break JRJ so that we can see a result in the GL Aggregation
		}

		#endregion

		#region WIPs and Accruals

		void CreateWIPsAndAccruals()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001006"));
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;

			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			charge.JR_LocalSellAmt = 400m;
			charge.JR_OSSellAmt = 400m;
			charge.JR_LocalCostAmt = 700m;
			charge.JR_OSCostAmt = 700m;
		}

		#endregion
	}
}
