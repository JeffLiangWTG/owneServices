using System.Windows.Forms;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.OSP.Module.Testing
{
	[TestedType(typeof(OSPConsolModuleOverride))]
	internal sealed class OSPConsolModuleOverrideTest : ZModuleBasherTest
	{
		public void TestImportOSPClientSpecific()
		{
			using (ZFilterModule module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuItem item = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From Combiline &File");
				item.PerformClick();
				Form form = ZFormModaliser.LastFormShownDialogForTest;
				AssertType(typeof(DataImporterForm), form);
			}
		}

#region Implementation
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobConsol;
		}
#endregion
	}
}
