using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.RemoteDesktopServices.Server.COM
{
	public class ErrorReporter : IErrorReporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public void Report(string key, string message, Exception exception)
		{
			MessageBox.Show(message + (message != exception.Message ? "\r\n" + exception.Message : ""), "Remote Desktop Services Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception exception)
		{
		}

		public void Clear()
		{
		}
	}
}
