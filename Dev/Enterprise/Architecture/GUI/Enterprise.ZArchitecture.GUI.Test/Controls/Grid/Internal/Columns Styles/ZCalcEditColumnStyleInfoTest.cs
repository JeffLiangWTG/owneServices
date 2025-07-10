namespace Enterprise.ZArchitecture
{
	sealed class ZCalcEditColumnStyleInfoTest : NUnit.Framework.TestCase
	{
		public void TestBindToDecimalPlaces()
		{
			var info = new ZCalcEditColumnStyleInfo { BindToDecimalPlaces = "" };
			AssertEquals("", info.BindToDecimalPlaces);
			info.BindToDecimalPlaces = "Noodle";
			AssertEquals("Noodle", info.BindToDecimalPlaces);
		}
	}
}
