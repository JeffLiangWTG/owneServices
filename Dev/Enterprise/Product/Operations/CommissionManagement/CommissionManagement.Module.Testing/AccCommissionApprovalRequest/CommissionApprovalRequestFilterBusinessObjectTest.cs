using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionApprovalRequestFilterBusinessObject))]
	internal class CommissionApprovalRequestFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Module Filters

		public void TestBatchNumberFilter()
		{
			var approvalRequestA = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequestA.CRQ_BatchNumber = "00001001";

			var approvalRequestB = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequestB.CRQ_BatchNumber = "00001002";

			var approvalRequestC = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			approvalRequestC.CRQ_BatchNumber = "00002002";

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var batchNumberFilter = (ModuleTextFilter)filterBizObj[CommissionApprovalRequestFilterBusinessObject.FilterDescription.BatchNumber];
			AssertNotNull(batchNumberFilter);
			AssertEquals("Batch Number", batchNumberFilter.MultilingualDescription);

			batchNumberFilter.IsActive = true;

			batchNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			batchNumberFilter.Property = "00001001";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { approvalRequestA });

			batchNumberFilter.Property = "00001002";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { approvalRequestB });

			batchNumberFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			batchNumberFilter.Property = "00001001";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { approvalRequestB, approvalRequestC });
		}

		public void TestApprovingStaffFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "RIS";

			var requestA = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			requestA.CRQ_GS_NKApprovingStaff1 = "ADL";

			var requestB = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			requestB.CRQ_GS_NKApprovingStaff1 = "ADL";
			requestB.CRQ_GS_NKApprovingStaff2 = "SCW";

			var requestC = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			requestC.CRQ_GS_NKApprovingStaff1 = "RIS";
			requestC.CRQ_GS_NKApprovingStaff2 = "ADL";

			var requestD = Factory.NewWithValidTestData<AccCommissionApprovalRequest>();
			requestD.CRQ_GS_NKApprovingStaff1 = "SCW";

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var approvingStaffFilter = (ModuleNkFilter)filterBizObj[CommissionApprovalRequestFilterBusinessObject.FilterDescription.ApprovingStaff];
			AssertNotNull(approvingStaffFilter);
			AssertEquals("Authorization Staff", approvingStaffFilter.MultilingualDescription);

			approvingStaffFilter.IsActive = true;

			approvingStaffFilter.Property = "ADL";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { requestA, requestB, requestC });

			approvingStaffFilter.Property = "SCW";
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { requestB, requestD });
		}

		public void TestStatusFilter()
		{
			var request1 = Factory.New<AccCommissionApprovalRequest>();
			request1.CRQ_BatchNumber = "0001001";
			request1.CRQ_GS_NKApprovingStaff1 = ZString.Empty;
			request1.CRQ_GS_NKApprovingStaff2 = ZString.Empty;
			request1.CRQ_Staff1HasApproved = false;
			request1.CRQ_Staff2HasApproved = false;
			request1.Items.AddNew().FillWithValidTestData();

			var request2 = Factory.New<AccCommissionApprovalRequest>();
			request2.CRQ_BatchNumber = "0001002";
			request2.CRQ_GS_NKApprovingStaff1 = "ADL";
			request2.CRQ_GS_NKApprovingStaff2 = ZString.Empty;
			request2.CRQ_Staff1HasApproved = false;
			request2.CRQ_Staff2HasApproved = false;
			request2.Items.AddNew().FillWithValidTestData();

			var request3 = Factory.New<AccCommissionApprovalRequest>();
			request3.CRQ_BatchNumber = "0001003";
			request3.CRQ_GS_NKApprovingStaff1 = "ADL";
			request3.CRQ_GS_NKApprovingStaff2 = ZString.Empty;
			request3.CRQ_Staff1HasApproved = true;
			request3.CRQ_Staff2HasApproved = false;
			request3.Items.AddNew().FillWithValidTestData();

			var request4 = Factory.New<AccCommissionApprovalRequest>();
			request4.CRQ_BatchNumber = "0001004";
			request4.CRQ_GS_NKApprovingStaff1 = "ADL";
			request4.CRQ_GS_NKApprovingStaff2 = "SCW";
			request4.CRQ_Staff1HasApproved = false;
			request4.CRQ_Staff2HasApproved = false;
			request4.Items.AddNew().FillWithValidTestData();

			var request5 = Factory.New<AccCommissionApprovalRequest>();
			request5.CRQ_BatchNumber = "0001005";
			request5.CRQ_GS_NKApprovingStaff1 = "ADL";
			request5.CRQ_GS_NKApprovingStaff2 = "SCW";
			request5.CRQ_Staff1HasApproved = true;
			request5.CRQ_Staff2HasApproved = false;
			request5.Items.AddNew().FillWithValidTestData();

			var request6 = Factory.New<AccCommissionApprovalRequest>();
			request6.CRQ_BatchNumber = "0001006";
			request6.CRQ_GS_NKApprovingStaff1 = "ADL";
			request6.CRQ_GS_NKApprovingStaff2 = "SCW";
			request6.CRQ_Staff1HasApproved = true;
			request6.CRQ_Staff2HasApproved = true;
			request6.Items.AddNew().FillWithValidTestData();

			var canceledRequest = Factory.New<AccCommissionApprovalRequest>();
			canceledRequest.CRQ_BatchNumber = "0001007";
			canceledRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			canceledRequest.CRQ_GS_NKApprovingStaff2 = "SCW";
			canceledRequest.CRQ_Staff1HasApproved = true;
			canceledRequest.CRQ_Staff2HasApproved = false;

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var statusFilter = (ModuleTextFilter)filterBizObj[CommissionApprovalRequestFilterBusinessObject.FilterDescription.Status];
			AssertNotNull(statusFilter);
			AssertEquals("Status", statusFilter.MultilingualDescription);
			AssertEquals(FilterCategories.StatusAndFlags, statusFilter.Category);

			AssertContainsExactElementsInAnyOrder(
				new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual },
				statusFilter.ComparisonOperator_List.Cast<ICodeDescription>().Select(x => x.Code));

			statusFilter.IsActive = true;

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			statusFilter.Property = CommissionApprovalRequestStatusList.Codes.Approved;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request3, request6 });

			statusFilter.Property = CommissionApprovalRequestStatusList.Codes.Pending;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request2, request4, request5 });

			statusFilter.Property = CommissionApprovalRequestStatusList.Codes.Canceled;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { canceledRequest });

			statusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			statusFilter.Property = CommissionApprovalRequestApproveStatusList.Codes.Approved;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request2, request4, request5, canceledRequest });

			statusFilter.Property = CommissionApprovalRequestApproveStatusList.Codes.Pending;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request3, request6, canceledRequest });

			statusFilter.Property = CommissionApprovalRequestStatusList.Codes.Canceled;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request2, request3, request4, request5, request6 });
		}

		public void TestContainsCommissionForStaffFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();

			var staff1LineA = Factory.NewWithValidTestData<AccCommissionLine>();
			staff1LineA.CL0_GS_NKStaff = staff1.GS_Code;
			var staff1LineB = Factory.NewWithValidTestData<AccCommissionLine>();
			staff1LineB.CL0_GS_NKStaff = staff1.GS_Code;

			var staff2LineA = Factory.NewWithValidTestData<AccCommissionLine>();
			staff2LineA.CL0_GS_NKStaff = staff2.GS_Code;
			var staff2LineB = Factory.NewWithValidTestData<AccCommissionLine>();
			staff2LineB.CL0_GS_NKStaff = staff2.GS_Code;

			var staff3LineA = Factory.NewWithValidTestData<AccCommissionLine>();
			staff3LineA.CL0_GS_NKStaff = staff3.GS_Code;
			var staff3LineB = Factory.NewWithValidTestData<AccCommissionLine>();
			staff3LineB.CL0_GS_NKStaff = staff3.GS_Code;

			var request1 = Factory.New<AccCommissionApprovalRequest>();
			request1.Items.AddNew().CRI_CL0 = staff1LineA.PK;
			request1.Items.AddNew().CRI_CL0 = staff1LineB.PK;
			request1.Items.AddNew().CRI_CL0 = staff2LineA.PK;
			request1.Items.AddNew().CRI_CL0 = staff3LineA.PK;

			var request2 = Factory.New<AccCommissionApprovalRequest>();
			request2.Items.AddNew().CRI_CL0 = staff2LineB.PK;
			request2.Items.AddNew().CRI_CL0 = staff3LineB.PK;

			var request3 = Factory.New<AccCommissionApprovalRequest>();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var statusFilter = (ModuleNkFilter)filterBizObj[CommissionApprovalRequestFilterBusinessObject.FilterDescription.ContainsCommissionForStaff];
			AssertNotNull(statusFilter);
			AssertEquals("Contains Commission for Staff", statusFilter.MultilingualDescription);

			statusFilter.IsActive = true;

			statusFilter.Property = staff1.GS_Code;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, });

			statusFilter.Property = staff2.GS_Code;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request2 });

			statusFilter.Property = staff3.GS_Code;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request2 });
		}

		public void TestContainsCommissionForPartyFilter()
		{
			var party1 = Factory.NewWithValidTestData<OrgHeader>();
			var party2 = Factory.NewWithValidTestData<OrgHeader>();
			var party3 = Factory.NewWithValidTestData<OrgHeader>();

			var party1LineA = Factory.NewWithValidTestData<AccCommissionLine>();
			party1LineA.CL0_OH_Party = party1.PK;
			var party1LineB = Factory.NewWithValidTestData<AccCommissionLine>();
			party1LineB.CL0_OH_Party = party1.PK;

			var party2LineA = Factory.NewWithValidTestData<AccCommissionLine>();
			party2LineA.CL0_OH_Party = party2.PK;
			var party2LineB = Factory.NewWithValidTestData<AccCommissionLine>();
			party2LineB.CL0_OH_Party = party2.PK;

			var party3LineA = Factory.NewWithValidTestData<AccCommissionLine>();
			party3LineA.CL0_OH_Party = party3.PK;
			var party3LineB = Factory.NewWithValidTestData<AccCommissionLine>();
			party3LineB.CL0_OH_Party = party3.PK;

			var request1 = Factory.New<AccCommissionApprovalRequest>();
			request1.Items.AddNew().CRI_CL0 = party1LineA.PK;
			request1.Items.AddNew().CRI_CL0 = party1LineB.PK;
			request1.Items.AddNew().CRI_CL0 = party2LineA.PK;
			request1.Items.AddNew().CRI_CL0 = party3LineA.PK;

			var request2 = Factory.New<AccCommissionApprovalRequest>();
			request2.Items.AddNew().CRI_CL0 = party2LineB.PK;
			request2.Items.AddNew().CRI_CL0 = party3LineB.PK;

			var request3 = Factory.New<AccCommissionApprovalRequest>();

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			var statusFilter = (ModuleGuidFilter)filterBizObj[CommissionApprovalRequestFilterBusinessObject.FilterDescription.ContainsCommissionForParty];
			AssertNotNull(statusFilter);
			AssertEquals("Contains Commission for Organization", statusFilter.MultilingualDescription);

			statusFilter.IsActive = true;

			statusFilter.Property = party1.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1 });

			statusFilter.Property = party2.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request2 });

			statusFilter.Property = party3.PK;
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { request1, request2 });
		}

		#endregion

		#region Implementation

		void AssertContainsExactElementsInAnyOrder(FilterBusinessObject filterBizObj, IEnumerable<AccCommissionApprovalRequest> expectedApprovalRequests)
		{
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionApprovalRequest>.PKOnlyComparer,
				x => x.CRQ_BatchNumber,
				expectedApprovalRequests,
				Factory.Load<AccCommissionApprovalRequest>(filterBizObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommissionApprovalRequestFilterBusinessObject();
		}

		#endregion
	}
}
