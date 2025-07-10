using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PurgeSettingsControl))]
	sealed class PurgeSettingsControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestApplicationCodeRowIsReadOnlyWhenUnpurgable()
		{
			var applicationCodeCollection = new ApplicationCodeObjCollection();
			applicationCodeCollection.AddNew().IsUnpurgable = true;
			applicationCodeCollection.AddNew();

			var purgeSettings = (PurgeSettings)GetNewBusinessEntity();
			purgeSettings.SetApplicationCodes(applicationCodeCollection);

			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.SetDataBinding(purgeSettings, null);
				form.Show();
				var applicationCodeGrid = control.Controls.Find("ApplicationCodesGrid", true)[0] as ZGrid;

				var unpurgableApplicationCode = (ApplicationCodeObj)applicationCodeGrid.List[0];
				Assert("Row for unpurgable application code should be readonly", unpurgableApplicationCode.ReadOnly);

				var purgableApplicationCode = (ApplicationCodeObj)applicationCodeGrid.List[1];
				Assert("Row for purgable application code should not be readonly", !purgableApplicationCode.ReadOnly);
			}
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new PurgeSettingsControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new PurgeSettings();
		}
	}
}
