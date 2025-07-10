using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	[TestedType(typeof(ZStmModuleFilterModule))]
	sealed class ZStmModuleFilterModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.StmModuleFilter;
		}

		public void TestProperties()
		{
			using (var module = new ZStmModuleFilterModule())
			{
				AssertEquals(ModuleIDs.StmModuleFilter, module.ID);
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
				AssertEquals(false, module.AllowCopyFilterGridHyperlinkToClipboard);
				AssertEquals(false, module.AllowNew);
				AssertEquals(false, module.AllowEdit);
				AssertEquals(false, module.AllowView);
				AssertEquals(false, module.AllowDelete);
				AssertEquals(false, module.AllowDefaultActivateDeactivate);
			}
		}
	}
}
