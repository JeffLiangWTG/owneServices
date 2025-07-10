using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ApprovedWorkflowScheduleTypeFilter))]
	class ApprovedWorkflowScheduleTypeFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestApprovedScheduleType()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, isScaled: true);
			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var scheduleTypeFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.ApprovedScheduleType];
			scheduleTypeFilter.IsActive = true;
			scheduleTypeFilter.Property = ApprovedDiagramTypeList.Codes.CCPMApprovedDiagram;

			var jobOrWorkflowFilter = (JobOrWorkflowFilter)bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetJobOnly();

			var results = new BusinessObjectFactory().Load<ProcessHeader>(bizo.Filter);
			AssertEquals(0, results.Length);

			diagram.IsBuffered = true;
			Factory.Save();

			results = new BusinessObjectFactory().Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			BMSTestCaseWithFactory.AssertSamePK(jobHeader, results[0]);

			scheduleTypeFilter.Property = ApprovedDiagramTypeList.Codes.NonApprovedDiagram;

			results = new BusinessObjectFactory().Load<ProcessHeader>(bizo.Filter);
			AssertEquals(0, results.Length);

			diagram.UnApprove();
			Factory.Save();

			results = new BusinessObjectFactory().Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			BMSTestCaseWithFactory.AssertSamePK(jobHeader, results[0]);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ApprovedWorkflowScheduleTypeFilter();
		}
	}
}
