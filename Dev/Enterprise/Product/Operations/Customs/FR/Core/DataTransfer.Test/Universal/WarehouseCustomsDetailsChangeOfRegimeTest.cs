using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.FR.DataTransfer.Universal.Testing
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
			entryInstruction.Style = DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing;
			AssertEquals(CustomsRegime.InwardProcessing, warehouseCustomsDetailsChangeOfRegime.IntoRegimeType);

			entryInstruction.Style = "AA";
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime.IntoRegimeType);

			var shipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine> { new CommercialInvoiceLine { EntryInstructionLink = 1 } })),
					},
				},
			};
			var warehouseCustomsDetailsChangeOfRegime2 = new WarehouseCustomsDetailsChangeOfRegime(shipment2);
			AssertNull("Precondition", shipment2.EntryInstructionCollection);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime2.IntoRegimeType);
		}

		public void TestOutOfRegimeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader };
			var commercialInvoiceLine = new CommercialInvoiceLine { EntryInstructionLink = 1 };
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction> { new EntryInstruction { Link = 1, Style = "AA" } });

			var procedure1 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "BB", ZString.Empty, "description", "EXP");
			procedure1.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();
			commercialInvoiceLine.Procedure = "51BB";
			var warehouseCustomsDetailsChangeOfRegime1 = new WarehouseCustomsDetailsChangeOfRegime(shipment);
			AssertEquals(CustomsRegime.InwardProcessing, warehouseCustomsDetailsChangeOfRegime1.OutOfRegimeType);

			var procedure2 = helper.CreateRefCusProcedure(currentCountry, ZString.Empty, "AA", "DD", ZString.Empty, "description2", "EXP");
			procedure2.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.No;
			Factory.Save();
			commercialInvoiceLine.Procedure = "51DD";
			var warehouseCustomsDetailsChangeOfRegime2 = new WarehouseCustomsDetailsChangeOfRegime(shipment);
			AssertEquals(CustomsRegime.BondedWarehouse, warehouseCustomsDetailsChangeOfRegime2.OutOfRegimeType);
		}
	}
}
