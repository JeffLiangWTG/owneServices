using System;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class GoodsItemDetailsGridColumnBag
{
	[ThreadStatic]
	static GoodsItemDetailsGridColumnBag instance;

	public static GoodsItemDetailsGridColumnBag Instance => instance ??= new GoodsItemDetailsGridColumnBag();

	public GoodsItemDetailsGridColumnBag()
	{
		FormattedHarmonisedTariffColumn = new GridColumnReference<NctsTariffColumnStyleInfo>(NctsDepartureCargoDesc.Schema.BY_FormattedHarmonisedTariff, 100);
	}

	public IGridColumnReference FormattedHarmonisedTariffColumn { get; }
}
