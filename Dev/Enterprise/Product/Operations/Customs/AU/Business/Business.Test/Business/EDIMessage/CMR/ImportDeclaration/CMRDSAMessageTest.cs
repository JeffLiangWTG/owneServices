using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRDSAMessage))]
	sealed class CMRDSAMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestSetDefaultValues()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			AssertEquals("Message type is set", CMRMessage.CMRMessageTypes.DSA, message.EM_MessageType);
		}

		public void TestGetStatus()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			AssertEquals("GetStatus", "FINALISED", message.GetStatus());
		}

		public void TestGetCargoStatusForTransportLine()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			AssertEquals("Cargo Status TransportLine 1", "HELD", message.GetCargoStatusForTransportLine(1));
			AssertEquals("Cargo Status TransportLine 2", "CLEAR", message.GetCargoStatusForTransportLine(2));
		}

		public void TestGetCargoStatusForTransportLine_NotContainerised()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			AssertEquals("Cargo Status", "HELD", message.GetCargoStatusForTransportLine(0));
		}

		public void TestAbbreviatedStatusDescriptionForTransportLine()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			AssertEquals("Abbreviated Status TransportLine 1", "Y/N/Y/Y", message.AbbreviatedStatusDescriptionForTransportLine(1));
			AssertEquals("Abbreviated Status TransportLine 2", "Y/Y/Y/Y", message.AbbreviatedStatusDescriptionForTransportLine(2));
			AssertEquals("Abbreviated Status TransportLine 3", "", message.AbbreviatedStatusDescriptionForTransportLine(3));
		}

		public void TestAbbreviatedStatusDescriptionForTransportLine_NotContainerised()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			AssertEquals("Abbreviated Status", "Y/N/Y/Y", message.AbbreviatedStatusDescriptionForTransportLine(0));
		}

		public void TestGetCustomsStatusFromMessage()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			AssertEquals("GetStatus", "FINALISED", message.GetCustomsStatusFromMessage());
		}

		public void TestGetReportForDSAMessage()
		{
			var expectedEmailBody =
				@"Reference Number: S00001178/3
Declaration Reference: S00001178
Housebill: DSAH001B
Masterbill: DSA001
Entry Number: AAACG7XHL
Consignor: I am a supplier
Consignee: I am an importer
Origin: GBLON
Destination: AUSYD

Impediments:
Agency / Program: QUARANTINE
Impediment Document Version Number: 4
Impediment Type: Matched a community protection profile
Risk Identifier: 190: QUARANTINE: HIGH RISK
Risk Line Number: 2

Agency / Program: ACS
Impediment Document Version Number: 4
Impediment Type: Matched a community protection profile
Risk Identifier: 215: ARE THESE GOODS MADE OF, OR CONTAIN, CITES LISTED SPECIES?
Risk Line Number: 2

Agency / Program: ACS
Impediment Document Version Number: 3

Agency / Program: QUARANTINE
Impediment Document Version Number: 3
Impediment Type: Conditional release
Advice Note: PENDING AQIS ACTION (QUARANTINE)

Status: FINALISED

Status for Packing (Transport) Line: 1
***CONSOLIDATED CARGO STATUS: ***HELD***
ACSDec/ACSCR/AQISDec/AQISCR: Y/N/Y/Y
HB:DSAH001B (MB:DSA001)
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: HELD
CARGO REPORT ACS EVALUATED: NO
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO

Status for Packing (Transport) Line: 2
***CONSOLIDATED CARGO STATUS: ***CLEAR***
ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: CLEAR
CARGO REPORT ACS EVALUATED: YES
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO
";
			var message = Factory.New<CMRDSAMessage>();
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = AUJobMessageTypeList.Codes.Import;
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S00001178/3";
			testDec.JE_DeclarationReference = "S00001178";
			testDec.JE_HouseBill = "DSAH001B";
			testDec.JE_MasterBill = "DSA001";
			entryHeader.EntryNumber = "AAACG7XHL";
			var importer = OrgHeader.New(Factory);
			importer.OH_FullName = "I am an importer";
			var supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "I am a supplier";
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_OH_Supplier = supplier.PK;
			testDec.JE_RL_NKOrigin = "GBLON";
			testDec.JE_RL_NKFinalDestination = "AUSYD";
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			message.EM_LinkedObject = entryHeader;
			AssertMultilineASCIIEquals("Report", expectedEmailBody, message.GetReport());

			var expectedOneLineReport =
				@"Status for Packing (Transport) Line: 2
***CONSOLIDATED CARGO STATUS: ***CLEAR***
ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: CLEAR
CARGO REPORT ACS EVALUATED: YES
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO
";
			AssertMultilineASCIIEquals("ExpectedOneLineReport", expectedOneLineReport, message.GetStatusDescriptionOfLine(2));
		}

		public void TestGetStatusDescriptionOfLine_NotContainerised()
		{
			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;

			var expectedDetails = @"Status for Packing (Transport) Line: 1
***CONSOLIDATED CARGO STATUS: ***HELD***
ACSDec/ACSCR/AQISDec/AQISCR: Y/N/Y/Y
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: HELD
CARGO REPORT ACS EVALUATED: NO
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO
";
			AssertMultilineASCIIEquals("GetStatusDescriptionOfLine", expectedDetails, message.GetStatusDescriptionOfLine(0));
		}

		public void TestStatusOfLinesReport()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MasterBill = "08657962155";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S00001178/3";

			var packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CU_HouseBill = declaration.PrimaryMasterBill.PK;
			var pack1 = packGroup1.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_HouseBill = "HB1";

			var container = declaration.CusContainers.AddNew();
			packGroup1.CR_CO_Container = container.PK;
			packGroup1.CR_HouseContainerNumber = 1;

			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			message.EM_LinkedObject = entryHeader;

			ZString expectedReport =
				@"Status for Packing (Transport) Line: 1
***CONSOLIDATED CARGO STATUS: ***HELD***
ACSDec/ACSCR/AQISDec/AQISCR: Y/N/Y/Y
HB1
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: HELD
CARGO REPORT ACS EVALUATED: NO
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO

Status for Packing (Transport) Line: 2
***CONSOLIDATED CARGO STATUS: ***CLEAR***
ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: CLEAR
CARGO REPORT ACS EVALUATED: YES
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO
";
			AssertMultilineASCIIEquals("Report includes Bill Number", expectedReport, message.StatusOfLinesReport);
		}

		public void TestStatusOfLinesReport_NotContainerised()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MasterBill = "08657962155";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S00001178/3";

			var packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CU_HouseBill = declaration.PrimaryMasterBill.PK;
			var pack1 = packGroup1.Packages.AddNew();
			pack1.CW_PackQty = 1;
			pack1.CW_HouseBill = "HB1";

			CMRDSAMessage message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA;
			message.EM_LinkedObject = entryHeader;

			ZString expectedReport =
				@"Status for Packing (Transport) Line: 1
***CONSOLIDATED CARGO STATUS: ***HELD***
ACSDec/ACSCR/AQISDec/AQISCR: Y/N/Y/Y
HB1
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: HELD
CARGO REPORT ACS EVALUATED: NO
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO

Status for Packing (Transport) Line: 2
***CONSOLIDATED CARGO STATUS: ***CLEAR***
ACSDec/ACSCR/AQISDec/AQISCR: Y/Y/Y/Y
Screening Period Expiry: 10Jul2008 03:16 
Customs Indicator: Linked to Cargo Report Line 
CONSOLIDATED STATUS: CLEAR
CARGO REPORT ACS EVALUATED: YES
LCL UNDERBOND SATISFIED: NO
DECONSOL UNDERBOND SATISFIED: NO
";
			AssertMultilineASCIIEquals("Report includes Bill Number", expectedReport, message.StatusOfLinesReport);
		}

		public void TestDSAException()
		{
			var message = Factory.New<CMRDSAMessage>();
			message.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+3C4J J9B5 270G:1+11'DTM+9:20160706110200000000:ZZZ'TDT+20++A'NAD+MR+FGW649G::95'NAD+VT+AA37CY::95'NAD+CB+581::95+CLEMENGER INTERNATIONAL FREIGHT PTY:. LTD'NAD+IM++DKSH AUSTRALIA PTY. LTD.'RFF+ABO:S00223511/1/MEL4::4'RFF+ACW:FID'RFF+AAE:N10'RFF+ABT:ACP9PL3JL::1'RFF+ABQ:8279015069'RFF+ADU:S00223511'RFF+AMI:FINALISED'DOC+S+1'DTM+192:20160624:102'DTM+192:1322:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'DOC+S+1'DTM+192:20160705:102'DTM+192:0513:401'GIS+LCL:109:95'CST+1'FTX+AHN+++CONSOLIDATED STATUS:CLEAR'UNT+28+000001'";

			AssertNoExceptionThrown(delegate
			{ message.GetCargoStatusForTransportLine(1); });
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRDSAMessageTestClass message = Factory.New<CMRDSAMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.DSA.Replace("S00001178", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRDSAMessageTestClass message = (CMRDSAMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
