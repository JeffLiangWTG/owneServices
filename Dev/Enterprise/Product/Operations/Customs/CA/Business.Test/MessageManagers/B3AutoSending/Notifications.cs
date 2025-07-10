using System.Text;
using CargoWise.ComponentModel;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class Notifications : INotifications
	{
		public void Add(INotification notification)
		{
			text.AppendLine(notification.Message);
		}

		readonly StringBuilder text = new StringBuilder();

		public override string ToString()
		{
			return text.ToString();
		}
	}
}
