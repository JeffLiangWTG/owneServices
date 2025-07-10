using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_PT_PTTest : TestCase
	{
		public void TestGetNumberAsString()
		{
			AssertEquals("zero", new NumberToString_PT_PT().GetNumberAsString(0));
			AssertEquals("um", new NumberToString_PT_PT().GetNumberAsString(1));
			AssertEquals("três", new NumberToString_PT_PT().GetNumberAsString(3));
			AssertEquals("sete", new NumberToString_PT_PT().GetNumberAsString(7));
			AssertEquals("nove", new NumberToString_PT_PT().GetNumberAsString(9));
			AssertEquals("dez", new NumberToString_PT_PT().GetNumberAsString(10));
			AssertEquals("dezasseis", new NumberToString_PT_PT().GetNumberAsString(16));
			AssertEquals("dezenove", new NumberToString_PT_PT().GetNumberAsString(19));
			AssertEquals("trinta", new NumberToString_PT_PT().GetNumberAsString(30));
			AssertEquals("noventa e nove", new NumberToString_PT_PT().GetNumberAsString(99));
			AssertEquals("cem", new NumberToString_PT_PT().GetNumberAsString(100));
			AssertEquals("cento e um", new NumberToString_PT_PT().GetNumberAsString(101));
			AssertEquals("cento e sessenta", new NumberToString_PT_PT().GetNumberAsString(160));
			AssertEquals("novecentos e noventa e nove", new NumberToString_PT_PT().GetNumberAsString(999));
			AssertEquals("mil", new NumberToString_PT_PT().GetNumberAsString(1000));
			AssertEquals("mil e cem", new NumberToString_PT_PT().GetNumberAsString(1100));
			AssertEquals("mil e cento e um", new NumberToString_PT_PT().GetNumberAsString(1101));
			AssertEquals("novecentos e noventa e nove mil e novecentos e noventa e nove", new NumberToString_PT_PT().GetNumberAsString(999999));
			AssertEquals("um milhão", new NumberToString_PT_PT().GetNumberAsString(1000000));
			AssertEquals("novecentos e noventa e nove milhões e novecentos e noventa e nove mil e novecentos e noventa e nove", new NumberToString_PT_PT().GetNumberAsString(999999999));
			AssertEquals("um bilhão", new NumberToString_PT_PT().GetNumberAsString(1000000000));
			AssertEquals("novecentos e noventa e nove bilhões e novecentos e noventa e nove milhões e novecentos e noventa e nove mil e novecentos e noventa e nove", new NumberToString_PT_PT().GetNumberAsString(999999999999));
		}
	}
}
