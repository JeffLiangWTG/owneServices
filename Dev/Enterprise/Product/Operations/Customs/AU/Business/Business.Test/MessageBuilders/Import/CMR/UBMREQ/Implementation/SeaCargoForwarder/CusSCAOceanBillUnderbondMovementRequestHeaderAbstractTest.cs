using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusSCAOceanBillUnderbondMovementRequestHeaderAbstractTest : CusUnderbondUnderbondMovementRequestHeaderAbstractTest
	{
		public void TestEstimatedDateOfArrival()
		{
			AssertEquals("EstimatedDateOfArrival", ZDateTime.Empty, RequestHeader.EstimatedDateOfArrival);
		}

		public void TestFlightNumber()
		{
			AssertEquals("FlightNumber", ZString.Empty, RequestHeader.FlightNumber);
		}

		public void TestVesselID()
		{
			AssertEquals("VesselID", "8811924", RequestHeader.VesselID);
		}

		public void TestVoyageNumber()
		{
			AssertEquals("VoyageNumber", "123", RequestHeader.VoyageNumber);
		}

		public void TestIsBureau()
		{
			AssertEquals("Default value", false, RequestHeader.IsBureau);
		}

		CusSCAOceanBill scaOcean;
		protected CusSCAOceanBill SCAOcean
		{
			get
			{
				if (scaOcean == null)
				{
					scaOcean = Factory.New<CusSCAOceanBill>();
					scaOcean.CB_VesselName = "ADMIRALENGRACHT";
					scaOcean.CB_Voyage = "123";
				}
				return scaOcean;
			}
		}
	}
}
