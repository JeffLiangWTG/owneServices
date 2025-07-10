using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public abstract class ScanForOutturnHost
	{
		readonly ZForm hostForm;
		readonly ScanMasterBill hostBO;

		public ScanForOutturnHost(ZForm form, ScanMasterBill hostBO)
		{
			hostForm = Argument.NotNull(form, "form");
			this.hostBO = hostBO;
		}

		public void ScanForOutturnClick(object sender, EventArgs args)
		{
			var accessManager = new ScanAccessManager();
			if (!accessManager.HasAccess)
			{
				ShowError(
					Declaration.GUI.Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c81ee51", "License error"),
					Declaration.GUI.Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c81ee50", "To start scanning for outturn you need to obtain {0} license.", accessManager.LicenceName)
					);
				return;
			}

			string message = hostBO.Validate();

			if (!string.IsNullOrEmpty(message))
			{
				ShowError(
					Declaration.GUI.Res.GetString("22ec7290-87bd-48c9-89fe-ee7d3c81ee48", "Validation Error"),
					message
					);

				return;
			}

			hostBO.RefreshUnderbonds();

			var wizardForm = GetScanForOutturnWizard(hostBO);
			ZFormModaliser.Show(wizardForm, this.hostForm);
		}

		protected abstract ScanForOutturnWizard GetScanForOutturnWizard(ScanMasterBill hostBO);

		protected virtual void ShowError(string caption, string message)
		{
			Globals.Message.Show(
				   message,
				   caption,
				   MessageBoxButtons.OK,
				   MessageBoxIcon.Error);
		}
	}
}
