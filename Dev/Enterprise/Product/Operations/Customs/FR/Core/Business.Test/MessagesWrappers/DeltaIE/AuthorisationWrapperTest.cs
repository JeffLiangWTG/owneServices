using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class AuthorisationWrapperTest : Customs.Business.Testing.DataProviderTestCase<AuthorisationWrapper>
	{
		protected override AuthorisationWrapper GetProvider()
		{
			var owner = Factory.New<OrgHeader>();
			owner.FillWithValidTestData();
			owner.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			owner.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			Factory.Save();

			var auth = Factory.New<CusAuthorizationUsage>();

			auth.AGC_OH_Owner = owner.PK;
			auth.EffectiveReferenceNumber = "5678";
			auth.AGC_Code = "ACT";
			return AuthorisationWrapper.New(auth);
		}

		public void TestHolderOfTheAuthorisation()
		{
			AssertEquals("HolderOfTheAuthorisation should be equal to Eori number.", "FR12345678900001", Provider.HolderOfTheAuthorisation);
		}

		public void TestType()
		{
			SetUpRefData();
			AssertEquals("Type should be equal to mapped from AGC_Code to EUNAU codes.", "C520", Provider.Type);

			void SetUpRefData()
			{
				var startDate = ZDateTime.Today.AddDays(-1);
				var endDate = ZDateTime.Today.AddDays(1);
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
				helper.CreateCusMap("EUNAU", "ACT", "C520", startDate, endDate, "EUN");
				Factory.Save();
			}
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be equal to EffectiveReferenceNumber.", "5678", Provider.ReferenceNumber);
		}
	}
}
