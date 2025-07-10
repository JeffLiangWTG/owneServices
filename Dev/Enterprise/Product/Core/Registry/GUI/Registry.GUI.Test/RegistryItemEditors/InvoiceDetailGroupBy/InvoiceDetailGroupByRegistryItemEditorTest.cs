using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InvoiceDetailGroupByRegistryItemEditor))]
	sealed class InvoiceDetailGroupByRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new InvoiceDetailGroupByRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InvoiceDetailGroupByControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InvoiceDetailGroupByControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InvoiceDetailGroupByRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceDetailGroupBy());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new InvoiceDetailGroupBy() };
		}

		#endregion
	}
}
