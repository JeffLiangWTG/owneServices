using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class ExportSupportingDocumentsFieldsLayout : IPanelLayoutProvider
{
	public ExportSupportingDocumentsFieldsLayout()
	{
		Layout = CreateExportSupportingDocumentsFieldsLayout();
	}

	PanelLayout CreateExportSupportingDocumentsFieldsLayout()
	{
		var builder = new ExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();

		builder.Add(commonBag.CodeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.StatusDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.QuantityAndUnitUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.SecondQuantityAndUnitUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.ValueAndCurrencyUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.DateOfIssueAndExpiryUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);

		return builder.Build();
	}

	public PanelLayout Layout { get; }
}
