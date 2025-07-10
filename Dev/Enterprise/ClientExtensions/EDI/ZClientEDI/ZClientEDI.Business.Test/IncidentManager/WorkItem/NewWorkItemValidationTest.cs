using System;
using CargoWise.Types;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class NewWorkItemValidationTest : WorkItemActualValidationTest
	{
		public void TestCheckWKI_ActivitySubtype_NoRelatedItems()
		{
			// Arrange
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			Add(tree, "1AA", "2AA", "3AA", NewWorkItemLookups.WorkItemTypeConstants.IssueFix);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;

			Factory.Save();

			// Act
			workItem.Validation.ValidateAll();

			// Assert
			AssertEquals(0, workItem.RelatedItems.Count);
			AssertHasErrorContaining("Issue type work item should have at least one related item", workItem.WKI_ActivitySubtypeInfo, "This field cannot be set to issue type without a related Issue.");
		}

		public void TestCheckWKI_ActivitySubtype_HaveOtherTypeRelatedItems()
		{
			// Arrange
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			Add(tree, "1AA", "2AA", "3AA", NewWorkItemLookups.WorkItemTypeConstants.IssueFix);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident = Factory.New<SupportIncident>();
			var errorLog = Factory.New<EdiHelpErrorLog>();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedWorkItems.Add(workItem);
			errorLog.RelatedWorkItems.Add(workItem);
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;
			Factory.Save();

			// Act
			workItem.Validation.ValidateAll();

			// Assert
			AssertEquals(2, workItem.RelatedItems.Count);
			AssertNoErrors("Correct Issue Type Work Item", workItem.WKI_ActivitySubtypeInfo);
		}

		public void TestCheckWKI_ActivitySubtype_AllRelateItemsAreNotIssueType()
		{
			// Arrange
			var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
			Add(tree, "1AA", "2AA", "3AA", NewWorkItemLookups.WorkItemTypeConstants.IssueFix);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var incident1 = Factory.New<SupportIncident>();
			var incident2 = Factory.New<SupportIncident>();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident1.RelatedWorkItems.Add(workItem);
			incident2.RelatedWorkItems.Add(workItem);
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.IssueFix;

			Factory.Save();

			// Act
			workItem.Validation.ValidateAll();

			// Assert
			AssertEquals(2, workItem.RelatedItems.Count);
			AssertHasErrorContaining("All related item in issue work item should be issue type at least one", workItem.WKI_ActivitySubtypeInfo, "This field cannot be set to issue type without a related Issue.");
		}

		void Add(CodeDescriptionBoolTreeNodeCollection tree, params string[] codes)
		{
			string[] parentCodes = new string[codes.Length - 1];
			Array.Copy(codes, parentCodes, parentCodes.Length);
			string code = codes[codes.Length - 1];
			CodeDescriptionBoolTreeNode parent = parentCodes.Length > 0 ? tree.Find(parentCodes) : null;
			var result = tree.Add(code, (NoResString)(code + " depth " + (parentCodes.Length + 1)), true, false, parent);
			tree.AddSystemChildren(result);
		}

		public void TestHasDuplicateDatSubmissions_DuplicateGit()
		{
			var workItemOne = Factory.NewWithValidTestData<NewWorkItem>();

			var shelfTest = workItemOne.WorkflowItems.AddNew();
			shelfTest.P9_Type = "SH0";
			shelfTest.P9_Description = "Some Shelf";
			shelfTest.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			shelfTest.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workItemOne.Validation.ValidateAll();
			AssertNoErrors("Single Submission", workItemOne.HasDuplicateDatSubmissionsInfo);

			var checkInTask = workItemOne.WorkflowItems.AddNew();
			checkInTask.P9_Type = "CH0";
			checkInTask.P9_Description = "AnotherShelf";
			checkInTask.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItemOne.Validation.ValidateAll();
			AssertHasErrors("Duplicate Git pull request", workItemOne.HasDuplicateDatSubmissionsInfo);

			checkInTask.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1279?_a=overview";
			workItemOne.Validation.ValidateAll();
			AssertNoErrors("Error removed now there is no duplicate", workItemOne.HasDuplicateDatSubmissionsInfo);
		}

		public void TestHasDuplicateDatSubmissions_DuplicateGitWithMultiplePullRequestsInEachTask()
		{
			var workItemOne = Factory.NewWithValidTestData<NewWorkItem>();

			var shelfTest = workItemOne.WorkflowItems.AddNew();
			shelfTest.P9_Type = "SH0";
			shelfTest.P9_Description = "Some Shelf";
			shelfTest.P9_NotesAsString = @"
http://tfs.wtg.zone:8080/tfs/unique1/_git/borderwise/pullrequest/1001?_a=overview
http://tfs.wtg.zone:8080/tfs/duplicate/_git/borderwise/pullrequest/1234?_a=overview
http://tfs.wtg.zone:8080/tfs/unique2/_git/borderwise/pullrequest/1002?_a=overview
";
			shelfTest.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workItemOne.Validation.ValidateAll();
			AssertNoErrors("No error for Single Submission", workItemOne.HasDuplicateDatSubmissionsInfo);

			var checkInTask = workItemOne.WorkflowItems.AddNew();
			checkInTask.P9_Type = "CH0";
			checkInTask.P9_Description = "AnotherShelf";
			checkInTask.P9_NotesAsString = @"
http://tfs.wtg.zone:8080/tfs/unique3/_git/borderwise/pullrequest/1003?_a=overview
http://tfs.wtg.zone:8080/tfs/duplicate/_git/borderwise/pullrequest/1234?_a=overview
http://tfs.wtg.zone:8080/tfs/unique4/_git/borderwise/pullrequest/1004?_a=overview
";
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workItemOne.Validation.ValidateAll();
			AssertHasErrors("Duplicate Git pull request", workItemOne.HasDuplicateDatSubmissionsInfo);

			checkInTask.P9_NotesAsString = @"
http://tfs.wtg.zone:8080/tfs/unique3/_git/borderwise/pullrequest/1003?_a=overview
http://tfs.wtg.zone:8080/tfs/notduplicate/_git/borderwise/pullrequest/1234?_a=overview
http://tfs.wtg.zone:8080/tfs/unique4/_git/borderwise/pullrequest/1004?_a=overview
";
			workItemOne.Validation.ValidateAll();
			AssertNoErrors("All Git pull requests should now be unique", workItemOne.HasDuplicateDatSubmissionsInfo);
		}

		public void TestHasDuplicateDatSubmissions_GitIgnoreCase()
		{
			var workItemOne = Factory.NewWithValidTestData<NewWorkItem>();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "DV1";
			staff1.GS_LoginName = "dev.one";

			var shelfTest = workItemOne.WorkflowItems.AddNew();
			shelfTest.P9_Type = "SH0";
			shelfTest.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			shelfTest.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			shelfTest.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workItemOne.Validation.ValidateAll();
			AssertNoErrors("Single Submission", workItemOne.HasDuplicateDatSubmissionsInfo);

			var checkInTask = workItemOne.WorkflowItems.AddNew();
			checkInTask.P9_Type = "CH0";
			checkInTask.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			checkInTask.P9_NotesAsString = "http://TFS.WTG.ZONE:8080/TFS/CARGOWISE/_GIT/BORDERWISE/pullrequest/1278?_A=OVERVIEW";
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItemOne.Validation.ValidateAll();

			AssertHasErrors("Pull request is the same despite different case", workItemOne.HasDuplicateDatSubmissionsInfo);
		}

		public void TestHasDuplicateDatSubmissions_DifferentUsersWithTheSameShelf()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "DV1";
			staff1.GS_LoginName = "dev.one";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "DV2";
			staff2.GS_LoginName = "dev.two";

			var workItemOne = Factory.NewWithValidTestData<NewWorkItem>();
			var shelfTest = workItemOne.WorkflowItems.AddNew();
			shelfTest.P9_Type = "SH0";
			shelfTest.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			shelfTest.P9_Description = "ShelfName";
			shelfTest.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=overview";
			shelfTest.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workItemOne.Validation.ValidateAll();
			AssertNoErrors("Single Submission", workItemOne.HasDuplicateDatSubmissionsInfo);

			var checkInTask = workItemOne.WorkflowItems.AddNew();
			checkInTask.P9_Type = "CH0";
			checkInTask.P9_Description = "ShelfName";
			checkInTask.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			checkInTask.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1279?_a=overview";
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItemOne.Validation.ValidateAll();
			AssertNoErrors("Shelves are from different users, pull requests are different", workItemOne.HasDuplicateDatSubmissionsInfo);

			checkInTask.P9_NotesAsString = "http://tfs.wtg.zone:8080/tfs/cargowise/_git/borderwise/pullrequest/1278?_a=OVERVIEW";
			workItemOne.Validation.ValidateAll();
			AssertHasErrors("Duplicate Pull Request", workItemOne.HasDuplicateDatSubmissionsInfo);
		}

		public void TestHasDuplicateDatSubmissions_ErrorMessageText()
		{
			var workItemOne = Factory.NewWithValidTestData<NewWorkItem>();

			var shelfTest = workItemOne.WorkflowItems.AddNew();
			shelfTest.P9_Type = "SH0";
			shelfTest.P9_Description = "Some Shelf";
			shelfTest.P9_NotesAsString = "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/248725";
			shelfTest.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var checkInTask = workItemOne.WorkflowItems.AddNew();
			checkInTask.P9_Type = "CH0";
			checkInTask.P9_Description = "AnotherShelf";
			checkInTask.P9_NotesAsString = "https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/248725";
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workItemOne.Validation.ValidateAll();
			AssertHasError(workItemOne.HasDuplicateDatSubmissionsInfo, $"Multiple Check-In/Shelf Test tasks can be queued simultaneously. However each Git pull request must be unique. Check the task notes of any Check-In or Shelf Test tasks in this work item with Status set to '{ProcessTaskStatusCodeList.Codes.Assigned}'. This can occur when a task or workflow is cloned but the task notes have not been cleared.");
		}

		public void TestCheckWKI_Details_WarningIfPersonalSharePointLinkIsSpecified()
		{
			const string expectedWarning = "Description may contain a link to a personal SharePoint / OneDrive space. Please ensure all links point to official WiseTech Global SharePoint sites.";

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var targetPropertyInfo = workItem.WKI_DetailsInfo;
			workItem.WKI_Details = ZBlob.FromUTF8("Specification Description");
			AssertNoWarningContaining("When No link is present", targetPropertyInfo, expectedWarning);

			workItem.WKI_Details = ZBlob.FromUTF8("Specification Description plus link https://wisetechglobal-my.sharepoint.com/_layout/page/doc");
			AssertHasWarningContaining("When SharePoint Personal Link is present", targetPropertyInfo, expectedWarning);

			workItem.WKI_Details = ZBlob.FromUTF8("Some description https://wisetechglobal.sharepoint.com/:w:/r/_layouts/15/Doc.aspx");
			AssertNoWarningContaining("When SharePoint General Link is present", targetPropertyInfo, expectedWarning);

			workItem.WKI_Details = ZBlob.FromUTF8("Some description https://othercompany.sharepoint.com/:w:/r/_layouts/15/Doc.aspx");
			AssertNoWarningContaining("When SharePoint External Link is present", targetPropertyInfo, expectedWarning);

			workItem.WKI_Details = ZBlob.FromUTF8("Some description https://github.com/some-project/project.cs");
			AssertNoWarningContaining("When External Link is present", targetPropertyInfo, expectedWarning);
		}
	}
}
