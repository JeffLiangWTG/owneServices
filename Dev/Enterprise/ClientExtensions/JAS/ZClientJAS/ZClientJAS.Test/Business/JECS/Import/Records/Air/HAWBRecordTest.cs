using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class HAWBRecordTest : AWBRecordTestCase
	{
		#region TestLoadOrCreateShipment
		public void TestLoadOrCreateShipment_FromFactory()
		{
			SetupTestDataForTestLoadOrCreateShipment();
			SetFieldValuesForTestLoadOrCreateShipment("HB2", "555", "222", new ZDateTime(2006, 1, 2));
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingShipment shipment = record.LoadOrCreateShipment(Factory);
			Assert("Should not match, should create a new one", shipment.JS_BookingReference != "MATCHED");
			AssertShipmentDefaultValues(shipment);
			SetFieldValuesForTestLoadOrCreateShipment("HB2", "777", "603", new ZDateTime(2006, 1, 3));
			record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			shipment = record.LoadOrCreateShipment(Factory);
			Assert("Should not match, should create a new one", shipment.JS_BookingReference != "MATCHED");
			AssertShipmentDefaultValues(shipment);
			SetFieldValuesForTestLoadOrCreateShipment("HB3", "555", "333", new ZDateTime(2006, 1, 4));
			record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			shipment = record.LoadOrCreateShipment(Factory);
			AssertEquals("Should match", "MATCHED", shipment.JS_BookingReference);
		}

		public void TestLoadOrCreateShipment_FromConsol()
		{
			SetupTestDataForTestLoadOrCreateShipment();
			JASForwardingConsol consol = new BusinessObjectFactory().New<JASForwardingConsol>();
			SetFieldValuesForTestLoadOrCreateShipment("HB3", "555", "333", new ZDateTime(2006, 1, 4));
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingShipment resultShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			Assert("Should not match, Consol is in a different factory.", resultShipment.JS_BookingReference != "MATCHED");
			AssertShipmentDefaultValues(resultShipment);
			AssertContainAttachingNewShipmentNotification(NotificationBuffer, consol);
			consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment1 = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB101";
			shipment1.JS_RL_NKOrigin = "IDJKT";
			JASForwardingShipment shipment2 = (JASForwardingShipment)consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB102";
			shipment2.JS_RL_NKOrigin = "HKHKG";
			SetFieldValuesForTestLoadOrCreateShipment("HB102", "555", "222", new ZDateTime(2006, 1, 2));
			record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			resultShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			AssertEquals("Should match the housebill regardless of other details if attached to the same consol", shipment2.PK, resultShipment.PK);
		}

		[ExpectNoExceptions]
		public void TestLoadOrCreateShipment_NullParam()
		{
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, "");
			BusinessObjectFactory factory = null;
			AssertNull(record.LoadOrCreateShipment(factory));
			JASForwardingConsol consol = null;
			AssertNull(record.LoadOrCreateShipment(consol, NotificationBuffer));
		}

		public void TestLoadOrCreateShipment_ShouldBeAttachedToTheConsol()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_HouseBill = "HB102";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			SetFieldValuesForTestLoadOrCreateShipment("HB102", "", "", ZDateTime.Empty);
			HAWBRecord record = (HAWBRecord)GetNewRecord(JXCConstants.LineTypes.HAWB, Fields.ConvertToJXCLine());
			JASForwardingShipment resultShipment = record.LoadOrCreateShipment(consol, NotificationBuffer);
			AssertEquals("Should load shipment", shipment.PK, resultShipment.PK);
			Assert("Should be attached to the consol", consol.Shipments.Contains(resultShipment.PK));
			AssertContainAttachingExistingShipmentNotification(NotificationBuffer, consol, shipment);
		}

		void SetFieldValuesForTestLoadOrCreateShipment(ZString hAWBSerialNo, ZString originCityCode, ZString to1st, ZDateTime eTD)
		{
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.HAWBSerialNo, hAWBSerialNo);
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, originCityCode);
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, to1st);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, eTD);
		}

		void SetupTestDataForTestLoadOrCreateShipment()
		{
			SetupPortsForTestLoadOrCreateShipment();
			InsertShipment("HB1", "AU555", "US333", new ZDateTime(2006, 1, 1));
			InsertShipment("HB1", "AU555", "AU222", new ZDateTime(2006, 1, 2));
			InsertShipment("HB2", "AU555", "US603", new ZDateTime(2006, 1, 3));
			InsertShipment("HB3", "AU555", "US333", new ZDateTime(2006, 1, 4));
		}

		void SetupPortsForTestLoadOrCreateShipment()
		{
			InsertUNLOCO("AU555", "555");
			InsertUNLOCO("US333", "333");
			InsertUNLOCO("AU222", "222");
			InsertUNLOCO("US603", "603");
		}

		void InsertShipment(ZString hAWBSerialNo, ZString origin, ZString destination, ZDateTime eTD)
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_BookingReference = "MATCHED";
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_E_DEP = eTD;
			shipment.JS_HouseBill = hAWBSerialNo;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
		}

		void InsertUNLOCO(ZString code, ZString iATACode)
		{
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = code;
			port.RL_IATA = iATACode;
			RefCountry country = RefCountry.LoadFromCountryCode(Factory, code.Left(2));
			if (country != null)
			{
				port.RL_RN_NKCountryCode = country.Code;
			}
		}

		void AssertShipmentDefaultValues(JASForwardingShipment shipment)
		{
			AssertEquals(Core.Constants.TransportModes.Air, shipment.JS_TransportMode);
			AssertEquals(Core.Constants.ContainerModes.Loose, shipment.JS_PackingMode);
			AssertEquals(Core.Constants.ShipmentReleaseTypes.OriginalReq, shipment.JS_ReleaseType);
		}

		#endregion
		#region TestUpdateShipment
		public void TestUpdateShipment()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			SetFieldValuesForTestUpdateShipment();
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			DataImportFlagChanger.LastBizO = null;
			record.UpdateShipment(shipment, NotificationBuffer);
			AssertShipmentPropertiesAfterUpdateShipment(shipment, record);
			AssertEquals(shipment, DataImportFlagChanger.LastBizO);
		}

		public void TestUpdateShipment2()
		{
			JASOrgHeader originOfficeOrg = Factory.NewWithValidTestData<JASOrgHeader>();
			originOfficeOrg.OfficeCode = "USCOR";
			originOfficeOrg.OH_RL_NKClosestPort = "USATL";
			Factory.Save();
			// With alternative values
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			SetFieldValuesForTestUpdateShipment2();
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateShipment(shipment, NotificationBuffer);
			AssertShipmentPropertiesAfterUpdateShipment2(shipment);
		}

		[ExpectNoExceptions]
		public void TestUpdateShipment_ExcessivelyLongStringIsTrimmed()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			SetFieldValuesForTestUpdateShipment_ExcessivelyLongStringIsTrimmed();
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateShipment(shipment, NotificationBuffer);
		}

		public void TestUpdateShipment_AWBHeaderProperties()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Fields.SetFieldValue(ExpectedFieldPositions.SignatureOfCarrierOrAgent, "TESTING123");
			HAWBRecord record = (HAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateShipment(shipment, NotificationBuffer);
			AssertEquals("UpdateShipment() should call UpdateAWBHeader internally", "TESTING123", shipment.AWBHeader.EH_AWBAgentsSignature);
		}

		void SetFieldValuesForTestUpdateShipment()
		{
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.HAWBSerialNo, "ABC 889898 AD");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperName, "SHIPPER");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, "SHP Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress2, "SHP Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCity, "SEATTLE");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperState, "WA");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperPostCode, "90102");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCountryCode, "US");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAccountNo, "SHP ACC");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ShipperPhone, "SHPPHONE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ShipperEmail, "shpemail@email.com");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeName, "CONSIGNEE");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, "CNE Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress2, "CNE Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCity, "SYDNEY");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeState, "NSW");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneePostCode, "2000");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCountryCode, "AU");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAccountNo, "CNE ACC");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneePhone, "CNEPHONE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneeEmail, "cneemail@email.com");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, "H1");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo2, "H2");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo3, "H3");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyName, "NOTIFYNAME");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress1, "NOTIFYADDRESS1");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress2, "NOTIFYADDRESS2");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPlace, "NOTIFYPLACE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPhone, "NOTIFYPHONE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.OriginOfficeCode, "USCOR");
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, "SEA");
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, "SYD");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, "04/11/2004");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, "05/12/2005");
			Fields.SetFieldValue(ExpectedFieldPositions.ChargeCode, "CC");
			Fields.SetFieldValue(ExpectedFieldPositions.DeclaredValue, "100");
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForDeclaredValue, "AUD");
			Fields.SetFieldValue(ExpectedFieldPositions.TotalNoOfPieces, "399");
			Fields.SetFieldValue(ExpectedFieldPositions.TotalGrossWeight, "99.45");
			Fields.SetFieldValue(ExpectedFieldPositions.WeightUnit, "L");
		}

		void AssertShipmentPropertiesAfterUpdateShipment(JASForwardingShipment shipment, HAWBRecord record)
		{
			Assert("Should be set to true", shipment.JS_OverrideWaybillDefaults);
			AssertEquals("ABC 889898 AD", shipment.JS_HouseBill);
			JXCRecord.FindOrCreateTempOrganisationParams shipperParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("SHIPPER");
			AssertShipperOrgValueObjectForTestUpdateShipment(shipperParams.Organisation);
			AssertEquals(OrganisationTypes.Consignor, shipperParams.OrganisationType);
			AssertEquals(shipment, shipperParams.SourceObject);
			AssertEquals(NotificationBuffer, shipperParams.NotificationSubscriber);
			Assert("Should be assigned", !shipment.ConsignorPK.IsEmpty);
			JXCRecord.FindOrCreateTempOrganisationParams consigneeParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CONSIGNEE");
			AssertConsigneeOrgValueObjectForTestUpdateShipment(consigneeParams.Organisation);
			AssertEquals(OrganisationTypes.Consignee, consigneeParams.OrganisationType);
			AssertEquals(shipment, consigneeParams.SourceObject);
			AssertEquals(NotificationBuffer, consigneeParams.NotificationSubscriber);
			Assert("Should be assigned", !shipment.ConsigneePK.IsEmpty);
			StmNote[] handlingInstructionNotes = shipment.Notes.FindByDescription(JASPredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals(1, handlingInstructionNotes.Length);
			AssertEquals("H1\r\nH2\r\nH3", handlingInstructionNotes[0].ST_NoteText);
			AssertEquals("USSEA", shipment.JS_RL_NKOrigin);
			AssertEquals("AUSYD", shipment.JS_RL_NKDestination);
			AssertEquals(new ZDateTime(2004, 11, 4), shipment.JS_E_DEP);
			AssertEquals(new ZDateTime(2005, 12, 5), shipment.JS_E_ARV);
			AssertEquals(Core.Constants.IncoTerms.ExWorks, shipment.JS_INCO);
			AssertEquals(100m, shipment.JS_GoodsValue);
			AssertEquals("AUD", shipment.JS_RX_NKGoodsValueCurr);
			AssertEquals(399, shipment.JS_OuterPacks);
			AssertEquals(99.45m, shipment.JS_ActualWeight);
			AssertEquals(Core.Constants.Weight.Pounds, shipment.JS_UnitOfWeight);
		}

		void AssertShipperOrgValueObjectForTestUpdateShipment(Xsd.Organisation organisation)
		{
			AssertEquals("SHIPPER", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("SHP Address1", mainAddress.AddressLine1);
			AssertEquals("SHP Address2", mainAddress.AddressLine2);
			AssertEquals("SEATTLE", mainAddress.CityOrSuburb);
			AssertEquals("WA", mainAddress.StateOrProvince);
			AssertEquals("90102", mainAddress.PostCode);
			AssertEquals("US", mainAddress.Location.Country);
			Xsd.TelephoneNumber phoneNumber = mainAddress.TelephoneNumbers[0];
			AssertEquals(Xsd.TelephoneNumberNumberType.Business, phoneNumber.NumberType);
			AssertEquals("SHPPHONE", phoneNumber.Value);
			AssertEquals("shpemail@email.com", mainAddress.Email);
		}

		void AssertConsigneeOrgValueObjectForTestUpdateShipment(Xsd.Organisation organisation)
		{
			AssertEquals("CONSIGNEE", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("CNE Address1", mainAddress.AddressLine1);
			AssertEquals("CNE Address2", mainAddress.AddressLine2);
			AssertEquals("SYDNEY", mainAddress.CityOrSuburb);
			AssertEquals("NSW", mainAddress.StateOrProvince);
			AssertEquals("2000", mainAddress.PostCode);
			AssertEquals("AU", mainAddress.Location.Country);
			Xsd.TelephoneNumber phoneNumber = mainAddress.TelephoneNumbers[0];
			AssertEquals(Xsd.TelephoneNumberNumberType.Business, phoneNumber.NumberType);
			AssertEquals("CNEPHONE", phoneNumber.Value);
			AssertEquals("cneemail@email.com", mainAddress.Email);
		}

		void SetFieldValuesForTestUpdateShipment2()
		{
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.OriginOfficeCode, "USCOR");
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, "SYD");
			Fields.SetFieldValue(ExpectedFieldPositions.To3rd, "BNE");
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, "04/11/2004");
			Fields.SetFieldValue(ExpectedFieldPositions.ChargeCode, "PV");
			Fields.SetFieldValue(ExpectedFieldPositions.CustomsValue, "101.2222");
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForCustomsValue, "USD");
			Fields.SetFieldValue(ExpectedFieldPositions.WeightUnit, "K");
		}

		void AssertShipmentPropertiesAfterUpdateShipment2(JASForwardingShipment shipment)
		{
			Assert("Should be set to true", shipment.JS_OverrideWaybillDefaults);
			AssertEquals("USATL", shipment.JS_RL_NKOrigin);
			AssertEquals("AUBNE", shipment.JS_RL_NKDestination);
			AssertEquals(new ZDateTime(2004, 11, 4), shipment.JS_E_DEP);
			AssertEquals(new ZDateTime(2004, 11, 4), shipment.JS_E_ARV);
			AssertEquals(Core.Constants.IncoTerms.CostAndFreight, shipment.JS_INCO);
			AssertEquals(101.2222m, shipment.JS_GoodsValue);
			AssertEquals("USD", shipment.JS_RX_NKGoodsValueCurr);
			AssertEquals(Core.Constants.Weight.Kilograms, shipment.JS_UnitOfWeight);
			AssertEquals("Should not add handling instruction if not specified in the import file", 0, shipment.Notes.FindByDescription(JASPredefinedNoteTypes.Instance.HandlingInstructions.Description).Length);
		}

		void SetFieldValuesForTestUpdateShipment_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('0', 1000);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.HAWBSerialNo, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperName, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCity, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperState, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperPostCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCountryCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAccountNo, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ShipperPhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ShipperEmail, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeName, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCity, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeState, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneePostCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCountryCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAccountNo, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneePhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneeEmail, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPlace, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.OriginOfficeCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.OriginCityCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.To1st, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.FlightDate2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.ChargeCode, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.DeclaredValue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CurrencyCodeForDeclaredValue, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.TotalNoOfPieces, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.TotalGrossWeight, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.WeightUnit, excessivelyLongString);
		}

		#endregion
		#region TestUpdateAWBHeader
		protected override void SetFieldValuesForTestUpdateAWBHeader()
		{
			base.SetFieldValuesForTestUpdateAWBHeader();
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ShipperPhone, "SHPPHONE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneePhone, "CNEPHONE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyName, "NOTIFYNAME");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress1, "NOTIFYADDRESS1");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress2, "NOTIFYADDRESS2");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPlace, "NOTIFYPLACE");
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPhone, "NOTIFYPHONE");
		}

		protected override void AssertAWBHeaderPropertiesAfterUpdateAWBHeader(ExportAWBHeader aWBHeader)
		{
			base.AssertAWBHeaderPropertiesAfterUpdateAWBHeader(aWBHeader);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEPHONE, aWBHeader.EH_ShipperContactCode);
			AssertEquals("SHPPHONE", aWBHeader.EH_ShipperContactDetail);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEPHONE, aWBHeader.EH_ConsigneeContactCode);
			AssertEquals("CNEPHONE", aWBHeader.EH_ConsigneeContactDetail);
			AssertEquals("NOTIFYNAME", aWBHeader.EH_AlsoNotifyName);
			AssertEquals("NOTIFYADDRESS1", aWBHeader.EH_AlsoNotifyAddress);
			AssertEquals("NOTIFYADDRESS2", aWBHeader.EH_AlsoNotifyAddress2);
			AssertEquals("NOTIFYPLACE", aWBHeader.EH_AlsoNotifyPlace);
			AssertEquals(Core.Constants.AWB.ContactCodes.TELEPHONE, aWBHeader.EH_AlsoNotifyContactCode);
			AssertEquals("NOTIFYPHONE", aWBHeader.EH_AlsoNotifyContactDetail);
		}

		protected override void SetFieldValuesForTestUpdateAWBHeader_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('0', 1000);
			base.SetFieldValuesForTestUpdateAWBHeader_ExcessivelyLongStringIsTrimmed();
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ShipperPhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.ConsigneePhone, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyName, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress1, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyAddress2, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPlace, excessivelyLongString);
			Fields.SetFieldValue(JXCConstants.HAWBFieldPositions.AlsoNotifyPartyPhone, excessivelyLongString);
		}

		#endregion
		protected override JXCConstants.AWBFieldPositions ExpectedFieldPositions
		{
			get
			{
				return new JXCConstants.HAWBFieldPositions();
			}
		}

		protected override int ExpectedNumberOfFields
		{
			get
			{
				return JXCConstants.HAWBFieldCount;
			}
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new HAWBRecord(lineType, lineContent);
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.HAWB;
			}
		}

		protected override ZString ExpectedConsolAgentType
		{
			get
			{
				return Core.Constants.AgentType.Agent;
			}
		}
	}
}
