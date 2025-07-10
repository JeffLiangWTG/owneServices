using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class CommissionLineSourceGroupingValidationTest : BusinessObjectValidationTestCase
	{
		#region Properties

		public void TestCheckCompanyPk_NoErrorIfNotActive()
		{
			var inactiveCompany = Factory.New<GlbCompany>();
			inactiveCompany.GC_IsActive = false;

			var inactiveCompanyLine = Factory.New<ViewCommissionLine>();
			inactiveCompanyLine.VCL_GC_Company = inactiveCompany.PK;
			var grouping = GetNewGrouping(Factory, new[] { inactiveCompanyLine });
			grouping.Validation.ValidateCompanyPk();

			AssertNoErrors(grouping.CompanyPkInfo);
		}

		public void TestCheckStaff_NoErrorIfNotActive()
		{
			var inactiveStaff = Factory.New<GlbStaff>();
			inactiveStaff.GS_Code = "ADL";
			inactiveStaff.GS_IsActive = false;

			var inactiveStaffLine = Factory.New<ViewCommissionLine>();
			inactiveStaffLine.VCL_GS_NKStaff = inactiveStaff.GS_Code;
			var grouping = GetNewGrouping(Factory, new[] { inactiveStaffLine });
			grouping.Validation.ValidateStaffCode();

			AssertNoErrors(grouping.StaffCodeInfo);
		}

		public void TestCheckPartyPk_NoErrorIfNotActive()
		{
			var inactiveOrg = Factory.New<OrgHeader>();
			inactiveOrg.OH_IsActive = false;

			var inactiveOrgLine = Factory.New<ViewCommissionLine>();
			inactiveOrgLine.VCL_OH_Party = inactiveOrg.PK;
			var grouping = GetNewGrouping(Factory, new[] { inactiveOrgLine });
			grouping.Validation.ValidatePartyPk();

			AssertNoErrors(grouping.PartyPkInfo);
		}

		#endregion

		#region Implementation

		static CommissionLineGroupingForTest GetNewGrouping(BusinessObjectFactory factory, IEnumerable<ViewCommissionLine> commissionLines)
		{
			var grouping = new CommissionLineGroupingForTest(factory);
			grouping.Init(commissionLines);
			return grouping;
		}

		#endregion
	}
}
