using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7ItemsDetailsLayouts : IPanelLayoutProvider
	{
		public ESH7ItemsDetailsLayouts()
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
			builder.Add(common.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Auto);
			builder.Add(common.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(common.QuantityCalcEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(common.CustomEntriesSeparatorUserControl, ControlWidthClass.Auto);
			builder.Add(common.CustomEntriesGrid, ControlWidthClass.Auto);

			builder.SetCaption(common.IntrinsicValueConvertToLocalCurrencyControl, _ => Res.GetData("71c47b27-dc79-4c32-b8db-8b1fdace518c", "Intrinsic Value"));
			builder.SetCaption(common.SupplementaryDropEdit, _ => Res.GetData("6bbebde5-bdf7-4957-bef1-4db4f829345b", "Suppl. Units"));
			builder.SetCaption(common.QuantityCalcEdit, _ => Res.GetData("82dd8596-2ea2-41d9-bdab-97b2b671f7d9", "Quantity"));

			return builder.Build();
		}
	}
}
