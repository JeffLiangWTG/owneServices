using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DE.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsFallbackDetailWithEntryInstructionTest : TestCaseWithFactory
	{
		public void TestClone()
		{
			var source = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				IsExport = true,
				IsExWarehouse = true,
				SupplierAddress = WarehouseCustomsTestHelper.Supplier,
				ImporterAddress = WarehouseCustomsTestHelper.Importer,
				BuyerAddress = WarehouseCustomsTestHelper.Buyer,
				SellerAddress = WarehouseCustomsTestHelper.Seller,
				InvoiceLineAddInfosApplicableForInwardWarehousing = new List<string>
				{
					"Key1"
				},
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>
				{
					{ 71, "7100000" }
				},
				EntryInstructionEntryHeaderMap = new Dictionary<ZInt, List<EntryHeader>>
				{
					{ 1, new List<EntryHeader>
						{
							new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								Reference = "REF001"
							}
						}
					}
				},
				FallbackAddInfos = new Dictionary<ZString, AddInfo>
				{
					{ "Key1", AddInfo.New("GrossWeight", "32.29") }
				},
				CountryCode = Core.Constants.CountryCodes.Germany,
				LinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion,
				IncotermCode = "FOB",
				IncotermPlace = "Frankfurt",
				InvoiceDate = new ZDateTime(2023, 6, 21),
				InvoiceNumber = "INV123",
				PortOfFirstEUArrival = "DEHAM",
				PortOfLoading = "DEWIB",
				TransportMode = "SEA",
				ValuationCode = "21",
				supportingInfos = new List<CustomsSupportingInformation>
				{
					new CustomsSupportingInformation()
					{
						Type = new CodeDescriptionPair6Char() { Code = "C014" },
						DateOfExpiry = ZDateTime.BrettsBirthday,
						ReferenceNumber = "header-support1"
					}
				}
			};

			var target = (WarehouseCustomsFallbackDetailWithEntryInstruction)source.Clone();
			CombineAssertions(() =>
			{
				AssertEquals("IsExport", expected: true, target.IsExport);
				AssertEquals("IsExWarehouse", expected: true, target.IsExWarehouse);
				AssertEquals("SupplierAddress", "Supplier ORG", target.SupplierAddress.CompanyName);
				AssertEquals("ImporterAddress", "Importer ORG", target.ImporterAddress.CompanyName);
				AssertEquals("BuyerAddress", "Buyer ORG", target.BuyerAddress.CompanyName);
				AssertEquals("SellerAddress", "Seller ORG", target.SellerAddress.CompanyName);
				AssertEquals("InvoiceLineAddInfosApplicableForInwardWarehousing", "Key1", target.InvoiceLineAddInfosApplicableForInwardWarehousing.Single());
				AssertEquals("EntryInstructionProcedureMap.Key", 71, target.EntryInstructionProcedureMap.Single().Key);
				AssertEquals("EntryInstructionProcedureMap.Value", "7100000", target.EntryInstructionProcedureMap.Single().Value);
				AssertEquals("EntryInstructionEntryHeaderMap.Key", 1, target.EntryInstructionEntryHeaderMap.Single().Key);
				AssertEquals("EntryInstructionEntryHeaderMap.Value.Reference", "REF001", target.EntryInstructionEntryHeaderMap.Single().Value.Single().Reference);
				AssertEquals("FallbackAddInfos.Key", "Key1", target.FallbackAddInfos.Single().Key);
				AssertEquals("FallbackAddInfos.Value.Key", "GrossWeight", target.FallbackAddInfos.Single().Value.Key);
				AssertEquals("FallbackAddInfos.Value.Value", "32.29", target.FallbackAddInfos.Single().Value.Value);
				AssertEquals("CountryCode", Core.Constants.CountryCodes.Germany, target.CountryCode);
				AssertEquals("LinePriceCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, target.LinePriceCurrency);
				AssertEquals("IncotermCode", "FOB", target.IncotermCode);
				AssertEquals("IncotermPlace", "Frankfurt", target.IncotermPlace);
				AssertEquals("InvoiceDate", new ZDateTime(2023, 6, 21), target.InvoiceDate);
				AssertEquals("InvoiceNumber", "INV123", target.InvoiceNumber);
				AssertEquals("PortOfFirstEUArrival", "DEHAM", target.PortOfFirstEUArrival);
				AssertEquals("PortOfLoading", "DEWIB", target.PortOfLoading);
				AssertEquals("TransportMode", "SEA", target.TransportMode);
				AssertEquals("ValuationCode", "21", target.ValuationCode);
				AssertEquals("SupportingInfo", "header-support1", target.supportingInfos.Single().ReferenceNumber);
			});
		}

		public void TestCloneBaseProperties()
		{
			var source = new EU.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction()
			{
				IsExport = true,
				IsExWarehouse = true,
				SupplierAddress = WarehouseCustomsTestHelper.Supplier,
				InvoiceLineAddInfosApplicableForInwardWarehousing = new List<string>
				{
					"Key1"
				},
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>
				{
					{ 71, "7100000" }
				},
				EntryInstructionEntryHeaderMap = new Dictionary<ZInt, List<EntryHeader>>
				{
					{ 1, new List<EntryHeader>
						{
							new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								Reference = "REF001"
							}
						}
					}
				},
				FallbackAddInfos = new Dictionary<ZString, AddInfo>
				{
					{ "Key1", AddInfo.New("GrossWeight", "32.29") }
				},
				CountryCode = Core.Constants.CountryCodes.Germany,
				LinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion,
			};

			var result = WarehouseCustomsFallbackDetailWithEntryInstruction.CloneBaseProperties(source);
			CombineAssertions(() =>
			{
				AssertEquals("IsExport", expected: true, result.IsExport);
				AssertEquals("IsExWarehouse", expected: true, result.IsExWarehouse);
				AssertEquals("SupplierAddress", "Supplier ORG", result.SupplierAddress.CompanyName);
				AssertEquals("InvoiceLineAddInfosApplicableForInwardWarehousing", "Key1", result.InvoiceLineAddInfosApplicableForInwardWarehousing.Single());
				AssertEquals("EntryInstructionProcedureMap.Key", 71, result.EntryInstructionProcedureMap.Single().Key);
				AssertEquals("EntryInstructionProcedureMap.Value", "7100000", result.EntryInstructionProcedureMap.Single().Value);
				AssertEquals("EntryInstructionEntryHeaderMap.Key", 1, result.EntryInstructionEntryHeaderMap.Single().Key);
				AssertEquals("EntryInstructionEntryHeaderMap.Value.Reference", "REF001", result.EntryInstructionEntryHeaderMap.Single().Value.Single().Reference);
				AssertEquals("FallbackAddInfos.Key", "Key1", result.FallbackAddInfos.Single().Key);
				AssertEquals("FallbackAddInfos.Value.Key", "GrossWeight", result.FallbackAddInfos.Single().Value.Key);
				AssertEquals("FallbackAddInfos.Value.Value", "32.29", result.FallbackAddInfos.Single().Value.Value);
				AssertEquals("CountryCode", Core.Constants.CountryCodes.Germany, result.CountryCode);
				AssertEquals("LinePriceCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, result.LinePriceCurrency);
			});
		}
	}
}
