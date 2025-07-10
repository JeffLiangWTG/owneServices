using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AddressValidationDisabledCountryItemsRegistryItemEditor))]
	sealed class AddressValidationDisabledCountryItemsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AddressValidationDisabledCountryItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AddressValidationDisabledCountryItemsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AddressValidationDisabledCountryItemsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new AddressValidationDisabledCountryItemCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((AddressValidationDisabledCountryItemsControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
