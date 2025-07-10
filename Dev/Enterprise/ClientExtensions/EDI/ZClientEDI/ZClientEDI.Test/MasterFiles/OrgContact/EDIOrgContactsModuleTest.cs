using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrgContactsModule))]
	public class EDIOrgContactsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgContacts;
		}
	}
}
