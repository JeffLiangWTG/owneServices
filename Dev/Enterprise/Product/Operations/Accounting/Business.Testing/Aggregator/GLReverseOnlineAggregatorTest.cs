using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class GLReverseOnlineAggregatorTest : TestCaseWithFactory
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

			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch newBranch = currentCompany.Branches.AddNew();
			newBranch.GB_Code = "ZZZ";
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			ZQuery query = new ZQuery();
			query.MaximumRows = 4;

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
			GL.AH_TransactionType = TransactionTypes.GLReversingJournal;
			GL.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			GL.PostPeriod = Period0;
			GL.AgePeriod = ReversePeriod0;

			GL.GLJournalLines.AddNew();
			GL.GLJournalLines.AddNew();
			GL.GLJournalLines.AddNew();

			OriginalGL.GLJournalLines.AddNew();
			OriginalGL.GLJournalLines.AddNew();
			OriginalGL.GLJournalLines.AddNew();

			GL.GLJournalLines[0].AL_PostDate = OriginalGL.GLJournalLines[0].AL_PostDate = periodCalc.GetLastDayForPeriod(Period0).ToDateTime();
			GL.GLJournalLines[0].AL_OSExTaxAmount = OriginalGL.GLJournalLines[0].AL_OSExTaxAmount = LineAmount0;
			//GL.GLJournalLines[0].AL_LineType = "RJL";
			GL.GLJournalLines[0].AL_AG = OriginalGL.GLJournalLines[0].AL_AG = GLAccountPK0;
			GL.GLJournalLines[0].AL_GB = OriginalGL.GLJournalLines[0].AL_GB = BranchPK0;
			GL.GLJournalLines[0].AL_AC = OriginalGL.GLJournalLines[0].AL_AC = charge.PK;
			GL.GLJournalLines[0].AL_GE = OriginalGL.GLJournalLines[0].AL_GE = GlbDepartment.CurrentDepartment.PK;
			GL.GLJournalLines[0].AL_ReverseDate = OriginalGL.GLJournalLines[0].AL_ReverseDate = periodCalc.GetLastDayForPeriod(ReversePeriod0).ToDateTime();

			GL.GLJournalLines[1].AL_PostDate = OriginalGL.GLJournalLines[1].AL_PostDate = periodCalc.GetLastDayForPeriod(Period0).ToDateTime();
			GL.GLJournalLines[1].AL_OSExTaxAmount = OriginalGL.GLJournalLines[1].AL_OSExTaxAmount = LineAmount1;
			//GL.GLJournalLines[1].AL_LineType = "RJL";
			GL.GLJournalLines[1].AL_AG = OriginalGL.GLJournalLines[1].AL_AG = GLAccountPK1;
			GL.GLJournalLines[1].AL_GB = OriginalGL.GLJournalLines[1].AL_GB = BranchPK1;
			GL.GLJournalLines[1].AL_AC = OriginalGL.GLJournalLines[1].AL_AC = charge.PK;
			GL.GLJournalLines[1].AL_GE = OriginalGL.GLJournalLines[1].AL_GE = GlbDepartment.CurrentDepartment.PK;
			GL.GLJournalLines[1].AL_ReverseDate = OriginalGL.GLJournalLines[1].AL_ReverseDate = periodCalc.GetLastDayForPeriod(ReversePeriod0).ToDateTime();

			GL.GLJournalLines[2].AL_PostDate = OriginalGL.GLJournalLines[2].AL_PostDate = periodCalc.GetLastDayForPeriod(Period0).ToDateTime();
			GL.GLJournalLines[2].AL_OSExTaxAmount = OriginalGL.GLJournalLines[2].AL_OSExTaxAmount = LineAmount2;
			//GL.GLJournalLines[2].AL_LineType = "RJL";
			GL.GLJournalLines[2].AL_AG = OriginalGL.GLJournalLines[2].AL_AG = GLAccountPK2;
			GL.GLJournalLines[2].AL_GB = OriginalGL.GLJournalLines[2].AL_GB = BranchPK2;
			GL.GLJournalLines[2].AL_AC = OriginalGL.GLJournalLines[2].AL_AC = charge.PK;
			GL.GLJournalLines[2].AL_GE = OriginalGL.GLJournalLines[2].AL_GE = GlbDepartment.CurrentDepartment.PK;
			GL.GLJournalLines[2].AL_ReverseDate = OriginalGL.GLJournalLines[2].AL_ReverseDate = periodCalc.GetLastDayForPeriod(ReversePeriod0).ToDateTime();
		}

		public void TestNewGeneralJournalWithBizO()
		{
			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateDbCommand(1.0m);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestGenerateNewStatementWithBizO()
		{
			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateNewCommandsWithBizO(dbCommandFactory, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateDbCommand(1.0m);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestGenerateReverseStatementWithBizO()
		{
			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateReverseCommandsWithBizO(dbCommandFactory, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateDbCommand(-1.0m);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestDeletedGeneralJournalWithBizO()
		{
			TestFactory.Save();

			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;
			//GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[1].Delete();

			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1, @PostPeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount1, @ReversePeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK1", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@BranchPK1", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK1", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod1", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@ReversePeriod1", SqlDbType.Int, ReversePeriod0);
			expectedCommand.AddParameter("@LineAmount1", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@ReverseLineAmount1", SqlDbType.Money, LineAmount1);
			expectedCommand.AddParameterBasedOnDbColumn("@TransactionCategory1", InvoiceTypesList.Codes.FinalInvoice, AccTransactionHeaderSchema.AH_TransactionCategory);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestAmountUpdatedGeneralJournalWithBizO()
		{
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			TestFactory.Save();
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[1].AL_RX_NKTransactionCurrency = "USD";
			GL.GLJournalLines[1].AL_ExchangeRate = 0.90M;
			GL.GLJournalLines[1].AL_LocalExTaxAmount = NewLineAmount1;

			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1, @PostPeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount1, @ReversePeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK1", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@BranchPK1", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK1", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod1", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@LineAmount1", SqlDbType.Money, NewLineAmount1 - LineAmount1);
			expectedCommand.AddParameter("@ReversePeriod1", SqlDbType.Int, ReversePeriod0);
			expectedCommand.AddParameter("@ReverseLineAmount1", SqlDbType.Money, -(NewLineAmount1 - LineAmount1));
			expectedCommand.AddParameter("@TransactionCategory1", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

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

			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount2, @PostPeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount2, @ReversePeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount2, @NewPostPeriod2, @NewGLAccountPK2, @NewBranchPK2, @NewCompanyPK2, @NewDepartmentPK2, @NewTransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewReverseLineAmount2, @NewReversePeriod2, @NewGLAccountPK2, @NewBranchPK2, @NewCompanyPK2, @NewDepartmentPK2, @NewTransactionCategory2) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);
			expectedCommand.AddParameter("@BranchPK2", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@CompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod2", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@LineAmount2", SqlDbType.Money, -LineAmount2);
			expectedCommand.AddParameter("@TransactionCategory2", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@ReversePeriod2", SqlDbType.Int, ReversePeriod0);
			expectedCommand.AddParameter("@ReverseLineAmount2", SqlDbType.Money, LineAmount2);

			expectedCommand.AddParameter("@NewGLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);
			expectedCommand.AddParameter("@NewBranchPK2", SqlDbType.UniqueIdentifier, NewBranchPK2);
			expectedCommand.AddParameter("@NewCompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod2", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@NewLineAmount2", SqlDbType.Money, LineAmount2);
			expectedCommand.AddParameter("@NewTransactionCategory2", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewReversePeriod2", SqlDbType.Int, ReversePeriod0);
			expectedCommand.AddParameter("@NewReverseLineAmount2", SqlDbType.Money, -LineAmount2);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestPeriodChangedUpdatedGeneralJournalWithBizO()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupSinglePeriod(200309, new ZDateTime(2003, 9, 1), new ZDateTime(2003, 9, 30));
			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);

			//Re-load Original GL
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			TestFactory.Save();
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[2].AL_ReverseDate = periodCalc.GetFirstDayForPeriod(200309).ToDateTime().AddDays(1);

			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount2, @PostPeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount2, @ReversePeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount2, @NewPostPeriod2, @NewGLAccountPK2, @NewBranchPK2, @NewCompanyPK2, @NewDepartmentPK2, @NewTransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewReverseLineAmount2, @NewReversePeriod2, @NewGLAccountPK2, @NewBranchPK2, @NewCompanyPK2, @NewDepartmentPK2, @NewTransactionCategory2) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@GLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);
			expectedCommand.AddParameter("@BranchPK2", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@CompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@DepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@PostPeriod2", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@LineAmount2", SqlDbType.Money, -LineAmount2);
			expectedCommand.AddParameter("@TransactionCategory2", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@ReversePeriod2", SqlDbType.Int, ReversePeriod0);
			expectedCommand.AddParameter("@ReverseLineAmount2", SqlDbType.Money, LineAmount2);

			expectedCommand.AddParameter("@NewGLAccountPK2", SqlDbType.UniqueIdentifier, GLAccountPK2);
			expectedCommand.AddParameter("@NewBranchPK2", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@NewCompanyPK2", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK2", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod2", SqlDbType.Int, Period0);
			expectedCommand.AddParameter("@NewLineAmount2", SqlDbType.Money, LineAmount2);
			expectedCommand.AddParameter("@NewTransactionCategory2", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewReversePeriod2", SqlDbType.Int, 200309);
			expectedCommand.AddParameter("@NewReverseLineAmount2", SqlDbType.Money, -LineAmount2);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestNothingUpdatedIfAmountIsNotChanged()
		{
			InsertExistingAggregates();

			GL.AH_Desc = "Original Desc";
			GLReverseOnlineAggregator aggregator = new GLReverseOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			actualCommand.ExecuteNonQuery();

			// Pre Condition Assert
			AccGLAggregate[] result = GetAggregateResult(GLAccountPK0);
			AssertEquals(50.0m, CountAggregateTotal(result));

			result = GetAggregateResult(GLAccountPK1);
			AssertEquals(-50.0m, CountAggregateTotal(result));

			TestFactory.Save();

			// Change Description of Existing Journal
			GLJournal exsitingJournal = Factory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			exsitingJournal.AH_Desc = "Changed";
			GLReverseOnlineAggregator newAggregator = new GLReverseOnlineAggregator(exsitingJournal, GL);

			newAggregator.Aggregate();

			result = GetAggregateResult(GLAccountPK0);
			AssertEquals(50.0m, CountAggregateTotal(result));

			result = GetAggregateResult(GLAccountPK1);
			AssertEquals(-50.0m, CountAggregateTotal(result));
		}

		AccGLAggregate[] GetAggregateResult(Guid pK)
		{
			ZQuery query = new ZQuery(AccGLAggregateSchema.AA_AG, pK);
			return Factory.Load<AccGLAggregate>(query);
		}

		decimal CountAggregateTotal(AccGLAggregate[] aggregate)
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

		protected DbCommand GetGLAggregateDbCommand(decimal reverseSign)
		{
			string mergedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount0, @PostPeriod0, @GLAccountPK0, @BranchPK0, @CompanyPK0, @DepartmentPK0, @TransactionCategory0) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount0, @ReversePeriod0, @GLAccountPK0, @BranchPK0, @CompanyPK0, @DepartmentPK0, @TransactionCategory0) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1, @PostPeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount1, @ReversePeriod1, @GLAccountPK1, @BranchPK1, @CompanyPK1, @DepartmentPK1, @TransactionCategory1) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount2, @PostPeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @ReverseLineAmount2, @ReversePeriod2, @GLAccountPK2, @BranchPK2, @CompanyPK2, @DepartmentPK2, @TransactionCategory2) " +
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

			command.AddParameter("@ReversePeriod0", SqlDbType.Int, ReversePeriod0);
			command.AddParameter("@ReversePeriod1", SqlDbType.Int, ReversePeriod0);
			command.AddParameter("@ReversePeriod2", SqlDbType.Int, ReversePeriod0);

			command.AddParameter("@LineAmount0", SqlDbType.Money, LineAmount0 * reverseSign);
			command.AddParameter("@LineAmount1", SqlDbType.Money, LineAmount1 * reverseSign);
			command.AddParameter("@LineAmount2", SqlDbType.Money, LineAmount2 * reverseSign);

			command.AddParameter("@ReverseLineAmount0", SqlDbType.Money, -LineAmount0 * reverseSign);
			command.AddParameter("@ReverseLineAmount1", SqlDbType.Money, -LineAmount1 * reverseSign);
			command.AddParameter("@ReverseLineAmount2", SqlDbType.Money, -LineAmount2 * reverseSign);

			command.AddParameter("@TransactionCategory0", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);
			command.AddParameter("@TransactionCategory1", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);
			command.AddParameter("@TransactionCategory2", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			return command;
		}
	}
}
