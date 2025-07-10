using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static System.FormattableString;

namespace CargoWiseOne.WebInfrastructure.Integration.Strong.Test.Helpers
{
	class SiteInstallerViaAppManager : WTG.DevTools.TestFramework.ExecuteOrInvokeViaAppManager
	{
		public static void ExecuteOrInvoke(string serverName, string databaseName, string siteName)
		{
			new SiteInstallerViaAppManager
			{
				serverName = serverName,
				databaseName = databaseName,
				siteName = siteName,
			}.ExecuteOrInvoke();
		}

		public override void Execute()
		{
			var siteInstaller = new SiteInstaller();
			var sites = InstallSites.LoadSitesFromConfig().InstallSiteItems;
			sites.Where(x => x.Install).ToList().ForEach(x => x.Install(siteName));

			using var sqlContext = new WebUpgradeSqlContext(serverName, databaseName, () =>
			{
				var connectionString = GetConnectionString(
					serverName,
					databaseName);

				using var sqlConnection = new SqlConnection(connectionString);
				sqlConnection.Open();
				return sqlConnection;
			});

			siteInstaller.Install(serverName, sqlContext, sites);

			AddHostNameToHostsForLocalHostNameResolution(siteName);
		}

		static string GetConnectionString(string serverName, string databaseName)
		{
			var connStringBuilder = new SqlConnectionStringBuilder
			{
				PersistSecurityInfo = false,
				Pooling = true,
				ApplicationName = "CargoWiseOne.WebInfrastructure.TestFramework",
				DataSource = serverName,
				InitialCatalog = databaseName,
				IntegratedSecurity = true,
				Encrypt = false,
			};

			return connStringBuilder.ConnectionString;
		}

		static void AddHostNameToHostsForLocalHostNameResolution(string siteName)
		{
			var hostName = siteName.GetHostName();
			var hostsFilePath = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");
			var lines = File.ReadAllLines(hostsFilePath);
			var configuredLocalHostNames = new HashSet<string>();

			foreach (var line in lines)
			{
				var match = Regex.Match(line, @"^\s*127\.0\.0\.1\s+(?<hostName>.*)$");
				if (match.Success)
				{
					configuredLocalHostNames.Add(match.Groups["hostName"].Value?.Trim());
				}
			}

			if (!configuredLocalHostNames.Contains(hostName))
			{
				File.AppendAllText(hostsFilePath, Invariant($"{Environment.NewLine}127.0.0.1        {hostName}{Environment.NewLine}"));
			}
		}

		protected override object[] GetState()
		{
			return new object[] { serverName, databaseName, siteName };
		}

		protected override void ReadState(object[] state)
		{
			serverName = (string)state[0];
			databaseName = (string)state[1];
			siteName = (string)state[2];
		}

		string serverName;
		string databaseName;
		string siteName;
	}
}
