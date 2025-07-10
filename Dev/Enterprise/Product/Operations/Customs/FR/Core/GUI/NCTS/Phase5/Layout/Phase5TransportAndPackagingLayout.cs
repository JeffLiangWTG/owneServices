using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
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
			var builder = new TransportAndPackagingLayoutBuilder<NctsDepartureMovementHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.TransportMethodOfPaymentDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CarrierDocAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.PortOfPresentationCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ChargePaymentOrDestinationIDDropEdit, ControlWidthClass.Long);
			return builder.Build();
		}
	}
}
