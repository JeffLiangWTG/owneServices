using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	internal class AccConsolidationMemberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOrganisationOrCompanyIsMandatory()
		{
			var group = Factory.New<AccConsolidationGroup>();
			var member = group.GroupMembers.AddNew();
			member.RunPreSaveValidation();
			AssertHasErrors(member.YM_GC_CompanyInfo);
			AssertHasErrors(member.YM_OH_OrganisationInfo);

			var company = Factory.New<GlbCompany>();
			var org = Factory.New<OrgHeader>();

			member.YM_GC_Company = company.PK;
			AssertNoErrors(member.YM_GC_CompanyInfo);
			AssertNoErrors(member.YM_OH_OrganisationInfo);

			member.YM_OH_Organisation = org.PK;
			AssertHasErrors(member.YM_GC_CompanyInfo);
			AssertHasErrors(member.YM_OH_OrganisationInfo);

			member.YM_GC_Company = ZGuid.Empty;
			AssertNoErrors(member.YM_GC_CompanyInfo);
			AssertNoErrors(member.YM_OH_OrganisationInfo);

			member.YM_OH_Organisation = ZGuid.Empty;
			AssertHasErrors(member.YM_GC_CompanyInfo);
			AssertHasErrors(member.YM_OH_OrganisationInfo);

			member.YM_OH_Organisation = org.PK;
			AssertNoErrors(member.YM_GC_CompanyInfo);
			AssertNoErrors(member.YM_OH_OrganisationInfo);
		}
	}
}