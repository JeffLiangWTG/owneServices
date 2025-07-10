using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeCodesWithTypeRegistryItemEditor))]
	public class ChargeCodesWithTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ChargeCodesWithTypeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ChargeCodesWithTypeRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ChargeCodesWithTypeRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ChargeCodeWithTypeRegistryItem("", null, null, null, RegistryStorageFlags.System, ChargeCodeWithTypeCollection.GetDefaultCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { ChargeCodeWithTypeCollection.GetDefaultCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
