using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class NewClientModuleInfoTest : TestCase
	{
		public void TestConstructor()
		{
			ClientModuleIdentifier moduleID = new ClientModuleIdentifier(TestClientModuleId.ClientModuleID, (NoResString)"ClientModuleID");
			ModuleInfo info = new ModuleInfo(moduleID, "Enterprise.ZArchitecture.GUI", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule");
			NewClientModuleInfo clientModuleInfo = new NewClientModuleInfo("CategoryName", "SectionName", info);
			AssertNotNull("NewClientModuleInfo", clientModuleInfo);
			AssertEquals("CategoryName", clientModuleInfo.CategoryName);
			AssertEquals("SectionName", clientModuleInfo.SectionName);
			AssertEquals(moduleID, clientModuleInfo.ID);
			AssertEquals(info, clientModuleInfo.Info);
		}
	}
}
