using System.Linq;
using Enterprise.Customs.CA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestExportStandaloneCommercialInvoiceDataStateOfOrigin()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "501999";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "S222101575080";

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			invoiceLine.JI_StateOrRegionOfOrigin = "ON";

			var writer = new StandaloneCommercialInvoiceDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var declarationData = writer.GetDataObject(invoice);

			var invoiceData = declarationData.CommercialInfo.CommercialInvoiceCollection.FirstOrDefault(x => x.InvoiceNumber.GetValueOrDefault() == "501999");
			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);

			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault(x => x.PartNo.GetValueOrDefault() == "S222101575080");
			AssertEquals("invoiceLineData.CountryOfOrigin.Code", "CA", invoiceLineData.CountryOfOrigin.Code);
			AssertEquals("invoiceLineData.StateOfOrigin.Code", "ON", invoiceLineData.StateOfOrigin.Code);
			AssertEquals("invoiceLineData.AddInfoCollection.ProvinceOfOrigin", "ON", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin).Value);
		}
	}
}
