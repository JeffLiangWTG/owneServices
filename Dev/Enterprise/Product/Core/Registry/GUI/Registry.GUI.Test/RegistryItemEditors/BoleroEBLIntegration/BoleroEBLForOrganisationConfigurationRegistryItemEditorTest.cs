using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing.BoleroEBLIntegration
{
	[TestedType(typeof(BoleroEBLForOrganisationConfigurationRegistryItemEditor))]
	sealed class BoleroEBLForOrganisationConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectNoExceptions]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			using (OrganisationRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLForOrganisationConfiguration()))
			{
				RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLForOrganisationConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString(), Timeout = 60 });
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new BoleroEBLForOrganisationConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BoleroEBLForOrganisationConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BoleroEBLForOrganisationConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BoleroEBLForOrganisationConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, new BoleroEBLForOrganisationConfiguration());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new BoleroEBLForOrganisationConfiguration() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
