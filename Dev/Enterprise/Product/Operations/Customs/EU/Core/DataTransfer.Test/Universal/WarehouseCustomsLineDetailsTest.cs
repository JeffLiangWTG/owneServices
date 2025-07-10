using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using BondedWarehousingHelper = Enterprise.Customs.EU.Business.BondedWarehousingHelper;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestPreviousDocuments()
		{
			var lineDetails = CreateLineDetails(true, createPreviousDocs: true);
			var additionalAddInfos = lineDetails.AdditionalAddInfos.ToArray();
			AssertEquals("additionalAddInfos with previous docs", 8, additionalAddInfos.Length);
			AssertWarehouseCustomsLineAddInfo(additionalAddInfos[4], "PRE", "Type=IM*Reference=12345", ZString.Empty);
			AssertWarehouseCustomsLineAddInfo(additionalAddInfos[5], "PRE", "Type=IM*Reference=24680", ZString.Empty);
		}

		public void TestAuthorizations()
		{
			var lineDetails = CreateLineDetails(true);
			var additionalAddInfos = lineDetails.AdditionalAddInfos.ToArray();
			AssertEquals("additionalAddInfos", 6, additionalAddInfos.Length);
			AssertWarehouseCustomsLineAddInfo(additionalAddInfos[4], "AUT", "Code=CW1*Number=12345", ZString.Empty);
			AssertWarehouseCustomsLineAddInfo(additionalAddInfos[5], "AUT", "Code=CW2*Number=67890", ZString.Empty);
		}

		public void TestProperties_WhenInward()
		{
			var lineDetails = CreateLineDetails(true);
			CombineAssertions(() =>
			{
				AssertWarehouseCustomsLineDetails(lineDetails, "NZ", "DE", 10m, "BX", 50m, "DTNE", "PP1", "1020304050", 1000m, "12345000000023", 2, "LinePrice=66*LinePriceCurrency=EUR*CountryOfSupply=CN*ValuationCode=3", null, null, "IMF", "1071F61", null, null);
				var additionalAddInfos = lineDetails.AdditionalAddInfos.ToArray();
				AssertEquals("additionalAddInfos", 6, additionalAddInfos.Length);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[0], "CCT", "Amount=1100*ChargeType=CH1*Currency=AUD*IsDutiable=Y*IsGSTApplicable=*IsIncludedInITOT=*IsStatisticalValueApplicable=Y", ZString.Empty);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[1], "CCT", "Amount=2200*ChargeType=CH2*Currency=NZD*IsDutiable=*IsGSTApplicable=Y*IsIncludedInITOT=*IsStatisticalValueApplicable=", ZString.Empty);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[2], "SUP", "Code=SUP1*Order=1", ZString.Empty);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[3], "SUP", "Code=SUP2*Order=2", ZString.Empty);
			});
		}

		public void TestProperties_WhenOutward()
		{
			var lineDetails = CreateLineDetails(false);
			CombineAssertions(() =>
			{
				AssertWarehouseCustomsLineDetails(lineDetails, "NZ", "DE", 10m, "BX", 50m, "DTNE", "PP1", "1020304050", 1000m, "12345000000023", 2, null, "12345000000012", 3, "IMF", "4271C33", 5, "TestOrderNumber");
				var additionalAddInfos = lineDetails.AdditionalAddInfos.ToArray();
				AssertEquals("additionalAddInfos", 6, additionalAddInfos.Length);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[0], "CCT", "Amount=1100*ChargeType=CH1*Currency=AUD*IsDutiable=Y*IsGSTApplicable=*IsIncludedInITOT=*IsStatisticalValueApplicable=Y", ZString.Empty);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[1], "CCT", "Amount=2200*ChargeType=CH2*Currency=NZD*IsDutiable=*IsGSTApplicable=Y*IsIncludedInITOT=*IsStatisticalValueApplicable=", ZString.Empty);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[2], "SUP", "Code=SUP1*Order=1", ZString.Empty);
				AssertWarehouseCustomsLineAddInfo(additionalAddInfos[3], "SUP", "Code=SUP2*Order=2", ZString.Empty);
			});
		}

		public void TestCountryOfDestination_FallbackToGoodsDestination()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				GoodsDestination = "DE"
			};
			var lineDetails = new WarehouseCustomsLineDetails(Factory, new CommercialInvoiceLine(), new WarehouseCustomsFallbackDetailWithEntryInstruction(), shipment);
			AssertEquals("DE", lineDetails.CountryOfDestination);
		}

		public void TestDoNotMapLineGrossWeightAndUnitIfEmpty()
		{
			CombineAssertions(() =>
			{
				const string expectedAddInfo = "LinePrice=*LinePriceCurrency=*CountryOfSupply=*ValuationCode=";

				var customsLineDetails = new WarehouseCustomsLineDetails(
					Factory,
					new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance),
					new WarehouseCustomsFallbackDetailWithEntryInstruction(),
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance));
				AssertEquals("When Weight and WeightUnit are not filled in CommercialInvoiceLine, AddInfos", expectedAddInfo, customsLineDetails.AddInfos);

				customsLineDetails = new WarehouseCustomsLineDetails(
					Factory,
					new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance) { WeightUnit = new UnitOfWeight() { Code = null } },
					new WarehouseCustomsFallbackDetailWithEntryInstruction(),
					new Shipment(DefaultDataObjectWriterStrategy.TestInstance));
				AssertEquals("[EDGE-CASE]: When WeightUnit is filled in CommercialInvoiceLine with empty Code, AddInfos", expectedAddInfo, customsLineDetails.AddInfos);
			});
		}

		public void TestMapLineGrossWeightAndUnit()
		{
			const string expectedAddInfo = "LinePrice=*LinePriceCurrency=*CountryOfSupply=*ValuationCode=*LineGrossWeight=10.123*LineGrossWeightUnit=KG";

			var commercialInvoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Weight = 10.123m,
				WeightUnit = new UnitOfWeight() { Code = "KG" }
			};

			var customsLineDetails = new WarehouseCustomsLineDetails(
				Factory,
				commercialInvoiceLine,
				new WarehouseCustomsFallbackDetailWithEntryInstruction(),
				new Shipment(DefaultDataObjectWriterStrategy.TestInstance));
			AssertEquals("When Weight and WeightUnit are filled in CommercialInvoiceLine, AddInfos", expectedAddInfo, customsLineDetails.AddInfos);
		}

		WarehouseCustomsLineDetails CreateLineDetails(bool isInward, bool createPreviousDocs = false)
		{
			var procedure = isInward ? CreateInwardCusProcedure() : CreateOutwardCusProcedure();

			var invoiceLine = new CommercialInvoiceLine()
			{
				LineNo = 1,
				EntryLineNumber = 2,
				EntryNumber = "12345000000023",
				PreviousEntryNumber = "12345000000012",
				PreviousEntryLineNumber = 3,
				PrimaryPreference = "PP1",
				HarmonisedCode = "1020304050",
				CountryOfOrigin = new Country() { Code = "NZ" },
				Procedure = procedure.ZZ6_ProcedureCode + procedure.ZZ6_PreviousProcedureCode + procedure.ZZ6_Concession,
				LinePrice = 66m,
				CustomsValue = 1000m,
				CustomsQuantity = 10m,
				CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "BX" },
				CustomsSecondQuantity = 50m,
				CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "DTNE" },
				CustomsThirdQuantity = 20m,
				CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "BAG" },
				ValuationCode = new CodeDescriptionPair() { Code = "3" },
				BondedWHSOrderLineNumber = 5,
				BondedWHSOrderNumber = "TestOrderNumber",
				AddInfoCollection = new List<UniversalAddInfo>(new[]
				{
					new UniversalAddInfo() { Key = "ExciseProductCode", Value = "9902" },
					new UniversalAddInfo() { Key = EUAddInfoSchema.Constants.ZG_ValueAdjustmentCode.Substring(3), Value = "VIN23423" },
					new UniversalAddInfo() { Key = EUAddInfoSchema.Constants.ZG_StyleOfEntrySOE.Substring(3), Value = "ENG32425" },
					new UniversalAddInfo() { Key = EUAddInfoSchema.Constants.ZG_PrincipalsRepresentativeName.Substring(3), Value = "S" },
					new UniversalAddInfo() { Key = EUAddInfoSchema.Constants.ZG_CountryOfDestination.Substring(3), Value = "DE" },
					new UniversalAddInfo() { Key = BondedWarehousingHelper.Constants.CountryOfSupply, Value = Core.Constants.CountryCodes.China }
				}),
				CommercialChargeCollection = new List<CommercialCharge>(new[]
				{
					new CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = "CH1" },
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia },
						Amount = new ZDecimal(1100m),
						IsDutiable = ZBool.True,
						IsGSTApplicable = ZBool.False,
						IsNotIncludedInInvoice = ZBool.True,
						IsStatisticalValueApplicable = ZBool.True
					},
					new CommercialCharge()
					{
						ChargeType = new CodeDescriptionPair() { Code = "CH2" },
						Currency = new Currency() { Code = Core.Constants.CurrencyCodes.NewZealand },
						Amount = new ZDecimal(2200m),
						IsDutiable = ZBool.False,
						IsGSTApplicable = ZBool.True,
						IsNotIncludedInInvoice = ZBool.True,
						IsStatisticalValueApplicable = ZBool.False
					}
				}),
				CustomsReferenceCollection = new List<CustomsReference>
				{
					new CustomsReference
					{
						Type = new CodeDescriptionPair
						{
							Code = CusCodeDataTypeList.Codes.SupplementaryCode
						},
						SubType = new CodeDescriptionPair35Char
						{
							Code = "SUP1"
						},
						Order = 1
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair
						{
							Code = CusCodeDataTypeList.Codes.SupplementaryCode
						},
						SubType = new CodeDescriptionPair35Char
						{
							Code = "SUP2"
						},
						Order = 2
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair
						{
							Code = CusCodeDataTypeList.Codes.AdditionalProcedureCode
						},
						SubType = new CodeDescriptionPair35Char
						{
							Code = "APC1"
						}
					}
				},
				EntryInstructionLink = 1
			};

			if (createPreviousDocs)
			{
				invoiceLine.CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>
				{
					new CustomsSupportingInformation
					{
						Category = new CodeDescriptionPair { Code = "PRE" },
						Type = new CodeDescriptionPair6Char { Code = "IM" },
						ReferenceNumber = "12345",
					},
					new CustomsSupportingInformation
					{
						Category = new CodeDescriptionPair { Code = "PRE" },
						Type = new CodeDescriptionPair6Char { Code = "IM" },
						ReferenceNumber = "24680",
					},
				};
			}

			var fallbackDetail = new WarehouseCustomsFallbackDetailWithEntryInstruction
			{
				CountryCode = Core.Constants.CountryCodes.France,
				LinePriceCurrency = Core.Constants.CurrencyCodes.EuropeanUnion,
				EntryInstructionProcedureMap = new Dictionary<ZInt, ZString>
				{
					{ 1, procedure.ZZ6_ProcedureCode }
				}
			};

			var dataContext = DataContextFactory.New();
			var instruction = new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance);
			instruction.Link = 1;
			instruction.Style = "IM";
			instruction.SubStyle = new CodeDescriptionPair { Code = "F" };
			instruction.CustomsReferenceCollection = new List<CustomsReference>
				{
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = "AUT" },
						SubType = new CodeDescriptionPair35Char { Code = "CW1" },
						Reference = "12345",
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = "AUT" },
						SubType = new CodeDescriptionPair35Char { Code = "CW2" },
						Reference = "67890",
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = "AUT" },
						SubType = new CodeDescriptionPair35Char { Code = "XXX" },
						Reference = "99999",
					},
				};
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "MB123",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				MessageSubType = new CodeDescriptionPair { Code = "IM" },
				CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>(new []
								{
									invoiceLine,
								})))
						})
				},
			};
			shipment.SetEntryInstructionCollection(() => { return new List<EntryInstruction> { instruction }; });
			return new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail, shipment);
		}

		void AssertWarehouseCustomsLineAddInfo(IWarehouseCustomsLineAddInfo warehouseCustomsLineAddInfo, ZString type, ZString addInfoData, ZString nAddInfoData)
		{
			AssertEquals("Type", type, warehouseCustomsLineAddInfo.Type);
			AssertEquals("AddInfoData", addInfoData, warehouseCustomsLineAddInfo.AddInfoData);
			AssertEquals("NAddInfoData", nAddInfoData, warehouseCustomsLineAddInfo.NAddInfoData);
		}

		void AssertWarehouseCustomsLineDetails(WarehouseCustomsLineDetails lineDetails, ZString countryOfOrigin, ZString? countryOfDestination, ZDecimal customsQty, ZString customsUQ, ZDecimal customsSecondQty, ZString customsSecondUQ,
			ZString? primaryPreference, ZString tariff, ZDecimal valueForDuty, ZString entryNumber, ZShort entryLineNumber, ZString addInfos, ZString? previousEntryNumber, ZShort? previousEntryLineNumber,
			ZString? inwardStyle, ZString? inwardProcedure, ZInt? orderLineNo, ZString? orderNumber)
		{
			AssertEquals("CountryOfOrigin", countryOfOrigin, lineDetails.CountryOfOrigin.GetCodeAsUpperCase());
			AssertEquals("CountryOfDestination", countryOfDestination, lineDetails.CountryOfDestination);
			AssertEquals("CustomsQuantity", customsQty, lineDetails.CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", customsUQ, lineDetails.CustomsQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("CustomsSecondQuantity", customsSecondQty, lineDetails.CustomsSecondQuantity);
			AssertEquals("CustomsSecondQuantityUnit", customsSecondUQ, lineDetails.CustomsSecondQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("CustomsThirdQuantity", 20m, lineDetails.CustomsThirdQuantity);
			AssertEquals("CustomsThirdQuantityUnit", "BAG", lineDetails.CustomsThirdQuantityUnit.GetCodeAsUpperCase());
			AssertEquals("EntryNumber", entryNumber, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", entryLineNumber, lineDetails.EntryLineNumber);
			AssertEquals("PrimaryPreference", primaryPreference, lineDetails.PrimaryPreference);
			AssertEquals("Tariff", tariff, lineDetails.Tariff);
			AssertEquals("ValueForDuty", valueForDuty, lineDetails.ValueForDuty);
			AssertEquals("AddInfos", addInfos, lineDetails.AddInfos);
			AssertEquals("PreviousEntryNumber", previousEntryNumber, lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", previousEntryLineNumber, lineDetails.PreviousEntryLineNumber);
			AssertEquals("Style", inwardStyle, lineDetails.Style);
			AssertEquals("Procedure", inwardProcedure, lineDetails.Procedure);
			AssertEquals("OrderNumber", orderNumber, lineDetails.OrderNumber);
			AssertEquals("OrderNumber", orderLineNo, lineDetails.OrderLineNo);
		}

		RefCusProcedure CreateInwardCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "10", "71", "F61", "", "IMP", "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();
			return procedure;
		}

		RefCusProcedure CreateOutwardCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "42", "71", "C33", "", "IMP", "42P");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			Factory.Save();
			return procedure;
		}
	}
}
