using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Modules.AirCargo
{
	public partial class DeveloperToolsForm : ZForm
	{
		public DeveloperToolsForm()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl);
		}

		void WriteOutput(string msg)
		{
			outputBox.Text = msg + System.Environment.NewLine + outputBox.Text;
		}

		void uploadButton_Click(object sender, EventArgs e)
		{
			var logger = new ServiceTaskLogger();
			new BISIUploadServiceTask(logger).RunTask(CancellationToken.None);

			if (logger.Count > 0)
			{
				WriteOutput(logger.ToString());
			}
		}

		void imageImportButton_Click(object sender, EventArgs e)
		{
			var logger = new ServiceTaskLogger();
			new DocumentImageImportServiceTask(logger).RunTask(CancellationToken.None);

			if (logger.Count > 0)
			{
				WriteOutput(logger.ToString());
			}
		}

		void queueProcessorButton_Click(object sender, EventArgs e)
		{
			var logger = new ServiceTaskLogger();
			new ResolutionQueueServiceTask(logger).RunTask(CancellationToken.None);

			if (logger.Count > 0)
			{
				WriteOutput(logger.ToString());
			}
		}
		
		class ServiceTaskLogger : ILogger
		{
			public void Log(LogType type, string message)
			{
				Log(type, message, null);
			}

			public void Log(LogType type, string message, Exception ex)
			{
				logLines.Add($"{type}|{message}{(ex != null ? $"|{ex}" : string.Empty)}");
			}
			
			public override string ToString()
			{
				var builder = new StringBuilder();
				logLines.ForEach(delegate (string line) { builder.AppendLine(line); });
				return builder.ToString();
			}

			public int Count => logLines.Count;

			readonly List<string> logLines = new();
		}
	}
}
