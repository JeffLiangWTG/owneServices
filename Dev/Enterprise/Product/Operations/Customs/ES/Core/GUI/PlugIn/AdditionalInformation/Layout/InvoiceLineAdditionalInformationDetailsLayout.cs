using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class InvoiceLineAdditionalInformationDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public InvoiceLineAdditionalInformationDetailsLayout()
	{
		Layout = CreateInvoiceLineAdditionalInformationDetailsLayout();
	}

	static PanelLayout CreateInvoiceLineAdditionalInformationDetailsLayout()
	{
		var builder = new EU.GUI.PlugIn.AdditionalInformationDetailsLayoutBuilder/*<AdditionalInfo>*/();
		var euBag = builder.CommonBag;
		var esBag = AdditionalInformationDetailsControlBag.Instance;
		builder.AddControlBag(esBag);

		builder.AddColumn();
		builder.Add(euBag.KindDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.FullTypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
		builder.Add(euBag.DescriptionTextBox, ControlWidthClass.Long);
		builder.Add(euBag.DetailTextBox, ControlWidthClass.Long);
		builder.Add(euBag.CurrencyDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.AmountCalcEdit, ControlWidthClass.Long);
		builder.Add(esBag.NKCountryCodeFindBox, ControlWidthClass.Auto);

		builder.SetVisibility(esBag.NKCountryCodeFindBox, i => i.Declaration.IsUCC6AndIsImport, i => i.Declaration.JE_MessageTypeInfo);

		return builder.Build();
	}
}
