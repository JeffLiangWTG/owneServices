using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal.Testing
{
	class WarehouseCustomsLineDetailsWithEntryInstructionTest : TestCaseWithFactory
	{
		public void TestGetThirdQuantityAsAddInfosForWarehousing()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance).AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>
							{
								new CommercialInvoiceLine
								{
									BondedWarehouseQuantity = 10m,
									CustomsThirdQuantity = 1m
								}
							}))
					}
				}
			};
			var contextMock = new Mock<IDataContextDataObject>();
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var lineDetails = shipment.GetWarehouseCustomsLineDetails(contextMock.Object);
			var line = lineDetails.First();
			AssertEquals(1m, line.CustomsThirdQuantity);
			AssertEquals(ZString.Empty, line.AddInfos);
			contextMock.VerifyAll();
		}

		public void TestProperties()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "71", "00", "", "no bondedwarehouse for IMP", "IMP", group: "IM7", intoWarehouse: false, outOfWarehouse: false);
			helper.CreateRefCusProcedure(currentCountry, "A", "71", "50", "", "Inward", "IMP", group: "IM7", intoWarehouse: true, outOfWarehouse: false);
			helper.CreateRefCusProcedure(currentCountry, "A", "72", "71", "000", "OutWard", "IMP", group: "IM7", intoWarehouse: false, outOfWarehouse: true);
			helper.CreateRefCusProcedure(currentCountry, "B", "71", "71", "", "OwnershipChange", "IMP", group: "IM7", intoWarehouse: true, outOfWarehouse: true);
			helper.CreateRefCusProcedure(currentCountry, "B", "10", "00", "", "no bondedwarehouse for EXP", "EXP", group: "EX1", intoWarehouse: false, outOfWarehouse: false);
			Factory.Save();
			var invoiceLine = CreateInvoiceLine();
			invoiceLine.EntryInstructionLink = 1;
			invoiceLine.Procedure = "7100";
			var fallbackDetail = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>(),
				EntryInstructionEntryHeaderMap = new Dictionary<ZInt, List<EntryHeader>>(),
				IsExport = false,
			};
			fallbackDetail.EntryInstructionProcedureMap.Add(1, "IM7");
			fallbackDetail.EntryInstructionProcedureMap.Add(2, "EX1");
			var lineDetails = new WarehouseCustomsLineDetailsWithEntryInstruction(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000002", 2, null, null);
			invoiceLine.Procedure = "7150";
			lineDetails = new WarehouseCustomsLineDetailsWithEntryInstruction(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000002", 2, null, null);
			invoiceLine.Procedure = "7271000";
			lineDetails = new WarehouseCustomsLineDetailsWithEntryInstruction(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000002", 2, "12345000000001", 1);
			invoiceLine.Procedure = "7171";
			lineDetails = new WarehouseCustomsLineDetailsWithEntryInstruction(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000002", 2, "12345000000001", 1);
			invoiceLine.Procedure = "1000";
			invoiceLine.EntryInstructionLink = 2;
			lineDetails = new WarehouseCustomsLineDetailsWithEntryInstruction(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000002", 2, null, null);
		}

		static CommercialInvoiceLine CreateInvoiceLine()
		{
			var invoiceLine = new CommercialInvoiceLine
			{
				LineNo = 1,
				EntryLineNumber = 2,
				EntryNumber = "12345000000002",
				PreviousEntryLineNumber = 1,
				PreviousEntryNumber = "12345000000001",
				BondedWarehouseQuantity = 10m,
			};
			return invoiceLine;
		}

		static void AssertWarehouseCustomsLineDetails(WarehouseCustomsLineDetails lineDetails, ZString entryNumber, ZShort entryLineNumber, ZString? previousEntryNumber, ZShort? previousEntryLineNumber)
		{
			AssertEquals("EntryNumber", entryNumber, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", entryLineNumber, lineDetails.EntryLineNumber);
			AssertEquals("PreviousEntryNumber", previousEntryNumber, lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", previousEntryLineNumber, lineDetails.PreviousEntryLineNumber);
		}
	}
}
