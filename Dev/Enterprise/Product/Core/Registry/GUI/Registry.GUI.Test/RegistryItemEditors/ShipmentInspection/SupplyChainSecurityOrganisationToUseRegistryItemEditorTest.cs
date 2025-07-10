using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SupplyChainSecurityOrganisationToUseRegistryItemEditor))]
	sealed class SupplyChainSecurityOrganisationToUseRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new SupplyChainSecurityOrganisationToUseRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SupplyChainSecurityOrganisationToUseControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SupplyChainSecurityOrganisationToUseControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SupplyChainSecurityOrganisationToUseRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new SupplyChainSecurityOrganisationToUseCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new SupplyChainSecurityOrganisationToUseCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
