using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class ImportSupportingDocumentsFieldsControlBag : ControlBag
{
	public ImportSupportingDocumentsFieldsControlBag()
	{
		ImportSupportingDocumentReferenceNumberUserControl = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.ImportSupportingDocumentReferenceNumberUserControl));
		CodeCodeFindBox = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.CodeCodeFindBox));
		QuantityAndUnitUserControl = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.QuantityAndUnitUserControl));
		SecondQuantityAndUnitUserControl = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.SecondQuantityAndUnitUserControl));
		StatusAndProcedureUserControl = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.StatusAndProcedureUserControl));
		ValueAndCurrencyUserControl = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.ValueAndCurrencyUserControl));
		DateOfIssueAndExpiryUserControl = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.DateOfIssueAndExpiryUserControl));
		AdditionalDescriptionTextBox = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.AdditionalDescriptionTextBox));
		ItemNumberCalcEdit = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.ItemNumberCalcEdit));
		NKCountryCodeFindBox = RegisterControl(nameof(ImportSupportingDocumentsFieldsControl.NKCountryCodeFindBox));
	}

	public static ImportSupportingDocumentsFieldsControlBag Instance => instance ?? (instance = new ImportSupportingDocumentsFieldsControlBag());

	[ThreadStatic]
	static ImportSupportingDocumentsFieldsControlBag instance;

	protected override Control CreateTemplate() => new ImportSupportingDocumentsFieldsControl();

	public ControlReference ImportSupportingDocumentReferenceNumberUserControl { get; }
	public ControlReference CodeCodeFindBox { get; }
	public ControlReference AdditionalDescriptionTextBox { get; }
	public ControlReference ItemNumberCalcEdit { get; }
	public ControlReference NKCountryCodeFindBox { get; }
	public ControlReference QuantityAndUnitUserControl { get; }
	public ControlReference SecondQuantityAndUnitUserControl { get; }
	public ControlReference StatusAndProcedureUserControl { get; }
	public ControlReference ValueAndCurrencyUserControl { get; }
	public ControlReference DateOfIssueAndExpiryUserControl { get; }
}
