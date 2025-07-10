using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
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
						TransportRegistrationNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("7593535C-6230-4568-BA85-29F57CE49993", "Flight Number");
						break;
					case TransportTypeList.Codes.Sea:
					case TransportTypeList.Codes.InlandWaterwayTransport:
						TransportRegistrationNumTextBox.Visible = false;
						VesselCodeFindBox.Visible = true;
						break;
					default:
						VesselCodeFindBox.Visible = false;
						TransportRegistrationNumTextBox.Visible = true;
						TransportRegistrationNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("44DC639F-2A8A-4269-96C3-0BD99D90C5BA", "Transport Reg. No.");
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
