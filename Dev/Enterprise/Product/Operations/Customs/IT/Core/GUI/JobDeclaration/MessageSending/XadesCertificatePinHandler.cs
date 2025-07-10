using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Certificates;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ITGlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.GUI;

public class XadesCertificatePinHandler
{
	public XadesCertificatePinHandler()
	{
	}

	public XadesCertificatePinHandler(Form parentForm)
	{
		this.parentForm = parentForm;
	}

	#region XadesCertificatePinHandlerResult

	public enum XadesCertificatePinHandlerResult
	{
		None,
		Completed,
		Cancelled,
		UserCancelled,
		ErrorReported
	}

	#endregion

	public XadesCertificatePinHandlerResult HandleTokenPin()
	{
#if DEBUG
		if (GlbStaff.CurrentUser.IsSupportUserAndIsNotTestEnviroment())
		{
			return XadesCertificatePinHandlerResult.Completed;
		}
#endif

		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();

		if (cryptokiCertificate == null)
		{
			Globals.Message.ShowError(XadesCertificateIsMissingForTheCurrentUser, XadesCertificateCaptionError);
			return XadesCertificatePinHandlerResult.Cancelled;
		}

		var tokenPinStore = cryptokiCertificate.TokenPinStore;
		if (!tokenPinStore.IsPinCached())
		{
			if (!CanLocateCertificateFromPluggedUsbToken(cryptokiCertificate))
			{
				Globals.Message.ShowError(CannotLocateXadesCertificateForTheCurrentUser, XadesCertificateCaptionError);
				return XadesCertificatePinHandlerResult.ErrorReported;
			}

			if (!PromptUserToEnterTokenPin(tokenPinStore))
			{
				return XadesCertificatePinHandlerResult.UserCancelled;
			}
		}

		return XadesCertificatePinHandlerResult.Completed;
	}

	protected virtual bool CanLocateCertificateFromPluggedUsbToken(CryptokiExternalPassword cryptokiCertificate)
	{
		var certificateSelector = TokenCertificateSelector.New(cryptokiCertificate.GP_Name);
		return certificateSelector.CanLocateCertificate(cryptokiCertificate.GP_CertificateSerialNumber);
	}

	protected virtual UserEnterableTokenPin GetNewUserEnterableTokenPin() => new UserEnterableTokenPin();

	bool PromptUserToEnterTokenPin(ITokenPinStore tokenPinStore)
	{
		var userEnterableTokenPin = GetNewUserEnterableTokenPin();
		if (ZFormModaliser.ShowDialogAndDispose(new EnterCryptokiCertificatePinForm(userEnterableTokenPin), this.parentForm) == DialogResult.OK)
		{
			tokenPinStore.SetPin(userEnterableTokenPin.Pin);
			return true;
		}

		return false;
	}

	static string XadesCertificateCaptionError => Res.GetString("65D35DE9-5649-43A5-A388-106A5464A765", "Error: XADES Certificate");
	static string XadesCertificateIsMissingForTheCurrentUser => Res.GetString("D232A018-CC7C-4142-BA69-64A4A369605A", "XADES certificate is missing for the current user. Please fill XADES Certificate information in Staff and Resources > Credentials.");
	static string CannotLocateXadesCertificateForTheCurrentUser => Res.GetString("DE35E1BA-17A3-40FE-ADCE-0E68E337FF77", "Cannot locate XADES certificate for the current user. Please insert the certificate in your computer and retry the operation.");

	readonly Form parentForm;
}
