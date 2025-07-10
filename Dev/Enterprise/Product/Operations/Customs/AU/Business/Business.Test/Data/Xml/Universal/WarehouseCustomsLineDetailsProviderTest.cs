using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestIWarehouseCustomsLineDetailsMembers()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);
			invoice.AddInfoCollection = new List<UniversalAddInfo>();

			var addInfo = new UniversalAddInfo();
			addInfo.Key = AUAddInfo.Schema.ZA_HeaderREL_Hidden.Substring(3);
			addInfo.Value = CMRRelatedTransaction.Yes.Code;
			invoice.AddInfoCollection.Add(addInfo);

			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoiceLine.BondedWarehouseQuantity = 1;
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);

			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			addInfo = new UniversalAddInfo();
			addInfo.Key = AUAddInfo.Schema.ZA_REL_Hidden.Substring(3);
			addInfo.Value = CMRRelatedTransaction.Default.Code;
			invoiceLine.AddInfoCollection.Add(addInfo);
			addInfo = new UniversalAddInfo();
			addInfo.Key = UniversalExtensions.TILV4Warehouse;
			addInfo.Value = "100.00";
			invoiceLine.AddInfoCollection.Add(addInfo);

			var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(shipment);
			var lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			var line = lines[0];
			AssertEquals(100m, line.TILV);
			AssertContains(AUAddInfo.Schema.ZA_REL_Hidden.Substring(3) + "=" + CMRRelatedTransaction.Yes.Code, line.AddInfos);
			AssertNotContains(UniversalExtensions.TILV4Warehouse + "=" + "100.00", line.AddInfos);
		}
	}
}
