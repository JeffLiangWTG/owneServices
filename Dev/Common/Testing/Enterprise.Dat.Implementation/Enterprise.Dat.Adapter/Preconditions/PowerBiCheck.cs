using System;
using System.Management;
using System.Net;
using Microsoft.Win32;

namespace Enterprise.Dat.Implementation.Preconditions
{
	public static class PowerBiCheck
	{
		#region IsPowerBi Constants

		const string ServerName = "localhost";

		#endregion

		#region IsPowerBi2016OrLater Functions

		public static bool IsPowerBiInstalledLocally()
		{
			using (var view64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (RegistryKey key = view64.OpenSubKey(@"Software\Microsoft\Microsoft SQL Server\PBIRS\Setup"))
			{
				if (key == null)
				{
					return false;
				}
			}
			return true;
		}

		static string GetWmiNameSpaceFrom(string serverName)
		{
			return @"\\" + serverName + @"\root\Microsoft\SqlServer\ReportServer\RS_PBIRS\v15";
		}

		public static bool IsPowerBiWorking(string machineName = ServerName)
		{
			var machineWmiNamespace = GetWmiNameSpaceFrom(machineName);

			ConnectionOptions connOptions = new ConnectionOptions();
			connOptions.Authentication = AuthenticationLevel.Default;

			ObjectGetOptions getOptions = new ObjectGetOptions();
			getOptions.Timeout = new TimeSpan(0, 0, 30);

			ManagementScope machineScope = new ManagementScope(machineWmiNamespace, connOptions);
			machineScope.Connect();

			var retVal = true;
			ManagementPath path = new ManagementPath("MSReportServer_Instance");
			using var serverClass = new ManagementClass(machineScope, path, getOptions);
			serverClass.Get();

			using var instances = serverClass.GetInstances();

			foreach (ManagementObject instance in instances)
			{
				ManagementBaseObject serverStatusObject = instance.InvokeMethod("GetReportServerUrls", null, null);

				Array urls = (Array)serverStatusObject["URLs"];
				int length = (int)serverStatusObject["Length"];

				if (length > 0)
				{
					Uri pbirsUrl = new Uri(urls.GetValue(0).ToString());

#if NETFRAMEWORK
					var webRequest = WebRequest.Create(pbirsUrl);
					webRequest.Credentials = CredentialCache.DefaultCredentials;

					using var response = (HttpWebResponse)webRequest.GetResponse();
					if (response.StatusCode != HttpStatusCode.OK)
					{
						retVal = false;
					}
#else
					var handler = new System.Net.Http.SocketsHttpHandler
					{
						Credentials = CredentialCache.DefaultCredentials,
					};

					using var httpClient = new System.Net.Http.HttpClient();

					using var response = httpClient.GetAsync(pbirsUrl).Result;
					if (response.StatusCode != HttpStatusCode.OK)
					{
						retVal = false;
					}
#endif
				}
				else
				{
					retVal = false;
				}
			}
			return retVal;
		}

		#endregion
	}
}
