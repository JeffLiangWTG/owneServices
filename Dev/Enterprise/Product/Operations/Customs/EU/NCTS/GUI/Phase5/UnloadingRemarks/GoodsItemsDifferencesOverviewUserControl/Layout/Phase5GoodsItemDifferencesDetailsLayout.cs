using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDifferencesDetailsLayout : IPanelLayoutProvider
	{
		public Phase5GoodsItemDifferencesDetailsLayout()
		{
			Layout = CreateLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new Phase5GoodsItemDifferencesDetailsLayoutBuilder<Business.NctsArrivalCargoDesc>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SequenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ItemNumberTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
