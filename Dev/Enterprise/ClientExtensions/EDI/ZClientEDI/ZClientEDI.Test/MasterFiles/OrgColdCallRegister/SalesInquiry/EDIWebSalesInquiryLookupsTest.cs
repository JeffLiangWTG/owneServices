using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIWebSalesInquiryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobRoleList()
		{
			AssertNotNull(Inquiry.Lookups.JobRoleList);
		}

		public void TestTypeOfBusiness()
		{
			AssertNotNull(Inquiry.Lookups.TypeOfBusinessList);
		}

		public void TestReasonForRequestingAccessList()
		{
			AssertNotNull(Inquiry.Lookups.ReasonForRequestingAccessList);
		}

		public void TestCompanySizeList()
		{
			AssertNotNull(Inquiry.Lookups.CompanySizeList);
		}

		public void TestCountries()
		{
			RefCountry country = Factory.Load<RefCountry>(new ZQuery())[0];
			country.RN_IsActive = false;
			Factory.Save();
			AssertNotNull(Inquiry.Lookups.Countries);
			AssertCollectionNotContains(country, Inquiry.Lookups.Countries);
		}

		EDIWebSalesInquiry Inquiry;
		protected override void SetUp()
		{
			base.SetUp();
			Inquiry = Factory.New<EDIWebSalesInquiry>();
		}
	}
}
