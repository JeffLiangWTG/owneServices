using System.Data;

namespace Enterprise.Registry.Business
{
	public interface IRegistryCollectionToTVP
	{
		DataTable CreateDataTable();
		string TVPType { get; }
	}
}
