using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Testing
{
	sealed class WarehouseCustomsDetailsChangeOfRegimeTest : TestCaseWithFactory
	{
		public void TestIntoRegimeType()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader };
			var commercialInvoiceLine = new CommercialInvoiceLine { EntryInstructionLink = 1 };
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });
			var entryInstruction = new EntryInstruction { Link = 1 };
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction> { entryInstruction });

			var warehouseCustomsDetailsChangeOfRegime = new WarehouseCustomsDetailsChangeOfRegime(shipment);
			entryInstruction.Procedure = ImportMainProcedureCodeList.Codes._51;
			AssertEquals(CustomsRegime.InwardProcessing, warehouseCustomsDetailsChangeOfRegime.IntoRegimeType);

			entryInstruction.Procedure = "AA";
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime.IntoRegimeType);

			var shipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { new CommercialInvoiceLine { EntryInstructionLink = 1 } }))
					},
				},
			};
			var warehouseCustomsDetailsChangeOfRegime2 = new WarehouseCustomsDetailsChangeOfRegime(shipment2);
			AssertNull("Precondition", shipment2.EntryInstructionCollection);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime2.IntoRegimeType);
		}
	}
}
