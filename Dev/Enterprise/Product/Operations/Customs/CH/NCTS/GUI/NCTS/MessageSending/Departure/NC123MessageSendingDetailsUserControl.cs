using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class NC123MessageSendingDetailsUserControl : ZUserControl
{
	public NC123MessageSendingDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		TransportDepartureDynamicLayoutPanel.UpdateLayout(new ActivationTransportDepartureLayout());
	}
}
