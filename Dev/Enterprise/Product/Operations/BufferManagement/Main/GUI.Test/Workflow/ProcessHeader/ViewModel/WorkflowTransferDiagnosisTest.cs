using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkflowTransferDiagnosis))]
	class WorkflowTransferDiagnosisTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDestinationType_Bucket()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, @"Just couldn't release, OK?", bucket2, ReleaseLogFailureService);

			Factory.Save();

			AssertEquals(false, link.ComponentFrom.IsBuffer);
			AssertEquals(false, link.ComponentTo.IsBuffer);

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals("Bucket", transferFailure.DestinationType);
		}

		public void TestDestinationType_Buffer()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1", 2);

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = buffer1.PK;
			link.FL_IsReleaseGateRuleApplied = false;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, @"Just couldn't release, OK?", buffer1, ReleaseLogFailureService);

			Factory.Save();

			AssertEquals(false, link.ComponentFrom.IsBuffer);
			AssertEquals(true, link.ComponentTo.IsBuffer);
			AssertEquals(false, link.FL_IsReleaseGateRuleApplied);

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals("Buffer", transferFailure.DestinationType);
		}

		public void TestTransferFailure_ShouldNotDeleteLastTransferFailureReason()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			BMSTestHelper.LinkComponents(bucket, buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Something happened...", buffer, ReleaseLogFailureService);

			Factory.Save();

			var viewModel = new WorkflowTransferDiagnosisViewModel(workflow);
			AssertEquals(1, viewModel.TransferDiagnoses.Count);
			AssertMultilineASCIIEquals("", @"Something happened...", viewModel.TransferDiagnoses[0].FailureReason);
		}

		public void TestFilterRuleMatchingStatus()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Just couldn't release, OK?", bucket2, ReleaseLogFailureService);

			Factory.Save();
			workflow.Reload();

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals("No", transferFailure.FilterRuleMatchingStatus);

			transferFailure.TryTransfer();
			AssertEquals("Yes", transferFailure.FilterRuleMatchingStatus);
		}

		public void TestComponentName()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals("bucket2", transferFailure.ComponentToName);

			link.FL_Name = "Quick Mysteries";
			AssertEquals("The component's name shouldn't affect the To Component string. SAD!", "bucket2", transferFailure.ComponentToName);
		}

		public void TestLinkName()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var failure = new WorkflowTransferDiagnosis(config.ComponentLink, workflow);

			AssertEquals("Buffer Entry", failure.LinkName);

			config.ComponentLink.FL_Name = ZString.Empty;
			AssertEquals("bucket -> buffer, Sequence: 0", failure.LinkName);
		}

		public void TestTryTransfer()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, @"Just couldn't release, OK?", bucket2, ReleaseLogFailureService);

			Factory.Save();

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals(string.Empty, transferFailure.FailureReason);

			transferFailure.TryTransfer();
			AssertMultilineASCIIEquals("", "The selected destination component is not a buffer.", transferFailure.FailureReason);
		}

		public void TestTryTransfer_TransferAllowed()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Just couldn't release, OK?", bucket2, ReleaseLogFailureService);

			Factory.Save();
			workflow.Reload();

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals(string.Empty, transferFailure.FailureReason);

			transferFailure.TryTransfer();
			AssertMultilineASCIIEquals("", "The selected destination component is not a buffer.", transferFailure.FailureReason);
		}

		public void TestTryTransfer_BucketToBucket()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, @"Just couldn't release, OK?", bucket2, ReleaseLogFailureService);

			Factory.Save();

			AssertEquals(false, link.ComponentFrom.IsBuffer);
			AssertEquals(false, link.ComponentTo.IsBuffer);

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals(string.Empty, transferFailure.FailureReason);

			transferFailure.TryTransfer();
			AssertMultilineASCIIEquals("", "The selected destination component is not a buffer.", transferFailure.FailureReason);
		}

		public void TestTryTransfer_BucketToBuffer()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1", 2);

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = buffer1.PK;
			link.FL_IsReleaseGateRuleApplied = false;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, @"Just couldn't release, OK?", buffer1, ReleaseLogFailureService);

			Factory.Save();

			AssertEquals(false, link.ComponentFrom.IsBuffer);
			AssertEquals(true, link.ComponentTo.IsBuffer);
			AssertEquals(false, link.FL_IsReleaseGateRuleApplied);

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals(string.Empty, transferFailure.FailureReason);

			transferFailure.TryTransfer();
			AssertMultilineASCIIEquals("", "The selected component link is not configured as a 'Release Gate' link, so does not evaluate resource capacity.", transferFailure.FailureReason);
		}

		public void TestTryTransfer_WhenComponentLinkIsDisabled_ShouldSpecifyRelevantFailureReason()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;
			link.FL_TransferRulesEnabled = false;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, @"Just couldn't release, OK?", bucket2, ReleaseLogFailureService);

			Factory.Save();

			var transferFailure = new WorkflowTransferDiagnosis(link, workflow);

			AssertEquals(string.Empty, transferFailure.FailureReason);

			transferFailure.TryTransfer();
			AssertMultilineASCIIEquals("", "The selected component link is currently disabled.", transferFailure.FailureReason);
		}

		public void TestFailureReason_WhenReleaseGateNotApplicable_ShouldNotIncludeLastFailureReason()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var bucketToBucketLink = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var releaseGateLink = BMSTestHelper.LinkComponents(bucket2, buffer, isReleaseGate: true);
			var nonReleaseGateBufferLink = BMSTestHelper.LinkComponents(bucket1, buffer, isReleaseGate: false);
			var linkWithFilters = BMSTestHelper.LinkComponents(bucket1, bucket2);

			FilterStripsTestHelper.AddStartsWithFilter(linkWithFilters.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Moist");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow");
			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Information won't be available until the next time the service task runs", bucket2, ReleaseLogFailureService);
			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Information won't be available until the next time the service task runs", buffer, ReleaseLogFailureService);

			var diagnosis = new WorkflowTransferDiagnosis(bucketToBucketLink, workflow);
			diagnosis.TryTransfer();
			AssertEquals("The selected destination component is not a buffer.", diagnosis.FailureReason);

			diagnosis = new WorkflowTransferDiagnosis(releaseGateLink, workflow);
			diagnosis.TryTransfer();
			AssertEquals("Information won't be available until the next time the service task runs", diagnosis.FailureReason);

			diagnosis = new WorkflowTransferDiagnosis(nonReleaseGateBufferLink, workflow);
			diagnosis.TryTransfer();
			AssertEquals("The selected component link is not configured as a 'Release Gate' link, so does not evaluate resource capacity.", diagnosis.FailureReason);

			diagnosis = new WorkflowTransferDiagnosis(linkWithFilters, workflow);
			diagnosis.TryTransfer();
			AssertEquals("This workflow cannot be released until the filter strip requirements are met.", diagnosis.FailureReason);
		}

		[TestDate(2019, 11, 15, 0, 0, 0)] // DST in Sydney, not Brisbane
		public void TestTryTransfer_UsesComponentBranchIfSet()
		{
			TestDateAttribute.UseUNLOCO = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, name: "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, name: "Bucket 2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var branchSYD = Factory.NewWithValidTestData<GlbBranch>();
			branchSYD.GB_BranchName = "Rancid Knee";
			branchSYD.GB_Code = "AUS";
			branchSYD.GB_RL_NKHomePort = "AUSYD";

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			AssertEquals("Precondition", "Australia/Sydney", branchSYD.HomePort.TimeZoneSet.R3_TimeZoneSetName);
			AssertEquals("Precondition", "Australia/Brisbane", branchBNE.HomePort.TimeZoneSet.R3_TimeZoneSetName);

			bucket2.FC_GB_AgingBranch = branchSYD.PK;

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today,
			});

			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow");
			workflow.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 14, 13, 0, 0); // 14th NOV 11pm BNE/15th NOV 12am SYD -- 14th NOV 1pm UTC

			Factory.Save();

			var diagnosis = new WorkflowTransferDiagnosis(link, workflow);
			diagnosis.TryTransfer();
			AssertEquals("Should pass filter rules: it's already 15th Nov in SYD", true, diagnosis.PassedFilterRules);

			bucket2.FC_GB_AgingBranch = branchBNE.PK;
			Factory.Save();

			diagnosis = new WorkflowTransferDiagnosis(link, workflow);
			diagnosis.TryTransfer();
			AssertEquals("Should not pass filter rules: it's still 14th Nov in BNE", false, diagnosis.PassedFilterRules);
		}

		[TestDate(2019, 11, 15, 0, 0, 0)] // Friday
		public void TestTryTransfer_UsesComponentDepartmentIfSet()
		{
			TestDateAttribute.UseUNLOCO = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = BMSTestHelper.CreateBucket(system, name: "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, name: "Bucket 2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var deptDEP = Factory.NewWithValidTestData<GlbDepartment>();
			deptDEP.GE_Desc = "Jony";
			deptDEP.GE_Code = "DEP";
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, deptDEP.PK);

			var deptZZZ = Factory.NewWithValidTestData<GlbDepartment>();
			deptZZZ.GE_Desc = "Sleep";
			deptZZZ.GE_Code = "ZZZ";

			AssertEquals("Precondition", "                  ****************", deptDEP.WorkTimes.FridayWorkingHours);
			AssertEquals("Precondition", "", deptZZZ.WorkTimes.FridayWorkingHours);

			bucket2.FC_GE_AgingDepartment = deptDEP.PK;

			var filter = new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f =>
				{
					var dataFilter = f as ModuleDateFilter;
					dataFilter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
					dataFilter.Property2 = new ZDateTime(2024, 1, 1, 8, 0, 0);
					dataFilter.PropertyDecimal2 = 8m;
					dataFilter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
				}
			};

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, filter);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow");
			workflow.FH_AgreedDeliveryDate = new ZDateTime(2019, 11, 16, 13, 0, 0);

			Factory.Save();

			var diagnosis = new WorkflowTransferDiagnosis(link, workflow);
			diagnosis.TryTransfer();
			AssertEquals("Should pass filter rules: 8 hours goes past weekend", true, diagnosis.PassedFilterRules);

			bucket2.FC_GE_AgingDepartment = deptZZZ.PK;
			Factory.Save();

			diagnosis = new WorkflowTransferDiagnosis(link, workflow);
			diagnosis.TryTransfer();
			AssertEquals("Should not pass filter rules: 8 hours is still on Friday", false, diagnosis.PassedFilterRules);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();

			return new WorkflowTransferDiagnosis(link, workflow);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ReleaseLogFailureService = new ReleaseGateFailureLogService_ForTest();
			BMSTestCaseWithFactory.SetFactoryReleaseLogFailureService(Factory, ReleaseLogFailureService);
		}

		protected ReleaseGateFailureLogService_ForTest ReleaseLogFailureService { get; private set; }
	}
}
