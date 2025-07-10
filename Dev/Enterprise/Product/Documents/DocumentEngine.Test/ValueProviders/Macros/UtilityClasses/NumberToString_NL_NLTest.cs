using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_NL_NLTest : TestCase
	{
		public void TestGetNumberAsString()
		{
			var dch = new NumberToString_NL_NL();
			AssertEquals("één", dch.GetNumberAsString(1));
			AssertEquals("twee", dch.GetNumberAsString(2));
			AssertEquals("drie", dch.GetNumberAsString(3));
			AssertEquals("vier", dch.GetNumberAsString(4));
			AssertEquals("vijf", dch.GetNumberAsString(5));
			AssertEquals("zes", dch.GetNumberAsString(6));
			AssertEquals("zeven", dch.GetNumberAsString(7));
			AssertEquals("acht", dch.GetNumberAsString(8));
			AssertEquals("negen", dch.GetNumberAsString(9));
			AssertEquals("tien", dch.GetNumberAsString(10));
			AssertEquals("elf", dch.GetNumberAsString(11));
			AssertEquals("twaalf", dch.GetNumberAsString(12));
			AssertEquals("dertien", dch.GetNumberAsString(13));
			AssertEquals("veertien", dch.GetNumberAsString(14));
			AssertEquals("vijftien", dch.GetNumberAsString(15));
			AssertEquals("zestien", dch.GetNumberAsString(16));
			AssertEquals("zeventien", dch.GetNumberAsString(17));
			AssertEquals("achttien", dch.GetNumberAsString(18));
			AssertEquals("negentien", dch.GetNumberAsString(19));
			AssertEquals("twintig", dch.GetNumberAsString(20));
			AssertEquals("éénentwintig", dch.GetNumberAsString(21));
			AssertEquals("tweeëntwintig", dch.GetNumberAsString(22));
			AssertEquals("drieëntwintig", dch.GetNumberAsString(23));
			AssertEquals("vierentwintig", dch.GetNumberAsString(24));
			AssertEquals("vijfentwintig", dch.GetNumberAsString(25));
			AssertEquals("zesentwintig", dch.GetNumberAsString(26));
			AssertEquals("zevenentwintig", dch.GetNumberAsString(27));
			AssertEquals("achtentwintig", dch.GetNumberAsString(28));
			AssertEquals("negenentwintig", dch.GetNumberAsString(29));
			AssertEquals("dertig", dch.GetNumberAsString(30));
			AssertEquals("vijfendertig", dch.GetNumberAsString(35));
			AssertEquals("veertig", dch.GetNumberAsString(40));
			AssertEquals("vijftig", dch.GetNumberAsString(50));
			AssertEquals("zestig", dch.GetNumberAsString(60));
			AssertEquals("zeventig", dch.GetNumberAsString(70));
			AssertEquals("tachtig", dch.GetNumberAsString(80));
			AssertEquals("negentig", dch.GetNumberAsString(90));
			AssertEquals("honderd", dch.GetNumberAsString(100));
			AssertEquals("honderd-en-tien", dch.GetNumberAsString(110));
			AssertEquals("honderd-en-twintig", dch.GetNumberAsString(120));
			AssertEquals("honderd-en-dertig", dch.GetNumberAsString(130));
			AssertEquals("honderd-en-veertig", dch.GetNumberAsString(140));
			AssertEquals("honderd-en-vijftig", dch.GetNumberAsString(150));
			AssertEquals("tweehonderd", dch.GetNumberAsString(200));
			AssertEquals("tweehonderd-en-tweeënzeventig", dch.GetNumberAsString(272));
			AssertEquals("negenhonderd-en-negenennegentig", dch.GetNumberAsString(999));
			AssertEquals("duizend", dch.GetNumberAsString(1000));
			AssertEquals("duizend tweehonderd-en-éénenveertig", dch.GetNumberAsString(1241));
			AssertEquals("vijfduizend driehonderd-en-achtenzestig", dch.GetNumberAsString(5368));
			AssertEquals("tienduizend", dch.GetNumberAsString(10000));
			AssertEquals("vijftigduizend", dch.GetNumberAsString(50000));
			AssertEquals("honderdduizend", dch.GetNumberAsString(100000));
			AssertEquals("honderd-en-vijftigduizend", dch.GetNumberAsString(150000));
			AssertEquals("driehonderd-en-tweeëntachtigduizend honderd-en-éénentwintig", dch.GetNumberAsString(382121));
			AssertEquals("één miljoen", dch.GetNumberAsString(1000000));
			AssertEquals("twee miljoen", dch.GetNumberAsString(2000000));
			AssertEquals("twee miljoen driehonderd-en-zesenveertigduizend tweeëntwintig", dch.GetNumberAsString(2346022));
			AssertEquals("één miljard", dch.GetNumberAsString(1000000000));
		}
	}
}
