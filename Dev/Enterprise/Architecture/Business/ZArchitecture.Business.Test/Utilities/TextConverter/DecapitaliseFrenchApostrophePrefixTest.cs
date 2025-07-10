using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Utilities.Testing
{
	sealed class DecapitaliseFrenchApostrophePrefixTest : TestCase
	{
		public void TestConvert()
		{
			var converter = new DecapitaliseFrenchApostrophePrefix();
			AssertEquals("d'Amerique", converter.Convert("D'Amerique"));
			AssertEquals("l'orthographe", converter.Convert("L'orthographe"));

			AssertEquals("O'Riordan", converter.Convert("O'Riordan"));
			AssertEquals("O'Deed", converter.Convert("O'Deed"));
			AssertEquals("Mc'Daniel", converter.Convert("Mc'Daniel"));
		}
	}
}
