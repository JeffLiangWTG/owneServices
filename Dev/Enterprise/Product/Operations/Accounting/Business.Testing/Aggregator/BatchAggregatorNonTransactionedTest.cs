using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Aggregator.Testing
{
	public class BatchAggregatorNonTransactionedTest : TransactionedTestCase
	{
		[UseSnapshotProtection]
		public void TestCreatesErrorReportWhenExceptionHandled()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			TestConnection.ExecuteNonQuery("UPDATE dbo.AccChargeCode SET AC_AG_CostAccount = 'AC129D82-B88D-45EE-BCE5-25592F734023', AC_SystemLastEditTimeUtc = GETUTCDATE(), AC_SystemLastEditUser = 'TST' where AC_CODE = 'BOND' OR AC_CODE = 'CLAIM'");
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_GuidValue) VALUES ('{0}', '{1}', '{2}', '{3}')", ZGuid.NewZGuid(), "GL_AP_SUSPENSE_CONTROL_ACCOUNT", "GID", "AC129D82-B88D-45EE-BCE5-25592F734023"));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_GuidValue) VALUES ('{0}', '{1}', '{2}', '{3}')", ZGuid.NewZGuid(), "GL_AR_SUSPENSE_CONTROL_ACCOUNT", "GID", "AC129D82-B88D-45EE-BCE5-25592F734023"));

			var staff = factory.NewWithValidTestData<GlbStaff>();
			factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			using (Globals.SetIsUserInteractiveForTest(false))
			using (var cmd = TestConnection.Command(string.Format("SELECT COUNT(*) FROM [{0}]", StmErrorReportSchema.Constants.TableName)))
			{
				int startingRowCount = (int)cmd.ExecuteScalar();
				TestHelper.SetUpPeriods();

				TestBatchAggregator aggregator = new TestBatchAggregator(TestConnection, true, TestHelper);
				aggregator.Aggregate();

				var endingRowCount = (int)cmd.ExecuteScalar();
				AssertEquals("New error report must exist", startingRowCount + 1, endingRowCount);
			}
		}

		protected BatchTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new BatchTestHelper(new BusinessObjectFactory())); }
		}
		BatchTestHelper testHelper;

		protected override DbConnection TestConnection
		{
			get { return Db.Connection; }
		}
	}
}
