using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionContextTest : TestCaseWithFactory
	{
		public void TestModuleName()
		{
			AssertEquals("Module Name", new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name").ModuleName);
			AssertEquals("Module Name2", new OperationalActionContext(new MockOperationalActionSupportable().OperationalActionSupporter, "Module Name2").ModuleName);
		}

		public void TestSupporter()
		{
			OperationalActionSupporter supporter = new MockOperationalActionSupportable().OperationalActionSupporter;
			AssertSame(supporter, new OperationalActionContext(supporter, "X").Supporter);
		}
	}
}
