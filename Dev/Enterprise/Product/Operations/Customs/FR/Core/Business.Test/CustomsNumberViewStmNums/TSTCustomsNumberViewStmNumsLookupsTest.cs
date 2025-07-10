using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CustomsNumberViewStmNums.Testing
{
	public class TSTCustomsNumberViewStmNumsLookupsTest : TestCaseWithFactory
	{
		public void TestTypeList()
		{
			AssertEquals("DDT", lookups.TypeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authorisation = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Number = "TS000040";
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			var provider = authorisation.CustomsNumberProvider;
			var stmNums = provider.CustomsNumbers.AddNew();
			lookups = (TSTCustomsNumberViewStmNumsLookups)stmNums.Lookups;
		}

		TSTCustomsNumberViewStmNumsLookups lookups;
	}
}
