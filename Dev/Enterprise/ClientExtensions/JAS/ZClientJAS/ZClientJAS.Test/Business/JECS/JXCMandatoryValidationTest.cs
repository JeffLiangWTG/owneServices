using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Testing
{
	internal class JXCMandatoryValidationTest : TestCaseWithDummy
	{
		public void TestWarnIfNotEntered()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_VarCharMax = "TEST";
				JXCMandatoryValidation.WarnIfNotEntered(Dummy.Z0_VarCharMaxInfo);
				Assert(!Dummy.Z0_VarCharMaxInfo.HasMessageErrors());
				Dummy.Z0_VarCharMax = "";
				JXCMandatoryValidation.WarnIfNotEntered(Dummy.Z0_VarCharMaxInfo);
				Assert(Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage().StartsWith("JXC: " + JXCMandatoryValidation.YouHaveNotEntered));
				Dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
				JXCMandatoryValidation.WarnIfNotEntered(Dummy.Z0_VarCharMaxInfo, "TESTER");
				Assert(Dummy.Z0_VarCharMaxInfo.GetWarnings().GetFirstMessage().StartsWith("JXC: " + JXCMandatoryValidation.YouHaveNotEntered + " a TESTER"));
			}
		}
	}
}
