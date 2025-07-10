using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDataElementConfigurationRegistryItemEditor))]
	public class KoreaSouthEInvoicingDataElementConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new KoreaSouthEInvoicingDataElementConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((KoreaSouthEInvoicingDataElementConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(KoreaSouthEInvoicingDataElementConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var defaultValue = new KoreaSouthEInvoicingDataElementConfigurationCollection
			{
				new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, $"<외국인등록번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.ForeignerRegistrationNumber)}> / 여권번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.PassportNumber)}>"),
			};

			return new KoreaSouthEInvoicingDataElementConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, defaultValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { GetRegistryItemWithSystemStorageLevel().DefaultValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
