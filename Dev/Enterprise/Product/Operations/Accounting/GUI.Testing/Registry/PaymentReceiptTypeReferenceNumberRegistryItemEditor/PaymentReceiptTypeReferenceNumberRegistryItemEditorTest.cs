using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(PaymentReceiptTypeReferenceNumberRegistryItemEditor))]
	public class PaymentReceiptTypeReferenceNumberRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new PaymentReceiptTypeReferenceNumberRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PaymentReceiptTypeReferenceNumberControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PaymentReceiptTypeReferenceNumberControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PaymentReceiptTypeReferenceNumberRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, AccountingConfigurationRegistry.PaymentReceiptTypeReferenceNumberDefaultValueGetter);
		}

		readonly PaymentReceiptTypeReferenceNumberCollection Collection = new PaymentReceiptTypeReferenceNumberCollection();

		protected override object[] GetValidRegistryValues()
		{
			Collection.AddDefaultValues(new string[] { "abc" });
			return new object[] { Collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
