using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public sealed class CertificateCryptokiDetails : ICryptokiDetails
{
	public CertificateCryptokiDetails(GlbCertificatePassword password)
	{
		this.password = Argument.NotNull(password, nameof(password));
	}

	readonly GlbCertificatePassword password;

	ZString ICryptokiDetails.LibraryName => password.LibraryName;

	ZString ICryptokiDetails.CertificateSerialNumber => password.GP_CertificateSerialNumber;

	ITokenPinStore ICryptokiDetails.TokenPinStore => tokenPinStore ??= CertificateTokenPinStore.Instance;
	ITokenPinStore tokenPinStore;
}
