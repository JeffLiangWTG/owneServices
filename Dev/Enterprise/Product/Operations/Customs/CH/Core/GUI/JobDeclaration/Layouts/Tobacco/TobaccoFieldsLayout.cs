using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class TobaccoFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	public TobaccoFieldsLayout()
	{
		Layout = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new TobaccoFieldsLayoutBuilder<Tobacco>();
		var bag = builder.CommonBag;
		builder.AddControlBag(bag);
		builder.AddColumn();
		builder.Add(bag.MainGroupDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.SubGroupDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.DesignationTextBox, ControlWidthClass.Long);
		builder.Add(bag.SequentialNumberIntEdit, ControlWidthClass.Auto);
		builder.Add(bag.RetailPriceCalcEdit, ControlWidthClass.Auto);
		builder.Add(bag.ReverseNumberTextBox, ControlWidthClass.Auto);
		builder.Add(bag.SpecialUnitOfMeasureDropEdit, ControlWidthClass.Auto);
		builder.Add(bag.TobaccoBrandDropEdit, ControlWidthClass.Auto);

		builder.SetVisibility(bag.SpecialUnitOfMeasureDropEdit, tobacco => tobacco.Parent.JobDeclaration.IsExportOrExportDeclarationActivation, addInfo => addInfo.Parent.JobDeclaration.JE_MessageTypeInfo);
		builder.SetVisibility(bag.ReverseNumberTextBox, tobacco => tobacco.Parent.JobDeclaration.IsExportOrExportDeclarationActivation, addInfo => addInfo.Parent.JobDeclaration.JE_MessageTypeInfo);
		return builder.Build();
	}
}
