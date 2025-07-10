using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHGlbStaffWrapper))]
public class CHGlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<CHGlbStaffWrapper>
{
	protected override CHGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
	{
		return CHGlbStaffWrapper.Get(staff);
	}
}
