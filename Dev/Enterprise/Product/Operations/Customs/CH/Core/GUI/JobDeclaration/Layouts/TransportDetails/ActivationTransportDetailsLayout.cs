using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ActivationTransportDetailsLayout : IPanelLayoutProvider
{
	PanelLayout TransportDetails { get; }

	public PanelLayout Layout => TransportDetails;

	public ActivationTransportDetailsLayout()
	{
		TransportDetails = CreateTransportDetailsLayout();
	}

	PanelLayout CreateTransportDetailsLayout()
	{
		var builder = new ActivationTransportDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;

		var chBag = TransportDetailsControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(chBag.TransportModeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportMeansDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.MasterBillTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.VoyageAndNationalityUserControl, ControlWidthClass.Auto);

		builder.SetVisibility(commonBag.TransportIDAndNationalityUserControl, h => !h.IsAir, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.MasterBillTextBox, h => h.IsAir, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.VoyageAndNationalityUserControl, h => h.IsAir, h => h.JE_TransportModeInfo);
		builder.SetVisibility(commonBag.TransportMeansDropEdit, h => h.IsOwnPropulsion, h => h.JE_TransportModeInfo);

		return builder.Build();
	}
}
