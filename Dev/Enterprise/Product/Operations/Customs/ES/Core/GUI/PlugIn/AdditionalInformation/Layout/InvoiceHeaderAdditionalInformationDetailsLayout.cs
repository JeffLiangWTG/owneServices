using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class InvoiceHeaderAdditionalInformationDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public InvoiceHeaderAdditionalInformationDetailsLayout()
	{
		Layout = CreateInvoiceHeaderAdditionalInformationDetailsLayout();
	}

	static PanelLayout CreateInvoiceHeaderAdditionalInformationDetailsLayout()
	{
		var builder = new EU.GUI.PlugIn.AdditionalInformationDetailsLayoutBuilder();
		var euBag = builder.CommonBag;
		var esBag = AdditionalInformationDetailsControlBag.Instance;
		builder.AddControlBag(esBag);

		builder.AddColumn();
		builder.Add(euBag.KindDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.FullTypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
		builder.Add(euBag.DescriptionTextBox, ControlWidthClass.Long);
		builder.Add(esBag.NKCountryCodeFindBox, ControlWidthClass.Auto);

		builder.SetVisibility(esBag.NKCountryCodeFindBox, i => i.Declaration.IsUCC6AndIsImport, i => i.Declaration.JE_MessageTypeInfo);

		return builder.Build();
	}
}
