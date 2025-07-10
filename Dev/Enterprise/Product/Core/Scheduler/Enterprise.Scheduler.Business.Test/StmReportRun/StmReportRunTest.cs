using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmReportRun))]
	sealed class StmReportRunTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConcurrencyPolicy()
		{
			AssertEquals(ConcurrencyPolicy.Ignore, StmReportRun.RRI_StatusInfo.ConcurrencyPolicy);
			foreach (ZPropertyInfo property in StmReportRun.ZPropertyInfoHash)
			{
				if (property.IsPersistent)
				{
					AssertEquals(ConcurrencyPolicy.Ignore, property.ConcurrencyPolicy);
				}
			}
		}

		[TestDate(2018, 03, 16, 18, 0, 0)]
		public void TestTimeBeingProcessed()
		{
			StmReportRun.RRI_StartTimeUtc = new ZDateTime(2018, 03, 16, 15, 0, 0);
			AssertEquals("01-Jan-00 03:00:00", StmReportRun.TimeBeingProcessed.ToString());

			StmReportRun.RRI_StartTimeUtc = ZDateTime.Invalid;
			AssertEquals("01-Jan-00 00:00:00", StmReportRun.TimeBeingProcessed.ToString());
		}

		public void TestDocManagerInfo()
		{
			AssertEquals("DocManagerInfo.GetType()", typeof(DocManagerInfo), ((IDocManagerSupport)StmReportRun).DocManagerInfo.GetType());
			AssertEquals("DocManagerInfo.DocManagerCode", Core.Constants.DocManagerCodes.ReportStatistic, ((IDocManagerSupport)StmReportRun).DocManagerInfo.DocManagerCode);
		}

		public void TestShouldDeleteStorageMainWhenDeleteStmReportRun()
		{
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);

			((IDocManagerSupport)StmReportRun).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3, 4, 5 }, "xxx.yyy", "DUM");
			((IDocManagerSupport)StmReportRun).DocManagerInfo.MasterFactory.Save();
			Factory.Save();

			var count = ((IDbConnected)Factory).Connection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StorageMain");
			AssertEquals("StmReportRun should have 1 file", 1, ((IDocManagerSupport)StmReportRun).DocManagerInfo.AllEDocs.Count);
			AssertEquals("StorageMain table should 1 record", 1, count);

			var eDocFile = ((IDocManagerSupport)StmReportRun).DocManagerInfo.AllEDocs[0] as BusinessObject;
			StmReportRun.Delete();
			Factory.Save();

			count = ((IDbConnected)Factory).Connection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StorageMain");
			AssertEquals("eDoc file should be deleted", true, eDocFile.IsDeleted);
			AssertEquals("StorageMain table should empty", 0, count);
		}

		#region Implementation

		StmReportRun StmReportRun
		{
			get
			{
				if (stmReportRun == null)
				{
					stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
					stmReportRun.RRI_S5_Schedule = Task.PK;
				}
				return stmReportRun;
			}
		}
		StmReportRun stmReportRun;

		StmScheduleTask Task => task ?? (task = Factory.NewWithValidTestData<StmScheduleTask>());
		StmScheduleTask task;

		#endregion
	}
}
