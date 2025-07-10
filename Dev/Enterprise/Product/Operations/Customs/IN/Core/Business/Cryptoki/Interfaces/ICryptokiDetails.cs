using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

public interface ICryptokiDetails
{
	ZString LibraryName { get; }

	ZString CertificateSerialNumber { get; }

	ITokenPinStore TokenPinStore { get; }
}
