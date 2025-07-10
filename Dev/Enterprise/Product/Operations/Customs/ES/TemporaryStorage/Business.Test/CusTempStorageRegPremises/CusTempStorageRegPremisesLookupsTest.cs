using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing
{
	sealed class CusTempStorageRegPremisesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAuthorizationNumberList()
		{
			CombineAssertions(() =>
			{
				premises.SRP_Type = "ADT";
				var adtType = lookups.AuthorizationNumberList.FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>()[0].Value;
				AssertEquals("CPH_Type for ADT is TST", adtType, "TST");
				premises.SRP_Type = "LAM";
				var lamType = lookups.AuthorizationNumberList.FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>()[0].Value;
				AssertEquals("CPH_Type for LAM is LAME", lamType, "LAME");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			premises = Factory.New<CusTempStorageRegPremises>();
			lookups = premises.Lookups as CusTempStorageRegPremisesLookups;
		}
		CusTempStorageRegPremisesLookups lookups;
		CusTempStorageRegPremises premises;
	}
}
