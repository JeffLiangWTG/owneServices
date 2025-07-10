using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class FreightDataLayerTest : ContainerMessagingDataAbstractTest
	{
		public override void TestContainerIsWaitingForResponse()
		{
			ZDateTime now = ZDateTime.Now;

			CommonContainer container = Factory.New<CommonContainer>();
			FreightDataLayer dataLayer = new FreightDataLayer(container);
			AssertEquals(false, dataLayer.ContainerIsWaitingForResponse);

			EDIMessage message1 = container.PRAMessages.AddNew();
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now;
			AssertEquals(true, dataLayer.ContainerIsWaitingForResponse);

			EDIMessage message2 = container.PRAMessages.AddNew();
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			AssertEquals(false, dataLayer.ContainerIsWaitingForResponse);
		}

		public void TestOverhangHeightInCMDoesNotOverflow()
		{
			var container = Factory.New<ForwardingConsol>().Containers.AddNew();
			var shipment = container.Consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			container.JC_TotalHeight = Int16.MaxValue + 1;

			FreightDataLayer freightDataLayer = new FreightDataLayer(container);
			var expectedResult = Convert.ToInt32(Core.Constants.Length.Convert(Int16.MaxValue + 1, Core.Constants.Length.Feet, Core.Constants.Length.Centimetres));
			var actualResult = freightDataLayer.OverhangHeightInCM;
			AssertEquals(expectedResult, actualResult);
		}

		public void TestECNorCRNReturnsCANIfEXD()
		{
			const string TestCustomsEntryNumber = "12345";
			var container = Factory.New<ForwardingConsol>().Containers.AddNew();
			var shipment = container.Consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.Consol, container);
			FreightDataLayer freightDataLayer = new FreightDataLayer(container);
			shipment.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			shipment.CustomsEntryNumber = TestCustomsEntryNumber;
			AssertEquals("DataLayer.ECNorCRN", TestCustomsEntryNumber, freightDataLayer.ECNorCRN);
			shipment.CustomsEntryNumberType = "XXX";
			shipment.CustomsEntryNumber = TestCustomsEntryNumber;
			freightDataLayer = new FreightDataLayer(container);
			AssertEquals("DataLayer.ECNorCRN", ZString.Empty, freightDataLayer.ECNorCRN);
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.ECN;
			shipment.CustomsEntryNumber = TestCustomsEntryNumber;
			freightDataLayer = new FreightDataLayer(container);
			AssertEquals("DataLayer.ECNorCRN", TestCustomsEntryNumber, freightDataLayer.ECNorCRN);

			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.EX1;
			freightDataLayer = new FreightDataLayer(container);
			AssertEquals("DataLayer.ECNorCRN", CusEntryNumberTypes.Australia.EX1, freightDataLayer.ECNorCRN);

			shipment.CustomsEntryNumberType = CANType.Exemptions.EXPE.Code;
			freightDataLayer = new FreightDataLayer(container);
			AssertEquals("DataLayer.ECNorCRN", CANType.Exemptions.EXPE.Code, freightDataLayer.ECNorCRN);
		}

		public void TestContainerCustomsNumberUsedAsFallback()
		{
			const string ECNOrCan = "001777888";
			var container = Factory.New<ForwardingConsol>().Containers.AddNew();
			var shipment = container.Consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.Consol, container);
			shipment.CustomsEntryNumber = ECNOrCan;
			shipment.CustomsEntryNumberType = CusEntryNumberTypes.Australia.CAN;
			var data = new FreightDataLayer(container);
			AssertEquals("Should get the ECNOrCan from Container", ECNOrCan, data.ECNorCRN);
		}

		public override void TestGetMessageReferenceIsRightForImplementationInThisModule()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonContainer container = consol.Containers.AddNew();
			FreightDataLayer dataLayer = new FreightDataLayer(container);
			consol.JK_UniqueConsignRef = "123456";
			container.JC_ContainerNum = "OOCL1234567";

			AssertEquals("Message Reference should be returned in specified format", "CON-123456-OOCL1234567", dataLayer.MessageReference);
		}

		public void TestFreightDataIsNotCached()
		{
			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Z00000001";
			consol.JK_RL_NKLoadPort = "AUSYD";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "ZELORUR";
			container.JC_SealNum = "FROHGURT";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "LALALALA";
			ZGuid shipmentKey1 = shipment1.PK;
			PackLine packLine = shipment1.OuterPackLines.AddNew();
			packLine.Containers.Add(container);

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "LALALALA";
			ZGuid shipmentKey2 = shipment2.PK;
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.Containers.Add(container);

			Factory.Save();
			FreightDataLayer dataLayer1 = new FreightDataLayer(container);
			AssertEquals("Should be empty to begin with", ZString.Empty, dataLayer1.ECNorCRN);

			ForwardingShipment newShipment1 = newFactory1.Load<ForwardingShipment>(shipmentKey1);
			newShipment1.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			newShipment1.CustomsEntryNumber = "12345678";
			newFactory1.Save();

			FreightDataLayer dataLayer2 = new FreightDataLayer(container);
			AssertEquals("Should be empty still as CAN's differ", ZString.Empty, dataLayer2.ECNorCRN);

			ForwardingShipment newShipment2 = newFactory2.Load<ForwardingShipment>(shipmentKey2);
			newShipment2.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			newShipment2.CustomsEntryNumber = "12345678";
			newFactory2.Save();

			FreightDataLayer dataLayer3 = new FreightDataLayer(container);
			AssertEquals("Should now have CAN", "12345678", dataLayer3.ECNorCRN);
		}

		public override void TestFieldMappingsAfterPassingContainerIntoConstructor()
		{
			ZString saveCurrentBranchName = GlbBranch.CurrentBranch.OrgProxy.OH_FullName;
			try
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "CONZIGNOR GRIMO";

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "CONSIGNOR LTD";
				consignor.MainAddress.OA_Address1 = "72 ORiordan St";
				consignor.MainAddress.OA_City = "Alexandria";
				consignor.MainAddress.OA_State = "NSW";
				consignor.MainAddress.OA_PostCode = "2015";
				consignor.OH_RL_NKClosestPort = "AUSYD";

				var contact = consignor.Contacts.AddNew();
				contact.OC_ContactName = "MR CONSIGNOR";
				contact.OC_Phone = "(03) 1111 2222";
				contact.OC_Email = "contact@edi.com.au";

				OrgHeader shippingLine = Factory.New<OrgHeader>();
				OrgCusCode cusCode1 = shippingLine.CustomsCodes.AddNew();
				cusCode1.OK_CustomsRegNo = "BBBB2";
				cusCode1.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				cusCode1.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;

				OrgAddress cTOAddress = Factory.New<OrgAddress>();
				OrgHeader cTO = Factory.New<OrgHeader>();
				cTOAddress.OA_OH = cTO.PK;
				OrgCusCode cusCode2 = cTO.CustomsCodes.AddNew();
				cusCode2.OK_CustomsRegNo = "YYYY2";
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
				cusCode2.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;
				cusCode2.OK_OA_PremisesAddress = cTOAddress.PK;

				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "Z00000001";
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_AgentType = Core.Constants.AgentType.Direct;
				consol.SetDefaultSendingForwarderAddress(consignor);

				Transport mainTransport = consol.Transports[0];
				RefVessel refVessel = Factory.New<RefVessel>();
				refVessel.RV_LloydsNumber = "0982432";
				refVessel.RV_Code = "VESSELNAME";
				mainTransport.JW_Vessel = refVessel.RV_Code;
				AssertEquals("consol.JE_VesselName", "VESSELNAME", mainTransport.JW_Vessel);

				mainTransport.JW_RL_NKDiscPort = "USLAX";
				mainTransport.JW_VoyageFlight = "V102";

				Transport transportLeg = consol.Transports.AddNew();
				transportLeg.JW_RL_NKLoadPort = "USLAX";
				transportLeg.JW_RL_NKDiscPort = "USABQ";
				transportLeg.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
				transportLeg.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.OnForwarding;

				consol.JK_RL_NKDischargePort = "USABQ";
				consol.SetDefaultShippingLineAddress(shippingLine);
				consol.JK_OA_DepartureCTOAddress = cTOAddress.PK;
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

				CommonContainer container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TEST!@#";
				container.JC_SealNum = "HARPIC";

				container.JC_AirVentFlow = 100.15m;
				container.JC_AirVentFlowRateUnit = "MQH";

				container.JC_DepartureDeliveryByRail = true;
				container.JC_HumidityPercent = 33;

				RefContainer refContainer = Factory.New<RefContainer>();
				container.JC_RC = refContainer.PK;
				container.Container.RC_GrossWeight = 12000m;
				container.JC_DunnageWeight = 10000m;
				container.JC_TareWeight = 2000m;
				container.Container.RC_HasTynes = true;
				container.Container.RC_ISOType = "1099";

				container.JC_TotalLength = container.RefContainer.RC_Length + Core.Constants.Length.Convert(11m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JC_TotalWidth = container.RefContainer.RC_Width + Core.Constants.Length.Convert(22m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JC_TotalHeight = container.RefContainer.RC_Height + Core.Constants.Length.Convert(33m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JC_OverhangBack = Core.Constants.Length.Convert(3m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JC_OverhangRight = Core.Constants.Length.Convert(4m, Core.Constants.Length.Centimetres, Core.Constants.Length.Feet);
				container.JC_RefrigGeneratorID = "R101";
				container.JC_SetPointTemp = 12.0m;
				container.JC_DepartureSlotReference = "VBSBOOK";

				ForwardingShipment shipment = consol.Shipments.AddNew();
				shipment.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
				shipment.CustomsEntryNumber = "12341234";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.ConsignorDocumentaryAddress.ContactPK = consignor.Contacts[0].PK;

				PackLine packLine = shipment.OuterPackLines.AddNew();
				packLine.Containers.Add(container);

				OrgHeader cartageCompany = Factory.New<OrgHeader>();
				cartageCompany.PrimaryRegistrationNumber.Number = "ZZZZ2";
				shipment.DocsAndCartage.PickupCartageCoPK = cartageCompany.PK;

				RefCommodityCode code = Factory.New<RefCommodityCode>();
				code.RH_Description = "YOUGUESS";
				code.RH_Code = "XXXX";
				container.JC_RH_NKContainerCommodityCode = code.RH_Code;

				FreightDataLayer dataLayer = new FreightDataLayer(container);

				CombineAssertions(delegate
				{
					AssertEquals("dataLayer.ContainerNumber", "TEST!@#", dataLayer.ContainerNumber);
					AssertEquals("dataLayer.ECNorCRN", "12341234", dataLayer.ECNorCRN);
					AssertEquals("dataLayer.AirVentSetting", 100, dataLayer.AirVentSetting);
					AssertEquals("dataLayer.AirVentSetting", "MQH", dataLayer.AirVentSettingUnit);
					AssertEquals("dataLayer.ArrivingAtCTOByRail", true, dataLayer.ArrivingAtCTOByRail);
					AssertEquals("dataLayer.Consignor", "CONZIGNOR GRIMO", dataLayer.ConsignorName);
					AssertEquals("dataLayer.ContainerGrossWeight", 12000m, dataLayer.ContainerGrossWeight);
					AssertEquals("dataLayer.ContainerTareWeight", 2000m, dataLayer.ContainerTareWeight);
					AssertEquals("dataLayer.ContainerNetWeight", 10000m, dataLayer.ContainerNetWeight);
					AssertEquals("dataLayer.Commodity1StopCode", "XXXX", dataLayer.Commodity1StopCode);
					AssertEquals("dataLayer.GoodsDescription", "YOUGUESS", dataLayer.GoodsDescription);
					AssertEquals("dataLayer.HasTynes", true, dataLayer.HasTynes);
					AssertEquals("dataLayer.HumidityPercentage", 33, dataLayer.HumidityPercentage);
					AssertEquals("dataLayer.ISOContainerType", "1099", dataLayer.ISOContainerType);
					AssertEquals("dataLayer.LloydsNumber", "0982432", dataLayer.LloydsNumber);
					AssertEquals("dataLayer.MessageReference", "CON-Z00000001-TEST!@#", dataLayer.MessageReference);
					AssertEquals("DataLayer.OverhangFrontInCM", 8, dataLayer.OverhangFrontInCM);
					AssertEquals("DataLayer.OverhangBackInCM", 3, dataLayer.OverhangBackInCM);
					AssertEquals("DataLayer.OverhangLeftInCM", 18, dataLayer.OverhangLeftInCM);
					AssertEquals("DataLayer.OverhangRightInCM", 4, dataLayer.OverhangRightInCM);
					AssertEquals("dataLayer.OverhangHeightInCM", 33, dataLayer.OverhangHeightInCM);
					AssertEquals("dataLayer.PortOfDischarge", "USLAX", dataLayer.PortOfDischarge);
					AssertEquals("dataLayer.PortOfFinalDischarge", "USABQ", dataLayer.PortOfFinalDischarge);
					AssertEquals("dataLayer.PortOfLoading", "AUSYD", dataLayer.PortOfLoading);
					AssertEquals("dataLayer.ReeferGeneratorID", "R101", dataLayer.ReeferGeneratorID);
					AssertEquals("dataLayer.SealNumber", "HARPIC", dataLayer.SealNumber);
					AssertEquals("dataLayer.SenderContactName", GlbStaff.CurrentUser.GS_FullName, dataLayer.SenderContactName);
					AssertEquals("dataLayer.SenderFax", GlbStaff.CurrentUser.GS_FaxNum, dataLayer.SenderFax);
					AssertEquals("dataLayer.SenderPhone", GlbStaff.CurrentUser.GS_WorkPhone, dataLayer.SenderPhone);
					AssertEquals("dataLayer.TemperatureSetting", "12.0", dataLayer.TemperatureSettingFormatted);
					AssertEquals("dataLayer.TerminalVBSBooking", "VBSBOOK", dataLayer.TerminalVBSBooking);
					AssertEquals("dataLayer.VesselName", "VESSELNAME", dataLayer.VesselName);
					AssertEquals("dataLayer.Voyage", "V102", dataLayer.Voyage);
					AssertEquals("dataLayer.ShippingLine1StopCode", "BBBB2", dataLayer.ShippingLine1StopCode);
					AssertEquals("dataLayer.CartageCompanyABN", "ZZZZ2", dataLayer.CartageCompanyABN);
					AssertEquals("dataLayer.LoadTerminal1StopCode", "YYYY2", dataLayer.LoadTerminal1StopCode);
					AssertEquals("dataLayer.GrossWeightVerifiedDeclarantSignature", "MR CONSIGNOR", dataLayer.GrossWeightVerifiedDeclarantSignature);
					AssertEquals("dataLayer.GrossWeightVerifiedDeclarantContact", "MR CONSIGNOR;CONSIGNOR LTD;(03) 1111 2222;contact@edi.com.au", dataLayer.GrossWeightVerifiedDeclarantContact);

					//UnMapped Fields
					AssertEquals("dataLayer.ShippingLineBookingReference", "", dataLayer.ShippingLineBookingReference);
					AssertEquals("dataLayer.FlatRackID", "", dataLayer.FlatRackID);
					AssertEquals("dataLayer.TruckRegoNumber", "", dataLayer.TruckRegoNumber);
					AssertEquals("dataLayer.RoadDest1StopCode", "", dataLayer.RoadDest1StopCode);
					AssertEquals("dataLayer.RoadOrig1StopCode", "", dataLayer.RoadOrig1StopCode);
					AssertEquals("dataLayer.CartageBookingReference", "", dataLayer.CartageBookingReference);
					AssertEquals("dataLayer.RoadScheduledArrival", ZDateTime.Empty, dataLayer.RoadScheduledArrival);
					AssertEquals("dataLayer.RoadScheduledDeparture", ZDateTime.Empty, dataLayer.RoadScheduledDeparture);
				});
			}
			finally
			{
				GlbBranch.CurrentBranch.OrgProxy.OH_FullName = saveCurrentBranchName;
			}
		}

		public void TestAirVentSettingWithDecimalPlace()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_AirVentFlow = 15.4m;
			container.JC_AirVentFlowRateUnit = "MQH";
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("AirVentFlow with decimal point", 15, data.AirVentSetting);
		}

		public void TestGetAirVentSettingUnit()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_AirVentFlow = 15.4m;
			container.JC_AirVentFlowRateUnit = "MQH";
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("AirVentSettingUnit", "MQH", data.AirVentSettingUnit);
		}

		public void TestGetContainerGrossWeight()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_TareWeight = 3000m;
			container.JC_GrossWeight = 10000m;

			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("ContainerGrossWeight", 10000m, data.ContainerGrossWeight);
		}

		public void TestGetIsTempControlledIsTrueWhenUsingCustomContainerSize()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			RefContainer containerDetails = Factory.New<RefContainer>();
			container.JC_RC = containerDetails.PK;
			containerDetails.RC_ContainerType = Core.Constants.ContainerTypes.Refrigerated;
			containerDetails.RC_Length = 88;
			container.JC_IsControlledAtmosphere = true;
			FreightDataLayer data = new FreightDataLayer(container);
			Assert("Should be refrigerated", data.IsTempControlled);
		}

		public void TestSetPointTemperatureWithDecimalPlaces()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_SetPointTemp = -15.4m;
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("Temperature with decimal point", "-15.4", data.TemperatureSettingFormatted);
		}

		public void TestSetPointTemperatureWithNoDecimalPlaces()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_SetPointTemp = 11m;
			FreightDataLayer data1 = new FreightDataLayer(container);
			AssertEquals("Temperature with no decimal point", "11.0", data1.TemperatureSettingFormatted);
			FreightDataLayer data2 = new FreightDataLayer(container);
			container.JC_SetPointTemp = 4m;
			AssertEquals("Temperature with no decimal point (1 long)", "04.0", data2.TemperatureSettingFormatted);
		}

		public void TestBookingReference()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_BookingReference = "CONSOLREF";

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ReleaseNum = "CONTREF";

			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("Should use consol JK_BookingReference", "CONSOLREF", data.ShippingLineBookingReference);

			consol.JK_BookingReference = ZString.Empty;
			data = new FreightDataLayer(container);
			AssertEquals("Should use Release Number / Booking Ref from Container", "CONTREF", data.ShippingLineBookingReference);
		}

		public void TestDirectConsolReturnsCorrectConsignee()
		{
			const string ConsignorName = "Senior Cardgraves";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonContainer container = consol.Containers.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = ConsignorName;
			consol.ShipmentConsignor = consignor.PK;
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("Direct Consol should return Consignee", ConsignorName, data.ConsignorName);
		}

		public void TestSendingForwarderReturnedIfNotDirectConsol()
		{
			const string ForwarderName = "Mepsi Pax";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonContainer container = consol.Containers.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Other;
			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = ForwarderName;
			consol.SetDefaultSendingForwarderAddress(forwarder);
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("Direct Consol should return Forwarder", ForwarderName, data.ConsignorName);
		}

		public void TestTruckRegistrationNumberIsAlwaysEmpty()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonPickupDeliveryConfirm leg = container.OriginCFSDeparture;
			leg.EU_VehicleRegistration = "KDKS223";
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("Truck Registration should not be included", "", data.TruckRegoNumber);
		}

		public void TestGetGrossWeightVerifiedByAddressWithNoException()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "IDJKT";
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TESU1234567";
			consol.JK_BookingReference = "BOOKING123";

			container.JC_TareWeight = 0m;
			container.JC_GrossWeight = 11600m;
			container.JC_SealNum = "289184";
			container.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 4, 1, 13, 59, 0);
			container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
			container.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
			container.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
			container.GrossWeightVerifiedByAddress.E2_Address2 = "Address 2";
			container.GrossWeightVerifiedByAddress.E2_City = "Mascot";
			container.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
			container.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "";
			FreightDataLayer data = new FreightDataLayer(container);
			AssertEquals("CONTAINER WEIGHTING LTD;Address 1 Address 2;Mascot", data.GrossWeightVerifiedByAddress);
		}

		public void TestLoadFromNewFreightContainer()
		{
			var collection = new DangerousGoodsCollection();
			var container = Factory.New<CommonContainer>();
			var dataLayer = new FreightDataLayer(container);
			dataLayer.LoadFromContainer(collection);
			AssertEquals(0, collection.Count);
		}

		public void TestLoad2LinesFromRealFreightContainer()
		{
			var collection = new DangerousGoodsCollection();
			AssertEquals("Pre-Condition", 0, collection.Count);

			var container = Factory.New<ForwardingContainer>();
			var shipment = Factory.New<ForwardingShipment>();

			var subsa = Factory.New<UNDGSubstance>();
			subsa.DG_UNNO = "123";
			subsa.DG_Variant = "a";
			subsa.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsb = Factory.New<UNDGSubstance>();
			subsb.DG_UNNO = "123";
			subsb.DG_Variant = "b";
			subsb.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.Containers.Add(container);
			packLine1.UNDGs.AddNew().LinkDefault(subsa);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.Containers.Add(container);
			packLine2.UNDGs.AddNew().LinkDefault(subsb);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.Containers.Add(container);

			var dataLayer = new FreightDataLayer(container);
			dataLayer.LoadFromContainer(collection);
			AssertEquals("Should only be 2 Packlines with DG on them", 2, collection.Count);
		}

		public void TestLoadFromContainer_DGItemWithoutWeightEntered_UsePacklineWeight()
		{
			var collection = new DangerousGoodsCollection();
			var container = Factory.New<ForwardingContainer>();
			var shipment = Factory.New<ForwardingShipment>();

			var subsa = Factory.New<UNDGSubstance>();
			subsa.DG_UNNO = "123";
			subsa.DG_Variant = "a";
			subsa.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsb = Factory.New<UNDGSubstance>();
			subsb.DG_UNNO = "123";
			subsb.DG_Variant = "b";
			subsb.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var packline = shipment.OuterPackLines.AddNew();
			packline.Containers.Add(container);
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var undg = packline.UNDGs.AddNew();
			undg.LinkDefault(subsa);

			var undg2 = packline.UNDGs.AddNew();
			undg2.LinkDefault(subsb);

			var dataLayer = new FreightDataLayer(container);
			dataLayer.LoadFromContainer(collection);

			AssertEquals((decimal)200, collection[0].Weight);
			AssertEquals((decimal)200, collection[1].Weight);
		}

		public void TestLoadFromContainer_DGItemsWithWeightEntered_UseDGItemWeight()
		{
			var collection = new DangerousGoodsCollection();
			var container = Factory.New<ForwardingContainer>();
			var shipment = Factory.New<ForwardingShipment>();

			var subsa = Factory.New<UNDGSubstance>();
			subsa.DG_UNNO = "123";
			subsa.DG_Variant = "a";
			subsa.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subsb = Factory.New<UNDGSubstance>();
			subsb.DG_UNNO = "123";
			subsb.DG_Variant = "b";
			subsb.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var packline = shipment.OuterPackLines.AddNew();
			packline.Containers.Add(container);
			packline.JL_ActualWeight = 200;
			packline.JL_ActualWeightUQ = "KG";

			var undg1 = packline.UNDGs.AddNew();
			undg1.LinkDefault(subsa);
			undg1.DI_DGWeight = 10;
			undg1.DI_UnitOfWeight = "KG";

			var undg2 = packline.UNDGs.AddNew();
			undg2.LinkDefault(subsb);
			undg2.DI_DGWeight = 20;
			undg2.DI_UnitOfWeight = "KG";

			var dataLayer = new FreightDataLayer(container);
			dataLayer.LoadFromContainer(collection);

			AssertEquals((decimal)10, collection[0].Weight);
			AssertEquals((decimal)20, collection[1].Weight);
		}

		public void TestLoadValuesFromRealFreightContainer()
		{
			var collection = new DangerousGoodsCollection();
			AssertEquals("Pre-Condition", 0, collection.Count);

			var container = Factory.New<CommonContainer>();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "2120";
			subs.DG_Variant = "2";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "111";
			subs.DG_FlashPoint = "23";
			subs.DG_PG = "II";
			subs.DG_PSN = "PUPPY DOGS TAILS";

			var dangerousOrganisation = Factory.New<OrgHeader>();
			var dGContact = dangerousOrganisation.Contacts.AddNew();
			dGContact.OC_ContactName = "Roger Roger";
			dGContact.OC_Phone = "1234567890";
			dGContact.OC_Fax = "0987654321";
			dGContact.OC_Email = "rogerroger@roger.com";

			var dGContactDetails = dGContact.DocDeliveryDetails();
			AssertEquals("Precondition on DGContactDetails.Name", "Roger Roger", dGContactDetails.Name);
			AssertEquals("Precondition on DGContactDetails.Phone", "1234567890", dGContactDetails.Phone);
			AssertEquals("Precondition on DGContactDetails.Fax", "0987654321", dGContactDetails.Fax);
			AssertEquals("Precondition on DGContactDetails.Email", "rogerroger@roger.com", dGContactDetails.Email);

			var packLine = Factory.New<ForwardingShipment>().OuterPackLines.AddNew();
			packLine.Containers.Add(container);

			var dgItem = packLine.UNDGs.AddNew();
			dgItem.LinkDefault(subs);
			dgItem.DI_OC_DGContact = dGContact.PK;
			dgItem.DI_DGFlashPoint = 33.3m;

			packLine.JL_ActualWeight = 30m;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			var dataLayer = new FreightDataLayer(container);
			dataLayer.LoadFromContainer(collection);

			AssertEquals("Count of ContainerMessagingDangerousGoods in Collection", 1, collection.Count);

			ContainerMessagingDangerousGoods goods = collection[0];

			AssertEquals("Goods.IMDGClass", dgItem.Substance.DG_Class, goods.IMDGClass);
			AssertEquals("Goods.IMDGCodePage", "", goods.IMDGCodePage);
			AssertEquals("Goods.IMDGCodeVersion", dgItem.Substance.DG_Variant, goods.IMDGCodeVersion);
			AssertEquals("Goods.UNDGNumber", dgItem.Substance.DG_UNNO, goods.UNDGNumber);
			AssertEquals("Goods.FlashpointTemperatureInCelcius", TemperatureFormatter.FormatTemperatureString(dgItem.DI_DGFlashPoint.ToString()), goods.FlashpointTemperatureInCelcius);
			AssertEquals("Goods.PackingGroup", dgItem.Substance.DG_PG, goods.PackingGroup);
			AssertEquals("Goods.TechnicalName", dgItem.Substance.DG_PSN, goods.TechnicalName);
			AssertEquals("Goods.Weight", Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, Core.Constants.Weight.Kilograms), goods.Weight);

			AssertEquals("Goods.ContactName", dGContactDetails.Name, goods.ContactName);
			AssertEquals("Goods.ContactPhoneNumber", dGContactDetails.Phone, goods.ContactPhoneNumber);
			AssertEquals("Goods.ContactFaxNumber", dGContactDetails.Fax, goods.ContactFaxNumber);
			AssertEquals("Goods.ContactEmailAddress", dGContactDetails.Email, goods.ContactEmailAddress);
		}

		public void TestLoadValuesFromRealFreightContainer_IsCombustibleFlashPoint()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var collection = new DangerousGoodsCollection();
				AssertEquals("Pre-Condition", 0, collection.Count);

				var container = Factory.New<CommonContainer>();

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "2120";
				subs.DG_Variant = "2";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				subs.DG_Class = "111";
				subs.DG_FlashPoint = "23";

				var packLine = Factory.New<ForwardingShipment>().OuterPackLines.AddNew();
				packLine.Containers.Add(container);
				var dgItem = packLine.UNDGs.AddNew();
				dgItem.LinkDefault(subs);
				dgItem.DI_DGFlashPoint = 33.3m;
				dgItem.DI_IsCombustible = false;

				var dataLayer1 = new FreightDataLayer(container);
				dataLayer1.LoadFromContainer(collection);
				AssertEquals("Goods.FlashpointTemperatureInCelcius", ZString.Empty, collection[0].FlashpointTemperatureInCelcius);

				dgItem.DI_IsCombustible = true;
				var dataLayer2 = new FreightDataLayer(container);
				dataLayer2.LoadFromContainer(collection);
				AssertEquals("Goods.FlashpointTemperatureInCelcius", "33.3", collection[1].FlashpointTemperatureInCelcius);
			}
		}

		protected override ContainerMessagingData GetDataLayerPassingNullIntoConstructor() => new FreightDataLayer(Factory.New<ForwardingContainer>());

		protected override ContainerMessagingData GetDataLayerPassingEmptyContainerIntoConstructor()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonContainer container = consol.Containers.AddNew();
			containerMessaging = container;
			return new FreightDataLayer(container);
		}

		protected override ContainerMessagingData GetReadyToSaveDataLayer() => GetDataLayerPassingEmptyContainerIntoConstructor();

		protected override string ExpectedPRAErrors
		{
			get
			{
				return @"
Shipping Line Booking Reference. (Consol Tab or Container Tab)
ECN or CRN. (Against Consol or Shipment as applicable)
   NB: If you are relying on the ECN from the Shipment, make sure you have
   this container selected in the 'Packing' section of the relevant Shipment.
   Important: 1-Stop can only accept one reference per container, so if you 
   have multiple shipments in the one container with individual CAN's, you will 
   have to submit a CRN for the Consol to get a single CAN for the contents 
   of the whole container before sending a PRA.
Carrier 1-Stop Code. (Registration Numbers on the Config Tab in the Carrier Master File)
Vessel Name. (Consol Tab)
Voyage. (Consol Tab)
Lloyds Number. (Vessel Master File for selected vessel on Consol Tab)
Port Of Loading. (Consol Tab)
Departure CTO 1-Stop Code. (Registration Numbers on the Config Tab in the CTO Master File)
Port Of Discharge. (Consol Tab)
Container Number. (Containers Tab in the Grid)
ISO Container Type. (Containers Tab in the Grid)
Commodity 1-Stop Code. (Containers Tab in the Grid)
Container Gross Weight. (Containers Tab in the Grid)
Seal Number Not Entered. (Containers Tab in the Grid)
Container Verified Weight Method must be CNT or PKG. (VGM Tab on the Containers Tab)
";
			}
		}

		protected override string ECNorCRNErrorText
		{
			get
			{
				return @"ECN or CRN. (Against Consol or Shipment as applicable)
   NB: If you are relying on the ECN from the Shipment, make sure you have
   this container selected in the 'Packing' section of the relevant Shipment.
   Important: 1-Stop can only accept one reference per container, so if you 
   have multiple shipments in the one container with individual CAN's, you will 
   have to submit a CRN for the Consol to get a single CAN for the contents 
   of the whole container before sending a PRA.
";
			}
		}
	}
}
