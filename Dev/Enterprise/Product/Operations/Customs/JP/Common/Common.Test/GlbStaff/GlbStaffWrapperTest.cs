using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(GlbStaffWrapper))]
	sealed class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
	{
		public void TestPasswordCollection()
		{
			AssertType<GlbExternalPasswordCUSCollection>(Wrapper.PasswordCollection);
		}

		protected override GlbStaffWrapper CreateNewWrapper(MasterFiles.Business.GlbStaff staff)
		{
			return GlbStaffWrapper.Get(staff);
		}
	}
}
