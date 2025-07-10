using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Utilities.Testing
{
	sealed class CapitaliseApostropheSuffixTest : TestCase
	{
		public void TestConvert()
		{
			var converter = new CapitaliseApostropheSuffix(4);
			AssertEquals("O'Riordan", converter.Convert("O'riordan"));
			AssertEquals("O'Deed", converter.Convert("O'deed"));
			AssertEquals("mc'Daniel", converter.Convert("mc'daniel"));

			AssertEquals("that's", converter.Convert("that's"));
			AssertEquals("I'll", converter.Convert("I'll"));
			AssertEquals("they're", converter.Convert("they're"));
			AssertEquals("c'est", converter.Convert("c'est"));

			var converter2 = new CapitaliseApostropheSuffix(2);
			AssertEquals("O'Riordan", converter2.Convert("O'riordan"));
			AssertEquals("O'Deed", converter2.Convert("O'deed"));

			AssertEquals("that's", converter2.Convert("that's"));
			AssertEquals("I'Ll", converter2.Convert("I'll"));
			AssertEquals("they'Re", converter2.Convert("they're"));
		}
	}
}
