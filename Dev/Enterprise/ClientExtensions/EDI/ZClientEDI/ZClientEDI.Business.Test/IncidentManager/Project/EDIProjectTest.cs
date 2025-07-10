using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(EDIProject))]
	class EDIProjectTest : ProjectTest
	{
		public void TestReloadDoesntCloseProject()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			var task1 = project.WorkflowItems.AddNew();
			project.WorkflowItems.AddNew();
			var exceptions = project.WorkflowItems.ExceptionsIncludingRelated;

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			string message = "Log should {0} contain '{1}'. Log:\r\n" + project.LogText;
			Assert(string.Format(message, "", "Project Created"), project.LogText.Contains("Project Created"));
			Assert(string.Format(message, "not", "Closed as Completed"), !project.LogText.Contains("Closed as Completed"));
			Assert(string.Format(message, "not", "Re-Opened"), !project.LogText.Contains("Re-Opened"));
		}

		public void TestDelete_ProjectWithWorkflowItem()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			project.WorkflowItems.AddNew();
			Factory.Save();

			var clientWorkProjectPK = project.ClientWorkProject.PK;
			project.Delete();
			AssertEquals("Should be the same PK. If different PK, it has created an unwanted instance", clientWorkProjectPK, project.ClientWorkProject.PK);
		}

		public void TestISendEmailSourceProperties()
		{
			AssertEquals(IncidentConstants.ImplementationDefaultReplyToName, ((ISendEmailSource)Project).DefaultFromDisplayName);
			AssertEquals(EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value, ((ISendEmailSource)Project).OverridingDefaultFromEmailAddress);
			AssertEquals(Project.WKP_ProjectNumber + ": Installation Update", ((ISendEmailSource)Project).EmailSubject);
		}

		public void TestLicenceOrganisation()
		{
			var project = Factory.New<EDIProject>();
			AssertNull(((IClientOrgLicenceProvider)project).LicenceOrganisation);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			project.WKP_OA_ClientAddress = org.MainAddress.PK;

			AssertEquals(org, ((IClientOrgLicenceProvider)project).LicenceOrganisation);
		}

		public new void TestInvoicingSupporter()
		{
			var project = Factory.New<Project>();
			IJobInvoicingPlugIn job = project;
			AssertType(typeof(EDIProjectInvoicingSupporter), job.InvoicingSupporter);
		}

		#region Number Fountain Issue

		public void TestSaveDoesntUseTwoNumberFountainNumbers()
		{
			EDIProject item = Factory.NewWithValidTestData<EDIProject>();
			Factory.Save();

			EDIProject item2 = Factory.NewWithValidTestData<EDIProject>();
			Factory.Save();

			ZInt number = Convert.ToInt32(item.JobNumber.Substring(3));
			ZInt number2 = Convert.ToInt32(item2.JobNumber.Substring(3));

			AssertEquals(number + 1, number2);
		}

		#endregion

		#region Business Object Overrides

		public new void TestDefaultValues()
		{
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, Project.WKP_Status);
		}

		#endregion

		#region Properties

		public void TestLicence()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "SAMUEL";
			org.OH_FullName = "Test Organisation";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			LicenceCompany company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "COM";
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;

			LicenceHeader header = Factory.New<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database.PK;
			Factory.Save();

			EDIProject project = (EDIProject)GetNewBusinessObject();
			project.WKP_OA_ClientAddress = org.MainAddress.PK;
			project.LicenceHeaderPK = header.PK;
			AssertEquals(header, project.Licence);
			Assert(!project.IsRegisteredEditableChildObject(header));

			project.LicenceHeaderPK = ZGuid.Empty;
			AssertNull(project.Licence);
			Assert(!project.IsRegisteredEditableChildObject(header));
			AssertNotEquals(ZGuid.Empty, project.WKP_OA_ClientAddress);
		}

		public void TestSiteLiveDate()
		{
			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = true;

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var org = lic.Company.Header;

			Factory.Save();

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.ChangeClientOrganisation(org);

			AssertEquals(ZDateTime.Empty, project.SiteLiveDate);
			AssertEquals(true, project.SiteLiveDateInfo.ReadOnly);
			AssertType<ZWrappedPropertyInfo>(project.SiteLiveDateInfo);

			project.LicenceHeaderPK = lic.PK;
			Factory.Save();

			Assert(!project.HasChanges);
			project.SiteLiveDate = new ZDateTime(2013, 10, 31);

			AssertEquals(new ZDateTime(2013, 10, 31), project.SiteLiveDate);
			Assert(project.HasChanges);

			AssertEquals(new ZDateTime(2013, 10, 31), project.Licence.LA_SiteLiveDate);

			project.SiteLiveDate = ZDateTime.Invalid;
			AssertHasErrors("Site Live Date should have error", project.SiteLiveDateInfo);
			Assert("Project should have error", project.HasErrors);
			project.SiteLiveDate = new ZDateTime(2013, 10, 31);
			AssertEquals(false, project.SiteLiveDateInfo.ReadOnly);
			AssertType<ZWrappedPropertyInfo>(project.SiteLiveDateInfo);

			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = false;
			AssertEquals(true, project.SiteLiveDateInfo.ReadOnly);
		}

		public void TestEstimatedSiteLiveDate()
		{
			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = true;

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var org = lic.Company.Header;
			Factory.Save();

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.ChangeClientOrganisation(org);

			AssertEquals(ZDateTime.Empty, project.EstimatedSiteLiveDate);
			AssertEquals(true, project.EstimatedSiteLiveDateInfo.ReadOnly);

			project.LicenceHeaderPK = lic.PK;
			Factory.Save();

			AssertEquals(false, project.EstimatedSiteLiveDateInfo.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceModifyInstallationDetails.IsAllowed = false;
			AssertEquals(true, project.EstimatedSiteLiveDateInfo.ReadOnly);
		}

		public void TestAgreedLiveDate()
		{
			EDISecurityCheckpoints.OrgLicenceModifyAgreedGoLive.IsAllowed = true;

			var lic = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var org = lic.Company.Header;
			Factory.Save();

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.ChangeClientOrganisation(org);

			AssertEquals(ZDateTime.Empty, project.AgreedLiveDate);
			AssertEquals(true, project.AgreedLiveDateInfo.ReadOnly);

			project.LicenceHeaderPK = lic.PK;
			Factory.Save();

			Assert(!project.HasChanges);
			project.AgreedLiveDate = new ZDateTime(2013, 10, 31);

			AssertEquals(new ZDateTime(2013, 10, 31), project.AgreedLiveDate);
			Assert(project.HasChanges);

			AssertEquals(new ZDateTime(2013, 10, 31), project.Licence.LA_AgreedLiveDate);

			project.AgreedLiveDate = ZDateTime.Invalid;
			AssertHasErrors("should have error", project.AgreedLiveDateInfo);
			Assert("Project should have error", project.HasErrors);
			project.AgreedLiveDate = new ZDateTime(2013, 10, 31);
			AssertEquals(false, project.AgreedLiveDateInfo.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceModifyAgreedGoLive.IsAllowed = false;
			AssertEquals(true, project.AgreedLiveDateInfo.ReadOnly);
		}

		public void TestLicenceClearedWhenClientChanges()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_OH = org1.PK;
			LicenceCompany company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_LE = enterprise1.PK;
			company1.LC_OH = org1.PK;
			LicenceHeader licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			licence1.LA_LC = company1.PK;

			OrgHeader org2 = Factory.New<OrgHeader>();
			LicenceHeader licence2 = Factory.NewWithValidTestData<LicenceHeader>();

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_OA_ClientAddress = org1.MainAddress.PK;
			project.LicenceHeaderPK = licence1.PK;

			project.WKP_OA_ClientAddress = org2.MainAddress.PK;
			AssertEquals(ZGuid.Empty, project.LicenceHeaderPK);

			project.WKP_OA_ClientAddress = org2.MainAddress.PK;
			project.LicenceHeaderPK = licence2.PK;
			AssertEquals(licence2.PK, project.LicenceHeaderPK);

			project.WKP_OA_ClientAddress = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, project.LicenceHeaderPK);
		}

		public void TestPlannedInstall()
		{
			AssertEquals("Pre-condition", ZDateTime.Empty, Project.PlannedInstall);
			AssertEquals("Pre-condition", ProcessTaskStatusCodeList.Codes.Open, Project.WKP_Status);

			Project.PlannedInstall = ZDateTime.BrettsBirthday;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
		}

		public void TestInstallDate()
		{
			AssertEquals("Pre-condition", ZDateTime.Empty, Project.InstallDate);
			AssertEquals("Pre-condition", ProcessTaskStatusCodeList.Codes.Open, Project.WKP_Status);

			Project.InstallDate = ZDateTime.BrettsBirthday;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
		}

		public void TestIsClosedOrCancelled()
		{
			Project project = Factory.NewWithValidTestData<EDIProject>();
			Assert(!((IWorkTaskRelatedItem)project).IsClosedOrCancelled);

			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert(((IWorkTaskRelatedItem)project).IsClosedOrCancelled);
		}

		public void TestEnterpriseCodeEnterpriseID()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_OH = org1.PK;
			enterprise1.LE_EnterpriseCode = "DDD";
			enterprise1.LE_EnterpriseID = "E005";
			var company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_LE = enterprise1.PK;
			company1.LC_OH = org1.PK;

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			AssertEquals("", project.EnterpriseCode);

			project.WKP_OA_ClientAddress = org1.MainAddress.PK;
			AssertEquals("DDD", project.EnterpriseCode);
			AssertEquals("E005", project.EnterpriseID);
		}

		public void TestFilteredRelatedItems()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			EdiHelpErrorLog issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			issue.HE_FixedDate = ZDateTime.Now;
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();

			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.RelatedItems.Add(incident);
			project.RelatedItems.Add(issue);
			project.RelatedItems.Add(workitem);

			AssertEquals(false, project.FilteredRelatedItems.IncludeAllItems);

			AssertEquals(1, project.FilteredRelatedItems.Count);
			AssertCollectionNotContains(incident, project.FilteredRelatedItems);
			AssertCollectionNotContains(issue, project.FilteredRelatedItems);
			AssertCollectionContains(workitem, project.FilteredRelatedItems);

			project.ShowOnlyNonClosedItems = false;
			AssertEquals(3, project.FilteredRelatedItems.Count);
			AssertCollectionContains(incident, project.FilteredRelatedItems);
			AssertCollectionContains(issue, project.FilteredRelatedItems);
			AssertCollectionContains(workitem, project.FilteredRelatedItems);
		}

		public void TestAttachAndDetachFeatureRequest()
		{
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.SetupForNewCreatedFeatureRequest();
			featureRequest.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;

			project.RelatedItems.Add(featureRequest);
			AssertEquals(SupportIncidentLookups.SourceListConstants.CreatedFromProject, featureRequest.IM_Source);

			project.RelatedItems.Load();
			project.RelatedItems.Remove(featureRequest);
			AssertEquals(SupportIncidentLookups.SourceListConstants.ERequestPortal, featureRequest.IM_Source);
		}

		#endregion

		#region Status

		public void TestCalculateStatus_WhenTaskStatusIsSet()
		{
			EDIProject project = Factory.New<EDIProject>();
			project.WKP_Type = "AAA";

			ProcessTask task = project.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "C";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, project.WKP_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, project.WKP_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, project.WKP_Status);

			task.P9_GS_NKAssignedStaffMember = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, project.WKP_Status);
		}

		public void TestCalculateStatus_OnFactorySaving()
		{
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_Type = "AAA";
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, project.WKP_Status);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			EDIProject projectInNewFactory = newFactory.Load<EDIProject>(project.PK);
			ProjectProcessTask task = projectInNewFactory.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			newFactory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, projectInNewFactory.WKP_Status);
		}

		public void TestCalculateStatus_OnWorkflowItemsCountChanged()
		{
			EDIProject project = Factory.New<EDIProject>();
			project.WKP_Type = "AAA";
			CombineAssertions(delegate
			{
				AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, project.WKP_Status);
			});

			ProcessTask task = project.WorkflowItems.Tasks.AddNew();
			CombineAssertions(delegate
			{
				AssertEquals("Sanity check", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				AssertEquals("Sanity check", GlbStaff.CurrentUser.GS_Code, task.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, project.WKP_Status);
			});

			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Cancelled, project.WKP_Status);

			ProcessTask task2 = project.WorkflowItems.Tasks.AddNew();
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Cancelled, project.WKP_Status);
		}

		#endregion

		#region Actions

		public void TestBeginPreInstall()
		{
			Project.BeginPreInstall();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
			AssertContainsOnlyOneMatch("Pre-Install Started", Project.LogText);
		}

		public void TestBeginPreInstall_OnSaving()
		{
			var installationProject = Factory.NewWithValidTestData<EDIProject>();
			installationProject.WKP_Type = installationProject.InstallationProjectCode;
			installationProject.WKP_GS_NKProjectManager = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Precondition: not in the DB", false, installationProject.IsInDatabase);
			Factory.Save();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, installationProject.WKP_Status);
			AssertContainsOnlyOneMatch("Pre-Install Started", installationProject.LogText);

			Factory.Save();
			AssertEquals("Precondition: is in the DB so shouldn't add another log", true, installationProject.IsInDatabase);
			AssertContainsOnlyOneMatch("Pre-Install Started", installationProject.LogText);
		}

		public void TestOnSaving_TechnicalContactNotificationGroup()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.IsInformationServicesTechnicalAdministrator = false;
			var project1 = Factory.NewWithValidTestData<EDIProject>();
			project1.WKP_OC_TechnicalContact = ZGuid.Empty;
			Factory.Save();

			AssertEquals(ZGuid.Empty, project1.WKP_OC_TechnicalContact);
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);

			project1.WKP_OC_TechnicalContact = contact.PK;
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);
			Factory.Save();
			AssertEquals(true, contact.IsInformationServicesTechnicalAdministrator);

			contact.IsInformationServicesTechnicalAdministrator = false;
			project1.WKP_OC_TechnicalContact = ZGuid.Empty;
			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE dbo.WorkProject SET WKP_Summary = '@1234!!!!' WHERE WKP_PK = '{project1.PK}';");

			project1.WKP_OC_TechnicalContact = contact.PK;
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals(false, contact.IsInformationServicesTechnicalAdministrator);
		}

		public void TestCompletePreInstall()
		{
			Project.CompletePreInstall();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
			AssertContainsOnlyOneMatch("Pre-Install Completed", Project.LogText);

			ResetProject();
			Project.PlannedInstall = ZDateTime.Now;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
			AssertContainsOnlyOneMatch("Pre-Install Completed", Project.LogText);
		}

		[TestDate(2008, 9, 9)]
		public void TestCompleteInstall()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ZAC";
			Project.InstallDate = ZDateTime.Empty;
			Project.CompleteInstall();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
			AssertZDatesWithin5Minutes("Install Date date must be set to current Date-Time", ZDateTime.Now, Project.InstallDate);
			AssertContainsOnlyOneMatch("Install Completed", Project.LogText);

			ResetProject();
			var task = AddTask(staff);

			Project.InstallDate = ZDateTime.Empty;
			Project.CompleteInstall();
			AssertEquals("Install Date must be set to current Date-Time", ZDateTime.Now, Project.InstallDate);
			AssertContainsOnlyOneMatch("Install Completed - Pending Pre-Install Task(s)", Project.LogText);

			ResetProject();
			task = AddTask(staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Project.InstallDate = ZDateTime.Now;
			AssertContainsOnlyOneMatch("Install Completed", Project.LogText);

			ResetProject();
			task = AddTask(staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Project.InstallDate = ZDateTime.Now;
			AssertContainsOnlyOneMatch("Install Completed - Pending Pre-Install Task(s)", Project.LogText);
		}

		ProcessTask AddTask(GlbStaff staff)
		{
			ProcessTask task = Project.WorkflowItems.AddNew();
			task.P9_Type = "PRE";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			return task;
		}

		public void TestBeginTraining()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Project.BeginTraining();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
			AssertContainsOnlyOneMatch("Training Started", Project.LogText);
		}

		public void TestCompleteTraining()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Project.CompleteTraining();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, Project.WKP_Status);
			AssertContainsOnlyOneMatch("Training Completed", Project.LogText);
		}

		public new void TestClose()
		{
			var project = Factory.New<EDIProject>();
			project.CallbackBy = ZDateTime.Now;
			var task = project.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var task2 = project.WorkflowItems.AddNew();
			var task3 = project.WorkflowItems.AddNew();
			var task4 = project.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, project.WKP_Status);

			project.Close(ProcessTaskStatusCodeList.Codes.Closed, "");
			AssertEquals("WKP_Status", ProcessTaskStatusCodeList.Codes.Closed, project.WKP_Status);
			AssertEquals("task", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Closed, task3.P9_Status);
			AssertEquals("task4", ProcessTaskStatusCodeList.Codes.Cancelled, task4.P9_Status);
			AssertEquals(ZDateTime.Empty, project.CallbackBy);
		}

		public new void TestCancel()
		{
			var project = Factory.New<EDIProject>();
			project.CallbackBy = ZDateTime.Now;
			var task = project.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var task2 = project.WorkflowItems.AddNew();
			var task3 = project.WorkflowItems.AddNew();
			var task4 = project.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, project.WKP_Status);

			project.Close(ProcessTaskStatusCodeList.Codes.Cancelled, "");
			AssertEquals("WKP_Status", ProcessTaskStatusCodeList.Codes.Cancelled, project.WKP_Status);
			AssertEquals("task", ProcessTaskStatusCodeList.Codes.Cancelled, task.P9_Status);
			AssertEquals("task2", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals("task3", ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);
			AssertEquals("task4", ProcessTaskStatusCodeList.Codes.Closed, task4.P9_Status);
			AssertEquals(ZDateTime.Empty, project.CallbackBy);
		}

		public new void TestReOpen()
		{
			base.TestReOpen();
		}

		#endregion

		#region IARInvoiceSavingNotificationSubscriber

		public void TestGetRecipientsForInvoicePaidNotificationEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ENTPRJTST";

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "Samuel.Wang@cargowise.com";
			staff1.Groups.Add(group);

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "Projects@cargowise.com";
			staff2.Groups.Add(group);

			Factory.Save();

			EDIDataRegistry.Instance.ProjectInvoiceEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Project project = Factory.NewWithValidTestData<EDIProject>();
			IARInvoiceSavingNotificationSubscriber subscriber = (IARInvoiceSavingNotificationSubscriber)project;
			AssertEquals("Should contain two email addresses: ", 2, subscriber.GetRecipientsForNotification().Length);
		}

		#endregion

		#region IWorkTaskTreeNode

		public void TestIWorkTaskTreeNodeMembers()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			var workItem = Factory.NewWithValidTestData<EDIWorkItem>();
			var projectChild = Factory.NewWithValidTestData<EDIProject>();
			var agreedDeliveryDate = ZDateTime.Now;
			SetupProject(project, agreedDeliveryDate, workItem, projectChild);

			AssertEquals(agreedDeliveryDate, project.AgreedDeliveryDate.ToUniversalBranchTime());
			AssertEquals("Task test", project.CurrentTaskDescription);
			AssertEquals("686 - Capability desc", project.CurrentTaskCapabilityCodeDescription);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, project.CurrentTaskAssigned);

			AssertEquals(2, project.ChildrenOnlyRelatedItems.Count);
			AssertEquals(1, project.ChildrenOnlyRelatedItems.Count(a => a.PK == workItem.PK));
			AssertEquals(1, project.ChildrenOnlyRelatedItems.Count(a => a.PK == projectChild.PK));
			AssertNull(project.ParentsOnlyRelatedItems);
		}

		void SetupProject(EDIProject project, ZDateTime agreedDeliveryDate, EDIWorkItem workItem, EDIProject projectChild)
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, project.WorkflowType);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Capability desc";

			var jobHeader = helper.GetJobHeaderForParent(project, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Exported Workflow");
			var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 2, description: "Be exported");
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Description = "Task test";

			project.JobWorkflow.FH_AgreedDeliveryDate = agreedDeliveryDate;
			project.RelatedItems.Add(workItem);
			project.RelatedItems.Add(projectChild);
		}

		#endregion

		#region Implementation

		EDIProject Project
		{
			get
			{
				if (project == null)
				{
					project = Factory.New<EDIProject>();
				}
				return project;
			}
		}

		EDIProject project;

		void ResetProject()
		{
			project = Factory.New<EDIProject>();
		}

		#endregion
	}

	#region EDIProjectRelatableActivityTest

	[TestedType(typeof(EDIProject))]
	class EDIProjectRelatableActivityTest : RelatableActivityTestCase<EDIProject>
	{
		protected override EDIProject GetNewActivity()
		{
			return Factory.NewWithValidTestData<EDIProject>();
		}
	}

	#endregion

	[TestedType(typeof(EDIProject))]
	sealed class EDIProjectRelatedItemTest : ProjectRelatedItemTestCase
	{
	}

	[TestedType(typeof(EDIProject))]
	sealed class EDIProjectRelatedItemSourceTest : ProjectRelatedItemSourceTestCase
	{
	}
}
