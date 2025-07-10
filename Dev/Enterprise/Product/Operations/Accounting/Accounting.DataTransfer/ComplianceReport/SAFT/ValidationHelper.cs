using System.Xml.Schema;

using CargoWise.ComponentModel;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class ValidationHelper
	{
		internal ValidationHelper(INotifications notifications)
		{
			Notifications = notifications;
		}

		readonly INotifications Notifications;

		internal void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			if (e.Severity == XmlSeverityType.Warning)
			{
				Notifications.AddWarning(Res.GetString("826548e7-a38c-45f0-9487-86f0c04e2f01", "{0} (Line {1}, Position {2})", e.Message, e.Exception.LineNumber, e.Exception.LinePosition));
			}
			else if (e.Severity == XmlSeverityType.Error)
			{
				Notifications.AddError(Res.GetString("826548e7-a38c-45f0-9487-86f0c04e2f01", "{0} (Line {1}, Position {2})", e.Message, e.Exception.LineNumber, e.Exception.LinePosition));
			}
		}
	}
}
