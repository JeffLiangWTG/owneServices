using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Testing
{
	public class ExportHouseBillOfLadingTest : TestCaseWithFactory
	{
		#region Constructor
		public void TestConstructor()
		{
			AssertNotNull(HouseBillOfLading);
			AssertEquals(Shipment, HouseBillOfLading.Shipment);
			AssertEquals(Consol, HouseBillOfLading.HeaderData);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "Shipment cannot be null")]
		public void TestConstructor_NullShipment()
		{
			ExportHouseBillOfLading houseBillOfLading = new ExportHouseBillOfLading(Consol, null);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "HeaderData cannot be null")]
		public void TestConstructor_NullHeaderData()
		{
			ExportHouseBillOfLading houseBillOfLading = new ExportHouseBillOfLading(null, Shipment);
		}

		#endregion
		#region Property Tests
		#region Consol
		public void TestConsol()
		{
			AssertEquals(Consol, HouseBillOfLading.Consol);
			AssertNull("This is a pre-shipment, should not have consol", PreShipmentHouseBillOfLading.Consol);
		}

		#endregion
		#region Identifier
		public void TestOceanHouseBillOfLadingNum()
		{
			Shipment.JS_HouseBill = "HB111XX";
			AssertEquals("HB111XX", HouseBillOfLading.OceanHouseBillOfLadingNo);
		}

		public void TestOceanBillOfLadingNum()
		{
			Consol.JK_MasterBillNum = "OBLCONS009";
			AssertEquals("OBLCONS009", HouseBillOfLading.OceanBillOfLadingNo);
		}

		public void TestOceanBillOfLadingNum_PreShipment()
		{
			AssertEquals("Should not throw exception when consol is null", "", PreShipmentHouseBillOfLading.OceanBillOfLadingNo);
		}

		#endregion
		#region Port of Loading / Port of Discharge
		public void TestPortOfLoading()
		{
			Shipment.JS_RL_NKOrigin = "AUMEL";
			Consol.JK_RL_NKLoadPort = "AUBNE";
			AssertEquals("Port of Loading", "AUBNE", HouseBillOfLading.PortOfLoading.Code);
			AssertEquals("Port of Loading", "AUBNE", HouseBillOfLading.PortOfLoadingCode);
			AssertEquals("Port of Loading", Consol.LoadPort.RL_PortName, HouseBillOfLading.PortOfLoadingName);
		}

		public void TestPortOfLoading_PreShipment()
		{
			Shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals("Should be from Shipment's origin", "AUMEL", PreShipmentHouseBillOfLading.PortOfLoading.Code);
			AssertEquals("Should be from Shipment's origin", "AUMEL", PreShipmentHouseBillOfLading.PortOfLoadingCode);
			AssertEquals("Should be from Shipment's origin", Shipment.Origin.RL_PortName, PreShipmentHouseBillOfLading.PortOfLoadingName);
		}

		public void TestPortOfDischarge()
		{
			Shipment.JS_RL_NKDestination = "USATL";
			Consol.JK_RL_NKDischargePort = "ITMIL";
			AssertEquals("Port of Discharge", "ITMIL", HouseBillOfLading.PortOfDischarge.Code);
			AssertEquals("Port of Discharge", "ITMIL", HouseBillOfLading.PortOfDischargeCode);
			AssertEquals(HouseBillOfLading.PortOfDischarge.RL_PortName, HouseBillOfLading.PortOfDischargeName);
		}

		public void TestPortOfDischarge_PreShipment()
		{
			Shipment.JS_RL_NKDestination = "USATL";
			AssertEquals("Should be from Shipment's destination", "USATL", PreShipmentHouseBillOfLading.PortOfDischarge.Code);
			AssertEquals("Should be from Shipment's destination", "USATL", PreShipmentHouseBillOfLading.PortOfDischargeCode);
			AssertEquals("Should be from Shipment's destination", Shipment.Destination.RL_PortName, PreShipmentHouseBillOfLading.PortOfDischargeName);
		}

		#endregion
		#region Place of Receipt / Place of Delivery
		public void TestPlaceOfReceipt()
		{
			Shipment.JS_RL_NKOrigin = "";
			AssertEquals(ZString.Empty, HouseBillOfLading.PlaceOfReceipt);
			Shipment.JS_RL_NKOrigin = "AUBNE";
			RefUNLOCO expected = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			AssertEquals(expected.RL_PortName, HouseBillOfLading.PlaceOfReceipt);
		}

		public void TestPlaceOfDelivery()
		{
			Shipment.JS_RL_NKDestination = "";
			AssertEquals(ZString.Empty, HouseBillOfLading.PlaceOfDelivery);
			Shipment.JS_RL_NKDestination = "GBLON";
			RefUNLOCO expected = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
			AssertEquals(expected.RL_PortName, HouseBillOfLading.PlaceOfDelivery);
		}

		#endregion
		#region Shipping Line
		public void TestShippingLineDetails()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLineName);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLineAddress1);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLineAddress2);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLineCity);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLinePortCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLineSSLCode);
			SetupShippingLineDetails();
			AddPatternMatchOverridesForOtherCodes();
			AssertEquals("Dodgy Shipping Line", HouseBillOfLading.ShippingLineName);
			AssertEquals("Address 123", HouseBillOfLading.ShippingLineAddress1);
			AssertEquals("This is another line2", HouseBillOfLading.ShippingLineAddress2);
			AssertEquals("Montevideo", HouseBillOfLading.ShippingLineCity);
			AssertEquals("AUADL", HouseBillOfLading.ShippingLinePortCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShippingLineSSLCode);
			AddPatternMatchOverrideForSSLCode();
			AssertEquals("XYZ", HouseBillOfLading.ShippingLineSSLCode);
		}

		void SetupShippingLineDetails()
		{
			if (Consol.ShippingLine == null)
			{
				OrgHeader shippingLine = Factory.New<OrgHeader>();
				shippingLine.OH_RL_NKClosestPort = "AUADL";
				shippingLine.MainAddress.OA_Address1 = "Address 123";
				shippingLine.MainAddress.OA_Address2 = "This is another line2";
				shippingLine.MainAddress.OA_City = "Montevideo";
				shippingLine.OH_FullName = "Dodgy Shipping Line";
				shippingLine.OH_Code = "SHPLIN1";
				Consol.SetDefaultShippingLineAddress(shippingLine);
			}
		}

		void AddPatternMatchOverridesForOtherCodes()
		{
			OrgPatternMatchOverride override1 = Consol.ShippingLine.CreatePatternMatchOverrideForTest();
			override1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			override1.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
			override1.OO_ForeignCode = "ABC";
			OrgPatternMatchOverride override2 = Consol.ShippingLine.CreatePatternMatchOverrideForTest();
			override2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			override2.OO_LocalGuid = ZGuid.NewZGuid();
			override2.OO_ForeignCode = "123";
		}

		void AddPatternMatchOverrideForSSLCode()
		{
			OrgPatternMatchOverride @override = Consol.ShippingLine.CreatePatternMatchOverrideForTest();
			@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			@override.OO_LocalGuid = JASDataRegistry.Instance.JASWWOrganisationPK;
			@override.OO_ForeignCode = "XYZ";
		}

		#endregion
		#region Shipper
		public void TestShipperDetails()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperName);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperAddress1);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperAddress2);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperCity);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperCountryCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperEmail);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperPhone);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperPostalCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperState);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperAccount);
			AssertEquals(ZString.Empty, HouseBillOfLading.ShipperPortCode);
			SetupShipperDetails();
			AssertEquals("Another lame shipper", HouseBillOfLading.ShipperName);
			AssertEquals("Shipper Add1", HouseBillOfLading.ShipperAddress1);
			AssertEquals("Shipper 123099", HouseBillOfLading.ShipperAddress2);
			AssertEquals("Lima", HouseBillOfLading.ShipperCity);
			AssertEquals("JP", HouseBillOfLading.ShipperCountryCode);
			AssertEquals("test@lima.edi.com.au", HouseBillOfLading.ShipperEmail);
			AssertEquals("889-7182", HouseBillOfLading.ShipperPhone);
			AssertEquals("89701", HouseBillOfLading.ShipperPostalCode);
			AssertEquals("Queenstown", HouseBillOfLading.ShipperState);
			AssertEquals("SHPER01", HouseBillOfLading.ShipperAccount);
			AssertEquals("JPTYO", HouseBillOfLading.ShipperPortCode);
		}

		void SetupShipperDetails()
		{
			if (Shipment.Consignor == null)
			{
				OrgHeader shipper = Factory.New<OrgHeader>();
				shipper.OH_RL_NKClosestPort = "JPTYO";
				shipper.MainAddress.OA_Address1 = "Shipper Add1";
				shipper.MainAddress.OA_Address2 = "Shipper 123099";
				shipper.MainAddress.OA_City = "Lima";
				shipper.MainAddress.OA_State = "Queenstown";
				shipper.MainAddress.OA_PostCode = "89701";
				shipper.MainAddress.OA_Phone = "889-7182";
				shipper.MainAddress.OA_Email = "test@lima.edi.com.au";
				shipper.OH_FullName = "Another lame shipper";
				shipper.OH_Code = "SHPER01";
				Shipment.ConsignorPK = shipper.PK;
			}
		}

		#endregion
		#region Consignee
		public void TestConsigneeDetails()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeName);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeAddress1);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeAddress2);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeCity);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeCountryCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeEmail);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneePhone);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneePostalCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeState);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneeAccount);
			AssertEquals(ZString.Empty, HouseBillOfLading.ConsigneePortCode);
			SetupConsigneeDetails();
			AssertEquals("Consignee Awaiting Eagerly", HouseBillOfLading.ConsigneeName);
			AssertEquals("Conadd-10", HouseBillOfLading.ConsigneeAddress1);
			AssertEquals("Convict house 667", HouseBillOfLading.ConsigneeAddress2);
			AssertEquals("Bustard", HouseBillOfLading.ConsigneeCity);
			AssertEquals("IT", HouseBillOfLading.ConsigneeCountryCode);
			AssertEquals("victoria@bustard.edi.com.au", HouseBillOfLading.ConsigneeEmail);
			AssertEquals("9998989", HouseBillOfLading.ConsigneePhone);
			AssertEquals("XX901V2", HouseBillOfLading.ConsigneePostalCode);
			AssertEquals("Victoria", HouseBillOfLading.ConsigneeState);
			AssertEquals("CON998", HouseBillOfLading.ConsigneeAccount);
			AssertEquals("ITTRN", HouseBillOfLading.ConsigneePortCode);
		}

		void SetupConsigneeDetails()
		{
			if (Shipment.Consignee == null)
			{
				OrgHeader consignee = Factory.New<OrgHeader>();
				consignee.OH_RL_NKClosestPort = "ITTRN";
				consignee.MainAddress.OA_Address1 = "Conadd-10";
				consignee.MainAddress.OA_Address2 = "Convict house 667";
				consignee.MainAddress.OA_City = "Bustard";
				consignee.MainAddress.OA_State = "Victoria";
				consignee.MainAddress.OA_PostCode = "XX901V2";
				consignee.MainAddress.OA_Phone = "9998989";
				consignee.MainAddress.OA_Email = "victoria@bustard.edi.com.au";
				consignee.OH_FullName = "Consignee Awaiting Eagerly";
				consignee.OH_Code = "CON998";
				Shipment.ConsigneePK = consignee.PK;
			}
		}

		#endregion
		#region Delivery Agent
		public void TestDeliveryAgent()
		{
			AssertNull(HouseBillOfLading.DeliveryAgent);
			Consol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			AssertEquals(Consol.ReceivingForwarder, HouseBillOfLading.DeliveryAgent);
			Shipment.JS_OH_DeliveryAgent = Factory.New(typeof(OrgHeader)).PK;
			AssertEquals("Delivery agent should be the shipment's delivery agent now", Shipment.DeliveryAgent, HouseBillOfLading.DeliveryAgent);
		}

		public void TestDeliveryAgentDetails()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.DeliveryAgentName);
			AssertEquals(ZString.Empty, HouseBillOfLading.DeliveryAgentAddress1);
			AssertEquals(ZString.Empty, HouseBillOfLading.DeliveryAgentAddress2);
			AssertEquals(ZString.Empty, HouseBillOfLading.DeliveryAgentCity);
			AssertEquals(ZString.Empty, HouseBillOfLading.DeliveryAgentPortCode);
			SetupDeliveryAgentDetails();
			AssertEquals("Whatever agent", HouseBillOfLading.DeliveryAgentName);
			AssertEquals("new address1", HouseBillOfLading.DeliveryAgentAddress1);
			AssertEquals("new line 2", HouseBillOfLading.DeliveryAgentAddress2);
			AssertEquals("Barcelona", HouseBillOfLading.DeliveryAgentCity);
			AssertEquals("ESBCN", HouseBillOfLading.DeliveryAgentPortCode);
		}

		void SetupDeliveryAgentDetails()
		{
			if (Shipment.DeliveryAgent == null)
			{
				OrgHeader deliveryAgent = Factory.New<OrgHeader>();
				deliveryAgent.OH_RL_NKClosestPort = "ESBCN";
				deliveryAgent.MainAddress.OA_Address1 = "new address1";
				deliveryAgent.MainAddress.OA_Address2 = "new line 2";
				deliveryAgent.MainAddress.OA_City = "Barcelona";
				deliveryAgent.OH_FullName = "Whatever agent";
				Shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			}
		}

		#endregion
		#region Notify Party
		public void TestNotifyPartyDetails()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.NotifyPartyName);
			AssertEquals(ZString.Empty, HouseBillOfLading.NotifyPartyAddress1);
			AssertEquals(ZString.Empty, HouseBillOfLading.NotifyPartyAddress2);
			AssertEquals(ZString.Empty, HouseBillOfLading.NotifyPartyCity);
			AssertEquals(ZString.Empty, HouseBillOfLading.NotifyPartyEmail);
			AssertEquals(ZString.Empty, HouseBillOfLading.NotifyPartyPhone);
			SetupNotifyPartyDetails();
			AssertEquals("Mr. Smith", HouseBillOfLading.NotifyPartyName);
			AssertEquals("NO1", HouseBillOfLading.NotifyPartyAddress1);
			AssertEquals("NO2", HouseBillOfLading.NotifyPartyAddress2);
			AssertEquals("Bandung", HouseBillOfLading.NotifyPartyCity);
			AssertEquals("Email@edi.com.au", HouseBillOfLading.NotifyPartyEmail);
			AssertEquals("23456789", HouseBillOfLading.NotifyPartyPhone);
		}

		void SetupNotifyPartyDetails()
		{
			if (Shipment.NotifyParty == null)
			{
				OrgHeader notifyOrg = Factory.New<OrgHeader>();
				notifyOrg.OH_RL_NKClosestPort = "IDBKA";
				notifyOrg.MainAddress.OA_Address1 = "NO1";
				notifyOrg.MainAddress.OA_Address2 = "NO2";
				notifyOrg.MainAddress.OA_City = "Bandung";
				notifyOrg.MainAddress.OA_Phone = "23456789";
				notifyOrg.MainAddress.OA_Email = "Email@edi.com.au";
				notifyOrg.OH_FullName = "Notify Org1";
				notifyOrg.OH_Code = "NOTYORG";
				OrgContact notifyParty = notifyOrg.Contacts.AddNew();
				notifyParty.OC_ContactName = "Mr. Smith";
				OrgDocument doc1 = notifyParty.Documents.AddNew();
				doc1.OD_DocumentGroup = ContactType.NotifyParty.Code;
				Shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyOrg.PK;
			}
		}

		#endregion
		#region Freight Charges
		public void TestPaymentTerm()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.PaymentTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Prepaid);
			AssertEquals(Core.Constants.PaymentType.Prepaid, HouseBillOfLading.PaymentTerm);
			Shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Collect);
			AssertEquals(Core.Constants.PaymentType.Collect, HouseBillOfLading.PaymentTerm);
		}

		public void TestCurrency()
		{
			ZString originalCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			try
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "IDR";
				AssertEquals("IDR", HouseBillOfLading.Currency);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = originalCurrency;
			}
		}

		public void TestTotalFreightAmount()
		{
			Consol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			JASJob shipmentJobHeader = CreateJobHeaderForShipment();
			JASJob otherJobHeader = CreateJobHeaderFromDifferentCompany();
			AssertEquals(ZDecimal.Zero, HouseBillOfLading.TotalFreightAmount);
			CreateTestJobCharges(shipmentJobHeader, 88.89m, 10m, 20m);
			CreateTestJobCharges(otherJobHeader, 12.34m);
			AssertEquals(118.89m, HouseBillOfLading.TotalFreightAmount);
		}

		void CreateTestJobCharges(JASJob header, params ZDecimal[] freightAmounts)
		{
			foreach (ZDecimal freightAmount in freightAmounts)
			{
				JobCharge freightCharge = Factory.New<JobCharge>();
				freightCharge.JR_AC = Env.Registry.FreightChargeCode;
				freightCharge.JR_LocalSellAmt = freightAmount;
				freightCharge.JR_JH = header.PK;
			}

			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);
			filter.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, Env.Registry.FreightChargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(filter);
			JobCharge charge2 = Factory.New<JobCharge>();
			charge2.JR_AC = chargeCode.PK;
			charge2.JR_LocalSellAmt = 1;
			charge2.JR_JH = header.PK;
			JobCharge charge3 = Factory.New<JobCharge>();
			charge3.JR_LocalSellAmt = 12;
			charge3.JR_JH = header.PK;
			JobCharge charge4 = Factory.New<JobCharge>();
			charge4.JR_LocalSellAmt = 13;
			charge4.JR_JH = header.PK;
			JobCharge charge5 = Factory.New<JobCharge>();
			charge5.JR_AC = chargeCode.PK;
			charge5.JR_LocalSellAmt = 14;
			charge5.JR_JH = header.PK;
			header.Charges.Load();
		}

		JASJob CreateJobHeaderForShipment()
		{
			JASJob header = Factory.NewJobForTesting<JASJob>();
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_ParentID = Shipment.PK;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			return header;
		}

		JASJob CreateJobHeaderFromDifferentCompany()
		{
			JASJob header = Factory.NewJobForTesting<JASJob>();
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_ParentID = Shipment.PK;
			ZQuery filter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(filter);
			header.JH_GC = otherCompany.PK;
			return header;
		}

		#endregion
		#region Vessel
		public void TestVesselDetails()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.VesselName);
			AssertEquals(ZString.Empty, HouseBillOfLading.VesselCountryCode);
			AssertEquals(ZString.Empty, HouseBillOfLading.VesselLloydsCode);
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "___Test Vessel__";
			vessel.RV_LloydsNumber = "LL123";
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = vessel.RV_Code;
			AssertEquals("___Test Vessel__", HouseBillOfLading.VesselName);
			AssertEquals("LL123", HouseBillOfLading.VesselLloydsCode);
			AssertEquals("Should still be empty since the RV_RN_NKCountryOfReg is not yet populated", ZString.Empty, HouseBillOfLading.VesselCountryCode);
			transport.Vessel.RV_RN_NKCountryOfReg = "ES";
			AssertEquals("ES", HouseBillOfLading.VesselCountryCode);
		}

		public void TestVessel()
		{
			AssertNull(HouseBillOfLading.Vessel);
			RefVessel vessel1 = Factory.LoadTop1<RefVessel>(new ZQuery());
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = vessel1.RV_Code;
			AssertEquals(vessel1, HouseBillOfLading.Vessel);
		}

		public void TestVessel_PreShipment()
		{
			AssertNull("Should not throw exception when consol is null", PreShipmentHouseBillOfLading.Vessel);
			AssertEquals("Should not throw exception when consol is null", "", PreShipmentHouseBillOfLading.VesselCountryCode);
			AssertEquals("Should not throw exception when consol is null", "", PreShipmentHouseBillOfLading.VesselLloydsCode);
			AssertEquals("Should not throw exception when consol is null", "", PreShipmentHouseBillOfLading.VesselName);
		}

		#endregion
		#region Voyage
		public void TestVoyageNumber()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.VoyageNumber);
			Transport transport = Consol.Transports[0];
			transport.JW_VoyageFlight = "VOY10098";
			AssertEquals("VOY10098", HouseBillOfLading.VoyageNumber);
		}

		public void TestVoyageNumber_PreShipment()
		{
			AssertEquals("Should not throw exception when consol is null", "", PreShipmentHouseBillOfLading.VoyageNumber);
		}

		#endregion
		#region OnBoardDate
		public void TestOnBoardDate()
		{
			AssertEquals(ZDateTime.Empty, HouseBillOfLading.OnBoardDate);
			Shipment.JS_ShippedOnBoardDate = new ZDateTime(2004, 12, 12);
			AssertEquals(new ZDateTime(2004, 12, 12), HouseBillOfLading.OnBoardDate);
		}

		#endregion
		#region Special Instructions
		public void TestSpecialInstructions()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.SpecialInstructions);
			StmNote note1 = Shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;
			note1.ST_NoteText = "Blabla";
			StmNote note2 = Shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			note2.ST_NoteText = "Special Information";
			AssertEquals("Special Information", HouseBillOfLading.SpecialInstructions);
		}

		#endregion
		#region Handling Instructions
		public void TestHandlingInstructions()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.HandlingInstructions);
			StmNote note1 = Shipment.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			note1.ST_NoteText = "Blabla";
			StmNote note2 = Shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note2.ST_NoteText = "Handling INstruction";
			AssertEquals("Handling INstruction", HouseBillOfLading.HandlingInstructions);
		}

		#endregion
		#region FreightInfo
		public void TestNoOfPackages()
		{
			AssertEquals(ZInt.Zero, HouseBillOfLading.TotalNoOfPackages);
			Shipment.JS_OuterPacks = 45;
			AssertEquals(45, HouseBillOfLading.TotalNoOfPackages);
		}

		public void TestPackageType()
		{
			AssertEquals(FreightPacksDataRegistry.Instance.OuterPackUnit.Value, HouseBillOfLading.PackageType);
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			AssertEquals(Core.Constants.PkgUnit.Bag, HouseBillOfLading.PackageType);
		}

		public void TestTotalNoOfPackagesAndUnitInWords()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.TotalNoOfPackagesAndUnitInWords);
			Shipment.JS_OuterPacks = 23;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			AssertEquals("TWENTY THREE BAG(S)", HouseBillOfLading.TotalNoOfPackagesAndUnitInWords);
			Shipment.JS_OuterPacks = 1;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			AssertEquals("ONE CARTON(S)", HouseBillOfLading.TotalNoOfPackagesAndUnitInWords);
		}

		public void TestGrossWeightInKilograms()
		{
			AssertEquals(ZDecimal.Zero, HouseBillOfLading.GrossWeightInKilograms);
			Shipment.JS_ActualWeight = 87.89M;
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AssertEquals(87.89M, HouseBillOfLading.GrossWeightInKilograms);
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals(39.866233M, HouseBillOfLading.GrossWeightInKilograms);
		}

		public void TestMeasurementInCubicMetres()
		{
			AssertEquals(ZDecimal.Zero, HouseBillOfLading.MeasurementInCubicMetres);
			Shipment.JS_ActualVolume = 100;
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			AssertEquals(100M, HouseBillOfLading.MeasurementInCubicMetres);
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals(2.831685M, HouseBillOfLading.MeasurementInCubicMetres);
		}

		public void TestDescriptionOfPackagesAndGoods()
		{
			AssertEquals(ZString.Empty, HouseBillOfLading.GoodsDescription);
			Shipment.JS_GoodsDescription = "Test";
			AssertEquals("Test", HouseBillOfLading.GoodsDescription);
			Shipment.DetailedGoodsDescriptionNoteText = "Testing123 345";
			AssertEquals("Testing123 345", HouseBillOfLading.GoodsDescription);
		}

		public void TestHarmonisedCommodityCode()
		{
			Shipment.OuterPackLines.RemoveAll();
			AssertEquals(ZString.Empty, HouseBillOfLading.HarmonisedCommodityCode);
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "1000";
			AssertEquals("1000", HouseBillOfLading.HarmonisedCommodityCode);
			PackLine packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "2929";
			AssertEquals("Should still retrieve the value from the first packline", "1000", HouseBillOfLading.HarmonisedCommodityCode);
		}

		public void TestNoOfContainers()
		{
			Consol.Containers.RemoveAll();
			AssertEquals(0, HouseBillOfLading.NoOfContainers);
			AddContainer();
			AssertEquals(1, HouseBillOfLading.NoOfContainers);
			AddContainer();
			AssertEquals(2, HouseBillOfLading.NoOfContainers);
		}

		[TestDate(2004, 10, 10)]
		public void TestFreightRate()
		{
			Shipment.JS_UnitFreightRate = 35.2m;
			AssertEquals(35.2m, HouseBillOfLading.FreightRate);
			RefCurrency uSCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			Shipment.JS_RX_NKFrtRateCurrency = uSCurrency.RX_Code;
			AssertEquals(35.2m, HouseBillOfLading.FreightRate);
			RefExchangeRate exchangeRate = uSCurrency.ExchangeRates.AddNew();
			exchangeRate.RE_SellRate = 2;
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_StartDate = new ZDateTime(2004, 10, 1);
			exchangeRate.RE_ExpiryDate = new ZDateTime(2004, 10, 9);
			AssertEquals(35.2m, HouseBillOfLading.FreightRate);
			exchangeRate.RE_StartDate = new ZDateTime(2004, 10, 09);
			exchangeRate.RE_ExpiryDate = new ZDateTime(2004, 10, 14);
			AssertEquals(35.2m, HouseBillOfLading.FreightRate);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			AssertEquals(70.4m, HouseBillOfLading.FreightRate);
		}

		void AddContainer()
		{
			CommonContainer container = Consol.Containers.AddNew();
			PackLine outerPack = Shipment.OuterPackLines.AddNew();
			outerPack.SetContainer(Consol, container);
		}

		#endregion
		#region Date and Place of Issue
		public void TestDateOfIssue()
		{
			Shipment.JS_HouseBillIssueDate = new ZDateTime(2005, 5, 23);
			AssertEquals(Shipment.JS_HouseBillIssueDate, HouseBillOfLading.DateOfIssue);
		}

		public void TestPlaceOfIssue()
		{
			Consol.JK_RL_NKLoadPort = "";
			AssertEquals("", HouseBillOfLading.PlaceOfIssue);
			Consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("Sydney, Australia", HouseBillOfLading.PlaceOfIssue);
			Consol.LoadPort.RL_RN_NKCountryCode = ZString.Empty;
			AssertEquals("Sydney", HouseBillOfLading.PlaceOfIssue);
		}

		#endregion
		#region Delivery Instructions
		public void TestDeliveryInstructions()
		{
			SetPredefinedNoteText(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, null);
			AssertEquals(ZString.Empty, HouseBillOfLading.DeliveryInstructions);
			SetPredefinedNoteText(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, "Delivery Instruction 123");
			AssertEquals("Delivery Instruction 123", HouseBillOfLading.DeliveryInstructions);
		}

		void SetPredefinedNoteText(PredefinedNoteType noteType, ZString text)
		{
			StmNote[] notes = Shipment.Notes.FindByDescription(noteType.Description);
			if (text.IsEmpty && notes.Length > 0)
			{
				Shipment.Notes.RemoveAndDeleteAll();
			}
			else
			{
				if (notes.Length == 0)
				{
					Shipment.Notes.AddNew(false, noteType.Description, text);
				}
				else
				{
					notes[0].ST_NoteText = text;
				}
			}
		}

		#endregion
		#region MarksAndNumbers
		public void TestMarksAndNumbers()
		{
			SetPredefinedNoteText(PredefinedNoteTypes.Instance.MarksAndNumbers, null);
			AssertEquals(ZString.Empty, HouseBillOfLading.MarksAndNumbers);
			SetPredefinedNoteText(PredefinedNoteTypes.Instance.MarksAndNumbers, "Test marks 123");
			AssertEquals("Test marks 123", HouseBillOfLading.MarksAndNumbers);
		}

		#endregion
		#region Payable By
		public void TestPayableBy()
		{
			Shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Prepaid);
			AssertEquals("ORIGIN", HouseBillOfLading.FreightPayableBy);
			Shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(Core.Constants.PaymentType.Collect);
			AssertEquals("DESTINATION", HouseBillOfLading.FreightPayableBy);
		}

		#endregion
		#region NoOfBillOfLadings
		public void TestNoOfBillOfLadings()
		{
			Shipment.JS_NoOriginalBills = 0;
			AssertEquals(ZByte.Zero, HouseBillOfLading.NoOfBillOfLadings);
			Shipment.JS_NoOriginalBills = 12;
			AssertEquals((ZByte)12, HouseBillOfLading.NoOfBillOfLadings);
		}

		#endregion
		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
		}

		ExportHouseBillOfLadingForTest HouseBillOfLading
		{
			get
			{
				if (fHouseBillOfLading == null)
				{
					fHouseBillOfLading = new ExportHouseBillOfLadingForTest(Consol, Shipment);
				}

				return fHouseBillOfLading;
			}
		}

		ExportHouseBillOfLadingForTest PreShipmentHouseBillOfLading
		{
			get
			{
				if (fPreShipmentHouseBillOfLading == null)
				{
					fPreShipmentHouseBillOfLading = new ExportHouseBillOfLadingForTest(PreShipmentWrapper, Shipment);
				}

				return fPreShipmentHouseBillOfLading;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = (JASForwardingShipment)Consol.Shipments.AddNew();
				}

				return fShipment;
			}
		}

		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		PreShipmentWrapper PreShipmentWrapper
		{
			get
			{
				if (fPreShipmentWrapper == null)
				{
					fPreShipmentWrapper = new PreShipmentWrapper(Shipment);
				}

				return fPreShipmentWrapper;
			}
		}

		JASForwardingConsol fConsol;
		JASForwardingShipment fShipment;
		ExportHouseBillOfLadingForTest fHouseBillOfLading;
		ExportHouseBillOfLadingForTest fPreShipmentHouseBillOfLading;
		PreShipmentWrapper fPreShipmentWrapper;
		#region HouseBillOfLadingForTest
		class ExportHouseBillOfLadingForTest : ExportHouseBillOfLading
		{
			public ExportHouseBillOfLadingForTest(IJXCExportHeader headerData, JASForwardingShipment shipment) : base(headerData, shipment)
			{
			}

			public OrgHeader DeliveryAgent
			{
				get
				{
					if (fDeliveryAgentProperty == null)
					{
						fDeliveryAgentProperty = typeof(ExportHouseBillOfLading).GetProperty("DeliveryAgent", BindingFlags.NonPublic | BindingFlags.Instance);
					}

					return (OrgHeader)fDeliveryAgentProperty.GetValue(this, null);
				}
			}

			public RefVessel Vessel
			{
				get
				{
					if (fVesselProperty == null)
					{
						fVesselProperty = typeof(ExportHouseBillOfLading).GetProperty("Vessel", BindingFlags.Instance | BindingFlags.NonPublic);
					}

					return (RefVessel)fVesselProperty.GetValue(this, null);
				}
			}

			PropertyInfo fDeliveryAgentProperty;
			PropertyInfo fVesselProperty;
		}
		#endregion
		#endregion
	}
}
