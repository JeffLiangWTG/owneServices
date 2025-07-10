using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeableWeightRoundingRegistryItemEditor))]
	sealed class ChargeableWeightRoundingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ChargeableWeightRoundingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ChargeableWeightRoundingRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ChargeableWeightRoundingRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ChargeableWeightRoundingRegistryItem("", null, null, null, RegistryStorageFlags.System, ChargeableWeightRoundingCollection.GetDefault());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { ChargeableWeightRoundingCollection.GetDefault() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
