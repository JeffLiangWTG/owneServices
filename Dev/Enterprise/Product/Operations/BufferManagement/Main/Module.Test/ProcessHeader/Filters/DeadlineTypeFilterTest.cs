using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(DeadlineTypeFilter))]
	class DeadlineTypeFilterTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeadlineTypeFilter();
		}

		public void TestDeadlineTypeFilter_EffectiveAndImmediateAreExclusive()
		{
			var filter = new DeadlineTypeFilter();

			filter.IsEffective = true;
			AssertEquals(true, filter.IsEffective);
			AssertEquals(false, filter.IsImmediate);

			filter.IsImmediate = true;
			AssertEquals(false, filter.IsEffective);
			AssertEquals(true, filter.IsImmediate);

			filter.IsEffective = true;
			AssertEquals(true, filter.IsEffective);
			AssertEquals(false, filter.IsImmediate);
		}

		public void TestDeadlineTypeFilter_FiltersAllSixCasesCorrectly()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeaderH = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JLW the Hard");
			var jobHeaderS = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JLW the Soft");
			var jobHeaderN = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JLW the None");
			var workflowHH = BMSTestHelper.CreateWorkflow(jobHeaderH, "Workflow HH");
			var workflowHS = BMSTestHelper.CreateWorkflow(jobHeaderH, "Workflow HS");
			var workflowHN = BMSTestHelper.CreateWorkflow(jobHeaderH, "Workflow HN");
			var workflowSH = BMSTestHelper.CreateWorkflow(jobHeaderS, "Workflow SH");
			var workflowSS = BMSTestHelper.CreateWorkflow(jobHeaderS, "Workflow SS");
			var workflowSN = BMSTestHelper.CreateWorkflow(jobHeaderS, "Workflow SN");
			var workflowNH = BMSTestHelper.CreateWorkflow(jobHeaderN, "Workflow NH");
			var workflowNS = BMSTestHelper.CreateWorkflow(jobHeaderN, "Workflow NS");
			var workflowNN = BMSTestHelper.CreateWorkflow(jobHeaderN, "Workflow NN");

			foreach (var header in new[] { jobHeaderH, workflowHH, workflowSH, workflowNH })
			{
				header.FH_DeadlineType = DeadlineTypeList.Codes.Hard;
			}

			foreach (var header in new[] { jobHeaderS, workflowHS, workflowSS, workflowNS })
			{
				header.FH_DeadlineType = DeadlineTypeList.Codes.Soft;
			}

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (DeadlineTypeFilter)bizo[ProcessHeader.ModuleFilterConstants.DeadlineType];

			filter.IsActive = true;

			CombineAssertions(() =>
			{
				filter.IsEffective = true;

				filter.DeadlineType = DeadlineTypeFilterTypeList.Codes.Hard;
				AssertContainsExactElementsInAnyOrder($"Testing effective HRD", new[] { jobHeaderH, workflowHH, workflowHN, workflowSH, workflowNH }, Factory.Load<ProcessHeader>(bizo.Filter));

				filter.DeadlineType = DeadlineTypeFilterTypeList.Codes.Soft;
				AssertContainsExactElementsInAnyOrder($"Testing effective SFT", new[] { jobHeaderS, workflowHS, workflowSS, workflowSN, workflowNS }, Factory.Load<ProcessHeader>(bizo.Filter));

				filter.DeadlineType = DeadlineTypeFilterTypeList.Codes.None;
				AssertContainsExactElementsInAnyOrder($"Testing effective NON", new[] { jobHeaderN, workflowNN }, Factory.Load<ProcessHeader>(bizo.Filter));

				filter.IsImmediate = true;

				filter.DeadlineType = DeadlineTypeFilterTypeList.Codes.Hard;
				AssertContainsExactElementsInAnyOrder($"Testing immediate HRD", new[] { jobHeaderH, workflowHH, workflowSH, workflowNH }, Factory.Load<ProcessHeader>(bizo.Filter));

				filter.DeadlineType = DeadlineTypeFilterTypeList.Codes.Soft;
				AssertContainsExactElementsInAnyOrder($"Testing immediate SFT", new[] { jobHeaderS, workflowHS, workflowSS, workflowNS }, Factory.Load<ProcessHeader>(bizo.Filter));

				filter.DeadlineType = DeadlineTypeFilterTypeList.Codes.None;
				AssertContainsExactElementsInAnyOrder($"Testing immediate NON", new[] { jobHeaderN, workflowHN, workflowSN, workflowNN }, Factory.Load<ProcessHeader>(bizo.Filter));
			});
		}
	}
}
