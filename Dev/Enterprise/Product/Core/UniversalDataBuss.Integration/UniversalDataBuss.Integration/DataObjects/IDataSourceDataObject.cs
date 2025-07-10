using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataSourceDataObject : IDataObject
	{
		ZString? Type { get; set; }
		ZString? Key { get; set; }
	}
}
