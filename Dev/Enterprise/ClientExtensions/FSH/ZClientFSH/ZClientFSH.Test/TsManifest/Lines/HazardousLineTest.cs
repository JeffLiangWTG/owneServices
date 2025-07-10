namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	public class HazardousLineTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("IMOClass", "5.1", Hazardous.IMOClass);
			AssertEquals("FlashPoint", "0", Hazardous.FlashPoint);
			AssertEquals("IMOUNNumber", "66", Hazardous.IMOUNNumber);
			AssertNotNull(Hazardous.CargoField);
		}
	}
}
