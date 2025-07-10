using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataTargetDataObject : IDataObject
	{
		ZString? Type { get; set; }
		ZString? Key { get; set; }
		IOrganizationAddress Owner { get; set; }
	}
}
