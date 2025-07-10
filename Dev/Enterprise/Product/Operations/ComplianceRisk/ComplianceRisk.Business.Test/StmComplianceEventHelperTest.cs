using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public static class StmComplianceEventHelperTest
	{
		public static StmComplianceEvent CreateAssessmentEvent(BusinessObject bizO, string assessmentType)
		{
			var eventLog = bizO.Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = assessmentType;
			eventLog.SCE_ParentID = bizO.PK;
			return eventLog;
		}
	}
}
