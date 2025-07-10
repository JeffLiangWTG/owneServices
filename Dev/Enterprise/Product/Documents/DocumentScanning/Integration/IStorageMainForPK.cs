using CargoWise.Types;

namespace Enterprise.DocumentScanning.Integration
{
	public interface IStorageMainForPK
	{
		IDocumentsView GetStorageMain(ZGuid bizOPK);
	}
}
