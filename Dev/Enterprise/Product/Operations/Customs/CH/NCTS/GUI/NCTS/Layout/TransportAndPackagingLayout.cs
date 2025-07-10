using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class TransportAndPackagingLayout : IPanelLayoutProvider
{
	public TransportAndPackagingLayout()
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
		return builder.Build();
	}
}
