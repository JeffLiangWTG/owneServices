using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.Aggregator.Test;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Aggregator.Testing
{
	public class MutexTest : TestCaseWithFactory
	{
		public void TestAggregateMethodSuccessWhenObtainMutex()
		{
			SetupDataForAggregateCall();

			var testAggregateRunner = new AggregateRunner();
			bool result = testAggregateRunner.Aggregate();

			Assert("Aggregation Should Succeed Due To No Mutex.", result);
			AssertEquals("FailedToAquireMutex property should be false", false, testAggregateRunner.FailedToAquireMutex);
		}

		public void TestAggregateMethodFailureWhenFailtoObtainMutex()
		{
			OtherMutex = new ZGlobalMutex(MutexIDs.BatchAggregtorRunning, Env.CurrentCompany.PK.ToString());
			Assert(OtherMutex.Lock());

			var testAggregateRunner = new AggregateRunner();
			bool result = testAggregateRunner.Aggregate();

			AssertEquals("Aggregation Should Fail Due To Mutex using CurrentCompany PK.", false, result);
			AssertContains("Correct error message returned", "GL Account update has been canceled because a GL Account update is currently being run by user 'CargoWise Support'", testAggregateRunner.AggregateResult);
			Assert("FailedToAquireMutex property should be true", testAggregateRunner.FailedToAquireMutex);
		}

		public void TestAggregateMethodFailureWhenFailtoObtainMutexAndLockInfoIsNull()
		{
			OtherMutex = new ZGlobalMutex(MutexIDs.BatchAggregtorRunning, Env.CurrentCompany.PK.ToString());
			Assert(OtherMutex.Lock());

			var testAggregateRunner = new AggregateRunner();
			testAggregateRunner.ReleaseLock_ForTest = () => OtherMutex.Unlock();
			bool result = testAggregateRunner.Aggregate();

			AssertEquals("Aggregation Should Fail Due To Mutex using CurrentCompany PK.", false, result);
			AssertEquals("Correct error message returned", "GL Account update has been canceled because a GL Account update is currently being run by user '*unknown user*' since *unknown time*", testAggregateRunner.AggregateResult);
			Assert("FailedToAquireMutex property should be true", testAggregateRunner.FailedToAquireMutex);
		}

		public void TestAggregateMethodUsesCompanySpecificMutex()
		{
			OtherMutex = new ZGlobalMutex(MutexIDs.BatchAggregtorRunning, ZGuid.NewZGuid().ToString());
			Assert(OtherMutex.Lock());

			SetupDataForAggregateCall();

			var testAggregateRunner = new AggregateRunner();
			bool result = testAggregateRunner.Aggregate();

			Assert("Aggregation Should Succeed Due To Company Specific Mutex.", result);
			AssertEquals("FailedToAquireMutex property should be false", false, testAggregateRunner.FailedToAquireMutex);
		}

		protected override void TearDown()
		{
			if (OtherMutex != null && OtherMutex.IsLocked)
			{
				OtherMutex.Unlock();
			}
		}
		#region Implementation

		BatchTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new BatchTestHelper(Factory)); }
		}
		BatchTestHelper testHelper;

		void SetupDataForAggregateCall()
		{
			TestBatchAggregator testAggregator = new TestBatchAggregator(TestHelper);
			TestHelper.SetControlAccounts();
			TestHelper.UpdateChargeAccounts();
			TestHelper.SetUpPeriods();
			testAggregator.SetControlAccount(AccountingUtils.ARControl, TestHelper.ARControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APControl, TestHelper.APControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.GSTInput, TestHelper.GSTIn);
			testAggregator.SetControlAccount(AccountingUtils.GSTOutput, TestHelper.GSTOut);
			testAggregator.SetControlAccount(AccountingUtils.ARSuspenseControlAccount, TestHelper.ARSuspenseControlAccount);
			testAggregator.SetControlAccount(AccountingUtils.APSuspenseControlAccount, TestHelper.APSuspenseControlAccount);
		}

		protected ZGlobalMutex OtherMutex;

		#endregion
	}
}
