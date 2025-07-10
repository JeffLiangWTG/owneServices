using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_TR_TRTest : TestCase
	{
		public void TestGetNumberAsString()
		{
			var trk = new NumberToString_TR_TR();
			AssertEquals("sıfır", trk.GetNumberAsString(0));
			AssertEquals("bir", trk.GetNumberAsString(1));
			AssertEquals("iki", trk.GetNumberAsString(2));
			AssertEquals("üç", trk.GetNumberAsString(3));
			AssertEquals("dört", trk.GetNumberAsString(4));
			AssertEquals("beş", trk.GetNumberAsString(5));
			AssertEquals("altı", trk.GetNumberAsString(6));
			AssertEquals("yedi", trk.GetNumberAsString(7));
			AssertEquals("sekiz", trk.GetNumberAsString(8));
			AssertEquals("dokuz", trk.GetNumberAsString(9));
			AssertEquals("on", trk.GetNumberAsString(10));
			AssertEquals("onbir", trk.GetNumberAsString(11));
			AssertEquals("oniki", trk.GetNumberAsString(12));
			AssertEquals("onüç", trk.GetNumberAsString(13));
			AssertEquals("yirmi", trk.GetNumberAsString(20));
			AssertEquals("yirmibir", trk.GetNumberAsString(21));
			AssertEquals("yirmiiki", trk.GetNumberAsString(22));
			AssertEquals("otuz", trk.GetNumberAsString(30));
			AssertEquals("kırk", trk.GetNumberAsString(40));
			AssertEquals("elli", trk.GetNumberAsString(50));
			AssertEquals("altmış", trk.GetNumberAsString(60));
			AssertEquals("yetmiş", trk.GetNumberAsString(70));
			AssertEquals("seksen", trk.GetNumberAsString(80));
			AssertEquals("doksan", trk.GetNumberAsString(90));
			AssertEquals("yüz", trk.GetNumberAsString(100));
			AssertEquals("ikiyüz", trk.GetNumberAsString(200));
			AssertEquals("üçyüz", trk.GetNumberAsString(300));
			AssertEquals("bin", trk.GetNumberAsString(1000));
			AssertEquals("ikibin", trk.GetNumberAsString(2000));
			AssertEquals("üçbin", trk.GetNumberAsString(3000));
			AssertEquals("onbin", trk.GetNumberAsString(10000));
			AssertEquals("yirmibin", trk.GetNumberAsString(20000));
			AssertEquals("otuzbin", trk.GetNumberAsString(30000));
			AssertEquals("birmilyon", trk.GetNumberAsString(1000000));
			AssertEquals("birmilyar", trk.GetNumberAsString(1000000000));
			AssertEquals("yüzyirmiüçmilyondörtyüzellialtıbinyediyüzseksendokuz", trk.GetNumberAsString(123456789));
		}
	}
}
