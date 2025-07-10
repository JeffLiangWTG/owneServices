using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.MFI
{
	[TestedType(typeof(MFIConsolModuleOverride))]
	public class MFIConsolModuleOverrideTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobConsol;
		}

		public void TestMenuItems()
		{
			using (MFIConsolModuleOverride module = new MFIConsolModuleOverride())
			{
				MenuAssertion.AssertHasMenu("Should contain 'From Carotrans File'", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From Carotrans File");
				MenuAssertion.AssertHasMenu("Should contain 'To Carotrans File'", module.FormActionMenu, "&Actions", "D&ata Transfer", "Export To Carotrans File");
			}
		}
	}
}
