using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentTriageChecklistItemModule))]
	public class IncidentTriageChecklistItemModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.IncidentTriageChecklistItem;
		}
	}
}
