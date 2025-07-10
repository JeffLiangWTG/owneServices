using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.GLConsolidations.Testing
{
	[TestedType(typeof(GLConsolidationGroupModule))]
	public class GLConsolidationGroupModuleTest : ZModuleBasherTest
	{
		protected GLConsolidationGroupModule Module;

		protected override void SetUp()
		{
			base.SetUp();
			Module = ZModuleFactory.Instance.Create(GetModuleID()) as GLConsolidationGroupModule;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}

			base.TearDown();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLConsolidationGroups;
		}
	}
}
