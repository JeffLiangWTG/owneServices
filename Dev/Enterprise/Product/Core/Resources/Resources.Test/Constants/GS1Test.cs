using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class GS1Test : TestCase
	{
		public void TestGS1PrefixLengths()
		{
			AssertEquals(7, Constants.GS1.PrefixMinLength);
			AssertEquals(11, Constants.GS1.PrefixMaxLength);
		}
	}
}
