using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native
{
	public interface INativeFactoryProvider
	{
		BusinessObjectFactory GetNewFactory(DbConnection connection);
	}
}
