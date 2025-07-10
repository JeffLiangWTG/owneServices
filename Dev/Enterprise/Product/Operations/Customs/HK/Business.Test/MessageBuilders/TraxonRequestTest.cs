using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.HK.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.HK.Traxon.MessageBuilders.Testing
{
	class TraxonRequestTest : TestCaseWithFactory
	{
		public void TestGenerateSingleShipmentOriginalImportAir_NullConsignee()
		{
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong }))
			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ForwardingConsol testConsolImports = ImportJobSetup();
				testConsolImports.Shipments[0].ConsigneePK = ZGuid.Empty;
				var shipment = testConsolImports.Shipments[0];
				shipment.JS_HouseBill = CommonShipment.PreAllocatedHouseBillPrefix + "1234567890";
				CusEntryNumber importLicense = testConsolImports.Shipments[0].CusEntryNumbers.AddNew();
				importLicense.CE_EntryNum = ZString.Empty;
				importLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ImportLicense;
				ZString originalMessageImport = GetConfiguredTraxonRequest(testConsolImports);
				AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest Failed", TestOriginalMessageImport_NullConsignee.Replace("\r\n\t\t\t", ""), originalMessageImport, '\'');
			}
		}

		public void TestGenerateSingleShipmentOriginalExportAir()
		{
			var testConsolExports = ExportJobSetup();
			var shipment = testConsolExports.Shipments[0];
			var consignee = shipment.Consignee;
			consignee.MainAddress.OA_Fax = "61280012101";
			var shipper = shipment.Consignor;
			shipper.MainAddress.OA_Phone = "61280012102";
			var exportLicense = shipment.CusEntryNumbers.AddNew();
			exportLicense.CE_EntryNum = "3B003571077FDC";
			exportLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
			var originalMessageExport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest Failed", TestOriginalMessageExport.Replace("\r\n\t\t\t", ""), originalMessageExport, '\'');
		}

		public void TestGenerateSingleShipmentOriginalwithManyPermitsExportAir()
		{
			ForwardingConsol testConsolExports = ExportJobSetup();
			CusEntryNumber exportLicense = testConsolExports.Shipments[0].CusEntryNumbers.AddNew();
			exportLicense.CE_EntryNum = " ,2131231231 1242141241 12341242141";
			exportLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
			ZString originalMessageWithManyPermitsExport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest with many permits Failed", TestWithManyPermitNumbersExport.Replace("\r\n\t\t\t", ""), originalMessageWithManyPermitsExport, '\'');
		}

		public void TestGenerateSingleShipmentOriginalwithManyJobsExportAir()
		{
			ForwardingConsol testConsolExports = ManyExportJobSetup();
			ZString originalMessageWithManyJobsExport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest with many permits Failed", TestWithManyJobsExport.Replace("\r\n\t\t\t", ""), originalMessageWithManyJobsExport, '\'');
		}

		public void TestGenerateSingleShipmentOriginalImportAir()
		{
			ForwardingConsol testConsolImports = ImportJobSetup();
			var shipment = testConsolImports.Shipments[0];
			shipment.JS_HouseBill = CommonShipment.PreAllocatedHouseBillPrefix + "1234567890";
			CusEntryNumber importLicense = testConsolImports.Shipments[0].CusEntryNumbers.AddNew();
			importLicense.CE_EntryNum = ZString.Empty;
			importLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ImportLicense;
			ZString originalMessageImport = GetConfiguredTraxonRequest(testConsolImports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest Failed", TestOriginalMessageImport.Replace("\r\n\t\t\t", ""), originalMessageImport, '\'');
		}

		public void TestGenerateBuyerConsolLeadImportAir()
		{
			ForwardingConsol testConsolImports = ImportJobSetupWithBCN();
			CusEntryNumber importLicense = testConsolImports.Shipments[0].CusEntryNumbers.AddNew();
			importLicense.CE_EntryNum = ZString.Empty;
			importLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ImportLicense;
			ZString originalMessageImport = GetConfiguredTraxonRequest(testConsolImports);
			AssertMultilineEquals("BCN: all subshipments are included", TestOriginalMessageImportBCN.Replace("\r\n\t\t\t", ""), originalMessageImport, '\'');
		}

		public void TestGenerateSingleShipmentOriginalwithManyPermitsImportAir()
		{
			ForwardingConsol testConsolImports = ImportJobSetup();
			CusEntryNumber importLicense = testConsolImports.Shipments[0].CusEntryNumbers.AddNew();
			importLicense.CE_EntryNum = "2131231231, 1242141241, 12341242141";
			importLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ImportLicense;
			ZString originalMessageWithManyPermitsImport = GetConfiguredTraxonRequest(testConsolImports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest with many permits Failed", TestWithManyPermitNumbersImport.Replace("\r\n\t\t\t", ""), originalMessageWithManyPermitsImport, '\'');
		}

		public void TestGenerateSingleShipmentOriginalwithManyJobsImportAir()
		{
			ForwardingConsol testConsolExports = ManyImportJobSetup();
			ZString originalMessageWithManyJobsImport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest with many permits Failed", TestWithManyJobsImport.Replace("\r\n\t\t\t", ""), originalMessageWithManyJobsImport, '\'');
		}

		public void TestSingleMessageGeneratedForNShipments()
		{
			var testConsolExports = ManyExportJobSetup();
			while (testConsolExports.ShipmentCount < 20)
			{
				AddLastShipment(testConsolExports);
			}
			AddLastShipment(testConsolExports);//One of the ManyExport Shipments is a coload and does not get added to the list of shipments going to Traxon

			var traxonMessages = Factory.Load<TraxonMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, testConsolExports.PK));
			AssertEquals("Pre-Condition, There are no messages to begin with", 0, traxonMessages.Length);
			var consolStatus = new TraxonConsolStatus(testConsolExports);
			var generator = new TraxonMessageGenerator(consolStatus, 20);
			generator.GenerateSendMessages();
			traxonMessages = Factory.Load<TraxonMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, testConsolExports.PK));
			AssertEquals("Should be 1 message generated", 1, traxonMessages.Length);
		}

		public void TestSingleMessageGeneratedForNPlus1Shipments()
		{
			var testConsolExports = ManyExportJobSetup();
			while (testConsolExports.ShipmentCount < 20)
			{
				AddLastShipment(testConsolExports);
			}
			AddLastShipment(testConsolExports);//One of the ManyExport Shipments is a coload and does not get added to the list of shipments going to Traxon
			AddLastShipment(testConsolExports);

			var traxonMessages = Factory.Load<TraxonMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, testConsolExports.PK));
			AssertEquals("Pre-Condition, There are no messages to begin with", 0, traxonMessages.Length);
			var generator = new TraxonMessageGenerator(new TraxonConsolStatus(testConsolExports), 20);
			generator.GenerateSendMessages();
			traxonMessages = Factory.Load<TraxonMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, testConsolExports.PK));
			AssertEquals("Should be 2 message generated", 2, traxonMessages.Length);
			Assert("Replace Type", traxonMessages[0].EM_MessageText.IndexOf("BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'") != -1);
			Assert("Replace Type", traxonMessages[1].EM_MessageText.IndexOf("BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'") != -1);

			Assert("Total Count", traxonMessages[0].EM_MessageText.IndexOf("CNT+10:21'") != -1);
			Assert("Total Count", traxonMessages[1].EM_MessageText.IndexOf("CNT+10:21'") != -1);
		}

		public void TestCancelSingleMessage()
		{
			ForwardingConsol testConsolExports = ExportJobSetup();
			ZString cancelMessage = GetConfiguredTraxonRequest(testConsolExports, true);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest with many permits Failed", TestCancelImport.Replace("\r\n\t\t\t", ""), cancelMessage, '\'');
		}

		public void TestGoodsDescriptionIsOnly30CharactersInLength()
		{
			var testConsolExports = ExportJobSetup();
			var shipment = testConsolExports.Shipments[0];
			shipment.JS_GoodsDescription = "3 CARTONS STC \"HOMETIME\" SAMPL";
			var consignee = shipment.Consignee;
			consignee.MainAddress.OA_Fax = "61280012101";
			var shipper = shipment.Consignor;
			shipper.MainAddress.OA_Phone = "61280012102";
			var exportLicense = shipment.CusEntryNumbers.AddNew();
			exportLicense.CE_EntryNum = "3B003571077FDC";
			exportLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
			var originalMessageExport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Generate a Single SCA Housebill Orginal Manifest Failed due to Goods Description length.", TestOriginalMessageExport.Replace("\r\n\t\t\t", ""), originalMessageExport, '\'');
		}

		public void TestPackageCountAlsoComesFromOuterPacks()
		{
			ForwardingConsol testConsolExports = ExportJobSetup();
			testConsolExports.Shipments[0].JS_TotalPackageCount = 1;
			testConsolExports.Shipments[0].JS_OuterPacks = 3;
			var awb = testConsolExports.Shipments[0].AWBHeader;
			awb.EH_ShippingLoadAndCount = 4;

			CusEntryNumber exportLicense = testConsolExports.Shipments[0].CusEntryNumbers.AddNew();
			exportLicense.CE_EntryNum = "3B003571077FDC";
			exportLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
			ZString originalMessageExport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Traxon Package count failed.", TestOriginalMessageExport2.Replace("\r\n\t\t\t", ""), originalMessageExport, '\'');
		}

		public void TestPPDAndCOLMonetaryAmounts()
		{
			ForwardingConsol testConsolExports = ExportJobSetup();
			AddAWBWithMonetaryAmounts(testConsolExports.Shipments[0]);
			ZString originalMessageExport = GetConfiguredTraxonRequest(testConsolExports);
			AssertMultilineEquals("Test AWB amounts failed.", TestMonetaryAmountsOriginal.Replace("\r\n\t\t\t", ""), originalMessageExport, '\'');
		}

		public void TestContactDetails()
		{
			ForwardingConsol testConsolExports = ExportJobSetup();
			testConsolExports.JK_RL_NKDischargePort = "USCHI";
			testConsolExports.Shipments[0].JS_RL_NKDestination = "USCHI";
			AddAWBWithContactDetails(testConsolExports.Shipments[0]);
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				ZString originalMessageExport = GetConfiguredTraxonRequest(testConsolExports);
				AssertMultilineEquals("Test AWB amounts failed.", TestContactDetailsOriginal.Replace("\r\n\t\t\t", ""), originalMessageExport, '\'');
			}
		}

		public void TestContactDetailsPhoneFormat()
		{
			var testConsolExports = ExportJobSetup();
			testConsolExports.JK_RL_NKDischargePort = "USCHI";
			var shipment = testConsolExports.Shipments[0];
			shipment.JS_RL_NKDestination = "USCHI";
			AddAWBWithContactDetails(shipment);

			var notifyParty = shipment.NotifyPartyDocumentaryAddress;
			notifyParty.E2_AddressOverride = true;
			notifyParty.E2_CompanyName = "NOTIFY NAME";
			notifyParty.E2_Address1 = "TEST NOTIFY ADDRESS 1";
			notifyParty.E2_Address2 = "TEST NOTIFY ADDRESS 2";
			notifyParty.E2_City = "TEST NOTIFY CITY";
			notifyParty.E2_Postcode = "9999";
			notifyParty.E2_RN_NKCountryCode = "HK";
			notifyParty.E2_Phone = "+1 415-637-5009";
			notifyParty.E2_Fax = "+1 234-567-8901";

			GetConfiguredTraxonRequest(testConsolExports);
			var messageText = testConsolExports.Messages[0].EM_MessageText;
			AssertContains("Consignee contact is formatted phone", "NAD+CN++TE 0889322740+", messageText);
			AssertContains("Consignor contact is formatted phone", "NAD+CZ++TE 0889322345+", messageText);
			AssertContains("NotifyParty contact is formatted phone", "NAD+NI++TE 14156375009+", messageText);

			shipment.AWBHeader.EH_ConsigneeContactCode = "FX";
			shipment.AWBHeader.EH_ConsigneeContactDetail = "08 8765-2740";
			shipment.AWBHeader.EH_ShipperContactCode = "FX";
			shipment.AWBHeader.EH_ShipperContactDetail = "08 8888-2345";
			notifyParty.E2_Phone = "";

			GetConfiguredTraxonRequest(testConsolExports);
			messageText = testConsolExports.Messages[1].EM_MessageText;
			AssertContains("Consignee contact is formatted fax when no phone", "NAD+CN++FX 0887652740+", messageText);
			AssertContains("Consignor contact is formatted fax when no phone", "NAD+CZ++FX 0888882345+", messageText);
			AssertContains("NotifyParty contact is formatted fax when no phone", "NAD+NI++FX 12345678901+", messageText);
		}

		public void TestReplaceUNLOCOWithIATACode()
		{
			#region Setup IATA

			var hkUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "HKHKG"));
			if (hkUNLOCO == null)
			{
				hkUNLOCO = Factory.New<RefUNLOCO>();
				hkUNLOCO.RL_Code = "HKHKG";
			}
			hkUNLOCO.RL_IATA = "INC";

			var auUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));
			if (auUNLOCO == null)
			{
				auUNLOCO = Factory.New<RefUNLOCO>();
				auUNLOCO.RL_Code = "AUBNE";
			}
			auUNLOCO.RL_IATA = "TST";

			#endregion

			var consol = ExportJobSetup();
			var iATACodeFoundMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("IATA code will be used in LOC segment.", TestReplaceUNLOCOWithIATAMessage.Replace("\r\n\t\t\t", ""), iATACodeFoundMessage, '\'');

			consol.Messages.RemoveAndDeleteAll();
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "HKINC";
			transport.JW_RL_NKDiscPort = "AUTST";
			consol.JK_RL_NKDischargePort = "AUTST";
			var noMatchingUNLOCOMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("No matching UNLOCO exists, last 3 characters of UNLOCO will be used.", TestReplaceUNLOCOWithIATAMessage.Replace("\r\n\t\t\t", ""), noMatchingUNLOCOMessage, '\'');

			consol.Messages.RemoveAndDeleteAll();
			hkUNLOCO.RL_IATA = ZString.Empty;
			auUNLOCO.RL_IATA = ZString.Empty;
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "AUBNE";
			var unlocoMessage = TestReplaceUNLOCOWithIATAMessage.Replace("LOC+5+INC", "LOC+5+HKG").Replace("LOC+8+TST", "LOC+8+BNE");
			var noIATACodeOnMatchingUNLOCOMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("Last 3 characters of UNLOCO will be used if find matching UNLOCO but have no IATA code.", unlocoMessage.Replace("\r\n\t\t\t", ""), noIATACodeOnMatchingUNLOCOMessage, '\'');
		}

		public void TestDestinationPortWithMultipleLegs()
		{
			var consol = ExportJobSetup();
			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "AUTST";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUTST";
			transport2.JW_RL_NKDiscPort = "AUBNE";

			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var multipleTransportsMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("Last 3 characters of UNLOCO will be used if find matching UNLOCO but have no IATA code.", TestDestinationPortMessage.Replace("\r\n\t\t\t", ""), multipleTransportsMessage, '\'');
		}

		public void TestMasterBillDestinationPortShouldDisPortOfAirLeg_WithAirTransportMode()
		{
			var consol = ExportJobSetup();

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "AUTST";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_RL_NKLoadPort = "AUTST";
			transport2.JW_RL_NKDiscPort = "AUBNE";

			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var multipleTransportsMessage = GetConfiguredTraxonRequest(consol);
			AssertContains("When JobConsol.JK_TransportMode = AIR, The value of Master bill DestinationPort should be the last discharge port of air leg. ", "LOC+8+TST", multipleTransportsMessage);
		}

		public void TestHouseBillDestinationPort()
		{
			var consol = ExportJobSetup();

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "AUTST";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_RL_NKLoadPort = "AUTST";
			transport2.JW_RL_NKDiscPort = "AUBNE";

			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var multipleTransportsMessage = GetConfiguredTraxonRequest(consol);
			AssertContains("House bill DestinationPort should be the last discharge port. ", "LOC+8+TST", multipleTransportsMessage);
			AssertContains("House bill DestinationPort should be the last discharge port. ", "LOC+8+BNE", multipleTransportsMessage);
		}

		public void TestISACMessageWithDetailedGoodsDescriptionOnShipment()
		{
			var consol = ImportJobSetup();
			var shipmentOne = consol.Shipments[0];
			shipmentOne.JS_GoodsDescription = "THIS IS SHORT GOODS DESCRIPTION";

			var goodsDescNoteOne = shipmentOne.Notes.AddNew();
			goodsDescNoteOne.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescNoteOne.ST_NoteText = @"ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJ
KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY
ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY1234567890
KKKKKKKKKKKKKKK";

			var shipmentTwo = consol.Shipments.AddNew();
			shipmentTwo.JS_HouseBill = "12150049";
			shipmentTwo.JS_RL_NKOrigin = "AUSYD";
			shipmentTwo.JS_RL_NKDestination = "HKHKG";
			shipmentTwo.JS_GoodsDescription = "THIS IS SHORT DESCRIPTION";
			shipmentTwo.JS_GoodsValue = 213.00m;
			shipmentTwo.JS_RX_NKGoodsValueCurr = "HKD";

			var requestMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("Generate longer Goods Description length 585.", TestISACWithDetailedGoodsDescriptionOnShipmentMessage.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');

			consol.Messages.RemoveAndDeleteAll();
			((IBusinessObjectInternals)goodsDescNoteOne).Row[StmNoteSchema.Constants.ST_NoteText] = goodsDescNoteOne.ST_NoteText.Left(520) + " \r\n";
			requestMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("Generate longer Goods Description with empty space at the end.", TestISACWithDetailedGoodsDescriptionWithEmptySpaceOnShipmentMessage.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');
		}

		public void TestISACMessageWithDetailedGoodsDescriptionWithACINumberOnShipment()
		{
			var consol = ImportJobSetup();
			var shipmentOne = consol.Shipments[0];
			shipmentOne.JS_GoodsDescription = "THIS IS SHORT GOODS DESCRIPTION";
			var number = shipmentOne.Numbers.AddNew();
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number.CE_EntryNum = "1234567890123456789";

			var goodsDescNoteOne = shipmentOne.Notes.AddNew();
			goodsDescNoteOne.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescNoteOne.ST_NoteText = @"ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJ
KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY
ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY1234567890
KKKKKKKKKKKKKKK";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Egypt }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);
				AssertMultilineEquals("Generate longer Goods Description length 520 and ACI number.", TestISACWithDetailedGoodsDescriptionWithACINumberMessage.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');
			}
		}

		public void TestISACMessageWithDetailedGoodsDescriptionWithACINumberOnConsol()
		{
			var consol = ImportJobSetup();
			var shipmentOne = consol.Shipments[0];
			shipmentOne.JS_GoodsDescription = "THIS IS SHORT GOODS DESCRIPTION";
			var number = consol.Numbers.AddNew();
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number.CE_EntryNum = "1234567890123456789";

			var goodsDescNoteOne = shipmentOne.Notes.AddNew();
			goodsDescNoteOne.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescNoteOne.ST_NoteText = @"ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJ
KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY
ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY1234567890
KKKKKKKKKKKKKKK";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Egypt }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);
				AssertMultilineEquals("Generate longer Goods Description length 520 and ACI number.", TestISACWithDetailedGoodsDescriptionWithACINumberMessage.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');
			}
		}

		public void TestOtherCustomsInformation()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var consigneeAddress = shipment1.Consignee.MainAddress;
			consigneeAddress.OA_Phone = "61280012200";
			consigneeAddress.OA_Email = "jane.doe@shadydealings.com";
			var shipperAddress = shipment1.Consignor.MainAddress;
			shipperAddress.OA_Phone = "61280012201";
			shipperAddress.OA_Email = "john.smith@genericcompany.com";

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_ConsigneeCountryCode = "AU";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.ConsigneePK = GetConsigneeAddressSecondExportJob();
			shipment2.ConsignorPK = GetConsignorAddressSecondExportJob();

			var s2consigneeAddress = shipment2.Consignee.MainAddress;
			s2consigneeAddress.OA_Phone = "61280012406";
			s2consigneeAddress.OA_Email = "frednerk@bogus.com";
			var s2shipperAddress = shipment2.Consignor.MainAddress;
			s2shipperAddress.OA_Phone = "61280012408";

			var awbHeader2 = shipment2.AWBHeader;
			awbHeader2.EH_ConsigneeCountryCode = "AU";
			awbHeader2.EH_ConsigneeContactName = string.Empty;
			awbHeader2.EH_ConsigneeContactDetail = "081128521789";
			awbHeader2.EH_ConsigneeTraderNoType = "PAS";
			awbHeader2.EH_ConsigneeTraderNo = string.Empty;

			awbHeader2.EH_ShipperCountryCode = "HK";
			awbHeader2.EH_ShipperContactName = "ShipperName2";
			awbHeader2.EH_ShipperContactDetail = string.Empty;
			awbHeader2.EH_ShipperTraderNoType = "USC";
			awbHeader2.EH_ShipperTraderNo = "R2002410";

			awbHeader2.EH_AlsoNotifyCountryCode = "SG";
			awbHeader2.EH_AlsoNotifyContactName = "AlsoNotifyName2";
			awbHeader2.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader2.EH_AlsoNotifyTraderNoType = "888";
			awbHeader2.EH_AlsoNotifyTraderNo = "12345678";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);
				AssertMultilineEquals("Generate other customs information when these values are valid.", TestSendOtherCustomsInformation.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');
				AssertNotContains("FTX+CUS+1++9999WE02322324:HK:SHP:T", requestMessage);

				consol.Messages.RemoveAndDeleteAll();
				awbHeader1.EH_ShipperTraderNoType = "999";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains("FTX+CUS+1++9999WE02322324:HK:SHP:T", requestMessage);
			}

			// Activate ACAS (for goods going into USA)
			consol.Messages.RemoveAndDeleteAll();
			shipment1.JS_RL_NKDischargePort = "USCHI";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var usRequestMessage = GetConfiguredTraxonRequest(consol);
				AssertMultilineEquals("Generate other customs information when these values are valid.", TestSendOtherCustomsInformation_ACAS.Replace("\r\n\t\t\t", "").Trim(), usRequestMessage, '\'');
			}
		}

		public void TestOtherCustomsInformation_TranshipUSA()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var consigneeAddress = shipment1.Consignee.MainAddress;
			consigneeAddress.OA_Phone = "61280012200";
			consigneeAddress.OA_Email = "jane.doe@shadydealings.com";
			var shipperAddress = shipment1.Consignor.MainAddress;
			shipperAddress.OA_Phone = "61280012201";
			shipperAddress.OA_Email = "john.smith@genericcompany.com";

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_ConsigneeCountryCode = "AU";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			var requestMessage = GetConfiguredTraxonRequest(consol);
			AssertMultilineEquals("Don't generate other customs information when US not in registry", TestSendOtherCustomsInformation_TRANS.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');

			consol.Messages.RemoveAndDeleteAll();
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				requestMessage = GetConfiguredTraxonRequest(consol);
			}
			AssertMultilineEquals("Generate other customs information when these values are valid.", TestSendOtherCustomsInformation_ACAS_TRANS.Replace("\r\n\t\t\t", "").Trim(), requestMessage, '\'');
		}

		public void TestOtherCustomsInformation_ControllingCustomerAccountHolderAndAccountName()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCM";
			controllingCustomer.OH_RL_NKClosestPort = "HKHKG";
			controllingCustomer.OH_FullName = "Customs Agents";
			controllingCustomer.MainAddress.OA_Address1 = "CC MAIN ADDRESS";
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			shipment1.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountHolderAndAccountName_ControllingCustomer.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_PrepaidShipperAccountHolderAndAccountName()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";
			awbHeader1.EH_ShipperName = "company Shipper";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountHolderAndAccountName_Shipper.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_CollectConsigneeAccountHolderAndAccountName()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";
			awbHeader1.EH_ConsigneeName = "Company Consignee";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountHolderAndAccountName_Consignee.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_BothPrepaidAccountHolderAndAccountName()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";
			awbHeader1.EH_ConsigneeName = "Company Consignee";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";
			awbHeader1.EH_ShipperName = "Company Shipper";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountHolderAndAccountName_Shipper.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_BothCollectAccountHolderAndAccountName()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.JS_INCO = Constants.IncoTerms.ExWorks;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";
			awbHeader1.EH_ConsigneeName = "Company Consignee";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";
			awbHeader1.EH_ShipperName = "Company Shipper";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountHolderAndAccountName_Consignee.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_TruncateOtherCustomsInformationEnabled()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var consigneeAddress = shipment1.Consignee.MainAddress;
			consigneeAddress.OA_Phone = "61280012200";
			consigneeAddress.OA_Email = "jane.doe@shadydealings.com";
			var shipperAddress = shipment1.Consignor.MainAddress;
			shipperAddress.OA_Phone = "61280012201";
			shipperAddress.OA_Email = "john.smith@genericcompany.com";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty,
						Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						true))
			using (HKDataRegistry.Instance.TruncateOtherCustomsInformation.SetTemporaryValue(Guid.Empty,
						Guid.Empty, Guid.Empty, true))
			{
				// less than 35 characters
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien Intl";
				var requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTL:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
				consol.Messages.RemoveAndDeleteAll();

				// exact 35 characters
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien International CoLtd";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTERNATIONAL COLTD:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
				consol.Messages.RemoveAndDeleteAll();

				// more than 35 characters, truncate to 35 characters
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien International Import & Export Co., Ltd";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTERNATIONAL IMPOR:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
				consol.Messages.RemoveAndDeleteAll();

				// more than 70 characters, truncate to 35 characters
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien International Import & Export Co., Ltd somewhere in the world";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTERNATIONAL IMPOR:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
			}
		}

		public void TestOtherCustomsInformation_TruncateOtherCustomsInformationDisabled()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var consigneeAddress = shipment1.Consignee.MainAddress;
			consigneeAddress.OA_Phone = "61280012200";
			consigneeAddress.OA_Email = "jane.doe@shadydealings.com";
			var shipperAddress = shipment1.Consignor.MainAddress;
			shipperAddress.OA_Phone = "61280012201";
			shipperAddress.OA_Email = "john.smith@genericcompany.com";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty,
						Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						true))
			using (HKDataRegistry.Instance.TruncateOtherCustomsInformation.SetTemporaryValue(Guid.Empty,
						Guid.Empty, Guid.Empty, false))
			{
				// less than 35 characters
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien Intl";
				var requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTL:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
				consol.Messages.RemoveAndDeleteAll();

				// exact 35 characters
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien International CoLtd";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTERNATIONAL COLTD:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
				consol.Messages.RemoveAndDeleteAll();

				// more than 35 characters, repeat 2 times
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien International Import & Export Co., Ltd";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTERNATIONAL IMPOR:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++T & EXPORT CO., LTD:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
				consol.Messages.RemoveAndDeleteAll();

				// more than 70 characters, repeat 3 times
				shipment1.ConsigneeDocumentaryAddress.E2_Contact = "Guangzhou Shien International Import & Export Co., Ltd somewhere in the world";
				requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains(
					"FTX+CUS+1++GUANGZHOU SHIEN INTERNATIONAL IMPOR:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++T & EXPORT CO., LTD SOMEWHERE IN TH:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++E WORLD:HK:CNE:CP'"
					+ "CST'"
					+ "FTX+CUS+1++TE 61280012200:HK:CNE:CT",
					requestMessage);
			}
		}

		public void TestHSCodeNotInGroup15_When_SendHsCodeIsFalse()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var packline = shipment1.OuterPackLines.AddNew();
			packline.JL_HarmonisedCode = "280140";

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertNotContains("280140", requestMessage);
				}
			}
		}

		public void TestHSCodeInGroup15_When_SendHsCodeIsTrue()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAll();
			var packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "110111";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "220222";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "330333";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "440444";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "55055555";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "660666";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "770777";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "880888";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "990999";

			packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "000000";

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty,
						   Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertContains("FTX+AAA+1++HSCODE/110111/220222/330333/440444/550555/660666/770777/880888", requestMessage);
					AssertContains("FTX+AAA+1++HSCODE/990999/000000", requestMessage);
					AssertNotContains("/55055555", requestMessage);
				}
			}
		}

		public void TestHSCodeInGroup15_When_SendHsCodeMoreThan72()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments[0];
			shipment.JS_GoodsDescription = string.Empty;
			shipment.Notes.RemoveAndDeleteAll();
			shipment.OuterPackLines.RemoveAll();
			for (var i = 0; i < 75; i++)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_HarmonisedCode = "110111";
			}

			var packLineAfter81 = shipment.OuterPackLines.AddNew();
			packLineAfter81.JL_HarmonisedCode = "222222";

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty,
						   Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertContains(@"FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'CST'FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111", requestMessage);
					AssertNotContains("222222", requestMessage);
				}
			}
		}

		public void TestHSCodeInGroup15_When_Has9GroupGoodsDescriptions()
		{
			var consol = ImportJobSetup();
			var shipment = consol.Shipments[0];
			shipment.JS_GoodsDescription = "THIS IS SHORT GOODS DESCRIPTION";

			var goodsDescNoteOne = shipment.Notes.AddNew();
			goodsDescNoteOne.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescNoteOne.ST_NoteText = @"ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJ
KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY
ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY1234567890
KKKKKKKKKKKKKKK";

			for (var i = 0; i < 72; i++)
			{
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_HarmonisedCode = "110111";
			}

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty,
						   Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertContains(@"FTX+AAA+1++ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO'", requestMessage);
					AssertContains(@"FTX+AAA+1++  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABC'", requestMessage);
					AssertContains(@"FTX+AAA+1++DE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOP'", requestMessage);
					AssertContains(@"FTX+AAA+1++QRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCD'", requestMessage);
					AssertContains(@"FTX+AAA+1++EFGHIJ  KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQ'", requestMessage);
					AssertContains(@"FTX+AAA+1++RSTUVWXY  ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE'", requestMessage);
					AssertContains(@"FTX+AAA+1++FGHIJKLMNO  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQR'", requestMessage);
					AssertContains(@"FTX+AAA+1++STUVWXYABCDE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEF'", requestMessage);
					AssertContains(@"FTX+AAA+1++GHIJKLMNOPQRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRS'", requestMessage);
					AssertNotContains(@"FTX+AAA+1++HSCODE/110111/110111/110111/110111/110111/110111/110111/110111'", requestMessage);
				}
			}
		}

		public void TestHSCodeInGroup15_When_HasGoodsDescriptions()
		{
			var consol = ImportJobSetup();
			var shipment = consol.Shipments[0];
			shipment.JS_GoodsDescription = "THIS IS SHORT GOODS DESCRIPTION";

			var goodsDescNoteOne = shipment.Notes.AddNew();
			goodsDescNoteOne.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescNoteOne.ST_NoteText = @"ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRST
UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJ
KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXY
ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO
PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE
FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEF";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "110111";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "220222";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "330333";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "440444";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "55055555";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "660666";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "770777";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "880888";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "990999";

			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "000000";

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty,
						   Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertContains(@"FTX+AAA+1++ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO'", requestMessage);
					AssertContains(@"FTX+AAA+1++  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABC'", requestMessage);
					AssertContains(@"FTX+AAA+1++DE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOP'", requestMessage);
					AssertContains(@"FTX+AAA+1++QRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCD'", requestMessage);
					AssertContains(@"FTX+AAA+1++EFGHIJ  KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQ'", requestMessage);
					AssertContains(@"FTX+AAA+1++RSTUVWXY  ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE'", requestMessage);
					AssertContains(@"FTX+AAA+1++FGHIJKLMNO  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQR'", requestMessage);
					AssertContains(@"FTX+AAA+1++STUVWXYABCDE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEF'", requestMessage);
					AssertContains("FTX+AAA+1++HSCODE/110111/220222/330333/440444/550555/660666/770777/880888", requestMessage);
					AssertNotContains("FTX+AAA+1++HSCODE/99099/000000", requestMessage);
				}
			}
		}

		public void TestOtherCustomsInformation_ControllingCustomerAccountIssuerAndAccountNumber()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCS";
			controllingCustomer.OH_RL_NKClosestPort = "HKHKG";
			controllingCustomer.OH_FullName = "Customs Agents";
			controllingCustomer.MainAddress.OA_Address1 = "CC MAIN ADDRESS";
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			shipment1.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";
			Factory.Save();

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountIssuerAndAccountNumber_ControllingCustomer.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_PrepaidShipperAccountIssuerAndAccountNumber()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";

			awbHeader1.EH_ShipperAccount = "ShipperAccount";
			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";
			awbHeader1.EH_ShipperName = "company Shipper";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountIssuerAndAccountNumber_Shipper.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_CollectConsigneAccountIssuerAndAccountNumber()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";
			awbHeader1.EH_ConsigneeAccount = "ConsigneeAcc";
			awbHeader1.EH_ConsigneeName = "Company Consignee";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountIssuerAndAccountNumber_Consignee.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_BothPrepaidAccountIssuerAndAccountNumber()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";
			awbHeader1.EH_ShipperAccount = "ShipperAccount";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";
			awbHeader1.EH_ShipperName = "Company Shipper";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountIssuerAndAccountNumber_Shipper.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestOtherCustomsInformation_BothCollectAccountIssuerAndAccountNumber()
		{
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			var consol = ExportJobSetup();
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "HKHKG";
			transport1.JW_RL_NKDiscPort = "USLAX";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.JS_INCO = Constants.IncoTerms.ExWorks;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;
			awbHeader1.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			awbHeader1.EH_ConsigneeCountryCode = "US";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactCode = "TE";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "PAS";
			awbHeader1.EH_ConsigneeTraderNo = "WHE2322323203";
			awbHeader1.EH_ConsigneeAccount = "ConsigneeAcc";
			awbHeader1.EH_ConsigneeName = "Company Consignee";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactCode = "TE";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";
			awbHeader1.EH_ShipperName = "Company Shipper";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactCode = "FX";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Constants.CountryGuids.UnitedStates }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);

				AssertMultilineEquals(
					"Generate Account Holder and Account Name when these values are valid.",
					TestSendAccountIssuerAndAccountNumber_Consignee.Replace("\r\n\t\t\t", "").Trim(),
					requestMessage,
					'\''
				);
			}
		}

		public void TestACINumberIfCargoArrivedAtEgyptBefore_WithoutEGConfiguredInRegistry()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var number = shipment1.Numbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number.CE_EntryNum = "1234567890123456789012345678901234567890";
			var number2 = shipment1.Numbers.AddNew();
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number2.CE_EntryNum = "98765432109876543210987654321";
			var number3 = shipment1.Numbers.AddNew();
			number3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			number3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number3.CE_EntryNum = "abcdefghijklmnopqrstuvwxyz";
			var number4 = shipment1.Numbers.AddNew();
			number4.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number4.CE_EntryType = "ACT";
			number4.CE_EntryNum = "abcdefghijklmnopqrstuvwxyz";

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertNotContains("ACI 12345678901234567890123456789012345,9876543210987654321098765", requestMessage);
					AssertNotContains("abcdefghijklmnopqrstuvwxyz", requestMessage);
				}
			}
		}
		public void TestACINumberIfCargoArrivedAtEgyptBefore_WithEGConfiguredInRegistry()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();
			var number = shipment1.Numbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number.CE_EntryNum = "1234567890123456789012345678901234567890";
			var number2 = shipment1.Numbers.AddNew();
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number2.CE_EntryNum = "98765432109876543210987654321";
			var number3 = shipment1.Numbers.AddNew();
			number3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			number3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			number3.CE_EntryNum = "abcdefghijklmnopqrstuvwxyz";
			var number4 = shipment1.Numbers.AddNew();
			number4.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Egypt;
			number4.CE_EntryType = "ACT";
			number4.CE_EntryNum = "abcdefghijklmnopqrstuvwxyz";

			using (HKDataRegistry.Instance.SendHsCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Egypt }))
				{
					var requestMessage = GetConfiguredTraxonRequest(consol);
					AssertContains("ACI 12345678901234567890123456789012345,9876543210987654321098765", requestMessage);
					AssertNotContains("abcdefghijklmnopqrstuvwxyz", requestMessage);
				}
			}
		}

		public void TestEORICompanyNumber()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "FRPAR";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "ACO SEWINGART LTD";
			consignor.MainAddress.OA_Address1 = "RM 833 METRO CENTER 2, 21 LAM HING";
			consignor.MainAddress.OA_Address2 = "ST., KOWLOON BAY";
			consignor.MainAddress.OA_City = "HONG KONG";
			consignor.OH_RL_NKClosestPort = "HKHKG";
			AddCompanyCode(consignor, OrgCusCode.HKCodeTypes.AEO, "339276");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "FRANCOIS HERMES";
			consignee.MainAddress.OA_Address1 = "2215 Rue de la Orangerie";
			consignee.MainAddress.OA_Address2 = "Arrondisment Quatre";
			consignee.MainAddress.OA_City = "Paris";
			consignee.OH_RL_NKClosestPort = "FRPAR";
			AddCompanyCode(consignee, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "57289");

			var exportShipment = consol.Shipments[0];
			exportShipment.ConsignorPK = consignor.PK;
			exportShipment.ConsigneePK = consignee.PK;
			exportShipment.JS_GoodsDescription = string.Empty;
			exportShipment.Notes.RemoveAndDeleteAll();

			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong }))
				{
					using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.France }))
					{
						var requestMessage = GetConfiguredTraxonRequest(consol);
						AssertContains("FTX+CUS+1++FR57289:FR:CNE:T", requestMessage);
						AssertContains("FTX+CUS+1++AEO339276:HK:SHP:T", requestMessage);
						AssertNotContains("no contact name", "Alexander", requestMessage);
						AssertNotContains("no contact phone", "987456321", requestMessage);
					}
				}
			}
		}

		public void TestEORICompanyNumberFormatting()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "FRPAR";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "ACO SEWINGART LTD";
			consignor.MainAddress.OA_Address1 = "RM 833 METRO CENTER 2, 21 LAM HING";
			consignor.MainAddress.OA_Address2 = "ST., KOWLOON BAY";
			consignor.MainAddress.OA_City = "HONG KONG";
			consignor.OH_RL_NKClosestPort = "HKHKG";
			AddCompanyCode(consignor, OrgCusCode.HKCodeTypes.AEO, "339276");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "NINA LAMBOURGHINI";
			consignee.MainAddress.OA_Address1 = "PIAZZA DEL ROMA";
			consignee.MainAddress.OA_Address2 = "MILAN";
			consignee.MainAddress.OA_City = "MILAN";
			consignee.OH_RL_NKClosestPort = "ITMIL";
			AddCompanyCode(consignee, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "8740029");

			var exportShipment = consol.Shipments[0];
			exportShipment.ConsignorPK = consignor.PK;
			exportShipment.ConsigneePK = consignee.PK;
			exportShipment.JS_GoodsDescription = string.Empty;
			exportShipment.Notes.RemoveAndDeleteAll();

			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong }))
				{
					using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.France }))
					{
						var requestMessage = GetConfiguredTraxonRequest(consol);
						AssertContains("EORI number must be in the correct format to send in the message (CountryCode & upto 10 digits) - XX9999999999", "FTX+CUS+1++IT8740029:IT:CNE:T", requestMessage);
					}
				}
			}
		}

		public void TestEORICompanyAlreadyFormattedNumber()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "FRPAR";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "ACO SEWINGART LTD";
			consignor.MainAddress.OA_Address1 = "RM 833 METRO CENTER 2, 21 LAM HING";
			consignor.MainAddress.OA_Address2 = "ST., KOWLOON BAY";
			consignor.MainAddress.OA_City = "HONG KONG";
			consignor.OH_RL_NKClosestPort = "HKHKG";
			AddCompanyCode(consignor, OrgCusCode.HKCodeTypes.AEO, "339276");

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "LINDT CHOCLATAIRES";
			consignee.MainAddress.OA_Address1 = "1520 SWASSENGRUBER AVENUE";
			consignee.MainAddress.OA_Address2 = "LAKE GENEVA";
			consignee.MainAddress.OA_City = "GENEVA";
			consignee.OH_RL_NKClosestPort = "CHGVA";
			AddCompanyCode(consignee, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "CH2386725915");

			var exportShipment = consol.Shipments[0];
			exportShipment.ConsignorPK = consignor.PK;
			exportShipment.ConsigneePK = consignee.PK;
			exportShipment.JS_GoodsDescription = string.Empty;
			exportShipment.Notes.RemoveAndDeleteAll();

			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong }))
				{
					using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.France }))
					{
						var requestMessage = GetConfiguredTraxonRequest(consol);
						AssertContains("EORI number already in the correct format simply sends in the message", "FTX+CUS+1++CH2386725915:CH:CNE:T", requestMessage);
					}
				}
			}
		}

		public void TestEORICompanyNumberFormattingFromAWB()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "ITMIL";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();

			var awbHeader1 = shipment1.AWBHeader;

			awbHeader1.EH_ConsigneeCountryCode = "IT";
			awbHeader1.EH_ConsigneeContactName = "ConsigneeName1";
			awbHeader1.EH_ConsigneeContactDetail = "081123456789";
			awbHeader1.EH_ConsigneeTraderNoType = "EORI No.";
			awbHeader1.EH_ConsigneeTraderNo = "2323203";

			awbHeader1.EH_ShipperCountryCode = "HK";
			awbHeader1.EH_ShipperContactName = "ShipperName1";
			awbHeader1.EH_ShipperContactDetail = "080152045450";
			awbHeader1.EH_ShipperTraderNoType = "XYZ";
			awbHeader1.EH_ShipperTraderNo = "WE02322324";

			awbHeader1.EH_AlsoNotifyCountryCode = "SG";
			awbHeader1.EH_AlsoNotifyContactName = "AlsoNotifyName1";
			awbHeader1.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader1.EH_AlsoNotifyTraderNoType = "ID";
			awbHeader1.EH_AlsoNotifyTraderNo = "2K2323002";

			var shipment2 = consol.Shipments.AddNew();
			var awbHeader2 = shipment2.AWBHeader;

			awbHeader2.EH_ConsigneeCountryCode = "IT";
			awbHeader2.EH_ConsigneeContactName = string.Empty;
			awbHeader2.EH_ConsigneeContactDetail = "081128521789";
			awbHeader2.EH_ConsigneeTraderNoType = "EORI NO.";
			awbHeader2.EH_ConsigneeTraderNo = "IT49587277";

			awbHeader2.EH_ShipperCountryCode = "HK";
			awbHeader2.EH_ShipperContactName = "ShipperName2";
			awbHeader2.EH_ShipperContactDetail = string.Empty;
			awbHeader2.EH_ShipperTraderNoType = "USC";
			awbHeader2.EH_ShipperTraderNo = "R2002410";

			awbHeader2.EH_AlsoNotifyCountryCode = "SG";
			awbHeader2.EH_AlsoNotifyContactName = "AlsoNotifyName2";
			awbHeader2.EH_AlsoNotifyContactDetail = "08012000032";
			awbHeader2.EH_AlsoNotifyTraderNoType = "888";
			awbHeader2.EH_AlsoNotifyTraderNo = "12345678";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Italy }))
			{
				var requestMessage = GetConfiguredTraxonRequest(consol);
				AssertContains("EORI number must be in the correct format to send in the message (CountryCode & upto 10 digits) - XX9999999999", "FTX+CUS+1++IT2323203:IT:CNE:T", requestMessage);

				AssertContains("EORI number already formatted", "FTX+CUS+1++IT49587277:IT:CNE:T", requestMessage);
			}
		}

		public void TestAEOCompanyNumber()
		{
			var consol = ExportJobSetup();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsDescription = string.Empty;
			shipment1.Notes.RemoveAndDeleteAll();

			AddCompanyCode(shipment1.Consignee, OrgCusCode.HKCodeTypes.AEO, "738728");
			AddCompanyCode(shipment1.Consignor, OrgCusCode.HKCodeTypes.AEO, "479855");

			using (HKDataRegistry.Instance.SendInfoFromShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong }))
				{
					using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
					{
						var requestMessage = GetConfiguredTraxonRequest(consol);
						AssertContains("FTX+CUS+1++AEO738728:HK:CNE:T", requestMessage);
						AssertContains("FTX+CUS+1++AEO479855:AU:SHP:T", requestMessage);
					}
				}
			}
		}

		#region Implementation

		protected const string MarksAndNumbers = "HERE ARE SOME TEST MARKS AND NUMBERS BY SCOTT AS I NEED THEM FOR THE TEST MESSAGE. THIS IS THE GREATEST DISCUSSION IN THE WORLD ABOUT THE STATE OF NANOPARTICULES IN A MAGNETIC FIELD AS DESCRIBED BY MAXWELLS EQUATIONS. A ROUND OF APPLAUSE FOR SCOTT R WRIGHT THE WORLD LEADER IN THIS AREA. NOW THAT I HAVE ENOUGH MARKS AND NUMBERS";

		protected override void SetUp()
		{
			base.SetUp();
			HKDataRegistry.Instance.CosacAgentCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1331929");
			storedBranchCode = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "HKHKG";
		}

		ZString storedBranchCode;

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = storedBranchCode;
		}

		protected void AddCompanyCode(OrgHeader company, string codeType, string code)
		{
			company.CustomsCodes.AddNew(codeType, code);
		}

		protected ForwardingConsol ExportJobSetup()
		{
			var testConsolExports = Factory.New<ForwardingConsol>();
			testConsolExports.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsolExports.JK_UniqueConsignRef = "C00003134";

			var transport = testConsolExports.Transports[0];
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "AUBNE";
			testConsolExports.JK_RL_NKLoadPort = "HKHKG";
			testConsolExports.JK_RL_NKDischargePort = "AUBNE";
			testConsolExports.JK_MasterBillNum = "16068974393";
			transport.JW_VoyageFlight = "CX102";
			transport.JW_ETA = new ZDateTime(2000, 12, 23);

			var exportShipment = testConsolExports.Shipments.AddNew();
			((ISupportDataImporting)exportShipment).IsImportingData = true;
			exportShipment.JS_HouseBill = "3134-001";
			exportShipment.JS_OuterPacks = 3;
			exportShipment.JS_ActualWeight = 94.0m;
			exportShipment.ConsigneePK = GetConsigneeAddressSingleExportJob();
			exportShipment.ConsignorPK = GetConsignorAddressSingleExportJob();
			exportShipment.JS_RL_NKOrigin = "HKHKG";
			exportShipment.JS_RL_NKDestination = "AUBNE";
			exportShipment.JS_GoodsDescription = "3 CARTONS STC \"HOMETIME\" SAMPL";
			exportShipment.JS_OverrideWaybillDefaults = true;
			exportShipment.AWBHeader.EH_CustomsValue = 1710.02m;
			exportShipment.AWBHeader.EH_DeclaredValue = 12.34m;
			exportShipment.AWBHeader.EH_Currency = "AUD";
			exportShipment.ConsigneeDocumentaryAddress.E2_Contact = "Alexander";
			exportShipment.ConsigneeDocumentaryAddress.E2_Phone = "987456321";

			var marksAndNumbersNote = exportShipment.Notes.AddNew();
			marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote.ST_NoteText = MarksAndNumbers;

			return testConsolExports;
		}

		protected ForwardingConsol ImportJobSetup()
		{
			ForwardingConsol testConsolImports = Factory.New<ForwardingConsol>();
			testConsolImports.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsolImports.JK_UniqueConsignRef = "C00010924";

			Transport transport = testConsolImports.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF522";
			testConsolImports.MasterBillMAWB = "76152650";
			transport.JW_ETA = new ZDateTime(2001, 02, 04);
			testConsolImports.JK_RL_NKLoadPort = "AUSYD";
			testConsolImports.JK_RL_NKDischargePort = "HKHKG";

			ForwardingShipment importShipment = testConsolImports.Shipments.AddNew();
			((ISupportDataImporting)importShipment).IsImportingData = true;
			importShipment.JS_HouseBill = "12150048";
			importShipment.JS_OuterPacks = 1;
			importShipment.JS_ActualWeight = 112.0m;
			importShipment.ConsigneePK = GetConsigneeAddressSingleImportJob();
			importShipment.ConsignorPK = GetConsignorAddressSingleImportJob();
			importShipment.JS_RL_NKOrigin = "AUSYD";
			importShipment.JS_RL_NKDestination = "HKHKG";
			importShipment.JS_GoodsDescription = "sports bags";
			importShipment.JS_GoodsValue = 213.00m;
			importShipment.JS_RX_NKGoodsValueCurr = "HKD";

			StmNote marksAndNumbersNote = importShipment.Notes.AddNew();
			marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote.ST_NoteText = "Test marks and numbers";

			return testConsolImports;
		}

		ForwardingConsol ImportJobSetupWithBCN()
		{
			ForwardingConsol testConsolImports = Factory.New<ForwardingConsol>();
			testConsolImports.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsolImports.JK_UniqueConsignRef = "C00010924";

			Transport transport = testConsolImports.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF522";
			testConsolImports.MasterBillMAWB = "76152650";
			transport.JW_ETA = new ZDateTime(2001, 02, 04);

			testConsolImports.JK_RL_NKLoadPort = "AUSYD";
			testConsolImports.JK_RL_NKDischargePort = "HKHKG";

			ForwardingShipment importShipment = testConsolImports.Shipments.AddNew();
			((ISupportDataImporting)importShipment).IsImportingData = true;
			importShipment.JS_HouseBill = "12150048";
			importShipment.JS_OuterPacks = 1;
			importShipment.JS_ActualWeight = 112.0m;
			importShipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;//should be converted to KG
			importShipment.ConsigneePK = GetConsigneeAddressSingleImportJob();
			importShipment.ConsignorPK = GetConsignorAddressSingleImportJob();
			importShipment.JS_RL_NKOrigin = "AUSYD";
			importShipment.JS_RL_NKDestination = "HKHKG";
			importShipment.JS_GoodsDescription = "sports bags";
			importShipment.JS_GoodsValue = 213.00m;
			importShipment.JS_RX_NKGoodsValueCurr = "HKD";
			importShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var importShipment2 = testConsolImports.Shipments.AddNew();
			((ISupportDataImporting)importShipment2).IsImportingData = true;
			importShipment2.JS_HouseBill = "3136-002";
			importShipment2.JS_OuterPacks = 1;
			importShipment2.JS_ActualWeight = 15.0m;
			importShipment2.JS_UnitOfWeight = ZString.Empty;//should not cause any system error
			importShipment2.ConsigneePK = GetConsigneeAddressThirdExportJob();
			importShipment2.ConsignorPK = GetConsignorAddressThirdExportJob();
			importShipment2.JS_RL_NKOrigin = "HKHKG";
			importShipment2.JS_RL_NKDestination = "NCNOU";
			importShipment2.JS_GoodsDescription = "MOTORCYCLE PARTS AND ACCESSORI";
			importShipment2.AWBHeader.EH_CustomsValue = 949.65m;
			importShipment2.AWBHeader.EH_Currency = "AUD";
			importShipment2.JS_JS_ColoadMasterShipment = importShipment.PK;

			StmNote marksAndNumbersNote = importShipment.Notes.AddNew();
			marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote.ST_NoteText = "Test marks and numbers";

			return testConsolImports;
		}

		protected ForwardingConsol ManyExportJobSetup()
		{
			ForwardingConsol testConsolManyJobsExports = ExportJobSetup();
			testConsolManyJobsExports.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			CusEntryNumber exportLicense = testConsolManyJobsExports.Shipments[0].CusEntryNumbers.AddNew();
			exportLicense.CE_EntryNum = "12341242141, 3B003571077FDC";
			exportLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;

			ForwardingShipment importShipment2 = testConsolManyJobsExports.Shipments.AddNew();
			((ISupportDataImporting)importShipment2).IsImportingData = true;
			importShipment2.JS_HouseBill = "3135-001";
			importShipment2.JS_OuterPacks = 4;
			importShipment2.JS_ActualWeight = 80.0m;
			importShipment2.ConsigneePK = GetConsigneeAddressSecondExportJob();
			importShipment2.ConsignorPK = GetConsignorAddressSecondExportJob();
			importShipment2.JS_RL_NKOrigin = "HKHKG";
			importShipment2.JS_RL_NKDestination = "SARUH";
			importShipment2.JS_GoodsDescription = "WRIST WATCHES";
			importShipment2.AWBHeader.EH_CustomsValue = 6120.00m;
			importShipment2.AWBHeader.EH_Currency = "AUD";

			CusEntryNumber exportLicense2 = importShipment2.CusEntryNumbers.AddNew();
			exportLicense2.CE_EntryNum = "3B010031233WTC";
			exportLicense2.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;

			ForwardingShipment importShipment3 = testConsolManyJobsExports.Shipments.AddNew();
			((ISupportDataImporting)importShipment3).IsImportingData = true;
			importShipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			importShipment3.JS_HouseBill = "3136-001";
			importShipment3.JS_OuterPacks = 1;
			importShipment3.JS_ActualWeight = 15.0m;
			importShipment3.ConsigneePK = GetConsigneeAddressThirdExportJob();
			importShipment3.ConsignorPK = GetConsignorAddressThirdExportJob();
			importShipment3.JS_RL_NKOrigin = "HKHKG";
			importShipment3.JS_RL_NKDestination = "NCNOU";
			importShipment3.JS_GoodsDescription = "MOTORCYCLE PARTS AND ACCESSORI";
			importShipment3.AWBHeader.EH_CustomsValue = 949.65m;
			importShipment3.AWBHeader.EH_Currency = "AUD";

			var importShipment4 = testConsolManyJobsExports.Shipments.AddNew();
			((ISupportDataImporting)importShipment4).IsImportingData = true;
			importShipment4.JS_HouseBill = "3136-002";
			importShipment4.JS_OuterPacks = 1;
			importShipment4.JS_ActualWeight = 15.0m;
			importShipment4.JS_RL_NKOrigin = "HKHKG";
			importShipment4.JS_RL_NKDestination = "NCNOU";
			importShipment4.JS_GoodsDescription = "MOTORCYCLE PARTS AND ACCESSORI";
			importShipment4.JS_JS_ColoadMasterShipment = importShipment3.PK;

			StmNote marksAndNumbersNote = importShipment3.Notes.AddNew();
			marksAndNumbersNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote.ST_NoteText = "Some Test marks and numbers";

			CusEntryNumber exportLicense3 = importShipment3.CusEntryNumbers.AddNew();
			exportLicense3.CE_EntryNum = "3B010051283SBC";
			exportLicense3.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;

			return testConsolManyJobsExports;
		}

		protected ForwardingConsol ManyImportJobSetup()
		{
			ForwardingConsol testConsolManyJobsImports = Factory.New<ForwardingConsol>();
			testConsolManyJobsImports.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsolManyJobsImports.JK_UniqueConsignRef = "C00010924";

			Transport transport = testConsolManyJobsImports.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_VoyageFlight = "QF522";
			testConsolManyJobsImports.MasterBillMAWB = "76152650";
			transport.JW_ETA = new ZDateTime(2001, 02, 04);
			testConsolManyJobsImports.JK_RL_NKLoadPort = "AUBNE";
			testConsolManyJobsImports.JK_RL_NKDischargePort = "HKHKG";

			ForwardingShipment importShipment = testConsolManyJobsImports.Shipments.AddNew();
			importShipment.JS_HouseBill = "00329";
			importShipment.JS_OuterPacks = 1;
			importShipment.JS_ActualWeight = 26.0m;
			importShipment.ConsigneePK = GetConsigneeAddressFirstImportJob();
			importShipment.ConsignorPK = GetConsignorAddressFirstImportJob();
			importShipment.JS_RL_NKOrigin = "CNSHA";
			importShipment.JS_RL_NKDestination = "HKHKG";
			importShipment.JS_GoodsDescription = "PRINTING EQUIPMENT";
			importShipment.JS_GoodsValue = 1234.00m;
			importShipment.JS_RX_NKGoodsValueCurr = "USD";

			ForwardingShipment importShipment2 = testConsolManyJobsImports.Shipments.AddNew();
			importShipment2.JS_HouseBill = "12150048";
			importShipment2.JS_OuterPacks = 1;
			importShipment2.JS_ActualWeight = 112.0m;
			importShipment2.ConsigneePK = GetConsigneeAddressSingleImportJob();
			importShipment2.ConsignorPK = GetConsignorAddressSingleImportJob();
			importShipment2.JS_RL_NKOrigin = "AUSYD";
			importShipment2.JS_RL_NKDestination = "HKHKG";
			importShipment2.JS_GoodsDescription = "sports bags";
			importShipment2.JS_GoodsValue = 213.00m;
			importShipment2.JS_RX_NKGoodsValueCurr = "HKD";

			StmNote marksAndNumbersNote2 = importShipment2.Notes.AddNew();
			marksAndNumbersNote2.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote2.ST_NoteText = "Test marks and numbers";

			ForwardingShipment importShipment3 = testConsolManyJobsImports.Shipments.AddNew();
			importShipment3.JS_HouseBill = "154927";
			importShipment3.JS_OuterPacks = 11;
			importShipment3.JS_ActualWeight = 31.0m;
			importShipment3.ConsigneePK = GetConsigneeAddressThirdImportJob();
			importShipment3.ConsignorPK = GetConsignorAddressThirdImportJob();
			importShipment3.JS_RL_NKOrigin = "AUBNE";
			importShipment3.JS_RL_NKDestination = "HKHKG";
			importShipment3.JS_GoodsDescription = "WEARING APPAREL";
			importShipment3.JS_GoodsValue = 2131.00m;
			importShipment3.JS_RX_NKGoodsValueCurr = "IDR";

			StmNote marksAndNumbersNote3 = importShipment3.Notes.AddNew();
			marksAndNumbersNote3.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marksAndNumbersNote3.ST_NoteText = "WOW GREAT";

			return testConsolManyJobsImports;
		}

		public void AddLastShipment(ForwardingConsol testConsolManyJobsImports)
		{
			ForwardingShipment importShipment = testConsolManyJobsImports.Shipments.AddNew();
			importShipment.JS_HouseBill = "327598801";
			importShipment.JS_OuterPacks = 234;
			importShipment.JS_ActualWeight = 34.5m;
			importShipment.ConsigneePK = GetConsigneeAddressFirstImportJob();
			importShipment.ConsignorPK = GetConsignorAddressFirstImportJob();
			importShipment.JS_RL_NKOrigin = "HKHKG";
			importShipment.JS_RL_NKDestination = "AUSYD";
			importShipment.JS_GoodsDescription = "SHOULD BE ON ANOTHER MESSAGE";
			importShipment.JS_GoodsValue = 378.34m;
			importShipment.JS_RX_NKGoodsValueCurr = "HKD";
		}

		ZString GetConfiguredTraxonRequest(ForwardingConsol consol, bool isCancel = false)
		{
			var result = new TraxonMessageGenerator(new TraxonConsolStatus(consol));

			if (isCancel)
			{
				result.GenerateWithdrawMessages();
			}
			else
			{
				result.GenerateSendMessages();
			}

			consol.Messages.Load();

			return consol.Messages[0].EM_MessageText;
		}

		void AddAWBWithMonetaryAmounts(ForwardingShipment shipment)
		{
			shipment.JS_OverrideWaybillDefaults = ZBool.True;
			shipment.AWBHeader.EH_ValuationCOL = 10.00m;
			shipment.AWBHeader.EH_ValuationPPD = 10.00m;
		}

		void AddAWBWithContactDetails(ForwardingShipment shipment)
		{
			shipment.JS_OverrideWaybillDefaults = ZBool.True;
			shipment.AWBHeader.EH_ConsigneeContactCode = "TE";
			shipment.AWBHeader.EH_ConsigneeContactDetail = "08 8932-2740";
			shipment.AWBHeader.EH_ShipperContactCode = "TE";
			shipment.AWBHeader.EH_ShipperContactDetail = "08 8932-2345";
		}

		#region TestOrgAddresses

		protected ZGuid GetConsigneeAddressSingleExportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "ACO SEWINGART LTD";
			result.MainAddress.OA_Address1 = "RM 833 METRO CENTER 2, 21 LAM HING";
			result.MainAddress.OA_Address2 = "ST., KOWLOON BAY";
			result.MainAddress.OA_City = "HONG KONG";
			result.OH_RL_NKClosestPort = "HKHKG";
			return result.PK;
		}

		protected ZGuid GetConsigneeAddressSingleImportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "YOKOHAMA OKADAYA PTY LTD";
			result.MainAddress.OA_Address1 = "PO BOX 1281";
			result.MainAddress.OA_Address2 = "SURFERS PARADISE";
			result.MainAddress.OA_City = "QLD";
			result.OH_RL_NKClosestPort = "HKHKG";
			return result.PK;
		}

		protected ZGuid GetConsigneeAddressSecondExportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "EL-ELRAD TRADING AND CONTRACTING COMP";
			result.MainAddress.OA_Address1 = "PO BOX 4430";
			result.MainAddress.OA_Address2 = "RIYADH  11491";
			result.MainAddress.OA_City = "SAUDI ARABIA";
			result.OH_RL_NKClosestPort = "SARUH";
			return result.PK;
		}

		protected ZGuid GetConsigneeAddressThirdExportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "WHEELING MOTOS";
			result.MainAddress.OA_Address1 = "B.P. 12294 MAGENTA";
			result.MainAddress.OA_City = "NOUMEA  98802";
			result.OH_RL_NKClosestPort = "NCNOU";
			return result.PK;
		}

		protected ZGuid GetConsigneeAddressFirstImportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "GEIGER ENTERPRISES PTY LTD";
			result.MainAddress.OA_Address1 = "22 MERIVALE STREET";
			result.MainAddress.OA_Address2 = "SOUTH BRISBANE";
			result.MainAddress.OA_City = "BRISBANE";
			result.MainAddress.OA_State = "QLD";
			result.OH_RL_NKClosestPort = "AUBNE";
			return result.PK;
		}

		protected ZGuid GetConsigneeAddressThirdImportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "COLES MYER GROUP";
			result.MainAddress.OA_Address1 = "COLES MYER HEAD OFFICE";
			result.MainAddress.OA_Address2 = "800 TOORAK ROAD";
			result.MainAddress.OA_City = "TOORONGA  VIC";
			result.OH_RL_NKClosestPort = "HKHKG";
			return result.PK;
		}

		protected ZGuid GetConsignorAddressSingleExportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "J. LEUTENEGGER PTY LTD";
			result.MainAddress.OA_Address1 = "PO BOX 6241";
			result.MainAddress.OA_Address2 = "FAIRFIELD GARDENS";
			result.MainAddress.OA_City = "BRISBANE";
			result.MainAddress.OA_State = "QLD";
			result.MainAddress.OA_PostCode = "";
			result.OH_RL_NKClosestPort = "AUBNE";
			return result.PK;
		}

		protected ZGuid GetConsignorAddressSingleImportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "LESPORTSAC INC";
			result.MainAddress.OA_Address1 = "320 FIFTH AVE";
			result.MainAddress.OA_City = "NEW YORK";
			result.MainAddress.OA_State = "NY";
			result.MainAddress.OA_PostCode = "10001";
			result.OH_RL_NKClosestPort = "USNYC";
			return result.PK;
		}

		protected ZGuid GetConsignorAddressSecondExportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "COINWATCH AUSTRALIA PTY LTD";
			result.MainAddress.OA_Address1 = "PO BOX 38";
			result.MainAddress.OA_Address2 = "PARADISE POINT";
			result.MainAddress.OA_City = "BRISBANE";
			result.MainAddress.OA_State = "QLD";
			result.OH_RL_NKClosestPort = "AUBNE";
			return result.PK;
		}

		protected ZGuid GetConsignorAddressThirdExportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "MOTORCYCLE SPECIALTIES";
			result.MainAddress.OA_Address1 = "PO BOX 491";
			result.MainAddress.OA_Address2 = "KENMORE";
			result.MainAddress.OA_City = "BRISBANE";
			result.MainAddress.OA_State = "QLD";
			result.OH_RL_NKClosestPort = "AUBNE";
			return result.PK;
		}

		protected ZGuid GetConsignorAddressFirstImportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "SHANGHAI NEW UNION TEXTRA IMPRT AND E";
			result.MainAddress.OA_Address1 = "CO LTD";
			result.MainAddress.OA_Address2 = "1565 JIAO TONG ROAD";
			result.MainAddress.OA_City = "SHANGHAI";
			result.MainAddress.OA_State = "31";
			result.OH_RL_NKClosestPort = "CNSHA";
			return result.PK;
		}

		protected ZGuid GetConsignorAddressThirdImportJob()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "WERIL INSTUMENTOS MUSICAIS LTDA.";
			result.MainAddress.OA_Address1 = "RUA MIGUEL S LERUSSI, 300";
			result.MainAddress.OA_Address2 = "CEP FRANCO DA ROCHA - SP";
			result.MainAddress.OA_City = "BRASILIA";
			result.MainAddress.OA_State = "DF";
			result.OH_RL_NKClosestPort = "BRBSB";
			return result.PK;
		}

		#endregion

		#endregion

		#region TestMessages

		public const string TestOriginalMessageExport = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++FX 61280012101+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 61280012102+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			DOC+811:::3B003571077FDC'
			UNT+28+HMF6897439X160'";

		public const string TestOriginalMessageExport2 = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:4'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			DOC+811:::3B003571077FDC'
			UNT+28+HMF6897439X160'";

		public const string TestWithManyPermitNumbersExport = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			DOC+811:::2131231231'
			DOC+811:::1242141241'
			DOC+811:::12341242141'
			UNT+30+HMF6897439X160'";

		public const string TestWithManyJobsExport = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:189.0:KGM'
			CNT+8:8'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:3'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			DOC+811:::12341242141'
			DOC+811:::3B003571077FDC'
			CNI+2+3135001'
			CNT+8:4'
			CNT+1:4'
			MEA+WT++KGM:80.0'
			LOC+5+HKG'
			LOC+8+RUH'
			NAD+CN+++EL-ELRAD TRADING AND CONTRACTING CO+PO BOX 4430:RIYADH  11491+SAUDI ARABIA+++SA'
			NAD+CZ+++COINWATCH AUSTRALIA PTY LTD+PO BOX 38:PARADISE POINT+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++WRIST WATCHES'
			MOA+43:6120.00:AUD'
			MOA+95:0.00:AUD'
			MOA+94:0.00:AUD'
			DOC+811:::3B010031233WTC'
			CNI+3+3136001'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:15.0'
			LOC+5+HKG'
			LOC+8+NOU'
			NAD+CN+++WHEELING MOTOS+B.P. 12294 MAGENTA+NOUMEA  98802+++NC'
			NAD+CZ+++MOTORCYCLE SPECIALTIES+PO BOX 491:KENMORE+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++MOTORCYCLE PARTS AND ACCESSORI'
			PAC'
			PCI+28+SOME TEST MARKS AND NUMBERS'
			MOA+43:949.65:AUD'
			MOA+95:0.00:AUD'
			MOA+94:0.00:AUD'
			DOC+811:::3B010051283SBC'
			UNT+59+HMF6897439X160'";

		public const string TestOriginalMessageImport = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:112.0:KGM'
			CNT+8:1'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:1'
			CNI+1+1234567890'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++SPORTS BAGS'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			UNT+27+HMF7615265X081'";

		public const string TestOriginalMessageImport_NullConsignee = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:112.0:KGM'
			CNT+8:1'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:1'
			CNI+1+1234567890'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++SPORTS BAGS'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			UNT+27+HMF7615265X081'";

		public const string TestOriginalMessageImportBCN = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:50.8:KGM'
			CNT+8:2'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:2'
			CNI+1+12150048'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:50.8'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++SPORTS BAGS'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			CNI+2+3136002'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:0.0'
			LOC+5+HKG'
			LOC+8+NOU'
			NAD+CN+++WHEELING MOTOS+B.P. 12294 MAGENTA+NOUMEA  98802+++NC'
			NAD+CZ+++MOTORCYCLE SPECIALTIES+PO BOX 491:KENMORE+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++MOTORCYCLE PARTS AND ACCESSORI'
			MOA+43:949.65:AUD'
			MOA+95:0.00:AUD'
			MOA+94:0.00:AUD'
			UNT+40+HMF7615265X081'";

		public const string TestWithManyPermitNumbersImport = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:112.0:KGM'
			CNT+8:1'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:1'
			CNI+1+12150048'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++SPORTS BAGS'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			DOC+911:::2131231231'
			DOC+911:::1242141241'
			DOC+911:::12341242141'
			UNT+30+HMF7615265X081'";

		public const string TestWithManyJobsImport = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+BNE'
			LOC+8+HKG'
			CNT+7:169.0:KGM'
			CNT+8:13'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:3'
			CNI+1+00329'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:26.0'
			LOC+5+SHA'
			LOC+8+HKG'
			NAD+CN+++GEIGER ENTERPRISES PTY LTD+22 MERIVALE STREET:SOUTH BRISBANE+BRISBANE+QLD++AU'
			NAD+CZ+++SHANGHAI NEW UNION TEXTRA IMPRT AND+CO LTD:1565 JIAO TONG ROAD+SHANGHAI+31++CN'
			GDS+12'
			FTX+AAA+++PRINTING EQUIPMENT'
			MOA+43:1234.00:USD'
			MOA+95:0.00:USD'
			MOA+94:0.00:USD'
			CNI+2+12150048'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++SPORTS BAGS'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			CNI+3+154927'
			CNT+8:11'
			CNT+1:11'
			MEA+WT++KGM:31.0'
			LOC+5+BNE'
			LOC+8+HKG'
			NAD+CN+++COLES MYER GROUP+COLES MYER HEAD OFFICE:800 TOORAK ROAD+TOORONGA  VIC+++HK'
			NAD+CZ+++WERIL INSTUMENTOS MUSICAIS LTDA.+RUA MIGUEL S LERUSSI, 300:CEP FRANCO DA ROCHA - SP+BRASILIA+DF++BR'
			GDS+12'
			FTX+AAA+++WEARING APPAREL'
			PAC'
			PCI+28+WOW GREAT'
			MOA+43:2131.00:IDR'
			MOA+95:0.00:IDR'
			MOA+94:0.00:IDR'
			UNT+55+HMF7615265X081'";

		public const string TestCancelImport = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+3'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			UNT+27+HMF6897439X160'";

		public const string TestContactDetailsOriginal = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+MDW'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+MDW'
			NAD+CN++TE 0889322740+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 0889322345+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++TE 0889322740::CNE:CT'
			CST'
			FTX+CUS+1++TE 0889322345::SHP:CT'
			UNT+31+HMF6897439X160'";

		public const string TestMonetaryAmountsOriginal = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			MOA+335'
			MOA+336'
			UNT+29+HMF6897439X160'";

		public const string TestReplaceUNLOCOWithIATAMessage = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+INC'
			LOC+8+TST'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'CNI+1+3134001'
			CNT+8:3'CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+INC'
			LOC+8+TST'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			UNT+27+HMF6897439X160'";

		public const string TestDestinationPortMessage = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+BNE'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'CNI+1+3134001'
			CNT+8:3'CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN+++ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ+++J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'FTX+AAA+++3 CARTONS STC ""HOMETIME"" SAMPL'
			PAC'
			PCI+28+HERE ARE SOME TEST MARKS AND NUMBER:S BY SCOTT AS I NEED THEM FOR THE T:EST MESSAGE. THIS IS THE GREATEST D:ISCUSSION IN THE WORLD ABOUT THE ST:ATE OF NANOPARTICULES IN A MAGNETIC: FIELD AS DESCRIBED BY MAXWELLS EQU:ATIONS. A ROUND OF APPLAUSE FOR SCO:TT R WRIGHT THE WORLD LEADER IN THI:S AREA. NOW THAT I HAVE ENOUGH MARK:S AND NUMBERS'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			UNT+27+HMF6897439X160'";

		public const string TestISACWithDetailedGoodsDescriptionOnShipmentMessage = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:112.0:KGM'
			CNT+8:1'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:2'
			CNI+1+12150048'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++THIS IS SHORT GOODS DESCRIPTION'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			CST'
			FTX+AAA+1++ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO'
			CST'
			FTX+AAA+1++  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABC'
			CST'
			FTX+AAA+1++DE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOP'
			CST'
			FTX+AAA+1++QRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCD'
			CST'
			FTX+AAA+1++EFGHIJ  KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQ'
			CST'
			FTX+AAA+1++RSTUVWXY  ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE'
			CST'
			FTX+AAA+1++FGHIJKLMNO  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQR'
			CST'
			FTX+AAA+1++STUVWXYABCDE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEF'
			CST'
			FTX+AAA+1++GHIJKLMNOPQRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRS'
			CNI+2+12150049'
			CNT+8:0'
			MEA+WT++KGM:0.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN'
			NAD+CZ'
			GDS+12'
			FTX+AAA+++THIS IS SHORT DESCRIPTION'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			UNT+57+HMF7615265X081'";

		public const string TestISACWithDetailedGoodsDescriptionWithEmptySpaceOnShipmentMessage = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:112.0:KGM'
			CNT+8:1'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:2'
			CNI+1+12150048'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++THIS IS SHORT GOODS DESCRIPTION'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			CST'
			FTX+AAA+1++ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO'
			CST'
			FTX+AAA+1++  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABC'
			CST'
			FTX+AAA+1++DE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOP'
			CST'
			FTX+AAA+1++QRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCD'
			CST'
			FTX+AAA+1++EFGHIJ  KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQ'
			CST'
			FTX+AAA+1++RSTUVWXY  ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE'
			CST'
			FTX+AAA+1++FGHIJKLMNO  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQR'
			CST'
			FTX+AAA+1++STUVWXYABCDE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEF'
			CNI+2+12150049'
			CNT+8:0'
			MEA+WT++KGM:0.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN'
			NAD+CZ'
			GDS+12'
			FTX+AAA+++THIS IS SHORT DESCRIPTION'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			UNT+55+HMF7615265X081'";

		public const string TestISACWithDetailedGoodsDescriptionWithACINumberMessage = @"UNH+HMF7615265X081+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+08176152650/C00010924+5'
			LOC+5+SYD'
			LOC+8+HKG'
			CNT+7:112.0:KGM'
			CNT+8:1'
			NAD+PK+1331929'
			TDT+13+QF522'
			DTM+132:010204:101'
			RFF+MWB:08176152650'
			CNT+10:1'
			CNI+1+12150048'
			CNT+8:1'
			CNT+1:1'
			MEA+WT++KGM:112.0'
			LOC+5+SYD'
			LOC+8+HKG'
			NAD+CN+++YOKOHAMA OKADAYA PTY LTD+PO BOX 1281:SURFERS PARADISE+QLD+++HK'
			NAD+CZ+++LESPORTSAC INC+320 FIFTH AVE+NEW YORK+NY+10001+US'
			GDS+12'
			FTX+AAA+++THIS IS SHORT GOODS DESCRIPTION'
			PAC'
			PCI+28+TEST MARKS AND NUMBERS'
			MOA+43:213.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			CST'
			FTX+AAA+1++ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNO'
			CST'
			FTX+AAA+1++  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABC'
			CST'
			FTX+AAA+1++DE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOP'
			CST'
			FTX+AAA+1++QRST  UVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCD'
			CST'
			FTX+AAA+1++EFGHIJ  KLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQ'
			CST'
			FTX+AAA+1++RSTUVWXY  ABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDE'
			CST'
			FTX+AAA+1++FGHIJKLMNO  PQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQR'
			CST'
			FTX+AAA+1++STUVWXYABCDE  FGHIJKLMNOPQRSTUVWXYABCDEFGHIJKLMNOPQRSTUVWXYABCDEF'
			CST'
			FTX+AAA+1++ACI 1234567890123456789'
			UNT+45+HMF7615265X081'";

		public const string TestSendOtherCustomsInformation = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+SYD'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:2'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:AU:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:AU:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:AU:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CNI+2'
			CNT+8:0'
			MEA+WT++KGM:0.0'
			LOC+5+BNE'
			LOC+8+RUH'
			NAD+CN++TE 081128521789+EL-ELRAD TRADING AND CONTRACTING CO+PO BOX 4430:RIYADH  11491+SAUDI ARABIA+++SA'
			NAD+CZ++TE 61280012408+COINWATCH AUSTRALIA PTY LTD+PO BOX 38:PARADISE POINT+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+96:0.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			CST'
			FTX+CUS+1++TE 081128521789:AU:CNE:CT'
			CST'
			FTX+CUS+1++USCIR2002410:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME2:SG:NFY:CP'
			CST'
			FTX+CUS+1++TE 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++888812345678:SG:NFY:T'
			UNT+63+HMF6897439X160'";

		public const string TestSendOtherCustomsInformation_ACAS = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+SYD'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:2'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:AU:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:AU:CNE:CT'
			CST'
			FTX+CUS+1++JANE.DOE:US:CNE:MU'
			CST'
			FTX+CUS+1++SHADYDEALINGS.COM:US:CNE:MD'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:AU:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++JOHN.SMITH:US:SHP:MU'
			CST'
			FTX+CUS+1++GENERICCOMPANY.COM:US:SHP:MD'
			CST'
			FTX+CUS+1++9999WE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CNI+2'
			CNT+8:0'
			MEA+WT++KGM:0.0'
			LOC+5+BNE'
			LOC+8+RUH'
			NAD+CN++TE 081128521789+EL-ELRAD TRADING AND CONTRACTING CO+PO BOX 4430:RIYADH  11491+SAUDI ARABIA+++SA'
			NAD+CZ++TE 61280012408+COINWATCH AUSTRALIA PTY LTD+PO BOX 38:PARADISE POINT+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+96:0.00:HKD'
			MOA+95:0.00:HKD'
			MOA+94:0.00:HKD'
			UNT+61+HMF6897439X160'";

		public const string TestSendOtherCustomsInformation_TRANS = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+SYD'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			UNT+25+HMF6897439X160'";

		public const string TestSendOtherCustomsInformation_ACAS_TRANS = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+SYD'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:AU:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:AU:CNE:CT'
			CST'
			FTX+CUS+1++JANE.DOE:US:CNE:MU'
			CST'
			FTX+CUS+1++SHADYDEALINGS.COM:US:CNE:MD'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:AU:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++JOHN.SMITH:US:SHP:MU'
			CST'
			FTX+CUS+1++GENERICCOMPANY.COM:US:SHP:MD'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			UNT+49+HMF6897439X160'";

		public const string TestSendAccountHolderAndAccountName_ControllingCustomer = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+LAX'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:US:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:US:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:US:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CST'
			FTX+CUS+1++3:US:CUS:AH'
			CST'
			FTX+CUS+1++CUSTOMS AGENTS:US:CUS:AN'
			UNT+45+HMF6897439X160'";

		public const string TestSendAccountHolderAndAccountName_Shipper = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+LAX'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:US:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:US:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:US:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CST'
			FTX+CUS+1++S:US:CUS:AH'
			CST'
			FTX+CUS+1++COMPANY SHIPPER:US:CUS:AN'
			UNT+45+HMF6897439X160'";

		public const string TestSendAccountHolderAndAccountName_Consignee = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+LAX'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:US:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:US:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:US:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CST'
			FTX+CUS+1++C:US:CUS:AH'
			CST'
			FTX+CUS+1++COMPANY CONSIGNEE:US:CUS:AN'
			UNT+45+HMF6897439X160'";

		public const string TestSendAccountIssuerAndAccountNumber_ControllingCustomer = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+LAX'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:US:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:US:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:US:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CST'
			FTX+CUS+1++3:US:CUS:AH'
			CST'
			FTX+CUS+1++CUSTOMS AGENTS:US:CUS:AN'
			CST'
			FTX+CUS+1++12345670000:US:CUS:AI'
			CST'
			FTX+CUS+1++CUSAGEHKG:US:CUS:AR'
			UNT+49+HMF6897439X160'";

		public const string TestSendAccountIssuerAndAccountNumber_Shipper = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+LAX'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:US:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:US:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:US:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CST'
			FTX+CUS+1++S:US:CUS:AH'
			CST'
			FTX+CUS+1++COMPANY SHIPPER:US:CUS:AN'
			CST'
			FTX+CUS+1++12345670000:US:CUS:AI'
			CST'
			FTX+CUS+1++SHIPPERACCOUNT:US:CUS:AR'
			UNT+49+HMF6897439X160'";

		public const string TestSendAccountIssuerAndAccountNumber_Consignee = @"UNH+HMF6897439X160+CUSEXP:D:95A:UN+<<MSGNO PLACEHOLDER>>'
			BGM+85:::EXPRESS CONSIGNMENT MANIFEST+16068974393/C00003134+5'
			LOC+5+HKG'
			LOC+8+LAX'
			CNT+7:94.0:KGM'
			CNT+8:3'
			NAD+PK+1331929'
			TDT+13+CX102'
			DTM+132:001223:101'
			RFF+MWB:16068974393'
			CNT+10:1'
			CNI+1+3134001'
			CNT+8:3'
			CNT+1:3'
			MEA+WT++KGM:94.0'
			LOC+5+HKG'
			LOC+8+BNE'
			NAD+CN++TE 081123456789+ACO SEWINGART LTD+RM 833 METRO CENTER 2, 21 LAM HING:ST., KOWLOON BAY+HONG KONG+++HK'
			NAD+CZ++TE 080152045450+J. LEUTENEGGER PTY LTD+PO BOX 6241:FAIRFIELD GARDENS+BRISBANE+QLD++AU'
			GDS+12'
			FTX+AAA'
			MOA+43:1710.02:AUD'
			MOA+44:12.34:AUD'
			MOA+94:0.00:AUD'
			CST'
			FTX+CUS+1++CONSIGNEENAME1:US:CNE:CP'
			CST'
			FTX+CUS+1++TE 081123456789:US:CNE:CT'
			CST'
			FTX+CUS+1++PASSPORTWHE2322323203:US:CNE:T'
			CST'
			FTX+CUS+1++TE 080152045450:HK:SHP:CT'
			CST'
			FTX+CUS+1++XYZWE02322324:HK:SHP:T'
			CST'
			FTX+CUS+1++ALSONOTIFYNAME1:SG:NFY:CP'
			CST'
			FTX+CUS+1++FX 08012000032:SG:NFY:CT'
			CST'
			FTX+CUS+1++ID2K2323002:SG:NFY:T'
			CST'
			FTX+CUS+1++C:US:CUS:AH'
			CST'
			FTX+CUS+1++COMPANY CONSIGNEE:US:CUS:AN'
			CST'
			FTX+CUS+1++12345670000:US:CUS:AI'
			CST'
			FTX+CUS+1++CONSIGNEEACC:US:CUS:AR'
			UNT+49+HMF6897439X160'";

		#endregion
	}
}
