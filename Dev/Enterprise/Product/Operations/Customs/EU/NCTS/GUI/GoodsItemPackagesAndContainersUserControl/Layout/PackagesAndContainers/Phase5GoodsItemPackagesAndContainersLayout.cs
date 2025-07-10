using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemPackagesAndContainersLayout : IPanelLayoutProvider
	{
		public Phase5GoodsItemPackagesAndContainersLayout()
		{
			Layout = CreateGoodsItemPackagesAndContainersLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateGoodsItemPackagesAndContainersLayout()
		{
			var builder = new Phase5GoodsItemPackagesAndContainersLayoutBuilder<Business.NctsPackage>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DynamicPackagesUserControl, ControlWidthClass.LongControl);

			builder.AddColumn();
			builder.Add(commonBag.ContainersUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
