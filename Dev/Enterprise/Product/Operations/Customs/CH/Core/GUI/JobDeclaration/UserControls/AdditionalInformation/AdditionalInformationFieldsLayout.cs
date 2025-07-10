using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class AdditionalInformationFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	public AdditionalInformationFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new AdditionalInformationFieldsLayoutBuilder<AdditionalInformation>();
		var bag = builder.CommonBag;
		builder.AddControlBag(bag);

		builder.AddColumn();
		builder.Add(bag.CodeDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.ReferenceNumberDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.DescriptionTextBox, ControlWidthClass.Long);

		builder.SetVisibility(bag.ReferenceNumberDropEdit, addInfo => !addInfo.Parent.JobDeclaration.IsExportOrExportDeclarationActivation, addInfo => addInfo.Parent.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(bag.DescriptionTextBox, addInfo => addInfo.Parent.JobDeclaration.IsExportOrExportDeclarationActivation, addInfo => addInfo.Parent.JobDeclaration.JE_MessageTypeInfo);
		return builder.Build();
	}
}
