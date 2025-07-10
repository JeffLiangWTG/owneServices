using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class AuthorisationUsageWrapperTest : Customs.Business.Testing.DataProviderTestCase<AuthorisationUsageWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal EffectiveReferenceNumber.", "5678", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "ACT", "C520", startDate, endDate, "EUN");
			Factory.Save();

			AssertEquals("Type should equal AGC_Code.", "C520", Provider.Type);
		}

		protected override AuthorisationUsageWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var authorisationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorisationUsage.EffectiveReferenceNumber = "5678";
			authorisationUsage.AGC_Code = "ACT";
			return AuthorisationUsageWrapper.New(authorisationUsage);
		}
	}
}
