using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbHealth.Check
{
	class DbServerAddressChecker : IChecker
	{
		const string RegistryItemActionTemplate = "Avoid using IP address and ensure the fully qualified domain name of the server is used for the registry item {0}.";
		const string RegistryItemWarningTemplate = "The registry item {0} is configured using IP address instead of fully qualified domain name.";
		const string DatabaseNameActionTemplate = "Avoid using IP address and ensure the fully qualified domain name of the server is used to launch the application.";
		const string DatabaseNameWarningTemplate = "An IP address is being used to connect to the SQL server instead of the FQDN.";

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			CheckAndAddWarningIfRequired(SystemDataRegistry.Instance.BiAuditServer, warningList);
			CheckAndAddWarningIfRequired(SystemDataRegistry.Instance.BiDataWarehouseServer, warningList);
			CheckAndAddWarningIfRequired(SystemDataRegistry.Instance.ReportingDbServerNames, warningList);
			CheckAndAddWarningIfRequired(SystemDataRegistry.Instance.ModuleQueryDbServerNames, warningList);

			CheckAndAddWarningForDbServerName(Db.ServerName, warningList);
		}

		internal void CheckAndAddWarningForDbServerName(string dbServerName, DbHealthWarningList warningList)
		{
			if (ContainsIpAddress(dbServerName))
			{
				warningList.Add(new ServerWarning(
					source: "Database Server Address",
					warningType: ServerWarning.ConfiguredServerAddressWarning,
					description: DatabaseNameWarningTemplate,
					action: DatabaseNameActionTemplate));
			}
		}

		void CheckAndAddWarningIfRequired(StringRegistryItem registryItem, DbHealthWarningList warningList)
		{
			if (ContainsIpAddress(registryItem.Value))
			{
				AddWarning(registryItem, warningList);
			}
		}

		void CheckAndAddWarningIfRequired(StringArrayRegistryItem registryItem, DbHealthWarningList warningList)
		{
			if (registryItem.Value.Any(serverName => ContainsIpAddress(serverName)))
			{
				AddWarning(registryItem, warningList);
			}
		}

		void AddWarning(IRegistryItemInternals registryItem, DbHealthWarningList warningList)
		{
			var path = registryItem.HumanReadableRegistryPath();
			warningList.Add(new ServerWarning(
				source: path,
				warningType: ServerWarning.ConfiguredServerAddressWarning,
				description: string.Format(RegistryItemWarningTemplate, path.QuoteName('"')),
				action: string.Format(RegistryItemActionTemplate, path.QuoteName('"'))));
		}

		string IChecker.Description
		{
			get { return "Check all configured database server addresses in cargowise registry."; }
		}

		readonly static Lazy<Regex> IPRegEx = new Lazy<Regex>(() =>
		{
			var regex = new Regex(@"
(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|
([0-9a-fA-F]{1,4}:){1,7}:|
([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|
([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|
([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|
([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|
([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|
[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|
:((:[0-9a-fA-F]{1,4}){1,7}|:)|
fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|
::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|
([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))|
(\d+).(\d+).(\d+).(\d+)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
			return regex;
		}, false);

		public static bool ContainsIpAddress(string serverAddress)
		{
			if (string.IsNullOrWhiteSpace(serverAddress))
			{
				return false;
			}
			var matchresult = IPRegEx.Value.Match(serverAddress);
			return matchresult.Success;
		}
	}
}
