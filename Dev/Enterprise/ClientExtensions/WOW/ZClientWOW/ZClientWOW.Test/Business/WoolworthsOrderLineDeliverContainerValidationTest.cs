using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.Wow.Testing
{
	public class WoolworthsOrderLineDeliverContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateVessel()
		{
			WoolworthsOrderLineDeliverContainer container = Factory.New<WoolworthsOrderLineDeliverContainer>();
			AssertNoErrors(container.J5_RV_NKArrivalVesselInfo);
			container.J5_RV_NKArrivalVessel = "ABC";
			AssertNoErrors(container.J5_RV_NKArrivalVesselInfo);
			container.J5_RV_NKArrivalVessel = "";
			AssertHasErrors(container.J5_RV_NKArrivalVesselInfo);
			container.J5_RV_NKArrivalVessel = "DEF";
			AssertNoErrors(container.J5_RV_NKArrivalVesselInfo);
		}

		public void TestValidateVoyage()
		{
			WoolworthsOrderLineDeliverContainer container = Factory.New<WoolworthsOrderLineDeliverContainer>();
			AssertNoErrors(container.J5_VoyageInfo);
			container.J5_Voyage = "XYZ";
			AssertNoErrors(container.J5_VoyageInfo);
			container.J5_Voyage = "";
			AssertHasErrors(container.J5_VoyageInfo);
			container.J5_Voyage = "123";
			AssertNoErrors(container.J5_VoyageInfo);
		}
	}
}
