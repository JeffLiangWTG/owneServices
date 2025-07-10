using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public partial class GVMSRouteCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateRouteID()
		{
			var gvmsManifest = Factory.New<AsycudaManifestHeader>();
			gvmsManifest.AMA_RL_NKPortOfLoading = "BEZEE";
			AssertEquals("Value will be blank when only load port is populated", "", gvmsManifest.CalculatedRouteID);

			gvmsManifest.AMA_RL_NKPortOfDischarge = "GBHUL";
			AssertEquals("Value will be blank when only load port and discharge port is populated", "", gvmsManifest.CalculatedRouteID);

			gvmsManifest.AMA_CarrierCode = "ABC";
			AssertEquals("Route R1 should be chosen BEZEE -> GBHUL", "R1", gvmsManifest.CalculatedRouteID);
			AssertEquals("When RouteID is blank it should be populated when calculating the route", "R1", gvmsManifest.RouteId);

			gvmsManifest.AMA_RL_NKPortOfDischarge = "GBPME";
			AssertEquals("Value will be blank when BEZEE -> GBPME for carrier ABC", "", gvmsManifest.CalculatedRouteID);
			AssertEquals("When RouteID is already populated it should be not be changed when calculating the route", "R1", gvmsManifest.RouteId);

			gvmsManifest.AMA_CarrierCode = "DEF";
			AssertEquals("Route R2 should be chosen BEZEE -> GBPME for carrier DEF", "R2", gvmsManifest.CalculatedRouteID);
			AssertEquals("When RouteID is already populated it should be not be changed when calculating the route", "R1", gvmsManifest.RouteId);

			gvmsManifest.Validation.ValidateRouteId();
			AssertHasMessageErrorContaining("", gvmsManifest.RouteIdInfo, "CargoWise has calculated that the route code for BEZEE->GBPME via carrier DEF should be R2");

			gvmsManifest.RouteId = "R2";
			gvmsManifest.Validation.ValidateRouteId();
			AssertNoMessageErrorContaining("", gvmsManifest.RouteIdInfo, "CargoWise has calculated that the route code for BEZEE->GBPME via carrier DEF should be R2");

			gvmsManifest.AMA_RL_NKPortOfDischarge = "GBSTN";
			AssertEquals("Value will be blank when BEZEE -> GBSTN for carrier DEF as GBSTN has no port id available", "", gvmsManifest.CalculatedRouteID);
			AssertEquals("When RouteID is already populated it should be not be changed when calculating the route", "R2", gvmsManifest.RouteId);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GVMSTestHelper.SetupPortsForRouteCalculation(Factory);
			GVMSTestHelper.SetupRoutesForRouteCalculation(Factory);
			GVMSTestHelper.SetUpCarriersForRouteCalculation(Factory);
		}
	}
}

