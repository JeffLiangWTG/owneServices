using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class AFRHeaderDataEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestIncomingEventLinksToHeadersWithinMAWBRecycledPeriodAndForGeneralEvents()
		{
			const string eventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>house</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var oldHeader = Factory.New<JPAFRHeader>();
			oldHeader.JPH_MasterBillNumber = "20257654321";
			oldHeader.JPH_IsShippingLineEntry = true;
			oldHeader.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value).AddMonths(-1);
			var bill = oldHeader.Bills.AddNew();
			bill.JPB_BillNumber = "20257654321";
			var matchingHeader1 = Factory.New<JPAFRHeader>();
			matchingHeader1.JPH_MasterBillNumber = "20257654321";
			matchingHeader1.JPH_IsShippingLineEntry = true;
			matchingHeader1.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value).AddMonths(1);
			bill = matchingHeader1.Bills.AddNew();
			bill.JPB_BillNumber = "20257654321";
			var matchingHeader2 = Factory.New<JPAFRHeader>();
			matchingHeader2.JPH_MasterBillNumber = "20257654321";
			matchingHeader2.JPH_IsShippingLineEntry = true;
			matchingHeader2.JPH_SystemCreateTimeUtc = ZDateTime.Now;
			bill = matchingHeader2.Bills.AddNew();
			bill.JPB_BillNumber = "20257654321";
			var matchingHeader3 = Factory.New<JPAFRHeader>();
			matchingHeader3.JPH_MasterBillNumber = "20257654321";
			matchingHeader3.JPH_SystemCreateTimeUtc = ZDateTime.Now;
			bill = matchingHeader3.Bills.AddNew();
			bill.JPB_BillNumber = "house";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match two LogParent", 1, logParents.Length);
			AssertEquals(GetHumanReadableID(logParents[0]), GetHumanReadableID(matchingHeader3));

			string eventXmlText2 = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = eventDeserializer.Parse(eventXmlText2);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match two LogParent", 2, logParents.Length);
			Assert(logParents.Any(parent => GetHumanReadableID(parent) == GetHumanReadableID(matchingHeader1)));
			Assert(logParents.Any(parent => GetHumanReadableID(parent) == GetHumanReadableID(matchingHeader2)));
		}

		public void TestIncomingEventLinksToHeaderMatchingMBOLAndHBOL_ForHouseFiling()
		{
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>AHR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var headerWithDifferrentBill = Factory.New<JPAFRHeader>();
			headerWithDifferrentBill.JPH_MasterBillNumber = "OTES2203212";
			var billOfHeaderWithDifferrentBill = headerWithDifferrentBill.Bills.AddNew();
			billOfHeaderWithDifferrentBill.JPB_BillNumber = "HB4656";
			var matchingHeader = Factory.New<JPAFRHeader>();
			matchingHeader.JPH_MasterBillNumber = "OTES2203212";
			var matchingBill1 = matchingHeader.Bills.AddNew();
			matchingBill1.JPB_BillNumber = "072343343";
			var matchingBill2 = matchingHeader.Bills.AddNew();
			matchingBill2.JPB_BillNumber = "20257654321";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(matchingHeader), GetHumanReadableID(logParents[0]));

			const string eventXmlText2 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>CHR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>072343343</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(eventXmlText2);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match one LogParent", 1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(matchingHeader), GetHumanReadableID(logParents[0]));

			matchingHeader.JPH_IsShippingLineEntry = true;
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);

			const string eventXmlText3 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>AHR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>072343343</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>695578554</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(eventXmlText3);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);
		}

		public void TestIncomingEventLinksToHeaderMatchingMBOL_ForMasterFiling()
		{
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>AMR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var headerWithDifferrentBill = Factory.New<JPAFRHeader>();
			headerWithDifferrentBill.JPH_MasterBillNumber = "OTES2203212";
			var billOfHeaderWithDifferrentBill = headerWithDifferrentBill.Bills.AddNew();
			billOfHeaderWithDifferrentBill.JPB_BillNumber = "HB4656";
			var matchingHeader = Factory.New<JPAFRHeader>();
			matchingHeader.JPH_MasterBillNumber = "OTES2203212";
			var matchingBill1 = matchingHeader.Bills.AddNew();
			matchingBill1.JPB_BillNumber = "OTES2203212";
			var matchingBill2 = matchingHeader.Bills.AddNew();
			matchingBill2.JPB_BillNumber = "20257654321";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(0, logParents.Length);

			matchingHeader.JPH_IsShippingLineEntry = true;
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(matchingHeader), GetHumanReadableID(logParents[0]));

			matchingBill1.JPB_BillNumber = "test";
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(0, logParents.Length);

			const string eventXmlText2 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>CMR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(eventXmlText2);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match as no MBOLNumber", 0, logParents.Length);

			const string eventXmlText3 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>CMR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			xmlEvent = eventDeserializer.Parse(eventXmlText3);

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match as no MBOLNumber", 0, logParents.Length);
		}

		public void TestIncomingEventLinksToHeaderMatchingBillAndTransport_ForRiskAssessment()
		{
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>RAR</Code>
				<Description>SAS1110</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>xxxxxxx</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>POLAR BIR</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>009N</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>SPQB</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var randomHeader = Factory.New<JPAFRHeader>();
			randomHeader.JPH_MasterBillNumber = "OTES2203211";
			var randomBill = randomHeader.Bills.AddNew();
			randomBill.JPB_BillNumber = "20257654321";

			var matchingHeader1 = Factory.New<JPAFRHeader>();
			matchingHeader1.JPH_MasterBillNumber = "OTES2203212";
			var matchingBill1_1 = matchingHeader1.Bills.AddNew();
			matchingBill1_1.JPB_BillNumber = "20257654321";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);

			matchingHeader1.JPH_Voyage = "009N";
			matchingHeader1.JPH_CarrierCode = "SPQB";
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			matchingHeader1.JPH_VesselName = testVessel.RV_Code;
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(0, logParents.Length);

			testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "POLAR BIR";
			matchingHeader1.JPH_VesselName = testVessel.RV_Code;
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals(GetHumanReadableID(matchingHeader1), GetHumanReadableID(logParents[0]));

			var matchingHeader2 = Factory.New<JPAFRHeader>();
			matchingHeader2.JPH_MasterBillNumber = "OTES2203212";
			var matchingBill2_1 = matchingHeader2.Bills.AddNew();
			matchingBill2_1.JPB_BillNumber = "20257654321";
			Factory.SaveForTesting();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match 1 header as the new one doesn't match the transport detail", 1, logParents.Length);
			AssertEquals(GetHumanReadableID(matchingHeader1), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToHeaderMatchingVesselInformation_ForDepartureTimeRegistration()
		{
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>DTR</Code>
				<Description>DTR</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>xxxxxxx</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>009N</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>SPQB</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var randomHeader = Factory.New<JPAFRHeader>();
			randomHeader.JPH_MasterBillNumber = "OTES2203211";
			var randomBill = randomHeader.Bills.AddNew();
			randomBill.JPB_BillNumber = "20257654321";

			var matchingHeader1 = Factory.New<JPAFRHeader>();
			matchingHeader1.JPH_MasterBillNumber = "OTES2203212";
			var matchingBill1_1 = matchingHeader1.Bills.AddNew();
			matchingBill1_1.JPB_BillNumber = "OTES2203212";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);

			matchingHeader1.JPH_Voyage = "009N";
			matchingHeader1.JPH_CarrierCode = "SPQB";
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_LloydsNumber = "xxxxxxx";
			matchingHeader1.JPH_VesselName = testVessel.RV_Code;
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not match", 0, logParents.Length);

			matchingHeader1.JPH_RL_NKLoading = "AUSYD";
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(0, logParents.Length);

			Factory.SaveForTesting();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals(GetHumanReadableID(matchingHeader1), GetHumanReadableID(logParents[0]));

			matchingHeader1.JPH_LoadingPortSuffix = "A";
			Factory.SaveForTesting();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(GetHumanReadableID(matchingHeader1), GetHumanReadableID(logParents[0]));

			const string eventXmlText2 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>DTR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>xxxxxxx</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>POLAR BIRD</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>009N</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>SPQB</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>A</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			xmlEvent = eventDeserializer.Parse(eventXmlText2);
			matchingHeader1.JPH_VesselName = "POLAR BIRD";
			var matchingHeader2 = Factory.New<JPAFRHeader>();
			matchingHeader2.JPH_MasterBillNumber = "OTES2203212";
			var matchingBill2_1 = matchingHeader2.Bills.AddNew();
			matchingBill2_1.JPB_BillNumber = "20257654321";
			matchingHeader2.JPH_Voyage = "009N";
			matchingHeader2.JPH_CarrierCode = "SPQB";
			matchingHeader2.JPH_VesselName = "POLAR BIRD";
			matchingHeader2.JPH_RL_NKLoading = "AUSYD";
			Factory.SaveForTesting();

			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should match 1 header as the new one doesn't have the same suffix", 1, logParents.Length);
			AssertEquals(GetHumanReadableID(matchingHeader1), GetHumanReadableID(logParents[0]));
		}

		public void TestIncomingEventLinksToHeaderMatchingVesselInformation_OnlyForVOCCJob()
		{
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>DIF</Code>
				<Description>SAS108</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>xxxxxxx</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>009N</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>SPQB</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			var nVOCCHeader = Factory.New<JPAFRHeader>();
			nVOCCHeader.JPH_MasterBillNumber = "OTES2203211";
			nVOCCHeader.JPH_Voyage = "009N";
			nVOCCHeader.JPH_CarrierCode = "SPQB";
			nVOCCHeader.JPH_RL_NKLoading = "AUSYD";
			nVOCCHeader.JPH_VesselName = testVessel.RV_Code;

			var vOCCHeader = Factory.New<JPAFRHeader>();
			vOCCHeader.JPH_IsShippingLineEntry = true;
			vOCCHeader.JPH_Voyage = "009N";
			vOCCHeader.JPH_CarrierCode = "SPQB";
			vOCCHeader.JPH_RL_NKLoading = "AUSYD";
			vOCCHeader.JPH_VesselName = testVessel.RV_Code;
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals(vOCCHeader.PK, logParents[0].PK);
		}

		public void TestIncomingEventLinksToHeaderMatchingVesselInformation_OnlyForNVOCCJob()
		{
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NRS</Code>
				<Description>SAS148</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>MB20170306</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>A P MOLLER</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQ2";
			testVessel.RV_Code = "A P MOLLER";
			var nVOCCHeader = Factory.New<JPAFRHeader>();
			nVOCCHeader.JPH_MasterBillNumber = "MB20170306";
			nVOCCHeader.JPH_Voyage = "12345678";
			nVOCCHeader.JPH_CarrierCode = "JEFF";
			nVOCCHeader.JPH_RL_NKLoading = "AUSYD";
			nVOCCHeader.JPH_VesselName = testVessel.RV_Code;

			var vOCCHeader = Factory.New<JPAFRHeader>();
			vOCCHeader.JPH_IsShippingLineEntry = true;
			vOCCHeader.JPH_Voyage = "12345678";
			vOCCHeader.JPH_CarrierCode = "JEFF";
			vOCCHeader.JPH_RL_NKLoading = "AUSYD";
			vOCCHeader.JPH_VesselName = testVessel.RV_Code;
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals(nVOCCHeader.PK, logParents[0].PK);
		}

		public void TestInvalidMAWBRecyclePeriodWillMatchAll()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			const string eventXmlText1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>AMR</Code>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>MSC</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>OTES2203212</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var matchingHeader = Factory.New<JPAFRHeader>();
			matchingHeader.JPH_MasterBillNumber = "OTES2203212";
			matchingHeader.JPH_IsShippingLineEntry = true;
			matchingHeader.JPH_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var matchingBill = matchingHeader.Bills.AddNew();
			matchingBill.JPB_BillNumber = "OTES2203212";
			Factory.SaveForTesting();

			var subscriber = GetNewEventParentFinder();

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText1);

			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			AssertEquals("Should get best matching Header.", GetHumanReadableID(matchingHeader), GetHumanReadableID(logParents[0]));
		}

		#region Implementation

		AFRHeaderDataEventParentFinder GetNewEventParentFinder()
		{
			return AFREventParentFinderHelper.GetNewEventParentFinder(Factory.BOFactory);
		}

		string GetHumanReadableID(BusinessObject businessObject)
		{
			return AFREventParentFinderHelper.GetHumanReadableID(businessObject);
		}

		#endregion
	}

	internal class AFREventParentFinderHelper
	{
		internal static AFRHeaderDataEventParentFinder GetNewEventParentFinder(BusinessObjectFactory factory, TestErrorLogger logger = null)
		{
			return new AFRHeaderDataEventParentFinder(factory, new AFRHeaderDataContextManager(), logger ?? new TestErrorLogger());
		}

		internal static string GetHumanReadableID(BusinessObject businessObject)
		{
			return businessObject.HumanReadableName + " (" + businessObject.GetType().FullName + ") - PK: " + businessObject.PK;
		}
	}
}
