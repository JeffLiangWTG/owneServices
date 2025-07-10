using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5TransportAndPackagingLayout : IPanelLayoutProvider
	{
		public Phase5TransportAndPackagingLayout()
		{
			Layout = CreateTransportAndPackagingLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateTransportAndPackagingLayout()
		{
			var builder = new TransportAndPackagingLayoutBuilder<Business.NctsDepartureMovementHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TransportMethodOfPaymentDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CarrierDocAddressControl, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
