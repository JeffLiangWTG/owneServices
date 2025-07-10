using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Licensing.GUI
{
	public class CargoWiseNextLicenseStrategy : LicenseAcceptanceGuarantorStrategy
	{
		public CargoWiseNextLicenseStrategy()
		{
			Factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = FormattableString.Invariant($"{nameof(CargoWiseNextLicenseStrategy)} Factory"),
			};
		}

		BusinessObjectFactory Factory { get; }
		LicenseAgreement Agreement { get; set; }

		static string ProductCode => LicenseAgreementTypeList.Codes.CargoWiseNext;

		protected sealed async override Task GetAcceptanceCore()
		{
			if (HostPanel == null)
			{
				return;
			}

			var tcs = new TaskCompletionSource<bool>();
			var userControl = GetAgreementContentControl(tcs);
			if (userControl != null)
			{
				var panel = new LicenseAgreementBackgroundPanel(userControl);

				HostPanel.AllowOverlap(panel);
				HostPanel.Controls.Add(panel);
				panel.Dock = System.Windows.Forms.DockStyle.Fill;
				panel.BringToFront();

				var result = await tcs.Task;

				HostPanel.Controls.Remove(panel);
				panel.Dispose();

				OnContentDialogClosed(result);
			}
		}

		protected virtual UserControl GetAgreementContentControl(TaskCompletionSource<bool> dialogResult)
		{
			var userControl = new LicenseAgreementUserControl(dialogResult);
			userControl.SetDataBinding(Agreement, "");
			userControl.Dock = DockStyle.Fill;

			return userControl;
		}

		protected virtual void OnContentDialogClosed(bool dialogResult)
		{
			if (!dialogResult)
			{
				return;
			}

			try
			{
				if (Agreement.HasChanges)
				{
					Factory.Save();
				}
			}
			catch (ZSaveConcurrencyException ex)
			{
				Agreement.Reload();
				ResolveConcurrentUpdate(Agreement, ex);
			}
		}

		static void ResolveConcurrentUpdate(LicenseAgreement licenseAgreement, Exception ex)
		{
			switch (licenseAgreement.LAG_Status.ToString())
			{
				case LicenseAgreementStatusList.Codes.Accepted:
				case LicenseAgreementStatusList.Codes.Queued:
					Globals.Message.Show(Res.GetString("e423bcfb-4def-4128-9204-1559fcc97189", "Agreement was already accepted by another party."));
					break;
				case LicenseAgreementStatusList.Codes.Cancelled:
					Globals.Message.Show(Res.GetString("3ed9b968-a080-4f61-a0d0-540c17125913", "Agreement is no longer valid."));
					break;

				default:
					ErrorReporter.ReportOnce("b1d0397f-94c2-4511-9860-4eceb55dd0be", $"Unexpected status after concurrency error: {licenseAgreement.LAG_Status}", ex);
					break;
			}
		}

		static ZQuery GetPendingLicenseQuery()
		{
			var query = new ZQuery(LicenseAgreementSchema.LAG_Status, LicenseAgreementStatusList.Codes.Pending)
				.AddToFilter(LicenseAgreementSchema.LAG_Type, ProductCode);
			query.ReLoadExistingRows = true;
			return query;
		}

		protected override bool NeedsLicenseCore()
		{
			if (Agreement != null && Agreement.IsInDatabase)
			{
				Agreement.Reload();
			}

			if (Agreement == null || Agreement.LAG_Status != LicenseAgreementStatusList.Codes.Pending)
			{
				Agreement = Factory.LoadTop1<LicenseAgreement>(GetPendingLicenseQuery());

				if (Agreement == null && !Factory.ExistsInDatabase(LicenseAgreementSchema.Constants.TableName, new ZQuery(LicenseAgreementSchema.LAG_Type, ProductCode)))
				{
					// It's possible the LAG service task hasn't ever ran, so we call the web service directly.

					Agreement = GetAgreementFromWebService();
				}
			}

			return Agreement != null;
		}

		LicenseAgreement GetAgreementFromWebService()
		{
			var agreementMessage = ObjectFactory.Get<IUserPortalClient>()
				.GetUserAgreementAsync(ProductCode)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			if (agreementMessage != null && agreementMessage.Success && agreementMessage.Response.Required)
			{
				return LicenseAgreement.ImportNewAgreement(Factory, agreementMessage.Response, ProductCode);
			}
			return null;
		}
	}
}
