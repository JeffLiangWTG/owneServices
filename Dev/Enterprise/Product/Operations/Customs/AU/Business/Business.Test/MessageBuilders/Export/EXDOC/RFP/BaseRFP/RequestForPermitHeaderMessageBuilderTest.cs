using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitHeaderMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateTransfereeEDIUserIdentifier()
		{
			quarantineHeader.QH_TransfereeEDIUserIdentifier = "98760";
			builder.GenerateTransferRFPMessage();
			AssertEquals("Transferee EDI User Identifier", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'GIS+N::AQ:CT'CTA+AG+98760'UNT+5+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting);
		}

		public void TestGenerateTransfereeExporterNumber()
		{
			quarantineHeader.QH_TransfereeExporterNumber = "12345";
			builder.GenerateTransferRFPMessage();
			AssertEquals("Transferee Exporter Number", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'GIS+N::AQ:CT'PNA+TT+12345'UNT+5+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting);
		}

		public void TestGenerateCancelTransferIndicator()
		{
			quarantineHeader.QH_CancelTransferIndicator = true;
			builder.GenerateTransferRFPMessage();
			AssertEquals("Cancel Transfer Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'GIS+Y::AQ:CT'UNT+4+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting);
		}

		public void TestGenerateRFPForwardStatus()
		{
			quarantineHeader.QH_ForwardStatus = EXDOCComplianceStatusCodes.Codes.Completed;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Forward Status", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+COMP::AQ:FW'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateForwardeeEDIUserIdentifier()
		{
			quarantineHeader.QH_ForwardeeEDIUserIdentifier = "123TEST";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("ForwardeeEDIUserIndetifier", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'CTA+AG+123TEST'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateBGMForSea()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("BGM is sea", BGMForSeaMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateBGMForAir()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("BGM is air", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:0++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'TDT+12++4'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateBGMForRFPNumber()
		{
			quarantineHeader.QH_RequestForPermitNumber = "12345";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("BGM has RFP Number", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9+12345+13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateBorderInspection()
		{
			quarantineHeader.QH_RL_NKBorderInspectionPort = "AUSYD";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Border Inspection Port", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+43+AUSYD'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateStorageProcessInformation()
		{
			quarantineHeader.QH_StorageEstablishment = "88";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Storage Process Information", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PRC+ST:PP:AQ'PNA+MP+88'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateLoadingPort()
		{
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Loading Port", LoadingPortMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateDischargePort()
		{
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Discharge Port", DischargePortMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateDestinationCity()
		{
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Discharge Port", DestinationCityMessage, builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateDestinationCountry()
		{
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Destination Country", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+12+NZAKL'LOC+8+AUCKLAND'LOC+36+NZ'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+12+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "DestinationCountry", "LOC+36+NZ'", () => new RequestForPermitHeaderMessageBuilderForTest(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateProductSourceCountry()
		{
			quarantineHeader.QH_RN_NKOriginCountry = Core.Constants.CountryCodes.Fiji;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Product Source Country", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+FJ'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTransitCountry()
		{
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_RL_NKPortOfFirstArrival = "GBLON";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Transit Country", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+12+NZAKL'LOC+8+AUCKLAND'LOC+36+NZ'LOC+30+AU'LOC+49+GB'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+13+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateCertificateRequiredLocation()
		{
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			quarantineHeader.QH_CertificateRequiredLocation = "SYD";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Certificate Required Location", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'LOC+91+SYD'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateAQISRegion()
		{
			quarantineHeader.QH_AQISRegion = "BNE";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("AQIS Region", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'LOC+48+BNE'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateExporterReferenceNotOwnersReference()
		{
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Exporter Reference", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateExporterReferenceIsOwnersReference()
		{
			declaration.JE_UseOwnerRefAsQuarantineRef = true;
			declaration.JE_OwnerRef = "TEST A";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Exporter Reference", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:TEST A'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateCustomsAuthorityNumber()
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "AAE34567";
			entryNumber.CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = JobDeclaration.Schema.TableName;
			entryNumber.CE_RN_NKCountryCode = "AU";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Customs Authority Number", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'RFF+AAE:AAE34567'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateExporterDeclaration()
		{
			quarantineHeader.QH_ExporterDeclaration = @"TEST EXPORTER DECLARATION LINE1-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE2-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE3-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE4-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE5-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE6-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE7-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE8-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE9-------------------------------------->" +
																			"TEST EXPORTER DECLARATION LINE10------------------------------------->";

			builder.GenerateRFPMessage();
			Assert("Exporter Declaration", builder.MessageTextForTesting.Contains(ExporterDeclarationMessage));
		}

		public void TestGenerateInspectorComments()
		{
			quarantineHeader.QH_InspectorComments = "COMMENTS BY SCOTT FOR A TEST";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Inspector Comments", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+AAY+++COMMENTS BY SCOTT FOR A TEST'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateNotifyParty()
		{
			var notifyTextNote = declaration.Notes.AddNew();
			notifyTextNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCNotifyText.Description;
			notifyTextNote.ST_NoteText = "NEDDY SEAGOON";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Notify Party", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+AAG+++NEDDY SEAGOON'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateLetterOfCredit()
		{
			var additionalInfotNote = declaration.Notes.AddNew();
			additionalInfotNote.ST_Description = PredefinedNoteTypes.Instance.EXDOCLetterOfCredit.Description;
			additionalInfotNote.ST_NoteText = "SCOTT,BONE IN, FROZEN, LOIN";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Letter of Credit Test", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'FTX+AAW+++SCOTT,BONE IN, FROZEN, LOIN'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateFOBCurrencyUnit()
		{
			quarantineHeader.QH_ObtainExportCustomsPermit = true;
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("FOB Currency Unit", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'MOA+63::AUD'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateCertificatePrintIndicator()
		{
			quarantineHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Certificate Print Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+A::AQ:PHC'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateSeperateCertificateContainerIndicator()
		{
			quarantineHeader.QH_SplitHealthCertByContainer = true;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Certificate Container Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+Y::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateSeperateCertificateMarksIndicator()
		{
			quarantineHeader.QH_SplitHealthCertByMarks = true;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Certificate Marks Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+Y::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateSeperateCertificatePackerIndicator()
		{
			quarantineHeader.QH_SplitHealthCertByPacker = true;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Certificate Packer Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+Y::AQ:SP'GIS+N::AQ:ACS'UNT+9+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateCustomsAgentIndicator()
		{
			quarantineHeader.QH_ObtainExportCustomsPermit = true;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Customs Agent Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+N::AQ:DCT'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateDeclarationEstimateIndicator()
		{
			quarantineHeader.QH_ObtainExportCustomsPermit = true;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.Confirming;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Declaration Estimate Indicator", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+Y::AQ:ACS'GIS+C::AQ:DCT'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateImportPermitSameOnTwoLines()
		{
			invoiceLine.JI_TempImportNum = "123456";
			invoiceLine.JI_TempImportDate = new ZDateTime(2006, 12, 12);
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_TempImportNum = "123456";
			invoiceLine2.JI_TempImportDate = new ZDateTime(2006, 12, 12);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Only one permit segment is sent", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'DOC+911+123456'DTM+137:20061212:102'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateImportPermitDifferentOnTwoLines()
		{
			invoiceLine.JI_TempImportNum = "123456";
			invoiceLine.JI_TempImportDate = new ZDateTime(2006, 12, 12);
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_TempImportNum = "53453";
			invoiceLine2.JI_TempImportDate = new ZDateTime(2006, 12, 13);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Two permit segments are sent", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'DOC+911+123456'DTM+137:20061212:102'DOC+911+53453'DTM+137:20061213:102'UNT+13+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateOwnerExportNumber()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "TESTSUP";
			var exportNum = supplier.CustomsCodes.AddNew();
			exportNum.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber;
			exportNum.OK_RN_NKCodeCountry = "AU";
			exportNum.OK_OH = supplier.PK;
			exportNum.OK_CustomsRegNo = "12345";
			declaration.JE_OH_Supplier = supplier.PK;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Generate Owner Export Number", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+EX+12345'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateConsigneeName()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE FULL NAME";
			consignee.CustomsClientID = "CONSIGNEE REFERENCE NUMBER";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Consignee Name", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+CN+CONSIGNEE REFERENCE NUMBER++++10:CONSIGNEE FULL NAME'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestLongConsigneeNameIsTruncated()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "ORLANDO WYNDHAM WINE MANUFACTURING GROUP PTY LTD";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertNotContains("Full name should not have been generated in the RFP message", "ORLANDO WYNDHAM WINE MANUFACTURING GROUP PTY LTD", builder.MessageTextForTesting);
			AssertContains("Truncated name should be in the RFP message", "ORLANDO WYNDHAM WINE MANUFACTURING ", builder.MessageTextForTesting);
		}

		public void TestGenerateConsigneeAddressAndPhone()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "456 HIGH ST";
			consignee.MainAddress.OA_Address2 = "GOOGLE BUTT";
			consignee.MainAddress.OA_City = "TAIPEI";
			consignee.MainAddress.OA_PostCode = "654321";
			consignee.OH_RL_NKClosestPort = "TWTPE";
			consignee.MainAddress.OA_State = "TWSTATE";
			consignee.MainAddress.OA_Phone = "02 1234 5678";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Consignee Address", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+12+TWTPE'LOC+8+TAIPEI'LOC+36+TW'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'ADR++5:456 HIGH ST:GOOGLE BUTT+TAIPEI+654321+TW+:::TWSTATE'CTA+CN'COM+0212345678:TE'UNT+15+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateConsigneeAddressAndPhone2()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_Address1 = "Unit 3 72 O'Riordan Street";
			consignee.MainAddress.OA_Address2 = "Alexandria";
			consignee.MainAddress.OA_City = "Sydney";
			consignee.MainAddress.OA_PostCode = "2020";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_Phone = "02 1234 5678";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Consignee Address", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+12+AUSYD'LOC+8+SYDNEY'LOC+36+AU'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'ADR++5:UNIT 3 72 O RIORDAN STREET:ALEXANDRIA+SYDNEY+2020+AU+:::NSW'CTA+CN'COM+0212345678:TE'UNT+15+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateStateForConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CHINA IMPORTS TRADING LTD";
			consignee.MainAddress.OA_Address1 = "RM 310 CHINA QING AN BUILDING";
			consignee.MainAddress.OA_Address2 = "NO 27 XIAO YUN ROAD";
			consignee.MainAddress.OA_City = "CHAOYANG DISTRICT BEIJING";
			consignee.MainAddress.OA_PostCode = "100027";
			consignee.OH_RL_NKClosestPort = "CNBJS";
			consignee.MainAddress.OA_State = "11";
			consignee.MainAddress.OA_Phone = "02 1234 5678";
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertContains("When the state is stored as a number, the State value generated in the message should be the State description", "'ADR++5:RM 310 CHINA QING AN BUILDING:NO 27 XIAO YUN ROAD+CHAOYANG DISTRICT BEIJING+100027+CN+:::BEIJING'", builder.MessageTextForTesting);
			Assert(builder.MessageTextForTesting.Contains("'ADR++5:RM 310 CHINA QING AN BUILDING:NO 27 XIAO YUN ROAD+CHAOYANG DISTRICT BEIJING+100027+CN+:::BEIJING'"));

			consignee.MainAddress.OA_State = "BJG";
			builder.GenerateRFPMessage();
			AssertContains("If a State abbreviation has been entered, the State value generated should be that abbreviated State code", "'ADR++5:RM 310 CHINA QING AN BUILDING:NO 27 XIAO YUN ROAD+CHAOYANG DISTRICT BEIJING+100027+CN+:::BJG'", builder.MessageTextForTesting);
		}

		public void TestGenerateConsigneeReferenceNumber()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CHINA IMPORTS TRADING LTD";
			var cusCode = consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456");
			declaration.JE_OH_Importer = consignee.PK;
			builder.GenerateRFPMessage();
			AssertContains("PNA+CN+123456", builder.MessageTextForTesting);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			var newBuilder = new RequestForPermitHeaderMessageBuilderForTest(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
			newBuilder.GenerateRFPMessage();
			AssertNotContains("PNA+CN+123456", newBuilder.MessageTextForTesting);
		}

		public void TestGenerateToOrder()
		{
			declaration.JE_ToOrder = true;
			builder.GenerateRFPMessage();
			AssertMultilineEquals("To Order", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+CN+++++10:TO ORDER'ADR++5+UNKNOWN'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateToOrderComment()
		{
			declaration.JE_ToOrder = true;
			declaration.JE_ToOrderComment = "CONSIGNEE TO BE ANNOUNCED LATER ON";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("To Order Comment", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PNA+CN+++++10:TO ORDER'ADR++5+CONSIGNEE TO BE ANNOUNCED LATER ON'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTransportDetails()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_VoyageFlightNo = "V123";
			declaration.JE_VesselName = "ADMIRALENGRACHT";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Transport Details", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'TDT+12+V123+1+++++:::ADMIRALENGRACHT'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateTransportDetailsForAirIncludesAirlineCode()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS FREIGHT LTD";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_VoyageFlightNo = "QF415";
			declaration.JE_VesselName = "";

			builder.GenerateRFPMessage();
			AssertMultilineEquals("Transport Details", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:0++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'TDT+12+QF415+4++:::QANTAS FREIGHT LTD+++:::QF'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateDepartureDate()
		{
			declaration.JE_ExportDate = new ZDateTime(2006, 12, 12);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Departure Date", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'DTM+136:20061212:102'UNT+10+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestGenerateVesselHoldSeals()
		{
			quarantineHeader.QH_StartHoldSeal = "1234";
			quarantineHeader.QH_EndHoldSeal = "1236";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Vessel Hold Seals", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'EQD+VH'SEL+1234+1236'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
		}

		public void TestInspectionRequestedDate()
		{
			quarantineHeader.QH_InspectionRequestedDate = new ZDateTime(2006, 12, 11);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Inspection Requested Date", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PRC+IN:PP:AQ'DTM+318:20061211:102'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "InspectionRequestedDate", "DTM+318:20061211:102'", () => new RequestForPermitHeaderMessageBuilderForTest(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateAuthorisedEndDate()
		{
			quarantineHeader.QH_AuthorisedEndDate = new ZDateTime(2006, 12, 12);
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Authorised End Date", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PRC+IN:PP:AQ'DTM+119:20061212:102'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "AuthorisedEndDate", "DTM+119:20061212:102'", () => new RequestForPermitHeaderMessageBuilderForTest(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateAuthorisationEstablishment()
		{
			quarantineHeader.QH_AuthorisationEstablishment = "AD2132";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Authorisiation Establishment", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PRC+IN:PP:AQ'PNA+FO+AD2132'UNT+11+<<MSGNO PLACEHOLDER>>'", builder.MessageTextForTesting, '\'');
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertAmendPermissionDeterminesPresenceInGeneratedMessage(quarantineHeader, "AuthorisationEstablishment", "PNA+FO+AD2132'", () => new RequestForPermitHeaderMessageBuilderForTest(invoiceHeader, EXDOCMessageTypeCodes.Codes.RPL));
		}

		public void TestGenerateAuthorisingOfficerIdentifier()
		{
			quarantineHeader.QH_AuthorisingOfficerID = "AQAMEAT";
			builder.GenerateRFPMessage();
			AssertMultilineEquals("Authorising Officer Identifier", "UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'PRC+IN:PP:AQ'PNA+AV+AQAMEAT'UNT+11+<<MSGNO PLACEHOLDER>>", builder.MessageTextForTesting, '\'');
		}

		internal delegate RequestForPermitHeaderMessageBuilder BuilderDelegate();
		internal static void AssertAmendPermissionDeterminesPresenceInGeneratedMessage(QuarantineExDocHeader header, ZString fieldName, ZString stringToLookFor, BuilderDelegate builderDelegate)
		{
			var builder = builderDelegate();
			header.RequestForPermitStatus = EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted;
			builder.GenerateRFPMessage();
			Assert("Should not contain " + fieldName, !builder.MessageTextForTesting.Contains(stringToLookFor));
			header.RequestForPermitStatus = ZString.Empty;
			builder = builderDelegate();
			builder.GenerateRFPMessage();
			Assert("Should contain " + fieldName, builder.MessageTextForTesting.Contains(stringToLookFor));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			declaration = helper.Declaration;
			invoiceHeader = helper.Header1;
			invoiceLine = helper.Line1;
			quarantineHeader = invoiceHeader.QuarantineExDocHeader;
			builder = new RequestForPermitHeaderMessageBuilderForTest(invoiceHeader, EXDOCMessageTypeCodes.Codes.LDG);
		}
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		RequestForPermitHeaderMessageBuilderForTest builder;
		QuarantineExDocHeader quarantineHeader;

		const string BGMForSeaMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'TDT+12++1'UNT+10+<<MSGNO PLACEHOLDER>>'";
		const string LoadingPortMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+9+SYD'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+10+<<MSGNO PLACEHOLDER>>'";
		const string DischargePortMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+12+NZAKL'LOC+8+AUCKLAND'LOC+36+NZ'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+12+<<MSGNO PLACEHOLDER>>'";
		const string DestinationCityMessage = @"UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:RF0801'BGM+TST::AQ:9++13'LOC+12+NZAKL'LOC+8+AUCKLAND'LOC+36+NZ'LOC+30+AU'RFF+ABE:<<SENDERS REFERENCE PLACE HOLDER>>'GIS+N::AQ:SC'GIS+N::AQ:SM'GIS+N::AQ:SP'GIS+N::AQ:ACS'UNT+12+<<MSGNO PLACEHOLDER>>'";
		const string ExporterDeclarationMessage = "'FTX+DCL+++TEST EXPORTER DECLARATION LINE1-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE2-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE3-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE4-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE5-------------------------------------->" +
																		"'FTX+DCL+++TEST EXPORTER DECLARATION LINE6-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE7-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE8-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE9-------------------------------------->" +
																		":TEST EXPORTER DECLARATION LINE10------------------------------------->'";

		sealed class RequestForPermitHeaderMessageBuilderForTest : RequestForPermitHeaderMessageBuilder
		{
			public RequestForPermitHeaderMessageBuilderForTest(JobComInvoiceHeader invoiceHeader, string messageTypeToSend)
				: base(invoiceHeader, messageTypeToSend)
			{
			}

			protected override void GenerateRFPLines()
			{
				//Not testing Lines
			}

			protected override DocumentMessageNameCodedList CommodityType() => DocumentMessageNameCodedList.GetFromString("TST");
		}
	}
}
