namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	public class PlaceInfoLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("PortOfDestination", "AUADL", PlaceInfo.PortOfDestination);
			AssertEquals("PortOfDischarge", "AUADL", PlaceInfo.PortOfDischarge);
			AssertNotNull(PlaceInfo.OceanBill);
		}
	}
}
