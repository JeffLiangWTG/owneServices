using System.Management.Automation;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	public class RemoteCleanupOrphanedWeb
	{
		public void Invoke(string targetHost, string webServerInstallPath, ITaskLogger logger, bool verbose)
		{
			using var scriptFile = EmbeddedPowerShellScript.GetLocalTempFile("RemoteCleanupOrphanedWeb.ps1");
			using var ps = PowerShell.Create();

			_ = ps.AddCommand(scriptFile.Filename);
			_ = ps.AddParameter("targetHost", targetHost);
			_ = ps.AddParameter("webServerInstallPath", webServerInstallPath);

			ps.InvokeWithLogging(logger, verbose: verbose);
		}
	}
}
