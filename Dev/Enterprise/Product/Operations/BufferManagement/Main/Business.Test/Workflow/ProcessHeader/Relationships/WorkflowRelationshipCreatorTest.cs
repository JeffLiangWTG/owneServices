using System;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowRelationshipCreatorTest : BMSTestCaseWithFactory
	{
		public void TestCreateRelationship_WhenAlreadyExists()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var existingLink = Factory.New<ProcessHeaderLink>();
			existingLink.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			existingLink.FP_FH_HeaderFrom = jobHeader1.PK;
			existingLink.FP_FH_HeaderTo = jobHeader2.PK;

			AssertIsPrerequisite(jobHeader1, jobHeader2);

			var result1 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.CreateOnlyIfLinkDoesNotExist);

			AssertEquals(true, result1.IsExistingLink);
			AssertEquals(existingLink, result1.Link);
			AssertNoErrors(result1.Link);

			var result2 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.None);

			AssertEquals(false, result2.IsExistingLink);
			AssertNotEquals(existingLink, result2.Link);

			AssertHasError(result2.Link.FP_FH_HeaderFromInfo, "The From Workflow has been duplicated and must be unique.");
			AssertHasError(result2.Link.FP_FH_HeaderToInfo, "The To Workflow has been duplicated and must be unique.");
		}

		public void TestCreateRelationship_WhenInverseRelationshipAlreadyExists()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var existingLink = Factory.New<ProcessHeaderLink>();
			existingLink.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			existingLink.FP_FH_HeaderFrom = jobHeader1.PK;
			existingLink.FP_FH_HeaderTo = jobHeader2.PK;

			AssertIsPrerequisite(jobHeader1, jobHeader2);

			var result1 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader2, jobHeader1, RelationshipOptions.CreateOnlyIfLinkDoesNotExist | RelationshipOptions.ReverseExistingRelationship);
			result1.Link.RunPreSaveValidation();

			AssertEquals(false, result1.IsExistingLink);
			AssertNotEquals(existingLink, result1.Link);
			AssertNoErrors(result1.Link);
			AssertEquals(true, existingLink.IsDeleted);

			var result2 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.CreateOnlyIfLinkDoesNotExist);
			result2.Link.RunPreSaveValidation();

			AssertEquals(false, result2.IsExistingLink);
			AssertNotEquals(existingLink, result2.Link);
			AssertNotEquals(result1.Link, result2.Link);

			AssertHasError(result2.Link.FP_FH_HeaderFromInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (H5ZX52PAMCOI) - Job Organization (H5ZX52PAMCOI) is complete.
Organization (H5ZX52PAMCOI) - Job Workflow
Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.
Organization (XVBQP68SIYXQ) - Job Workflow");

			AssertHasError(result2.Link.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (H5ZX52PAMCOI) - Job Organization (H5ZX52PAMCOI) is complete.
Organization (H5ZX52PAMCOI) - Job Workflow
Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.
Organization (XVBQP68SIYXQ) - Job Workflow");
		}

		public void TestCreateRelationship_WhenNoRelationshipExists()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			AssertIsNotPrerequisite(jobHeader1, jobHeader2);

			var result1 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.None);

			AssertEquals(false, result1.IsExistingLink);
			AssertIsPrerequisite(jobHeader1, jobHeader2);

			var result2 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.None);

			AssertEquals(false, result2.IsExistingLink);
			AssertNotEquals(result1.Link, result2.Link);

			var result3 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.CreateOnlyIfLinkDoesNotExist);

			AssertEquals(true, result3.IsExistingLink);
			AssertEquals(result1.Link, result3.Link);
		}

		public void TestCreateRelationship_WhenParentChildRelationshipExists()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var parentChildLink = jobHeader1.GetOrCreateLinkToParent(jobHeader2);

			var result1 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader2, jobHeader1, RelationshipOptions.CreateOnlyIfLinkDoesNotExist | RelationshipOptions.ReverseExistingRelationship);

			AssertEquals(false, parentChildLink.IsDeleted);
			AssertEquals(false, result1.IsExistingLink);
			AssertNotEquals(parentChildLink, result1.Link);

			AssertHasError(result1.Link.FP_FH_HeaderFromInfo, @"Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.");

			var result2 = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, jobHeader1, jobHeader2, RelationshipOptions.CreateOnlyIfLinkDoesNotExist | RelationshipOptions.ReverseExistingRelationship);

			AssertEquals(false, parentChildLink.IsDeleted);
			AssertEquals(false, result2.IsExistingLink);
			AssertNotEquals(parentChildLink, result2.Link);

			AssertHasError(result2.Link.FP_FH_HeaderFromInfo, @"Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.");
		}
	}
}
