using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7ItemDetailsLayouts : IPanelLayoutProvider
	{
		public EUH7ItemDetailsLayouts()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateLayout()
		{
			var builder = new EUH7ItemDetailsControlLayoutBuilder<AsycudaPackedItem>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.TariffFindBox, ControlWidthClass.Auto);
			builder.Add(common.IntrinsicValueConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(common.SupplementaryDropEdit, ControlWidthClass.Auto);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(common.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.CustomEntriesSeparatorUserControl, ControlWidthClass.Auto);
			builder.Add(common.CustomEntriesGrid, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
