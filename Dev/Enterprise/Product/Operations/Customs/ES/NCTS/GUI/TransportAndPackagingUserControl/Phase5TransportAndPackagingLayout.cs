using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
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

			builder.AddColumn();
			builder.Add(commonBag.CarrierDocAddressControl, ControlWidthClass.LongNoCaption);

			builder.AddControlBehaviour(commonBag.CarrierDocAddressControl, new DocAddressControlDisplayModeCompactWithOverrideBehaviour());

			return builder.Build();
		}
	}
}
