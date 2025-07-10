using System;
using System.Data;
using System.IO;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AccGLAggregate = Enterprise.MasterFiles.Business.AccGLAggregate;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class GLGeneralOnlineAggregatorTest : TestCaseWithFactory
	{
		protected Guid GLAccountPK0;
		protected Guid GLAccountPK1;
		protected Guid GLAccountPK2;

		protected Guid BranchPK0;
		protected Guid BranchPK1;
		protected Guid BranchPK2;

		protected Guid DepartmentPK0;
		protected Guid DepartmentPK1;
		protected Guid DepartmentPK2;

		protected const int Period0 = 200302;
		protected const int Period1 = 200303;
		protected const int Period2 = 200304;

		protected const int ReversePeriod0 = 200305;
		protected const int ReversePeriod1 = 200306;
		protected const int ReversePeriod2 = 200307;

		protected const decimal LineAmount0 = 150.0m;
		protected const decimal LineAmount1 = -100.0m;
		protected const decimal LineAmount2 = -50.0m;

		protected const decimal NewLineAmount1 = -70.0m;
		protected Guid NewDepartmentPK2;
		protected Guid NewBranchPK2;

		protected GLJournal GL;
		protected GLJournal OriginalGL;

		protected BusinessObjectFactory TestFactory;
		protected BusinessObjectFactory TestFactory2;

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(MasterFiles.Business.AccPeriodManagement.Schema.TableName);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(Period0, new ZDateTime(2003, 2, 1), new ZDateTime(2003, 2, 28));
			testHelper.SetupSinglePeriod(Period1, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 30));
			testHelper.SetupSinglePeriod(Period2, new ZDateTime(2003, 4, 1), new ZDateTime(2003, 4, 30));
			testHelper.SetupSinglePeriod(ReversePeriod0, new ZDateTime(2003, 5, 1), new ZDateTime(2003, 5, 30));
			testHelper.SetupSinglePeriod(ReversePeriod1, new ZDateTime(2003, 6, 1), new ZDateTime(2003, 6, 30));
			testHelper.SetupSinglePeriod(ReversePeriod2, new ZDateTime(2003, 7, 1), new ZDateTime(2003, 7, 30));

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);

			TestFactory = new BusinessObjectFactory();
			TestFactory2 = new BusinessObjectFactory();

			ZQuery query = new ZQuery();
			query.MaximumRows = 4;

			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch newBranch = currentCompany.Branches.AddNew();
			newBranch.GB_Code = "ZZZ";
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			AccGLHeader[] gLHeader = TestFactory.Load<AccGLHeader>(query);
			GlbBranch[] branches = TestFactory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			GlbDepartment[] departments = TestFactory.Load<GlbDepartment>(query);

			GLAccountPK0 = gLHeader[0].PK.ToGuid();
			GLAccountPK1 = gLHeader[1].PK.ToGuid();
			GLAccountPK2 = gLHeader[2].PK.ToGuid();

			BranchPK0 = branches[0].PK.ToGuid();
			BranchPK1 = branches[1].PK.ToGuid();
			BranchPK2 = branches[2].PK.ToGuid();
			NewBranchPK2 = branches[3].PK.ToGuid();

			DepartmentPK0 = departments[0].PK.ToGuid();
			DepartmentPK1 = departments[1].PK.ToGuid();
			DepartmentPK2 = departments[2].PK.ToGuid();

			GL = TestFactory.New(typeof(GLJournal)) as GLJournal;
			OriginalGL = TestFactory2.New(typeof(GLJournal)) as GLJournal;

			AccChargeCode charge = TestFactory.LoadTop1<AccChargeCode>(new ZQuery());

			GL.AH_Ledger = LedgerTypes.General;
			GL.AH_TransactionType = TransactionTypes.GLStandardJournal;
			GL.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			GL.PostPeriod = Period0;

			GL.GLJournalLines.AddNew();
			GL.GLJournalLines.AddNew();
			GL.GLJournalLines.AddNew();

			OriginalGL.GLJournalLines.AddNew();
			OriginalGL.GLJournalLines.AddNew();
			OriginalGL.GLJournalLines.AddNew();

			GL.GLJournalLines[0].AL_PostDate = OriginalGL.GLJournalLines[0].AL_PostDate = periodCalc.GetLastDayForPeriod(Period0).ToDateTime();
			GL.GLJournalLines[0].AL_OSExTaxAmount = OriginalGL.GLJournalLines[0].AL_OSExTaxAmount = LineAmount0;
			GL.GLJournalLines[0].AL_AG = OriginalGL.GLJournalLines[0].AL_AG = GLAccountPK0;
			GL.GLJournalLines[0].AL_GB = OriginalGL.GLJournalLines[0].AL_GB = BranchPK0;
			GL.GLJournalLines[0].AL_AC = OriginalGL.GLJournalLines[0].AL_AC = charge.PK;
			GL.GLJournalLines[0].AL_GE = OriginalGL.GLJournalLines[0].AL_GE = GlbDepartment.CurrentDepartment.PK;
			GL.GLJournalLines[0].AL_ReverseDate = OriginalGL.GLJournalLines[0].AL_ReverseDate = periodCalc.GetLastDayForPeriod(ReversePeriod0).ToDateTime();

			GL.GLJournalLines[1].AL_PostDate = OriginalGL.GLJournalLines[1].AL_PostDate = periodCalc.GetLastDayForPeriod(Period0).ToDateTime();
			GL.GLJournalLines[1].AL_OSExTaxAmount = OriginalGL.GLJournalLines[1].AL_OSExTaxAmount = LineAmount1;
			GL.GLJournalLines[1].AL_AG = OriginalGL.GLJournalLines[1].AL_AG = GLAccountPK1;
			GL.GLJournalLines[1].AL_GB = OriginalGL.GLJournalLines[1].AL_GB = BranchPK1;
			GL.GLJournalLines[1].AL_AC = OriginalGL.GLJournalLines[1].AL_AC = charge.PK;
			GL.GLJournalLines[1].AL_GE = OriginalGL.GLJournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			GL.GLJournalLines[1].AL_ReverseDate = OriginalGL.GLJournalLines[1].AL_ReverseDate = periodCalc.GetLastDayForPeriod(ReversePeriod0).ToDateTime();

			GL.GLJournalLines[2].AL_PostDate = OriginalGL.GLJournalLines[2].AL_PostDate = periodCalc.GetLastDayForPeriod(Period0).ToDateTime();
			GL.GLJournalLines[2].AL_OSExTaxAmount = OriginalGL.GLJournalLines[2].AL_OSExTaxAmount = LineAmount2;
			GL.GLJournalLines[2].AL_AG = OriginalGL.GLJournalLines[2].AL_AG = GLAccountPK2;
			GL.GLJournalLines[2].AL_GB = OriginalGL.GLJournalLines[2].AL_GB = BranchPK2;
			GL.GLJournalLines[2].AL_AC = OriginalGL.GLJournalLines[2].AL_AC = charge.PK;
			GL.GLJournalLines[2].AL_GE = OriginalGL.GLJournalLines[2].AL_GE = GlbDepartment.CurrentDepartment.PK;
			GL.GLJournalLines[2].AL_ReverseDate = OriginalGL.GLJournalLines[2].AL_ReverseDate = periodCalc.GetLastDayForPeriod(ReversePeriod0).ToDateTime();
		}

		public void TestAmountUpdated()
		{
			InsertExistingAggregates();

			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			dbCommandFactory.ExecuteNonEmptyCommands();

			ZQuery query = new ZQuery(AccGLAggregateSchema.AA_AG, GLAccountPK0);
			AccGLAggregate[] result = Factory.Load<AccGLAggregate>(query);

			AssertEquals(200.0m, CountAggregateTotal(result));

			query = new ZQuery(AccGLAggregateSchema.AA_AG, GLAccountPK1);
			result = Factory.Load<AccGLAggregate>(query);

			AssertEquals(-150.0m, CountAggregateTotal(result));
		}

		public void TestNothingUpdatedIfAmountIsNotChanged()
		{
			InsertExistingAggregates();

			GL.AH_Desc = "Original Desc";
			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			dbCommandFactory.ExecuteNonEmptyCommands();

			// Pre Condition Assert
			AccGLAggregate[] result = GetAggregateResult(GLAccountPK0);
			AssertEquals(200.0m, CountAggregateTotal(result));

			result = GetAggregateResult(GLAccountPK1);
			AssertEquals(-150.0m, CountAggregateTotal(result));

			TestFactory.Save();

			// Change Description of Existing Journal
			GLJournal exsitingJournal = Factory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			exsitingJournal.AH_Desc = "Changed";
			GLGeneralOnlineAggregator newAggregator = new GLGeneralOnlineAggregator(exsitingJournal, GL);

			newAggregator.Aggregate();

			result = GetAggregateResult(GLAccountPK0);
			AssertEquals(200.0m, CountAggregateTotal(result));

			result = GetAggregateResult(GLAccountPK1);
			AssertEquals(-150.0m, CountAggregateTotal(result));
		}

		AccGLAggregate[] GetAggregateResult(Guid pK)
		{
			ZQuery query = new ZQuery(AccGLAggregateSchema.AA_AG, pK);
			return Factory.Load<AccGLAggregate>(query);
		}

		protected decimal CountAggregateTotal(AccGLAggregate[] aggregate)
		{
			decimal result = 0.0m;

			foreach (AccGLAggregate data in aggregate)
			{
				result += data.AA_Amount;
			}

			return result;
		}

		protected void InsertExistingAggregates()
		{
			AccGLAggregate aggregateToInsert = Factory.New(typeof(AccGLAggregate)) as AccGLAggregate;
			aggregateToInsert.AA_AG = GLAccountPK0;
			aggregateToInsert.AA_GE = DepartmentPK0;
			aggregateToInsert.AA_GB = BranchPK0;
			aggregateToInsert.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregateToInsert.AA_Amount = 20.0m;
			aggregateToInsert.AA_Period = Period0;
			Factory.Save();

			aggregateToInsert = Factory.New(typeof(AccGLAggregate)) as AccGLAggregate;
			aggregateToInsert.AA_AG = GLAccountPK0;
			aggregateToInsert.AA_GE = DepartmentPK0;
			aggregateToInsert.AA_GB = BranchPK0;
			aggregateToInsert.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregateToInsert.AA_Amount = 30.0m;
			aggregateToInsert.AA_Period = Period0;
			Factory.Save();

			aggregateToInsert = Factory.New(typeof(AccGLAggregate)) as AccGLAggregate;
			aggregateToInsert.AA_AG = GLAccountPK1;
			aggregateToInsert.AA_GE = DepartmentPK0;
			aggregateToInsert.AA_GB = BranchPK0;
			aggregateToInsert.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregateToInsert.AA_Amount = -50.0m;
			aggregateToInsert.AA_Period = Period0;
			Factory.Save();
		}

		public void TestNewGeneralJournalWithBizO()
		{
			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateDbCommandForBizO();

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestGenerateNewStatementWithBizO()
		{
			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateNewCommandsWithBizO(dbCommandFactory, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateDbCommandForBizO();

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestGenerateReverseStatementWithBizO()
		{
			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateReverseCommandsWithBizO(dbCommandFactory, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateDbCommandForBizO(-1.0m);
			//			ExpectedCommand.Parameters["@LineAmount0"].Value = -1.0m * LineAmount0;
			//			ExpectedCommand.Parameters["@LineAmount1"].Value = -1.0m * LineAmount1;
			//			ExpectedCommand.Parameters["@LineAmount2"].Value = -1.0m * LineAmount2;

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestDeletedGeneralJournalWithBizO()
		{
			TestFactory.Save();

			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[1].Delete();

			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1, @PostPeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK1", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@BranchPK1", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK1", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod1", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@LineAmount1", SqlDbType.Money, LineAmount1 * -1.0000m);
			expectedCommand.AddParameter("@TransactionCategory1", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestAmountUpdatedGeneralJournalWithBizO()
		{
			//Re-load Original GL
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			TestFactory.Save();
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[1].AL_RX_NKTransactionCurrency = "USD";
			GL.GLJournalLines[1].AL_ExchangeRate = 0.90M;
			GL.GLJournalLines[1].AL_LocalExTaxAmount = NewLineAmount1;

			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1, @PostPeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK1", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@BranchPK1", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK1", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod1", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@LineAmount1", SqlDbType.Money, NewLineAmount1 - LineAmount1);
			expectedCommand.AddParameterBasedOnDbColumn("@TransactionCategory1", InvoiceTypesList.Codes.FinalInvoice, AccTransactionHeaderSchema.AH_TransactionCategory);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestKeyChangedUpdatedGeneralJournalWithBizO()
		{
			//Re-load Original GL
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			TestFactory.Save();
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[2].AL_GB = NewBranchPK2;

			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount2, @PostPeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount2, @NewPostPeriod2, @NewGLAccountPK2, @NewBranchPK2, @NewCompanyPK2, @NewDepartmentPK2, @NewTransactionCategory2) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);
			expectedCommand.AddParameter("@BranchPK2", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@CompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod2", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@LineAmount2", SqlDbType.Money, -LineAmount2);
			expectedCommand.AddParameter("@TransactionCategory2", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewGLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);
			expectedCommand.AddParameter("@NewBranchPK2", SqlDbType.UniqueIdentifier, NewBranchPK2);
			expectedCommand.AddParameter("@NewCompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod2", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@NewLineAmount2", SqlDbType.Money, LineAmount2);
			expectedCommand.AddParameter("@NewTransactionCategory2", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		[ExpectNoExceptions]
		public void TestCanProcessLargeJornals()
		{
			FlatFileDataImporter importer = (FlatFileDataImporter)Activator.CreateInstance(ObjectFactory.GetType<Integration.IGLJournalFlatFileDataImporter>());
			StringReader reader = new StringReader(@"GLJHEAD,GJL,HA ,200701,200701,ADM,BRN
GLJLINE,1030.10.00,ADM,BRN,Operating Account - FL Capital Bank,""12,039.37"",DR
GLJLINE,1030.10.00,ADM,BRN,Intl Account - FL Capital Bank,""20,570.31"",CR
GLJLINE,1030.10.00,ADM,BRN,Onsite Acct - FL Capital Bank,""18,923.01"",DR
GLJLINE,1030.10.00,MCO,BRN,Onsite Acct - FL Capital Bank,""6,324.78"",CR
GLJLINE,1030.10.00,MEM,BRN,Onsite Acct - FL Capital Bank,95,CR
GLJLINE,1030.10.00,MIA,BRN,Onsite Acct - FL Capital Bank,""12,503.23"",CR
GLJLINE,1030.10.00,ADM,BRN,Onsite Acct DEN - FL Capital Bank,""5,262.37"",DR
GLJLINE,1030.10.00,DEN,BRN,Onsite Acct DEN - FL Capital Bank,""5,262.07"",CR
GLJLINE,1030.10.00,ADM,BRN,Payroll Account - Wachovia Bank,""2,500.00"",DR
GLJLINE,1030.10.00,ADM,BRN,Money Market Account - FL Capital,""4,372.08"",DR
GLJLINE,1030.10.00,CMH,BRN,AR Clearing Journal,""22,573.59"",DR
GLJLINE,1030.10.00,DEN,BRN,AR Clearing Journal,""92,359.13"",DR
GLJLINE,1030.10.00,MCO,BRN,AR Clearing Journal,""71,902.81"",CR
GLJLINE,1030.10.00,MEM,BRN,AR Clearing Journal,""455,687.94"",CR
GLJLINE,1030.10.00,MIA,BRN,AR Clearing Journal,""52,015.31"",CR
GLJLINE,1030.10.00,FLX,BRN,Accounts Receivable - Other,""124,067.81"",DR
GLJLINE,1030.10.00,TLC,BRN,Accounts Receivable - Other,""11,295.58"",CR
GLJLINE,1030.10.00,MCO,BRN,Accounts Receivable Adjustment,447.6,DR
GLJLINE,1030.10.00,MEM,BRN,Accounts Receivable Adjustment,""2,475.41"",DR
GLJLINE,1030.10.00,TLC,BRN,Accounts Receivable Adjustment,""3,035.21"",CR
GLJLINE,1030.10.00,CMH,BRN,Allowance for Doubtful Accts,350,CR
GLJLINE,1030.10.00,DEN,BRN,Allowance for Doubtful Accts,""1,100.00"",CR
GLJLINE,1030.10.00,FLX,BRN,Allowance for Doubtful Accts,""1,000.00"",CR
GLJLINE,1030.10.00,MCO,BRN,Allowance for Doubtful Accts,""10,034.98"",DR
GLJLINE,1030.10.00,MIA,BRN,Allowance for Doubtful Accts,450,CR
GLJLINE,1030.10.00,TLC,BRN,Allowance for Doubtful Accts,350,CR
GLJLINE,1030.10.00,MEM,BRN,Prepaid  Health Insurance,""3,250.00"",DR
GLJLINE,1030.10.00,ADM,BRN,Prepaid  Health Insurance,653.69,DR
GLJLINE,1030.10.00,ADM,BRN,Prepaid Workers' Comp Insurance,""1,634.95"",CR
GLJLINE,1030.10.00,MEM,BRN,Prepaid Workers' Comp Insurance,564.98,DR
GLJLINE,1030.10.00,ADM,BRN,Prepaid Insurance and Other Exp,""65,392.87"",CR
GLJLINE,1030.10.00,CMH,BRN,Prepaid Insurance and Other Exp,125,CR
GLJLINE,1030.10.00,DEN,BRN,Prepaid Insurance and Other Exp,125,CR
GLJLINE,1030.10.00,FLX,BRN,Prepaid Insurance and Other Exp,458.33,CR
GLJLINE,1030.10.00,MCO,BRN,Prepaid Insurance and Other Exp,721.27,CR
GLJLINE,1030.10.00,MEM,BRN,Prepaid Insurance and Other Exp,""31,720.00"",CR
GLJLINE,1030.10.00,ADM,BRN,Prepaid Computer Services,505.66,CR
GLJLINE,1030.10.00,ADM,BRN,Prepaid Computer Services,""1,366.76"",CR
GLJLINE,1030.10.00,MEM,BRN,Prepaid Computer Services,390.81,CR
GLJLINE,1030.10.00,FLX,BRN,Supplies on Hand,700,CR
GLJLINE,1030.10.00,MCO,BRN,Supplies on Hand,""3,650.80"",CR
GLJLINE,1030.10.00,TLC,BRN,Prepaid Insurance and Other Exp,""1,256.90"",CR
GLJLINE,1030.10.00,ADM,BRN,Employee Advances,254.51,CR
GLJLINE,1030.10.00,ADM,BRN,Employee Advances,69.55,DR
GLJLINE,1030.10.00,DEN,BRN,Employee Advances,""12,800.28"",DR
GLJLINE,1030.10.00,MCO,BRN,Employee Advances,""1,061.30"",DR
GLJLINE,1030.10.00,ADM,BRN,Due from DGL,""418,285.13"",DR
GLJLINE,1030.10.00,MEM,BRN,Due from DGL,965.79,DR
GLJLINE,1030.10.00,MCO,BRN,Note Receivable - TLS,""3,156.87"",CR
GLJLINE,1030.10.00,MEM,BRN,Vehicles,""91,332.78"",CR
GLJLINE,1030.10.00,MEM,BRN,Accum Depr - Vehicles,""84,139.49"",DR
GLJLINE,1030.10.00,TLC,BRN,Accum Depr - Vehicles,""2,201.13"",CR
GLJLINE,1030.10.00,MEM,BRN,Furniture and Equipment,""36,641.13"",CR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Furniture and Equip,195.67,CR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Furniture and Equip,44.36,CR
GLJLINE,1030.10.00,CMH,BRN,Accum Depr - Furniture and Equip,54.73,CR
GLJLINE,1030.10.00,DEN,BRN,Accum Depr - Furniture and Equip,194.42,CR
GLJLINE,1030.10.00,MCO,BRN,Accum Depr - Furniture and Equip,405.91,CR
GLJLINE,1030.10.00,MEM,BRN,Accum Depr - Furniture and Equip,""25,617.06"",DR
GLJLINE,1030.10.00,MIA,BRN,Accum Depr - Furniture and Equip,42.47,CR
GLJLINE,1030.10.00,TLC,BRN,Accum Depr - Furniture and Equip,38.98,CR
GLJLINE,1030.10.00,MCO,BRN,Warehouse Equipment,""1,193.65"",DR
GLJLINE,1030.10.00,MEM,BRN,Warehouse Equipment,""152,299.02"",CR
GLJLINE,1030.10.00,CMH,BRN,Accum Depr - Warehouse Equipment,106.11,CR
GLJLINE,1030.10.00,DEN,BRN,Accum Depr - Warehouse Equipment,30.52,CR
GLJLINE,1030.10.00,MCO,BRN,Accum Depr - Warehouse Equipment,""3,376.84"",CR
GLJLINE,1030.10.00,MEM,BRN,Accum Depr - Warehouse Equipment,""95,147.27"",DR
GLJLINE,1030.10.00,MEM,BRN,Computer Equipment,""32,747.71"",CR
GLJLINE,1030.10.00,MIA,BRN,Computer Equipment,""1,037.44"",DR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Computer Equipment,210.73,CR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Computer Equipment,""1,073.06"",CR
GLJLINE,1030.10.00,CMH,BRN,Accum Depr - Computer Equipment,211.12,CR
GLJLINE,1030.10.00,DEN,BRN,Accum Depr - Computer Equipment,456.66,CR
GLJLINE,1030.10.00,MCO,BRN,Accum Depr - Computer Equipment,700.14,CR
GLJLINE,1030.10.00,MEM,BRN,Accum Depr - Computer Equipment,""23,677.09"",DR
GLJLINE,1030.10.00,MIA,BRN,Accum Depr - Computer Equipment,193.07,CR
GLJLINE,1030.10.00,FLX,BRN,Computer Software,""1,680.00"",DR
GLJLINE,1030.10.00,MEM,BRN,Computer Software,""14,055.24"",CR
GLJLINE,1030.10.00,ADM,BRN,Computer Software Not in Use,""21,129.00"",DR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Computer Software,799.76,CR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Computer Software,787.33,CR
GLJLINE,1030.10.00,DEN,BRN,Accum Depr - Computer Software,123.61,CR
GLJLINE,1030.10.00,FLX,BRN,Accum Depr - Computer Software,294.86,CR
GLJLINE,1030.10.00,MCO,BRN,Accum Depr - Computer Software,81.08,CR
GLJLINE,1030.10.00,MEM,BRN,Accum Depr - Computer Software,""13,478.84"",DR
GLJLINE,1030.10.00,TLC,BRN,Accum Depr - Computer Software,388.22,CR
GLJLINE,1030.10.00,MCO,BRN,Leasehold Improvements,""1,183.00"",DR
GLJLINE,1030.10.00,MEM,BRN,Leasehold Improvements,""36,308.98"",CR
GLJLINE,1030.10.00,ADM,BRN,Accum Depr - Lshld Improvements,164.73,CR
GLJLINE,1030.10.00,CMH,BRN,Accum Depr - Lshld Improvements,166.22,CR
GLJLINE,1030.10.00,DEN,BRN,Accum Depr - Lshld Improvements,113.93,CR
GLJLINE,1030.10.00,MCO,BRN,Accum Depr - Lshld Improvements,""2,732.54"",CR
GLJLINE,1030.10.00,MEM,BRN,Accum Depr - Lshld Improvements,""19,695.83"",DR
GLJLINE,1030.10.00,ADM,BRN,Due from (to) TLS,""19,476.26"",DR
GLJLINE,1030.10.00,ADM,BRN,Due from (to) FLG Express,""91,513.68"",DR
GLJLINE,1030.10.00,FLX,BRN,Due from (to) FLG Express,""91,513.68"",CR
GLJLINE,1030.10.00,ADM,BRN,Due from (to) TLS Cartage,""20,510.48"",CR
GLJLINE,1030.10.00,TLC,BRN,Due from (to) TLS Cartage,""20,510.48"",DR
GLJLINE,1030.10.00,ADM,BRN,Due from (to) FLG Partners,81.98,DR
GLJLINE,1030.10.00,ADM,BRN,Due from Shareholder,""139,650.00"",DR
GLJLINE,1030.10.00,MEM,BRN,Security Deposits,""23,450.00"",CR
GLJLINE,1030.10.00,ADM,BRN,Note Receivable - DGL,""1,200,000.00"",DR
GLJLINE,1030.10.00,ADM,BRN,Current Surrender Value - Key Life,525.9,CR
GLJLINE,1030.10.00,MEM,BRN,Current Surrender Value - Key Life,""52,326.35"",CR
GLJLINE,1030.10.00,ADM,BRN,Goodwill,""1,000.00"",CR
GLJLINE,1030.10.00,ADM,BRN,AP Clearing Account,""293,706.10"",DR
GLJLINE,1030.10.00,ADM,BRN,AP Clearing Account,""1,152.34"",DR
GLJLINE,1030.10.00,CMH,BRN,AP Clearing Account,""36,555.66"",CR
GLJLINE,1030.10.00,DEN,BRN,AP Clearing Account,""207,029.13"",CR
GLJLINE,1030.10.00,MCO,BRN,AP Clearing Account,""70,123.83"",CR
GLJLINE,1030.10.00,MEM,BRN,AP Clearing Account,""397,871.88"",DR
GLJLINE,1030.10.00,MIA,BRN,AP Clearing Account,""34,408.38"",CR
GLJLINE,1030.10.00,TLC,BRN,AP Clearing Account,""2,103.04"",DR
GLJLINE,1030.10.00,FLX,BRN,AP Clearing Account,""28,157.85"",DR
GLJLINE,1030.10.00,TLC,BRN,AP Clearing Account,""4,752.19"",DR
GLJLINE,1030.10.00,ADM,BRN,AP Clearing Account,25,CR
GLJLINE,1030.10.00,DEN,BRN,AP Clearing Account,""2,856.91"",DR
GLJLINE,1030.10.00,MCO,BRN,AP Clearing Account,""10,327.93"",DR
GLJLINE,1030.10.00,ADM,BRN,Accounts Payable Adjustment,726.42,DR
GLJLINE,1030.10.00,MCO,BRN,Accounts Payable Adjustment,""1,960.36"",DR
GLJLINE,1030.10.00,MEM,BRN,Accounts Payable Adjustment,764.13,DR
GLJLINE,1030.10.00,TLC,BRN,Accounts Payable Adjustment,""2,234.49"",CR
GLJLINE,1030.10.00,ADM,BRN,Bank Overdraft,""26,818.28"",DR
GLJLINE,1030.10.00,FLX,BRN,Bank Overdraft,""15,261.32"",CR
GLJLINE,1030.10.00,TLC,BRN,Bank Overdraft,""1,884.75"",DR
GLJLINE,1030.10.00,FLX,BRN,Accrued Expenses - Operations,""48,313.32"",CR
GLJLINE,1030.10.00,CMH,BRN,Accrued Estimated Payables,58.42,DR
GLJLINE,1030.10.00,DEN,BRN,Accrued Estimated Payables,676.57,DR
GLJLINE,1030.10.00,MCO,BRN,Accrued Estimated Payables,""3,319.38"",CR
GLJLINE,1030.10.00,MEM,BRN,Accrued Estimated Payables,""4,125.46"",DR
GLJLINE,1030.10.00,MIA,BRN,Accrued Estimated Payables,""3,506.89"",CR
GLJLINE,1030.10.00,TLC,BRN,Accrued Estimated Payables,""4,836.24"",DR
GLJLINE,1030.10.00,MEM,BRN,Accrued Real Estate Taxes,573.75,DR
GLJLINE,1030.10.00,CMH,BRN,Accrued Personal Property Taxes,25,CR
GLJLINE,1030.10.00,DEN,BRN,Accrued Personal Property Taxes,100,CR
GLJLINE,1030.10.00,MCO,BRN,Accrued Personal Property Taxes,275,CR
GLJLINE,1030.10.00,MEM,BRN,Accrued Personal Property Taxes,""1,387.78"",DR
GLJLINE,1030.10.00,MIA,BRN,Accrued Personal Property Taxes,10,CR
GLJLINE,1030.10.00,CMH,BRN,Accrued Expenses - Other,""2,168.97"",DR
GLJLINE,1030.10.00,FLX,BRN,Accrued Expenses - Other,""1,250.00"",DR
GLJLINE,1030.10.00,MEM,BRN,Accrued Expenses - Other,""87,298.98"",DR
GLJLINE,1030.10.00,MCO,BRN,Accrued Sales and Use Tax,66.76,CR
GLJLINE,1030.10.00,MIA,BRN,Accrued Sales and Use Tax,68.18,DR
GLJLINE,1030.10.00,ADM,BRN,Accrued Payroll,""9,300.00"",DR
GLJLINE,1030.10.00,ADM,BRN,Accrued Payroll,""2,600.00"",DR
GLJLINE,1030.10.00,CMH,BRN,Accrued Payroll,""1,100.00"",DR
GLJLINE,1030.10.00,DEN,BRN,Accrued Payroll,""1,500.00"",CR
GLJLINE,1030.10.00,FLX,BRN,Accrued Payroll,300,DR
GLJLINE,1030.10.00,MCO,BRN,Accrued Payroll,""7,400.00"",DR
GLJLINE,1030.10.00,MEM,BRN,Accrued Payroll,""41,869.49"",DR
GLJLINE,1030.10.00,MIA,BRN,Accrued Payroll,800,DR
GLJLINE,1030.10.00,TLC,BRN,Accrued Payroll,""2,900.00"",DR
GLJLINE,1030.10.00,ADM,BRN,401(k) Plan Payable,512.63,DR
GLJLINE,1030.10.00,CMH,BRN,Payroll Taxes Payable,45.93,CR
GLJLINE,1030.10.00,ADM,BRN,Other Payroll Deductions Payable,""1,960.75"",DR
GLJLINE,1030.10.00,FLX,BRN,Management Fee Payable,""3,744.00"",DR
GLJLINE,1030.10.00,ADM,BRN,Line of Credit - FL Capital Bank,""944,817.49"",DR
GLJLINE,1030.10.00,FLX,BRN,C/L - Transplus Software,197.97,DR
GLJLINE,1030.10.00,MCO,BRN,Other Liabilities,443.75,CR
GLJLINE,1030.10.00,MEM,BRN,Other Liabilities,443.75,DR
GLJLINE,1030.10.00,MEM,BRN,Other Liabilities,""61,103.03"",DR
GLJLINE,1030.10.00,MIA,BRN,Note Payable - First National Bank,622.81,DR
GLJLINE,1030.10.00,TLC,BRN,Note Payable - Hitachi Capital,""1,037.94"",DR
GLJLINE,1030.10.00,MEM,BRN,Other Liabilities,""8,516.79"",DR
GLJLINE,1030.10.00,ADM,BRN,Equipment Line of Credit - FL Capit,""3,756.05"",DR
GLJLINE,1030.10.00,MEM,BRN,DUE TO STERLING POWERS - MEM SALE,""40,000.00"",CR
GLJLINE,1030.10.00,ADM,BRN,Current Year Distributions,""500,000.00"",DR
GLJLINE,1030.10.00,CMH,EXP,Air Freight Revenue Actual,""3,694.10"",CR
GLJLINE,1030.10.00,DEN,EXP,Air Freight Revenue Actual,""24,197.82"",CR
GLJLINE,1030.10.00,MCO,EXP,Air Freight Revenue Actual,""82,817.73"",CR
GLJLINE,1030.10.00,MIA,EXP,Air Freight Revenue Actual,""85,098.34"",CR
GLJLINE,1030.10.00,CMH,EXP,Ocean Freight Revenue Actual,""15,258.00"",CR
GLJLINE,1030.10.00,DEN,EXP,Ocean Freight Revenue Actual,""4,291.51"",CR
GLJLINE,1030.10.00,MCO,EXP,Ocean Freight Revenue Actual,""14,730.00"",CR
GLJLINE,1030.10.00,MEM,EXP,Ocean Freight Revenue Actual,218.62,DR
GLJLINE,1030.10.00,MIA,EXP,Ocean Freight Revenue Actual,811.83,CR
GLJLINE,1030.10.00,CMH,EXP,Handling Charges Revenue Actual,""1,740.31"",CR
GLJLINE,1030.10.00,DEN,EXP,Handling Charges Revenue Actual,""4,031.30"",CR
GLJLINE,1030.10.00,MCO,EXP,Handling Charges Revenue Actual,""30,899.22"",CR
GLJLINE,1030.10.00,MEM,EXP,Handling Charges Revenue Actual,218.62,CR
GLJLINE,1030.10.00,MIA,EXP,Handling Charges Revenue Actual,""20,933.33"",CR
GLJLINE,1030.10.00,CMH,EXP,Ground Trans Revenue Actual,""5,061.30"",CR
GLJLINE,1030.10.00,DEN,EXP,Ground Trans Revenue Actual,""7,242.93"",CR
GLJLINE,1030.10.00,MCO,EXP,Ground Trans Revenue Actual,""20,349.42"",CR
GLJLINE,1030.10.00,MIA,EXP,Ground Trans Revenue Actual,""11,529.85"",CR
GLJLINE,1030.10.00,DEN,IMP,Air Freight Revenue Actual,""70,518.85"",CR
GLJLINE,1030.10.00,MCO,IMP,Air Freight Revenue Actual,""49,243.48"",CR
GLJLINE,1030.10.00,MIA,IMP,Air Freight Revenue Actual,""27,335.42"",CR
GLJLINE,1030.10.00,DEN,IMP,Ocean Freight Revenue Actual,""85,262.25"",CR
GLJLINE,1030.10.00,MCO,IMP,Ocean Freight Revenue Actual,""93,819.39"",CR
GLJLINE,1030.10.00,MIA,IMP,Ocean Freight Revenue Actual,""3,122.42"",CR
GLJLINE,1030.10.00,DEN,IMP,Handling Charges Revenue Actual,27.57,CR
GLJLINE,1030.10.00,MCO,IMP,Handling Charges Revenue Actual,542.26,CR
GLJLINE,1030.10.00,CMH,IMP,Handling Charges Revenue Actual,416.16,CR
GLJLINE,1030.10.00,DEN,IMP,Handling Charges Revenue Actual,""10,067.24"",CR
GLJLINE,1030.10.00,MCO,IMP,Handling Charges Revenue Actual,""12,595.50"",CR
GLJLINE,1030.10.00,MIA,IMP,Handling Charges Revenue Actual,""7,270.86"",CR
GLJLINE,1030.10.00,DEN,IMP,Customs Brokerage Rev Actual,""27,068.69"",CR
GLJLINE,1030.10.00,MCO,IMP,Customs Brokerage Rev Actual,""29,429.51"",CR
GLJLINE,1030.10.00,MIA,IMP,Customs Brokerage Rev Actual,""2,580.94"",CR
GLJLINE,1030.10.00,DEN,IMP,Ground Trans Revenue Actual,""14,143.30"",CR
GLJLINE,1030.10.00,MCO,IMP,Ground Trans Revenue Actual,""20,758.00"",CR
GLJLINE,1030.10.00,MIA,IMP,Ground Trans Revenue Actual,""6,168.36"",CR
GLJLINE,1030.10.00,ADM,DOM,Ground Trans Revenue Actual,""14,864.92"",DR
GLJLINE,1030.10.00,TLC,DOM,Ground Trans Revenue Actual,""34,356.44"",CR
GLJLINE,1030.10.00,ADM,DOM,FLG EXPRESS REVENUE,""4,826.74"",DR
GLJLINE,1030.10.00,FLX,DOM,FLG EXPRESS REVENUE,""229,693.59"",CR
GLJLINE,1030.10.00,CMH,DOM,Air Freight Revenue Actual,""51,439.50"",CR
GLJLINE,1030.10.00,DEN,DOM,Air Freight Revenue Actual,""166,434.07"",CR
GLJLINE,1030.10.00,MCO,DOM,Air Freight Revenue Actual,""143,433.36"",CR
GLJLINE,1030.10.00,MIA,DOM,Air Freight Revenue Actual,""1,505.27"",CR
GLJLINE,1030.10.00,CMH,DOM,Handling Charges Revenue Actual,205,CR
GLJLINE,1030.10.00,DEN,DOM,Handling Charges Revenue Actual,""14,338.77"",CR
GLJLINE,1030.10.00,MCO,DOM,Handling Charges Revenue Actual,""1,518.81"",CR
GLJLINE,1030.10.00,MIA,DOM,Handling Charges Revenue Actual,""1,773.94"",CR
GLJLINE,1030.10.00,CMH,DOM,Ground Trans Revenue Actual,""42,514.24"",CR
GLJLINE,1030.10.00,DEN,DOM,Ground Trans Revenue Actual,680,CR
GLJLINE,1030.10.00,MCO,DOM,Ground Trans Revenue Actual,""16,578.11"",CR
GLJLINE,1030.10.00,MIA,DOM,Ground Trans Revenue Actual,""2,189.87"",CR
GLJLINE,1030.10.00,DEN,WHS,Warehouse Storage Revenue Actual,""7,200.00"",CR
GLJLINE,1030.10.00,MCO,WHS,Warehouse Storage Revenue Actual,""113,962.02"",CR
GLJLINE,1030.10.00,MIA,WHS,Warehouse Storage Revenue Actual,5,CR
GLJLINE,1030.10.00,DEN,WHS,Warehouse Services Revenue Actual,""3,718.00"",CR
GLJLINE,1030.10.00,MCO,WHS,Warehouse Services Revenue Actual,""7,745.54"",CR
GLJLINE,1030.10.00,CMH,FUL,Fulfillment Services Revenue Actual,""14,688.00"",CR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Services Revenue Actual,""44,261.70"",CR
GLJLINE,1030.10.00,CMH,BRN,Currency (Gain) Loss,21.68,DR
GLJLINE,1030.10.00,DEN,BRN,Currency (Gain) Loss,47.8,DR
GLJLINE,1030.10.00,MCO,BRN,Currency (Gain) Loss,154.27,CR
GLJLINE,1030.10.00,CMH,EXP,Air Freight Expense Actual,""1,339.83"",DR
GLJLINE,1030.10.00,DEN,EXP,Air Freight Expense Actual,""14,154.80"",DR
GLJLINE,1030.10.00,MCO,EXP,Air Freight Expense Actual,""54,592.53"",DR
GLJLINE,1030.10.00,MIA,EXP,Air Freight Expense Actual,""31,443.46"",DR
GLJLINE,1030.10.00,CMH,EXP,Ocean Freight Expense Actual,""11,945.94"",DR
GLJLINE,1030.10.00,DEN,EXP,Ocean Freight Expense Actual,""4,947.64"",DR
GLJLINE,1030.10.00,MCO,EXP,Ocean Freight Expense Actual,""11,586.00"",DR
GLJLINE,1030.10.00,MIA,EXP,Ocean Freight Expense Actual,576.48,DR
GLJLINE,1030.10.00,CMH,EXP,Handling Charge Expense Actual,""1,576.59"",DR
GLJLINE,1030.10.00,DEN,EXP,Handling Charge Expense Actual,""3,743.40"",DR
GLJLINE,1030.10.00,MCO,EXP,Handling Charge Expense Actual,""19,601.03"",DR
GLJLINE,1030.10.00,MIA,EXP,Handling Charge Expense Actual,""11,120.89"",DR
GLJLINE,1030.10.00,CMH,EXP,Ground Trans Expense Actual,""2,637.04"",DR
GLJLINE,1030.10.00,DEN,EXP,Ground Trans Expense Actual,""5,007.14"",DR
GLJLINE,1030.10.00,MCO,EXP,Ground Trans Expense Actual,""11,221.22"",DR
GLJLINE,1030.10.00,MIA,EXP,Ground Trans Expense Actual,""9,572.17"",DR
GLJLINE,1030.10.00,DEN,IMP,Air Freight Expense Actual,""71,550.88"",DR
GLJLINE,1030.10.00,MCO,IMP,Air Freight Expense Actual,""35,299.43"",DR
GLJLINE,1030.10.00,MIA,IMP,Air Freight Expense Actual,""25,069.18"",DR
GLJLINE,1030.10.00,DEN,IMP,Ocean Freight Expense Actual,""79,559.83"",DR
GLJLINE,1030.10.00,MCO,IMP,Ocean Freight Expense Actual,""84,058.61"",DR
GLJLINE,1030.10.00,MIA,IMP,Ocean Freight Expense Actual,""2,432.42"",DR
GLJLINE,1030.10.00,CMH,IMP,Handling Charge Expense Actual,10.58,DR
GLJLINE,1030.10.00,DEN,IMP,Handling Charge Expense Actual,""8,874.11"",DR
GLJLINE,1030.10.00,MCO,IMP,Handling Charge Expense Actual,""4,187.91"",DR
GLJLINE,1030.10.00,MIA,IMP,Handling Charge Expense Actual,""1,978.66"",DR
GLJLINE,1030.10.00,DEN,IMP,Customs Brokerage Exp Actual,""11,143.89"",DR
GLJLINE,1030.10.00,MCO,IMP,Customs Brokerage Exp Actual,""11,598.32"",DR
GLJLINE,1030.10.00,MIA,IMP,Customs Brokerage Exp Actual,""5,054.72"",DR
GLJLINE,1030.10.00,CMH,IMP,Customs Brokerage Exp Actual,35.25,DR
GLJLINE,1030.10.00,DEN,IMP,Customs Brokerage Exp Actual,""13,131.25"",DR
GLJLINE,1030.10.00,MCO,IMP,Customs Brokerage Exp Actual,""17,516.25"",DR
GLJLINE,1030.10.00,MIA,IMP,Customs Brokerage Exp Actual,""2,414.54"",DR
GLJLINE,1030.10.00,TLC,DOM,Ground Trans Expense Actual,""11,618.16"",DR
GLJLINE,1030.10.00,TLC,DOM,Truck Fuel,""2,812.73"",DR
GLJLINE,1030.10.00,TLC,DOM,Truck Leasing,""1,767.87"",DR
GLJLINE,1030.10.00,TLC,DOM,Truck Repairs and Maintenance,940.56,DR
GLJLINE,1030.10.00,TLC,DOM,Truck Tolls,250,DR
GLJLINE,1030.10.00,ADM,DOM,FLG Express Expenses,""4,826.74"",CR
GLJLINE,1030.10.00,FLX,DOM,FLG Express Expenses,""190,246.90"",DR
GLJLINE,1030.10.00,CMH,DOM,Air Freight Expense Actual,""23,316.78"",DR
GLJLINE,1030.10.00,DEN,DOM,Air Freight Expense Actual,""28,918.89"",DR
GLJLINE,1030.10.00,MCO,DOM,Air Freight Expense Actual,""106,306.86"",DR
GLJLINE,1030.10.00,MIA,DOM,Air Freight Expense Actual,921.56,DR
GLJLINE,1030.10.00,CMH,DOM,Handling Charge Expense Actual,392.23,DR
GLJLINE,1030.10.00,DEN,DOM,Handling Charge Expense Actual,199.14,DR
GLJLINE,1030.10.00,MCO,DOM,Handling Charge Expense Actual,""1,336.72"",DR
GLJLINE,1030.10.00,MIA,DOM,Handling Charge Expense Actual,""2,500.63"",DR
GLJLINE,1030.10.00,ADM,DOM,Ground Trans Expense Actual,""14,864.92"",CR
GLJLINE,1030.10.00,CMH,DOM,Ground Trans Expense Actual,""42,369.26"",DR
GLJLINE,1030.10.00,DEN,DOM,Ground Trans Expense Actual,""100,267.29"",DR
GLJLINE,1030.10.00,MCO,DOM,Ground Trans Expense Actual,""24,114.88"",DR
GLJLINE,1030.10.00,MIA,DOM,Ground Trans Expense Actual,""1,140.25"",DR
GLJLINE,1030.10.00,DEN,WHS,Warehouse Services Expense Actual,""1,483.68"",DR
GLJLINE,1030.10.00,MCO,WHS,Warehouse Services Expense Actual,""7,567.61"",DR
GLJLINE,1030.10.00,CMH,WHS,Warehouse Services Expense Actual,288.23,DR
GLJLINE,1030.10.00,MCO,WHS,Warehouse Services Expense Actual,""2,192.09"",DR
GLJLINE,1030.10.00,CMH,WHS,Warehouse Supplies Expense Actual,""2,286.40"",DR
GLJLINE,1030.10.00,MCO,WHS,Warehouse Supplies Expense Actual,""8,351.86"",DR
GLJLINE,1030.10.00,MCO,WHS,Forklift Fuel,274.65,DR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Payroll,""19,062.48"",DR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Payroll Taxes,""2,237.68"",DR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Workers' Comp,""1,391.80"",DR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Payroll,""4,914.67"",DR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Payroll Taxes,434.43,DR
GLJLINE,1030.10.00,MCO,FUL,Fulfillment Workers' Comp,348.93,DR
GLJLINE,1030.10.00,DEN,DOM,Courier Services,784.37,DR
GLJLINE,1030.10.00,MCO,DOM,Courier Services,435.49,DR
GLJLINE,1030.10.00,MIA,DOM,Courier Services,74.12,DR
GLJLINE,1030.10.00,MCO,BRN,Marketing and Advertising,572,DR
GLJLINE,1030.10.00,MIA,BRN,Marketing and Advertising,139.25,DR
GLJLINE,1030.10.00,TLC,BRN,Marketing and Advertising,141,DR
GLJLINE,1030.10.00,ADM,BRN,Meals and Entertainment,""3,510.83"",DR
GLJLINE,1030.10.00,ADM,BRN,Meals and Entertainment,92.07,DR
GLJLINE,1030.10.00,DEN,BRN,Meals and Entertainment,""1,780.20"",DR
GLJLINE,1030.10.00,FLX,BRN,Meals and Entertainment,681.76,DR
GLJLINE,1030.10.00,MCO,BRN,Meals and Entertainment,""1,768.84"",DR
GLJLINE,1030.10.00,MIA,BRN,Meals and Entertainment,""1,563.20"",DR
GLJLINE,1030.10.00,ADM,BRN,Travel,""1,590.83"",DR
GLJLINE,1030.10.00,DEN,BRN,Travel,""1,308.70"",DR
GLJLINE,1030.10.00,FLX,BRN,Travel,825.59,DR
GLJLINE,1030.10.00,MCO,BRN,Travel,""3,886.61"",DR
GLJLINE,1030.10.00,MIA,BRN,Travel,785.28,DR
GLJLINE,1030.10.00,CMH,BRN,Sales Commissions - Ind Contractors,""14,879.77"",DR
GLJLINE,1030.10.00,DEN,BRN,Sales Commissions - Ind Contractors,""1,302.27"",DR
GLJLINE,1030.10.00,FLX,BRN,Sales Commissions - Ind Contractors,855.93,DR
GLJLINE,1030.10.00,MCO,BRN,Sales Commissions - Ind Contractors,""3,842.26"",DR
GLJLINE,1030.10.00,ADM,BRN,Credit Card Processing,4.5,DR
GLJLINE,1030.10.00,FLX,BRN,Credit Card Processing,35.35,DR
GLJLINE,1030.10.00,MCO,BRN,Credit Card Processing,846.74,DR
GLJLINE,1030.10.00,MIA,BRN,Credit Card Processing,109.98,DR
GLJLINE,1030.10.00,ADM,BRN,Vehicle Expenses,242.68,DR
GLJLINE,1030.10.00,DEN,BRN,Vehicle Expenses,9.31,DR
GLJLINE,1030.10.00,MCO,BRN,Vehicle Expenses,""1,354.63"",DR
GLJLINE,1030.10.00,MIA,BRN,Vehicle Expenses,209.2,DR
GLJLINE,1030.10.00,DEN,BRN,Auto Allowance,""1,400.00"",DR
GLJLINE,1030.10.00,FLC,BRN,Auto Allowance,""1,500.00"",DR
GLJLINE,1030.10.00,MCO,BRN,Auto Allowance,900,DR
GLJLINE,1030.10.00,DEN,BRN,Warehouse Supplies,89.29,DR
GLJLINE,1030.10.00,MCO,BRN,Warehouse Supplies,""2,733.38"",DR
GLJLINE,1030.10.00,MIA,BRN,Warehouse Supplies,111.71,DR
GLJLINE,1030.10.00,MCO,BRN,Warehouse Equipment Repairs,""3,774.91"",DR
GLJLINE,1030.10.00,DEN,BRN,Warehouse Equipment Rental,398.82,DR
GLJLINE,1030.10.00,MCO,BRN,Warehouse Equipment Rental,948.25,DR
GLJLINE,1030.10.00,DEN,BRN,Manual Checks,""5,317.56"",CR
GLJLINE,1030.10.00,FLX,BRN,Manual Checks,807,DR
GLJLINE,1030.10.00,MCO,BRN,Manual Checks,""1,682.92"",CR
GLJLINE,1030.10.00,CMH,BRN,Temporary Help,585.58,DR
GLJLINE,1030.10.00,MIA,BRN,Temporary Help,150,DR
GLJLINE,1030.10.00,ADM,BRN,Salaries and Wages,""51,179.08"",DR
GLJLINE,1030.10.00,ADM,BRN,Salaries and Wages,""3,464.02"",DR
GLJLINE,1030.10.00,CMH,BRN,Salaries and Wages,""7,507.77"",DR
GLJLINE,1030.10.00,DEN,BRN,Salaries and Wages,""30,631.04"",DR
GLJLINE,1030.10.00,FLC,BRN,Salaries and Wages,""21,346.10"",DR
GLJLINE,1030.10.00,FLX,BRN,Salaries and Wages,""16,327.06"",DR
GLJLINE,1030.10.00,MCO,BRN,Salaries and Wages,""66,423.52"",DR
GLJLINE,1030.10.00,MIA,BRN,Salaries and Wages,""21,363.19"",DR
GLJLINE,1030.10.00,TLC,BRN,Salaries and Wages,""6,940.60"",DR
GLJLINE,1030.10.00,DEN,BRN,Sales Commissions - Employees,""12,587.15"",DR
GLJLINE,1030.10.00,FLX,BRN,Sales Commissions - Employees,474.38,DR
GLJLINE,1030.10.00,MCO,BRN,Sales Commissions - Employees,""2,995.85"",DR
GLJLINE,1030.10.00,ADM,BRN,Salaries and Wages,830.76,DR
GLJLINE,1030.10.00,ADM,BRN,Salaries and Wages,""1,119.90"",DR
GLJLINE,1030.10.00,DEN,BRN,Salaries and Wages,""1,942.32"",DR
GLJLINE,1030.10.00,FLC,BRN,Salaries and Wages,""1,730.76"",DR
GLJLINE,1030.10.00,FLX,BRN,Salaries and Wages,780,DR
GLJLINE,1030.10.00,MCO,BRN,Salaries and Wages,""2,694.66"",DR
GLJLINE,1030.10.00,MIA,BRN,Salaries and Wages,173.08,DR
GLJLINE,1030.10.00,TLC,BRN,Salaries and Wages,123.08,DR
GLJLINE,1030.10.00,ADM,BRN,Salaries and Wages,392.4,DR
GLJLINE,1030.10.00,DEN,BRN,Salaries and Wages,173.08,DR
GLJLINE,1030.10.00,MCO,BRN,Salaries and Wages,""1,954.12"",DR
GLJLINE,1030.10.00,ADM,BRN,Payroll Taxes,""3,659.23"",DR
GLJLINE,1030.10.00,ADM,BRN,Payroll Taxes,690.94,DR
GLJLINE,1030.10.00,CMH,BRN,Payroll Taxes,605.18,DR
GLJLINE,1030.10.00,DEN,BRN,Payroll Taxes,""3,450.92"",DR
GLJLINE,1030.10.00,FLC,BRN,Payroll Taxes,""1,864.92"",DR
GLJLINE,1030.10.00,FLX,BRN,Payroll Taxes,""1,513.98"",DR
GLJLINE,1030.10.00,MCO,BRN,Payroll Taxes,""5,961.68"",DR
GLJLINE,1030.10.00,MIA,BRN,Payroll Taxes,""2,162.91"",DR
GLJLINE,1030.10.00,TLC,BRN,Payroll Taxes,758.93,DR
GLJLINE,1030.10.00,DEN,BRN,Incentive Bonuses,654.93,DR
GLJLINE,1030.10.00,MIA,BRN,Incentive Bonuses,832.13,DR
GLJLINE,1030.10.00,TLC,BRN,Incentive Bonuses,284.94,DR
GLJLINE,1030.10.00,ADM,BRN,Workers' Comp Insurance,""1,130.43"",DR
GLJLINE,1030.10.00,ADM,BRN,Workers' Comp Insurance,63.21,DR
GLJLINE,1030.10.00,CMH,BRN,Workers' Comp Insurance,45.93,DR
GLJLINE,1030.10.00,DEN,BRN,Workers' Comp Insurance,420.56,DR
GLJLINE,1030.10.00,FLX,BRN,Workers' Comp Insurance,50.57,DR
GLJLINE,1030.10.00,MCO,BRN,Workers' Comp Insurance,""2,906.83"",DR
GLJLINE,1030.10.00,MIA,BRN,Workers' Comp Insurance,291.71,DR
GLJLINE,1030.10.00,TLC,BRN,Workers' Comp Insurance,""1,062.97"",DR
GLJLINE,1030.10.00,ADM,BRN,Employee Benefits,""3,197.91"",DR
GLJLINE,1030.10.00,ADM,BRN,Employee Benefits,295.52,DR
GLJLINE,1030.10.00,CMH,BRN,Employee Benefits,""1,330.03"",DR
GLJLINE,1030.10.00,DEN,BRN,Employee Benefits,""5,861.50"",DR
GLJLINE,1030.10.00,FLX,BRN,Employee Benefits,""2,402.25"",DR
GLJLINE,1030.10.00,MCO,BRN,Employee Benefits,""13,077.46"",DR
GLJLINE,1030.10.00,MIA,BRN,Employee Benefits,""1,363.11"",DR
GLJLINE,1030.10.00,TLC,BRN,Employee Benefits,""1,171.11"",DR
GLJLINE,1030.10.00,ADM,BRN,Employee Benefits,963.06,DR
GLJLINE,1030.10.00,ADM,BRN,Employee Benefits,93.59,DR
GLJLINE,1030.10.00,DEN,BRN,Employee Benefits,389.36,DR
GLJLINE,1030.10.00,FLX,BRN,Employee Benefits,22.21,DR
GLJLINE,1030.10.00,MCO,BRN,Employee Benefits,""2,878.46"",DR
GLJLINE,1030.10.00,TLC,BRN,Employee Benefits,186.87,DR
GLJLINE,1030.10.00,ADM,BRN,Employee Benefits,729.68,CR
GLJLINE,1030.10.00,ADM,BRN,Employee Benefits,40.28,CR
GLJLINE,1030.10.00,CMH,BRN,Employee Benefits,396.94,CR
GLJLINE,1030.10.00,DEN,BRN,Employee Benefits,794,CR
GLJLINE,1030.10.00,FLC,BRN,Employee Benefits,198.64,CR
GLJLINE,1030.10.00,FLX,BRN,Employee Benefits,765.3,CR
GLJLINE,1030.10.00,MCO,BRN,Employee Benefits,""1,714.74"",CR
GLJLINE,1030.10.00,MIA,BRN,Employee Benefits,312.12,CR
GLJLINE,1030.10.00,TLC,BRN,Employee Benefits,433.22,CR
GLJLINE,1030.10.00,ADM,BRN,Employer 401(k) Contributions,""1,966.39"",DR
GLJLINE,1030.10.00,ADM,BRN,Employer 401(k) Contributions,67.19,DR
GLJLINE,1030.10.00,FLX,BRN,Employer 401(k) Contributions,374.14,DR
GLJLINE,1030.10.00,MCO,BRN,Employer 401(k) Contributions,""1,903.46"",DR
GLJLINE,1030.10.00,MIA,BRN,Employer 401(k) Contributions,528.79,DR
GLJLINE,1030.10.00,TLC,BRN,Employer 401(k) Contributions,204.19,DR
GLJLINE,1030.10.00,ADM,BRN,Contract Services,937.5,DR
GLJLINE,1030.10.00,FLX,BRN,Contract Services,""5,133.32"",DR
GLJLINE,1030.10.00,MCO,BRN,Contract Services,""10,241.48"",DR
GLJLINE,1030.10.00,ADM,BRN,Payroll Exp from FLGC,""9,060.61"",DR
GLJLINE,1030.10.00,CMH,BRN,Payroll Exp from FLGC,""3,020.21"",DR
GLJLINE,1030.10.00,DEN,BRN,Payroll Exp from FLGC,""12,080.82"",DR
GLJLINE,1030.10.00,FLC,BRN,Payroll Exp from FLGC,""26,561.64"",CR
GLJLINE,1030.10.00,DEN,BRN,Inter-Branch Allocations,""4,410.00"",DR
GLJLINE,1030.10.00,MCO,BRN,Inter-Branch Allocations,""4,410.00"",CR
GLJLINE,1030.10.00,ADM,BRN,Bldg Repairs and Maintenance,513.74,DR");
			importer.ImportData(reader, "", new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			GLJournal lastImportedJournal = (GLJournal)importer.GetType().GetProperty("LastImportedJournal").GetValue(importer, null);
			GLGeneralOnlineAggregator aggregator = new GLGeneralOnlineAggregator(lastImportedJournal, lastImportedJournal);
			aggregator.Aggregate();
		}

		protected DbCommand GetGLAggregateDbCommandForBizO()
		{
			return GetGLAggregateDbCommandForBizO(1.0m);
		}

		protected DbCommand GetGLAggregateDbCommandForBizO(decimal reverseFactor)
		{
			string mergedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount0, @PostPeriod0, @GLAccountPK0, @BranchPK0, @CompanyPK0, @DepartmentPK0, @TransactionCategory0) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1, @PostPeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount2, @PostPeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; ";

			DbCommand command = Db.Connection.Command(mergedSQL);

			command.AddParameter("@GLAccountPK0", SqlDbType.UniqueIdentifier, GLAccountPK0);
			command.AddParameter("@GLAccountPK1", SqlDbType.UniqueIdentifier, GLAccountPK1);
			command.AddParameter("@GLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);

			command.AddParameter("@BranchPK0", SqlDbType.UniqueIdentifier, BranchPK0);
			command.AddParameter("@BranchPK1", SqlDbType.UniqueIdentifier, BranchPK1);
			command.AddParameter("@BranchPK2", SqlDbType.UniqueIdentifier, BranchPK2);

			command.AddParameter("@CompanyPK0", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);

			command.AddParameter("@DepartmentPK0", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK1", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);

			command.AddParameter("@PostPeriod0", SqlDbType.Int, Period0);
			command.AddParameter("@PostPeriod1", SqlDbType.Int, Period0);
			command.AddParameter("@PostPeriod2", SqlDbType.Int, Period0);

			command.AddParameter("@LineAmount0", SqlDbType.Money, LineAmount0 * reverseFactor);
			command.AddParameter("@LineAmount1", SqlDbType.Money, LineAmount1 * reverseFactor);
			command.AddParameter("@LineAmount2", SqlDbType.Money, LineAmount2 * reverseFactor);

			command.AddParameter("@TransactionCategory0", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);
			command.AddParameter("@TransactionCategory1", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);
			command.AddParameter("@TransactionCategory2", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);

			return command;
		}
	}
}
