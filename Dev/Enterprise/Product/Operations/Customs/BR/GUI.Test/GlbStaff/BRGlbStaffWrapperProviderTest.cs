using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(BRGlbStaffWrapperProvider))]
	internal class BRGlbStaffWrapperProviderTest : MasterFiles.GUI.Testing.GlbStaffWrapperProviderTest<BRGlbStaffWrapperProvider>
	{
		public void TestGetNewTopLevelMenu()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XX";
			staff.GS_LoginName = "test";

			var wrapper = BRGlbStaffWrapper.Get(staff);

			var provider = new BRGlbStaffWrapperProvider();
			AssertType<SubscriptionPlugInMenu>(provider.GetNewTopLevelMenu(wrapper));
		}
	}
}
