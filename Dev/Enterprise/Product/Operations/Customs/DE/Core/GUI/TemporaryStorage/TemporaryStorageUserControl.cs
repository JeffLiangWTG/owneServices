using System;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class TemporaryStorageUserControl : ZUserControl
	{
		public TemporaryStorageUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			header = DataSource as CusTempStorageJobHeader;
			if (header != null)
			{
				SetSumAOrReExportControls();
			}
		}

		void SetSumAOrReExportControls()
		{
			if (header.IsReExport)
			{
				SetControlVisiblityAndDock(REXTransportDetailsUserControl, true);
				SetControlVisiblityAndDock(REXCustomsDetailsUserControl, true);
				SetControlVisiblityAndDock(SUMCustomsDetailsUserControl, false);
				SetControlVisiblityAndDock(SUMTransportDetailsUserControl, false);
			}
			else
			{
				SetControlVisiblityAndDock(REXTransportDetailsUserControl, false);
				SetControlVisiblityAndDock(REXCustomsDetailsUserControl, false);
				SetControlVisiblityAndDock(SUMCustomsDetailsUserControl, true);
				SetControlVisiblityAndDock(SUMTransportDetailsUserControl, true);
			}
		}

		void SetControlVisiblityAndDock(ZUserControl control, bool visibility)
		{
			control.Visible = visibility;
			control.Dock = DockStyle.Fill;
		}

		CusTempStorageJobHeader header;
	}
}
