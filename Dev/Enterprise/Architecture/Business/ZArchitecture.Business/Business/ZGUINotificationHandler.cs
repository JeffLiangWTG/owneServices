using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZGUINotificationHandler : INotificationHandler
	{
		public void ReportInformation(string message, string caption)
		{
			Globals.Message.ShowInformation(message, caption);
		}

		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			Globals.Message.ShowError(message, caption);
		}
	}
}
