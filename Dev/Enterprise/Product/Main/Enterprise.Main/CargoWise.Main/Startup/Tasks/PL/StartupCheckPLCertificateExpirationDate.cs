using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using GlbStaff = Enterprise.MasterFiles.Business.GlbStaff;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class StartupCheckPLCertificateExpirationDate : IPostLoginTask
	{
		const int CertificateExpiryWarningBufferDays = 14;

		public bool ShouldExecute()
		{
			return true;
		}

		public void Execute()
		{
			if (!GlbStaff.CurrentUser.GS_IsSystemAccount && IsCertificateExpiringSoon())
			{
				Globals.Message.Show(Res.GetString("86269978-ac1c-4d75-a17d-e93322cc09d1", "Your certificate will expire soon. You can continue working, but you must apply for a new one."),
					Res.GetString("8cc8b7cf-0101-482b-85b6-8b5e76b2cbcd", "Warning"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}

		bool IsCertificateExpiringSoon()
		{
			var certificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
			var certificateExpiryDate = certificate.GP_ExpiryDate;
			return certificate != null
				&& !certificate.GP_Certificate.IsEmpty
				&& !certificateExpiryDate.IsEmpty
				&& (certificateExpiryDate >= ZDateTime.Now && certificateExpiryDate < ZDateTime.Now.AddDays(CertificateExpiryWarningBufferDays));
		}

		public string TaskDescription => Res.GetString("3614eee7-7c3c-4be3-afaa-7efef52f41b1", "Check the certificate expiration date");
	}
}
