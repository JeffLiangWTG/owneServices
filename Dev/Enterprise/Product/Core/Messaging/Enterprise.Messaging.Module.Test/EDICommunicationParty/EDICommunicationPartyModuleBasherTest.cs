using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Test.EDICommunicationParty
{
	[TestedType(typeof(EDICommunicationPartyModule))]
	public class EDICommunicationPartyModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Messaging.EDICommunicationParty;
	}
}
