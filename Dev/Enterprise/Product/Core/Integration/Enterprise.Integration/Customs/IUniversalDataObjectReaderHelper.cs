using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IUniversalDataObjectReaderHelper
	{
		ZString? GetFreightUnitForPackType(ZString? packType);
	}
}
