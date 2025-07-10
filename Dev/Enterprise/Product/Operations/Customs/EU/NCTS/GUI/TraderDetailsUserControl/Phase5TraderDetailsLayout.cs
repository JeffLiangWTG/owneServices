using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5TraderDetailsLayout : IPanelLayoutProvider
	{
		public Phase5TraderDetailsLayout()
		{
			Layout = CreateDepartureDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateDepartureDetailsLayout()
		{
			var builder = new TraderDetailsLayoutBuilder<Business.NctsHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.PrincipalDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.RepresentativeDocAddressControl, ControlWidthClass.LongControl);
			builder.Add(commonBag.FromWarehouseGroupBox, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
