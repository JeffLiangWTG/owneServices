using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class StandaloneCommercialInvoiceDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestTILVAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_ICN = "1";
			invoiceLine.AddInfo.ZA_TILV = "100.00AUD";

			var writer = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var shipment = writer.GetDataObject(invoice);
			var addInfos = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoCollection;
			AssertNotNull(addInfos.FirstOrDefault(x => x.Key.HasValue && x.Value.HasValue && x.Key.Value == UniversalExtensions.TILV4Warehouse && x.Value.Value == "100.00"));
		}
	}
}
