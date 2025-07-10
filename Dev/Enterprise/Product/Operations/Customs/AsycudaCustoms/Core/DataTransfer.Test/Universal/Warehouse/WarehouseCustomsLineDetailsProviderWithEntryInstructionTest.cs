using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AsycudaCustoms.DataTransfer.Universal.Testing
{
	class WarehouseCustomsLineDetailsProviderWithEntryInstructionTest : TestCaseWithFactory
	{
		public void TestGetLineDetails()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "71", "00", "", "no bondedwarehouse for IMP", "IMP", group: "IM7", intoWarehouse: false, outOfWarehouse: false);
			helper.CreateRefCusProcedure(currentCountry, "A", "71", "50", "", "Inward", "IMP", group: "IM7", intoWarehouse: true, outOfWarehouse: false);
			helper.CreateRefCusProcedure(currentCountry, "A", "72", "00", "", "OutWard", "IMP", group: "IM7", intoWarehouse: false, outOfWarehouse: true);
			helper.CreateRefCusProcedure(currentCountry, "B", "71", "71", "", "OwnershipChange", "IMP", group: "IM7", intoWarehouse: true, outOfWarehouse: true);
			helper.CreateRefCusProcedure(currentCountry, "B", "10", "00", "", "no bondedwarehouse for EXP", "EXP", group: "EX1", intoWarehouse: false, outOfWarehouse: false);
			Factory.Save();
			var shipment = CreateShipment("IM7", "EX1", DataContextType.WarehouseOrder);
			var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProviderWithEntryInstruction(shipment);
			var lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			AssertWarehouseCustomsLineDetails(lines[0], "12345000000002", 2, "12345000000001", 1);
			AssertWarehouseCustomsLineDetails(lines[1], "12345000000005", 5, null, null);
		}

		static Shipment CreateShipment(ZString style1, ZString style2, DataContextType dataContextType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(dataContextType, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				CommercialInfo = new CommercialInfo
				{
					Name = "GROUPINV",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine
								{
									LineNo = 1,
									EntryInstructionLink = 1,
									EntryLineNumber = 2,
									EntryNumber = "12345000000002",
									PreviousEntryLineNumber = 1,
									PreviousEntryNumber = "12345000000001",
									Procedure = "7200",
									BondedWarehouseQuantity = 10m
								},
								new CommercialInvoiceLine
								{
									LineNo = 2,
									EntryInstructionLink = 2,
									EntryLineNumber = 5,
									EntryNumber = "12345000000005",
									PreviousEntryLineNumber = 4,
									PreviousEntryNumber = "12345000000004",
									Procedure = "7200",
									BondedWarehouseQuantity = 20m
								}
							})))
						.AdditionalSetup(x => x.SetAddInfoCollection(() => new List<AddInfo>()
						{
							new AddInfo() { Key = "Quantity", Value = "7" }
						}))
					})
				}
			};
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[]
			{
				new EntryInstruction
				{
					Style = style1,
					Link = 1
				},
				new EntryInstruction
				{
					Style = style2,
					Link = 2
				}
			}));
			return shipment;
		}

		static void AssertWarehouseCustomsLineDetails(IWarehouseCustomsLineDetails lineDetails, ZString entryNumber, ZShort entryLineNumber, ZString? previousEntryNumber, ZShort? previousEntryLineNumber)
		{
			AssertEquals("EntryNumber", entryNumber, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", entryLineNumber, lineDetails.EntryLineNumber);
			AssertEquals("PreviousEntryNumber", previousEntryNumber, lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", previousEntryLineNumber, lineDetails.PreviousEntryLineNumber);
		}
	}
}
