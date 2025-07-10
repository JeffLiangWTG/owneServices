using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class ImportSupportingDocumentsFieldsLayout : IPanelLayoutProvider
{
	public ImportSupportingDocumentsFieldsLayout()
	{
		Layout = CreateImportSupportingDocumentsFieldsLayout();
	}

	PanelLayout CreateImportSupportingDocumentsFieldsLayout()
	{
		var builder = new ImportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();

		builder.Add(commonBag.CodeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ImportSupportingDocumentReferenceNumberUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.StatusAndProcedureUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.QuantityAndUnitUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.SecondQuantityAndUnitUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ValueAndCurrencyUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.DateOfIssueAndExpiryUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NKCountryCodeFindBox, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.AdditionalDescriptionTextBox, i => i.IsUCC6AndIsImport, i => i.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.ItemNumberCalcEdit, i => i.IsUCC6AndIsImport, i => i.JE_MessageTypeInfo);
		builder.SetVisibility(commonBag.NKCountryCodeFindBox, i => i.IsUCC6AndIsImport, i => i.JE_MessageTypeInfo);

		return builder.Build();
	}

	public PanelLayout Layout { get; }
}
