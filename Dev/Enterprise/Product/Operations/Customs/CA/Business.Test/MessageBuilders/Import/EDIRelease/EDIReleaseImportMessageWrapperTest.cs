using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class EDIReleaseImportMessageWrapperTest : TestCaseWithFactory
	{
		public void TestPopulateEntrySubmittedDateIfRequired()
		{
			var declaration = Declaration;
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.EDIRelease);

			// with not set declaration
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var wrapper = new EDIReleaseImportMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == entryHeader.CH_EntrySubmittedDate);

			// with already set declaration
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-30);
			declaration.JE_EntrySubmittedDate = declarationTime;
			wrapper = new EDIReleaseImportMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		public void TestProperties()
		{
			//Min
			var testEDIReleaseMessageWrapper = new EDIReleaseImportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			AssertEquals("TransactionNumber", Declaration.TransactionNumber.ToString(), testEDIReleaseMessageWrapper.TransactionNumber);
			AssertEquals("ServiceOptionID", ServiceOptions.Codes.RMDOGD, testEDIReleaseMessageWrapper.ServiceOptionID);
			AssertEquals("AssessmentOption", AssessmentOptions.Codes.AQtoFollow, testEDIReleaseMessageWrapper.AssessmentOption);

			AssertEquals("ImporterNumber", importerBMR.OK_CustomsRegNo, testEDIReleaseMessageWrapper.ImporterNumber);
			Declaration.Importer.CustomsCodes.RemoveAll();
			AssertEquals("ImporterNumber", ZString.Empty, testEDIReleaseMessageWrapper.ImporterNumber);

			AssertEquals("PriorityIndicator", Declaration.CA_PriorityInd, testEDIReleaseMessageWrapper.PriorityIndicator);
			AssertEquals("PortOfClearance", Declaration.JE_CustomsOffice, testEDIReleaseMessageWrapper.PortOfClearance);

			AssertEquals("GoodsLocationCode", Declaration.JE_LocationOfGoods, testEDIReleaseMessageWrapper.GoodsLocationCode);
			AssertEquals("GoodsLocationName", ZString.Empty, testEDIReleaseMessageWrapper.GoodsLocationName);

			Declaration.JE_LocationOfGoods = ZString.Empty;
			Declaration.CA_SubLocationName = "Test Sub-Location";
			AssertEquals("GoodsLocationCode", ZString.Empty, testEDIReleaseMessageWrapper.GoodsLocationCode);
			AssertEquals("GoodsLocationName", Declaration.CA_SubLocationName, testEDIReleaseMessageWrapper.GoodsLocationName);

			AssertEquals("DateOfArrival", ZDateTime.Empty, testEDIReleaseMessageWrapper.DateOfArrival);
			AssertEquals("DateOfDeparture", Declaration.JE_WarehouseReleaseDate, testEDIReleaseMessageWrapper.DateOfDeparture);
			Declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			testEDIReleaseMessageWrapper = new EDIReleaseImportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			AssertEquals("DateOfArrival", Declaration.JE_DateOfFirstArrival, testEDIReleaseMessageWrapper.DateOfArrival);
			AssertEquals("DateOfDeparture", ZDateTime.Empty, testEDIReleaseMessageWrapper.DateOfDeparture);

			AssertEquals("GoodsLocationCode for PARS", ZString.Empty, testEDIReleaseMessageWrapper.GoodsLocationCode);
			AssertEquals("GoodsLocationName for PARS", "Test Sub-Location", testEDIReleaseMessageWrapper.GoodsLocationName);

			AssertEquals("DateOfDeparture", ZDateTime.Empty, testEDIReleaseMessageWrapper.DateOfDeparture);

			AssertEquals("GrossWeight", 1001.5m, testEDIReleaseMessageWrapper.GrossWeight);
			AssertEquals("GrossWeightUnits", "KG", testEDIReleaseMessageWrapper.GrossWeightUnits);
			AssertEquals("NetWeight", 501.4m, testEDIReleaseMessageWrapper.NetWeight);
			AssertEquals("NetWeightUnits", "KG", testEDIReleaseMessageWrapper.NetWeightUnits);

			AssertEquals("ContainerNumbers", 2, testEDIReleaseMessageWrapper.ContainerNumbers.Length);
			AssertEquals("First container number", "TURE1111111", testEDIReleaseMessageWrapper.ContainerNumbers[0]);
			AssertEquals("Second container number", "TURE2222222", testEDIReleaseMessageWrapper.ContainerNumbers[1]);

			AssertEquals("1 CCN", 1, testEDIReleaseMessageWrapper.CargoControlNumbers.Length);
			AssertEquals("CCN", "2ITN12345678987654321", testEDIReleaseMessageWrapper.CargoControlNumbers[0]);
			var number = Declaration.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "1111111";
			Declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = ZString.Empty;
			AssertEquals("2 CCNs", 2, testEDIReleaseMessageWrapper.CargoControlNumbers.Length);
			AssertEquals("CCN1", "2ITN12345678987654321", testEDIReleaseMessageWrapper.CargoControlNumbers[0]);
			AssertEquals("CCN2", "1111111", testEDIReleaseMessageWrapper.CargoControlNumbers[1]);

			AssertEquals("NumberOfPackages Count", 2, testEDIReleaseMessageWrapper.NumberOfPackages.Length);
			AssertEquals("TypeOfPackages Count", 2, testEDIReleaseMessageWrapper.TypeOfPackages.Length);
			AssertEquals("NumberOfPackages", Declaration.PackingGroups[0].Packages[1].CW_PackQty, testEDIReleaseMessageWrapper.NumberOfPackages[1]);
			AssertEquals("TypeOfPackages", Declaration.PackingGroups[0].Packages[1].CW_PackType, testEDIReleaseMessageWrapper.TypeOfPackages[1]);

			AssertEquals("Importer", Declaration.ImporterDocumentaryAddress, testEDIReleaseMessageWrapper.Importer);
			AssertEquals("Carrier", Declaration.CA_CarrierName, testEDIReleaseMessageWrapper.Carrier.E2_CompanyName);
			AssertEquals("Broker", "BROKER NAME", testEDIReleaseMessageWrapper.Broker.E2_CompanyName);
			AssertEquals("DeliveryAddress", Declaration.ClientPickupDeliveryAddress, testEDIReleaseMessageWrapper.DeliveryAddress);
			AssertEquals("DeliveryInstructions", Declaration.CustomsEntryHeaders[0].CH_CustomsDeliveryInstructions, testEDIReleaseMessageWrapper.DeliveryInstructions);
			AssertEquals("TotalValueForDuty", Declaration.CustomsEntryHeaders[0].CustomsValue, testEDIReleaseMessageWrapper.TotalValueForDuty);
			AssertNotNull("Invoices", testEDIReleaseMessageWrapper.Invoices);

			//OGD
			AssertEquals("OGDCFIA", Declaration.CA_OGDCFIA, testEDIReleaseMessageWrapper.OGDCFIA);
			AssertEquals("OGDIC", Declaration.CA_OGDIC, testEDIReleaseMessageWrapper.OGDIC);
			AssertEquals("OGDNR", Declaration.CA_OGDNR, testEDIReleaseMessageWrapper.OGDNR);
			AssertEquals("OGDTC", Declaration.CA_OGDTC, testEDIReleaseMessageWrapper.OGDTC);
			AssertEquals("DeliveryPhone", Declaration.ClientPickupDeliveryAddress.E2_Phone, testEDIReleaseMessageWrapper.DeliveryPhone);
			AssertEquals("DeliveryFax", Declaration.ClientPickupDeliveryAddress.E2_Fax, testEDIReleaseMessageWrapper.DeliveryFax);

			Declaration.ImporterOfRecordAddress.OrganisationPK = importerOrRecord.PK;
			AssertEquals("ImporterNumber", importerIMR.OK_CustomsRegNo, testEDIReleaseMessageWrapper.ImporterNumber);
			AssertEquals("Importer", Declaration.ImporterOfRecordAddress, testEDIReleaseMessageWrapper.Importer);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 6, 15, 10, 30, 25)]
		[ExpectNoExceptions]
		public void TestReleaseMessageContent()
		{
			CACustomsDataRegistry.Instance.AlwaysSendDeliveryAddressOnReleaseMessages.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var testEDIReleaseMessageWrapper = new EDIReleaseImportMessageWrapper(Declaration.CustomsEntryHeaders[0]);

			var builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, testEDIReleaseMessageWrapper);
			foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
			{
				var message = (EDIMessage)builderResult.Message;
				AssertMultilineASCIIEquals("MinReleaseMessageContent", expectedMinResult, message.EM_FormattedMessageText);
			}

			Declaration.CA_ServiceOption = ServiceOptions.Codes.PARS;
			Declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			testEDIReleaseMessageWrapper = new EDIReleaseImportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, testEDIReleaseMessageWrapper);
			foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
			{
				var message = (EDIMessage)builderResult.Message;
				AssertMultilineASCIIEquals("AQReleaseMessageContent", expectedAQResult, message.EM_FormattedMessageText);
			}

			Declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			testEDIReleaseMessageWrapper = new EDIReleaseImportMessageWrapper(Declaration.CustomsEntryHeaders[0]);
			builder = new EDIReleaseMessageBuilder(MessageSubTypes.Create, testEDIReleaseMessageWrapper);
			foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
			{
				var message = (EDIMessage)builderResult.Message;
				AssertMultilineASCIIEquals("OGDReleaseMessageContent", expectedOGDResult, message.EM_FormattedMessageText);
				var expectedInterpretation = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business.Test\MessageBuilders\TestFiles\EDIReleaseMessageInterpretation.html");
				AssertMultilineASCIIEquals("EDI Release Message Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
			}
		}

		#region Expected Messages

		readonly ZString expectedMinResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345000067897+9
CST++471:105+2:117+987654321RM001:58++1:110
LOC+22+0351+3021
DTM+133:201006202200:203
MEA+WT+AAD+KGM:1002
MEA+WT+AAC+KGM:501
EQD+CN+TURE1111111
EQD+CN+TURE2222222
RFF+CN:2ITN12345678987654321
PAC+25++:::PKG
PAC+2++:::AMM
NAD+IM+++ABC IMPORTING COMPANY WITH A VERY L:ONG NAME+123 IMPORTER ST.+OTTAWA+ON+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++BROKER NAME
TOD+++:::DELIVERY INSTRUCTIONS LINEXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX:ADDITIONAL
MOA+39:343
UNS+D
DMS+INV123456789
MOA+39:1234.50:AUD
NAD+VN+++MAIN SUPPLIER+123 SUPPLIER ST.+MASCOT+NSW+2020+AU
NAD+BY+++PURCHASER NAME+BUYER ADDRESS 1+TORONTO+ON+K1C1C1+CA
NAD+UC+++CONSIGNEE NAME+CONSIGNEE ADDRESS 1+BEAVER LODGE+AB+K2D2D2+CA
NAD+EX+++EXPORTER NAME+EXPORTER ADDRESS 1+AUCKLAND+++CA
DOC+862
LOC+27+VAR+UNY
LIN+1++0301100000
MOA+38:100.00:AUD
TOD+5
LOC+27+MX
IMD+++TRDESC:::A LONG GOODS DESCRIPTION WHICH EXCE:EDS 35 CHARACTERS
LIN+1++0301990010
QTY+KGM:5.25
MOA+146:19.0476
MOA+38:100.00:AUD
TOD+5
LOC+27+UAK
IMD+++TRDESC:::AN EXTREMELY LONG GOODS DESCRIPTION:WHICH EXCEEDS 140 CHARACTERS 1234*
FTX+AAA+++6789012345678901234567890123456789012345678901234567890123456789012345
LIN+1
QTY+PCE:2
MOA+146:250.00
MOA+38:500.00:AUD
TOD+5
IMD+++TRDESC:::A SHORT GOODS DESCRIPTION
DMS+INV2
MOA+39:1.00:CAD
NAD+VN+++ALTERNATE SUPPLIER NAME+ALT SUPPLIER ADDRESS+MELBOURNE+VIC+3000+AU
DOC+862
LOC+27+UAL
LIN+1++0301100000
IMD+++TRDESC:::DESCRIPTION
DMS+INV3
NAD+VN+++MAIN SUPPLIER+123 SUPPLIER ST.+MASCOT+NSW+2020+AU
DOC+862
LOC+27+NZ+GB
LIN+1++0301100000
IMD+++TRDESC:::DESCRIPTION
UNS+S
UNT+60+<<MSGNO PLACEHOLDER>>";

		readonly ZString expectedAQResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345000067897+9
CST++125:105+1:117+987654321RM001:58++1:110
LOC+22+0351+3021
DTM+232:200906202200:203
MEA+WT+AAD+KGM:1002
MEA+WT+AAC+KGM:501
EQD+CN+TURE1111111
EQD+CN+TURE2222222
RFF+CN:2ITN12345678987654321
PAC+25++:::PKG
PAC+2++:::AMM
NAD+IM+++ABC IMPORTING COMPANY WITH A VERY L:ONG NAME+123 IMPORTER ST.+OTTAWA+ON+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++BROKER NAME
TOD+++:::DELIVERY INSTRUCTIONS LINEXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX:ADDITIONAL
MOA+39:343
UNS+D
DMS+INV123456789
DTM+3:20090428:102
MOA+39:1234.50:AUD
MOA+144:74
MOA+105:98
MOA+107:147
TOD+++:::OTHER REFERENCE
NAD+VN+++MAIN SUPPLIER+123 SUPPLIER ST.+MASCOT+NSW+2020+AU
NAD+BY+++PURCHASER NAME+BUYER ADDRESS 1+TORONTO+ON+K1C1C1+CA
NAD+UC+++CONSIGNEE NAME+CONSIGNEE ADDRESS 1+BEAVER LODGE+AB+K2D2D2+CA
NAD+OS+++SHIPPER NAME+SHIPPER ADDRESS+AUCKLAND+++NZ
NAD+EX+++EXPORTER NAME+EXPORTER ADDRESS 1+AUCKLAND+++CA
DOC+862:::DEPARTMENTAL RULINGS+::BRISBANE
DTM+253:20090428:102
LOC+27+VAR+UNY+SG
PAT+1+6:::CONDITIONS OF SALE:TERMS OF PAYMENT
ALC+G+10
LIN+1++0301100000+:1
MOA+146:0.00
MOA+38:100.00:AUD
TOD+5
LOC+27+MX
IMD+++TRDESC:::A LONG GOODS DESCRIPTION WHICH EXCE:EDS 35 CHARACTERS
LIN+1++0301990010+:2
QTY+KGM:5.25
MOA+146:19.0476
MOA+38:100.00:AUD
TOD+5
LOC+27+UAK
IMD+++TRDESC:::AN EXTREMELY LONG GOODS DESCRIPTION:WHICH EXCEEDS 140 CHARACTERS 1234*
FTX+AAA+++6789012345678901234567890123456789012345678901234567890123456789012345
LIN+1+++:3
QTY+PCE:2
MOA+146:250.00
MOA+38:500.00:AUD
TOD+5
IMD+++TRDESC:::A SHORT GOODS DESCRIPTION
DMS+INV2
DTM+3:20090629:102
MOA+39:1.00:CAD
MOA+145:15
MOA+209:50
MOA+106:60
NAD+VN+++ALTERNATE SUPPLIER NAME+ALT SUPPLIER ADDRESS+MELBOURNE+VIC+3000+AU
DOC+862
LOC+27+UAL
ALC+G+01
LIN+2++0301100000+:1
MOA+146:0.00
MOA+38:0.00:CAD
IMD+++TRDESC:::DESCRIPTION
DMS+INV3
DTM+3:20090615:102
MOA+39:0.00:CAD
NAD+VN+++MAIN SUPPLIER+123 SUPPLIER ST.+MASCOT+NSW+2020+AU
DOC+862
LOC+27+NZ+GB
LIN+3++0301100000+:1
MOA+146:0.00
MOA+38:0.00:CAD
IMD+++TRDESC:::DESCRIPTION
UNS+S
UNT+81+<<MSGNO PLACEHOLDER>>";

		readonly ZString expectedOGDResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96A:UN
BGM++12345000067897+9
CST++463:105+1:117+987654321RM001:58++1:110
LOC+22+0351+3021
DTM+232:200906202200:203
GIS+18:::1
MEA+WT+AAD+KGM:1002
MEA+WT+AAC+KGM:501
EQD+CN+TURE1111111
EQD+CN+TURE2222222
RFF+CN:2ITN12345678987654321
PAC+25++:::PKG
PAC+2++:::AMM
NAD+IM+++ABC IMPORTING COMPANY WITH A VERY L:ONG NAME+123 IMPORTER ST.+OTTAWA+ON+K1A1A1+CA
NAD+CA+++CUSTOM TRUCKING CO.
NAD+AE+++BROKER NAME
NAD+DP+++DELIVERY NAME+DELIVERY ADDRESS 1:DELIVERY ADDRESS 2+DELIVERY CITY+BC+K1B1B1
COM+2345678901:TE
COM+2345679999:FX
TOD+++:::DELIVERY INSTRUCTIONS LINEXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX:ADDITIONAL
MOA+39:343
UNS+D
DMS+INV123456789
DTM+3:20090428:102
MOA+39:1234.50:AUD
MOA+144:74
MOA+105:98
MOA+107:147
TOD+++:::OTHER REFERENCE
NAD+VN+++MAIN SUPPLIER+123 SUPPLIER ST.+MASCOT+NS+2020+AU
NAD+BY+++PURCHASER NAME+BUYER ADDRESS 1+TORONTO+ON+K1C1C1+CA
NAD+UC+++CONSIGNEE NAME+CONSIGNEE ADDRESS 1+BEAVER LODGE+AB+K2D2D2+CA
NAD+OS+++SHIPPER NAME+SHIPPER ADDRESS+AUCKLAND+++NZ
NAD+EX+++EXPORTER NAME+EXPORTER ADDRESS 1+AUCKLAND+++CA
NAD+MF+++MANUFACTURER NAME+MANUFACTURER ADDRESS 1+TORONTO+ON+K1C1C1+CA
DOC+862:::DEPARTMENTAL RULINGS+::BRISBANE
DTM+253:20090428:102
LOC+27+VAR+UNY+SG
PAT+1+6:::CONDITIONS OF SALE:TERMS OF PAYMENT
ALC+G+10
LIN+1++0301100000+:1
MOA+146:0.00
MOA+38:100.00:AUD
TOD+5
LOC+27+MX
IMD+++TRDESC:::A LONG GOODS DESCRIPTION WHICH EXCE:EDS 35 CHARACTERS
LIN+1++0301990010+:2
PIA+1++DESIGN/STYLE/STRUCTURE:MF+MODEL ID 12355:MN+MODEL BRAND:MP
QTY+KGM:5.25
MEA+ABC+:::HP/RPM
MOA+146:19.0476
MOA+38:100.00:AUD
GIR+1+AIRS1234+2+AIRS12+AB+END
GIR+7+MID
GIR+2+CFIA12345+COD
GIR+2+SITT12345+IC
GIR+3+3:AB+1:AC+TIN:AD
TOD+5
LOC+27+UAK+UAL
IMD+++TRDESC:::AN EXTREMELY LONG GOODS DESCRIPTION:WHICH EXCEEDS 140 CHARACTERS 1234*
FTX+AAA+++6789012345678901234567890123456789012345678901234567890123456789012345
IMD++5+02
LIN+1+++:3
QTY+PCE:2
MOA+146:250.00
MOA+38:500.00:AUD
TOD+5
LOC+27
IMD+++TRDESC:::A SHORT GOODS DESCRIPTION
DMS+INV2
DTM+3:20090629:102
MOA+39:1.00:CAD
MOA+145:15
MOA+209:50
MOA+106:60
NAD+VN+++ALTERNATE SUPPLIER NAME+ALT SUPPLIER ADDRESS+MELBOURNE+VI+3000+AU
NAD+MF+++MANUFACTURER NAME+MANUFACTURER ADDRESS 1+TORONTO+ON+K1C1C1+CA
DOC+862
LOC+27+UAL
ALC+G+01
LIN+2++0301100000+:1
MOA+146:0.00
MOA+38:0.00:CAD
IMD+++TRDESC:::DESCRIPTION
DMS+INV3
DTM+3:20090615:102
MOA+39:0.00:CAD
NAD+VN+++MAIN SUPPLIER+123 SUPPLIER ST.+MASCOT+NS+2020+AU
NAD+MF+++MANUFACTURER NAME+MANUFACTURER ADDRESS 1+TORONTO+ON+K1C1C1+CA
DOC+862
LOC+27+NZ+GB
LIN+3++0301100000+:1
MOA+146:0.00
MOA+38:0.00:CAD
IMD+++TRDESC:::DESCRIPTION
UNS+S
UNT+97+<<MSGNO PLACEHOLDER>>";

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var cad = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Canada);
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "~~";
			broker.GS_FullName = "BROKER NAME";
			helper = new DeclarationTestHelper(Factory, true);
			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			var depot = helper.CreateOrganisation("DPT", "DEPOT NAME", "CATOR", "DEPOT ADDRESS", "DEPOT CITY", "123 4567");
			var supplier2 = helper.CreateOrganisation("SUP", "ALTERNATE SUPPLIER NAME", "AUMEL", "ALT SUPPLIER ADDRESS", "MELBOURNE", "VIC", "3000", "123 4567");
			var manufacturer = helper.CreateOrganisation("MFT", "MANUFACTURER NAME", "CATOR", "MANUFACTURER ADDRESS", "TORONTO", "ON", "K1C1C1", "123 4567");
			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturerAddress.OA_City = "TORONTO";
			manufacturerAddress.OA_State = "ON";
			manufacturerAddress.OA_PostCode = "K1C1C1";
			var buyer = helper.CreateOrganisation("BUY", "PURCHASER NAME", "CATOR", "PURCHASER ADDRESS", "TORONTO", "ON", "K1C1C1", "123 4567");
			var buyerAddress = buyer.Addresses.AddNew();
			buyerAddress.OA_Address1 = "BUYER ADDRESS 1";
			buyerAddress.OA_City = "TORONTO";
			buyerAddress.OA_State = "ON";
			buyerAddress.OA_PostCode = "K1C1C1";
			var consignee = helper.CreateOrganisation("CNE", "CONSIGNEE NAME", "CABLO", "CONSIGNEE ADDRESS", "BEAVER LODGE", "AB", "K1D1D1", "123 4567");
			var consigneeAddress = consignee.Addresses.AddNew();
			consigneeAddress.OA_Address1 = "CONSIGNEE ADDRESS 1";
			consigneeAddress.OA_City = "BEAVER LODGE";
			consigneeAddress.OA_State = "AB";
			consigneeAddress.OA_PostCode = "K2D2D2";
			var shipper = helper.CreateOrganisation("SHP", "SHIPPER NAME", "NZAKL", "SHIPPER ADDRESS", "AUCKLAND", "123 4567");
			var exporter = helper.CreateOrganisation("EXP", "EXPORTER NAME", "NZAKL", "EXPORTER ADDRESS", "AUCKLAND", "123 4567");
			exporter.MainAddress.OA_State = string.Empty;
			var exporterAddress = exporter.Addresses.AddNew();
			exporterAddress.OA_Address1 = "EXPORTER ADDRESS 1";
			exporterAddress.OA_City = "AUCKLAND";
			exporterAddress.OA_State = string.Empty;
			exporterAddress.OA_PostCode = string.Empty;
			shipper.MainAddress.OA_State = string.Empty;
			depotCPP = depot.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "3021", canada);
			depotCPP.OK_OA_PremisesAddress = depot.MainAddress.PK;
			var declaration = Declaration;
			declaration.JE_OH_Supplier = helper.Consignor.PK;
			declaration.JE_OH_Importer = helper.Consignee.PK;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			helper.Consignee.OH_FullName = "ABC IMPORTING COMPANY WITH A VERY LONG NAME";
			helper.Consignee.MainAddress.OA_Address1 = "123 IMPORTER ST.";
			helper.Consignee.MainAddress.OA_City = "OTTAWA";
			helper.Consignee.MainAddress.OA_State = "ON";
			helper.Consignee.MainAddress.OA_PostCode = "K1A1A1";
			helper.Consignor.OH_FullName = "MAIN SUPPLIER";
			helper.Consignor.MainAddress.OA_Address1 = "123 SUPPLIER ST.";
			helper.Consignor.MainAddress.OA_City = "MASCOT";
			helper.Consignor.MainAddress.OA_State = "NSW";
			helper.Consignor.MainAddress.OA_PostCode = "2020";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			var delivery = declaration.ImporterDeliveryAddress;
			delivery.E2_AddressOverride = true;
			delivery.E2_CompanyName = "DELIVERY NAME";
			delivery.E2_Address1 = "DELIVERY ADDRESS 1";
			delivery.E2_Address2 = "DELIVERY ADDRESS 2";
			delivery.E2_City = "DELIVERY CITY";
			delivery.E2_Postcode = "K1B1B1";
			delivery.E2_State = "BC";
			delivery.E2_RN_NKCountryCode = "CA";
			delivery.E2_Phone = "(234)5678901";
			delivery.E2_Fax = "+1 234 567 9999";
			declaration.JE_DateOfFirstArrival = new ZDateTime(2009, 6, 20, 22, 0, 0);
			declaration.JE_EntryAuthorisationDate = new DateTime(2010, 6, 20);
			declaration.JE_WarehouseReleaseDate = new ZDateTime(2010, 6, 20, 22, 0, 0);
			declaration.JE_MasterBill = "MASTER";
			declaration.JE_TotalNoOfPacks = 25;
			declaration.JE_TotalNoOfPacksPackType = ACROSSPackageTypes.Codes.PACKAGE;
			declaration.CA_OGDCFIA = true;
			declaration.CA_OGDIC = true;
			declaration.CA_OGDNR = true;
			declaration.CA_OGDTC = true;
			declaration.CA_ServiceOption = ServiceOptions.Codes.RMDOGD;
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			importerBMR = helper.Consignee.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "987654321RM001", canada);
			declaration.CA_PriorityInd = "1";
			declaration.JE_CustomsOffice = "351";
			declaration.DepotDocAddress.E2_OA_Address = depot.MainAddress.PK;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "2ITN12345678987654321";
			declaration.CA_CarrierName = "CUSTOM TRUCKING CO.";
			var pack2 = declaration.PackingGroups[0].Packages.AddNew();
			pack2.CW_PackQty = 2;
			pack2.CW_PackType = ACROSSPackageTypes.Codes.AMMOPACK;
			helper.CreateCusContainer(declaration, "TURE1111111", "SEAL1", helper.Container40US, "FCL");
			helper.CreateCusContainer(declaration, "TURE2222222", "SEAL2", helper.Container40US, "FCL");
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_Weight = 1.5m;
			invoice1.JZ_WeightUQ = "KG";
			invoice1.JZ_NetWeight = 1.4m;
			invoice1.JZ_InvoiceAmount = 1234.50m;
			invoice1.JZ_RX_NKInvoice_Currency = helper.AUD.RX_Code;
			invoice1.JZ_InvoiceNumber = "INV123456789";
			invoice1.JZ_InvoiceDate = new ZDateTime(2009, 6, 30);
			invoice1.JZ_IncoTerm = "CIF";
			var invoice1Charge1 = invoice1.Charges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 100m);
			var invoice1Charge2 = invoice1.Charges.AddNew(CAChargeTypeList.Codes.OverseasInsurance, 50m);
			var invoice1Charge3 = invoice1.Charges.AddNew(CAChargeTypeList.Codes.Construction, 200m);
			var invoice1Charge4 = invoice1.Charges.AddNew(CAChargeTypeList.Codes.PackingCost, 300.60m);
			invoice1Charge4.J7_IsIncludedInITOT = true;
			invoice1.CA_OtherReference = "OTHER REFERENCE";
			invoice1.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoice1.BuyerDocumentaryAddress.E2_OA_Address = buyerAddress.PK;
			invoice1.FinalConsigneeAddress.E2_OA_Address = consigneeAddress.PK;
			invoice1.SupplierPickupDeliveryAddress.OrganisationPK = shipper.PK;
			invoice1.ExporterDocumentaryAddress.E2_OA_Address = exporterAddress.PK;
			invoice1.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			invoice1.CA_USStateOfExport = USStatesList.Codes.NewYork;
			invoice1.CA_DepartmentRuling = "DEPARTMENTAL RULINGS";
			invoice1.CA_RL_NKLastPort = "AUBNE";
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2009, 4, 28);
			invoice1.CA_RN_NKTranshipment = "SG";
			invoice1.CA_ConditionsOfSale = "CONDITIONS OF SALE";
			invoice1.CA_TermsOfPayment = "TERMS OF PAYMENT";
			invoice1.CA_RoyaltyInd = true;
			invoice1.JZ_RN_NKDefaultOrigin = ZString.Empty;
			invoice1Charge1.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoice1Charge2.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoice1Charge3.J7_Calc_IsIncludedInInvoiceAmount = true;
			invoice1Charge4.J7_Calc_IsIncludedInInvoiceAmount = true;
			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_Weight = 1m;
			invoice2.JZ_WeightUQ = "T";
			invoice2.JZ_NetWeight = 0.5m;
			invoice2.JZ_NetWeightUQ = "T";
			invoice2.JZ_InvoiceAmount = 1m;
			invoice2.JZ_RX_NKInvoice_Currency = cad.RX_Code;
			invoice2.JZ_InvoiceNumber = "INV2";
			invoice2.JZ_InvoiceDate = new ZDateTime(2009, 6, 29);
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.CA_OtherReference = "";
			invoice2.JZ_OH_Buyer = ZGuid.Empty;
			invoice2.JZ_OH_Consignee = ZGuid.Empty;
			invoice2.SupplierPickupDeliveryAddress.OrganisationPK = ZGuid.Empty;
			invoice2.ExporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoice2.JZ_OH_Supplier = supplier2.PK;
			invoice2.CA_RN_NKExport = "";
			invoice2.CA_USStateOfExport = "";
			invoice2.CA_DepartmentRuling = "";
			invoice2.CA_RL_NKLastPort = "";
			invoice2.JZ_ValuationDateOverride = ZDateTime.Empty;
			invoice2.CA_RN_NKTranshipment = "";
			invoice2.CA_ConditionsOfSale = "";
			invoice2.CA_TermsOfPayment = "";
			invoice2.CA_RoyaltyInd = false;
			invoice2.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice2.JZ_RW_NKOriginState = USStatesList.Codes.Alabama;
			invoice2.CA_ServicesInd = true;
			var invoice3 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice3.JZ_OH_Supplier = ZGuid.Empty;
			invoice3.JZ_RX_NKInvoice_Currency = cad.RX_Code;
			invoice3.JZ_InvoiceNumber = "INV3";
			invoice3.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.NewZealand;
			invoice3.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedKingdom;
			invoice3.CA_ServicesInd = false;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_LineNo = 1;
			invoice1Line1.CA_PageNumber = 1;
			invoice1Line1.JI_Tariff = "0301100000";
			invoice1Line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoice1Line1.JI_Description = "A LONG GOODS DESCRIPTION WHICH EXCEEDS 35 CHARACTERS";
			invoice1Line1.JI_LinePrice = 100m;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			//Min
			invoice1Line2.JI_Tariff = "0301990010";
			invoice1Line2.JI_CustomsQuantity = 5.25m;
			invoice1Line2.JI_CustomsUnitQty = "KGM";
			invoice1Line2.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice1Line2.JI_StateOrRegionOfOrigin = USStatesList.Codes.Alaska;
			invoice1Line2.JI_Description = "AN EXTREMELY LONG GOODS DESCRIPTION WHICH EXCEEDS 140 CHARACTERS 1234*678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234*67890";
			//AQ
			invoice1Line2.JI_LineNo = 2;
			invoice1Line2.CA_PageNumber = 1;
			invoice1Line2.JI_LinePrice = 100m;
			//OGD
			invoice1Line2.CA_ImportReasonCode = ImportReasonCodes.Codes.Export;
			invoice1Line2.SITTCertificationNumbers.AddNew("SITT12345");
			invoice1Line2.CFIARegistrationNumbers.AddNew("COD", "CFIA12345");
			invoice1Line2.CA_TIIN = "TIN";
			invoice1Line2.CA_CompliantCompletion = true;
			invoice1Line2.CA_CompliantImportDate = true;
			invoice1Line2.CA_Model = "DESIGN/STYLE/STRUCTURE";
			invoice1Line2.CA_ModelNumber = "MODEL ID 12355";
			invoice1Line2.JI_BrandName = "MODEL BRAND";
			invoice1Line2.CA_TypeSize = "HP/RPM";
			invoice1Line2.CA_RequirementID = "AIRS1234";
			invoice1Line2.CA_RequirementVer = "2";
			invoice1Line2.CA_AirsCode = "AIRS12";
			invoice1Line2.CA_DestinationProvince = CanadianProvinceList.Codes.Alberta;
			invoice1Line2.CA_EndUse = "END";
			invoice1Line2.CA_MiscID = "MID";
			invoice1Line2.CA_RN_NKCFIAOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoice1Line2.CA_CFIAUSStateOfOrigin = USStatesList.Codes.Alabama;
			var invoice1Line3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line3.JI_LineNo = 3;
			invoice1Line3.CA_PageNumber = 1;
			invoice1Line3.JI_CustomsQuantity = 2m;
			invoice1Line3.JI_CustomsUnitQty = ACROSSPackageTypes.Codes.PIECE;
			invoice1Line3.JI_Description = "A SHORT GOODS DESCRIPTION";
			invoice1Line3.JI_LinePrice = 500m;
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_LineNo = 1;
			invoice2Line1.CA_PageNumber = 1;
			invoice2Line1.JI_Tariff = "0301100000";
			invoice2Line1.JI_Description = "DESCRIPTION";
			var invoice3Line1 = invoice3.JobComInvoiceLines.AddNew();
			invoice3Line1.JI_LineNo = 1;
			invoice3Line1.CA_PageNumber = 1;
			invoice3Line1.JI_Tariff = "0301100000";
			invoice3Line1.JI_Description = "DESCRIPTION";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.DoMerge();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_CustomsDeliveryInstructions = "DELIVERY INSTRUCTIONS LINEXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXADDITIONAL";
			invoice2.GroupCharges.AddNew(CAChargeTypeList.Codes.OverseasFreight, 10m, cad.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoice2.GroupCharges.AddNew(CAChargeTypeList.Codes.OverseasInsurance, 5m, cad.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			var invoice2Charge3 = invoice2.GroupCharges.AddNew(CAChargeTypeList.Codes.Commission, 50m, cad.RX_Code);
			invoice2Charge3.J7_Calc_IsIncludedInInvoiceAmount = false;
			var invoice2Charge4 = invoice2.GroupCharges.AddNew(CAChargeTypeList.Codes.PackingCost, 60m, cad.RX_Code);
			invoice2Charge4.J7_Calc_IsIncludedInInvoiceAmount = false;

			importerOrRecord = helper.CreateOrganisation("IMPORTER OF RECORD NAME", "NZAKL", "IMPORTER OF RECORD ADDRESS", "AUCKLAND", "123 4567");
			importerIMR = importerOrRecord.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "987654321RM002", canada);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		OrgCusCode importerBMR;
		OrgCusCode importerIMR;
		OrgCusCode depotCPP;
		OrgHeader importerOrRecord;
		DeclarationTestHelper helper;

		#endregion Implementation
	}
}
