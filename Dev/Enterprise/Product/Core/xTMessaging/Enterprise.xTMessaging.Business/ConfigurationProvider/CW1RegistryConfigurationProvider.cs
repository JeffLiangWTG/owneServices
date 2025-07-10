using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business.ConfigurationProvider;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Business
{
	public class CW1RegistryConfigurationProvider : IXtConfigurationProvider
	{
		public Configuration GetConfiguration()
		{
			var result = new Configuration();
			var registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			if (registrationKey != null)
			{
				var registry = DirectxTMessagingRegistry.Instance;
				var passwordHash = CargoWise.eHub.Common.SHA512Encryptor.Encrypt(registrationKey.DatabaseNumber + registrationKey.Password);
				var refDatabase = new ReferenceDatabaseUtils(Db.Connection);

				switch (registry.ConnectionToXTServer.Value)
				{
					case ConnectionToXTServerOptions.Codes.XtProduction:
						result.Connect = refDatabase.RefSysConfigLoader.Load(XTServerServerProdKey, ZDateTime.Today)?.ZRC_StringValue ?? "";
						result.CA = refDatabase.RefSysConfigLoader.Load(XTServerCertificateProdKey, ZDateTime.Today)?.ZRC_StringValue ?? "";
						break;

					case ConnectionToXTServerOptions.Codes.XtTest:
						result.Connect = refDatabase.RefSysConfigLoader.Load(XTServerServerTestKey, ZDateTime.Today)?.ZRC_StringValue ?? "";
						result.CA = refDatabase.RefSysConfigLoader.Load(XTServerCertificateTestKey, ZDateTime.Today)?.ZRC_StringValue ?? "";
						break;

					case ConnectionToXTServerOptions.Codes.XtLocal:
						result.Connect = registry.XTLocalDeveloperAddress.Value ?? "";
						result.CA = registry.XTLocalDeveloperCertificate.Value ?? "";
						break;
				}

				result.Application = new Application
				{
					URI = registrationKey.EnterpriseCode + registrationKey.ServerCode,
					Password = passwordHash
				};
			}
			return result.IsValid() ? result : null;
		}

		const string XTServerCertificateTestKey = "XTCATEST";
		const string XTServerCertificateProdKey = "XTCAPROD";
		const string XTServerServerTestKey = "XTSVRTEST";
		const string XTServerServerProdKey = "XTSVRPROD";
	}
}
