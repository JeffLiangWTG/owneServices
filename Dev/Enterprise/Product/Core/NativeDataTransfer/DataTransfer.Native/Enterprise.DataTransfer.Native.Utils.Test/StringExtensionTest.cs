using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Utils
{
	class StringExtensionTest : TestCase
	{
		public void TestIsEmpty()
		{
			string result = "";
			AssertEquals(true, result.IsEmpty());

			result = null;
			AssertEquals(true, result.IsEmpty());

			result = "    ";
			AssertEquals(false, result.IsEmpty());
		}
	}
}
