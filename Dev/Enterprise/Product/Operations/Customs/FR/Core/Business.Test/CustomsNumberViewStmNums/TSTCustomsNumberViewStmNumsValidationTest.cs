using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CustomsNumberViewStmNums.Testing
{
	public class TSTCustomsNumberViewStmNumsValidationTest : TestCaseWithFactory
	{
		public void TestCheckSN_FountainName()
		{
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = stmNums.Wrapper;

			wrapper.SN_FountainName = "";
			AssertHasErrorContaining(wrapper.SN_FountainNameInfo, MandatoryValidation.MustBeEntered);

			wrapper.SN_FountainName = "GOODPREFIX";
			AssertNoErrors(wrapper.SN_FountainNameInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authorisation = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
			authorisation.CPH_Number = "TS000040";
			authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			provider = authorisation.CustomsNumberProvider;
		}

		CustomsNumberViewStmNumsBusinessProvider provider;
	}
}
