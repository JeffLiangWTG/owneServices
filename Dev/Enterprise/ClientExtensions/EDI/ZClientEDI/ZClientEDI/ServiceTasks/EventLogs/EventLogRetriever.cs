using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.EventLogs
{
	public interface IEventLogRetriever
	{
		XElement Retrieve(string machineName, EventLogDataTransmissionHandler cacheLastLogHandler, ILogger serviceLogger);
	}

	class EventLogRetriever : IEventLogRetriever
	{
		const int TIME_LIMIT = 60000;

		/// <summary>
		/// </summary>
		/// <param name="machinename"></param>
		/// <param name="cacheLastLogHandler"></param>
		/// <returns>Could return null if could not found the information</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Excute wevtutil from cmd,  not opening a file or url")]
		public XElement Retrieve(string machinename, EventLogDataTransmissionHandler cacheLastLogHandler, ILogger serviceLogger)
		{
			Argument.NotNullOrEmpty(machinename, "machinename");
			Argument.NotNull(cacheLastLogHandler, "cacheLastLogHandler");

			using (Process p = new Process())
			{
				p.StartInfo = GetProcessStartInfo(machinename, cacheLastLogHandler);
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.RedirectStandardError = true;

				p.Start();
				var data = RetreiveEventLogs(machinename, serviceLogger, p);

				if (data != null)
				{
					data.Insert(0, "<EventLogs>");
					data.Append("</EventLogs>");
					return XElement.Parse(data.ToString());
				}
			}
			return null;
		}

		internal virtual StringBuilder RetreiveEventLogs(string machinename, ILogger serviceLogger, Process p)
		{
			// A deadlock condition can result if the parent process calls p.WaitForExit before p.StandardOutput.ReadToEnd and the child process writes enough text to fill the redirected stream.
			// So we will use asynchronous
			StringBuilder output = new StringBuilder();
			StringBuilder error = new StringBuilder();

			using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
			using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
			{
				p.OutputDataReceived += (sender, eventData) => RetreiveData(eventData, output, outputWaitHandle);
				p.ErrorDataReceived += (sender, eventData) => RetreiveData(eventData, error, errorWaitHandle);

				p.BeginOutputReadLine();
				p.BeginErrorReadLine();

				if (IsFinished(p, outputWaitHandle, errorWaitHandle))
				{
					if (error.Length > 0)
					{
						serviceLogger.Log(Integration.LogType.Warning, ZString.Format("Problem occurs in machine: {0}. {1}", machinename, error));
					}
					else if (output.Length == 0)
					{
						serviceLogger.Log(Integration.LogType.Debug, "No issues retrieved from Machine: " + machinename);
					}
					else
					{
						return output;
					}
				}
				else
				{
					serviceLogger.Log(Integration.LogType.Warning, ZString.Format("Time out error occurs in machine:{0} while retrieving event logs in {1} ms. The process will be killed.", machinename, TIME_LIMIT));
					try
					{
						p.CancelErrorRead();
						p.CancelOutputRead();
						p.Kill();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}

					p.WaitForExit(TIME_LIMIT);
				}
			}
			return null;
		}

		internal virtual bool IsFinished(Process p, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle)
		{
			return p.WaitForExit(TIME_LIMIT) && outputWaitHandle.WaitOne(TIME_LIMIT) && errorWaitHandle.WaitOne(TIME_LIMIT);
		}

		internal virtual void RetreiveData(DataReceivedEventArgs eventData, StringBuilder source, AutoResetEvent waitHandler)
		{
			if (eventData.Data == null)
			{
				waitHandler.Set();
			}
			else
			{
				source.AppendLine(eventData.Data);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Not a file path")]
		ProcessStartInfo GetProcessStartInfo(string machinename, EventLogDataTransmissionHandler cacheLastLogHandler)
		{
			Argument.NotNullOrEmpty(machinename, "machinename");
			Argument.NotNull(cacheLastLogHandler, "cacheLastLogHandler");
			var highWaterMark = cacheLastLogHandler.FindHighWaterMark(machinename);
			return highWaterMark == null ?
			 new ProcessStartInfo("wevtutil", "qe Application /rd:true /f:XML /q:\"*[*[(Level=1 or Level=2 or Level=3) and TimeCreated[timediff(@SystemTime) <= 86400000]]]\" /r:\"" + machinename + "\"")
			 : new ProcessStartInfo("wevtutil", "qe Application /rd:true /f:XML /q:\"*[*[(Level=1 or Level=2 or Level=3) and (EventRecordID > " + highWaterMark + ")]]\" /r:\"" + machinename + "\"");
		}
	}
}
