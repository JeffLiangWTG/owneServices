using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_FR_FRTest : TestCase
	{
		public void TestGetNumberAsString()
		{
			var frn = new NumberToString_FR_FR();
			AssertEquals("un", frn.GetNumberAsString(1));
			AssertEquals("deux", frn.GetNumberAsString(2));
			AssertEquals("trois", frn.GetNumberAsString(3));
			AssertEquals("quatre", frn.GetNumberAsString(4));
			AssertEquals("cinq", frn.GetNumberAsString(5));
			AssertEquals("six", frn.GetNumberAsString(6));
			AssertEquals("sept", frn.GetNumberAsString(7));
			AssertEquals("huit", frn.GetNumberAsString(8));
			AssertEquals("neuf", frn.GetNumberAsString(9));
			AssertEquals("dix", frn.GetNumberAsString(10));
			AssertEquals("onze", frn.GetNumberAsString(11));
			AssertEquals("douze", frn.GetNumberAsString(12));
			AssertEquals("treize", frn.GetNumberAsString(13));
			AssertEquals("quatorze", frn.GetNumberAsString(14));
			AssertEquals("quinze", frn.GetNumberAsString(15));
			AssertEquals("seize", frn.GetNumberAsString(16));
			AssertEquals("dix-sept", frn.GetNumberAsString(17));
			AssertEquals("dix-huit", frn.GetNumberAsString(18));
			AssertEquals("dix-neuf", frn.GetNumberAsString(19));
			AssertEquals("vingt", frn.GetNumberAsString(20));
			AssertEquals("vingt-et-un", frn.GetNumberAsString(21));
			AssertEquals("vingt-deux", frn.GetNumberAsString(22));
			AssertEquals("vingt-trois", frn.GetNumberAsString(23));
			AssertEquals("vingt-quatre", frn.GetNumberAsString(24));
			AssertEquals("vingt-cinq", frn.GetNumberAsString(25));
			AssertEquals("vingt-six", frn.GetNumberAsString(26));
			AssertEquals("vingt-sept", frn.GetNumberAsString(27));
			AssertEquals("vingt-huit", frn.GetNumberAsString(28));
			AssertEquals("vingt-neuf", frn.GetNumberAsString(29));
			AssertEquals("trente", frn.GetNumberAsString(30));
			AssertEquals("trente-cinq", frn.GetNumberAsString(35));
			AssertEquals("quarante", frn.GetNumberAsString(40));
			AssertEquals("cinquante", frn.GetNumberAsString(50));
			AssertEquals("soixante", frn.GetNumberAsString(60));
			AssertEquals("soixante-dix", frn.GetNumberAsString(70));
			AssertEquals("quatre-vingts", frn.GetNumberAsString(80));
			AssertEquals("quatre-vingt-dix", frn.GetNumberAsString(90));
			AssertEquals("cent", frn.GetNumberAsString(100));
			AssertEquals("cent dix", frn.GetNumberAsString(110));
			AssertEquals("cent vingt", frn.GetNumberAsString(120));
			AssertEquals("cent trente", frn.GetNumberAsString(130));
			AssertEquals("cent quarante", frn.GetNumberAsString(140));
			AssertEquals("cent cinquante", frn.GetNumberAsString(150));
			AssertEquals("deux cents", frn.GetNumberAsString(200));
			AssertEquals("deux cent soixante-douze", frn.GetNumberAsString(272));
			AssertEquals("neuf cent quatre-vingt-dix-neuf", frn.GetNumberAsString(999));
			AssertEquals("mille", frn.GetNumberAsString(1000));
			AssertEquals("mille deux cent quarante-un", frn.GetNumberAsString(1241));
			AssertEquals("cinq mille trois cent soixante-huit", frn.GetNumberAsString(5368));
			AssertEquals("dix mille", frn.GetNumberAsString(10000));
			AssertEquals("cinquante mille", frn.GetNumberAsString(50000));
			AssertEquals("cent mille", frn.GetNumberAsString(100000));
			AssertEquals("cent cinquante mille", frn.GetNumberAsString(150000));
			AssertEquals("trois cent quatre-vingt-deux mille cent vingt-et-un", frn.GetNumberAsString(382121));
			AssertEquals("un million", frn.GetNumberAsString(1000000));
			AssertEquals("deux millions", frn.GetNumberAsString(2000000));
			AssertEquals("deux millions trois cent quarante-six mille vingt-deux", frn.GetNumberAsString(2346022));
		}
	}
}
