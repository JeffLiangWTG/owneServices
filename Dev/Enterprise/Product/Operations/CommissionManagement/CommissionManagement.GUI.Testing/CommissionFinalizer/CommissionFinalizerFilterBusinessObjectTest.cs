using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionFinalizerFilterBusinessObject))]
	internal class CommissionFinalizerFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filter

		public void TestFilter_DoesNotReturnCancelledNorPaid()
		{
			var commissionLine1 = Factory.NewWithValidTestData<AccCommissionLine>();
			var commissionLine2 = Factory.NewWithValidTestData<AccCommissionLine>();
			var paidCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			paidCommissionLine.CL0_PaidDateTimeUtc = new ZDateTime(2002, 2, 2);
			var cancelledCommissionLine = Factory.NewWithValidTestData<AccCommissionLine>();
			cancelledCommissionLine.CL0_CancelledDateTimeUtc = new ZDateTime(2002, 2, 2);

			Factory.Save();

			var filterBizObj = GetNewFilterStripBusinessObject();
			AssertContainsExactElementsInAnyOrder(filterBizObj, new[] { commissionLine1, commissionLine2 });
		}

		public void TestAddAlwaysVisibleFilterStrips()
		{
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var filterBizObj = new CommissionFinalizerFilterBusinessObject();

			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Company].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.RecognitionDate].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.CommissionStatus].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityStaff].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityOrganisation].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.AgreementId].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.ApprovalRequest].Visibility);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			filterBizObj = new CommissionFinalizerFilterBusinessObject();

			AssertNull(filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.Company]);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.RecognitionDate].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.CommissionStatus].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityStaff].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.EntityOrganisation].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.AgreementId].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filterBizObj[CommissionManagementFilterBusinessObject.FilterDescription.ApprovalRequest].Visibility);
		}

		#endregion

		#region Module Filters

		public void TestCommissionStatusesList()
		{
			var filterBizObj = GetNewFilterStripBusinessObject();
			var commissionStatusFilter = (ModuleTextFilter)filterBizObj[CommissionFinalizerFilterBusinessObject.FilterDescription.CommissionStatus];
			AssertNotNull(commissionStatusFilter);
			AssertCollectionNotContains(AccCommissionLineCommissionStatusList.Codes.Paid, commissionStatusFilter.List.Cast<ICodeDescription>().Select(x => x.Code));
		}

		#endregion

		#region Implementation

		void AssertContainsExactElementsInAnyOrder(FilterBusinessObject filterBizObj, IEnumerable<AccCommissionLine> expectedAccCommissionLines)
		{
			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<AccCommissionLine>.PKOnlyComparer,
				expectedAccCommissionLines,
				Factory.Load<ViewCommissionLine>(filterBizObj.Filter).Select(x => x.AccCommissionLine));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CommissionFinalizerFilterBusinessObject();
		}

		#endregion
	}
}
