using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(NewWorkItem))]
	public class NewWorkItemTest : ProcessManagement.Business.Test.WorkItemTest
	{
		#region Number Fountain Issue

		public void TestSaveDoesntUseTwoNumberFountainNumbers()
		{
			NewWorkItem item = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();

			NewWorkItem item2 = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();

			ZInt number = Convert.ToInt32(item.JobNumber.Substring(2));
			ZInt number2 = Convert.ToInt32(item2.JobNumber.Substring(2));

			AssertEquals(number + 1, number2);
		}

		#endregion

		public new void TestFetchForLoad()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<NewWorkItem>(workItem.PK));
			AssertEquals("Fetch hints other than GenCustomAddOnValue should be used", newFactory.ActiveFetchHintsForTable(GenCustomAddOnValueSchema.Constants.TableName), newFactory.ActiveTableFetchHints);
		}

		#region Related Objects

		public void TestWorkItemTasksCreatedAndLoadedProperly()
		{
			NewWorkItem item = Factory.NewWithValidTestData<NewWorkItem>();
			AssertNotNull(item.WorkflowItems);
			AssertEquals(0, item.WorkflowItems.Count);
			item.WorkflowItems.AddNew();
			AssertEquals(1, item.WorkflowItems.Count);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			NewWorkItem reloadedWorkItem = newFactory.Load<NewWorkItem>(item.PK);
			AssertEquals(1, reloadedWorkItem.WorkflowItems.Count);
		}

		public void TestWorkflowItems()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;
			taskTemplate.P0_SubType4 = "ENH";
			taskTemplate.WorkflowItems.AddNew();
			taskTemplate.WorkflowItems.AddNew();
			Factory.Save();

			NewWorkItem item = Factory.NewWithValidTestData<NewWorkItem>();
			AssertNotNull(item.WorkflowItems);
			AssertEquals(0, item.WorkflowItems.Count);

			item.WKI_ActivitySubtype = "ENH";
			Factory.Save();

			AssertEquals(2, item.WorkflowItems.Count);

			AssertEquals(GlbStaff.CurrentUser.GS_Code, item.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, item.WorkflowItems[1].P9_GS_NKAssignedStaffMember);
		}

		public void TestWorkflowItems_WhenFirstTaskAssignedToCapability_ShouldNotAssignResource()
		{
			var capability = Factory.New<GlbCapability>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;
			template.P0_SubType4 = "ENH";
			var templateTask = template.WorkflowItems.AddNew();
			templateTask.P9_G4_RequiredCapability = capability.PK;

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			AssertNotNull(workItem.WorkflowItems);
			AssertEquals(0, workItem.WorkflowItems.Count);

			workItem.WKI_ActivitySubtype = "ENH";
			Factory.Save();

			AssertEquals(1, workItem.WorkflowItems.Count);

			AssertEquals(ZString.Empty, workItem.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(capability.PK, workItem.WorkflowItems[0].P9_G4_RequiredCapability);
		}

		public void TestCloseWorkItemUpdateRelatedIncident()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemTask = workItem.WorkflowItems.AddNew();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, string.Empty);
			incident.RelatedItems.Add(workItem);
			Factory.Save();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, workItem.WKI_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
			AssertNotEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
		}

		#endregion

		#region Actions

		public void TestDisposition()
		{
			var workItem = Factory.New<NewWorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Description = "Failing Unit Test";
			task1.P9_GS_NKAssignedStaffMember = "";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(string.Format("{0} - Failing Unit Test", ProcessTaskStatusCodeList.Descriptions.Open), workItem.DispositionDescription);

			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(string.Format("{0} - Failing Unit Test", ProcessTaskStatusCodeList.Descriptions.Assigned), workItem.DispositionDescription);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Descriptions.Cancelled, workItem.DispositionDescription);

			var task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Description = "Coding";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(string.Format("{0} - Coding", ProcessTaskStatusCodeList.Descriptions.Working), workItem.DispositionDescription);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(string.Format("{0} - Coding", ProcessTaskStatusCodeList.Descriptions.Suspended), workItem.DispositionDescription);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Descriptions.Closed, workItem.DispositionDescription);
		}

		public new void TestCancel()
		{
			NewWorkItem item = Factory.NewWithValidTestData<NewWorkItem>();
			ProcessTask task1 = item.WorkflowItems.AddNew();
			ProcessTask task2 = item.WorkflowItems.AddNew();
			ProcessTask task3 = item.WorkflowItems.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("MEH MEH"));
			ProcessTask milestone = item.WorkflowItems.Milestones.AddNew();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS00001130";
			incident.IM_Description = "I'm an incident";
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.RelatedItems.Add(item);
			AssertEquals("Pre-condition", SupportIncidentLookups.Status.Working, incident.IM_Status);

			item.Cancel();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, item.WKI_Status);
			AssertNotEquals("Non tasks should not be affected", ProcessTaskStatusCodeList.Codes.Cancelled, milestone.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task3.P9_Status);
			AssertEquals("Incident should be closed", SupportIncidentLookups.Status.Closed, incident.IM_Status);
			Factory.Save(); // this previously caused an endless loop
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, item.WKI_Status);
		}

		public void TestCancel_ReloadWorkflow()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS00001130";
			incident.IM_Description = "I'm an incident";
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItem.WorkflowItems.AddNew();
			var task2 = workItem.WorkflowItems.AddNew();
			incident.RelatedItems.Add(workItem);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			workItem.WorkflowItems.Load();
			AssertNotContains("Closed As Closed Internally - All related work items are cancelled", incident.ResolutionNoteText);
		}

		#endregion

		#region Properties

		public void TestJobAgreedDeliveryDate()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			var type = system.RelatedWorkflowTypes.AddNew();
			type.FSW_WorkflowType = "WKI";
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var processHeader = Factory.New<ProcessHeader>();
			processHeader.FH_ParentId = workItem.PK;
			processHeader.FH_ParentTableCode = WorkItemSchema.Constants.Prefix;

			AssertEquals(ZDateTime.Empty, workItem.JobAgreedDeliveryDate);
			processHeader.AgreedDeliveryDateLocal = new ZDateTime(2018, 7, 3, 11, 0, 0);
			AssertEquals(new ZDateTime(2018, 7, 3, 11, 0, 0), workItem.JobAgreedDeliveryDate);
		}

		[ExpectNoExceptions]
		public void TestRelatedClientCode()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_FullName = "Name1";
			client1.OH_Code = "Client1";
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.OH_FullName = "Name2";
			client2.OH_Code = "Client2";

			NewWorkItem mainItem = Factory.NewWithValidTestData<NewWorkItem>();

			SupportIncident relatedItem1 = GetRelatedWorkItemWithClient(mainItem, client1);
			SupportIncident relatedItem2 = GetRelatedWorkItemWithClient(mainItem, client1);

			AssertEquals("RelatedItems.Count", 2, mainItem.RelatedItems.Count);
			AssertEquals("RelatedClientCode", "Client1", mainItem.RelatedClientCode);

			ProfessionalServicesQuote relatedItem3 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			relatedItem3.IM_OH_Client = client2.PK;
			mainItem.RelatedItems.Add(relatedItem3);

			AssertEquals("RelatedItems.Count", 3, mainItem.RelatedItems.Count);
			AssertEquals("RelatedClientCode", "Client1, Client2", mainItem.RelatedClientCode);

			SupportIncident relatedItem4 = Factory.New<SupportIncident>();
			relatedItem4.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			mainItem.RelatedItems.Add(relatedItem4);
			AssertEquals("RelatedItems.Count", 4, mainItem.RelatedItems.Count);
			AssertEquals("RelatedClientCode", "Client1, Client2", mainItem.RelatedClientCode);

			NewWorkItem wrongItem = Factory.New<NewWorkItem>();
			mainItem.RelatedItems.Add(wrongItem);
			AssertEquals("RelatedItems.Count", 4, mainItem.RelatedItems.Count);
			AssertEquals("RelatedClientCode", "Client1, Client2", mainItem.RelatedClientCode);
		}

		SupportIncident GetRelatedWorkItemWithClient(NewWorkItem mainItem, OrgHeader client)
		{
			SupportIncident relatedItem = Factory.NewWithValidTestData<SupportIncident>();
			relatedItem.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			relatedItem.IM_OH_Client = client.PK;
			mainItem.RelatedItems.Add(relatedItem);
			return relatedItem;
		}

		public void TestCriticalityBasedOnRelatedIncidents()
		{
			SupportIncident incident1 = Factory.New<SupportIncident>();
			incident1.IM_Priority = "CR6";
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;

			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.IM_Priority = "CR5";
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;

			NewWorkItem workItem1 = Factory.New<NewWorkItem>();
			AssertEquals("Criticality should be empty", string.Empty, workItem1.CriticalityBasedOnRelatedIncidents);
			workItem1.RelatedItems.Add(incident1);
			workItem1.RelatedItems.Add(incident2);
			AssertEquals("Highest related criticiality", "CR5", workItem1.CriticalityBasedOnRelatedIncidents);

			NewWorkItem workItem2 = Factory.New<NewWorkItem>();
			workItem2.RelatedItems.Add(incident2);
			workItem2.RelatedItems.Add(incident1);
			AssertEquals("Related Incident order shouldn't matter", "CR5", workItem2.CriticalityBasedOnRelatedIncidents);

			incident1.IM_Priority = "CR2";

			NewWorkItem workItem4 = Factory.New<NewWorkItem>();
			workItem4.RelatedItems.Add(incident1);
			workItem4.RelatedItems.Add(incident2);
			AssertEquals("related items", "CR2", workItem4.CriticalityBasedOnRelatedIncidents);

			incident1.IM_Priority = "CR3";

			NewWorkItem workItem6 = Factory.New<NewWorkItem>();
			workItem6.RelatedItems.Add(incident1);
			AssertEquals("CR3", workItem6.CriticalityBasedOnRelatedIncidents);

			incident1.IM_Priority = "";

			var workItem7 = Factory.New<NewWorkItem>();
			workItem7.RelatedItems.Add(incident1);
			AssertEquals("defaults to empty when attached incidents dont have priority", "", workItem7.CriticalityBasedOnRelatedIncidents);
		}

		public void TestNumberOfRelatedIncidents()
		{
			SupportIncident incident1 = Factory.New<SupportIncident>();
			SupportIncident incident2 = Factory.New<SupportIncident>();
			SupportIncident incident3 = Factory.New<SupportIncident>();
			SupportIncident incident4 = Factory.New<SupportIncident>();

			EDIProject project1 = CreateNewProject("PRJ1", "PM1");
			EDIProject project2 = CreateNewProject("PRJ2", "PM1");

			NewWorkItem workItem = Factory.New<NewWorkItem>();
			AssertEquals("Number of related Incidents should be 0", 0, workItem.NumberOfRelatedIncidents);

			workItem.RelatedItems.Add(incident1);
			AssertEquals("Number of related Incidents should be 1", 1, workItem.NumberOfRelatedIncidents);

			workItem.RelatedItems.Add(incident2);
			workItem.RelatedItems.Add(incident3);
			AssertEquals("Number of related Incidents should be 3", 3, workItem.NumberOfRelatedIncidents);

			workItem.RelatedItems.Add(incident4);
			workItem.RelatedItems.Add(project1);
			workItem.RelatedItems.Add(project2);
			AssertEquals("Number of related Incidents should be 4", 4, workItem.NumberOfRelatedIncidents);
		}

		public void TestIsAttachedToDefectOrIssue()
		{
			NewWorkItem workItem = Factory.New<NewWorkItem>();
			Assert("No related items", !workItem.IsAttachedToDefectOrIssue);

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			workItem.RelatedItems.Add(incident);
			Assert("Not attached to support", !workItem.IsAttachedToDefectOrIssue);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			Assert("Attached to defect", workItem.IsAttachedToDefectOrIssue);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			Assert("Not attached to defect", !workItem.IsAttachedToDefectOrIssue);

			EdiHelpErrorLog issue = Factory.New<EdiHelpErrorLog>();
			workItem.RelatedItems.Add(issue);
			Assert("Attached to issue", workItem.IsAttachedToDefectOrIssue);

			SupportIncident defect = Factory.New<SupportIncident>();
			defect.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			workItem.RelatedItems.Add(defect);
			Assert("Attached to defect & issue", workItem.IsAttachedToDefectOrIssue);

			workItem.RelatedItems.Remove(issue);
			Assert("Still attached to defect", workItem.IsAttachedToDefectOrIssue);
		}

		public void TestProjectIDsAndProjectManagerNames()
		{
			EDIProject project1 = CreateNewProject("PRJ1", "PM1");
			EDIProject project2 = CreateNewProject("PRJ2", "PM1");
			EDIProject project3 = CreateNewProject("PRJ3", "PM2");
			EDIProject project4 = CreateNewProject("PRJ4", "PM3");

			SupportIncident request1 = Factory.New<SupportIncident>();
			request1.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			SupportIncident request2 = Factory.New<SupportIncident>();
			request2.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			SupportIncident request3 = Factory.New<SupportIncident>();
			request3.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			SupportIncident request4 = Factory.New<SupportIncident>();
			request4.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			SupportIncident request5 = Factory.New<SupportIncident>();
			request5.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			SupportIncident loneChangeRequest = Factory.New<SupportIncident>();
			loneChangeRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;

			project1.RelatedItems.Add(request1);
			project2.RelatedItems.Add(request2);
			project2.RelatedItems.Add(request3);
			project3.RelatedItems.Add(request4);
			project4.RelatedItems.Add(request5);

			NewWorkItem workItem = Factory.New<NewWorkItem>();
			CombineAssertions(delegate
			{
				AssertEquals("", workItem.ProjectIDs);
				AssertEquals("", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Add(loneChangeRequest);
			CombineAssertions(delegate
			{
				AssertEquals("", workItem.ProjectIDs);
				AssertEquals("", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.RemoveAll();
			workItem.RelatedItems.Add(request1);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ1", workItem.ProjectIDs);
				AssertEquals("Mr. PM1", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Add(request2);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ1, PRJ2", workItem.ProjectIDs);
				AssertEquals("Mr. PM1", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Add(request3);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ1, PRJ2", workItem.ProjectIDs);
				AssertEquals("Mr. PM1", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Add(request4);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ1, PRJ2, PRJ3", workItem.ProjectIDs);
				AssertEquals("Mr. PM1, Mr. PM2", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Add(request5);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ1, PRJ2, PRJ3, PRJ4", workItem.ProjectIDs);
				AssertEquals("Mr. PM1, Mr. PM2, Mr. PM3", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Remove(request1);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ2, PRJ3, PRJ4", workItem.ProjectIDs);
				AssertEquals("Mr. PM1, Mr. PM2, Mr. PM3", workItem.ProjectManagerNames);
			});

			workItem.RelatedItems.Remove(request4);
			CombineAssertions(delegate
			{
				AssertEquals("PRJ2, PRJ4", workItem.ProjectIDs);
				AssertEquals("Mr. PM1, Mr. PM3", workItem.ProjectManagerNames);
			});
		}

		EDIProject CreateNewProject(string id, string projectManagerCode)
		{
			GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, projectManagerCode);
			if (staff == null)
			{
				staff = Factory.New<GlbStaff>();
				staff.GS_Code = projectManagerCode;
				staff.GS_FullName = "Mr. " + projectManagerCode;
			}

			EDIProject result = Factory.New<EDIProject>();
			result.WKP_ProjectNumber = id;
			result.WKP_GS_NKProjectManager = staff.GS_Code;
			return result;
		}

		#endregion

		#region Email

		public void TestProfessionalServicesQuoteEmail()
		{
			GlbStaff someStaff = Factory.NewWithValidTestData<GlbStaff>();

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ProfessionalServicesQuote quote = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote.IM_GS_NKCustServiceContact = ZString.Empty;
			quote.RelatedItems.Add(workItem);
			ProcessTask task = workItem.WorkflowItems.AddNew();

			Factory.Save();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("No contact on ps quote", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			quote.IM_GS_NKCustServiceContact = someStaff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Contact on ps quote has no email address", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			someStaff.GS_EmailAddress = "test@edi.com.au";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(workItem.WKI_WorkItemNumber + " completed (PS Quote " + quote.IM_IncidentNumber + ")", Env.OutgoingMailManager.EmailsCreated[0].Subject);

			AssertContains("Work Item " + workItem.WKI_WorkItemNumber, Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("has been completed and is linked to", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("Professional Services Quote " + quote.IM_IncidentNumber, Env.OutgoingMailManager.EmailsCreated[0].Body);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Factory.Save();
			AssertEquals("No status change", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			workItem.RelatedItems.RemoveAll();
			Factory.Save();
			AssertEquals("Working and no related quote", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Closed, but no qutoe attached", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			ProfessionalServicesQuote quote2 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			quote2.IM_GS_NKCustServiceContact = someStaff.GS_Code;

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			quote.RelatedItems.Add(workItem);
			quote2.RelatedItems.Add(workItem);
			Factory.Save();
			AssertEquals("Working", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Closed so 2 emails - one per quote", 2, Env.OutgoingMailManager.EmailsCreated.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		#endregion

		#region Shelves handling

		public void TestHasCancelledShelfCheckInTasksForReleaseRing()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;

			foreach (var staffMember in new[] { "", "U01", "DAT" })
			{
				task.P9_GS_NKAssignedStaffMember = staffMember;

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				AssertEquals("HasCancelledShelfCheckInTasksForReleaseRing", false, workItem.HasCancelledShelfCheckInTasksForReleaseRing("ALP"));
				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				AssertEquals("HasCancelledShelfCheckInTasksForReleaseRing", false, workItem.HasCancelledShelfCheckInTasksForReleaseRing("ALP"));
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				AssertEquals("HasCancelledShelfCheckInTasksForReleaseRing", false, workItem.HasCancelledShelfCheckInTasksForReleaseRing("DPR"));
				AssertEquals("HasCancelledShelfCheckInTasksForReleaseRing", true, workItem.HasCancelledShelfCheckInTasksForReleaseRing("ALP"));

				task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", false, workItem.HasCancelledShelfCheckInTasksForReleaseRing("ALP"));
			}
		}

		public void TestHasClosedShelfCheckInTasksForReleaseRing()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;

			foreach (var staffMember in new[] { "", "U01", "DAT" })
			{
				task.P9_GS_NKAssignedStaffMember = staffMember;
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", false, workItem.HasClosedShelfCheckInTasksForReleaseRing("ALP"));
				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", false, workItem.HasClosedShelfCheckInTasksForReleaseRing("ALP"));
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", false, workItem.HasClosedShelfCheckInTasksForReleaseRing("DPR"));
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", true, workItem.HasClosedShelfCheckInTasksForReleaseRing("ALP"));

				task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", false, workItem.HasClosedShelfCheckInTasksForReleaseRing("ALP"));
			}
		}

		public void TestHasShelfCheckInTasksForReleaseRing()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			foreach (var staffMember in new[] { "", "U01", "DAT" })
			{
				task.P9_GS_NKAssignedStaffMember = staffMember;

				AssertEquals("HasCancelledShelfCheckInTasksForReleaseRing", false, workItem.HasShelfCheckInTasksForReleaseRing("ALP"));

				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", true, workItem.HasShelfCheckInTasksForReleaseRing("ALP"));
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", false, workItem.HasShelfCheckInTasksForReleaseRing("DPR"));

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", true, workItem.HasShelfCheckInTasksForReleaseRing("ALP"));

				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", true, workItem.HasShelfCheckInTasksForReleaseRing("DPR"));
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", false, workItem.HasShelfCheckInTasksForReleaseRing("ALP"));

				task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
				AssertEquals("HasClosedShelfCheckInTasksForReleaseRing", false, workItem.HasShelfCheckInTasksForReleaseRing("ALP"));
			}
		}

		public void TestHasOpenedShelfCheckInTasksForReleaseRing()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			foreach (var staffMember in new[] { "", "U01", "DAT" })
			{
				task.P9_GS_NKAssignedStaffMember = staffMember;

				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", false, workItem.HasOpenedShelfCheckInTasksForReleaseRing("ALP"));
				task.P9_Type = EDITaskTypes.TaskCheckin;
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", false, workItem.HasOpenedShelfCheckInTasksForReleaseRing("ALP"));
				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", false, workItem.HasOpenedShelfCheckInTasksForReleaseRing("DPR"));
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", true, workItem.HasOpenedShelfCheckInTasksForReleaseRing("ALP"));

				task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
				AssertEquals("HasOpenedShelfCheckInTasksForReleaseRing", false, workItem.HasOpenedShelfCheckInTasksForReleaseRing("ALP"));
			}
		}

		public void TestHasShelfCheckInTasks()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			task.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			AssertEquals("HasShelfCheckInTasks", false, workItem.HasShelfCheckInTasks);
			task.P9_Type = EDITaskTypes.TaskCheckin;
			AssertEquals("HasShelfCheckInTasks", false, workItem.HasShelfCheckInTasks);
			foreach (var checkin in ReleaseRingsLookup.CheckInTaskTypes)
			{
				task.P9_Type = checkin;
				AssertEquals("HasShelfCheckInTasks", true, workItem.HasShelfCheckInTasks);
			}

			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertEquals("HasShelfCheckInTasks", false, workItem.HasShelfCheckInTasks);
		}

		public void TestHasClosedCheckInTasks()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			var checkinTypeList = ReleaseRingsLookup.CheckInTaskTypes.ToArray();

			var otherTypeList = new string[] {
					EDITaskTypes.TaskCheckin, // in this list since it can't be a closed shelf checkin
				EDITaskTypes_ForTest.TaskCodingOfFunctionality };

			var otherStatusList = new string[] {
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Assigned,
				ProcessTaskStatusCodeList.Codes.Suspended,
				ProcessTaskStatusCodeList.Codes.Working
			};

			foreach (string status in otherStatusList)
			{
				task.P9_Status = status;
				foreach (string checkinType in checkinTypeList)
				{
					task.P9_Type = checkinType;
					AssertEquals("HasClosedCheckInTasks " + checkinType, false, workItem.HasClosedCheckInTasks);
				}
				foreach (string otherType in otherTypeList)
				{
					task.P9_Type = otherType;
					AssertEquals("HasClosedCheckInTasks", false, workItem.HasClosedCheckInTasks);
				}
			}

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			foreach (string checkinType in checkinTypeList)
			{
				task.P9_Type = checkinType;
				AssertEquals("HasClosedCheckInTasks " + checkinType, true, workItem.HasClosedCheckInTasks);
			}
			foreach (string otherType in otherTypeList)
			{
				task.P9_Type = otherType;
				AssertEquals("HasClosedCheckInTasks", false, workItem.HasClosedCheckInTasks);
			}

			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertEquals("HasClosedCheckInTasks", false, workItem.HasClosedCheckInTasks);
		}

		public void TestHasCheckInTasks()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();

			var otherTypeList = new string[] {
				EDITaskTypes_ForTest.TaskCodingOfFunctionality };

			task.P9_Type = EDITaskTypes.TaskCheckin;
			AssertEquals("HasCheckInTasks", true, workItem.HasCheckInTasks);

			foreach (string checkinType in ReleaseRingsLookup.CheckInTaskTypes)
			{
				task.P9_Type = checkinType;
				AssertEquals("HasCheckInTasks", true, workItem.HasCheckInTasks);
			}
			foreach (string otherType in otherTypeList)
			{
				task.P9_Type = otherType;
				AssertEquals("HasCheckInTasks", false, workItem.HasCheckInTasks);
			}

			task.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertEquals("HasClosedCheckInTasks", false, workItem.HasCheckInTasks);
		}

		public void TestGetClosedShelfCheckInTasksForReleaseRing()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var members = new[] { "", "U01", "DAT" };
			var idx = 0;

			foreach (string checkin in ReleaseRingsLookup.CheckInTaskTypes)
			{
				WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
				task.P9_Type = checkin;
				task.P9_GS_NKAssignedStaffMember = members[idx++ % 3];
				task.P9_Description = "Working task";
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			foreach (string ringCode in ReleaseRingsLookup.CheckInRingCodes)
			{
				var task = workItem.GetClosedShelfCheckInTasksForReleaseRing(ringCode);
				AssertEquals(ringCode, ReleaseRingsLookup.GetRingCodeByCheckInType(task.P9_Type));
			}

			WorkItemProcessTask taskAlp = workItem.GetClosedShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.ALP);

			foreach (WorkItemProcessTask task in workItem.WorkflowItems)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			}

			foreach (string ringCode in ReleaseRingsLookup.CheckInRingCodes)
			{
				AssertNull(workItem.GetClosedShelfCheckInTasksForReleaseRing(ringCode));
			}

			taskAlp.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNotNull(workItem.GetClosedShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.ALP));

			taskAlp.P9_Type = "COD";
			AssertNull(workItem.GetClosedShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.ALP));

			taskAlp.P9_Type = WorkItemProcessTask.ShelfsetTestTask;
			AssertNull(workItem.GetClosedShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.ALP));
		}

		public void TestTemplateWithActiveShelfTask_ShouldNotBeValidWithoutStaff()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = JobInvoicingConsumerTypes.WorkItem.Code;
			taskTemplate.P0_SubType4 = "ENH";

			var t1 = taskTemplate.WorkflowItems.AddNew();
			t1.P9_Status = "ASN";
			t1.P9_Type = EDITaskTypes.TaskActiveCheckin;
			var t2 = taskTemplate.WorkflowItems.AddNew();
			t2.P9_Status = "ASN";
			t2.P9_Type = "CHS";
			Factory.Save();

			NewWorkItem item = Factory.NewWithValidTestData<NewWorkItem>();
			item.WKI_ActivitySubtype = "ENH";
			Factory.Save();

			AssertEquals(2, item.WorkflowItems.Count);
			AssertEquals(expected: false, item.WorkflowItems[0].HasShelfToAdd);
			AssertEquals(expected: false, item.WorkflowItems[1].HasShelfToAdd);

			item.WorkflowItems[0].P9_GS_NKAssignedStaffMember = "ABC";
			item.WorkflowItems[1].P9_GS_NKAssignedStaffMember = "ABC";

			AssertEquals(2, item.WorkflowItems.Count);
			AssertEquals(expected: true, item.WorkflowItems[0].HasShelfToAdd);
			AssertEquals(expected: true, item.WorkflowItems[1].HasShelfToAdd);
		}

		#endregion

		#region TestIJobInvoicingPlugin Members

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<NewWorkItem>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<NewWorkItem>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		public void TestWorkItemActivitySubtypeCapitalization()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			tree.AddSystemChildren();
			const string ALL = CodeDescriptionBoolTreeNode.AllCode;
			var parent = tree.Find(ALL, ALL, ALL);
			tree.Add("TST", (NoResString)"Test Description", true, false, parent);
			tree.Add("TS2", (NoResString)"Test Description Two", true, false, parent);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var collection = new ActivitySubtypeAssignmentCollection();
			collection.AddNew("TST", "Assignment Description for TST", true);
			EDIDataRegistry.Instance.ActivitySubtypeAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();

			workItem.WKI_ActivitySubtype = "TS2";
			AssertHasError(workItem.WKI_ActivitySubtypeInfo, "Code must be entered in the Capitalized Development Change Types registry");

			workItem.WKI_ActivitySubtype = "TST";
			AssertNoErrors(workItem.WKI_ActivitySubtypeInfo);

			Factory.Save();

			collection = new ActivitySubtypeAssignmentCollection();
			collection.AddNew("TS2", "Assignment Description for TS2", true);
			EDIDataRegistry.Instance.ActivitySubtypeAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			workItem.WKI_ActivitySubtype = "TST";
			AssertNoErrors(workItem.WKI_ActivitySubtypeInfo);

			workItem.WKI_ActivitySubtype = "TS2";
			AssertNoErrors(workItem.WKI_ActivitySubtypeInfo);
		}

		public void TestDeletingTaskShouldCauseHasChanges()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ProcessTask task1 = workItem.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task2 = workItem.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_GS_NKAssignedStaffMember = "C";
			Factory.Save();
			Assert("Precondition", !workItem.HasChanges);

			workItem.WorkflowItems.Tasks.RemoveAndDelete(task1);
			Assert("Should set HasChanges to true", workItem.HasChanges);
		}

		public void TestCreateIncidentsFromRelatedIssues()
		{
			var log = Factory.New<EdiHelpErrorLog>();
			var occurrence = CreateOccurrenceWithLicence(log);
			var workItem = log.RelatedWorkItems.AddNew();

			AssertCollectionContains("Precondition", log, workItem.RelatedItems);
			AssertEquals(0, log.RelatedIncidents.Count);
			workItem.CreateIncidentsFromRelatedIssues();
			AssertEquals(1, log.RelatedIncidents.Count);
		}

		HelpErrorLogOccurrence CreateOccurrenceWithLicence(EdiHelpErrorLog log)
		{
			var factory = log.Factory;
			EDIOrgHeader org = factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = org.LicCompany.LicDatabases.AddNew();
			var clientCompany = factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;
			HelpErrorLogOccurrence result = log.Occurrences.AddNew();
			result.HO_LD = clientCompany.LCC_LD;
			result.HO_LCC = clientCompany.PK;
			return result;
		}

		public void TestFilteredRelatedItems()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			workItem.RelatedItems.Add(incident1);
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			workItem.RelatedItems.Add(incident2);
			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			ProfessionalServicesQuote quote1 = Factory.NewWithValidTestData<ProfessionalServicesQuote>();
			workItem.RelatedItems.Add(quote1);

			IWorkTaskRelatedItemSource itemSource = workItem;

			AssertEquals("Log.FilteredRelatedItems.Count", 3, itemSource.FilteredRelatedItems.Count);
			Assert("Log.FilteredRelatedItems should contain WI1.", itemSource.FilteredRelatedItems.Contains(incident1));
			Assert("Log.FilteredRelatedItems should contain WI2.", itemSource.FilteredRelatedItems.Contains(incident2));
			Assert("Log.FilteredRelatedItems should contain WI3.", itemSource.FilteredRelatedItems.Contains(quote1));

			itemSource.ShowOnlyNonClosedItems = true;
			AssertEquals("Log.FilteredRelatedItems.Count", 2, itemSource.FilteredRelatedItems.Count);
			Assert("Log.FilteredRelatedItems should contain WI1.", itemSource.FilteredRelatedItems.Contains(incident1));
			Assert("Log.FilteredRelatedItems should not contain WI2.", !itemSource.FilteredRelatedItems.Contains(incident2));
			Assert("Log.FilteredRelatedItems should contain WI3.", itemSource.FilteredRelatedItems.Contains(quote1));
		}

		public new void TestCanDeleteAndReasonMessage()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			AssertEquals(false, workItem.CanDelete);
			AssertEquals("Work Items should be cancelled with a reason rather than deleted directly.", workItem.ReasonForNotAbleToDelete);
		}

		public void TestAmnestyClosure()
		{
			var workItemToBeClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemToBeCancelled = Factory.NewWithValidTestData<NewWorkItem>();
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var asmPk = Guid.NewGuid();
				var clsPk = Guid.NewGuid();
				var method1Pk = Guid.NewGuid();
				var method2Pk = Guid.NewGuid();

				using (var cmd = conn.Command(@"
						insert into [Assembly] (E8_PK, E8_AssemblyName) values (@asmPk, 'Assembly')
						insert into TestClass (E2_PK, E2_E8, E2_TestClass) values (@clsPk, @asmPk, 'Class')
						insert into TestMethod (E6_PK, E6_E2, E6_MethodName, E6_DateCreated) values (@method1Pk, @clsPk, 'Method1', '2016-03-04 15:18:25.933')
						insert into TestMethod (E6_PK, E6_E2, E6_MethodName, E6_DateCreated) values (@method2Pk, @clsPk, 'Method2', '2008-04-29 08:42:13.793')
						insert into AmnestyFailures (AF_PK, AF_E6, AF_StartDate, AF_IM, AF_ExpiryDate) values (newid(), @method1Pk, getdate(), @workItem1Pk, NULL)
						insert into AmnestyFailures (AF_PK, AF_E6, AF_StartDate, AF_IM, AF_ExpiryDate) values (newid(), @method2Pk, getdate(), @workItem2Pk, NULL)"))
				{
					cmd.AddParameter("asmPk", SqlDbType.UniqueIdentifier, asmPk);
					cmd.AddParameter("clsPk", SqlDbType.UniqueIdentifier, clsPk);
					cmd.AddParameter("method1Pk", SqlDbType.UniqueIdentifier, method1Pk);
					cmd.AddParameter("method2Pk", SqlDbType.UniqueIdentifier, method2Pk);
					cmd.AddParameter("workItem1Pk", SqlDbType.UniqueIdentifier, workItemToBeClosed.PK.ToGuid());
					cmd.AddParameter("workItem2Pk", SqlDbType.UniqueIdentifier, workItemToBeCancelled.PK.ToGuid());
					cmd.ExecuteNonQuery();
				}

				var task1 = workItemToBeClosed.WorkflowItems.AddNew();
				task1.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				var task2 = workItemToBeCancelled.WorkflowItems.AddNew();
				task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();
				AssertEquals(DBNull.Value, conn.ExecuteScalar(string.Format("select AF_ExpiryDate from AmnestyFailures where AF_E6 = '{0}'", method1Pk)));
				AssertEquals(DBNull.Value, conn.ExecuteScalar(string.Format("select AF_ExpiryDate from AmnestyFailures where AF_E6 = '{0}'", method2Pk)));
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				Factory.Save();

				var now = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");

				// Closed WI
				var expiryDate = GetAmnestyExpiry(conn, method1Pk);
				Assert("First test amnesty was not closed. " + expiryDate.Value.ToString("o") + " <= " + now.ToString("o"), expiryDate <= now);

				// Cancelled WI
				expiryDate = GetAmnestyExpiry(conn, method2Pk);
				Assert("Second test amnesty was not closed. " + expiryDate.Value.ToString("o") + " <= " + now.ToString("o"), expiryDate <= now);
			}
		}

		public void TestAmnestyClosureOnlySetsExpiredWhenNotAlreadySet()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var asmPk = Guid.NewGuid();
				var clsPk = Guid.NewGuid();
				var methodPk = Guid.NewGuid();

				using (var cmd = conn.Command(@"
						insert into [Assembly] (E8_PK, E8_AssemblyName) values (@asmPk, 'Assembly')
						insert into TestClass (E2_PK, E2_E8, E2_TestClass) values (@clsPk, @asmPk, 'Class')
						insert into TestMethod (E6_PK, E6_E2, E6_MethodName, E6_DateCreated) values (@methodPk, @clsPk, 'Method1', '2016-03-04 15:18:25.933')
						insert into AmnestyFailures (AF_PK, AF_E6, AF_StartDate, AF_IM, AF_ExpiryDate) values (newid(), @methodPk, getdate(), @workItemPk, NULL)"))
				{
					cmd.AddParameter("asmPk", SqlDbType.UniqueIdentifier, asmPk);
					cmd.AddParameter("clsPk", SqlDbType.UniqueIdentifier, clsPk);
					cmd.AddParameter("methodPk", SqlDbType.UniqueIdentifier, methodPk);
					cmd.AddParameter("workItemPk", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					cmd.ExecuteNonQuery();
				}

				var task1 = workItem.WorkflowItems.AddNew();
				task1.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();
				AssertNull(GetAmnestyExpiry(conn, methodPk));
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				var now = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");

				var expiryDate = GetAmnestyExpiry(conn, methodPk);
				Assert("First test amnesty was not closed. " + expiryDate.Value.ToString("o") + " <= " + now.ToString("o"), expiryDate.Value <= now);

				var task2 = workItem.WorkflowItems.AddNew();
				task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				var expiryDate2 = GetAmnestyExpiry(conn, methodPk);

				AssertEquals("Date should equal the first date", expiryDate, expiryDate2);
			}
		}

		DateTime? GetAmnestyExpiry(DbConnection connection, Guid methodPk)
		{
			using (var cmd = connection.Command("select AF_ExpiryDate from AmnestyFailures where AF_E6 = @methodPk"))
			{
				cmd.AddParameter("methodPk", SqlDbType.UniqueIdentifier, methodPk);
				var expiryDateObject = cmd.ExecuteScalar();
				return (expiryDateObject != DBNull.Value) ? (DateTime?)expiryDateObject : null;
			}
		}

		public void TestCancelPreApprovedWorkItemsOnAspectsWhenWorkItemClosedOrCancelled()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemToBeClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemToBeCancelled = Factory.NewWithValidTestData<NewWorkItem>();
			var preApproval = Guid.NewGuid();
			var preApprovalClose = Guid.NewGuid();
			var preApprovalCancel = Guid.NewGuid();

			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var command = @"
					DECLARE @UserPK uniqueidentifier = NEWID()
					DECLARE @AspectPK uniqueidentifier = NEWID()

					INSERT INTO [User] (U1_PK, U1_Name) Values (@UserPK, 'Username')
					INSERT INTO Aspect (AS_PK, AS_Name, AS_Capability, AS_DataExtratorType, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES (@AspectPK, 'AspectName', 'CAP', '', 1, 0, 1)
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApproval, @AspectPK, @WorkItem, @UserPK, GETDATE())
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApprovalClose, @AspectPK, @WorkItemClose, @UserPK, GETDATE())
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApprovalCancel, @AspectPK, @WorkItemCancel, @UserPK, GETDATE())
				";
				using (var cmd = conn.Command(command))
				{
					cmd.AddParameter("WorkItem", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					cmd.AddParameter("WorkItemClose", SqlDbType.UniqueIdentifier, workItemToBeClosed.PK.ToGuid());
					cmd.AddParameter("WorkItemCancel", SqlDbType.UniqueIdentifier, workItemToBeCancelled.PK.ToGuid());
					cmd.AddParameter("PreApproval", SqlDbType.UniqueIdentifier, preApproval);
					cmd.AddParameter("PreApprovalClose", SqlDbType.UniqueIdentifier, preApprovalClose);
					cmd.AddParameter("PreApprovalCancel", SqlDbType.UniqueIdentifier, preApprovalCancel);
					cmd.ExecuteNonQuery();
				}

				var taskToClose = workItemToBeClosed.WorkflowItems.AddNew();
				taskToClose.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				taskToClose.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				var taskToCancel = workItemToBeCancelled.WorkflowItems.AddNew();
				taskToCancel.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				taskToCancel.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var before = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");
				AssertNull(GetAP_DateInactive(conn, preApproval));
				AssertNull(GetAP_DateInactive(conn, preApprovalClose));
				AssertNull(GetAP_DateInactive(conn, preApprovalCancel));

				taskToClose.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				AssertNull(GetAP_DateInactive(conn, preApproval));
				AssertNotNull(GetAP_DateInactive(conn, preApprovalClose));
				AssertNull(GetAP_DateInactive(conn, preApprovalCancel));

				taskToCancel.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				Factory.Save();

				AssertNull(GetAP_DateInactive(conn, preApproval));
				AssertNotNull(GetAP_DateInactive(conn, preApprovalClose));
				AssertNotNull(GetAP_DateInactive(conn, preApprovalCancel));

				var closed = GetAP_DateInactive(conn, preApprovalClose).Value;
				var cancelled = GetAP_DateInactive(conn, preApprovalCancel).Value;

				var after = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");

				AssertLessThanOrEqualTo("Before timestamp should be less than Closed timestamp. " + before.ToString("o") + " <= " + closed.ToString("o"), before, closed);
				AssertLessThan("Closed timestamp should be less than Cancelled timestamp. " + closed.ToString("o") + " <= " + cancelled.ToString("o"), closed, cancelled);
				AssertLessThanOrEqualTo("Cancelled timestamp should be less than After timestamp. " + cancelled.ToString("o") + " <= " + after.ToString("o"), cancelled, after);
			}
		}

		public void TestCancelPreApprovedWorkItemsOnAspectsWithMultpleApprovals()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemToBeClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var preApproval = Guid.NewGuid();
			var preApprovalClose1 = Guid.NewGuid();
			var preApprovalClose2 = Guid.NewGuid();

			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var command = @"
					DECLARE @UserPK uniqueidentifier = NEWID()
					DECLARE @Aspect1PK uniqueidentifier = NEWID()
					DECLARE @Aspect2PK uniqueidentifier = NEWID()

					INSERT INTO [User] (U1_PK, U1_Name) Values (@UserPK, 'Username')
					INSERT INTO Aspect (AS_PK, AS_Name, AS_Capability, AS_DataExtratorType, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES (@Aspect1PK, 'Aspect1', 'CAP', '', 1, 0, 1)
					INSERT INTO Aspect (AS_PK, AS_Name, AS_Capability, AS_DataExtratorType, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES (@Aspect2PK, 'Aspect2', 'ABC', '', 1, 0, 1)
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApproval, @Aspect1PK, @WorkItem, @UserPK, GETDATE())
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApprovalClose1, @Aspect1PK, @WorkItemClose, @UserPK, GETDATE())
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApprovalClose2, @Aspect2PK, @WorkItemClose, @UserPK, GETDATE())
				";
				using (var cmd = conn.Command(command))
				{
					cmd.AddParameter("WorkItem", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					cmd.AddParameter("WorkItemClose", SqlDbType.UniqueIdentifier, workItemToBeClosed.PK.ToGuid());
					cmd.AddParameter("PreApproval", SqlDbType.UniqueIdentifier, preApproval);
					cmd.AddParameter("PreApprovalClose1", SqlDbType.UniqueIdentifier, preApprovalClose1);
					cmd.AddParameter("PreApprovalClose2", SqlDbType.UniqueIdentifier, preApprovalClose2);
					cmd.ExecuteNonQuery();
				}

				var taskToClose = workItemToBeClosed.WorkflowItems.AddNew();
				taskToClose.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				taskToClose.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var before = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");
				AssertNull(GetAP_DateInactive(conn, preApproval));
				AssertNull(GetAP_DateInactive(conn, preApprovalClose1));
				AssertNull(GetAP_DateInactive(conn, preApprovalClose2));

				taskToClose.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				AssertNull(GetAP_DateInactive(conn, preApproval));
				AssertNotNull(GetAP_DateInactive(conn, preApprovalClose1));
				AssertNotNull(GetAP_DateInactive(conn, preApprovalClose2));

				var closed1 = GetAP_DateInactive(conn, preApprovalClose1).Value;
				var closed2 = GetAP_DateInactive(conn, preApprovalClose2).Value;

				var after = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");

				AssertLessThanOrEqualTo("Before timestamp should be less than Closed timestamp. " + before.ToString("o") + " <= " + closed1.ToString("o"), before, closed1);
				AssertEquals("Closed timestamps should be the same. " + closed1.ToString("o") + " = " + closed2.ToString("o"), closed1, closed2);
				AssertLessThanOrEqualTo("Closed timestamp should be less than After timestamp. " + closed2.ToString("o") + " <= " + after.ToString("o"), closed2, after);
			}
		}

		public void TestCancelPreApprovedWorkItemsOnAspectsOnlySetsInactiveWhenNotAlreadySet()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var preApproval = Guid.NewGuid();

			using (var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				var command = @"
					DECLARE @UserPK uniqueidentifier = NEWID()
					DECLARE @AspectPK uniqueidentifier = NEWID()

					INSERT INTO [User] (U1_PK, U1_Name) Values (@UserPK, 'Username')
					INSERT INTO Aspect (AS_PK, AS_Name, AS_Capability, AS_DataExtratorType, AS_IsActive, AS_IsFromXML, AS_HasBaseline) VALUES (@AspectPK, 'AspectName', 'CAP', '', 1, 0, 1)
					INSERT INTO AspectPreApproval (AP_PK, AP_AS, AP_WorkItem, AP_U1, AP_DatePreApproved) VALUES (@PreApproval, @AspectPK, @WorkItem, @UserPK, GETDATE())
				";
				using (var cmd = conn.Command(command))
				{
					cmd.AddParameter("WorkItem", SqlDbType.UniqueIdentifier, workItem.PK.ToGuid());
					cmd.AddParameter("PreApproval", SqlDbType.UniqueIdentifier, preApproval);
					cmd.ExecuteNonQuery();
				}

				var task = workItem.WorkflowItems.AddNew();
				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				var before = (DateTime)conn.ExecuteScalar("select cast(getdate() as datetime)");
				AssertNull(GetAP_DateInactive(conn, preApproval));

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				var inactiveDate = GetAP_DateInactive(conn, preApproval);

				AssertNotNull(inactiveDate);

				var task2 = workItem.WorkflowItems.AddNew();
				task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				Factory.Save();

				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();

				var inactiveDate2 = GetAP_DateInactive(conn, preApproval);
				AssertEquals("Date should equal the first date", inactiveDate, inactiveDate2);
			}
		}

		DateTime? GetAP_DateInactive(DbConnection conn, Guid preApprovalPK)
		{
			using (var cmd = conn.Command("select AP_DateInactive from AspectPreApproval where AP_PK = @PreApprovalPK"))
			{
				cmd.AddParameter("PreApprovalPK", SqlDbType.UniqueIdentifier, preApprovalPK);
				var result = cmd.ExecuteScalar();
				return result == DBNull.Value ? null : (DateTime?)result;
			}
		}

		public void TestHasAllMandatoryReviewTasksAndTasksAreComplete()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			var system = helper.CreateSystem(Factory, "WKI");

			var list = new CodeDescriptionPairList();
			list.AddPair("FOO", "fooey");
			list.AddPair("BAR", "bar");
			EDIDataRegistry.Instance.TasksMandatoryInShelfCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			AssertEquals(false, workItem.AreCheckinPrerequisiteTasksOfAllRequiredTypes(null));
			AssertEquals(true, workItem.AreAllCheckinPrerequisiteTasksComplete(null));

			WorkItemProcessTask designReview = workItem.WorkflowItems.AddNew();
			designReview.P9_Type = "FOO";
			designReview.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			WorkItemProcessTask codeReview = workItem.WorkflowItems.AddNew();
			codeReview.P9_Type = "BAR";
			codeReview.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertEquals(true, workItem.AreCheckinPrerequisiteTasksOfAllRequiredTypes(null));
			AssertEquals(false, workItem.AreAllCheckinPrerequisiteTasksComplete(null));

			codeReview.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(true, workItem.AreCheckinPrerequisiteTasksOfAllRequiredTypes(null));
			AssertEquals(true, workItem.AreAllCheckinPrerequisiteTasksComplete(null));

			var jobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			jobHeader.ProcessHeaders.AddNew().FH_CompletionStatement = "foo";
			WorkItemProcessTask checkin = workItem.WorkflowItems.AddNew();
			checkin.P9_FH_ProcessHeader = ZGuid.Empty;
			checkin.P9_Type = EDITaskTypes.TaskCheckin;
			AssertNull("ProcessHeader", checkin.ProcessHeader);
			AssertEquals(true, workItem.AreCheckinPrerequisiteTasksOfAllRequiredTypes(checkin));
			AssertEquals(true, workItem.AreAllCheckinPrerequisiteTasksComplete(checkin));
		}

		public void TestGetRecentReviewTaskDateTimeUTC()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("CRF", "code");
			list.AddPair("FRU", "func");
			list.AddPair("DRV", "design");
			list.AddPair("XRV", "final");
			EDIDataRegistry.Instance.TasksMandatoryInShelfCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

			WorkItemProcessTask codingTask = workItem.WorkflowItems.AddNew();
			codingTask.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			codingTask.TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2012, 8, 14, 9, 30, 0));

			Factory.Save();
			AssertEquals(1, workItem.WorkflowItems.Count);
			AssertEquals(ZDateTime.Empty, workItem.GetRecentReviewTaskDateTimeUTC());

			WorkItemProcessTask codeReviewTask = workItem.WorkflowItems.AddNew();
			codeReviewTask.P9_Type = "CRF";
			codeReviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			codeReviewTask.TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2012, 8, 14, 9, 30, 0));

			WorkItemProcessTask functionalReviewTask = workItem.WorkflowItems.AddNew();
			functionalReviewTask.P9_Type = "FRU";
			functionalReviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			functionalReviewTask.TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2012, 8, 14, 14, 30, 0));

			WorkItemProcessTask finalReviewTask = workItem.WorkflowItems.AddNew();
			finalReviewTask.P9_Type = "XRV";
			finalReviewTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			finalReviewTask.TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2012, 8, 14, 15, 30, 0));

			Factory.Save();
			AssertEquals(4, workItem.WorkflowItems.Count);
			AssertEquals(workItem.GetRecentReviewTaskDateTimeUTC(), functionalReviewTask.P9_ActualDateForBinding.ToUtcZDateTime());
		}

		public void TestHasDuplicateDatSubmissions_IncludingAcrossMultipleWorkflows()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "DV1";
			staff1.GS_LoginName = "dev.one";

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			var system = helper.CreateSystem(Factory, "WKI");

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var processJobHeader = ProcessJobHeaderProvider.GetForParent(workItem, Factory);
			var workflow1 = processJobHeader.ProcessHeaders[0];
			var workflow2 = processJobHeader.ProcessHeaders.AddNew();

			var task1InWorkflow1 = workItem.WorkflowItems.AddNew();
			task1InWorkflow1.P9_FH_ProcessHeader = workflow1.PK;
			task1InWorkflow1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task1InWorkflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1InWorkflow1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			var task2InWorkflow1 = workItem.WorkflowItems.AddNew();
			task2InWorkflow1.P9_FH_ProcessHeader = workflow1.PK;
			task2InWorkflow1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task2InWorkflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2InWorkflow1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			var task1InWorkflow2 = workItem.WorkflowItems.AddNew();
			task1InWorkflow2.P9_FH_ProcessHeader = workflow2.PK;
			task1InWorkflow2.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task1InWorkflow2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1InWorkflow2.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			var task2InWorkflow2 = workItem.WorkflowItems.AddNew();
			task2InWorkflow2.P9_FH_ProcessHeader = workflow2.PK;
			task2InWorkflow2.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task2InWorkflow2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2InWorkflow2.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			AssertEquals(false, workItem.HasDuplicateDatSubmissions);

			var s1 = "http://example.com/pullrequest/1";
			var s2 = "http://example.com/pullrequest/2";
			var s3 = "http://example.com/pullrequest/3";
			var s4 = "http://example.com/pullrequest/4";

			task1InWorkflow1.P9_Type = "SH0";
			task1InWorkflow1.P9_NotesAsString = s1;
			task2InWorkflow1.P9_Type = "SH0";
			task2InWorkflow1.P9_NotesAsString = s1;
			task1InWorkflow2.P9_Type = "SH0";
			task1InWorkflow2.P9_NotesAsString = s1;
			task2InWorkflow2.P9_Type = "SH0";
			task2InWorkflow2.P9_NotesAsString = s1;
			AssertEquals(true, workItem.HasDuplicateDatSubmissions);

			task1InWorkflow1.P9_NotesAsString = s1;
			task2InWorkflow1.P9_NotesAsString = s2;
			task1InWorkflow2.P9_NotesAsString = s3;
			task2InWorkflow2.P9_NotesAsString = s4;
			AssertEquals(false, workItem.HasDuplicateDatSubmissions);

			task2InWorkflow1.P9_NotesAsString = s1;
			task2InWorkflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(false, workItem.HasDuplicateDatSubmissions);

			task2InWorkflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(true, workItem.HasDuplicateDatSubmissions);

			task2InWorkflow1.P9_NotesAsString = s2;

			task1InWorkflow1.P9_Type = "CH0";
			task2InWorkflow1.P9_Type = "CH0";
			task1InWorkflow2.P9_Type = "CH0";
			task2InWorkflow2.P9_Type = "CH0";
			AssertEquals(false, workItem.HasDuplicateDatSubmissions);

			task1InWorkflow1.P9_NotesAsString = s1;
			task2InWorkflow1.P9_NotesAsString = s1;
			task1InWorkflow2.P9_NotesAsString = s1;
			task2InWorkflow2.P9_NotesAsString = s1;
			AssertEquals(true, workItem.HasDuplicateDatSubmissions);

			task1InWorkflow2.P9_NotesAsString = s3;
			task2InWorkflow2.P9_NotesAsString = s4;
			AssertEquals(true, workItem.HasDuplicateDatSubmissions);

			task2InWorkflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(false, workItem.HasDuplicateDatSubmissions);

			task2InWorkflow1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2InWorkflow1.P9_NotesAsString = s2;
			AssertEquals(false, workItem.HasDuplicateDatSubmissions);
		}

		public void TestProjectIDsOfRelatedItemProjects()
		{
			EDIProject project1 = CreateNewProject("PRJ1", "PM1");
			EDIProject project2 = CreateNewProject("PRJ2", "PM2");
			EDIProject project3 = CreateNewProject("PRJ3", "PM2");
			NewWorkItem workItemOne = Factory.New<NewWorkItem>();
			AssertEquals("", workItemOne.ProjectIDs);

			workItemOne.RelatedItems.Add(project1);
			workItemOne.RelatedItems.Add(project2);
			workItemOne.RelatedItems.Add(project3);
			AssertEquals("PRJ1, PRJ2, PRJ3", workItemOne.ProjectIDs);

			NewWorkItem workItemTwo = Factory.New<NewWorkItem>();
			AssertEquals("", workItemTwo.ProjectIDs);

			workItemTwo.RelatedItems.Add(project2);
			AssertEquals("PRJ2", workItemTwo.ProjectIDs);

			EDIProject project4 = CreateNewProject("PRJ4", "PM3");
			NewWorkItem workItemThree = Factory.New<NewWorkItem>();
			SupportIncident featureRequest1 = Factory.New<SupportIncident>();
			project4.RelatedItems.Add(featureRequest1);

			workItemThree.RelatedItems.Add(featureRequest1);
			workItemThree.RelatedItems.Add(project3);
			AssertEquals("PRJ4, PRJ3", workItemThree.ProjectIDs);
		}

		[TestDate(2014, 10, 28, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestJobCloseDateUtc()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItemOpen = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemCancelled = Factory.NewWithValidTestData<NewWorkItem>();
			var task0 = workItemOpen.WorkflowItems.AddNew();
			var task1 = workItemClosed.WorkflowItems.AddNew();
			var task2 = workItemCancelled.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, workItemOpen.WKI_Status);
			AssertEquals("JobClosedDateUtc of open WI", ZDateTime.Empty, workItemOpen.JobCloseDateUtc);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, workItemClosed.WKI_Status);
			AssertEquals("JobClosedDateUtc of closed WI", TestDateAttribute.Date, workItemClosed.JobCloseDateUtc);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Cancelled, workItemCancelled.WKI_Status);
			AssertEquals("JobClosedDateUtc of cancelled WI", TestDateAttribute.Date, workItemCancelled.JobCloseDateUtc);
		}

		[TestDate(2014, 10, 28, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRelatedItems_AddingOrRemovingIssueSetsFixedDate()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKI");

			var workItemClosed = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemCancelled = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItemClosed.WorkflowItems.AddNew();
			var task2 = workItemCancelled.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var log = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			Factory.Save();

			var expectedFixedDate = TestDateAttribute.Date;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			workItemClosed.RelatedItems.Add(log);
			workItemCancelled.RelatedItems.Add(log);
			AssertEquals("Fixed date", expectedFixedDate, log.HE_FixedDate);

			workItemClosed.RelatedItems.Remove(log);
			AssertEquals("Precondition", 1, log.RelatedWorkItems.Count);
			AssertEquals("Fixed date is not removed", expectedFixedDate, log.HE_FixedDate);

			workItemCancelled.RelatedItems.Remove(log);
			AssertEquals("Precondition", 0, log.RelatedWorkItems.Count);
			AssertEquals("Fixed date is removed", ZDateTime.Empty, log.HE_FixedDate);
		}

		[TestDate(2014, 10, 28, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRelatedItems_ReOpeningResetsFixedDateOnAttachedIssues()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			var system = helper.CreateSystem(Factory, "WKI");

			var workItemFromClosedToWorking = Factory.NewWithValidTestData<NewWorkItem>();
			var workItemFromCancelledToWorking = Factory.NewWithValidTestData<NewWorkItem>();
			var task1 = workItemFromClosedToWorking.WorkflowItems.AddNew();
			var task2 = workItemFromCancelledToWorking.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			var expectedFixedDate = TestDateAttribute.Date;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			var log1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			workItemFromClosedToWorking.RelatedItems.Add(log1);
			AssertEquals("Precondition", expectedFixedDate, log1.HE_FixedDate);

			var task3 = workItemFromClosedToWorking.WorkflowItems.AddNew();
			AssertEquals("Fixed date is removed because closed WI got re-opened", ZDateTime.Empty, log1.HE_FixedDate);

			var log2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			workItemFromCancelledToWorking.RelatedItems.Add(log2);
			AssertEquals("Fixed date from cancelled WI", expectedFixedDate, log2.HE_FixedDate);

			var task4 = workItemFromCancelledToWorking.WorkflowItems.AddNew();
			AssertEquals("Fixed date is removed because cancelled WI got re-opened", ZDateTime.Empty, log2.HE_FixedDate);
		}

		[TestDate(2018, 6, 25, 0, 0, 0)]
		public void TestAgreedDeliveryDateQueryPerformance()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = helper.CreateSystem(Factory, "WKI");
			var bucket = helper.CreateBucket(system, "charlie");
			var buffer = helper.CreateBucket(system, "willy");

			var workItem1 = Factory.New<WorkItem>();
			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(workItem1, Factory);
			workItem1.WKI_Priority = ReleaseRings.Codes.ALP;
			workItem1.WKI_ActivitySubtype = "CFM";
			workItem1.WKI_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var workItem2 = Factory.New<WorkItem>();
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(workItem2, Factory);
			workItem2.WKI_Priority = ReleaseRings.Codes.GP1;
			workItem2.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;

			var workItem3 = Factory.New<WorkItem>();
			var jobHeader3 = ProcessJobHeaderProvider.GetForParent(workItem3, Factory);
			workItem3.WKI_Priority = ReleaseRings.Codes.DPR;
			workItem3.WKI_ActivitySubtype = "MMR";
			workItem3.WKI_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var workflow11 = jobHeader1.ProcessHeaders.AddNew();
			workflow11.FH_FC_CurrentComponent = bucket.PK;
			workflow11.FH_AgreedDeliveryDate = new ZDateTime(2018, 6, 24, 0, 0, 0);
			var workflow12 = jobHeader1.ProcessHeaders.AddNew();
			workflow12.FH_FC_CurrentComponent = buffer.PK;
			workflow12.FH_AgreedDeliveryDate = new ZDateTime(2018, 6, 25, 0, 0, 0);

			var workflow21 = jobHeader2.ProcessHeaders.AddNew();
			workflow21.FH_FC_CurrentComponent = bucket.PK;
			workflow21.FH_AgreedDeliveryDate = new ZDateTime(2018, 6, 26, 0, 0, 0);
			var workflow22 = jobHeader2.ProcessHeaders.AddNew();
			workflow22.FH_FC_CurrentComponent = buffer.PK;
			workflow22.FH_AgreedDeliveryDate = new ZDateTime(2018, 6, 25, 0, 0, 0);

			var workflow31 = jobHeader3.ProcessHeaders.AddNew();
			workflow31.FH_FC_CurrentComponent = bucket.PK;
			workflow31.FH_AgreedDeliveryDate = new ZDateTime(2018, 6, 25, 0, 0, 0);
			var workflow32 = jobHeader3.ProcessHeaders.AddNew();
			workflow32.FH_FC_CurrentComponent = buffer.PK;
			workflow32.FH_AgreedDeliveryDate = new ZDateTime(2018, 6, 25, 0, 0, 0);

			Factory.Save();

			AssertEquals(ZGuid.Empty, workflow11.FH_P0_Template);
			AssertNotEquals(ZGuid.Empty, workflow11.FH_FH_ParentHeader);

			#region Query SQL
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddFilterAndZSQLParameterCollection(string.Format(@"(
	FH_FH_ParentHeader is not NULL 
	AND
	FH_P0_Template is NULL 
	AND
	FH_IsActive = 1 
	AND
	(
		FH_FC_CurrentComponent = '{0}' 
		OR
		(
			FH_ParentId IN 
			(
				SELECT WKI_PK FROM dbo.WorkItem WHERE 
				(
					WKI_ActivitySubtype in 
					(
						'CFM', 'MMR'
					)
				)
				AND
				(
					WKI_Status in 
					(
						'ASN', 'OPN', 'SUS', 'WRK'
					)
				)
			)
		)
	)
)
AND
(
	(
		(
			FH_ParentId IN 
			(
				SELECT WKI_PK FROM dbo.WorkItem WHERE WKI_Priority = 'GP1' 
				AND
				(
					WKI_Status in 
					(
						'ASN', 'OPN', 'SUS', 'WRK'
					)
				)
			)
		)
		AND
		(
			FH_AgreedDeliveryDate <= '2018-06-26 00:00:00.000' 
			OR
			(
				FH_FH_ParentHeader is not NULL 
				AND
				FH_AgreedDeliveryDate is NULL 
				AND
				FH_FH_ParentHeader IN 
				(
					SELECT FH_PK FROM dbo.ProcessHeader WHERE FH_AgreedDeliveryDate <= '2018-06-26 00:00:00.000'
				)
			)
		)
	)
	AND
	(
		FH_AgreedDeliveryDate >= '2018-06-24 00:00:00.000' 
		AND
		FH_AgreedDeliveryDate <= '2018-06-26 00:00:00.000' 
		OR
		(
			FH_FH_ParentHeader is not NULL 
			AND
			FH_AgreedDeliveryDate is NULL 
			AND
			(
				FH_FH_ParentHeader IN 
				(
					SELECT FH_PK FROM dbo.ProcessHeader WHERE FH_AgreedDeliveryDate >= '2018-06-24 00:00:00.000' 
					AND
					FH_AgreedDeliveryDate <= '2018-06-26 00:00:00.000'
				)
			)
		)
	)
)
OR
(
	FH_ParentId IN 
	(
		SELECT WKI_PK FROM dbo.WorkItem WHERE WKI_Priority = 'DPR' 
		AND
		(
			WKI_Status in 
			(
				'ASN', 'OPN', 'SUS', 'WRK'
			)
		)
	)
) -- this is a query", bucket.PK), new ZSqlParameterCollection());
			#endregion

			Factory.Save();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var newFactory = Factory.CreateNewFactory();
				newFactory.Load<ProcessHeader>(query);

				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("-- this is a query"));
				var planalyser = new QueryPlanalyzer(queryPlans?.Item2.Last());

				CombineAssertions(() =>
				{
					AssertCollectionNotContains(
						"We expected no table scans on ProcessHeader nor WorkItem, and yet...",
						new[] { ProcessHeaderSchema.Constants.TableName, WorkItemSchema.Constants.TableName },
						planalyser.TableScans.Select(x => x.TableName));

					AssertCollectionContains(
						"Index seeks on NR_RX__WKI_PK",
						"NR_RX__WKI_PK",
						planalyser.IndexSeeks.Select(x => x.IndexName));

					AssertCollectionContains(
						"Index seeks on NR_RX__FH_PK_2",
						"NR_RX__FH_PK_2",
						planalyser.IndexSeeks.Select(x => x.IndexName));

					AssertContainsExactElementsInAnyOrder(
						"We expected no RID lookups, and yet...",
						Array.Empty<Tuple<string, string>>(),
						planalyser.RowIDLookups.SelectMany(x => x.OutputList).Select(c => Tuple.Create(c.TableName, c.ColumnName)));
				});
			}
		}

		protected override IEnumerable<string> ExpectedSupportedRelatedItemModules =>
			base.ExpectedSupportedRelatedItemModules
				.Except(new[] { ModuleIDs.CustomerServiceTicket.Name }) // CSTs don't support the tree view, so can't be attached to an EDIWorkItem without creating an EDIWorkRequest subclass and implementing the interfaces as needed.
				.Concat(new[] { "IssueManager", "SupportIncident", "ProfessionalServicesQuote" });

		public new void TestDocManagerInfo()
		{
			var workItem = Factory.New<NewWorkItem>();

			Assert(workItem.DocManagerInfo is NewWorkItemDocManagerInfo);
		}

		public void TestGetAllEConversationBroadcastMessages()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			AssertEquals(false, workItem.HasEConversationBroadcastMessages());
			AssertNull(workItem.GetAllEConversationBroadcastMessages());

			Factory.Save();
			AssertEquals(false, workItem.HasEConversationBroadcastMessages());
			AssertNotNull(workItem.GetAllEConversationBroadcastMessages());
			AssertEquals(0, workItem.GetAllEConversationBroadcastMessages().Count);

			workItem.Conversation.Messages.AddNew(null, "broadcast message", false, false, true);
			workItem.Conversation.Messages.AddNew(null, "non broadcast message", false, false, false);
			AssertEquals(true, workItem.HasEConversationBroadcastMessages());
			AssertEquals(1, workItem.GetAllEConversationBroadcastMessages().Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<NewWorkItem>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<NewWorkItem>();
		}

		protected new NewWorkItem CachedWorkItem
		{
			get { return (NewWorkItem)CachedBusinessObject; }
		}

		#endregion
	}

	[TestedType(typeof(NewWorkItem))]
	sealed class NewWorkItemRelatedItemTest : EDIWorkItemRelatedItemTestCase
	{
	}
}
