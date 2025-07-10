using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class InvoiceLineExportSupportingDocumentsFieldsControlBag : ControlBag
{
	public InvoiceLineExportSupportingDocumentsFieldsControlBag()
	{
		ReferenceNumberTextBox = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ReferenceNumberTextBox));
		ReferenceNumberCodeFindBox = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ReferenceNumberCodeFindBox));
		CodeCodeFindBox = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.CodeCodeFindBox));
		StatusDropEdit = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.StatusDropEdit));
		QuantityAndUnitUserControl = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.QuantityAndUnitUserControl));
		SecondQuantityAndUnitUserControl = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.SecondQuantityAndUnitUserControl));
		ValueAndCurrencyUserControl = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ValueAndCurrencyUserControl));
		DateOfIssueAndExpiryUserControl = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.DateOfIssueAndExpiryUserControl));
		AdditionalDescriptionTextBox = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.AdditionalDescriptionTextBox));
		ItemNumberCalcEdit = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.ItemNumberCalcEdit));
		PackQuantityAndUnitUserControl = RegisterControl(nameof(InvoiceLineExportSupportingDocumentsFieldsControl.PackQuantityAndUnitUserControl));
	}

	public static InvoiceLineExportSupportingDocumentsFieldsControlBag Instance => instance ?? (instance = new InvoiceLineExportSupportingDocumentsFieldsControlBag());

	[ThreadStatic]
	static InvoiceLineExportSupportingDocumentsFieldsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineExportSupportingDocumentsFieldsControl();

	public ControlReference ReferenceNumberTextBox { get; }
	public ControlReference CodeCodeFindBox { get; }
	public ControlReference StatusDropEdit { get; }
	public ControlReference ReferenceNumberCodeFindBox { get; }
	public ControlReference AdditionalDescriptionTextBox { get; }
	public ControlReference ItemNumberCalcEdit { get; }
	public ControlReference QuantityAndUnitUserControl { get; }
	public ControlReference SecondQuantityAndUnitUserControl { get; }
	public ControlReference ValueAndCurrencyUserControl { get; }
	public ControlReference DateOfIssueAndExpiryUserControl { get; }
	public ControlReference PackQuantityAndUnitUserControl { get; }
}
