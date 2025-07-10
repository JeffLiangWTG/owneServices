using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// Summary description for ZWebNotificationHandler.
	/// </summary>
	public class ZWebNotificationHandler : INotificationHandler
	{
		public ZWebNotificationHandler(ZPage page)
		{
		}

		#region Properties

		[DefaultValue("")]
		public string Message { get; private set; }

		[DefaultValue("")]
		public string Caption { get; private set; }

		#endregion

		#region INotificationHandler Members

		public void ReportInformation(string message, string caption)
		{
			this.Message = message;
			this.Caption = caption;
		}

		public void ReportError(string message, string caption, string errorContext = null, Exception excpetion = null)
		{
			this.Message = message;
			this.Caption = caption;
		}

		#endregion
	}
}
