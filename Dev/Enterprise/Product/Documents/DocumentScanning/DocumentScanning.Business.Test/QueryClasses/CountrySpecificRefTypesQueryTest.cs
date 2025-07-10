using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class CountrySpecificRefTypesQueryTestForAU : TransactionedTestCase
	{
		public void TestQueryClass()
		{
			AssertEquals("Precondition: Country is AU", "AU", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Assert(!AUQuery.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AUQuery = new CountrySpecificRefTypesQuery();
			GlbCompany.CurrentCompany.SetCountry("AU");
		}

		CountrySpecificRefTypesQuery AUQuery;
	}

	public class CountrySpecificRefTypesQueryTestForSG : TransactionedTestCase
	{
		public void TestQueryClass()
		{
			AssertEquals("Precondition: Country is SG", "SG", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			Assert(!SGQuery.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SGQuery = new CountrySpecificRefTypesQuery();
			GlbCompany.CurrentCompany.SetCountry("SG");
		}

		CountrySpecificRefTypesQuery SGQuery;
	}
}
