using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class WarehouseOrderFlatFileImporter : FlatFileImporter
	{
		public WarehouseOrderFlatFileImporter(GuidRegistryItem notificationGroup)
		{
			Argument.NotNull(notificationGroup, "notificationGroup");
			NotificationGroup = notificationGroup;
		}

		protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
		{
			var importer = new WhsOrderFlatFileDataImporterWithNotification(notifications, NotificationGroup, dataFile.FullName);
			importer.ImportData(dataFile.FullName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.WarehouseOrderFlatFileImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, dataFile.Name));
		}

		readonly GuidRegistryItem NotificationGroup;
	}
}
