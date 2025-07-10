using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class CusEntryLineExtendedInformationQuantitiesControlBag : ControlBag
{
	public static CusEntryLineExtendedInformationQuantitiesControlBag Instance => instance ?? (instance = new CusEntryLineExtendedInformationQuantitiesControlBag());

	[ThreadStatic]
	static CusEntryLineExtendedInformationQuantitiesControlBag instance;

	CusEntryLineExtendedInformationQuantitiesControlBag()
	{
		CalcCustomsNetWeightDropEdit = RegisterControl(nameof(CusEntryLineExtendedInformationQuantitiesUserControl.CalcCustomsNetWeightDropEdit));
		CalcAdditionalQuantityDropEdit = RegisterControl(nameof(CusEntryLineExtendedInformationQuantitiesUserControl.CalcAdditionalQuantityDropEdit));
		CalcNetWeightDropEdit = RegisterControl(nameof(CusEntryLineExtendedInformationQuantitiesUserControl.CalcNetWeightDropEdit));
		CalcGrossWeightDropEdit = RegisterControl(nameof(CusEntryLineExtendedInformationQuantitiesUserControl.CalcGrossWeightDropEdit));
	}

	protected override Control CreateTemplate() => new CusEntryLineExtendedInformationQuantitiesUserControl();

	public ControlReference CalcCustomsNetWeightDropEdit { get; }
	public ControlReference CalcAdditionalQuantityDropEdit { get; }
	public ControlReference CalcNetWeightDropEdit { get; }
	public ControlReference CalcGrossWeightDropEdit { get; }
}
