using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class TransportDetailsUserControl : ZUserControl
	{
		public TransportDetailsUserControl()
		{
			InitializeComponent();
		}
		protected override void OnAfterFirstBinding(EventArgs e)
		{
			header = DataSource as CusTempStorageJobHeader;

			if (header != null)
			{
				header.SJH_TransportModeInfo.ValueChanged -= SJH_TransportModeInfo_ValueChanged;
			}

			base.OnAfterFirstBinding(e);

			if (header != null)
			{
				header.SJH_TransportModeInfo.ValueChanged += SJH_TransportModeInfo_ValueChanged;
				SJH_TransportModeInfo_ValueChanged(null, null);
			}
		}
		CusTempStorageJobHeader header;

		void SJH_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (header != null)
			{
				switch (header.SJH_TransportMode)
				{
					case TransportTypeList.Codes.Air:
						VesselCodeFindBox.Visible = false;
						TransportRegistrationNumTextBox.Visible = true;
						TransportRegistrationNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("77089CD0-794C-4374-9A45-42D87B3ABC7C", "Flight Number");
						break;
					case TransportTypeList.Codes.Sea:
					case TransportTypeList.Codes.InlandWaterwayTransport:
						TransportRegistrationNumTextBox.Visible = false;
						VesselCodeFindBox.Visible = true;
						break;
					default:
						VesselCodeFindBox.Visible = false;
						TransportRegistrationNumTextBox.Visible = true;
						TransportRegistrationNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("A45A7785-CDCE-4DB1-AAE2-97182399942E", "Transport Reg. No.");
						break;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (header != null)
			{
				header.SJH_TransportModeInfo.ValueChanged -= SJH_TransportModeInfo_ValueChanged;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
