using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(InvoiceLineExportSupportingDocumentsFieldsLayout))]
sealed class InvoiceLineExportSupportingDocumentsFieldsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}
 
	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.CodeCodeFindBox, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.StatusDropEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.QuantityAndUnitUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.SecondQuantityAndUnitUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.ValueAndCurrencyUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.DateOfIssueAndExpiryUserControl, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
			yield return (InvoiceLineExportSupportingDocumentsFieldsControlBag.Instance.PackQuantityAndUnitUserControl, ControlWidthClass.Auto);
		}
	}
}
