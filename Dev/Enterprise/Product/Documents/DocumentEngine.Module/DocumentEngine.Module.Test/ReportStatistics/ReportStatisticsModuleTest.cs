using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ReportStatisticsModule))]
	sealed class ReportStatisticsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ReportStatistics;
		}

		public void TestModuleID()
		{
			using (ReportStatisticsModule module = new ReportStatisticsModule())
			{
				AssertEquals("ModuleID", ModuleIDs.ReportStatistics, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (ReportStatisticsModule module = new ReportStatisticsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ReportStatistics, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (ReportStatisticsModuleForTest module = new ReportStatisticsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is ReportStatisticsFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (ReportStatisticsModuleForTest module = new ReportStatisticsModuleForTest())
			{
				IBusinessObjectCollection scheduleTasksCollection = module.NewGridCollection;
				Assert("Invalid type", scheduleTasksCollection is StmReportRunCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (ReportStatisticsModuleForTest module = new ReportStatisticsModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is ReportStatisticsFilterBusinessObject);
			}
		}

		#endregion
	}
}
