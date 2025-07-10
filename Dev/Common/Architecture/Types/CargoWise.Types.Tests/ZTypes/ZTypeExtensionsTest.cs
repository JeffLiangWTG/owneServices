using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	class ZTypeExtensionsTest : TestCase
	{
		ZString fallbackString => "empty";

		public void TestIfEmptyUse()
		{
			ZString testString = "Test";
			AssertEquals("Test", testString.IfEmptyUse(() => fallbackString));

			testString = "";
			AssertEquals("empty", testString.IfEmptyUse(() => fallbackString));
		}
	}
}
