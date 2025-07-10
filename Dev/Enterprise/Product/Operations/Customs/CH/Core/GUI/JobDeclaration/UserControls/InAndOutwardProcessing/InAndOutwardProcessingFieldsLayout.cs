using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class InAndOutwardProcessingFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	public InAndOutwardProcessingFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new InAndOutwardProcessingFieldsLayoutBuilder();
		var bag = builder.CommonBag;
		builder.AddControlBag(bag);

		builder.AddColumn();
		builder.Add(bag.SubTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.CodeDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.ProcedureDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.IssuerTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.StatusCheckBox, ControlWidthClass.Auto);
		builder.Add(bag.DescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(bag.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);

		builder.SetVisibility(bag.SubTypeDropEdit, x => x.IsImport, x => x.Declaration?.JE_MessageTypeInfo);
		builder.SetVisibility(bag.CustomsOfficeCodeFindBox, x => x.IsExportOrExportDeclarationActivation, x => x.Declaration?.JE_MessageTypeInfo);

		return builder.Build();
	}
}
