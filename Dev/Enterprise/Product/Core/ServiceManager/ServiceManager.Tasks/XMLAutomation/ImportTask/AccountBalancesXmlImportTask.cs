using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.AccountBalanceUpdate;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class AccountBalancesXmlImportTask : XmlImportTask
	{
		public AccountBalancesXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.ARAPBalancesUpdateImportDirectoryItem, notify, NotificationDataRegistry.Instance.UpdateAPARAccountBalancesProcessNotificationGroupItem)
		{
		}

		public AccountBalancesXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.AccountBalancesXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("c1f14ba0-3b79-4cc6-a69e-00c8e523b36d", "Account Balances Update Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new XmlDataImporter(new BalanceValueObjectDataAdapter());
		}
	}
}
