using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licensing.GUI
{
	public partial class LicenseAgreementUserControl : ZUserControl
	{
		public LicenseAgreementUserControl(TaskCompletionSource<bool> tcs)
		{
			InitializeComponent();

			if (!DesignMode)
			{
				registration = ObjectFactory.Get<IProductRegistration>();
			}

			this.jobTitleLabel.Text = Res.GetString("5961a2ac-97ce-49e2-863c-fe8059f0509a", "Job Title");
			this.jobTitleTextBox.Text = Env.CurrentUser.Title;
			this.fullNameLabel.Text = Res.GetString("336e28f0-8705-44dd-b8cc-881558003019", "Full Name");
			this.fullNameTextBox.Text = Env.CurrentUser.FullName;

			CaptionRenderingEnabled = true;

			declarationLabel.Text = Res.GetString("c38bd67c-960d-4230-aa36-37899e39b830", "Declaration");
			acceptanceDescription.Text = Res.GetString("e6fc1b31-c669-4350-b898-5be53fa755f9", "By clicking Accept, you are confirming you are authorized to accept these legally binding terms and conditions on behalf of the Customer for the use of CargoWise Next and its sub products.");
			confirmationCheckBox.Text = Res.GetString("772f7dbb-0a5a-48bb-9518-9daf1efc3755", "I declare that I am an authorized officer of the Customer, and I have the authority to enter into this agreement by checking this box");
			acceptButton.Text = Res.GetString("3f9cc445-0d4d-4df8-825e-34896d4433a7", "Accept");
			cancelButton.Text = Res.GetString("75f354fa-5475-4e29-a4e2-840873d50222", "Cancel");

			this.tcs = tcs;
		}

		readonly TaskCompletionSource<bool> tcs;
		readonly IProductRegistration registration;
		LicenseAgreement Agreement => DataSource as LicenseAgreement;

		string GetIPAddress(GlbStaff currentUser)
		{
			try
			{
				var productKey = registration.Key;
				return ObjectFactory.Get<IWiseCloudSecurityClient>().GetClientIPAddress(productKey.EnterpriseCode + productKey.ServerCode, currentUser.GS_LoginName);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("IP Retrieval Failed", ex.ToString());
				return "0.0.0.0";
			}
		}

		void AcceptButton_Click(object sender, EventArgs e)
		{
			if (confirmationCheckBox.Checked)
			{
				var currentUser = GlbStaff.CurrentUser;
				var agreement = Agreement;
				agreement.LAG_Status = LicenseAgreementStatusList.Codes.Queued;
				agreement.LAG_AcceptedByEmail = currentUser.GS_EmailAddress;
				agreement.LAG_AcceptedByName = currentUser.GS_FullName;
				agreement.LAG_AcceptedIPAddress = GetIPAddress(currentUser);
				agreement.LAG_AcceptedTimeUtc = ZDateTime.UtcNow;

				tcs.SetResult(true);
			}
			else
			{
				var msg = Res.GetString("c7d0d056-fb17-4c12-ae48-a7fcffa0faef", @"To execute this CargoWise Next license agreement, you must be an authorized officer of ""the Customer"".

By checking the check box [x], you declare you have the Authority to enter this agreement by clicking [Accept].");
				Globals.Message.Show(msg);
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			var msg = Res.GetString("17d92c28-b3f3-4c2b-8a47-f87aeb35edf7", "CargoWise Next will now quit - Please have an authorized officer log in and accept the agreement:");
			Globals.Message.Show(msg);
			System.Environment.Exit(0);
		}
	}
}
