using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentConstantsTest : TestCase
	{
		#region Test Status Lists

		public void TestGetStatusCodeDescPairList()
		{
			CodeDescriptionPairList statusList = IncidentConstants.GetStatusCodeDescPairList();

			AssertEquals("StatusCodeDescPairList.Count", 15, statusList.Count);

			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.Unassigned + ")", "Unassigned", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.Unassigned));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.Assigned + ")", "Assigned, but Not Started", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.Assigned));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.InProgress + ")", "In Progress", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.InProgress));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.Waiting + ")", "Waiting on Someone Else", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.Waiting));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.AwaitingFeatureAnalysis + ")", "Awaiting Feature Analysis", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.AwaitingFeatureAnalysis));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.AwaitingSchedulingTeam + ")", "Awaiting Scheduling Team", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.AwaitingSchedulingTeam));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.AwaitingTeamScheduler + ")", "Awaiting Team Scheduler", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.AwaitingTeamScheduler));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.AwaitingDevelopmentWork + ")", "Awaiting Development Work", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.AwaitingDevelopmentWork));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.FinishedPendingCodeReview + ")", "Finished - Pending Code Review", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.FinishedPendingCodeReview));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.FinishedPendingCheckIn + ")", "Finished - Pending Check In", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.FinishedPendingCheckIn));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.FinishedCheckedInAndPendingDeploy + ")", "Finished - Checked In and Pending Deploy", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.FinishedCheckedInAndPendingDeploy));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.Closed + ")", "Closed", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.Closed));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.Cancelled + ")", "Cancelled", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.Cancelled));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.Deferred + ")", "Deferred", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.Deferred));
			AssertEquals("GetDescriptionFromCode(" + IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification + ")", "Finished - Deployed and Pending Verification", statusList.GetDescriptionFromCode(IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification));
		}

		#endregion

		public void TestIncidentType()
		{
			AssertEquals("IncidentType.ProfessionalServicesQuote", "PSQ", IncidentConstants.IncidentType.ProfessionalServicesQuote);
			AssertEquals("IncidentType.SupportIncident", "INC", IncidentConstants.IncidentType.SupportIncident);
		}

		public void TestGetIncidentTypeDescription()
		{
			AssertEquals("GetIncidentTypeDescription(\"PSQ\")", "Professional Services Quote", IncidentConstants.GetIncidentTypeDescription("PSQ"));
			AssertEquals("GetIncidentTypeDescription(\"INC\")", "Customer Service Incident", IncidentConstants.GetIncidentTypeDescription("INC"));
			AssertEquals("GetIncidentTypeDescription(\"XI\")", "", IncidentConstants.GetIncidentTypeDescription("XI"));
		}

		public void TestSupportConstants()
		{
			AssertEquals("SupportDisplayName", "CargoWise Customer Service", IncidentConstants.SupportDisplayName);
		}

		public void TestProgramAreaList()
		{
			CodeDescriptionPairList programAreaList = IncidentConstants.GetProgramAreaList();

			AssertEquals("Count", 6, programAreaList.Count);
			AssertEquals("GetDescriptionFromCode(\"GEN\")", "General", programAreaList.GetDescriptionFromCode("GEN"));
			AssertEquals("GetDescriptionFromCode(\"DOC\")", "Documents", programAreaList.GetDescriptionFromCode("DOC"));
			AssertEquals("GetDescriptionFromCode(\"REP\")", "Reports", programAreaList.GetDescriptionFromCode("REP"));
			AssertEquals("GetDescriptionFromCode(\"GRA\")", "Graphics", programAreaList.GetDescriptionFromCode("GRA"));
			AssertEquals("GetDescriptionFromCode(\"INT\")", "Interface", programAreaList.GetDescriptionFromCode("INT"));
			AssertEquals("GetDescriptionFromCode(\"DAT\")", "Data Fix", programAreaList.GetDescriptionFromCode("DAT"));
		}

		public void TestPriorityCodeDescPairList()
		{
			CodeDescriptionPairList priorityCodeDescPairList = IncidentConstants.GetPriorityCodeDescPairList();

			AssertEquals("Count", 4, priorityCodeDescPairList.Count);
			AssertEquals("GetDescriptionFromCode(\"CRT\")", "Critical", priorityCodeDescPairList.GetDescriptionFromCode("CRT"));
			AssertEquals("GetDescriptionFromCode(\"HI\")", "High", priorityCodeDescPairList.GetDescriptionFromCode("HI"));
			AssertEquals("GetDescriptionFromCode(\"MED\")", "Medium", priorityCodeDescPairList.GetDescriptionFromCode("MED"));
			AssertEquals("GetDescriptionFromCode(\"LOW\")", "Low", priorityCodeDescPairList.GetDescriptionFromCode("LOW"));
		}
	}
}
