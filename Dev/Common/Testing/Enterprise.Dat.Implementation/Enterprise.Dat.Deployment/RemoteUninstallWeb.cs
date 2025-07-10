using System.Management.Automation;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	public class RemoteUninstallWeb
	{
		public void Invoke(string targetHost, string webServerInstallPath, string databaseServer, string databaseName, string domain, bool disallowDuplicateSites, ITaskLogger logger, bool verbose)
		{
			using (var scriptFile = EmbeddedPowerShellScript.GetLocalTempFile("RemoteUninstallWeb.ps1"))
			using (var ps = PowerShell.Create())
			{
				ps.AddCommand(scriptFile.Filename);
				ps.AddParameter("targetHost", targetHost);
				ps.AddParameter("webServerInstallPath", webServerInstallPath);
				ps.AddParameter("databaseServer", databaseServer);
				ps.AddParameter("databaseName", databaseName);
				ps.AddParameter("domain", domain);
				ps.AddParameter("disallowDuplicateSites", disallowDuplicateSites);
				ps.InvokeWithLogging(logger, verbose);
			}
		}
	}
}
