using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItemEditor))]
	class ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ExcludedFullyDigitalizedElectronicInvoiceDataControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExcludedFullyDigitalizedElectronicInvoiceDataControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem("", null, null, null, RegistryStorageFlags.System, new ExcludedFullyDigitalizedElectronicInvoiceData());
		}

		protected override object[] GetValidRegistryValues()
		{
			var data = new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerAddress = true };

			return new object[] { data };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
