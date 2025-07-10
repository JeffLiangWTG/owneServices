using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Client.UPE.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Client.UPE.Business.SGAccess.Testing
{
	class SGDecisionSupporterTest : TestCaseWithFactory
	{
		public void TestShouldCycleNumberBeAutoNominated()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignee = GetUPEConsignee;

			//Alternate Broker
			var relatedOrg = Factory.NewWithValidTestData<UPEOrgHeader>();
			SetShipmentTransportMode(shipment, Level1DataFileImporterForSGAccess.Constants.TransportMode.Road);
			consignee.AddRelatedParty(relatedOrg.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Level1DataFileImporterForSGAccess.Constants.TransportMode.Road, ZString.Empty, GlbCompany.CurrentCompany);
			Assert(decisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee));
			consignee.AddRelatedParty(relatedOrg.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Level1DataFileImporterForSGAccess.Constants.TransportMode.Road, ZString.Empty, GlbCompany.CurrentCompany);
			Assert(decisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee));

			SetShipmentTransportMode(shipment, Level1DataFileImporterForSGAccess.Constants.TransportMode.Air);
			consignee.AddRelatedParty(relatedOrg.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, ZString.Empty, GlbCompany.CurrentCompany);
			Assert(decisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee));
			consignee.AddRelatedParty(relatedOrg.PK, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Level1DataFileImporterForSGAccess.Constants.TransportMode.Air, ZString.Empty, GlbCompany.CurrentCompany);
			Assert(decisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee));

			//FTZ
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organisationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				Postcode = "50000",
				Country = new Country { Code = Core.Constants.CountryCodes.Singapore },
			};
			shipment.OrganizationAddressCollection.Add(organisationAddress);
			Assert(decisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee));

			//Direct
			var directDelivery = GetUPECustomsCode(UPEOrgCusCode.SingaporeCodeTypes.DirectDelivery);
			consignee.CustomsCodes.Add(directDelivery);
			Assert(decisionSupporter.ShouldCycleNumberBeAutoNominated(shipment, consignee));
		}

		[ExpectNoExceptions()]
		public void TestFreeTradeZoneHasNoExceptionsWithInvalidRegistryStructure()
		{
			UPEDataRegistry.Instance.StopPostcodeRangesForSGFreeTradeZones = new string[] { "Invalid structure to fail" };
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 399);
			var organisationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				Postcode = "50000",
				Country = new Country() { Code = Core.Constants.CountryCodes.Singapore },
			};
			Assert("Valid FTZ shipment with invalid registry setup", !decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestLowValueFreeTradeZone()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 399);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organisationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				Postcode = "50000",
				Country = new Country() { Code = Core.Constants.CountryCodes.Singapore },
			};
			shipment.OrganizationAddressCollection.Add(organisationAddress);
			Assert("Low Value Shipment in FTZ postcode range", decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is within the Free Trade Zone", decisionSupporter.DecisionReason);
			organisationAddress.Country.Code = Core.Constants.CountryCodes.Australia;
			Assert("Low Value Shipment postcode in valid range but country AU", !decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			organisationAddress.Country.Code = Core.Constants.CountryCodes.Singapore;
			organisationAddress.Postcode = "61000";
			Assert("Low Value Shipment postcode not in FTZ for SG", !decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			organisationAddress.Postcode = "61000A";
			Assert("Low Value Shipment invalid postcode for SG", !decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			organisationAddress.Postcode = "81002";
			shipment.CommercialInfo.CommercialChargeCollection.Remove(commercialCharge);
			Assert("Independant of value for FTZ", decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is within the Free Trade Zone", decisionSupporter.DecisionReason);
			shipment.CommercialInfo.CommercialChargeCollection.Add(commercialCharge);
			shipment.OrganizationAddressCollection.Remove(organisationAddress);
			Assert("Low Value but no consignee to determine postcode", !decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			shipment.OrganizationAddressCollection.Add(organisationAddress);
			organisationAddress.Country = null;
			Assert("Low Value with FTZ postcode but no country", !decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestLowValueDutiableItems()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			var dutyEntryInstruction = new EntryInstruction()
			{
				AddInfoCollection = new List<AddInfo>(1)
			};
			var dutyAmountAddInfo = AddInfo.New(Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.BillCountry.DutyAmount, "0.01");
			shipment.EntryInstructionCollection.Add(dutyEntryInstruction);
			dutyEntryInstruction.AddInfoCollection.Add(dutyAmountAddInfo);
			SetShipmentValue(shipment, 399);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contains dutiable goods", decisionSupporter.DecisionReason);
			dutyAmountAddInfo.Value = ZString.Empty;
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestHighValueMajorExporterScheme()
		{
			var consignee = GetUPEConsignee;
			var majorExporter = GetUPECustomsCode(UPEOrgCusCode.SingaporeCodeTypes.PartyStatusType);
			consignee.CustomsCodes.Add(majorExporter);
			majorExporter.OK_CustomsRegNo = YesNoList.Codes.Yes;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 401);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = consignee.OH_Code,
			};
			shipment.OrganizationAddressCollection.Add(organizationAddress);

			Assert(decisionSupporter.IsTradeNet(shipment, consignee, Factory));
			AssertEquals("Shipment is high value and consignee has the major exporter indicator", decisionSupporter.DecisionReason);
		}

		public void TestLoeValueMajorExporterScheme()
		{
			var consignee = GetUPEConsignee;
			var majorExporter = GetUPECustomsCode(UPEOrgCusCode.SingaporeCodeTypes.PartyStatusType);
			consignee.CustomsCodes.Add(majorExporter);
			majorExporter.OK_CustomsRegNo = YesNoList.Codes.Yes;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 399);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = consignee.OH_Code,
			};
			shipment.OrganizationAddressCollection.Add(organizationAddress);
			Assert(!decisionSupporter.IsTradeNet(shipment, consignee, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestHighValueInterbankGiro()
		{
			var consignee = GetUPEConsignee;
			var interbankGIRO = GetUPECustomsCode(UPEOrgCusCode.SingaporeCodeTypes.InterbankGIRO);
			consignee.CustomsCodes.Add(interbankGIRO);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 401);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = consignee.OH_Code,
			};
			shipment.OrganizationAddressCollection.Add(organizationAddress);
			Assert(decisionSupporter.IsTradeNet(shipment, consignee, Factory));
			AssertEquals("Shipment is high value and consignee has the Interbank Giro indicator", decisionSupporter.DecisionReason);
			commercialCharge.Amount = 399;
		}

		public void TestLowValueInterbankGiro()
		{
			var consignee = GetUPEConsignee;
			var interbankGIRO = GetUPECustomsCode(UPEOrgCusCode.SingaporeCodeTypes.InterbankGIRO);
			consignee.CustomsCodes.Add(interbankGIRO);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 399);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = consignee.OH_Code,
			};
			shipment.OrganizationAddressCollection.Add(organizationAddress);
			Assert(!decisionSupporter.IsTradeNet(shipment, consignee, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestHighValueFreeTradeZone()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 401);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				Postcode = "50000",
				Country = new Country() { Code = Core.Constants.CountryCodes.Singapore },
			};
			shipment.OrganizationAddressCollection.Add(organizationAddress);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is within the Free Trade Zone", decisionSupporter.DecisionReason);
			shipment.OrganizationAddressCollection[0].Country.Code = Core.Constants.CurrencyCodes.Australia;
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
			shipment.OrganizationAddressCollection[0].Country.Code = Core.Constants.CurrencyCodes.Singapore;
			shipment.OrganizationAddressCollection[0].Postcode = "61000";
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
			shipment.OrganizationAddressCollection[0].Postcode = "61000A";
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
			shipment.CommercialInfo.CommercialChargeCollection.Add(commercialCharge);
			shipment.OrganizationAddressCollection.Remove(organizationAddress);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
			shipment.OrganizationAddressCollection.Add(organizationAddress);
			organizationAddress.Country = null;
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
		}

		public void TestHighAndLowValueDirectDelivery()
		{
			var consignee = GetUPEConsignee;
			var directDelivery = GetUPECustomsCode(UPEOrgCusCode.SingaporeCodeTypes.DirectDelivery);
			consignee.CustomsCodes.Add(directDelivery);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				OrganizationCode = consignee.OH_Code,
			};
			shipment.OrganizationAddressCollection.Add(organizationAddress);

			var commercialCharge = SetShipmentValue(shipment, 399);
			Assert(decisionSupporter.IsTradeNet(shipment, consignee, Factory));
			AssertEquals("Shipment consignee has the Direct Delivery indicator", decisionSupporter.DecisionReason);
			commercialCharge.Amount = 401;
			Assert(decisionSupporter.IsTradeNet(shipment, consignee, Factory));
			AssertEquals("Shipment consignee has the Direct Delivery indicator", decisionSupporter.DecisionReason);
		}

		public void TestHighAndLowValueDutiableGoods()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			var dutyEntryInstruction = new EntryInstruction()
			{
				AddInfoCollection = new List<AddInfo>(1)
			};
			var dutyAmountAddInfo = AddInfo.New(Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.BillCountry.DutyAmount, "0.01");
			shipment.EntryInstructionCollection.Add(dutyEntryInstruction);
			dutyEntryInstruction.AddInfoCollection.Add(dutyAmountAddInfo);
			SetShipmentValue(shipment, 399);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contains dutiable goods", decisionSupporter.DecisionReason);
			SetShipmentValue(shipment, 401);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contains dutiable goods", decisionSupporter.DecisionReason);
			dutyAmountAddInfo.Value = ZString.Empty;
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
		}

		public void TestControlledItem()
		{
			var controlledGoodTypeAddInfo = AddInfo.New(Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.PackedItem.GoodsType,
				Level1DataFileImporterForSGAccess.Constants.GoodsType.ControlledGoods);
			var normalGoodTypeAddInfo = AddInfo.New(Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.PackedItem.GoodsType,
				Level1DataFileImporterForSGAccess.Constants.GoodsType.NormalGoods);

			var controlledPackingLineDetailAddInfoItem = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.PackedItem.PackingItemAddInfoType },
				AddInfoCollection = new List<AddInfo>() { controlledGoodTypeAddInfo }
			};
			var normalPackingLineDetailAddInfoItem = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.PackedItem.PackingItemAddInfoType },
				AddInfoCollection = new List<AddInfo>() { normalGoodTypeAddInfo }
			};

			var controlledPackingLineDetailAddInfoCollection = new List<AddInfoGroup>();
			controlledPackingLineDetailAddInfoCollection.Add(controlledPackingLineDetailAddInfoItem);
			var controlledPackingLineDetail = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			controlledPackingLineDetail.SetAddInfoGroupCollection(() => controlledPackingLineDetailAddInfoCollection);
			var controlledPackingLineDetailsCollection = new List<PackingLine>();
			controlledPackingLineDetailsCollection.Add(controlledPackingLineDetail);
			var controlledPackingLineHeader = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			controlledPackingLineHeader.SetPackingLineCollection(() => controlledPackingLineDetailsCollection);

			var normalPackingLineDetailAddInfoCollection = new List<AddInfoGroup>();
			normalPackingLineDetailAddInfoCollection.Add(normalPackingLineDetailAddInfoItem);
			var normalPackingLineDetail = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			normalPackingLineDetail.SetAddInfoGroupCollection(() => normalPackingLineDetailAddInfoCollection);
			var normalPackingLineDetailsCollection = new List<PackingLine>();
			normalPackingLineDetailsCollection.Add(normalPackingLineDetail);
			var normalPackingLineHeader = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			normalPackingLineHeader.SetPackingLineCollection(() => normalPackingLineDetailsCollection);

			var packingLineCollection = new DataObjectList<PackingLine>();
			packingLineCollection.Add(controlledPackingLineHeader);
			packingLineCollection.Add(normalPackingLineHeader);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => packingLineCollection);

			var commercialCharge = SetShipmentValue(shipment, 401);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contained a pack line with controlled goods", decisionSupporter.DecisionReason);
			commercialCharge.Amount = 399;
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contained a pack line with controlled goods", decisionSupporter.DecisionReason);
			controlledGoodTypeAddInfo.Value = Level1DataFileImporterForSGAccess.Constants.GoodsType.DutiableGoods;
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestControlledItemStopWord()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGGoodsDescription = new string[] { "UPS Stop" };

			var normalPackingLineHeader = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			normalPackingLineHeader.GoodsDescription = "Lots of Goods";
			var stopPackingLineHeader = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			stopPackingLineHeader.GoodsDescription = "UPS Stop";
			var packingLineCollection = new DataObjectList<PackingLine>();
			packingLineCollection.Add(stopPackingLineHeader);
			packingLineCollection.Add(normalPackingLineHeader);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetPackingLineCollection(() => packingLineCollection);

			var commercialCharge = SetShipmentValue(shipment, 401);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contains a pack line with goods description details matching a stop word", decisionSupporter.DecisionReason);
			commercialCharge.Amount = 399;
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment contains a pack line with goods description details matching a stop word", decisionSupporter.DecisionReason);
		}

		public void TestTranshipmentFilterTrue()
		{
			UPEDataRegistry.Instance.FilterSGTranshipments = true;

			Assert(!decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "USLAX", "AUSYD"));
			AssertEquals("Shipment is a transhipment", decisionSupporter.DecisionReason);
			Assert("Import Bill", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "USLAX", "SGSIN"));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			Assert("Export Bill", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "SGLAK", "AUSYD"));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			Assert("Unable to accurately determine transhipment", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "SGLAK", ZString.Empty));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			Assert("Transhipment", !decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "MNYUF", "AUSYD"));
			AssertEquals("Shipment is a transhipment", decisionSupporter.DecisionReason);
			Assert("Unable to accurately determine transhipment", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "AUSYD"));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestTranshipmentFilterFalse()
		{
			UPEDataRegistry.Instance.FilterSGTranshipments = false;
			Assert(decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "AUSYD", "USLAX"));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			Assert(decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, "USLAX", "AUSYD"));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestConsigneeNameStopWord()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGConsigneeName = new string[] { "Stop Consignee Name" };

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				CompanyName = "ABC Stop Consignee Name DEF"
			};
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });

			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			orgAddress.CompanyName = "THIS IS NOT Tradenet";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestConsigneeAddressStopWord()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAddress = new string[] { "Stop Consignee Address" };

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
				Address1 = "Tower Stop Consignee Address floor",
				Address2 = ZString.Empty
			};
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });

			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			orgAddress.Address1 = "THIS IS NOT Tradenet";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			orgAddress.Address2 = "Second Stop Consignee Address location";
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			orgAddress.Address2 = "THIS IS NOT Tradenet";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestConsigneeAccountStopNumber()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAccountNum = new string[] { "STPNUM123" };

			var registrationNumber = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber },
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Singapore },
				Value = "STPNUM123"
			};
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress),
			};
			orgAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { registrationNumber });
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });

			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			registrationNumber.Value = "VALID123";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestConsignorNameStopWord()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGConsignorName = new string[] { "Stop Consignor Name" };

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress),
				CompanyName = "ABC Stop Consignor Name DEF"
			};
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });

			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			orgAddress.CompanyName = "THIS IS NOT Tradenet";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestConsignorAddressStopWord()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGConsignorAddress = new string[] { "Stop Consignor Address" };

			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress),
				Address1 = "Tower Stop Consignor Address floor",
			};
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });

			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			orgAddress.Address1 = "THIS IS NOT Tradenet";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			orgAddress.Address2 = "Second Stop Consignor Address PREMISE";
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			orgAddress.Address2 = "THIS IS NOT Tradenet";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestConsignorAccountStopNumber()
		{
			UPEDataRegistry.Instance.StopPhrasesForSGConsignorAccountNum = new string[] { "STPNUM456" };

			var registrationNumber = new RegistrationNumber
			{
				Type = new RegistrationNumberType { Code = UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber },
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Singapore },
				Value = "STPNUM456"
			};
			var orgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress),
			};
			orgAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber> { registrationNumber });
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgAddress });

			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment consignee or consignor details contained a stop word", decisionSupporter.DecisionReason);
			registrationNumber.Value = "VALID456";
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestBillsWithCustomsMessagingNotImported()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_BillNumber = MasterBillNumber;
			header.AMA_Voyage = "QF01";
			header.AMA_ManifestType = "MGI";

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "3824ARFY9JH";
			var pack1 = bill1.Packs.AddNew();
			var packedItem1 = pack1.PackedItem;
			packedItem1.API_MessageStatus = Customs.ASYCUDA.Business.MessageStatusCodeList.Codes.Accepted;
			var accessPermit = packedItem1.CustomsEntryNumbers.AddNew();
			accessPermit.CE_EntryType = Customs.ASYCUDA.Business.Constants.CustomsEntryType.ACCESSPermit;
			accessPermit.CE_EntryNum = "AHHHHHHHHHHH";

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "3947808NNSR";
			var pack2 = bill2.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			Factory.Save();

			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = true;
			UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1Loads = true;
			UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFound = false;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			shipment.SetAddInfoCollection(() => GetMatchingReferenceAddInfo("MATCHINGREFERENCE1"));
			Assert(!decisionSupporter.ImportThisShipment(Factory, MasterBillNumber, "QF01", "3824ARFY9JH", ZString.Empty, ZString.Empty));
			AssertEquals("Shipment already exists and has been submitted to Customs", decisionSupporter.DecisionReason);

			shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3947808NNSR" };
			shipment.SetAddInfoCollection(() => GetMatchingReferenceAddInfo("MATCHINGREFERENCE2"));
			Assert(decisionSupporter.ImportThisShipment(Factory, MasterBillNumber, "QF01", "3947808NNSR", ZString.Empty, ZString.Empty));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestBillUpdatesNotAllowedWhenRegistryToAllowIsFalseAndSetToHousebill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			header.MasterBill.ABL_BillNumber = MasterBillNumber;
			header.AMA_Voyage = "QF03";

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "3824ARFY9JH";
			Factory.Save();

			UPEDataRegistry.Instance.AllowMultipleLevel1LoadsForMasterBill = true;
			UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1Loads = false;
			UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFound = false;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			Assert(!decisionSupporter.ImportThisShipment(Factory, MasterBillNumber, "QF03", "3824ARFY9JH", ZString.Empty, ZString.Empty));
			AssertEquals("Shipment already exists and registry to allow bill updates is disabled", decisionSupporter.DecisionReason);

			shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3947808NNSR" };
			Assert(decisionSupporter.ImportThisShipment(Factory, MasterBillNumber, "QF03", "3947808NNSR", ZString.Empty, ZString.Empty));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestLowValueNormalGoods()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 399);

			Assert(decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			Assert(!decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
		}

		public void TestHighValueNormalGoods()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var commercialCharge = SetShipmentValue(shipment, 401);

			Assert(decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
			AssertEquals(ZString.Empty, decisionSupporter.DecisionReason);
			Assert(decisionSupporter.IsTradeNet(shipment, null, Factory));
			AssertEquals("Shipment is high value normal goods", decisionSupporter.DecisionReason);
		}

		public void TestHousebillForBillOutsideRecycleRegistryPeriod()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-2);
			Factory.Save();

			UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod = 1;
			_ = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			CombineAssertions(() =>
			{
				Assert("ImportShipment", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, "3824ARFY9JH", ZString.Empty, ZString.Empty));
				AssertEquals("DecisionReason", ZString.Empty, decisionSupporter.DecisionReason);
			});
		}

		public void TestHousebillForBillWithinTheRecycleRegistryPeriod()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			Factory.Save();

			UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod = 2;
			_ = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			Assert("ImportShipment should fail", !decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, "3824ARFY9JH", ZString.Empty, ZString.Empty));
			AssertEquals("Matching Housebill located in the system within the Shipment Number Recycle Period", decisionSupporter.DecisionReason);
		}

		public void TestHousebillForBillWithinTheRecycleRegistryPeriodIgnoresDisabledBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			bill.ABL_IsActive = false;
			Factory.Save();

			UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod = 2;
			_ = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			CombineAssertions(() =>
			{
				Assert("ImportShipment", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, "3824ARFY9JH", ZString.Empty, ZString.Empty));
				AssertEquals("DecisionReason", ZString.Empty, decisionSupporter.DecisionReason);
			});
		}

		public void TestHousebillForBillWithinTheRecycleRegistryPeriodIgnoresDisabledManifest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			header.AMA_IsActive = false;
			Factory.Save();

			UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod = 2;
			_ = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			CombineAssertions(() =>
			{
				Assert("ImportShipment", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, "3824ARFY9JH", ZString.Empty, ZString.Empty));
				AssertEquals("DecisionReason", ZString.Empty, decisionSupporter.DecisionReason);
			});
		}

		public void TestMatchForBillWithFilteringDisabledUsingHouseBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "3824ARFY9JH";
			bill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			Factory.Save();

			UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod = 2;
			UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFound = false;
			UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1Loads = true;

			_ = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { WayBillNumber = "3824ARFY9JH" };
			CombineAssertions(() =>
			{
				Assert("ImportShipment", decisionSupporter.ImportThisShipment(Factory, ZString.Empty, ZString.Empty, "3824ARFY9JH", ZString.Empty, ZString.Empty));
				AssertEquals("DecisionReason", ZString.Empty, decisionSupporter.DecisionReason);
			});
		}

		public void TestGetMatchingBillUsingHouseBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_BillNumber = "MAWBTEST01";
			header.AMA_Voyage = "JK01";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HAWB1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HAWB1";
			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "HAWB2";
			Factory.Save();

			AssertEquals(bill1, decisionSupporter.GetMatchingBill(Factory, "MAWBTEST01", "JK01", "HAWB1"));
			AssertNull(decisionSupporter.GetMatchingBill(Factory, "MAWBTEST01", "JK01", "HAWB3"));
			AssertEquals(bill3, decisionSupporter.GetMatchingBill(Factory, "MAWBTEST01", "JK01", "HAWB2"));
		}

		public void TestLoadManifestHeadersForMasterBillAndFlightNo()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.MasterBill.ABL_BillNumber = "MAWBTEST01";
			header1.AMA_Voyage = "JK01";

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.MasterBill.ABL_BillNumber = "MAWBTEST02";
			header2.AMA_Voyage = "FJ02";
			Factory.Save();

			AssertEquals(header1, decisionSupporter.LoadManifestHeadersForMasterBillAndFlightNo(Factory, "MAWBTEST01", "JK01").First());
			AssertEquals(header2, decisionSupporter.LoadManifestHeadersForMasterBillAndFlightNo(Factory, "MAWBTEST02", "FJ02").First());
			AssertNull(decisionSupporter.LoadManifestHeadersForMasterBillAndFlightNo(Factory, "MAWBTEST03", "EM01").FirstOrDefault());
		}

		public void TestShipmentNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => decisionSupporter.IsTradeNet(null, Factory.New<UPEOrgHeader>(), Factory));
		}

		public void TestFactoryNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => decisionSupporter.ImportThisShipment(null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
		}

		public void TestConsigneeNull()
		{
			AssertNoExceptionThrown(() => decisionSupporter.IsTradeNet(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), null, Factory));
		}

		#region Implementation

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			decisionSupporter = new SGDecisionSupporter();
			UPETestHelper.TaxOrFeeTestSetUp(Factory);
			base.SetUp();
		}
		SGDecisionSupporter decisionSupporter;

		CommercialCharge SetShipmentValue(Shipment shipment, ZDecimal amount)
		{
			shipment.CommercialInfo = new CommercialInfo();
			shipment.CommercialInfo.CommercialChargeCollection = new List<CommercialCharge>();
			var commercialCharge = new CommercialCharge()
			{
				ChargeType = new CodeDescriptionPair() { Code = Level1DataFileImporterForSGAccess.Constants.ChargeType.CustomsValue },
				Amount = amount,
			};
			shipment.CommercialInfo.CommercialChargeCollection.Add(commercialCharge);
			return commercialCharge;
		}

		void SetShipmentTransportMode(Shipment shipment, ZString transportMode)
		{
			shipment.TransportMode = new CodeDescriptionPair
			{
				Code = transportMode
			};
		}

		UPEOrgHeader GetUPEConsignee
		{
			get
			{
				var consignee = Factory.New<UPEOrgHeader>();
				consignee.OH_IsConsignee = true;
				consignee.OH_FullName = "FORD PRODUCT DEVELOPMENT";
				consignee.OH_Code = "FORMOTSIN";
				return consignee;
			}
		}

		UPEOrgCusCode GetUPECustomsCode(ZString codeType)
		{
			var cusCode = Factory.New<UPEOrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Singapore;
			cusCode.OK_CodeType = codeType;
			cusCode.OK_CustomsRegNo = "ANY";
			return cusCode;
		}

		List<AddInfo> GetMatchingReferenceAddInfo(ZString matchingReference)
		{
			return new List<AddInfo>()
				{
					AddInfo.New(Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.Bill.MatchingReference, matchingReference)
				};
		}
		const string MasterBillNumber = "08122222222";

		#endregion
	}
}
