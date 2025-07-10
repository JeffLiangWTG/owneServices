using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class SystemToSystemCertificateControl : RegistryBusinessObjectTemplateZUserControl
	{
		public SystemToSystemCertificateControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			resetButton.Enabled = !readOnly;
		}

		void ResetButton_Click(object sender, EventArgs e)
		{
			var message = Res.GetString("962F1621-7F67-4646-9DB6-3B29D18B7DE0", "Are you sure to reset the certificate?");
			var reset = Res.GetString("2A05EC59-4D41-40B4-8F55-F565DE905946", "Reset");
			if (Globals.Message.ShowConfirmation(message, reset, reset, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				var tokenServicesFactory = ObjectFactory.Get<ITokenServicesFactory>();
				var tokenConfigWriterService = tokenServicesFactory.GetTokenConfigWriterService();
				try
				{
					var resetAccessTokenRequest = new ResetAccessTokenRequest();
					var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(60));
					_ = tokenConfigWriterService.ResetAccessTokenAsync(resetAccessTokenRequest, cancellation.Token).GetAwaiter().GetResult();

					var newTrustInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;
					this.SetDataBinding(newTrustInfo, "");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}
	}
}
