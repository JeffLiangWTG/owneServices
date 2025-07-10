using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class InvoiceLineExportSupportingDocumentsFieldsLayout : IPanelLayoutProvider
{
	public InvoiceLineExportSupportingDocumentsFieldsLayout()
	{
		Layout = CreateInvoiceLineExportSupportingDocumentsFieldsLayout();
	}

	PanelLayout CreateInvoiceLineExportSupportingDocumentsFieldsLayout()
	{
		var builder = new InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();
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
		builder.Add(commonBag.PackQuantityAndUnitUserControl, ControlWidthClass.Auto);

		return builder.Build();
	}

	public PanelLayout Layout { get; }
}
