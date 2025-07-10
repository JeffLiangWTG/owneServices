using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(InvoiceCopiesRegistryItemEditor))]
	public class InvoiceCopiesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new InvoiceCopiesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InvoiceCopiesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InvoiceCopiesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InvoiceCopiesRegistryItem("", null, null, null, RegistryStorageFlags.System, new InvoiceCopyCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			InvoiceCopyCollection collection = new InvoiceCopyCollection();
			InvoiceCopy copy = collection.AddNew();

			copy.Name = (NoResString)"Name";
			copy.DeliveryMethod = nameof(PrintCopyType.FAX);

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
