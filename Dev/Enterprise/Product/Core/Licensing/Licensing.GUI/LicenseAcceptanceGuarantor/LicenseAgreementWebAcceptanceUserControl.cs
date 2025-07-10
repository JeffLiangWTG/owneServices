using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licensing.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class LicenseAgreementWebAcceptanceUserControl : ZUserControl
	{
		public LicenseAgreementWebAcceptanceUserControl(string agreementLink, TaskCompletionSource<bool> tcs)
		{
			InitializeComponent();
			CaptionRenderingEnabled = true;

			agreementLinkLabel.Text = agreementLink;
			this.tcs = tcs;
		}

		readonly TaskCompletionSource<bool> tcs;

		void AgreementLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(agreementLinkLabel.Text);
		}

		void RecheckLicenseStatus_Click(object sender, EventArgs e)
		{
			var client = ObjectFactory.Get<IUserPortalClient>();
			var result = client.GetEnterpriseAgreementUrlAsync(LicenseAgreementTypeList.Codes.CargoWiseNext).Result.Response.Required;
			if (result)
			{
				Globals.Message.Show(Res.GetString("96bf230d-c861-424a-a1e2-dd5b3529f515", "The agreement has not been signed. Please follow the link, sign the agreement, then try again."));
			}
			else
			{
				Globals.Message.Show(Res.GetString("71917a1e-4eb7-47f6-a584-a37e830fc881", "The agreement has been signed."));
				tcs.SetResult(true);
			}
		}

		void ExitButton_Click(object sender, EventArgs e)
		{
			var msg = Res.GetString("17d92c28-b3f3-4c2b-8a47-f87aeb35edf7", "CargoWise Next will now quit - Please have an authorized officer log in and accept the agreement:");
			Globals.Message.Show(msg);
			System.Environment.Exit(0);
		}
	}
}
