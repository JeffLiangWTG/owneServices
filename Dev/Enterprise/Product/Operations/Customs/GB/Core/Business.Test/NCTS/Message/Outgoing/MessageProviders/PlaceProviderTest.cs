using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class PlaceProviderTest : DataProviderTestCase<PlaceProvider>
	{
		public void TestUnLocode()
		{
			AssertEquals(nameof(PlaceProvider.UnLocode), "Loading Code", Provider.UnLocode);
		}

		public void TestCountry()
		{
			AssertEquals(nameof(PlaceProvider.Country), Core.Constants.CountryCodes.Belgium, Provider.Country);
		}

		public void TestLocation()
		{
			AssertEquals(nameof(PlaceProvider.Location), "Loading Code", Provider.Location);
		}

		protected override PlaceProvider GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var nctsMovementHeader = nctsHeader.MovementHeader;
			var refUnLoco = Factory.New<RefUNLOCO>();
			refUnLoco.RL_Code = "LCODE";
			refUnLoco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			refUnLoco.RL_PortName = "Loading Code";
			nctsMovementHeader.BM_RL_NKForeignDestPort = "LCODE";
			return new PlaceProvider(nctsMovementHeader.PlaceOfLoading, refUnLoco);
		}
	}
}
