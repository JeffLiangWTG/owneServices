using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.TrustedMessaging.Intergration;

namespace Enterprise.Licensing.GUI
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Currently, we are only suspending the relevant user agreements of CWN. When it is determined that it is completely unused, the relevant code will be removed")]
	public class CargoWiseNextNonProductionLicenseStrategy : CargoWiseNextLicenseStrategy
	{
		public CargoWiseNextNonProductionLicenseStrategy()
		{
			Factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = FormattableString.Invariant($"{nameof(CargoWiseNextNonProductionLicenseStrategy)} Factory"),
			};

			Client = ObjectFactory.Get<IUserPortalClient>();
		}

		BusinessObjectFactory Factory { get; }
		IUserPortalClient Client { get; }

		protected override UserControl GetAgreementContentControl(TaskCompletionSource<bool> dialogResult)
		{
			var url = Client.GetEnterpriseAgreementUrlAsync(LicenseAgreementTypeList.Codes.CargoWiseNext).Result.Response.Url;

			if (!string.IsNullOrEmpty(url))
			{
				return new LicenseAgreementWebAcceptanceUserControl(url, dialogResult);
			}

			return null;
		}

		protected override void OnContentDialogClosed(bool dialogResult) { }

		protected override bool NeedsLicenseCore()
		{
			if (base.NeedsLicenseCore())
			{
				var result = Client.GetEnterpriseAgreementUrlAsync(LicenseAgreementTypeList.Codes.CargoWiseNext).Result;

				if (result != null && result.Success)
				{
					return result.Response.Required;
				}
			}

			return false;
		}
	}
}
