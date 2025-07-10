using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CARSTMessageProcessorTest : TestCaseWithFactory
	{
		public void TestUsingContainerToFindCorrectDeclaration()
		{
			var declaration1 = JobDeclaration.New(Factory);
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration1.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration1.JE_MasterBill = "TBA";
			declaration1.JE_DeclarationReference = "B00001000";
			declaration1.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration1.CusContainers.AddNew().CO_ContainerNumber = "C1";
			declaration1.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			declaration1.JE_VoyageFlightNo = "08313";
			var shutterUpper1 = new SendsMessagesToCustomsShutterUpperer();
			declaration1.MessageInitiator = shutterUpper1;
			declaration1.DoMerge();
			var cusEntryHeader1 = declaration1.CustomsEntryHeaders[0];
			cusEntryHeader1.CH_BGMReference = "B00001000/XXXX";

			var declaration2 = JobDeclaration.New(Factory);
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration2.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);
			declaration2.JE_MasterBill = "TBA";
			declaration2.JE_DeclarationReference = "B00001001";
			declaration2.Invoices.AddNew().JobComInvoiceLines.AddNew();
			declaration2.CusContainers.AddNew().CO_ContainerNumber = "C2";
			declaration2.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			declaration2.JE_VoyageFlightNo = " 8313";
			var shutterUpper2 = new SendsMessagesToCustomsShutterUpperer();
			declaration2.MessageInitiator = shutterUpper2;
			declaration2.DoMerge();
			var cusEntryHeader2 = declaration2.CustomsEntryHeaders[0];
			cusEntryHeader2.CH_BGMReference = "B00001001/XXXX";

			Factory.Save();

			var logger = new LoggingInformation();

			var message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+IFBG 0J19 3AF:1+8'
TDT+20+ 8313++11++++8811924::11'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
RFF+MB:TBA'
RFF+AAQ:C1'
RFF+ABT:AAAFHA6GH'
DOC+1'
PAC+++FCL:67:95'
UNT+12+000001'".Replace("\r\n", "");
			message1.EM_MessageNum = "1";

			var processor = new CARSTMessageProcessor(logger);
			processor.ProcessMessage(message1);

			AssertEquals("1 message added to Entry Header on dec 1", 1, cusEntryHeader1.Messages.Count);
			AssertEquals("no message added to Entry Header on dec 2", 0, cusEntryHeader2.Messages.Count);

			var message2 = Factory.New<CMRCARSTMessage>();
			message2.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+IFBG 0J19 3AF:1+8'
TDT+20+08313++11++++8811924::11'
LOC+12+AUSYD::6'
NAD+MR+AAA374M::95'
RFF+MB:TBA'
RFF+AAQ:C2'
RFF+ABT:AAAFHA6GH'
DOC+1'
PAC+++FCL:67:95'
UNT+12+000001'".Replace("\r\n", "");
			message2.EM_MessageNum = "2";

			processor = new CARSTMessageProcessor(logger);
			processor.ProcessMessage(message2);

			AssertEquals("still 1 message added to Entry Header on dec 1", 1, cusEntryHeader1.Messages.Count);
			AssertEquals("1 message added to Entry Header on dec 2", 1, cusEntryHeader2.Messages.Count);
		}

		public void TestProcessCASTMessageForEntry()
		{
			LoggingInformation logger = new LoggingInformation();

			var factory2 = new BusinessObjectFactory();

			var testJobDeclaration = factory2.New<JobDeclaration>();
			testJobDeclaration.JE_DeclarationReference = "S00001178";
			var invoiceHeader = testJobDeclaration.Invoices.AddNew();
			var inoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = testJobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S00001178/1";
			entryHeader.EntryNumber = "AAACG4CNN";
			var entryLine = entryHeader.AllEntryLines.AddNew();
			inoiceLine.JI_CL = entryLine.PK;

			testJobDeclaration.JE_MasterBill = "08122220053";
			var packGroup1 = (PackingGroup)testJobDeclaration.Bills[0].PackingGroups.AddNew();
			packGroup1.CR_HouseContainerNumber = 1;
			var pack1 = packGroup1.Packages.AddNew();

			AssertEquals("Pre-condition", 0, entryHeader.Messages.Count);
			AssertEquals("Pre-condition", 0, packGroup1.Messages.Count);

			factory2.Save();

			var message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:S00001178/1/DAT1::1'
RFF+MWB:08122220053'
RFF+ABT:AAACG4CNN::2'
UNT+34+000001'".Replace("\r\n", "");
			message1.EM_MessageNum = "1";

			AssertNull(message1.EM_LinkedObject);
			AssertNotEquals("Pre-condition", EDIMessage.Status.Received, message1.EM_Status);

			var processor = new CARSTMessageProcessor(logger);
			processor.ProcessMessage(message1);

			testJobDeclaration = Factory.Load<JobDeclaration>(testJobDeclaration.PK);
			entryHeader = Factory.Load<CusEntryHeader>(entryHeader.PK);
			packGroup1 = Factory.Load<PackingGroup>(packGroup1.PK);

			AssertEquals("1 message added to Entry Header", 1, entryHeader.Messages.Count);
			AssertEquals("1 message added to packing group 1", 1, packGroup1.Messages.Count);
			AssertEquals("Message Status", EDIMessage.Status.Received, message1.EM_Status);

			Assert(((CMRCUSRESMessage)packGroup1.Messages[0]).CUSRESCacheForTesting == null);
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/Y/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "CLEAR", packGroup1.GetCargoStatusFromLatestMessage());
			Assert(((CMRCUSRESMessage)packGroup1.Messages[0]).CUSRESCacheForTesting == null);

			var calculator1 = new CARSTandDSAStatusCalculatorTestHelper(packGroup1);
			calculator1.DeriveStatusForTesting();
			AssertEquals("Pack group 1 status", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, packGroup1.CR_CargoStatus);
			AssertEquals("Declaration Cargo Status", CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, testJobDeclaration.JE_ConsolidatedCargoStatus);
			Assert(((CMRCUSRESMessage)packGroup1.Messages[0]).CUSRESCacheForTesting == null);

			message1 = Factory.New<CMRCARSTMessage>();
			message1.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2AI7 GEA9 4G65:1+8'
DTM+9:20050915174906614576:ZZZ'
DTM+132:20050908:102'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+9980++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9938N::95'
NAD+MR+AAA447Y::95'
NAD+UD+87003014042::95'
RFF+ABO:S00001178/1/DAT1::1'
RFF+MWB:08122220053'
RFF+ABT:AAACG4CNN::2'
UNT+34+000001'".Replace("\r\n", "");
			message1.EM_MessageNum = "2";

			processor = new CARSTMessageProcessor(logger);
			processor.ProcessMessage(message1);
			Assert(((CMRCUSRESMessage)packGroup1.Messages[0]).CUSRESCacheForTesting == null);
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "N/N/N/N", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "HELD", packGroup1.GetCargoStatusFromLatestMessage());
			Assert(((CMRCUSRESMessage)packGroup1.Messages[0]).CUSRESCacheForTesting == null);
		}
	}
}
