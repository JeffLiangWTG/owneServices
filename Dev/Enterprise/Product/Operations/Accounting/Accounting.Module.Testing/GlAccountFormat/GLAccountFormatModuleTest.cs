using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing.GlAccountFormat
{
	[TestedType(typeof(GLAccountFormatModule))]
	class GLAccountFormatModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLAccountFormat;
		}
	}
}
