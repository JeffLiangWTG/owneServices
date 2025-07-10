using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business;

public interface ICryptokiGlbExternalPassword : IGlbExternalPassword
{
	ZString GP_Name { get; set; }

	ZString GP_CertificateSerialNumber { get; set; }

	ITokenPinStore TokenPinStore { get; }
}
