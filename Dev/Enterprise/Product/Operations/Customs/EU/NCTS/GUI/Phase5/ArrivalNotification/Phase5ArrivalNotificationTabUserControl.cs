using System;
using System.ComponentModel;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5ArrivalNotificationTabUserControl : ZUserControl
	{
		public Phase5ArrivalNotificationTabUserControl()
		{
			InitializeComponent();

			TypeDescriptor.AddAttributes(ArrivalDetailsGroupBox, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(DeclarationDetailsGroupBox, new SuppressControlRequiresTextBasherAttribute());
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetArrivalNotificationLayout();
		}

		void SetArrivalNotificationLayout()
		{
			var provider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource.DefaultDataGroupingCode);
			DynamicArrivalDetailsPanel.UpdateLayout(provider.ArrivalNotificationDetailsPanelLayout);
			DynamicDeclarationDetailsPanel.UpdateLayout(provider.ArrivalDeclarationDetailsPanelLayout);
		}

		new NctsHeader DataSource => base.DataSource as NctsHeader;
	}
}
