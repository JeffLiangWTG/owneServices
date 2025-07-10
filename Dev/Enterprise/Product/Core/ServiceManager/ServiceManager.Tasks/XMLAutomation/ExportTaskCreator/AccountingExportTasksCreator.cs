using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class AccountingExportTasksCreator : CompanyLevelXMLTasksCreator
	{
		public AccountingExportTasksCreator(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		protected override IRegistryItem RegistryForImportOrExportTask
		{
			get { return SystemDataRegistry.Instance.AccountingTransactionsExport; }
		}

		protected override XMLTask CreateImportOrExportTask(GlbCompany company)
		{
			return new AccTransactionsExportTask(Notify, new AccountingTransactionExporter(Factory), RegistryForImportOrExportTask, company);
		}
	}
}
