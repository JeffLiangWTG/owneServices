using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class CommercialInvoiceXmlImportTask : XmlImportTask
	{
		public CommercialInvoiceXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.CommercialInvoiceXmlImport)
		{ }

		protected override DataImporter NewImporter()
		{
			return new InvoiceXmlDataImporter(StandAloneInvoiceValueObjectDataAdapter.New());
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("39856dc8-e06c-42c9-b2e1-a1f23222961a", "Commercial Invoice XML Import"); }
		}
	}
}
