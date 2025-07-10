using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing
{
	[TestedType(typeof(GlbStaffWrapperProvider))]
	sealed class GlbStaffWrapperProviderTest : MasterFiles.GUI.Testing.GlbStaffWrapperProviderTest<GlbStaffWrapperProvider>
	{
		public void TestGetNewUserControl()
		{
			using var userControl = provider.GetNewUserControl();
			CombineAssertions(() =>
			{
				AssertNotNull("Creates new user control for Staff Credentials", userControl);
				AssertType<StaffCredentialsUserControl>("StaffCredentialsUserControl Type", userControl);
			});
		}

		public void TestGlbStaffWrapperType()
		{
			CombineAssertions(() =>
			{
				AssertNull("No wrapper without Staff", provider.GetWrapper(null));

				var wrapper = provider.GetWrapper(Factory.New<MasterFiles.Business.GlbStaff>());
				AssertNotNull("Creates wrapper for Staff", wrapper);
				AssertType<GlbStaffWrapper>("GlbStaffWrapper Type", wrapper);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new GlbStaffWrapperProvider();
		}
		GlbStaffWrapperProvider provider;
	}
}
