using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using BaseWarehouseCustomsFallbackDetailWithEntryInstruction = Enterprise.Customs.DataTransfer.Universal.WarehouseCustomsFallbackDetailWithEntryInstruction;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	class WarehouseCustomsFallbackDetailWithEntryInstructionTest : TestCaseWithFactory
	{
		public void TestCloneFrom()
		{
			var baseFallbackDetail = new BaseWarehouseCustomsFallbackDetailWithEntryInstruction
			{
				IsExport = true,
				IsExWarehouse = true,
				SupplierAddress = new OrganizationAddress
				{
					Address1 = "NJG Office"
				},
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
				}
			};
			var fallbackDetail = WarehouseCustomsFallbackDetailWithEntryInstruction.CloneFrom(baseFallbackDetail);
			CombineAssertions(() =>
			{
				AssertEquals("IsExport", true, fallbackDetail.IsExport);
				AssertEquals("IsExWarehouse", true, fallbackDetail.IsExWarehouse);
				AssertEquals("SupplierAddress", "NJG Office", fallbackDetail.SupplierAddress.Address1);
				AssertEquals("InvoiceLineAddInfosApplicableForInwardWarehousing", "Key1", fallbackDetail.InvoiceLineAddInfosApplicableForInwardWarehousing.Single());
				AssertEquals("EntryInstructionProcedureMap.Key", 71, fallbackDetail.EntryInstructionProcedureMap.Single().Key);
				AssertEquals("EntryInstructionProcedureMap.Value", "7100000", fallbackDetail.EntryInstructionProcedureMap.Single().Value);
				AssertEquals("EntryInstructionEntryHeaderMap.Key", 1, fallbackDetail.EntryInstructionEntryHeaderMap.Single().Key);
				AssertEquals("EntryInstructionEntryHeaderMap.Value.Reference", "REF001", fallbackDetail.EntryInstructionEntryHeaderMap.Single().Value.Single().Reference);
				AssertEquals("FallbackAddInfos.Key", "Key1", fallbackDetail.FallbackAddInfos.Single().Key);
				AssertEquals("FallbackAddInfos.Value.Key", "GrossWeight", fallbackDetail.FallbackAddInfos.Single().Value.Key);
				AssertEquals("FallbackAddInfos.Value.Value", "32.29", fallbackDetail.FallbackAddInfos.Single().Value.Value);
			});
		}

		public void TestClone()
		{
			var source = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				IsExport = true,
				IsExWarehouse = true,
				SupplierAddress = new OrganizationAddress
				{
					Address1 = "NJG Office"
				},
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
				CountryCode = Core.Constants.CountryCodes.France,
				LinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion
			};
			var target = (WarehouseCustomsFallbackDetailWithEntryInstruction)source.Clone();
			CombineAssertions(() =>
			{
				AssertEquals("IsExport", true, target.IsExport);
				AssertEquals("IsExWarehouse", true, target.IsExWarehouse);
				AssertEquals("SupplierAddress", "NJG Office", target.SupplierAddress.Address1);
				AssertEquals("InvoiceLineAddInfosApplicableForInwardWarehousing", "Key1", target.InvoiceLineAddInfosApplicableForInwardWarehousing.Single());
				AssertEquals("EntryInstructionProcedureMap.Key", 71, target.EntryInstructionProcedureMap.Single().Key);
				AssertEquals("EntryInstructionProcedureMap.Value", "7100000", target.EntryInstructionProcedureMap.Single().Value);
				AssertEquals("EntryInstructionEntryHeaderMap.Key", 1, target.EntryInstructionEntryHeaderMap.Single().Key);
				AssertEquals("EntryInstructionEntryHeaderMap.Value.Reference", "REF001", target.EntryInstructionEntryHeaderMap.Single().Value.Single().Reference);
				AssertEquals("FallbackAddInfos.Key", "Key1", target.FallbackAddInfos.Single().Key);
				AssertEquals("FallbackAddInfos.Value.Key", "GrossWeight", target.FallbackAddInfos.Single().Value.Key);
				AssertEquals("FallbackAddInfos.Value.Value", "32.29", target.FallbackAddInfos.Single().Value.Value);
				AssertEquals("CountryCode", Core.Constants.CountryCodes.France, target.CountryCode);
				AssertEquals("LinePriceCurrency", Core.Constants.CurrencyCodes.EuropeanUnion, target.LinePriceCurrency);
			});
		}
	}
}
