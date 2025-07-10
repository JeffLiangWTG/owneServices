using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Native
{
	public class FactoryProvider : INativeFactoryProvider
	{
		public BusinessObjectFactory GetNewFactory(DbConnection connection)
		{
			return new BusinessObjectFactory(connection)
			{
				RefreshEnabled = false,
				NameForDebugging = "Native Xml",
			};
		}
	}
}
