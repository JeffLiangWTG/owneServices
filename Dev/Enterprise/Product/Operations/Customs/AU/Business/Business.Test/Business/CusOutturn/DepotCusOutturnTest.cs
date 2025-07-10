using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DepotCusOutturn))]
	sealed class DepotCusOutturnTest : CusOutturnTest
	{
		public void TestPubishCustomsStatusChangedEvent_AwaitingResponseToOriginal()
		{
			var header = Factory.NewWithValidTestData<CusOutturnHeader>();

			var outturn = header.Outturns.AddNew();
			outturn.FillWithValidTestData();

			outturn.C5_MasterBill = "MB200413";
			outturn.C5_HouseBill = "HB200413";
			outturn.C5_ContainerNumber = "CON000";

			outturn.C5_CustomsStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();

			var log = outturn.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
			AssertNull("Default to null.", log);

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageNum = "TST00000";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			outturn.Messages.Add(message);
			outturn.StatusCalculator.DeriveStatusNow();

			AssertEquals("Should update C5_CustomsStatus.", CMRBaseStatuses.Codes.AwaitingResponseToOriginal, outturn.C5_CustomsStatus);

			Factory.Save();

			log = outturn.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
			AssertNotNull("Should create a CustomsEntryStatus event.", log);
			AssertEquals("Should log C5_CustomsStatus.", "|RES=WTO|SER=PCS", log.SL_Reference);
		}

		public void TestPubishCustomsStatusChangedEvent_WithConsolidatedCargoStatus()
		{
			var heldMessage = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1GAG D03A D06F:1+8'
DTM+9:20051110003152681932:ZZZ'
DTM+132:20051110:102'
FTX+AHN+++CONSOLIDATED STATUS:CONDCLEAR'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:CONDITIONAL RELEASE'
FTX+AHN+++ACS/AQIS IMPEDIMENT DETAILS:TEST'
TDT +20+006++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+8553P::95'
NAD+MR+FGH939C::95'
NAD+UD+83003926181::95'
RFF+ABO:321/PRD1::1'
RFF+MWB:08145322174'
RFF+HWB:V0014102928'
UNT+34+000001'
";
			var header = Factory.NewWithValidTestData<CusOutturnHeader>();

			var outturn = header.Outturns.AddNew();
			outturn.FillWithValidTestData();

			outturn.C5_MasterBill = "MB200413";
			outturn.C5_HouseBill = "HB200413";
			outturn.C5_ContainerNumber = "CON000";

			outturn.C5_CustomsStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();

			var log = outturn.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
			AssertNull("Default to null.", log);

			var message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageNum = "TST00000";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			message.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = heldMessage.Replace("\r\n", "");

			outturn.Messages.Add(message);
			outturn.StatusCalculator.DeriveStatusNow();

			AssertEquals("Should update C5_CustomsStatus.", CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation,
				outturn.C5_CustomsStatus);

			Factory.Save();

			log = outturn.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
			AssertNotNull("Should create a CustomsEntryStatus event.", log);
			AssertEquals("Should log C5_CustomsStatus.", "|RES=CCL|SER=PCS|TYP=CLR", log.SL_Reference);
		}

		public void TestSetterSuspender()
		{
			var outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_OuterPackUnits = "BG";
			outturn.C5_OuterPacks = 10;
			outturn.C5_PackagesUnits = "BG";
			outturn.C5_PackagesOutturned = 5;
			Factory.Save();

			AssertEquals("C5_OutturnResultType: current value", "SH", outturn.C5_OutturnResultType);
			outturn.SetterSuspender.SuspendSetting(Customs.Business.AutoCusOutturn.Schema.C5_OutturnResultType);

			outturn.C5_OutturnResultType = "NIL";
			Factory.Save();

			AssertEquals("C5_OutturnResultType:setter suspended", true, outturn.SetterSuspender.IsSetterSuspended(Customs.Business.AutoCusOutturn.Schema.C5_OutturnResultType));
			AssertEquals("C5_OutturnResultType:not set new value", "SH", outturn.C5_OutturnResultType);

			outturn.SetterSuspender.ResumeSetting(Customs.Business.AutoCusOutturn.Schema.C5_OutturnResultType);

			outturn.C5_OuterPackUnits = "BG";
			outturn.C5_OuterPacks = 10;
			outturn.C5_PackagesUnits = "BG";
			outturn.C5_PackagesOutturned = 15;
			outturn.C5_OutturnResultType = "SU";
			Factory.Save();

			AssertEquals("C5_OutturnResultType:setter suspended", false, outturn.SetterSuspender.IsSetterSuspended(Customs.Business.AutoCusOutturn.Schema.C5_OutturnResultType));
			AssertEquals("C5_OutturnResultType:not set new value", "SU", outturn.C5_OutturnResultType);
		}

		public void TestOutturnStatus()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = "LCL";
			outturn.C5_ContainerNumber = "OCLU1233510";
			outturn.C5_HouseBill = "H3337C7";
			outturn.C5_MasterBill = "OBL3337";
			outturn.C5_CargoReceiptDate = new ZDateTime(2014, 2, 5, 12, 4, 0);
			outturn.C5_CargoUnpackDate = new ZDateTime(2014, 2, 5, 12, 4, 0);
			outturn.C5_OuterPacks = 4;
			outturn.C5_OuterPackUnits = "PK";
			outturn.C5_PackagesOutturned = 2;
			outturn.C5_PackagesUnits = "PK";

			AssertEquals(ZString.Empty, outturn.OutturnStatus);

			header.ResetCachedValuesForTesting();
			outturn.ResetCachedValuesForTesting();

			var sutTransmitMsg1 = Factory.New<CMRSEAOUTMessage>();
			sutTransmitMsg1.EM_LinkedObject = header;
			sutTransmitMsg1.EM_MessageSubType = "ORG";
			sutTransmitMsg1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sutTransmitMsg1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-50);
			sutTransmitMsg1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+263:::SEAOUT+O00000428/CMT1:4+4
NAD+VW+41065894724::95
TDT+20+3339++11++++7619410::11
LOC+4+9912J::95
CNI++:::I
RFF+AAQ:OCLU1233510
GID+1
RFF+ACU:SH
GID+1
RFF+BH:H3337C7
GID+1
RFF+MB:OBL3337
GIS+N:62:95
GIS+U:63:95
GIS+U:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20140205:102
DTM+420:0204:401
DTM+570:20140205:102
DTM+570:0204:401
GID+1
PAC+2
PAC+++PK:185:95
PAC+++LCL:67:95
UNT+30+1
".Replace("\r\n", "'");

			var sutReceiveMsg1 = Factory.New<CMRSEAOUTRMessage>();
			sutReceiveMsg1.EM_LinkedObject = header;
			sutReceiveMsg1.EM_MessageSubType = "CLR";
			sutReceiveMsg1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			sutReceiveMsg1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-49);
			sutReceiveMsg1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN
BGM+961:::SEAOUTR+2AF5 A882 H0F4:001+11
NAD+MR+AAA374M::95
RFF+ACW:SEAOUT
RFF+AFM:4
RFF+ABO:O00000428/CMT1::004
DTM+310:20140205040823:204
ERP+1
ERC+ADVICE:80:95
ERC+MS5203:6:95
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS
CNT+55:000
UNT+13+000001
".Replace("\r\n", "'");

			var sutReceiveMsg2 = Factory.New<CMRSEAOUTRMessage>();
			outturn.Messages.Add(sutReceiveMsg2);
			sutReceiveMsg2.EM_MessageSubType = "CLR";
			sutReceiveMsg2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			sutReceiveMsg2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-49);
			sutReceiveMsg2.EM_MessageText = @"BGM+961:::SEAOUTR+2AF5 A882 H0F4:001+11
NAD+MR+AAA374M::95
RFF+ACW:SEAOUT
RFF+AFM:4
RFF+ABO:O00000428/CMT1::004
DTM+310:20140205040823:204
ERP+1
ERC+ADVICE:80:95
ERC+MS5203:6:95
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS
CNT+55:000
UNT+13+000001
".Replace("\r\n", "'");

			AssertEquals(CMRMessage.CMRMessageStatusDescription.ACCEPTED, outturn.OutturnStatus);

			header.ResetCachedValuesForTesting();
			outturn.ResetCachedValuesForTesting();

			outturn.C5_PackagesOutturned = 3;
			AssertEquals(CMRMessage.CMRMessageStatusDescription.AMENDMENTDETECTED, outturn.OutturnStatus);

			var outturn2 = header.Outturns.AddNew();
			outturn2.C5_CargoType = "LCL";
			outturn2.C5_ContainerNumber = "OCLU1233510";
			outturn2.C5_HouseBill = "H3337C8";
			outturn2.C5_MasterBill = "OBL3337";
			outturn2.C5_CargoReceiptDate = new ZDateTime(2014, 2, 5, 14, 4, 0);
			outturn2.C5_CargoUnpackDate = new ZDateTime(2014, 2, 5, 14, 4, 0);
			outturn2.C5_OuterPacks = 1;
			outturn2.C5_OuterPackUnits = "PK";
			outturn2.C5_PackagesOutturned = 2;
			outturn2.C5_PackagesUnits = "PK";

			var sutTransmitMsg2 = Factory.New<CMRSEAOUTMessage>();
			sutTransmitMsg2.EM_LinkedObject = header;
			sutTransmitMsg2.EM_MessageSubType = "ORG";
			sutTransmitMsg2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sutTransmitMsg2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-50);
			sutTransmitMsg2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+263:::SEAOUT+O00000428/CMT1:4+4
NAD+VW+41065894724::95
TDT+20+3339++11++++7619410::11
LOC+4+9912J::95
CNI++:::I
RFF+AAQ:OCLU1233510
GID+1
RFF+ACU:SH
GID+1
RFF+BH:H3337C8
GID+1
RFF+MB:OBL3337
GIS+N:62:95
GIS+U:63:95
GIS+U:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20140205:102
DTM+420:0204:401
DTM+570:20140205:102
DTM+570:0204:401
GID+1
PAC+2
PAC+++PK:185:95
PAC+++LCL:67:95
UNT+30+1
".Replace("\r\n", "'");

			var sutReceiveMsg3 = Factory.New<CMRSEAOUTRMessage>();
			sutReceiveMsg3.EM_LinkedObject = header;
			sutReceiveMsg3.EM_MessageSubType = "CLR";
			sutReceiveMsg3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			sutReceiveMsg3.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-49);
			sutReceiveMsg3.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN
BGM+961:::SEAOUTR+46DB GE97 H0F4:001+11
NAD+MR+AAA374M::95
RFF+ACW:SEAOUT
RFF+AFM:4
RFF+ABO:O00000428/CMT1::003
DTM+310:20140205040551:204
ERP+1
ERC+ADVICE:80:95
ERC+MS5201:6:95
FTX+AAO+++THIS TRANSACTION WAS REJECTED
ERP+1
ERC+ERROR:80:95
ERC+CG1968:6:95
FTX+AAO+++CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510 MARKS AND NUMBERS ARE MANDATORY IF CARGO TYPE IS ""L CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510,OBL=OBL3337,HBL=H3337C7
ERP+1
ERC+ERROR:80:95
ERC+CG1960:6:95
FTX+AAO+++CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510 GOODS DESCRIPTION IS MANDATORY WHEN CARGO TYPE IS "" CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510,OBL=OBL3337,HBL=H3337C7
ERP+1
ERC+ERROR:80:95
ERC+CG2005:6:95
FTX+AAO+++AT LEAST ONE LINE DETAIL IS MANDATORY
CNT+55:003
UNT+25+000001
".Replace("\r\n", "'");

			var sutReceiveMsg4 = Factory.New<CMRSEAOUTRMessage>();
			outturn2.Messages.Add(sutReceiveMsg4);
			sutReceiveMsg4.EM_MessageSubType = "CLR";
			sutReceiveMsg4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			sutReceiveMsg4.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-49);
			sutReceiveMsg4.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN
BGM+961:::SEAOUTR+46DB GE97 H0F4:001+11
NAD+MR+AAA374M::95
RFF+ACW:SEAOUT
RFF+AFM:4
RFF+ABO:O00000428/CMT1::003
DTM+310:20140205040551:204
ERP+1
ERC+ADVICE:80:95
ERC+MS5201:6:95
FTX+AAO+++THIS TRANSACTION WAS REJECTED
ERP+1
ERC+ERROR:80:95
ERC+CG1968:6:95
FTX+AAO+++CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510 MARKS AND NUMBERS ARE MANDATORY IF CARGO TYPE IS ""L CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510,OBL=OBL3337,HBL=H3337C7
ERP+1
ERC+ERROR:80:95
ERC+CG1960:6:95
FTX+AAO+++CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510 GOODS DESCRIPTION IS MANDATORY WHEN CARGO TYPE IS "" CT=LCL,RDT=05/02/2014,RTM=130400,CNT=OCLU1233510,OBL=OBL3337,HBL=H3337C7
ERP+1
ERC+ERROR:80:95
ERC+CG2005:6:95
FTX+AAO+++AT LEAST ONE LINE DETAIL IS MANDATORY
CNT+55:003
UNT+25+000001
".Replace("\r\n", "'");

			header.ResetCachedValuesForTesting();
			outturn.ResetCachedValuesForTesting();
			AssertEquals(CMRMessage.CMRMessageStatusDescription.ERROR, outturn2.OutturnStatus);

			header.ResetCachedValuesForTesting();
			outturn.ResetCachedValuesForTesting();
			outturn.C5_CargoReceiptDate = ZDateTime.Empty;
			outturn.C5_CargoUnpackDate = ZDateTime.Empty;
			AssertEquals(CMRMessage.CMRMessageStatusDescription.WITHDRAWN, outturn.OutturnStatus);
		}

		public void TestRescindMessageReceived()
		{
			Header.RescindMessageReceived += AssertMessageWhenRescindTest;

			Outturn.C5_MasterBill = "Master";
			Outturn.C5_HouseBill = "House";
			Outturn.C5_ContainerNumber = "DFDF1111116";

			Outturn.C5_MessageStatus = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived;

			string messageText = "The outturn [DFDF1111116] - [" +
						"House/Master] has been rescinded!";

			Outturn.C5_CargoReceiptDate = DateTime.Now;
			AssertEquals(messageText, this.messageText);

			this.messageText = "";

			Outturn.C5_CargoUnpackDate = DateTime.Now;
			AssertEquals(messageText, this.messageText);
		}

		public void TestPopulatePackTypesFromManifestedPackTypes()
		{
			Outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.BeerCrate;
			AssertEquals(ZString.Empty, Outturn.C5_PackagesUnits);
			Outturn.C5_PackagesOutturned = 0;
			AssertEquals(ZString.Empty, Outturn.C5_PackagesUnits);
			Outturn.C5_PackagesOutturned = 5;
			AssertEquals(CMRPackageTypes.Codes.BeerCrate, Outturn.C5_PackagesUnits);
			Outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.Jutebag;
			Outturn.C5_PackagesOutturned = 7;
			AssertEquals(CMRPackageTypes.Codes.BeerCrate, Outturn.C5_PackagesUnits);
			Outturn.C5_PackagesUnits = ZString.Empty;
			Outturn.C5_PackagesOutturned = 9;
			AssertEquals(CMRPackageTypes.Codes.Jutebag, Outturn.C5_PackagesUnits);
		}

		public void TestURRInCustomsStatus()
		{
			DepotCusOutturnForTest outturn = Factory.New<DepotCusOutturnForTest>();
			Customs.Business.Testing.OutturnableDummy dummy = Factory.New<Customs.Business.Testing.OutturnableDummy>();
			outturn.Parent = dummy;

			AssertEquals("precondition", "WTO", ((IOutturnableLine)dummy).CargoStatus);
			outturn.C5_CustomsStatus = "BAR";
			AssertEquals("BAR", outturn.C5_CustomsStatus);
		}

		public new void TestValidation()
		{
			AssertNotNull("nullness", Outturn.Validation);
			AssertEquals("type", typeof(DepotCusOutturnValidation), Outturn.Validation.GetType());
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("Outturn result type defaults to nil", CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);
		}

		[ExpectNoExceptions()]
		public void TestClone()
		{
			AssertNotNull(Outturn.Clone());
		}

		public void TestHeader()
		{
			AssertNotNull("nullness", Outturn.Header);
			AssertEquals("type", Header, Outturn.Header);
		}

		public void TestC5_MessageStatusReadOnly()
		{
			AssertEquals(true, Outturn.C5_MessageStatusInfo.ReadOnly);
		}

		public void TestMessageStatusCalculator()
		{
			AssertNotNull(Outturn.MessageStatusCalculator);
			AssertEquals(typeof(DepotCusOutturnUnderbondStatusCalculator), Outturn.MessageStatusCalculator.GetType());
		}

		public void TestMessageStatus()
		{
			AssertNotNull(Outturn.MessageStatus);
			AssertEquals(typeof(UnderbondCusStatus), Outturn.MessageStatus.GetType());
			AssertEquals("", Outturn.MessageStatus.Code);
			Outturn.C5_MessageStatus = "FOO";
			AssertEquals("FOO", Outturn.MessageStatus.Code);
		}

		public void TestHasSplitMessageOriginalRejectedLog()
		{
			AssertEquals("Has no outstanding amendments yet", false, Header.HasSplitMessageOriginalRejectedLog);
			StmALog newLog = Header.Logs.AddNew(Events.UnderbondSplitOutturnOriginalRejected, "Test");
			AssertEquals("Has Outturn SplitMessageOriginalRejectedLog", true, Header.HasSplitMessageOriginalRejectedLog);
		}

		public void TestHasOutturnSplitMessageFailedLog()
		{
			AssertEquals("Has no outstanding amendments yet", false, Header.HasSplitMessageFailedLog);
			StmALog newLog = Header.Logs.AddNew(Events.Cancelled, "Test");
			AssertEquals("Has Outturn SplitMessageFailedLog", true, Header.HasSplitMessageFailedLog);
		}

		public void TestHasNonExistantLineAtCustoms()
		{
			AssertEquals("Has no outturn HasNonExistantLineAtCustoms", false, Header.HasNonExistantLineAtCustomsLog);
			Header.Logs.AddNew(Events.UnderbondOutturnRejected, "Partial Amendment Received");
			AssertEquals("Has HasNonExistantLineAtCustoms", true, Header.HasNonExistantLineAtCustomsLog);
			AssertEquals(Outturn.C5_C6, Header.PK);
		}

		public void TestStatusPriorities()
		{
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			CMRCARSTMessage carstMessage = Factory.New<CMRCARSTMessage>();
			CMRUBMREQRMessage expectedArrivalMessage = Factory.New<CMRUBMREQRMessage>();
			CMRUBMREQRMessage approvalMessage = Factory.New<CMRUBMREQRMessage>();

			carstMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+222++11++++7654321::11'
LOC+12+AUSYD::6'
LOC+4+1111A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+MB:OB5000'
RFF+BH:HB4000'
RFF+AAQ:AAAA1111117'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");
			expectedArrivalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+9999B::95'
LOC+4+1111A::95'
NAD+MR+CJM436P::95'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+MB:OB5000'
RFF+BH:HB4000'
RFF+AAQ:AAAA1111117'
UNT+17+000001'".Replace("\r\n", "");
			approvalMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+1111A::95'
LOC+4+2222O::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
RFF+MB:OB5000'
RFF+BH:HB4000'
UNT+17+000001'".Replace("\r\n", "");

			AssertEquals("No status initially", ZString.Empty, outturn.C5_CustomsStatus);

			outturn.Messages.Add(expectedArrivalMessage);
			outturn.StatusCalculator.DeriveStatusNow();
			outturn.MessageStatusCalculator.DeriveStatusNow();
			AssertEquals("Expected arrival status when only exp arrival in messages.", "EXP", outturn.CombinedStatus.Code);

			outturn.Messages.Add(carstMessage);
			outturn.StatusCalculator.DeriveStatusNow();
			outturn.MessageStatusCalculator.DeriveStatusNow();
			AssertEquals("HLD status when carst addded (overrides expected arrival)", "HLD", outturn.CombinedStatus.Code);

			outturn.Messages.Add(approvalMessage);
			outturn.StatusCalculator.DeriveStatusNow();
			outturn.MessageStatusCalculator.DeriveStatusNow();
			AssertEquals("APP status when underbond approval (overrides carst)", "APP", outturn.CombinedStatus.Code);
		}

		public void TestCombinedStatus()
		{
			AssertEquals(ZString.Empty, Outturn.RawCombinedStatus);
			AssertEquals(ZString.Empty, Outturn.CombinedStatus.Code);

			Outturn.C5_CustomsStatus = "HLD";
			AssertEquals("HLD", Outturn.RawCombinedStatus);
			AssertEquals("HLD", Outturn.CombinedStatus.Code);

			Outturn.C5_CustomsStatus = ZString.Empty;
			Outturn.C5_MessageStatus = "EXP";
			AssertEquals("EXP", Outturn.RawCombinedStatus);
			AssertEquals("EXP", Outturn.CombinedStatus.Code);
		}

		public void TestOceanBillIsNotPopulatedForFCXSeaCargoOutturn()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = OceanBillNum;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCLMixedShipper;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = HouseBill;
			shipment1.JS_OuterPacks = PackageCount;
			shipment1.JS_F3_NKPackType = PackageUnit;
			shipment1.OuterPackLines[0].SetContainer(container.PK);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = HouseBill;
			shipment2.JS_OuterPacks = PackageCount;
			shipment2.JS_F3_NKPackType = PackageUnit;
			shipment2.OuterPackLines[0].SetContainer(container.PK);

			var wrapper = CFSShipmentWrapper.Load(shipment1);
			var underbond = wrapper.Underbonds.AddNew();
			var outturn = (DepotCusOutturn)underbond.Outturns.AddNew();
			outturn.Parent = wrapper;

			AssertEquals("Cargo Type", Core.Constants.ContainerModes.FCLMixedShipper, outturn.C5_CargoType);
			AssertEquals("Container Number", ContainerNumber, outturn.C5_ContainerNumber);
			AssertEquals("House Bill Number should be blank for FCX outturn", ZString.Empty, outturn.C5_HouseBill);
			AssertEquals("Ocean Bill Number should be blank for FCX outturn", ZString.Empty, outturn.C5_MasterBill);
			AssertEquals("Outer Packs", PackageCount, outturn.C5_OuterPacks);
			AssertEquals("OuterPacks Unit", SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(PackageUnit), outturn.C5_OuterPackUnits);
		}

		public void TestOceanBillIsNotPopulatedForFCLSeaCargoOutturn()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = OceanBillNum;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = HouseBill;
			shipment1.JS_OuterPacks = PackageCount;
			shipment1.JS_F3_NKPackType = PackageUnit;
			shipment1.OuterPackLines[0].SetContainer(container.PK);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = HouseBill;
			shipment2.JS_OuterPacks = PackageCount;
			shipment2.JS_F3_NKPackType = PackageUnit;
			shipment2.OuterPackLines[0].SetContainer(container.PK);

			var wrapper = CFSShipmentWrapper.Load(shipment1);
			var underbond = wrapper.Underbonds.AddNew();
			var outturn = (DepotCusOutturn)underbond.Outturns.AddNew();
			outturn.Parent = wrapper;

			AssertEquals("Cargo Type", Core.Constants.ContainerModes.FCL, outturn.C5_CargoType);
			AssertEquals("Container Number", ContainerNumber, outturn.C5_ContainerNumber);
			AssertEquals("House Bill Number should be blank for FCL outturn", ZString.Empty, outturn.C5_HouseBill);
			AssertEquals("Ocean Bill Number should be blank for FCL outturn", ZString.Empty, outturn.C5_MasterBill);
			AssertEquals("Outer Packs", PackageCount, outturn.C5_OuterPacks);
			AssertEquals("OuterPacks Unit", SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(PackageUnit), outturn.C5_OuterPackUnits);
		}

		public void TestShipment()
		{
			SetHeaderDetails(Header);
			SetOutturnDetails(Outturn);
			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.LessThanContainerLoad;
			AssertNull(Outturn.LoadList);
			AssertNull(Outturn.Shipment);
			CFSShipmentCreator creator = new CFSShipmentCreator(Outturn, Factory);
			CFSShipment shipment = creator.Shipment;
			shipment.Factory.Save();
			AssertNull(Outturn.Container);
			AssertNotNull(Outturn.Shipment);
			AssertEquals(shipment, Outturn.Shipment);
			AssertNotNull(Outturn.LoadList);
			AssertEquals(shipment.Consols[0], Outturn.LoadList);
			AssertEquals(typeof(CFSShipmentWrapper), Outturn.Parent.GetType());

			CFSLoadListConsol consol = Outturn.LoadList;
			shipment.Consols.Remove(consol);
			AssertNotNull(Outturn.Shipment);
			AssertNull(Outturn.LoadList);
			shipment.Consols.Add(consol);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DepotCusOutturn outturn2 = factory2.Load<DepotCusOutturn>(Outturn.PK);
			AssertNotNull(outturn2.Shipment);
			AssertNull(outturn2.Container);
			AssertNotNull(outturn2.LoadList);
			AssertEquals(outturn2.Shipment.Consols[0], outturn2.LoadList);
			AssertEquals(typeof(CFSShipmentWrapper), outturn2.Parent.GetType());
		}

		public void TestContainer()
		{
			SetHeaderDetails(Header);
			SetOutturnDetails(Outturn);
			Outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			AssertNull(Outturn.LoadList);
			AssertNull(Outturn.Container);
			AssertNull(Outturn.Shipment);
			CFSContainerCreator creator = new CFSContainerCreator(Outturn, Factory);
			CFSContainer container = creator.Container;
			AssertNotNull(Outturn.Container);
			AssertNull(Outturn.Shipment);
			AssertEquals(container, Outturn.Container);
			AssertEquals(container.Consol, Outturn.LoadList);
			AssertEquals(typeof(CFSContainerWrapper), Outturn.Parent.GetType());

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DepotCusOutturn outturn2 = factory2.Load<DepotCusOutturn>(Outturn.PK);
			AssertNotNull(outturn2.Container);
			AssertNull(outturn2.Shipment);
			AssertNotNull(outturn2.LoadList);
			AssertEquals(outturn2.Container.Consol, outturn2.LoadList);
			AssertEquals(typeof(CFSContainerWrapper), outturn2.Parent.GetType());
		}

		public void TestIsBulk()
		{
			AssertEquals(false, Outturn.IsBulk);
			Outturn.C5_CargoType = Core.Constants.ContainerModes.Bulk;
			AssertEquals(true, Outturn.IsBulk);
		}

		public void TestIsBreakBulk()
		{
			AssertEquals(false, Outturn.IsBreakBulk);
			Outturn.C5_CargoType = CMRCargoTypes.Codes.BreakBulk;
			AssertEquals(true, Outturn.IsBreakBulk);
		}

		public void TestIsFCL()
		{
			AssertEquals(false, Outturn.IsFCL);
			Outturn.C5_CargoType = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, Outturn.IsFCL);
		}

		public void TestIsLCL()
		{
			AssertEquals(false, Outturn.IsLCL);
			Outturn.C5_CargoType = Core.Constants.ContainerModes.LCL;
			AssertEquals(true, Outturn.IsLCL);
		}

		public void TestIsFCX()
		{
			AssertEquals(false, Outturn.IsFCX);
			Outturn.C5_CargoType = "FCX";
			AssertEquals(true, Outturn.IsFCX);
		}

		public void TestIsSurplus()
		{
			AssertEquals(false, Outturn.IsSurplus);
			Outturn.C5_OutturnResultType = "SU";
			AssertEquals(true, Outturn.IsSurplus);
			Outturn.C5_OutturnResultType = "NIL";
			AssertEquals(false, Outturn.IsSurplus);
			Outturn.C5_OutturnResultType = "SC";
			AssertEquals(true, Outturn.IsSurplus);
		}

		public void TestSurplusAndShortLanded()
		{
			AssertEquals("precondition", 0, Outturn.C5_OuterPacks);
			AssertEquals("precondition", 0, Outturn.C5_PackagesOutturned);
			Outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.Package;
			Outturn.C5_PackagesUnits = CMRPackageTypes.Codes.Package;
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 5;
			Outturn.C5_PackagesOutturned = 5;
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 3;
			AssertEquals(CMROutturnResultType.Codes.SurplusPackages, Outturn.C5_OutturnResultType);

			Outturn.C5_PackagesOutturned = 3;
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);

			Outturn.C5_PackagesOutturned = 2;
			AssertEquals(CMROutturnResultType.Codes.ShortLanded, Outturn.C5_OutturnResultType);

			Outturn.C5_OuterPacks = 2;
			AssertEquals(CMROutturnResultType.Codes.NilDiscrepancy, Outturn.C5_OutturnResultType);
		}

		public void TestC5_CargoUnpackDate()
		{
			ZDateTime date = new ZDateTime(2005, 01, 01);
			ZDateTime date2 = new ZDateTime(2006, 06, 06);

			Outturn.C5_CargoUnpackDate = date;
			AssertEquals(date, Outturn.C5_CargoUnpackDate);
			AssertEquals(date, Outturn.C5_CargoReceiptDate);

			Outturn.C5_CargoUnpackDate = date2;
			AssertEquals(date2, Outturn.C5_CargoUnpackDate);
			AssertEquals(date, Outturn.C5_CargoReceiptDate);

			Outturn.C5_CargoUnpackDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, Outturn.C5_CargoUnpackDate);
			AssertEquals(date, Outturn.C5_CargoReceiptDate);
		}

		public void TestC5_CargoReceiptDate()
		{
			ZDateTime date = new ZDateTime(2005, 01, 01);
			ZDateTime date2 = new ZDateTime(2006, 06, 06);

			Outturn.C5_CargoReceiptDate = date;
			AssertEquals(date, Outturn.C5_CargoReceiptDate);
			AssertEquals(ZDateTime.Empty, Outturn.C5_CargoUnpackDate);

			Outturn.C5_CargoUnpackDate = date2;
			AssertEquals(date, Outturn.C5_CargoReceiptDate);
			AssertEquals(date2, Outturn.C5_CargoUnpackDate);

			Outturn.C5_CargoReceiptDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, Outturn.C5_CargoReceiptDate);
			AssertEquals(ZDateTime.Empty, Outturn.C5_CargoUnpackDate);
		}

		public void TestDefaultLineValuesFromShipmentParent()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = OceanBillNum;
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = HouseBill;
			shipment.JS_OuterPacks = PackageCount;
			shipment.JS_F3_NKPackType = PackageUnit;
			shipment.OuterPackLines[0].SetContainer(container.PK);

			CFSShipmentWrapper wrapper = CFSShipmentWrapper.Load(shipment);
			CusUnderbond underbond = wrapper.Underbonds.AddNew();
			var outturn = (DepotCusOutturn)underbond.Outturns.AddNew();
			outturn.Parent = wrapper;

			AssertEquals("Container Number", ContainerNumber, outturn.C5_ContainerNumber);
			AssertEquals("House Bill Number", HouseBill, outturn.C5_HouseBill);
			AssertEquals("Ocean Bill Number", OceanBillNum, outturn.C5_MasterBill);
			AssertEquals("Outer Packs", PackageCount, outturn.C5_OuterPacks);
			AssertEquals("OuterPacks Unit", SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(PackageUnit), outturn.C5_OuterPackUnits);
			AssertEquals("Cargo Type", Enterprise.Core.Constants.ContainerModes.LCL, outturn.C5_CargoType);
		}

		public void TestDefaultLineValuesFromContainerParent()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = OceanBillNum;
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = HouseBill;
			shipment.JS_OuterPacks = PackageCount;
			shipment.JS_F3_NKPackType = PackageUnit;
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = ContainerNumber;

			CFSContainerWrapper wrapper = CFSContainerWrapper.Load(container);
			CusUnderbond underbond = wrapper.Underbonds.AddNew();
			var outturn = (DepotCusOutturn)underbond.Outturns.AddNew();
			outturn.Parent = wrapper;

			AssertEquals("Container Number", ContainerNumber, outturn.C5_ContainerNumber);
			AssertEquals("Cargo Type", Enterprise.Core.Constants.ContainerModes.FCL, outturn.C5_CargoType);
			AssertEquals("Outer Packs", 1, outturn.C5_OuterPacks);
			AssertEquals("OuterPacks Unit", CMRPackageTypes.Codes.UnpackedOrPacked, outturn.C5_OuterPackUnits);
		}

		public void TestC5_OutturnResultType()
		{
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_OuterPacks = 5;
			outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.Box;
			outturn.C5_PackagesOutturned = 4;
			AssertEquals("Outturn Result should be Short", CMROutturnResultType.Codes.ShortLanded, outturn.C5_OutturnResultType);
			outturn.C5_PackagesOutturned = 5;
			AssertEquals("Outturn Result should be Nil Descrepency", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
			outturn.C5_PackagesOutturned = 6;
			AssertEquals("Outturn Result should be Surplus Packages", CMROutturnResultType.Codes.SurplusPackages, outturn.C5_OutturnResultType);

			outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_OuterPacks = 0;
			outturn.C5_OuterPackUnits = CMRPackageTypes.Codes.Box;
			outturn.C5_PackagesOutturned = 1;
			AssertEquals("Outturn Result should be Surplus Consignment", CMROutturnResultType.Codes.SurplusConsignment, outturn.C5_OutturnResultType);
		}

		public void TestC5_OutturnResultTypeMissingInformation()
		{
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_OuterPacks = 0;
			outturn.C5_PackagesOutturned = 0;
			AssertEquals("Outturn Result should be Nil Descrepency", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
			outturn.C5_PackagesOutturned = 6;
			AssertEquals("Outturn Result should be Nil Descrepency", CMROutturnResultType.Codes.NilDiscrepancy, outturn.C5_OutturnResultType);
		}

		public void TestPropertiesExUBMREQR()
		{
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_HouseBill = "HBL002";
			outturn.C5_MasterBill = "OBLDPT001";
			outturn.C5_ContainerNumber = "OCLU8911239";
			CMRUBMREQRMessage uBMMessage = (CMRUBMREQRMessage)outturn.Messages.AddNew(typeof(CMRUBMREQRMessage));
			uBMMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+1111A::95'
LOC+4+2222O::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
RFF+MB:OB5000'
RFF+BH:HB4000'
UNT+17+000001'".Replace("\r\n", "");
			uBMMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			uBMMessage.EM_MessageType = CMRMessage.CMRMessageTypes.UBMREQR;
			AssertEquals("UBMRequestReason", "MOV", outturn.UBMRequestReason);
			AssertEquals("UBMOriginID", "1111A", outturn.UBMOriginID);
			AssertEquals("UBMDestinationID", "2222O", outturn.UBMDestinationID);
		}

		public void TestPropertiesExSEI()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "RESPONSIBLE PARTY";
			OrgCusCode cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_CustomsRegNo = "41065894724";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_HouseBill = "HBL002";
			outturn.C5_MasterBill = "OBLDPT001";
			outturn.C5_ContainerNumber = "OCLU8911239";
			CMRSEIMessage seiMessage = (CMRSEIMessage)outturnHeader.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			AssertEquals("InlandMovementMode", "ROA", outturn.InlandMovementMode);
			AssertEquals("UBMResponsibleID", "41065894724", outturn.UBMResponsibleID);
			AssertEquals("UBMResponsibleIDName", "RESPONSIBLE PARTY", outturn.UBMResponsibleIDName);
			AssertEquals("RecipientSiteID", "AAA374M", outturn.RecipientSiteID);
			AssertEquals("FreightForwarderIndicator", "N", outturn.FreightForwarderIndicator);
			AssertEquals("ConsigneeName", "CONSIGNEE 2", outturn.ConsigneeName);
			AssertEquals("NetWeight", "7001 KG", outturn.NetWeight);
			AssertEquals("GrossWeight", "7000 KG", outturn.GrossWeight);
			AssertEquals("Volume", "7 CM", outturn.Volume);
		}

		public void TestPropertiesExSEIforMaster()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "RESPONSIBLE PARTY CLIENTID";
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cusCode.OK_CustomsRegNo = "41065894724";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_ContainerNumber = "OCLU8911239";
			CMRSEIMessage seiMessage = (CMRSEIMessage)outturnHeader.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			AssertEquals("InlandMovementMode", "ROA", outturn.InlandMovementMode);
			AssertEquals("UBMResponsibleID", "41065894724", outturn.UBMResponsibleID);
			AssertEquals("UBMResponsibleIDName", "RESPONSIBLE PARTY CLIENTID", outturn.UBMResponsibleIDName);
			AssertEquals("RecipientSiteID", "AAA374M", outturn.RecipientSiteID);
			AssertEquals("FreightForwarderIndicator", "Y", outturn.FreightForwarderIndicator);
			AssertEquals("ConsigneeName", "LOCAL FORWARDER", outturn.ConsigneeName);
			AssertEquals("NetWeight", "20000 KG", outturn.NetWeight);
			AssertEquals("GrossWeight", "23000 KG", outturn.GrossWeight);
			AssertEquals("Volume", "20 CM", outturn.Volume);
		}

		public void TestLastSEIDetails()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_HouseBill = "HBL002";
			outturn.C5_MasterBill = "OBLDPT001";
			outturn.C5_ContainerNumber = "OCLU8911239";
			CMRSEIMessage seiMessage = (CMRSEIMessage)outturnHeader.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = CMRSEIMessageTest.SampleSEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;
			CMRUBMREQRMessage uBMMessage = (CMRUBMREQRMessage)outturn.Messages.AddNew(typeof(CMRUBMREQRMessage));
			uBMMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+2C4G E33F GJ0F:1+32'
FTX+AAH+++CJM436P20191'
TDT+20+222++11++++7654321::11'
TDT+1++ROA'
LOC+5+1111A::95'
LOC+4+2222O::95'
NAD+MR+CJM436P::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000020++CT:185:95'
RFF+AAQ:AAAA1111117'
RFF+MB:OB5000'
RFF+BH:HB4000'
UNT+17+000001'".Replace("\r\n", "");
			uBMMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			uBMMessage.EM_MessageType = CMRMessage.CMRMessageTypes.UBMREQR;
			AssertEquals("LastSEIDetails", ExpectedSEIDetails, outturn.SEIDetails);
		}

		public void TestISeaCargoEstablishmentQueryInformationMembers()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_ResponsiblePartyID = "RESPID";
			header.C6_LloydsIMO = "1234567";
			header.C6_VoyageNum = "1234S";
			header.C6_OutturningPremiseID = "9876A";
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_C6 = header.PK;
			outturn.C5_HouseBill = "HBL002";
			outturn.C5_MasterBill = "OBLDPT001";
			outturn.C5_ContainerNumber = "OCLU8911239";
			outturn.C5_ContainerNumber = "OCLU8911239";
			AssertEquals("ContainerNumber", "OCLU8911239", ((ISeaCargoEstablishmentQueryInformation)outturn).ContainerNumber);
			AssertEquals("HouseBill", "HBL002", ((ISeaCargoEstablishmentQueryInformation)outturn).HouseBill);
			AssertEquals("OceanBill", "OBLDPT001", ((ISeaCargoEstablishmentQueryInformation)outturn).OceanBill);
			AssertEquals("ResponsiblePartyID", "RESPID", ((ISeaCargoEstablishmentQueryInformation)outturn).ResponsiblePartyID);
			AssertEquals("VesselID", "1234567", ((ISeaCargoEstablishmentQueryInformation)outturn).VesselID);
			AssertEquals("VoyageNumber", "1234S", ((ISeaCargoEstablishmentQueryInformation)outturn).VoyageNumber);
			AssertEquals("EstablishmentID", "9876A", ((ISeaCargoEstablishmentQueryInformation)outturn).EstablishmentID);
		}

		public void TestSendersReference()
		{
			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_SendersMessageReference = "SENDERSREF";
			DepotCusOutturn outturn = Factory.New<DepotCusOutturn>();
			outturn.C5_C6 = header.PK;
			AssertEquals("SendersReference", "SENDERSREF", ((ISendersMessageReferenceProvider)outturn).SendersReference);
		}

		protected override BusinessObject GetNewBusinessObject() => Outturn;

		CusOutturnHeader header;
		CusOutturnHeader Header => header ?? (header = CusOutturnHeader.New(Factory));

		DepotCusOutturn outturn;
		DepotCusOutturn Outturn => outturn ?? (outturn = Header.Outturns.AddNew());

		void SetHeaderDetails(CusOutturnHeader header)
		{
			header.C6_DateOfArrival = new ZDateTime(2006, 04, 01);
			header.C6_LloydsIMO = "100";
			header.C6_OutturningPremiseID = "200";
			header.C6_VoyageNum = "300";
		}

		void SetOutturnDetails(DepotCusOutturn outturn)
		{
			outturn.C5_MasterBill = "OBL100";
			outturn.C5_HouseBill = "HOUSE100";
			outturn.C5_ContainerNumber = "AAAA1111113";
			outturn.C5_PackagesOutturned = 50;
			outturn.C5_PackagesUnits = "PK";
		}

		void AssertMessageWhenRescindTest(object sender, RescindMessageEventArgs e)
		{
			messageText = e.MessageText;
		}
		string messageText;

		const string OceanBillNum = "OBL123L390280";
		const string ContainerNumber = "FCVU3039209";
		const string HouseBill = "HB789302";
		const int PackageCount = 600;
		const string PackageUnit = "BOX";

		const string ExpectedSEIDetails = @"SEI Details:
Processing Date: 17/03/2009 15:46:45
House Bill: HBL002
Ocean Bill: OBLDPT001
Container: OCLU8911239
Container Mode: LCL
Packages: 30 BX
Net Weight: 7001 KG
Gross Weight: 7000 KG
Volume: 7 CM
Consignee: CONSIGNEE 2
Goods Description: STUFF TYPE 2
Marks: MARKS1
MARKS2
MARKS3
MARKS4
MARKS5
MARKS6
MARKS7
MARKS8
MARKS9
UBM Details:
Mode of Movement : ROA/Road
Request Reason   : MOV/Other Movement
Origin  - Premise ID : 1111A
Address : *** Not on file***
Destination - Premis ID : 2222O
Address : *** Not on file***";

		class DepotCusOutturnForTest : DepotCusOutturn
		{
			public DepotCusOutturnForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override TypeLoaderCollection GetParentLoaders()
			{
				var result = base.GetParentLoaders();
				result.Add(new TypeLoader(typeof(Customs.Business.Testing.OutturnableDummy)));
				return result;
			}
		}
	}
}
