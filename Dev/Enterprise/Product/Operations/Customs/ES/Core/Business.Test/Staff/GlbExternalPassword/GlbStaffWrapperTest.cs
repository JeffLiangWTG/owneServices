using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.ES;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GlbStaffWrapper))]
	public class GlbStaffWrapperTest : Enterprise.MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
	{
		public void TestIGlbStaffWrapperMembers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = GlbStaffWrapper.Get(staff);
			IGlbStaffWrapper iWrapper = wrapper;
			AssertEquals(wrapper.ESBPasswordCollection, iWrapper.ESBPasswordCollection);
		}

		protected override GlbStaffWrapper CreateNewWrapper(GlbStaff staff)
			=> GlbStaffWrapper.Get(staff);
	}
}
