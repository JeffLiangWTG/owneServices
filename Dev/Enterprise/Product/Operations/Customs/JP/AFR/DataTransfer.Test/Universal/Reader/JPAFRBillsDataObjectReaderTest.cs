using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	partial class JPAFRBillsDataObjectReaderTest
	{
		public void TestImportingJPAFRBillsData()
		{
			DGSubstanceTestHelper.Create("0000", "08", "IMO");
			DGSubstanceTestHelper.Create("0000", "10", "IMO");
			DGSubstanceTestHelper.Create("0000", "12", "IMO");
			DGSubstanceTestHelper.Create("0000", "14", "IMO");
			DGSubstanceTestHelper.Create("0000", "16", "IMO");
			DGSubstanceTestHelper.Create("0000", "18", "IMO");
			DGSubstanceTestHelper.Create("0000", "20", "IMO");

			var billDataObject = SetupJPAFRBills("HB2343");
			billDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[] {
				SetupOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				SetupOrganizationAddress2(nameof(DocAddressType.ConsigneeAddress)),
				GetNewAddressData_WUFSHIJNB(DocAddressType.NotifyParty),
				GetNewAddressData_CRAHOLSYD(DocAddressType.NotifyParty2)
			}));
			billDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[] {
				SetupContainer("CNT12321"),
				SetupContainer2("CNT96854")
			}));
			var header = Factory.New<JPAFRHeader>();
			var headerHelper = new JPManifestDataObjectReaderHelper(Factory);
			var reader = new JPAFRBillsDataObjectReader(billDataObject, logger, headerHelper, header);
			var billBO = reader.ReadIntoBusinessObject();

			AssertNotNull(billBO);

			#region Check Contents of header Business Object

			CombineAssertions(delegate
			{
				AssertJPAFRBillsContents(billBO, "HB2343");

				AssertEquals("JPB_JPH_Header", header.PK, billBO.JPB_JPH_Header);
				AssertContents(billBO.Consignor, addresOverride: ZBool.True, orgAddressPK: OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);
				AssertContents2(billBO.Consignee, addresOverride: ZBool.True, orgAddressPK: OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress);

				AssertJobDocAddressContentMatches_WUFSHIJNB(billBO.NotifyParty1);
				AssertJobDocAddressContentMatches_CRAHOLSYD(billBO.NotifyParty2);

				AssertEquals("Should fill the first UNDG on JPB_DG", "000008", billBO.Substance.DG_Code);
				AssertContainsExactElementsInAnyOrder("Should skip the first one and take other UNDGs.", new ZString[] { "000012", "000014", "000016", "000018", "000020" }, billBO.UNDGs.Select(x => x.SubstanceCode));

				AssertEquals(2, billBO.Containers.Count);
				AssertNotNull("Container CNT12321 should have been added", billBO.Containers.FirstOrDefault(x => x.JPC_ContainerNum == "CNT12321"));
				AssertNotNull("Container CNT96854 should have been added", billBO.Containers.FirstOrDefault(x => x.JPC_ContainerNum == "CNT96854"));

				AssertMultilineASCIIEquals("logger.Logs", @" 
Information - No matching JPAFRBills found, creating new JPAFRBills.
Information - Populating JPAFRBills...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Org. Code: INTHEMSYD; Company Name: In The Moment; Address Code: THEMOMENT; Address 1: Unit 12, Level 3; Address 2: 233 Here St; City: ThereVille]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Org. Code: TOAPOLOGISE; Company Name: Too Late To Apologise; Address Code: TOOLATE; Address 1: Unit 24, Level 10; Address 2: 455 There St; City: Big City]'.
Warning - Matching 'NotifyParty':- No match found for '[Org. Code: WUFSHIJNB; Company Name: WUFU SHIPPING LINE; Address 1: Level 2, Building G; Address 2: 34 Dock Lane; City: Johannesburg]'.
Warning - Matching 'NotifyParty2':- No match found for '[Org. Code: CRAHOLSYD; Company Name: CRACKERJACK HOLDINGS; Address 1: 1804 Fudrucker Way; City: BOTANY]'.
Information - No matching JPAFRContainer found, creating new JPAFRContainer.
Information - Populating JPAFRContainer...
Information - Successfully loaded matching Container Type.
Information - No matching JPAFRContainer found, creating new JPAFRContainer.
Information - Populating JPAFRContainer...
Information - Successfully loaded matching Container Type.
Information - Added Bill Of Lading HB2343 from UniversalShipment.".Trim(), logger.Logs);
			});

			#endregion
		}
	}

	partial class JPManifestDataObjectReaderTestHelper
	{
		#region Implementation

		protected Shipment SetupJPAFRBills(ZString? wayBillNumber, UNLOCO portOfOrigin, UNLOCO portOfDestination, ZString? tariff, ZLong? manifestQty, PackageType manifestUQ, Country goodsOrigin, ZDecimal? grossWeight, UnitOfWeight grossWeightUQ, ZDecimal? volume, UnitOfVolume volumeUQ, ZDecimal? freightValue, Currency freightValueCurrency, ZString? goodsDescription, ZString? marksAndNumbers, ZString? remarks, UNLOCO delivery)
		{
			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = wayBillNumber,
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
				PortOfOrigin = portOfOrigin,
				PortOfDestination = portOfDestination,
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialChargeCollection = new List<UniversalCustoms.CommercialCharge>(new[]
					{
						new UniversalCustoms.CommercialCharge()
						{
							ChargeType = new CodeDescriptionPair() { Code = CustomsChargeTypeList.Codes.OverseasFreight },
							Amount = freightValue,
							Currency = freightValueCurrency
						}
					})
				},
			};
			result.SetNoteCollection(() => new DataObjectList<Note>(new[]
				{
					new Note() { Description = AddInfoConstants.Bill.Remarks, NoteText = remarks }
				}));
			result.SetAddInfoCollection(() => new List<AddInfo>(new[]
				{
					new AddInfo() { Key = AddInfoConstants.Bill.PlaceOfDeliveryCode, Value = delivery.Code },
					new AddInfo() { Key = AddInfoConstants.Bill.PlaceOfDeliveryName, Value = delivery.Name }
				}));
			result.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						HarmonisedCode = tariff,
						PackQty = manifestQty,
						PackType = manifestUQ,
						Weight = grossWeight,
						WeightUnit = grossWeightUQ,
						Volume = volume,
						VolumeUnit = volumeUQ,
						GoodsDescription = goodsDescription,
						MarksAndNos = marksAndNumbers,
						CountryOfOrigin = goodsOrigin,
					}
				})
			{
				Content = CollectionContent.Complete
			});

			result.PackingLineCollection[0].SetUNDGCollection(() =>
			{
				return new List<UNDG>(new[]
				{
					null,
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000008", Standard = "IMO" },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000010", Standard = "TST" },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000012", Standard = "IMO" },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000014", Standard = "IMO" },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000016", Standard = "IMO" },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000018", Standard = "IMO" },
					new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { UNDGCode = "000020", Standard = "IMO" },
				});
			});

			return result;
		}

		protected Shipment SetupJPAFRBills(ZString? wayBillNumber)
		{
			return SetupJPAFRBills(wayBillNumber, new UNLOCO() { Code = SeaForeignPort2.RL_Code }, new UNLOCO() { Code = SeaLocalPort3.RL_Code },
				"102030", 140, new PackageType() { Code = PackageTypeList.Codes.Basket }, new Country() { Code = Core.Constants.CountryCodes.Australia },
				1500.50m, new UnitOfWeight() { Code = WeightUnitCodeList.Codes.Kilogram },
				10.55m, new UnitOfVolume() { Code = VolumeUnitCodeList.Codes.CubicMeter }, 20500.50m, new Currency() { Code = Core.Constants.CurrencyCodes.Japan },
				"AWESOME GOODS", "WHAT ARE THOSE MARKS?", "YEAH!", new UNLOCO() { Code = SeaLocalPort1.RL_Code });
		}

		protected void AssertJPAFRBillsContents(JPAFRBills billBO, ZString wayBillNumber)
		{
			AssertJPAFRBillsContents(billBO, wayBillNumber, SeaForeignPort2.RL_Code, SeaLocalPort3.RL_Code,
				"102030", 140, PackageTypeList.Codes.Basket, Core.Constants.CountryCodes.Australia,
				1500.50m, WeightUnitCodeList.Codes.Kilogram,
				10.55m, VolumeUnitCodeList.Codes.CubicMeter, 20500.50m, Core.Constants.CurrencyCodes.Japan,
				"AWESOME GOODS", "WHAT ARE THOSE MARKS?", "YEAH!", SeaLocalPort1.RL_Code);
		}

		protected void AssertJPAFRBillsContents(JPAFRBills billBO, ZString wayBillNumber, ZString origin, ZString destination, ZString tariff, ZInt manifestQty, ZString manifestUQ, ZString goodsOrigin, ZDecimal grossWeight, ZString grossWeightUQ, ZDecimal volume, ZString volumeUQ, ZDecimal freightValue, ZString freightValueCurrency, ZString goodsDescription, ZString marksAndNumbers, ZString remarks, ZString delivery)
		{
			AssertEquals("billBO.JPB_BillNumber", wayBillNumber, billBO.JPB_BillNumber);
			AssertEquals("billBO.JPB_RL_NKOrigin", origin, billBO.JPB_RL_NKOrigin);
			AssertEquals("billBO.JPB_RL_NKFinalDestination", destination, billBO.JPB_RL_NKFinalDestination);
			AssertEquals("billBO.JPB_Tariff", tariff, billBO.JPB_Tariff);
			AssertEquals("billBO.JPB_ManifestQty", manifestQty, billBO.JPB_ManifestQty);
			AssertEquals("billBO.JPB_ManifestUQ", manifestUQ, billBO.JPB_ManifestUQ);
			AssertEquals("billBO.JPB_RN_NKGoodsOrigin", goodsOrigin, billBO.JPB_RN_NKGoodsOrigin);
			AssertEquals("billBO.JPB_GrossWeight", grossWeight, billBO.JPB_GrossWeight);
			AssertEquals("billBO.JPB_GrossWeightUQ", grossWeightUQ, billBO.JPB_GrossWeightUQ);
			AssertEquals("billBO.JPB_Volume", volume, billBO.JPB_Volume);
			AssertEquals("billBO.JPB_VolumeUQ", volumeUQ, billBO.JPB_VolumeUQ);
			AssertEquals("billBO.JPB_FreightValue", freightValue, billBO.JPB_FreightValue);
			AssertEquals("billBO.JPB_RX_NKFreightValueCurrency", freightValueCurrency, billBO.JPB_RX_NKFreightValueCurrency);
			AssertEquals("billBO.JPB_GoodsDescription", goodsDescription, billBO.JPB_GoodsDescription);
			AssertEquals("billBO.JPB_MarksAndNumbers", marksAndNumbers, billBO.JPB_MarksAndNumbers);
			AssertEquals("billBO.JPB_Remarks", remarks, billBO.JPB_Remarks);
			AssertEquals("billBO.JPB_RL_NKDelivery", delivery, billBO.JPB_RL_NKDelivery);
		}

		#region AFR InBondDetails Methods

		protected Shipment SetupAFRInBondDetails(Shipment shipment)
		{
			return SetupAFRInBondDetails(shipment, 1500.50m, Currency.New(USD), TemporaryLandingReasonCodeList.Codes.TranshippingCargoToOtherOutboundVesselAircraftInvolvingTransportation, 3, new ZDateTime(2013, 10, 3), new ZDateTime(2013, 10, 6), TransportModeList.Codes.Barge, new CodeDescriptionPair() { Code = "D4454", Description = "BONDED ARE 1" });
		}

		protected Shipment SetupAFRInBondDetails2(Shipment shipment)
		{
			return SetupAFRInBondDetails(shipment, 865m, Currency.New(JPY), TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers, 2, new ZDateTime(2013, 9, 8), new ZDateTime(2013, 9, 10), TransportModeList.Codes.RailExpress, new CodeDescriptionPair() { Code = "G3232", Description = "BONDED AREA 2" });
		}

		protected Shipment SetupAFRInBondDetails(Shipment shipment, ZDecimal? goodsValue, Currency goodsValueCurrency, ZString? temporaryLandingReason, ZInt? temporaryLandingDuration, ZDateTime? eSDT, ZDateTime? eFDT, ZString? transportMode, CodeDescriptionPair arrivalBondedArea)
		{
			shipment.GoodsValue = goodsValue;
			shipment.GoodsValueCurrency = goodsValueCurrency;
			if (temporaryLandingReason.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentReasonCode, Value = temporaryLandingReason }));
			}
			if (temporaryLandingDuration.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentDuration, Value = temporaryLandingDuration.Value.ToString() }));
			}
			if (eSDT.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentEstimatedStartDate, Value = eSDT.Value.ToISO8601String() }));
			}
			if (eFDT != null)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentEstimatedFinishDate, Value = eFDT.Value.ToISO8601String() }));
			}
			if (transportMode.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentTransportMode, Value = transportMode }));
			}
			if (arrivalBondedArea != null)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentArrivalPlaceCode, Value = arrivalBondedArea.Code }));
				shipment.AddInfoCollection?.Add(new AddInfo() { Key = AddInfoConstants.Bill.TranshipmentArrivalPlaceName, Value = arrivalBondedArea.Description });
			}
			return shipment;
		}

		protected void AssertContents(JPAFRInBondDetails inBondDetailsBO)
		{
			AssertContents(inBondDetailsBO, 1500.50m, USD.RX_Code, TemporaryLandingReasonCodeList.Codes.TranshippingCargoToOtherOutboundVesselAircraftInvolvingTransportation, 3, new ZDateTime(2013, 10, 3), new ZDateTime(2013, 10, 6), TransportModeList.Codes.Barge, "D4454");
		}

		protected void AssertContents2(JPAFRInBondDetails inBondDetailsBO)
		{
			AssertContents(inBondDetailsBO, 865m, JPY.RX_Code, TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers, 2, new ZDateTime(2013, 9, 8), new ZDateTime(2013, 9, 10), TransportModeList.Codes.RailExpress, "G3232");
		}

		protected void AssertContents(JPAFRInBondDetails inBondDetailsBO, ZDecimal goodsValue, ZString goodsValueCurrency, ZString temporaryLandingReason, ZInt temporaryLandingDuration, ZDateTime eSDT, ZDateTime eFDT, ZString transportMode, ZString arrivalBondedAreaCode)
		{
			AssertEquals("inBondDetailsBO.JPI_GoodsValue", goodsValue, inBondDetailsBO.JPI_GoodsValue);
			AssertEquals("inBondDetailsBO.JPI_RX_NKGoodsValueCurrency", goodsValueCurrency, inBondDetailsBO.JPI_RX_NKGoodsValueCurrency);
			AssertEquals("inBondDetailsBO.JPI_TemporaryLandingReason", temporaryLandingReason, inBondDetailsBO.JPI_TemporaryLandingReason);
			AssertEquals("inBondDetailsBO.JPI_TemporaryLandingDuration", temporaryLandingDuration, inBondDetailsBO.JPI_TemporaryLandingDuration);
			AssertEquals("inBondDetailsBO.JPI_ESDT", eSDT, inBondDetailsBO.JPI_ESDT);
			AssertEquals("inBondDetailsBO.JPI_EFDT", eFDT, inBondDetailsBO.JPI_EFDT);
			AssertEquals("inBondDetailsBO.JPI_TransportMode", transportMode, inBondDetailsBO.JPI_TransportMode);
			AssertEquals("inBondDetailsBO.JPI_ArrivalBondedAreaCode", arrivalBondedAreaCode, inBondDetailsBO.JPI_ArrivalBondedAreaCode);
		}

		#endregion

		#region AFR Other Law Methods

		protected Shipment SetupOtherRelevantLawCodes(Shipment shipment, ZString? law1, ZString? law2, ZString? law3, ZString? law4, ZString? law5)
		{
			if (law1.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.OtherRelevantLawCode1, Value = law1 }));
			}
			if (law2.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.OtherRelevantLawCode2, Value = law2 }));
			}
			if (law3.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.OtherRelevantLawCode3, Value = law3 }));
			}
			if (law4.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.OtherRelevantLawCode4, Value = law4 }));
			}
			if (law5.HasValue)
			{
				shipment.SetAddInfoCollection(() => shipment.AddInfoCollection.AddSafe(new AddInfo() { Key = AddInfoConstants.Bill.OtherRelevantLawCode5, Value = law5 }));
			}
			return shipment;
		}

		protected void AssertContains(ZString law, CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw> collection)
		{
			AssertNotNull("Should contain law: " + law, collection.FirstOrDefault(x => x.CY_Data == law));
		}

		#endregion

		#endregion
	}
}
