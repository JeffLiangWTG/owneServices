using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_ES_ESTest : TestCase
	{
		public void TestGetNumberAsString()
		{
			AssertEquals("cero", new NumberToString_ES_ES().GetNumberAsString(0));
			AssertEquals("veintidós", new NumberToString_ES_ES().GetNumberAsString(22));
			AssertEquals("nueve", new NumberToString_ES_ES().GetNumberAsString(9));
			AssertEquals("diez", new NumberToString_ES_ES().GetNumberAsString(10));
			AssertEquals("catorce", new NumberToString_ES_ES().GetNumberAsString(14));
			AssertEquals("diecinueve", new NumberToString_ES_ES().GetNumberAsString(19));
			AssertEquals("veinte", new NumberToString_ES_ES().GetNumberAsString(20));
			AssertEquals("veintiuno", new NumberToString_ES_ES().GetNumberAsString(21));
			AssertEquals("veintidós", new NumberToString_ES_ES().GetNumberAsString(22));
			AssertEquals("veintitrés", new NumberToString_ES_ES().GetNumberAsString(23));
			AssertEquals("veinticuatro", new NumberToString_ES_ES().GetNumberAsString(24));
			AssertEquals("veinticinco", new NumberToString_ES_ES().GetNumberAsString(25));
			AssertEquals("veintiséis", new NumberToString_ES_ES().GetNumberAsString(26));
			AssertEquals("veintisiete", new NumberToString_ES_ES().GetNumberAsString(27));
			AssertEquals("veintiocho", new NumberToString_ES_ES().GetNumberAsString(28));
			AssertEquals("veintinueve", new NumberToString_ES_ES().GetNumberAsString(29));
			AssertEquals("treinta", new NumberToString_ES_ES().GetNumberAsString(30));
			AssertEquals("treinta y cuatro", new NumberToString_ES_ES().GetNumberAsString(34));
			AssertEquals("treinta y cinco", new NumberToString_ES_ES().GetNumberAsString(35));
			AssertEquals("treinta y seis", new NumberToString_ES_ES().GetNumberAsString(36));
			AssertEquals("treinta y siete", new NumberToString_ES_ES().GetNumberAsString(37));
			AssertEquals("treinta y ocho", new NumberToString_ES_ES().GetNumberAsString(38));
			AssertEquals("treinta y nueve", new NumberToString_ES_ES().GetNumberAsString(39));
			AssertEquals("noventa y nueve", new NumberToString_ES_ES().GetNumberAsString(99));
			AssertEquals("cien", new NumberToString_ES_ES().GetNumberAsString(100));
			AssertEquals("ciento uno", new NumberToString_ES_ES().GetNumberAsString(101));
			AssertEquals("ciento once", new NumberToString_ES_ES().GetNumberAsString(111));
			AssertEquals("novecientos noventa y nueve", new NumberToString_ES_ES().GetNumberAsString(999));
			AssertEquals("mil", new NumberToString_ES_ES().GetNumberAsString(1000));
			AssertEquals("novecientos noventa y nueve mil novecientos noventa y nueve", new NumberToString_ES_ES().GetNumberAsString(999999));
			AssertEquals("un millón", new NumberToString_ES_ES().GetNumberAsString(1000000));
			AssertEquals("novecientos noventa y nueve millones novecientos noventa y nueve mil novecientos noventa y nueve", new NumberToString_ES_ES().GetNumberAsString(999999999));
			AssertEquals("mil millones", new NumberToString_ES_ES().GetNumberAsString(1000000000));
			AssertEquals("novecientos noventa y nueve mil millones novecientos noventa y nueve millones novecientos noventa y nueve mil novecientos noventa y nueve", new NumberToString_ES_ES().GetNumberAsString(999999999999));
		}
	}
}
