using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AdditionalHouseBillOfLadingTypeCollectionRegistryItemEditor))]
	sealed class AdditionalHouseBillOfLadingTypeCollectionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AdditionalHouseBillOfLadingTypeCollectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AdditionalHouseBillOfLadingTypeCollectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AdditionalHouseBillOfLadingTypeCollectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AdditionalHouseBillOfLadingTypeCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new AdditionalHouseBillOfLadingTypeCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.DefaultValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
