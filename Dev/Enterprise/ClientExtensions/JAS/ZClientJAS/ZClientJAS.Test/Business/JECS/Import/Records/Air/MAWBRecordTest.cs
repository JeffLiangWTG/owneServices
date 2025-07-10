using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class MAWBRecordTest : AWBRecordTestCase
	{
		#region TestPopulateSendingForwarder
		public virtual void TestPopulateSendingForwarder()
		{
			SetFieldValuesForTestPopulateSendingForwarder();
			var consolMock = Factory.NewMoq<JASForwardingConsol>();
			JASForwardingConsol consol = consolMock.Object;
			consolMock.Setup(m => m.JK_OA_SendingForwarderAddress).Returns(Factory.New<OrgHeader>().MainAddress.PK);
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertNull("Sending Forwarder already exist; Should not try to match", record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("SHIPPER"));
			consolMock.Reset();
			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			JXCRecord.FindOrCreateTempOrganisationParams @params = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("SHIPPER");
			AssertOrgValueObjectForTestPopulateSendingForwarder(@params.Organisation);
			AssertEquals(OrganisationTypes.Forwarder, @params.OrganisationType);
			AssertEquals(consol, @params.SourceObject);
			AssertEquals(NotificationBuffer, @params.NotificationSubscriber);
			Assert("Should be assigned to an organisation now", !consol.SendingForwarderPK.IsEmpty);
		}

		public virtual void TestPopulateSendingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			SetFieldValuesForTestPopulateSendingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated();
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(FactoryProvider);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			JXCRecord.FindOrCreateTempOrganisationParams @params = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("SHIPPER");
			AssertOrgValueObjectForTestPopulateSendingForwarder(@params.Organisation);
		}

		protected void SetFieldValuesForTestPopulateSendingForwarder()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperName, "SHIPPER");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, "SHP Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress2, "SHP Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCity, "SEATTLE");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperState, "WA");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperPostCode, "90102");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCountryCode, "US");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAccountNo, "SHP ACC");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USATL");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUSYD");
		}

		void SetFieldValuesForTestPopulateSendingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperName, "SHIPPER");
			Fields.SetFieldValue(JXCConstants.MAWBFieldPositions.ShipperStreetAddress, "SHP Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress2, "SHP Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCity, "SEATTLE");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperState, "WA");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperPostCode, "90102");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperCountryCode, "US");
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAccountNo, "SHP ACC");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USATL");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUSYD");
		}

		void AssertOrgValueObjectForTestPopulateSendingForwarder(Xsd.Organisation organisation)
		{
			AssertEquals("SHIPPER", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("SHP Address1", mainAddress.AddressLine1);
			AssertEquals("SHP Address2", mainAddress.AddressLine2);
			AssertEquals("SEATTLE", mainAddress.CityOrSuburb);
			AssertEquals("WA", mainAddress.StateOrProvince);
			AssertEquals("90102", mainAddress.PostCode);
			AssertEquals("US", mainAddress.Location.Country);
			AssertEquals("USCOR", organisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.UNC, "").Number);
			AssertEquals("USATL", organisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.UOC, "").Number);
		}

		#endregion
		#region TestPopulateReceivingForwarder
		public virtual void TestPopulateReceivingForwarder()
		{
			SetFieldValuesForTestPopulateReceivingForwarder();
			var consolMock = Factory.NewMoq<JASForwardingConsol>();
			JASForwardingConsol consol = consolMock.Object;
			consolMock.Setup(m => m.JK_OA_ReceivingForwarderAddress).Returns(Factory.New<OrgHeader>().MainAddress.PK);
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertNull("ReceivingForwarder already exist; Should not try to match", record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CONSIGNEE"));
			consolMock.Reset();
			consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			JXCRecord.FindOrCreateTempOrganisationParams @params = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CONSIGNEE");
			AssertOrgValueObjectForTestPopulateReceivingForwarder(@params.Organisation);
			AssertEquals(OrganisationTypes.Forwarder, @params.OrganisationType);
			AssertEquals(consol, @params.SourceObject);
			AssertEquals(NotificationBuffer, @params.NotificationSubscriber);
			Assert("Should be assigned to an organisation now", !consol.ReceivingForwarderPK.IsEmpty);
		}

		public virtual void TestPopulateReceivingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			SetFieldValuesForTestPopulateReceivingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated();
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			JXCRecord.FindOrCreateTempOrganisationParams @params = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CONSIGNEE");
			AssertOrgValueObjectForTestPopulateReceivingForwarder(@params.Organisation);
		}

		protected void SetFieldValuesForTestPopulateReceivingForwarder()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeName, "CONSIGNEE");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, "CNE Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress2, "CNE Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCity, "SYDNEY");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeState, "NSW");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneePostCode, "2000");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCountryCode, "AU");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAccountNo, "CNE ACC");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USATL");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUSYD");
		}

		void SetFieldValuesForTestPopulateReceivingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeName, "CONSIGNEE");
			Fields.SetFieldValue(JXCConstants.MAWBFieldPositions.ConsigneeStreetAddress, "CNE Address1");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress2, "CNE Address2");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCity, "SYDNEY");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeState, "NSW");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneePostCode, "2000");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeCountryCode, "AU");
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAccountNo, "CNE ACC");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingNettingCode, "USCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.SendingOfficeCode, "USATL");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestNettingCode, "AUCOR");
			HEADRecordFields.SetFieldValue(JXCConstants.HEADFieldPositions.DestOfficeCode, "AUSYD");
		}

		void AssertOrgValueObjectForTestPopulateReceivingForwarder(Xsd.Organisation organisation)
		{
			AssertEquals("CONSIGNEE", organisation.OrganisationDetails.Name);
			Xsd.OrgAddress mainAddress = organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("CNE Address1", mainAddress.AddressLine1);
			AssertEquals("CNE Address2", mainAddress.AddressLine2);
			AssertEquals("SYDNEY", mainAddress.CityOrSuburb);
			AssertEquals("NSW", mainAddress.StateOrProvince);
			AssertEquals("2000", mainAddress.PostCode);
			AssertEquals("AU", mainAddress.Location.Country);
			AssertEquals("AUCOR", organisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.UNC, "").Number);
			AssertEquals("AUSYD", organisation.OrganisationDetails.RegistrationNumbers.FindRegistrationNumber(Xsd.RegistrationNumberTypes.UOC, "").Number);
		}

		#endregion
		public void TestShouldNotCreateNewHandlingInstructionNoteIfAlreadyExist()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "DESCRIPTION");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, "HANDLING");
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			AssertEquals(1, consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description).Length);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			StmNote[] handlingInstructionNotes = consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Should not be adding a new one", 1, handlingInstructionNotes.Length);
			AssertEquals("HANDLING\r\n\r\n", handlingInstructionNotes[0].ST_NoteText);
		}

		public void TestShouldNotCreateOrEditHandlingInstructionNoteIfEmptyString()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertEquals("Should not add handling instruction if not specified in the import file", 0, consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description).Length);
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "DESCRIPTION");
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			StmNote[] handlingInstructionNotes = consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Should not be editing the existing one if not specified in the import file", 1, handlingInstructionNotes.Length);
			AssertEquals("Should not be editing the existing one if not specified in the import file", "DESCRIPTION", handlingInstructionNotes[0].ST_NoteText);
		}

		public void TestEH_ShipperAddress1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, "ADDRESS1");
			Fields.SetFieldValue(JXCConstants.MAWBFieldPositions.ShipperStreetAddress, "STREETADDRESS");
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateAWBHeader(consol, NotificationBuffer);
			AssertEquals("Should be assigning it from ShipperAddress field if available", "ADDRESS1", consol.AWBHeader.EH_ShipperAddress);
			Fields.SetFieldValue(ExpectedFieldPositions.ShipperAddress1, "");
			record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateAWBHeader(consol, NotificationBuffer);
			AssertEquals("Should be assigning it from ShipperStreetAddress field if ShipperAddress1 is not available", "STREETADDRESS", consol.AWBHeader.EH_ShipperAddress);
		}

		public void TestEH_ConsigneeAddress1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, "ADDRESS1");
			Fields.SetFieldValue(JXCConstants.MAWBFieldPositions.ConsigneeStreetAddress, "STREETADDRESS");
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateAWBHeader(consol, NotificationBuffer);
			AssertEquals("Should be assigning it from ConsigneeAddress field if available", "ADDRESS1", consol.AWBHeader.EH_ConsigneeAddress);
			Fields.SetFieldValue(ExpectedFieldPositions.ConsigneeAddress1, "");
			record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			record.UpdateAWBHeader(consol, NotificationBuffer);
			AssertEquals("Should be assigning it from ConsigneeStreetAddress field if ConsigneeAddress1 is not available", "STREETADDRESS", consol.AWBHeader.EH_ConsigneeAddress);
		}

		public void TestUpdateConsol_MatchCarrierFromAirlinePrefix()
		{
			Fields.SetFieldValue(ExpectedFieldPositions.AirlinePrefix, "081");
			CreateCarrierOrganisationToMatchAirlinePrefix();
			MAWBRecord record = (MAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(FactoryProvider);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertEquals("Should be matched from the airline prefix", "NEWCARRIER", consol.ShippingLine.OH_Code);
		}

		protected override void SetFieldValuesForTestUpdateConsol()
		{
			base.SetFieldValuesForTestUpdateConsol();
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, "handling 1");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo2, "handling 2");
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo3, "handling 3");
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierName, "CARRIER1");
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierAddress1, "CARADDR1");
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierAddress2, "CARADDR2");
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierCity, "CARCITY");
			RemoveCarrierOrganisationThatMatchesAirlinePrefix();
		}

		protected override void AssertConsolPropertiesAfterUpdateConsol(AWBRecord record, JASForwardingConsol consol)
		{
			base.AssertConsolPropertiesAfterUpdateConsol(record, consol);
			AssertEquals("handling 1\r\nhandling 2\r\nhandling 3", consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)[0].ST_NoteText);
			JXCRecord.FindOrCreateTempOrganisationParams carrierParams = record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CARRIER1");
			AssertEquals(OrganisationTypes.Carrier, carrierParams.OrganisationType);
			AssertEquals("CARRIER1", carrierParams.Organisation.OrganisationDetails.Name);
			AssertEquals(consol, carrierParams.SourceObject);
			AssertEquals(NotificationBuffer, carrierParams.NotificationSubscriber);
			Xsd.OrgAddress mainAddress = carrierParams.Organisation.OrganisationDetails.Addresses.GetOrCreateMainAddress();
			AssertEquals("CARADDR1", mainAddress.AddressLine1);
			AssertEquals("CARADDR2", mainAddress.AddressLine2);
			AssertEquals("CARCITY", mainAddress.CityOrSuburb);
			Assert("Should be assigned", !consol.ShippingLinePK.IsEmpty);
		}

		protected override void SetFieldValuesForTestUpdateConsol_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('X', 1000);
			base.SetFieldValuesForTestUpdateConsol_ExcessivelyLongStringIsTrimmed();
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.HandlingInfo1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierName, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierAddress1, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierAddress2, excessivelyLongString);
			Fields.SetFieldValue(ExpectedFieldPositions.CarrierCity, excessivelyLongString);
		}

		protected override ZString ExpectedConsolAgentType
		{
			get
			{
				return Core.Constants.AgentType.Agent;
			}
		}

		protected override JXCConstants.AWBFieldPositions ExpectedFieldPositions
		{
			get
			{
				return new JXCConstants.MAWBFieldPositions();
			}
		}

		protected override int ExpectedNumberOfFields
		{
			get
			{
				return JXCConstants.MAWBFieldCount;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.MAWB;
			}
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new MAWBRecord(lineType, lineContent);
		}

		void CreateCarrierOrganisationToMatchAirlinePrefix()
		{
			JASOrgHeader orgHeader = Factory.New<JASOrgHeader>();
			orgHeader.OH_Code = "NEWCARRIER";
			orgHeader.OH_IsAirLine = true;
			orgHeader.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "081").PK;
			Factory.Save();
		}

		void RemoveCarrierOrganisationThatMatchesAirlinePrefix()
		{
			var airline = RefAirline.LoadFromAirlinePrefix(Factory, "081");
			ZQuery filter = new ZQuery(OrgMiscServSchema.OM_RM_Airline, airline.PK);
			OrgMiscServ[] miscServs = (OrgMiscServ[])Factory.Load(typeof(OrgMiscServ), filter);
			foreach (OrgMiscServ miscServ in miscServs)
			{
				miscServ.Delete();
			}
		}
	}
}
