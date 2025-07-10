using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class InvoiceHeaderAdditionalInfoDetailsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new AdditionalInformationDetailsLayoutBuilder();
		var euBag = builder.CommonBag;
		var itBag = InvoiceHeaderAdditionalInfoDetailsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(euBag.KindDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.FullTypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceTextBox, widthClass: ControlWidthClass.Auto);
		builder.Add(itBag.DescriptionTextBox, widthClass: ControlWidthClass.Auto);

		return builder.Build();
	}
}
