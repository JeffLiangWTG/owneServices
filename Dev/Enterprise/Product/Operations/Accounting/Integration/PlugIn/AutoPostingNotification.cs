using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration
{
	public class AutoPostingNotification
	{
		public AutoPostingNotification(ZGuid[] emailReceipents, bool suppressUnpostARNotificationWhenAPPosted)
		{
			EmailRecipients = emailReceipents;
			SuppressUnpostARNotificationWhenAPPosted = suppressUnpostARNotificationWhenAPPosted;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] EmailRecipients { get; }

		public ZBool SuppressUnpostARNotificationWhenAPPosted { get; }
	}
}
