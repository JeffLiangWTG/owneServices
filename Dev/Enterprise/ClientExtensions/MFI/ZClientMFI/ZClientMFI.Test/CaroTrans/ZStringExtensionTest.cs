using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class ZStringExtensionTest : TestCaseWithFactory
	{
		public void TestGetStrLengthSafe()
		{
			AssertEquals("Pos before surrogate", 11, TestStr.GetStrLengthSafe(12));
			AssertEquals("truncated text", 13 , TestStr.GetStrLengthSafe(13));
		}

		static readonly ZString TestStr = "K-NSK test 😃😃 More surrogate";
	}
}
