using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class GLAutoOnlineAggregatorTest : TestCaseWithFactory
	{
		protected Guid GLAccountPK0;
		protected Guid GLAccountPK1;

		protected Guid BranchPK0;
		protected Guid BranchPK1;
		protected Guid BranchPK2;

		protected Guid DepartmentPK0;
		protected Guid DepartmentPK1;

		protected const int Period0 = 200303;
		protected const int Period1 = 200303;
		protected const int Period2 = 200304;

		protected const int ReversePeriod0 = 200305;
		protected const int ReversePeriod1 = 200305;

		protected const decimal LineAmount0 = 150.0m;
		protected const decimal LineAmount1 = -150.0m;

		protected const decimal NewLineAmount1 = -70.0m;
		protected Guid NewDepartmentPK1;

		protected GLJournal GL;
		protected GLJournal OriginalGL;

		protected BusinessObjectFactory TestFactory;
		protected BusinessObjectFactory TestFactory2;

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 1, 1), new ZDateTime(2003, 1, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1), new ZDateTime(2003, 2, 28, 23, 59, 59));
			testHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200304, new ZDateTime(2003, 4, 1), new ZDateTime(2003, 4, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200305, new ZDateTime(2003, 5, 1), new ZDateTime(2003, 5, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200306, new ZDateTime(2003, 6, 1), new ZDateTime(2003, 6, 30, 23, 59, 59));

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);

			TestFactory = new BusinessObjectFactory();
			TestFactory2 = new BusinessObjectFactory();

			ZQuery query = new ZQuery();
			query.MaximumRows = 4;

			AccGLHeader[] gLHeader = TestFactory.Load<AccGLHeader>(query);
			GlbBranch[] branches = TestFactory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			GlbDepartment[] departments = TestFactory.Load<GlbDepartment>(query);

			GLAccountPK0 = gLHeader[0].PK.ToGuid();
			GLAccountPK1 = gLHeader[1].PK.ToGuid();

			BranchPK0 = branches[0].PK.ToGuid();
			BranchPK1 = branches[1].PK.ToGuid();
			BranchPK2 = branches[2].PK.ToGuid();

			DepartmentPK0 = departments[0].PK.ToGuid();
			DepartmentPK1 = departments[1].PK.ToGuid();

			GL = TestFactory.New(typeof(GLJournal)) as GLJournal;
			OriginalGL = TestFactory2.New(typeof(GLJournal)) as GLJournal;

			AccChargeCode charge = TestFactory.LoadTop1<AccChargeCode>(new ZQuery());

			GL.AH_Ledger = LedgerTypes.General;
			GL.AH_TransactionType = TransactionTypes.GLAutoJournal;
			GL.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			GL.PostPeriod = Period0;
			GL.AgePeriod = ReversePeriod0;

			GL.GLJournalLines.AddNew();
			GL.GLJournalLines.AddNew();

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
		}

		public void TestEditingReverseDateOnBigAutoJournal()
		{
			BusinessObjectFactory originalJournalFactory = new BusinessObjectFactory();
			GLJournal originalJournal = originalJournalFactory.New<GLJournal>();
			originalJournal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			originalJournal.PostPeriod = 200301;
			originalJournal.AgePeriod = 200306;
			for (int index = 0; index <= 299; index++) //
			{
				GLJournalLine line = (GLJournalLine)originalJournal.Lines.AddNew();
				line.AL_AG = GLAccountPK1;
				line.UnsignedOSLineAmount = 100m;
				line.DebitCreditSign = nameof(DebitCredit.DR);
			}
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLAccountPK0);
			originalJournal.Balance();

			AggregateWrapper wrapper = new AggregateWrapper(originalJournal, originalJournal);
			BusinessObjectFactory.SaveTogether(originalJournalFactory, wrapper);
			BusinessObjectFactory editedJournalFactory = new BusinessObjectFactory();
			GLJournal editedJournal = editedJournalFactory.Load<GLJournal>(originalJournal.PK);
			editedJournal.AgePeriod = 200304;

			wrapper = new AggregateWrapper(editedJournal, originalJournal);
			BusinessObjectFactory.SaveTogether(editedJournalFactory, wrapper);

			AssertEquals("Should be 30000", 30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK1, 200301));
			AssertEquals("Should be 30000", -30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK0, 200301));

			AssertEquals("Should be 30000", 30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK1, 200302));
			AssertEquals("Should be 30000", -30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK0, 200302));

			AssertEquals("Should be 30000", 30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK1, 200303));
			AssertEquals("Should be 30000", -30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK0, 200303));

			AssertEquals("Should be 30000", 30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK1, 200304));
			AssertEquals("Should be 30000", -30000m, GetSumFromAccGLAggregateForAccount(GLAccountPK0, 200304));

			AssertEquals("Should be 0", 0m, GetSumFromAccGLAggregateForAccount(GLAccountPK1, 200305));
			AssertEquals("Should be 0", 0m, GetSumFromAccGLAggregateForAccount(GLAccountPK0, 200305));

			AssertEquals("Should be 0", 0m, GetSumFromAccGLAggregateForAccount(GLAccountPK1, 200306));
			AssertEquals("Should be 0", 0m, GetSumFromAccGLAggregateForAccount(GLAccountPK0, 200306));
		}

		decimal GetSumFromAccGLAggregateForAccount(Guid gLAccount, int period)
		{
			string sQL = "SELECT SUM(AA_Amount) as Amount FROM dbo.AccGLAggregate WHERE AA_AG = @AccountNum and AA_Period = @Period";
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@AccountNum", gLAccount, AccGLAggregateSchema.AA_AG);
			parameters.Add("@Period", period, AccGLAggregateSchema.AA_Period);
			DynamicBusinessObjectCollection dynamicCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicCollection.Load(sQL, parameters);
			return (ZDecimal)dynamicCollection[0]["Amount"];
		}

		public void TestNewGeneralJournalWithBizO()
		{
			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateSqlCommand();

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestGenerateNewStatementWithBizO()
		{
			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateNewCommandsWithBizO(dbCommandFactory, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateSqlCommand();

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestGenerateReverseStatementWithBizO()
		{
			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateReverseCommandsWithBizO(dbCommandFactory, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			DbCommand expectedCommand = GetGLAggregateSqlCommand(GLAccountPK0, GLAccountPK1, BranchPK0, BranchPK1, Period0, Period1, -LineAmount0, -LineAmount1, InvoiceTypesList.Codes.FinalInvoice, InvoiceTypesList.Codes.FinalInvoice);

			//			ExpectedCommand.Parameters["@LineAmount200303"].Value = -LineAmount0;
			//			ExpectedCommand.Parameters["@LineAmount200304"].Value = -LineAmount0;
			//			ExpectedCommand.Parameters["@LineAmount200305"].Value = -LineAmount0;
			//			ExpectedCommand.Parameters["@LineAmount1200303"].Value = -LineAmount1;
			//			ExpectedCommand.Parameters["@LineAmount1200304"].Value = -LineAmount1;
			//			ExpectedCommand.Parameters["@LineAmount1200305"].Value = -LineAmount1;

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestDeletedGeneralJournalWithBizO()
		{
			TestFactory.Save();

			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;
			//GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[1].Delete();

			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200303, @PostPeriod1200303, @GLAccountPK1200303, @BranchPK1200303, @CompanyPK1200303, @DepartmentPK1200303, @TransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200304, @PostPeriod1200304, @GLAccountPK1200304, @BranchPK1200304, @CompanyPK1200304, @DepartmentPK1200304, @TransactionCategory1200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200305, @PostPeriod1200305, @GLAccountPK1200305, @BranchPK1200305, @CompanyPK1200305, @DepartmentPK1200305, @TransactionCategory1200305) " +
				" ; "; // The Second Line

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);

			expectedCommand.AddParameter("@GLAccountPK1200303", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@GLAccountPK1200304", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@GLAccountPK1200305", SqlDbType.UniqueIdentifier, GLAccountPK1);

			expectedCommand.AddParameter("@BranchPK1200303", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@BranchPK1200304", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@BranchPK1200305", SqlDbType.UniqueIdentifier, BranchPK1);

			expectedCommand.AddParameter("@CompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@CompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@CompanyPK1200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);

			expectedCommand.AddParameter("@DepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@DepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@DepartmentPK1200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);

			expectedCommand.AddParameter("@PostPeriod1200303", SqlDbType.Int, Period1);
			expectedCommand.AddParameter("@PostPeriod1200304", SqlDbType.Int, Period1 + 1);
			expectedCommand.AddParameter("@PostPeriod1200305", SqlDbType.Int, Period1 + 2);

			expectedCommand.AddParameter("@LineAmount1200303", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@LineAmount1200304", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@LineAmount1200305", SqlDbType.Money, -LineAmount1);

			expectedCommand.AddParameter("@TransactionCategory1200303", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);
			expectedCommand.AddParameter("@TransactionCategory1200304", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);
			expectedCommand.AddParameter("@TransactionCategory1200305", SqlDbType.VarChar, 3, InvoiceTypesList.Codes.FinalInvoice);

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

			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200303, @PostPeriod1200303, @GLAccountPK1200303, @BranchPK1200303, @CompanyPK1200303, @DepartmentPK1200303, @TransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200304, @PostPeriod1200304, @GLAccountPK1200304, @BranchPK1200304, @CompanyPK1200304, @DepartmentPK1200304, @TransactionCategory1200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200305, @PostPeriod1200305, @GLAccountPK1200305, @BranchPK1200305, @CompanyPK1200305, @DepartmentPK1200305, @TransactionCategory1200305) " +
				" ; "; // The Second Line

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);

			expectedCommand.AddParameter("@GLAccountPK1200303", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@GLAccountPK1200304", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@GLAccountPK1200305", SqlDbType.UniqueIdentifier, GLAccountPK1);

			expectedCommand.AddParameter("@BranchPK1200303", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@BranchPK1200304", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@BranchPK1200305", SqlDbType.UniqueIdentifier, BranchPK1);

			expectedCommand.AddParameter("@CompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@CompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@CompanyPK1200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);

			expectedCommand.AddParameter("@DepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@DepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@DepartmentPK1200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);

			expectedCommand.AddParameter("@PostPeriod1200303", SqlDbType.Int, Period1);
			expectedCommand.AddParameter("@PostPeriod1200304", SqlDbType.Int, Period1 + 1);
			expectedCommand.AddParameter("@PostPeriod1200305", SqlDbType.Int, Period1 + 2);

			expectedCommand.AddParameter("@LineAmount1200303", SqlDbType.Money, NewLineAmount1 - LineAmount1);
			expectedCommand.AddParameter("@LineAmount1200304", SqlDbType.Money, NewLineAmount1 - LineAmount1);
			expectedCommand.AddParameter("@LineAmount1200305", SqlDbType.Money, NewLineAmount1 - LineAmount1);

			expectedCommand.AddParameter("@TransactionCategory1200303", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);
			expectedCommand.AddParameter("@TransactionCategory1200304", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);
			expectedCommand.AddParameter("@TransactionCategory1200305", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

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

			GL.GLJournalLines[1].AL_GB = BranchPK2;

			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount1200303, @OriginalPostPeriod1200303, @OriginalGLAccountPK1200303, @OriginalBranchPK1200303, @OriginalCompanyPK1200303, @OriginalDepartmentPK1200303, @OriginalTransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount1200304, @OriginalPostPeriod1200304, @OriginalGLAccountPK1200304, @OriginalBranchPK1200304, @OriginalCompanyPK1200304, @OriginalDepartmentPK1200304, @OriginalTransactionCategory1200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount1200305, @OriginalPostPeriod1200305, @OriginalGLAccountPK1200305, @OriginalBranchPK1200305, @OriginalCompanyPK1200305, @OriginalDepartmentPK1200305, @OriginalTransactionCategory1200305) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount1200303, @NewPostPeriod1200303, @NewGLAccountPK1200303, @NewBranchPK1200303, @NewCompanyPK1200303, @NewDepartmentPK1200303, @NewTransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount1200304, @NewPostPeriod1200304, @NewGLAccountPK1200304, @NewBranchPK1200304, @NewCompanyPK1200304, @NewDepartmentPK1200304, @NewTransactionCategory1200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount1200305, @NewPostPeriod1200305, @NewGLAccountPK1200305, @NewBranchPK1200305, @NewCompanyPK1200305, @NewDepartmentPK1200305, @NewTransactionCategory1200305) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@OriginalGLAccountPK1200303", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@OriginalBranchPK1200303", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@OriginalCompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@OriginalDepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@OriginalPostPeriod1200303", SqlDbType.Int, Period1);
			expectedCommand.AddParameter("@OriginalLineAmount1200303", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@OriginalTransactionCategory1200303", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@OriginalGLAccountPK1200304", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@OriginalBranchPK1200304", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@OriginalCompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@OriginalDepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@OriginalPostPeriod1200304", SqlDbType.Int, Period1 + 1);
			expectedCommand.AddParameter("@OriginalLineAmount1200304", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@OriginalTransactionCategory1200304", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@OriginalGLAccountPK1200305", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@OriginalBranchPK1200305", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@OriginalCompanyPK1200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@OriginalDepartmentPK1200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@OriginalPostPeriod1200305", SqlDbType.Int, Period1 + 2);
			expectedCommand.AddParameter("@OriginalLineAmount1200305", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@OriginalTransactionCategory1200305", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewGLAccountPK1200303", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@NewBranchPK1200303", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@NewCompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod1200303", SqlDbType.Int, Period1);
			expectedCommand.AddParameter("@NewLineAmount1200303", SqlDbType.Money, LineAmount1);
			expectedCommand.AddParameter("@NewTransactionCategory1200303", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewGLAccountPK1200304", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@NewBranchPK1200304", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@NewCompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod1200304", SqlDbType.Int, Period1 + 1);
			expectedCommand.AddParameter("@NewLineAmount1200304", SqlDbType.Money, LineAmount1);
			expectedCommand.AddParameter("@NewTransactionCategory1200304", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewGLAccountPK1200305", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@NewBranchPK1200305", SqlDbType.UniqueIdentifier, BranchPK2);
			expectedCommand.AddParameter("@NewCompanyPK1200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK1200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod1200305", SqlDbType.Int, Period1 + 2);
			expectedCommand.AddParameter("@NewLineAmount1200305", SqlDbType.Money, LineAmount1);
			expectedCommand.AddParameter("@NewTransactionCategory1200305", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestPeriodChangedUpdatedGeneralJournalWithBizO()
		{
			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);

			//Re-load Original GL
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;

			TestFactory.Save();
			GL = TestFactory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			GLJournal testGL = TestFactory2.Load(typeof(GLJournal), GL.PK) as GLJournal;

			GL.GLJournalLines[1].AL_ReverseDate = periodCalc.GetFirstDayForPeriod(200304).ToDateTime().AddDays(1);

			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, testGL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, testGL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			string expectedSQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount1200303, @OriginalPostPeriod1200303, @OriginalGLAccountPK1200303, @OriginalBranchPK1200303, @OriginalCompanyPK1200303, @OriginalDepartmentPK1200303, @OriginalTransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount1200304, @OriginalPostPeriod1200304, @OriginalGLAccountPK1200304, @OriginalBranchPK1200304, @OriginalCompanyPK1200304, @OriginalDepartmentPK1200304, @OriginalTransactionCategory1200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @OriginalLineAmount1200305, @OriginalPostPeriod1200305, @OriginalGLAccountPK1200305, @OriginalBranchPK1200305, @OriginalCompanyPK1200305, @OriginalDepartmentPK1200305, @OriginalTransactionCategory1200305) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount1200303, @NewPostPeriod1200303, @NewGLAccountPK1200303, @NewBranchPK1200303, @NewCompanyPK1200303, @NewDepartmentPK1200303, @NewTransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @NewLineAmount1200304, @NewPostPeriod1200304, @NewGLAccountPK1200304, @NewBranchPK1200304, @NewCompanyPK1200304, @NewDepartmentPK1200304, @NewTransactionCategory1200304) " +
				" ; ";

			DbCommand expectedCommand = Db.Connection.Command(expectedSQL);
			expectedCommand.AddParameter("@OriginalGLAccountPK1200303", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@OriginalBranchPK1200303", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@OriginalCompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@OriginalDepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@OriginalPostPeriod1200303", SqlDbType.Int, Period1);
			expectedCommand.AddParameter("@OriginalLineAmount1200303", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@OriginalTransactionCategory1200303", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@OriginalGLAccountPK1200304", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@OriginalBranchPK1200304", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@OriginalCompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@OriginalDepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@OriginalPostPeriod1200304", SqlDbType.Int, Period1 + 1);
			expectedCommand.AddParameter("@OriginalLineAmount1200304", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@OriginalTransactionCategory1200304", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@OriginalGLAccountPK1200305", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@OriginalBranchPK1200305", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@OriginalCompanyPK1200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@OriginalDepartmentPK1200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@OriginalPostPeriod1200305", SqlDbType.Int, Period1 + 2);
			expectedCommand.AddParameter("@OriginalLineAmount1200305", SqlDbType.Money, -LineAmount1);
			expectedCommand.AddParameter("@OriginalTransactionCategory1200305", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewGLAccountPK1200303", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@NewBranchPK1200303", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@NewCompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod1200303", SqlDbType.Int, Period1);
			expectedCommand.AddParameter("@NewLineAmount1200303", SqlDbType.Money, LineAmount1);
			expectedCommand.AddParameter("@NewTransactionCategory1200303", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			expectedCommand.AddParameter("@NewGLAccountPK1200304", SqlDbType.UniqueIdentifier, GLAccountPK1);
			expectedCommand.AddParameter("@NewBranchPK1200304", SqlDbType.UniqueIdentifier, BranchPK1);
			expectedCommand.AddParameter("@NewCompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			expectedCommand.AddParameter("@NewDepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			expectedCommand.AddParameter("@NewPostPeriod1200304", SqlDbType.Int, Period1 + 1);
			expectedCommand.AddParameter("@NewLineAmount1200304", SqlDbType.Money, LineAmount1);
			expectedCommand.AddParameter("@NewTransactionCategory1200304", SqlDbType.VarChar, InvoiceTypesList.Codes.FinalInvoice);

			//System.Diagnostics.Debug.WriteLine(ExpectedCommand.CommandText);
			//System.Diagnostics.Debug.WriteLine(ActualCommand.CommandText);

			AssertEquals("SQL String", expectedCommand.CommandText, actualCommand.CommandText);
			Assert(expectedCommand.ParameterCollectionEquals(actualCommand));
		}

		public void TestNothingUpdatedIfAmountIsNotChanged()
		{
			InsertExistingAggregates();

			GL.AH_Desc = "Original Desc";
			GLAutoOnlineAggregator aggregator = new GLAutoOnlineAggregator(GL, GL);
			AggregatorDbCommandFactory dbCommandFactory = new AggregatorDbCommandFactory();
			aggregator.GenerateUpdateCommandsWithBizO(dbCommandFactory, GL, GL);
			DbCommand actualCommand = dbCommandFactory.GetCommands()[0];

			actualCommand.ExecuteNonQuery();

			// Pre Condition Assert
			AccGLAggregate[] result = GetAggregateResult(GLAccountPK0);
			AssertEquals(500.0m, CountAggregateTotal(result));

			result = GetAggregateResult(GLAccountPK1);
			AssertEquals(-500.0m, CountAggregateTotal(result));

			TestFactory.Save();

			// Change Description of Existing Journal
			GLJournal exsitingJournal = Factory.Load(typeof(GLJournal), GL.PK) as GLJournal;
			exsitingJournal.AH_Desc = "Changed";
			GLAutoOnlineAggregator newAggregator = new GLAutoOnlineAggregator(exsitingJournal, GL);

			newAggregator.Aggregate();

			result = GetAggregateResult(GLAccountPK0);
			AssertEquals(500.0m, CountAggregateTotal(result));

			result = GetAggregateResult(GLAccountPK1);
			AssertEquals(-500.0m, CountAggregateTotal(result));
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

		protected string GetGLAggregateSqlCommandText()
		{
			string sQL = " INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount200303, @PostPeriod200303, @GLAccountPK200303, @BranchPK200303, @CompanyPK200303, @DepartmentPK200303, @TransactionCategory200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount200304, @PostPeriod200304, @GLAccountPK200304, @BranchPK200304, @CompanyPK200304, @DepartmentPK200304, @TransactionCategory200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount200305, @PostPeriod200305, @GLAccountPK200305, @BranchPK200305, @CompanyPK200305, @DepartmentPK200305, @TransactionCategory200305) " +
				" ; " + // The first line
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200303, @PostPeriod1200303, @GLAccountPK1200303, @BranchPK1200303, @CompanyPK1200303, @DepartmentPK1200303, @TransactionCategory1200303) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200304, @PostPeriod1200304, @GLAccountPK1200304, @BranchPK1200304, @CompanyPK1200304, @DepartmentPK1200304, @TransactionCategory1200304) " +
				" ; " +
				" INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) " +
				" VALUES (newid(), @LineAmount1200305, @PostPeriod1200305, @GLAccountPK1200305, @BranchPK1200305, @CompanyPK1200305, @DepartmentPK1200305, @TransactionCategory1200305) " +
				" ; "; // The Second Line

			return sQL;
		}

		protected DbCommand GetGLAggregateSqlCommand()
		{
			return GetGLAggregateSqlCommand(GLAccountPK0, GLAccountPK1, BranchPK0, BranchPK1, Period0, Period1, LineAmount0, LineAmount1, InvoiceTypesList.Codes.FinalInvoice, InvoiceTypesList.Codes.FinalInvoice);
		}

		protected DbCommand GetGLAggregateSqlCommand(Guid account0, Guid account1, Guid branch0, Guid branch1, int prd0, int prd1, decimal amount0, decimal amount1, string transactionCategory0, string transactionCategory1)
		{
			string mergedSQL = GetGLAggregateSqlCommandText();

			DbCommand command = Db.Connection.Command(mergedSQL);

			command.AddParameter("@GLAccountPK200303", SqlDbType.UniqueIdentifier, account0);
			command.AddParameter("@GLAccountPK200304", SqlDbType.UniqueIdentifier, account0);
			command.AddParameter("@GLAccountPK200305", SqlDbType.UniqueIdentifier, account0);
			command.AddParameter("@GLAccountPK1200303", SqlDbType.UniqueIdentifier, account1);
			command.AddParameter("@GLAccountPK1200304", SqlDbType.UniqueIdentifier, account1);
			command.AddParameter("@GLAccountPK1200305", SqlDbType.UniqueIdentifier, account1);

			command.AddParameter("@BranchPK200303", SqlDbType.UniqueIdentifier, branch0);
			command.AddParameter("@BranchPK200304", SqlDbType.UniqueIdentifier, branch0);
			command.AddParameter("@BranchPK200305", SqlDbType.UniqueIdentifier, branch0);
			command.AddParameter("@BranchPK1200303", SqlDbType.UniqueIdentifier, branch1);
			command.AddParameter("@BranchPK1200304", SqlDbType.UniqueIdentifier, branch1);
			command.AddParameter("@BranchPK1200305", SqlDbType.UniqueIdentifier, branch1);

			command.AddParameter("@CompanyPK200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK1200303", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK1200304", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);
			command.AddParameter("@CompanyPK1200305", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK);

			command.AddParameter("@DepartmentPK200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK1200303", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK1200304", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);
			command.AddParameter("@DepartmentPK1200305", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK);

			command.AddParameter("@PostPeriod200303", SqlDbType.Int, prd0);
			command.AddParameter("@PostPeriod200304", SqlDbType.Int, prd0 + 1);
			command.AddParameter("@PostPeriod200305", SqlDbType.Int, prd0 + 2);
			command.AddParameter("@PostPeriod1200303", SqlDbType.Int, prd1);
			command.AddParameter("@PostPeriod1200304", SqlDbType.Int, prd1 + 1);
			command.AddParameter("@PostPeriod1200305", SqlDbType.Int, prd1 + 2);

			command.AddParameter("@LineAmount200303", SqlDbType.Money, amount0);
			command.AddParameter("@LineAmount200304", SqlDbType.Money, amount0);
			command.AddParameter("@LineAmount200305", SqlDbType.Money, amount0);
			command.AddParameter("@LineAmount1200303", SqlDbType.Money, amount1);
			command.AddParameter("@LineAmount1200304", SqlDbType.Money, amount1);
			command.AddParameter("@LineAmount1200305", SqlDbType.Money, amount1);

			command.AddParameter("@TransactionCategory200303", SqlDbType.VarChar, 3, transactionCategory0);
			command.AddParameter("@TransactionCategory200304", SqlDbType.VarChar, 3, transactionCategory0);
			command.AddParameter("@TransactionCategory200305", SqlDbType.VarChar, 3, transactionCategory0);
			command.AddParameter("@TransactionCategory1200303", SqlDbType.VarChar, 3, transactionCategory1);
			command.AddParameter("@TransactionCategory1200304", SqlDbType.VarChar, 3, transactionCategory1);
			command.AddParameter("@TransactionCategory1200305", SqlDbType.VarChar, 3, transactionCategory1);

			return command;
		}
	}
}
