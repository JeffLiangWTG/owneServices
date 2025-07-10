using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	class ExporterDocumentsFromDeclarationTest : ExporterDocumentsBaseTest
	{
		public override BusinessObject GetBusinessObject
		{
			get
			{
				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var container = declaration.CusContainers.AddNew();
				return declaration;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Customs; }
		}
	}
}
