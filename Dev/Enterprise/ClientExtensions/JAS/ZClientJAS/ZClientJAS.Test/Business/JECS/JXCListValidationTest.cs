using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business.JXC.Testing
{
	internal class JXCListValidationTest : TestCaseWithDummy
	{
		public void TestWarnIfInvalidCode()
		{
			using (Dummy.SuspendValidationTesting())
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("AAA", "");
				list.AddPair("BBB", "");
				Dummy.Z0_Code = "";
				JXCListValidation.WarnIfInvalidCode(Dummy.Z0_CodeInfo, list);
				Assert("Should not have any warnings", !Dummy.Z0_CodeInfo.HasWarnings());
				Dummy.Z0_Code = "CCC";
				JXCListValidation.WarnIfInvalidCode(Dummy.Z0_CodeInfo, list);
				AssertEquals(1, Dummy.Z0_CodeInfo.GetWarnings().GetUniqueMessageList().Length);
				Assert("Should have warning", Dummy.Z0_CodeInfo.GetWarnings().GetFirstMessage().StartsWith(JXCConstants.JXCWarningPrefix));
			}
		}
	}
}
