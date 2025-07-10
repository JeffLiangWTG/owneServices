using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class DefaultPackedItemDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout PackedItemDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => PackedItemDetails;

		public DefaultPackedItemDetailsLayouts()
		{
			PackedItemDetails = CreatePackedItemDetailsLayout();
		}

		PanelLayout CreatePackedItemDetailsLayout()
		{
			var builder = new PackedItemDetailsLayoutBuilder<AsycudaPack>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.TariffFindBox, ControlWidthClass.Medium);
			builder.Add(common.GoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(common.MessageStatusTextBox, ControlWidthClass.Auto);
			builder.Add(common.PackStatusTextBox, ControlWidthClass.Auto);
			builder.Add(common.GoodsOriginCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(common.CustomsQtyCalcDropEdit, ControlWidthClass.Long);
			builder.Add(common.CustomsValueCalcEdit, ControlWidthClass.Medium);
			builder.Add(common.TaxAmountCalcEdit, ControlWidthClass.Medium);
			builder.Add(common.DutyAmountCalcEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(common.CustomEntriesSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(common.CustomEntriesGrid, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
