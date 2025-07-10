using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Testing
{
	class StandardXmlServiceTaskTests : StandardXmlServiceTaskTestBase
	{
		public void TestProcess2IdenticalAgencyShipmentInSameFactorySecondOneShouldUpdateFirstOne()
		{
			var messageOne = GetQueuedXMSAgencyMessage("AgencyBillOfLadingOne.xml");
			var messageTwo = GetQueuedXMSAgencyMessage("AgencyBillOfLadingTwo.xml");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "GHCREIDCO";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "PFLCarrier";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ASPPG_NZAKL";

			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "SAMOA";

			Factory.Save();

			var coutnBefore = Factory.Load<ICommonShipment>(new ZQuery()).Length;
			var notifications = ProcessMessages();
			var coutnAfter = Factory.Load<ICommonShipment>(new ZQuery()).Length;

			var regEx = new Regex(@$"Processing message batch. Number of messages: '2'
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing shipment with bill 'PPPG02757'.
Successfully matched organization with code 'PFLCarrier', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: PFLCarrier, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'ASPPG_NZAKL', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ASPPG_NZAKL, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'GHCREIDCO', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: GHCREIDCO, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'SAMOA', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: SAMOA, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Shipping Bill of Lading V00001000 created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Agency Bill Of Lading Import Notification Group'
Message '' in status 'RCV' and factory id 'MyFactoryNumberTemplate' in batch mode was processed.
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing shipment with bill 'PPPG02757'.
Successfully matched organization with code 'PFLCarrier', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: PFLCarrier, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'ASPPG_NZAKL', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ASPPG_NZAKL, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'GHCREIDCO', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: GHCREIDCO, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'SAMOA', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: SAMOA, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Shipping Bill of Lading V00001000 created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Agency Bill Of Lading Import Notification Group'
Message '' in status 'RCV' and factory id 'MyFactoryNumberTemplate' in batch mode was processed.
Processing finished. Saving changes...
Saving participants '1'. Current factory id 'MyFactoryNumberTemplate'.
Finished message processing.
".Trim().Replace("'", "\'").Replace("(", @"\(").Replace(")", @"\)").Replace("MyFactoryNumberTemplate", "(?<factoryNumber>[0-9]+)"));

			CombineAssertions(delegate
			{
				AssertEquals("RCV", messageOne.EM_Status);
				AssertEquals("RCV", messageTwo.EM_Status);
				AssertEquals(1, coutnAfter - coutnBefore);
				AssertLogs(regEx, notifications.AsString.Trim());
			});
		}

		public void TestProcess2IdenticalConsolsInSameFactorySecondOneShouldUpdateFirstOne()
		{
			var vessel = RefVessel.LookupVesselByName("ANRO AUSTRALIA", Factory).First();

			var existingVoyage = Factory.New<JobVoyage>();
			existingVoyage.JV_AirSeaRoad = "SEA";
			existingVoyage.JV_RV_NKVessel = vessel.RV_FK;
			existingVoyage.JV_VoyageFlight = "963";

			var messageOne = GetQueuedXMSConsolMessage("ConsolBRE.xml");
			var messageTwo = GetQueuedXMSConsolMessage("ConsolBRE.xml");
			Factory.Save();

			var coutnBefore = Factory.Load<ICommonConsol>(new ZQuery()).Length;
			var notifications = ProcessMessages();
			var coutnAfter = Factory.Load<ICommonConsol>(new ZQuery()).Length;

			var regEx = new Regex(@"Processing message batch. Number of messages: '2'
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing consol with Master Bill 'CONSOLtest2'
Sailing Port Pair (Load='NZAKL' Discharge='AUBNE') created
Importing shipment with House Bill 'HOUSEtest'
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ALCMAN', Mapping Organization: EDICUS, Matching by Foreign code: ALCMAN, Using: Similarity Matcher, Found match: True
Failed to matched organization with code/name 'CUSBROMDW' / 'CUSTOMS BROKER', Mapping Organization: EDICUS, Matching by Foreign code: 00001142, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (CUSBROMDW) created
Shipment S00001000 (House Bill='HOUSETEST') created
Successfully matched organization with code 'PLAAKL', Mapping Organization: EDICUS, Matching by Foreign code: PLASAIAKL, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ACAINT', Mapping Organization: EDICUS, Matching by Foreign code: ACAINTBNE, Using: Similarity Matcher, Found match: True
Consol C00001000 (Master Bill='CONSOLtest2') created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id 'MyFactoryNumberTemplate' in batch mode was processed.
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing consol with Master Bill 'CONSOLtest2'
Sailing Port Pair (Load='NZAKL' Discharge='AUBNE') created
Importing shipment with House Bill 'HOUSEtest'
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ALCMAN', Mapping Organization: EDICUS, Matching by Foreign code: ALCMAN, Using: Similarity Matcher, Found match: True
Shipment S00001000 (House Bill='HOUSETEST') created
Successfully matched organization with code 'PLAAKL', Mapping Organization: EDICUS, Matching by Foreign code: PLASAIAKL, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ACAINT', Mapping Organization: EDICUS, Matching by Foreign code: ACAINTBNE, Using: Similarity Matcher, Found match: True
Consol C00001000 (Master Bill='CONSOLtest2') created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id 'MyFactoryNumberTemplate' in batch mode was processed.
Processing finished. Saving changes...
Saving participants '1'. Current factory id 'MyFactoryNumberTemplate'.
Finished message processing.
".Trim().Replace("'", "\'").Replace("(", @"\(").Replace(")", @"\)").Replace("MyFactoryNumberTemplate", "(?<factoryNumber>[0-9]+)"));

			CombineAssertions(delegate
			{
				AssertEquals("RCV", messageOne.EM_Status);
				AssertEquals("RCV", messageTwo.EM_Status);
				AssertEquals(1, coutnAfter - coutnBefore);
				AssertLogs(regEx, notifications.AsString.Trim());
			});
		}

		public void TestProcess2ConsolsFromDifferentBranchInBatchKeepSameFactory()
		{
			var vessel1 = RefVessel.LookupVesselByName("ANRO AUSTRALIA", Factory).First();
			var vessel2 = RefVessel.LookupVesselByName("ANRO ASIA", Factory).First();

			var existingVoyage1 = Factory.New<JobVoyage>();
			existingVoyage1.JV_AirSeaRoad = "SEA";
			existingVoyage1.JV_RV_NKVessel = vessel1.RV_FK;
			existingVoyage1.JV_VoyageFlight = "963";

			var existingVoyage2 = Factory.New<JobVoyage>();
			existingVoyage2.JV_AirSeaRoad = "SEA";
			existingVoyage2.JV_RV_NKVessel = vessel2.RV_FK;
			existingVoyage2.JV_VoyageFlight = "852";

			var messageBRIBranch = GetQueuedXMSConsolMessage("ConsolBRE.xml");
			var messageSYDBranch = GetQueuedXMSConsolMessage("ConsolSYD.xml");
			Factory.Save();

			var notifications = ProcessMessages();

			var regEx = new Regex(@"
Processing message batch. Number of messages: '2'
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing consol with Master Bill 'CONSOLtest2'
Sailing Port Pair (Load='NZAKL' Discharge='AUBNE') created
Importing shipment with House Bill 'HOUSEtest'
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ALCMAN', Mapping Organization: EDICUS, Matching by Foreign code: ALCMAN, Using: Similarity Matcher, Found match: True
Failed to matched organization with code/name 'CUSBROMDW' / 'CUSTOMS BROKER', Mapping Organization: EDICUS, Matching by Foreign code: 00001142, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (CUSBROMDW) created
Shipment S00001000 (House Bill='HOUSETEST') created
Successfully matched organization with code 'PLAAKL', Mapping Organization: EDICUS, Matching by Foreign code: PLASAIAKL, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ACAINT', Mapping Organization: EDICUS, Matching by Foreign code: ACAINTBNE, Using: Similarity Matcher, Found match: True
Consol C00001000 (Master Bill='CONSOLtest2') created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id '\1' in batch mode was processed.
Processing message ''
Processing message '' in status 'QUE' and factory id '\1' in batch mode.
Combining message text
Running import
Importing consol with Master Bill 'BOL CONsea2'
Sailing Port Pair (Load='NZAKL' Discharge='AUSYD') created
Importing shipment with House Bill 'HOUSEtestsyd2'
Successfully matched organization with code 'MARTEC', Mapping Organization: EDICUS, Matching by Foreign code: MARTEC, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ADEBOX', Mapping Organization: EDICUS, Matching by Foreign code: ADEBOXAKL, Using: Similarity Matcher, Found match: True
Shipment S00001001 (House Bill='HOUSETESTSYD2') created
Successfully matched organization with code 'PLAAKL', Mapping Organization: EDICUS, Matching by Foreign code: PLASAIAKL, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'DIRCON', Mapping Organization: EDICUS, Matching by Foreign code: DIRCON, Using: Similarity Matcher, Found match: True
Failed to matched organization with code/name 'ANRSHIBWI' / 'ANRO SHIPPING LINE', Mapping Organization: EDICUS, Matching by Foreign code: ANRSHIBWI, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (ANRSHIBWI) created
Consol C00001001 (Master Bill='BOL CONsea2') created
Link message to Job
Import finished
Branch was changed. Successfully saved 1 of 2 messages in batch.
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate1' in batch mode.
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id '\2' in batch mode was processed.
Processing finished. Saving changes...
Saving participants '1'. Current factory id '\2'.
Finished message processing.
		".Trim().Replace("'", "\'").Replace("(", @"\(").Replace(")", @"\)").Replace("MyFactoryNumberTemplate1", "(?<factoryNumber1>[0-9]+)").Replace("MyFactoryNumberTemplate", "(?<factoryNumber>[0-9]+)"));

			CombineAssertions(delegate
			{
				AssertEquals("RCV", messageBRIBranch.EM_Status);
				AssertEquals("RCV", messageSYDBranch.EM_Status);
				AssertLogs(regEx, notifications.AsString.Trim());
			});
		}

		public void TestProcess2ShipmentsFromDifferentBranchInBatchKeepSameFactory()
		{
			var messageBRIBranch = GetQueuedXMSShipmentMessage("ShipmentBRE.xml");
			var messageSYDBranch = GetQueuedXMSShipmentMessage("ShipmentSYD.xml");
			Factory.Save();

			var notifications = ProcessMessages();

			var regEx = new Regex(@"
Processing message batch. Number of messages: '2'
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing shipment with House Bill 'TESTBNESHIPMENT'
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ADEBOX', Mapping Organization: EDICUS, Matching by Foreign code: ADEBOXAKL, Using: Similarity Matcher, Found match: True
Shipment S00001000 (House Bill='TESTBNESHIPMENT') created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id '\1' in batch mode was processed.
Processing message ''
Processing message '' in status 'QUE' and factory id '\1' in batch mode.
Combining message text
Running import
Importing shipment with House Bill 'TESTSYDSHIP'
Successfully matched organization with code 'MARTEC', Mapping Organization: EDICUS, Matching by Foreign code: MARTEC, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ADEBOX', Mapping Organization: EDICUS, Matching by Foreign code: ADEBOXAKL, Using: Similarity Matcher, Found match: True
Shipment S00001001 (House Bill='TESTSYDSHIP') created
Link message to Job
Import finished
Branch was changed. Successfully saved 1 of 2 messages in batch.
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate1' in batch mode.
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id '\2' in batch mode was processed.
Processing finished. Saving changes...
Saving participants '1'. Current factory id '\2'.
Finished message processing.
".Trim().Replace("'", "\'").Replace("(", @"\(").Replace(")", @"\)").Replace("MyFactoryNumberTemplate1", "(?<factoryNumber1>[0-9]+)").Replace("MyFactoryNumberTemplate", "(?<factoryNumber>[0-9]+)"));

			CombineAssertions(delegate
			{
				AssertEquals("RCV", messageBRIBranch.EM_Status);
				AssertEquals("RCV", messageSYDBranch.EM_Status);
				AssertLogs(regEx, notifications.AsString.Trim());
			});
		}

		public void TestProcess1ConsolWith2ShipmentsFromDifferentBranchInBatchKeepSameFactory()
		{
			var message = GetQueuedXMSConsolMessage("ConsolWith2Shipments.xml");

			Factory.Save();

			var notifications = ProcessMessages();

			var regEx = new Regex(@"
Processing message batch. Number of messages: '1'
Processing message ''
Processing message '' in status 'QUE' and factory id 'MyFactoryNumberTemplate' in batch mode.
Combining message text
Running import
Importing consol with Master Bill 'BOLSEA CONSOL TEST'
Failed to matched organization with code/name 'MAERSK' / 'MAERSK', Mapping Organization: EDICUS, Matching by Foreign code: MAERSK, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (MAERSK) created
Sailing Port Pair (Load='NZAKL' Discharge='AUSYD') created
Importing shipment with House Bill 'HOUSESYDNEY3'
Successfully matched organization with code 'MARTEC', Mapping Organization: EDICUS, Matching by Foreign code: MARTEC, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'ADEBOX', Mapping Organization: EDICUS, Matching by Foreign code: ADEBOXAKL, Using: Similarity Matcher, Found match: True
Shipment S0000100[0-1] (House Bill='HOUSESYDNEY3') created
Importing shipment with House Bill 'HOUSEBRISBANE3'
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True
Shipment S0000100[0-1] (House Bill='HOUSEBRISBANE3') created
Successfully matched organization with code 'PLAAKL', Mapping Organization: EDICUS, Matching by Foreign code: PLASAIAKL, Using: Similarity Matcher, Found match: True
Successfully matched organization with code 'DIRCON', Mapping Organization: EDICUS, Matching by Foreign code: DIRCON, Using: Similarity Matcher, Found match: True
Failed to matched organization with code/name 'ANRSHIBWI' / 'ANRO SHIPPING LINE', Mapping Organization: EDICUS, Matching by Foreign code: ANRSHIBWI, Using: Default Matcher, Found match: False, Should create Temp. Organization: True, Criteria has Name/Code: True, Temp. Organization created: True
Organization (ANRSHIBWI) created
Consol C00001000 (Master Bill='BOLSEA CONSOL TEST') created
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Consol Shipment Import Notification Group'
Message '' in status 'RCV' and factory id '\1' in batch mode was processed.
Processing finished. Saving changes...
Saving participants '1'. Current factory id '\1'.
Finished message processing.
".Trim().Replace("'", "\'").Replace("(", @"\(").Replace(")", @"\)").Replace("MyFactoryNumberTemplate", "(?<factoryNumber>[0-9]+)"));

			CombineAssertions(delegate
			{
				AssertEquals("RCV", message.EM_Status);
				AssertLogs(regEx, notifications.AsString.Trim());
			});
		}

		public void TestMultipleDeclarationsForTheSameShipment()
		{
			var micrommel = Factory.New<OrgHeader>();
			micrommel.OH_Code = "MICROMMEL";
			micrommel.OH_FullName = "MICROMEDIA TEST";
			var matchMicrommel = Factory.New<OrgPatternMatchOverride>();
			matchMicrommel.OO_Relationship = "ORG";
			matchMicrommel.OO_ForeignCode = "MICROMMEL";
			matchMicrommel.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			matchMicrommel.OO_LocalGuid = micrommel.PK;
			var relatedPartyMicrommel = Factory.New<OrgHeader>();
			relatedPartyMicrommel.OH_Code = "MICROMMEL_RP";
			relatedPartyMicrommel.OH_FullName = "MICROMMEL RELATED PARTY";
			var companyMicrommel = Factory.New<GlbCompany>();
			var branchMicrommel = companyMicrommel.Branches.AddNew();
			branchMicrommel.GB_OH_OrgProxy = relatedPartyMicrommel.PK;
			branchMicrommel.GB_RL_NKHomePort = "AUMEL";
			var relatedPartyRecordMicrommel = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedPartyRecordMicrommel.PR_OH_Parent = micrommel.PK;
			relatedPartyRecordMicrommel.PR_OH_RelatedParty = relatedPartyMicrommel.PK;
			relatedPartyRecordMicrommel.PR_PartyType = "CAB";
			relatedPartyRecordMicrommel.PR_FreightTransportMode = "ALL";
			relatedPartyRecordMicrommel.PR_FreightDirection = "DLV";
			relatedPartyRecordMicrommel.PR_Location = "AUMEL";

			var message1 = GetQueuedXMSConsolMessage("RepeatedMessage.xml");
			var message2 = GetQueuedXMSConsolMessage("RepeatedMessage.xml");
			Factory.Save();

			NotificationBuffer buffer = null;
			AssertNoExceptionThrown(() => buffer = ProcessMessages());
			AssertContains("Matched Organisation 'MICROMMEL'", "Successfully matched organization with code 'MICROMMEL'", buffer.AsString);
			var shipments = Factory.Load<ICommonShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "S00004433"));
			AssertEquals("One shipment was created", 1, shipments.Length);
			var declarations = Factory.Load<Customs.IBaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipments[0].PK));
			AssertEquals("One declaration was created", 1, declarations.Length);
		}

		public void TestCustomDeclarationProcessedCorrectly_WI00054532()
		{
			var interchange = Factory.New<IEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "HYEDUSDOL";
			interchange.EI_To = "HYEDAUDOL";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			interchange.EI_HeaderText = GetFileText("CustomDeclarationHeader.xml");
			interchange.EI_BodyText = GetFileText("CustomDeclarationBody.xml");

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Brokerage;
			message.EM_MessageText = GetFileText("CustomDeclaration.xml");
			message.EM_EI = interchange.PK;
			Factory.Save();

			int noOfDeclarationBeforeImport = Factory.GetDatabaseCount(ObjectFactory.GetType<Customs.IBaseJobDeclaration>());
			var notifications = ProcessMessages(false, false);
			int noOfDeclarationAfterImport = Factory.GetDatabaseCount(ObjectFactory.GetType<Customs.IBaseJobDeclaration>());

			CombineAssertions(delegate
			{
				AssertEquals("RCV", message.EM_Status);
				AssertContains("Declaration B00001000 created", notifications.AsString);
				AssertEquals("Declaration created number", 1, noOfDeclarationAfterImport - noOfDeclarationBeforeImport);
			});
		}
	}
}
