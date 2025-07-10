using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ICustomsMessageFilenameProvider
{
	ZString GenerateFilename();
}
