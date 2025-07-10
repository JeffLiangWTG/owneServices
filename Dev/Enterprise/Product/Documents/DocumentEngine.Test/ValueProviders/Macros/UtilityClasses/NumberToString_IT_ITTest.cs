using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_IT_ITTest : TestCase
	{
		public void TestGetNumberAsString()
		{
			var itl = new NumberToString_IT_IT();

			AssertEquals("uno", itl.GetNumberAsString(1));
			AssertEquals("due", itl.GetNumberAsString(2));
			AssertEquals("tre", itl.GetNumberAsString(3));
			AssertEquals("quattro", itl.GetNumberAsString(4));
			AssertEquals("cinque", itl.GetNumberAsString(5));
			AssertEquals("sei", itl.GetNumberAsString(6));
			AssertEquals("sette", itl.GetNumberAsString(7));
			AssertEquals("otto", itl.GetNumberAsString(8));
			AssertEquals("nove", itl.GetNumberAsString(9));
			AssertEquals("dieci", itl.GetNumberAsString(10));
			AssertEquals("undici", itl.GetNumberAsString(11));
			AssertEquals("dodici", itl.GetNumberAsString(12));
			AssertEquals("tredici", itl.GetNumberAsString(13));
			AssertEquals("quattordici", itl.GetNumberAsString(14));
			AssertEquals("quindici", itl.GetNumberAsString(15));
			AssertEquals("sedici", itl.GetNumberAsString(16));
			AssertEquals("diciassette", itl.GetNumberAsString(17));
			AssertEquals("diciotto", itl.GetNumberAsString(18));
			AssertEquals("diciannove", itl.GetNumberAsString(19));
			AssertEquals("venti", itl.GetNumberAsString(20));
			AssertEquals("ventuno", itl.GetNumberAsString(21));
			AssertEquals("ventidue", itl.GetNumberAsString(22));
			AssertEquals("ventitré", itl.GetNumberAsString(23));
			AssertEquals("ventiquattro", itl.GetNumberAsString(24));
			AssertEquals("venticinque", itl.GetNumberAsString(25));
			AssertEquals("ventisei", itl.GetNumberAsString(26));
			AssertEquals("ventisette", itl.GetNumberAsString(27));
			AssertEquals("ventotto", itl.GetNumberAsString(28));
			AssertEquals("ventinove", itl.GetNumberAsString(29));
			AssertEquals("trenta", itl.GetNumberAsString(30));
			AssertEquals("trentacinque", itl.GetNumberAsString(35));
			AssertEquals("quaranta", itl.GetNumberAsString(40));
			AssertEquals("cinquanta", itl.GetNumberAsString(50));
			AssertEquals("sessanta", itl.GetNumberAsString(60));
			AssertEquals("settanta", itl.GetNumberAsString(70));
			AssertEquals("ottanta", itl.GetNumberAsString(80));
			AssertEquals("novanta", itl.GetNumberAsString(90));
			AssertEquals("cento", itl.GetNumberAsString(100));
			AssertEquals("centodieci", itl.GetNumberAsString(110));
			AssertEquals("centoventi", itl.GetNumberAsString(120));
			AssertEquals("centotrenta", itl.GetNumberAsString(130));
			AssertEquals("centoquaranta", itl.GetNumberAsString(140));
			AssertEquals("centocinquanta", itl.GetNumberAsString(150));
			AssertEquals("duecento", itl.GetNumberAsString(200));
			AssertEquals("duecentosettantadue", itl.GetNumberAsString(272));
			AssertEquals("novecentonovantanove", itl.GetNumberAsString(999));
			AssertEquals("mille", itl.GetNumberAsString(1000));
			AssertEquals("mille duecentoquarantuno", itl.GetNumberAsString(1241));
			AssertEquals("cinquemila trecentosessantotto", itl.GetNumberAsString(5368));
			AssertEquals("novemila novecentonovantanove", itl.GetNumberAsString(9999));
			AssertEquals("diecimila", itl.GetNumberAsString(10000));
			AssertEquals("cinquantamila", itl.GetNumberAsString(50000));
			AssertEquals("centomila", itl.GetNumberAsString(100000));
			AssertEquals("centocinquantamila", itl.GetNumberAsString(150000));
			AssertEquals("trecentoottantaduemila centoventuno", itl.GetNumberAsString(382121));
			AssertEquals("un milione", itl.GetNumberAsString(1000000));
			AssertEquals("due milioni", itl.GetNumberAsString(2000000));
			AssertEquals("due milioni trecentoquarantaseimila ventidue", itl.GetNumberAsString(2346022));
			AssertEquals("un miliardo", itl.GetNumberAsString(1000000000));
		}
	}
}
