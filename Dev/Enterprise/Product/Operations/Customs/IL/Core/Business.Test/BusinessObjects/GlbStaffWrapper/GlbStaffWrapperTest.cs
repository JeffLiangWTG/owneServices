using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GlbStaffWrapper))]
	sealed class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
	{
		public void TestPasswordCollection()
		{
			var staff = Factory.NewWithValidTestData<MasterFiles.Business.GlbStaff>();
			var wrapper = GlbStaffWrapper.Get(staff);
			AssertType<GlbILStaffExternalPasswordCollection>("PasswordCollection Type", wrapper.PasswordCollection);
		}

		protected override GlbStaffWrapper CreateNewWrapper(MasterFiles.Business.GlbStaff staff)
		{
			return GlbStaffWrapper.Get(staff);
		}
	}
}
