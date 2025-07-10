using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class ExportSupportingDocumentsFieldsControlBag : ControlBag
{
	public ExportSupportingDocumentsFieldsControlBag()
	{
		ReferenceNumberTextBox = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.ReferenceNumberTextBox));
		ReferenceNumberCodeFindBox = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.ReferenceNumberCodeFindBox));
		CodeCodeFindBox = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.CodeCodeFindBox));
		StatusDropEdit = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.StatusDropEdit));
		QuantityAndUnitUserControl = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.QuantityAndUnitUserControl));
		SecondQuantityAndUnitUserControl = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.SecondQuantityAndUnitUserControl));
		ValueAndCurrencyUserControl = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.ValueAndCurrencyUserControl));
		DateOfIssueAndExpiryUserControl = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.DateOfIssueAndExpiryUserControl));
		AdditionalDescriptionTextBox = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.AdditionalDescriptionTextBox));
		ItemNumberCalcEdit = RegisterControl(nameof(ExportSupportingDocumentsFieldsControl.ItemNumberCalcEdit));
	}

	public static ExportSupportingDocumentsFieldsControlBag Instance => instance ?? (instance = new ExportSupportingDocumentsFieldsControlBag());

	[ThreadStatic]
	static ExportSupportingDocumentsFieldsControlBag instance;

	protected override Control CreateTemplate() => new ExportSupportingDocumentsFieldsControl();

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
}
