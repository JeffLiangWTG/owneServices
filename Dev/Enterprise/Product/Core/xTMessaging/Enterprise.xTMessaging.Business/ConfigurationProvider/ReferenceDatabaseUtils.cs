using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.xTMessaging.Business.ConfigurationProvider
{
	public class ReferenceDatabaseUtils
	{
		public ReferenceDatabaseUtils(DbConnection connection)
		{
			this.connection = connection;
		}

		public RefSysConfig.Loader RefSysConfigLoader
		{
			get
			{
				if (refSysConfigLoader == null)
				{
					refSysConfigLoader = new RefSysConfig.Loader(FactoryProvider.Current);
				}
				return refSysConfigLoader;
			}
		}

		protected RefSysConfig.Loader refSysConfigLoader;

		readonly DbConnection connection;

		BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (factoryProvider == null)
				{
					factoryProvider = new BusinessObjectFactoryProvider(connection);
					factoryProvider.Current.RefreshEnabled = false;
				}
				return factoryProvider;
			}
		}
		BusinessObjectFactoryProvider factoryProvider;
	}
}
