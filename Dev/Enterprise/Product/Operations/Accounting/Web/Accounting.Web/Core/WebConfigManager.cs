#if NETFRAMEWORK
using System.Configuration;
using System.Web.Configuration;
#endif
using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Accounting.Web.Core
{
	public static class WebConfigManager
	{
#if NETFRAMEWORK
		public static bool Validate()
		{
			bool ok = false;
			Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");
			if (configuration != null)
			{
				CustomErrorsSection customErrors = (CustomErrorsSection)configuration.GetSection("system.web/customErrors");
				if (customErrors != null)
				{
					if (customErrors.Mode == CustomErrorsMode.Off)
					{
						ok = true;
					}
				}
			}
			return ok;
		}
#endif

		#region Database Server and Name

		public static string EnterpriseDbServer
		{
			get { return Db.ServerName; }
		}

		public static string EnterpriseDbName
		{
			get { return Db.DatabaseName; }
		}

		#endregion

		#region Environment

		public static void InitialiseEnvironment()
		{
			WebInitialiser.Initialise();
		}

		#endregion
	}
}
