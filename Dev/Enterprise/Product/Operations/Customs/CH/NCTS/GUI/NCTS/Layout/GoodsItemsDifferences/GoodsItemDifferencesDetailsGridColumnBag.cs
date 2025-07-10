using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class GoodsItemDifferencesDetailsGridColumnBag
{
	[ThreadStatic]
	static GoodsItemDifferencesDetailsGridColumnBag instance;

	public static GoodsItemDifferencesDetailsGridColumnBag Instance => instance ??= new GoodsItemDifferencesDetailsGridColumnBag();

	public GoodsItemDifferencesDetailsGridColumnBag()
	{
		FormattedHarmonisedTariffColumn = new GridColumnReference<NctsTariffColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, 100);
	}

	public IGridColumnReference FormattedHarmonisedTariffColumn { get; }
}
