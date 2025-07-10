namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	sealed class VesselVoyageLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("Vessel", "AFRICA STAR", VesselVoyage.Vessel);
			AssertEquals("VesselCode", "8707434", VesselVoyage.VesselCode);
			AssertEquals("Voyage", "504S", VesselVoyage.Voyage);
		}

		public void TestOceanBills()
		{
			AssertEquals("Precondition: no oceanbills", 0, VesselVoyage.OceanBills.Count);
			var newOceanBill = VesselVoyage.AddNewOceanBill(OceanBillRow);
			AssertNotNull(newOceanBill);
			AssertEquals(1, VesselVoyage.OceanBills.Count);
			AssertEquals(newOceanBill, VesselVoyage.OceanBills[0]);
		}
	}
}
