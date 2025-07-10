using Enterprise.ZArchitecture.GUI.ZAudit.PlugIn;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.Controller.Testing
{
	[TestedType(typeof(AuditModule))]
	sealed class AuditModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Audit;
		}
	}
}
