using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class FilterStripLayoutsHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCurrentUserTablePrefix()
		{
			AssertEquals(GlbStaffSchema.Constants.Prefix, new FilterStripLayoutsHelper().CurrentUserTablePrefix);
		}

		public void TestCurrentUserPk()
		{
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, new FilterStripLayoutsHelper().CurrentUserPk);
		}
	}
}
