using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestTILVAddInfo()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);
			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			var addInfo = new UniversalAddInfo();
			invoiceLine.AddInfoCollection.Add(addInfo);
			addInfo.Key = UniversalExtensions.TILV4Warehouse;
			addInfo.Value = "100.00AUD";
			addInfo = new UniversalAddInfo();
			invoiceLine.AddInfoCollection.Add(addInfo);
			addInfo.Key = AUAddInfo.Schema.ZA_REL_Hidden.Substring(3);
			addInfo.Value = CMRRelatedTransaction.Yes.Code;

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var invoiceLineBO = declarationBO.Invoices[0].JobComInvoiceLines[0];
			AssertContains(AUAddInfo.Schema.ZA_REL_Hidden.Substring(3) + "=" + CMRRelatedTransaction.Yes.Code, invoiceLineBO.JI_AddInfo);
			AssertNotContains(UniversalExtensions.TILV4Warehouse + "=100.00", invoiceLineBO.JI_AddInfo);
		}
	}
}
