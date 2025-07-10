using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Utilities.Testing
{
	sealed class DecapitaliseNumberSuffixTest : TestCase
	{
		public void TestConvert()
		{
			var converter = new DecapitaliseNumberSuffix();
			AssertEquals("1st", converter.Convert("1ST"));
			AssertEquals("3a/72", converter.Convert("3A/72"));

			AssertEquals("1/A", converter.Convert("1/A"));
		}
	}
}
