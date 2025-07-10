using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI.Testing;

public sealed class XadesCertificatePinHandlerForTest : XadesCertificatePinHandler
{
	public XadesCertificatePinHandlerForTest(Form parentForm, string certificateSerialNumber, UserEnterableTokenPin userEnterableTokenPin)
		: base(parentForm)
	{
		this.certificateSerialNumber = certificateSerialNumber;
		this.userEnterableTokenPin = userEnterableTokenPin;
	}

	protected override bool CanLocateCertificateFromPluggedUsbToken(CryptokiExternalPassword cryptokiCertificate)
	{
		return string.IsNullOrWhiteSpace(this.certificateSerialNumber)
			? base.CanLocateCertificateFromPluggedUsbToken(cryptokiCertificate)
			: cryptokiCertificate.GP_CertificateSerialNumber == this.certificateSerialNumber;
	}

	protected override UserEnterableTokenPin GetNewUserEnterableTokenPin() => userEnterableTokenPin;

	readonly string certificateSerialNumber;
	readonly UserEnterableTokenPin userEnterableTokenPin;
}
