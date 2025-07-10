using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class JXCWarningInfoTest : TestCaseWithDummy
	{
		public void TestJXCWarningInfo()
		{
			JXCWarningInfo warningInfo = new JXCWarningInfo(Dummy.Z0_NVarCharMaxInfo, "MEH");
			AssertEquals("Should be assigned in the constructor", Dummy.Z0_NVarCharMaxInfo, warningInfo.Info);
			AssertEquals("Should be assigned in the constructor", "MEH", warningInfo.WarningMessage);
			AssertEquals("Should be the BizO of the PropertyInfo", Dummy, warningInfo.BizObj);
		}
	}
}
