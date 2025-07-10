using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class LocalCartageXmlImportTask : XmlImportTask
	{
		public LocalCartageXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, notify, NotificationDataRegistry.Instance.LocalCartageNotificationGroup)
		{
		}

		public LocalCartageXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.LocalCartageXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("562a2cce-008d-4b0a-840e-cc889fc91a7e", "Port Transport XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new LocalCartageDataImporter();
		}
	}
}
