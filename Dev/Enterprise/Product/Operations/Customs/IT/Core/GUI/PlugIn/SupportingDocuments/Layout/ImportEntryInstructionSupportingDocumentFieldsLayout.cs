using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class ImportEntryInstructionSupportingDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ImportEntryInstructionSupportingDocumentFieldsLayout()
	{
		Layout = CreateInvoiceDetailsLayout();
	}

	static PanelLayout CreateInvoiceDetailsLayout()
	{
		var builder = new SupportingDocumentFieldsLayoutBuilder<SupportingDocument>();

		var euBag = EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance;
		builder.AddControlBag(euBag);
		var itBag = SupportingDocumentFieldsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(euBag.CodeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceNumberCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.YearOfIssueTextBox, ControlWidthClass.Auto);
		builder.Add(itBag.CountryCodeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(itBag.IssuingAuthorityTextBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.ValueCalcFindBox, ControlWidthClass.Long);
		builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);

		builder.SetVisibility(euBag.ReferenceNumberCodeFindBox, doc => doc.ShowCodeFindBoxForReferenceNumber, doc => doc.CSI_CodeInfo);
		builder.SetVisibility(euBag.ReferenceNumberTextBox, doc => doc.ShowTextBoxForReferenceNumber, doc => doc.CSI_CodeInfo);

		builder.SetCaption(itBag.IssuingAuthorityTextBox, s => Res.GetData("0837a96d-f3d0-40d8-8a6a-203569c06441", "Issuing Authority"));
		builder.SetCaption(itBag.YearOfIssueTextBox, s => Res.GetData("ca85d537-7235-43cb-852e-672099d91cf8", "Year of Issue"));

		return builder.Build();
	}
}
