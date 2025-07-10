using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class LiabilityDetailsLayout : IPanelLayoutProvider
	{
		public LiabilityDetailsLayout()
		{
			Layout = CreateLiabilityDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLiabilityDetailsLayout()
		{
			var builder = new LiabilityDetailsLayoutBuilder<Business.NctsArrivalCargoDesc>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CommodityCodeTariffFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsFourthQuantityDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsValueCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.FeesUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
