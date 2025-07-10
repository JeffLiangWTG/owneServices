using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.Aggregator.Test;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Aggregator.Testing
{
	public class ReaggregateTest : TestCaseWithFactory
	{
		public void TestDumpAggregateData()
		{
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 2;

			AccGLHeader[] gLAccount = Factory.Load(typeof(AccGLHeader), filter) as AccGLHeader[];
			InsertTestAggregateRow(gLAccount[0].PK, 120, 200301);
			InsertTestAggregateRow(gLAccount[0].PK, 150, 200302);
			InsertTestAggregateRow(gLAccount[1].PK, 140, 200301);

			AggregateRunner testRunner = new AggregateRunner();
			testRunner.DumpAggregateData();

			int tempTableRowCount = GetNoOfRows(AggregateRunner.TempAggregate);
			AssertEquals(3, tempTableRowCount);
			AssertEquals(120m, GetAggregateValueFromTempTable(gLAccount[0].PK.ToGuid(), 200301));
			AssertEquals(150m, GetAggregateValueFromTempTable(gLAccount[0].PK.ToGuid(), 200302));
			AssertEquals(140m, GetAggregateValueFromTempTable(gLAccount[1].PK.ToGuid(), 200301));
		}

		//[ExpectException(typeof(SqlException))]
		[ExpectNoExceptions()]
		public void TestTempTableCreated()
		{
			InsertTestTransaction();
			AggregateRunner testRunner = new AggregateRunner();
			testRunner.Aggregate();
			testRunner.DumpAggregateData();
			testRunner.ReAggregate();
		}

		[ExpectException(typeof(System.Data.Common.DbException))]
		public void TestTempTableDropped()
		{
			InsertTestTransaction();
			AggregateRunner testRunner = new AggregateRunner();
			testRunner.ReAggregate();
			GetNoOfRows(AggregateRunner.TempAggregate);
		}

		public void TestTempTableExist()
		{
			AggregateRunner testRunner = new AggregateRunner();
			Assert(!testRunner.DoesTempTableExist_ForTestOnly());
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 2;

			AccGLHeader[] gLAccount = Factory.Load(typeof(AccGLHeader), filter) as AccGLHeader[];
			InsertTestAggregateRow(gLAccount[0].PK, 120, 200301);
			InsertTestAggregateRow(gLAccount[0].PK, 150, 200302);
			InsertTestAggregateRow(gLAccount[1].PK, 140, 200301);

			testRunner.DumpAggregateData();

			Assert(testRunner.DoesTempTableExist_ForTestOnly());
		}

		public void TestUnableToLock_NullUser()
		{
			var testRunner = new AggregateRunner();
			using (var otherMutex = new ZGlobalMutex(MutexIDs.BatchAggregtorRunning, Env.CurrentCompany.PK.ToString()))
			{
				Assert(otherMutex.Lock());
				testRunner.Aggregate();
			}
		}

		public void TestComparison()
		{
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 4;

			AccGLHeader[] gLAccount = Factory.Load(typeof(AccGLHeader), filter) as AccGLHeader[];
			InsertTestAggregateRow(gLAccount[0].PK, 120, 200301);
			InsertTestAggregateRow(gLAccount[0].PK, 150, 200302);
			InsertTestAggregateRow(gLAccount[1].PK, 140, 200301);
			Guid linePK = InsertTestAggregateRow(gLAccount[2].PK, 140, 200302);

			AggregateRunner testRunner = new AggregateRunner();
			testRunner.DumpAggregateData();

			int tempTableRowCount = GetNoOfRows(AggregateRunner.TempAggregate);
			AssertEquals(4, tempTableRowCount);

			InsertTestAggregateRow(gLAccount[1].PK, 140, 200301);
			InsertTestAggregateRow(gLAccount[3].PK, 140, 200302);
			DeleteFromAggregateRow(linePK);

			string result = testRunner.GenerateReport_ForTestOnly(GlbCompany.CurrentCompany);
			string expectedResult1 = "".PadRight(12) + " " +
				"".PadRight(3) + " " +
				"".PadRight(3) + " " + "".PadRight(10) + " " + "".PadRight(30) + " " +
				gLAccount[2].AG_AccountNum.PadRight(12) + " " +
				GlbBranch.CurrentBranch.GB_Code.PadRight(3) + " " +
				GlbDepartment.CurrentDepartment.GE_Code.PadRight(3) + " " + "200302".PadRight(10) + " " + "140.0000 ".PadRight(31) + System.Environment.NewLine;

			string expectedResult2 = gLAccount[3].AG_AccountNum.PadRight(12) + " " +
				GlbBranch.CurrentBranch.GB_Code.PadRight(3) + " " +
				GlbDepartment.CurrentDepartment.GE_Code.PadRight(3) + " " + "200302".PadRight(10) + " " + "140.0000 ".PadRight(30) + " " +
				"".PadRight(12) + " " +
				"".PadRight(3) + " " +
				"".PadRight(3) + " " + "".PadRight(10) + " " + "".PadRight(31) + System.Environment.NewLine;

			string expectedResult3 = gLAccount[1].AG_AccountNum.PadRight(12) + " " +
				GlbBranch.CurrentBranch.GB_Code.PadRight(3) + " " +
				GlbDepartment.CurrentDepartment.GE_Code.PadRight(3) + " " + "200301".PadRight(10) + " " + "280.0000 ".PadRight(30) + " " +
				gLAccount[1].AG_AccountNum.PadRight(12) + " " +
				GlbBranch.CurrentBranch.GB_Code.PadRight(3) + " " +
				GlbDepartment.CurrentDepartment.GE_Code.PadRight(3) + " " + "200301".PadRight(10) + " " + "140.0000 ".PadRight(31) + System.Environment.NewLine;

			string expectedResult = testRunner.GetErrorMsgHeader_ForTestOnly(GlbCompany.CurrentCompany) + System.Environment.NewLine +
				expectedResult1 + expectedResult2 + expectedResult3;

			tempTableRowCount = GetNoOfRows(AggregateRunner.TempAggregate);
			AssertEquals(4, tempTableRowCount);
			AssertEquals(expectedResult, result);
		}

		BatchTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new BatchTestHelper(Factory)); }
		}
		BatchTestHelper testHelper;

		public void TestForSinglePeriod()
		{
			Guid gLAccount1 = TestHelper.GLHeaders[0].PK.ToGuid();
			Guid gLAccount2 = TestHelper.GLHeaders[1].PK.ToGuid();

			AggregateRunner testRunner = new AggregateRunner();
			Assert(testRunner.IsAggregateOnlyForSinglePeriod_ForTestOnly(GlbCompany.CurrentCompany));

			TestHelper.InsertTestAggregateRow(Factory, gLAccount1, 120, 200301);
			TestHelper.InsertTestAggregateRow(Factory, gLAccount2, 120, 200301);

			Assert(testRunner.IsAggregateOnlyForSinglePeriod_ForTestOnly(GlbCompany.CurrentCompany));

			TestHelper.InsertTestAggregateRow(Factory, gLAccount2, 120, 200302);
			Assert(!testRunner.IsAggregateOnlyForSinglePeriod_ForTestOnly(GlbCompany.CurrentCompany));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper.InsertBanks();

			TestHelper.UpdateChargeAccounts(); // TODO This part is slow. Need to Improve...

			TestHelper.SetUpPeriods();

			TestHelper.SetControlAccounts();

			TestHelper.Branch1 = GlbBranch.CurrentBranch.PK.ToGuid();
			TestHelper.Department1 = GlbDepartment.CurrentDepartment.PK.ToGuid();
			TestHelper.PostDate1 = ZDateTime.Now;
		}

		public void TestForPeriodTotalEqualsZero()
		{
			Guid gLAccount1 = TestHelper.GLHeaders[0].PK.ToGuid();
			Guid gLAccount2 = TestHelper.GLHeaders[1].PK.ToGuid();

			AggregateRunner testRunner = new AggregateRunner();
			Assert(testRunner.IsPeriodTotalEqualsZero_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid(), 200301));

			TestHelper.InsertTestAggregateRow(Factory, gLAccount1, 120, 200301);
			TestHelper.InsertTestAggregateRow(Factory, gLAccount2, -120, 200301);

			Assert(testRunner.IsPeriodTotalEqualsZero_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid(), 200301));

			TestHelper.InsertTestAggregateRow(Factory, gLAccount2, 50, 200301);
			Assert(!testRunner.IsPeriodTotalEqualsZero_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid(), 200301));

			TestHelper.InsertTestAggregateRow(Factory, gLAccount1, -50, 200301);
			Assert(testRunner.IsPeriodTotalEqualsZero_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid(), 200301));
		}

		public void TestDeadlockException()
		{
			var aggregateRunner = new AggregateRunnerForTestDeadlock();
			aggregateRunner.SetupDeadlock();
			Assert("no deadlock yet", !aggregateRunner.FailedWithDeadlock);
			aggregateRunner.Aggregate();
			Assert("got deadlock", aggregateRunner.FailedWithDeadlock);
		}

		#region Implementation

		protected decimal GetAggregateValueFromTempTable(Guid gLAccount, int period)
		{
			decimal result = 0m;
			string sQL = @"Select sum(AA_Amount) From " + AggregateRunner.TempAggregate +
				@" Where AA_AG = @GLAccount AND AA_Period = @Period
				Group BY AA_Period, AA_AG, AA_GB, AA_GE";
			DbCommand cmd = ((IDbConnected)Factory).Connection.Command(sQL);
			cmd.AddParameter("@GLAccount", SqlDbType.UniqueIdentifier, gLAccount);
			cmd.AddParameter("@Period", SqlDbType.Int, period);

			object queryResult = cmd.ExecuteScalar();
			if (queryResult != null)
			{
				result = (decimal)queryResult;
			}

			return result;
		}

		protected void InsertTestTransaction()
		{
			TestHelper.TestDataSet = new AccTransactionHeaderCollection(Factory);

			TestHelper.SetReceipt(TestHelper.TestDataSet, "AR", 100.0m);

			Factory.Save();
		}

		protected int GetNoOfRows(string tableName)
		{
			int result = 0;
			string sQL = @"Select count(*) From " + tableName;
			DbCommand cmd = ((IDbConnected)Factory).Connection.Command(sQL);

			object queryResult = cmd.ExecuteScalar();
			if (queryResult != null)
			{
				result = (int)queryResult;
			}

			return result;
		}

		protected Guid InsertTestAggregateRow(ZGuid gLAccount, decimal amount, int period)
		{
			BatchTestHelper testHelper = new BatchTestHelper(Factory);
			return testHelper.InsertTestAggregateRow(Factory, gLAccount, amount, period);
		}

		protected void DeleteFromAggregateRow(Guid linePK)
		{
			((IDbConnected)Factory).Connection.ExecuteNonQuery("DELETE from dbo.AccGLAggregate WHERE AA_PK = '" + linePK.ToString() + "'");
		}

		class AggregateRunnerForTestDeadlock : AggregateRunner
		{
			public void SetupDeadlock()
			{
				var mock = new Mock<IBatchAggregator>();
				mock.Setup(m => m.Aggregate()).Returns(false);
				mock.Setup(m => m.AggregateResult).Returns("deadlock");
				mock.Setup(m => m.LastException).Returns(BatchAggregatorTestSqlExceptionHelper.CreateDeadlockSqlException<SqlException>());
				base.MainAggregator = mock.Object;
			}
		}

		#endregion
	}
}
