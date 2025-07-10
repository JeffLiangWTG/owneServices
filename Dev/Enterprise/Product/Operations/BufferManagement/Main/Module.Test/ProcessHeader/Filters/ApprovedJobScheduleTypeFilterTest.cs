using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ApprovedJobScheduleTypeFilter))]
	class ApprovedJobScheduleTypeFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestApprovedScheduleType()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, isScaled: true);
			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			var filter = new ApprovedJobScheduleTypeFilter(job.GetType());
			filter.IsActive = true;
			filter.Property = ApprovedDiagramTypeList.Codes.CCPMApprovedDiagram;

			var results = new BusinessObjectFactory().Load<OrgHeader>(filter.Query);
			AssertEquals(0, results.Length);

			diagram.IsBuffered = true;
			Factory.Save();

			results = new BusinessObjectFactory().Load<OrgHeader>(filter.Query);
			AssertEquals(1, results.Length);
			BMSTestCaseWithFactory.AssertSamePK(job, results[0]);

			filter.Property = ApprovedDiagramTypeList.Codes.NonApprovedDiagram;

			results = new BusinessObjectFactory().Load<OrgHeader>(filter.Query);
			AssertEquals(0, results.Length);

			diagram.UnApprove();
			Factory.Save();

			results = new BusinessObjectFactory().Load<OrgHeader>(filter.Query);
			AssertEquals(1, results.Length);
			BMSTestCaseWithFactory.AssertSamePK(job, results[0]);
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
			return new ApprovedJobScheduleTypeFilter(typeof(OrgHeader));
		}
	}
}
