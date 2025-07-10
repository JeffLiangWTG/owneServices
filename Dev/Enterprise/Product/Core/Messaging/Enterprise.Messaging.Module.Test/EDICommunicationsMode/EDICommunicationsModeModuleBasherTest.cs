using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDICommunicationsModeModule))]
	sealed class EDICommunicationsModeModuleBasherTest : ZModuleBasherTest
	{
		public void TestAllowNewEditDelete()
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertEquals("AllowNew", false, module.AllowNew);
				AssertEquals("AllowEdit", true, module.AllowEdit);
				AssertEquals("AllowDelete", false, module.AllowDelete);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Messaging.EDICommunicationsMode;
		}
	}
}
