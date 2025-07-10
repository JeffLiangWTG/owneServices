using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(InvoiceLineExportSupportingDocumentsFieldsControlBag))]
sealed class InvoiceLineExportSupportingDocumentsFieldsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => new InvoiceLineExportSupportingDocumentsFieldsControlBag();

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ReferenceNumberTextBox);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ReferenceNumberCodeFindBox);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.CodeCodeFindBox);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.StatusDropEdit);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.QuantityAndUnitUserControl);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.SecondQuantityAndUnitUserControl);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ValueAndCurrencyUserControl);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.DateOfIssueAndExpiryUserControl);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.AdditionalDescriptionTextBox);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ItemNumberCalcEdit);
			yield return nameof(InvoiceLineExportSupportingDocumentsFieldsControl.PackQuantityAndUnitUserControl);
		}
	}
}
