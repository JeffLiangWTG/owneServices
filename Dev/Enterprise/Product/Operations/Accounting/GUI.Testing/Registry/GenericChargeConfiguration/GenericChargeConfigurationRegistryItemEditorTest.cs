using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GenericChargeConfigurationRegistryItemEditor))]
	public class GenericChargeConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GenericChargeConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new GenericChargeConfigurationRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GenericChargeConfigurationControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Settings };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		GenericChargeConfigurationCollection settings;
		GenericChargeConfigurationCollection Settings
		{
			get
			{
				if (settings == null)
				{
					var charge = Factory.NewWithValidTestData<AccChargeCode>();
					charge.AC_Code = "NAB";
					charge.AC_GC = GlbCompany.CurrentCompany.PK;
					charge.AC_ChargeType = "MRG";

					Factory.Save();
					settings = new GenericChargeConfigurationCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
					var setting = settings.AddNew();
					setting.ChargePK = charge.PK;
				}
				return settings;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}

		#endregion
	}
}
