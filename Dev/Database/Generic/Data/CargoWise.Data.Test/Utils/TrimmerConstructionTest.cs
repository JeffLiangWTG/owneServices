using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class TrimmerConstructionTest : TestCase
	{
		public void TestConstructionWithNoArgs()
		{
			AssertEquals("MaxCommandTextLength", 10000, new CommandTextTrimmer().maxCommandTextLength);
		}

		public void TestConstructionWithMaxCommandTextLength()
		{
			AssertEquals("MaxCommandTextLength", 50, new CommandTextTrimmer(50).maxCommandTextLength);
		}
	}
}
