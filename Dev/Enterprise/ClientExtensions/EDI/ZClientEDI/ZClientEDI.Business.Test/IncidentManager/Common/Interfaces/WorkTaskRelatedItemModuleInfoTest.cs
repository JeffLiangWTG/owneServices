using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using WorkTaskRelatedItemModuleInfo = Enterprise.ProcessManagement.Business.WorkTaskRelatedItemModuleInfo;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class WorkTaskRelatedItemModuleInfoTest : TestCaseWithFactory
	{
		public void TestIssue()
		{
			WorkTaskRelatedItemModuleInfo issueInfo = EDIWorkTaskRelatedItemModuleInfo.Issue(Factory);
			AssertEquals("Issue", issueInfo.Caption);
			AssertEquals(EDIWorkTaskRelatedItemTypes.Issue, issueInfo.Type);
			AssertEquals(false, issueInfo.AllowNew);
			AssertEquals(true, issueInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.IssueManager, issueInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.IssueManager, issueInfo.ModuleID);
			AssertNotNull(issueInfo.FindBoxList);
		}

		public void TestQuote()
		{
			var quoteInfo = EDIWorkTaskRelatedItemModuleInfo.Quote(Factory);
			AssertEquals("Professional Services Quote", quoteInfo.Caption);
			AssertEquals(EDIWorkTaskRelatedItemTypes.ProfessionalServiceQuote, quoteInfo.Type);
			AssertEquals(true, quoteInfo.AllowNew);
			AssertEquals(true, quoteInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.ProfessionalServicesQuote, quoteInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.ProfessionalServicesQuote, quoteInfo.ModuleID);
			AssertNotNull(quoteInfo.FindBoxList);

			quoteInfo = EDIWorkTaskRelatedItemModuleInfo.Quote(Factory, false);
			AssertEquals("Professional Services Quote", quoteInfo.Caption);
			AssertEquals(EDIWorkTaskRelatedItemTypes.ProfessionalServiceQuote, quoteInfo.Type);
			AssertEquals(false, quoteInfo.AllowNew);
			AssertEquals(true, quoteInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.ProfessionalServicesQuote, quoteInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.ProfessionalServicesQuote, quoteInfo.ModuleID);
			AssertNotNull(quoteInfo.FindBoxList);
		}

		public void TestIncident()
		{
			WorkTaskRelatedItemModuleInfo incidentInfo = EDIWorkTaskRelatedItemModuleInfo.GenericIncident(Factory);
			AssertEquals("Incident", incidentInfo.Caption);
			AssertNull(incidentInfo.Type);
			AssertEquals(false, incidentInfo.AllowNew);
			AssertEquals(true, incidentInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.SupportIncident, incidentInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.SupportIncident, incidentInfo.ModuleID);
			AssertNotNull(incidentInfo.FindBoxList);

			incidentInfo = EDIWorkTaskRelatedItemModuleInfo.GenericIncident(Factory, false);
			AssertEquals("Incident", incidentInfo.Caption);
			AssertNull(incidentInfo.Type);
			AssertEquals(false, incidentInfo.AllowNew);
			AssertEquals(false, incidentInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.SupportIncident, incidentInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.SupportIncident, incidentInfo.ModuleID);
			AssertNotNull(incidentInfo.FindBoxList);

			WorkTaskRelatedItemModuleInfo defectInfo = EDIWorkTaskRelatedItemModuleInfo.Defect(Factory);
			AssertEquals("Defect", defectInfo.Caption);
			AssertEquals(EDIWorkTaskRelatedItemTypes.Defect, defectInfo.Type);
			AssertEquals(true, defectInfo.AllowNew);
			AssertEquals(false, defectInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.SupportIncident, defectInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.SupportIncident, defectInfo.ModuleID);
			AssertNull(defectInfo.FindBoxList);

			WorkTaskRelatedItemModuleInfo featureRequestInfo = EDIWorkTaskRelatedItemModuleInfo.FeatureRequest(Factory);
			AssertEquals("Feature Request", featureRequestInfo.Caption);
			AssertEquals(EDIWorkTaskRelatedItemTypes.FeatureRequest, featureRequestInfo.Type);
			AssertEquals(true, featureRequestInfo.AllowNew);
			AssertEquals(false, featureRequestInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.SupportIncident, featureRequestInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.SupportIncident, featureRequestInfo.ModuleID);
			AssertNull(featureRequestInfo.FindBoxList);

			WorkTaskRelatedItemModuleInfo escalatedIncidentInfo = EDIWorkTaskRelatedItemModuleInfo.EscalatedIncident(Factory, null);
			AssertEquals("Defect / Feature Request / Compliance Requirement / Service Request / Content Development", escalatedIncidentInfo.Caption);
			AssertNull(escalatedIncidentInfo.Type);
			AssertEquals(false, escalatedIncidentInfo.AllowNew);
			AssertEquals(true, escalatedIncidentInfo.AllowAttach);
			AssertEquals(ClientControllerRegistration.SupportIncident, escalatedIncidentInfo.ControllerID);
			AssertEquals(ClientModuleRegistration.SupportIncident, escalatedIncidentInfo.ModuleID);
			AssertNotNull(escalatedIncidentInfo.FindBoxList);
			AssertNotNull(escalatedIncidentInfo.AdditionalFilterForFindBox);
			var defectIncident = Factory.New<SupportIncident>();
			defectIncident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var featureIncident = Factory.New<SupportIncident>();
			featureIncident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");

			AssertContainsExactElementsInAnyOrder(new[] { defectIncident, featureIncident }, Factory.Load<SupportIncident>(escalatedIncidentInfo.AdditionalFilterForFindBox));
		}

		public void TestWorkItem()
		{
			WorkTaskRelatedItemModuleInfo workItemInfo = EDIWorkTaskRelatedItemModuleInfo.NewWorkItem(Factory);
			AssertEquals("Work Item", workItemInfo.Caption);
			AssertEquals(ProcessManagement.Business.WorkTaskRelatedItemTypes.WorkItem, workItemInfo.Type);
			AssertEquals(true, workItemInfo.AllowNew);
			AssertEquals(true, workItemInfo.AllowAttach);
			AssertEquals(ControllerIDs.WorkItem, workItemInfo.ControllerID);
			AssertEquals(ModuleIDs.WorkItem, workItemInfo.ModuleID);
			AssertNotNull(workItemInfo.FindBoxList);

			workItemInfo = EDIWorkTaskRelatedItemModuleInfo.NewWorkItem(Factory, false);
			AssertEquals("Work Item", workItemInfo.Caption);
			AssertEquals(ProcessManagement.Business.WorkTaskRelatedItemTypes.WorkItem, workItemInfo.Type);
			AssertEquals(false, workItemInfo.AllowNew);
			AssertEquals(true, workItemInfo.AllowAttach);
			AssertEquals(ControllerIDs.WorkItem, workItemInfo.ControllerID);
			AssertEquals(ModuleIDs.WorkItem, workItemInfo.ModuleID);
			AssertNotNull(workItemInfo.FindBoxList);
		}

		public void TestProject()
		{
			WorkTaskRelatedItemModuleInfo projectInfo = EDIWorkTaskRelatedItemModuleInfo.EDIProject(Factory);
			AssertEquals("Project", projectInfo.Caption);
			AssertEquals(ProcessManagement.Business.WorkTaskRelatedItemTypes.Project, projectInfo.Type);
			AssertEquals(true, projectInfo.AllowNew);
			AssertEquals(true, projectInfo.AllowAttach);
			AssertEquals(ControllerIDs.Project, projectInfo.ControllerID);
			AssertEquals(ModuleIDs.Project, projectInfo.ModuleID);
			AssertNotNull(projectInfo.FindBoxList);

			projectInfo = EDIWorkTaskRelatedItemModuleInfo.EDIProject(Factory, false);
			AssertEquals("Project", projectInfo.Caption);
			AssertEquals(ProcessManagement.Business.WorkTaskRelatedItemTypes.Project, projectInfo.Type);
			AssertEquals(false, projectInfo.AllowNew);
			AssertEquals(true, projectInfo.AllowAttach);
			AssertEquals(ControllerIDs.Project, projectInfo.ControllerID);
			AssertEquals(ModuleIDs.Project, projectInfo.ModuleID);
			AssertNotNull(projectInfo.FindBoxList);
		}

		public void TestOpportunity()
		{
			WorkTaskRelatedItemModuleInfo opportunityInfo = EDIWorkTaskRelatedItemModuleInfo.Opportunity(Factory);
			AssertEquals("Opportunity", opportunityInfo.Caption);
			AssertEquals(ProcessManagement.Business.WorkTaskRelatedItemTypes.Opportunity, opportunityInfo.Type);
			AssertEquals(true, opportunityInfo.AllowNew);
			AssertEquals(true, opportunityInfo.AllowAttach);
			AssertEquals(ControllerIDs.Opportunity, opportunityInfo.ControllerID);
			AssertEquals(ModuleIDs.Opportunity, opportunityInfo.ModuleID);
			AssertNotNull(opportunityInfo.FindBoxList);

			opportunityInfo = EDIWorkTaskRelatedItemModuleInfo.Opportunity(Factory, false);
			AssertEquals("Opportunity", opportunityInfo.Caption);
			AssertEquals(ProcessManagement.Business.WorkTaskRelatedItemTypes.Opportunity, opportunityInfo.Type);
			AssertEquals(false, opportunityInfo.AllowNew);
			AssertEquals(true, opportunityInfo.AllowAttach);
			AssertEquals(ControllerIDs.Opportunity, opportunityInfo.ControllerID);
			AssertEquals(ModuleIDs.Opportunity, opportunityInfo.ModuleID);
			AssertNotNull(opportunityInfo.FindBoxList);

			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			opportunityInfo = EDIWorkTaskRelatedItemModuleInfo.Opportunity(Factory, false, supportIncident);

			var expectedAdditionalFilter = $"P8_PK NOT IN (SELECT RAP_ChildActivityID FROM dbo.RelatedActivityPivot WHERE RAP_ParentActivityID <> '{supportIncident.PK}')";
			AssertEquals(expectedAdditionalFilter, ((ILegacyBusinessObjectCollectionInternals)opportunityInfo.FindBoxList).AdditionalFilter.ParameterisedText.LiteralTextSql);
		}
	}
}
