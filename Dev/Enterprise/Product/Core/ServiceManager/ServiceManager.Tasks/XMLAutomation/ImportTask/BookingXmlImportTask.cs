using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class BookingXmlImportTask : XmlImportTask
	{
		public BookingXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.BookingsDataImportDirectory, notify, NotificationDataRegistry.Instance.BookingImportNotificationGroup)
		{
		}

		public BookingXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.BookingXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("65f04cfc-09f8-4fe4-a956-777f36111548", "Booking XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new BookingDataImporter();
		}
	}
}
