using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class MovementAuthorisationWrapperTest : Customs.Business.Testing.DataProviderTestCase<MovementAuthorisationWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal AuthorizationNumber.", "auth12", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type should be equal to the code corresponding to the AuthorizationCode in List CL236.", "C520", Provider.Type);
		}

		protected override MovementAuthorisationWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.AuthorizationCode = "ACT";
			movementHeader.AuthorizationNumber = "auth12";
			return MovementAuthorisationWrapper.New(movementHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "ACT", "C520", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "EUN");
			Factory.Save();
		}
	}
}
