using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class DetailsTabUserControl : ZUserControl
	{
		public DetailsTabUserControl()
		{
			InitializeComponent();
		}

		new CusExitHeader DataSource => (CusExitHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var provider = ExitControlLayoutProvider.GetLayoutProvider(DataSource.CountryCode);

			SetHeaderOrganisationDetailsLayout(provider);
			SetDetailsReportGrid(provider);
		}

		void SetHeaderOrganisationDetailsLayout(IExitControlLayoutProvider layoutProvider)
		{
			HeaderOrganisationDetailsDynamicLayoutPanel.UpdateLayout(layoutProvider.HeaderDetailsPanelLayout);
		}

		void SetDetailsReportGrid(IExitControlLayoutProvider layoutProvider)
		{
			var gridControl = (Control)layoutProvider.CreateDetailsReportGridUserControl();
			DetailsSplitContainer.Panel2.Controls.Add(gridControl);
			BindingSource.SetBindingMember(gridControl, ".");
			gridControl.Dock = DockStyle.Fill;
		}
	}
}
