using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class NC123MessageSendingDetailsUserControl : ZUserControl
{
	public NC123MessageSendingDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		BindingSource.SetBindingMember(TransportDynamicLayoutPanel, nameof(ExportDeclarationMessageSendingObjectParent.SendingDeclaration));
		TransportDynamicLayoutPanel.UpdateLayout(new ActivationTransportDetailsLayout());
	}
}
