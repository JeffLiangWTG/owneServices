using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class ExportEntryInstructionSupportingDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ExportEntryInstructionSupportingDocumentFieldsLayout()
	{
		Layout = CreateInvoiceDetailsLayout();
	}

	static PanelLayout CreateInvoiceDetailsLayout()
	{
		var builder = new SupportingDocumentFieldsLayoutBuilder<Business.Declaration.SupportingDocument>();

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
		builder.Add(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto);
		builder.Add(itBag.LineNoCalcEdit, ControlWidthClass.Auto);

		builder.SetVisibility(euBag.ReferenceNumberCodeFindBox, doc => doc.ShowCodeFindBoxForReferenceNumber, doc => doc.CSI_CodeInfo);
		builder.SetVisibility(euBag.ReferenceNumberTextBox, doc => doc.ShowTextBoxForReferenceNumber, doc => doc.CSI_CodeInfo);

		return builder.Build();
	}
}
