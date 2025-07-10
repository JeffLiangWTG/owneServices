using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class TCVPRUserControl : ZUserControl
	{
		public TCVPRUserControl(bool isOnInvoiceLine)
		{
			InitializeComponent();
			this.isOnInvoiceLine = isOnInvoiceLine;
		}

		readonly bool isOnInvoiceLine;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var pgaHeader = CurrentDataItem as TCPGAHeader;
			if (pgaHeader != null)
			{
				pgaHeader.CA_SubProgramInfo.ValueChanged -= OnCA_SubProgram_Changed;
				pgaHeader.CA_SubProgramInfo.ValueChanged += OnCA_SubProgram_Changed;
				OnCA_SubProgram_Changed(null, null);

				foreach (var control in subProgramControls.Values)
				{
					control.SetDataBinding(pgaHeader, "");
				}
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var pgaHeader = CurrentDataItem as TCPGAHeader;
			if (pgaHeader != null)
			{
				pgaHeader.CA_SubProgramInfo.ValueChanged -= OnCA_SubProgram_Changed;
			}
		}

		void OnCA_SubProgram_Changed(object sender, EventArgs e)
		{
			var pgaHeader = CurrentDataItem as TCPGAHeader;
			if (pgaHeader != null && (pgaHeader.AddInfoLookups.SubProgramCodesList.ContainsCode(pgaHeader.CA_SubProgram) || pgaHeader.CA_SubProgram.IsEmpty))
			{
				var oldProgramControl = DetailsPanel.Controls.Cast<Control>().FirstOrDefault();
				var programControl = GetProgramControl(pgaHeader.CA_SubProgram);
				DetailsPanel.Controls.Clear();
				if (programControl != null)
				{
					if (oldProgramControl != null)
					{
						oldProgramControl.Visible = false;
					}

					programControl.Visible = true;
					DetailsPanel.Controls.Add(programControl);
				}
			}
		}

		ZUserControl GetProgramControl(string programCode)
		{
			if (!subProgramControls.TryGetValue(programCode, out ZUserControl result))
			{
				switch (programCode)
				{
					case TCPGAVehicleProgramCodes.Codes.PIG:
						result = new ZUserControl();
						break;
					case TCPGAVehicleProgramCodes.Codes.PIL:
						result = new TCPILUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VCC:
						result = new TCVCCUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VFC:
						result = new TCVFCUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VFS:
						result = new TCVFSUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VAE:
						result = new TCVAEUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VCR:
						result = new TCVCRUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VUV:
						result = new TCVUVUserControl(isOnInvoiceLine);
						break;
					case TCPGAVehicleProgramCodes.Codes.VVP:
						result = new TCVVPUserControl(isOnInvoiceLine);
						break;
				}

				var pgaHeader = CurrentDataItem as TCPGAHeader;
				if (result != null && pgaHeader != null)
				{
					result.Dock = DockStyle.Fill;
					result.SetDataBinding(pgaHeader, string.Empty);
					subProgramControls.Add(programCode, result);
				}
			}

			return result;
		}

		readonly Dictionary<string, ZUserControl> subProgramControls = new Dictionary<string, ZUserControl>();
	}
}
