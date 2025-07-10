using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	[TestedType(typeof(EDIRefZoneHeaderModule))]
	class EDIRefZoneHeaderModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.InternationalZone;
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (EDIRefZoneHeaderModule module = new EDIRefZoneHeaderModule())
			{
				AssertEquals(typeof(EDIRefZoneHeaderFilterBusinessObject), module.InternalGetNewFilterBusinessObject().GetType());
			}
		}
	}
}
