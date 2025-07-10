using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class CusEntryLineExtendedInformationQuantitiesLayout : IPanelLayoutProvider
{
	PanelLayout CusEntryLineExtendedInformations { get; }

	PanelLayout IPanelLayoutProvider.Layout => CusEntryLineExtendedInformations;

	public CusEntryLineExtendedInformationQuantitiesLayout()
	{
		CusEntryLineExtendedInformations = CreateCusEntryLineExtendedInformationsLayout();
	}

	PanelLayout CreateCusEntryLineExtendedInformationsLayout()
	{
		var builder = new CusEntryLineExtendedInformationQuantitiesLayoutBuilder<CusEntryLine>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.CalcCustomsNetWeightDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CalcAdditionalQuantityDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CalcNetWeightDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CalcGrossWeightDropEdit, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.CalcCustomsNetWeightDropEdit, h => h.Declaration.IsImport, h => h.Declaration.JE_MessageTypeInfo);

		return builder.Build();
	}
}
