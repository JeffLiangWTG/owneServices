using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class ReturnCodeTest : TestCase
	{
		public void TestValues()
		{
			AssertEquals("Failure", -1, (int)ReturnCode.Failure);
			AssertEquals("Sucess", 0, (int)ReturnCode.Success);
		}
	}
}