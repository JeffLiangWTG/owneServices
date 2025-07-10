using System;
using System.Management.Automation;
using System.Text;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	static class PowerShellExtensions
	{
		public static void InvokeWithLogging(this PowerShell ps, ITaskLogger logger, bool verbose)
		{
			var allOutput = new StringBuilder();
			var output = ps.Invoke();

			foreach (var psObject in output)
			{
				var lineToLog = psObject.ToString();
				if (verbose || ps.HadErrors || !IsVerbose(lineToLog))
				{
					allOutput.AppendLine(psObject.ToString());
				}
			}

			logger.RecordInfo(allOutput.ToString());

			if (ps.HadErrors)
			{
				var errorOutput = new StringBuilder();
				foreach (var errorRecord in ps.Streams.Error.ReadAll())
				{
					errorOutput.AppendLine(errorRecord.Exception.Message);
					errorOutput.AppendLine(errorRecord.ScriptStackTrace).AppendLine();
					if (errorRecord.ErrorDetails != null)
					{
						errorOutput.AppendLine(errorRecord.ErrorDetails.ToString());
					}

					if (errorRecord.Exception is RemoteException remoteException)
					{
						errorOutput.AppendLine(remoteException.SerializedRemoteException.ToString());
					}
				}

				logger.RecordInfo(errorOutput.ToString());
				throw new InvalidOperationException($"Remote Powershell failed:{errorOutput}");
			}
		}

		static bool IsVerbose(string line)
		{
			return line.StartsWith("[Debug]") || line.StartsWith("[Verbose]");
		}
	}
}

