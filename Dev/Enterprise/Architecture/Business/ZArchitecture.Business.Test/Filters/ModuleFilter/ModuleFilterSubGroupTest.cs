using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleFilterSubGroupTest : TestCaseWithFactory
	{
		public void TestDefault()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "BLT");
			AssertSame(ModuleFilterSubGroup.Default, ModuleFilterSubGroup.Default);
			AssertSame(query, ModuleFilterSubGroup.Default.GetSubQuery(query));
			AssertSame(null, ModuleFilterSubGroup.Default.Parent);
		}
	}
}
