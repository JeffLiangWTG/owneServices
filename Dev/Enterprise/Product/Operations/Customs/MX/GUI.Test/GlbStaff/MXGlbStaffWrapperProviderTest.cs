using Enterprise.Customs.MX.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(MXGlbStaffWrapperProvider))]
	class MXGlbStaffWrapperProviderTest : MasterFiles.GUI.Testing.GlbStaffWrapperProviderTest<MXGlbStaffWrapperProvider>
	{
		public void TestProperties()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XX";
			staff.GS_LoginName = "test";

			var provider = new MXGlbStaffWrapperProviderForTesting();
			using (var newUserControl = provider.GetNewUserControlCoreExposed())
			{
				AssertType<StaffCredentialsUserControl>(newUserControl);
				AssertType<MXGlbStaffWrapper>(provider.GetWrapperCoreExposed(staff));
			}
		}

		class MXGlbStaffWrapperProviderForTesting : MXGlbStaffWrapperProvider
		{
			public MasterFiles.GUI.StaffCredentialsUserControl GetNewUserControlCoreExposed() => GetNewUserControlCore();

			public GlbStaffWrapper GetWrapperCoreExposed(GlbStaff staff) => GetWrapperCore(staff);
		}
	}
}
