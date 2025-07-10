using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DepotAddressColorSoundControl))]
	sealed class DepotAddressColorSoundControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DepotAddressColorSoundCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((DepotAddressColorSoundControl)control).depotAddressColorSoundGrid.ReadOnly;
		}

		public void TestMassUpdateOptionHidden()
		{
			using (var control = new DepotAddressColorSoundControl())
			{
				control.depotAddressColorSoundGrid.OnPopup_CallForTesting();
				Assert("Mass update should not be visible", !control.depotAddressColorSoundGrid.ContextMenu.MenuItems.Cast<MenuItem>().Any(x => x.Text.ToLower().Contains("mass update") && x.Visible));
			}
		}
	}
}
