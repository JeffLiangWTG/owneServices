using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TimeEngineScheduler.Business.Test
{
	[TestedType(typeof(TimeActionSchedule))]
	class TimeActionScheduleTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyProcessEstimateLog = Factory.New<TimeActionSchedule>();
			dummyProcessEstimateLog.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow;
			dummyProcessEstimateLog.TAS_ActionCode = "DUM";
			dummyProcessEstimateLog.TAS_TargetTableCode = "TAS";
			dummyProcessEstimateLog.TAS_GB_Branch = GlbBranch.CurrentBranch.PK;
			dummyProcessEstimateLog.TAS_GE_Department = GlbDepartment.CurrentDepartment.PK;
			return dummyProcessEstimateLog;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
