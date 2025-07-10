using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing.BoleroEBLIntegration
{
	[TestedType(typeof(BoleroEBLConfigurationRegistryItemEditor))]
	sealed class BoleroEBLConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectNoExceptions]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration()))
			{
				RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() });
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BoleroEBLConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BoleroEBLConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BoleroEBLConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BoleroEBLConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BoleroEBLConfiguration());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new BoleroEBLConfiguration() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
