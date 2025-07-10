using System;
using System.IO;
using CargoWise.ComponentModel;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class InvoiceFlatFileImporter : FlatFileImporter
	{
		public InvoiceFlatFileImporter(Guid notificationGroupPk)
		{
			this.NotificationGroupPk = notificationGroupPk;
		}

		protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
		{
			new FlatFileUnattachedInvoiceDataImporterWithNotification(notifications, NotificationGroupPk, dataFile.FullName).Import();
		}

		readonly Guid NotificationGroupPk;
	}
}
