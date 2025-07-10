using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(InvoicePostingExRateOptionRegistryItemEditor))]
	class InvoicePostingExRateOptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() =>
			new InvoicePostingExRateOptionRegistryItemEditor(new InvoicePostingExRateOptionRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		protected override Type GetExpectedEditorPaneType() => typeof(InvoicePostingExRateOptionControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new InvoicePostingExRateOptionRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new InvoicePostingExRateOptionCollection();
			var configuration = collection.AddNew();
			configuration.InvoiceCurrencyType = "LOC";
			configuration.ExRateOption = "DEF";
			configuration.OffSet = 1;

			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((InvoicePostingExRateOptionControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
