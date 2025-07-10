using CargoWise.EntityFramework;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(SAS112EventProcessor))]
	class SAS112EventProcessorTest : SAS111EventProcessorAbstractTest<SAS112EventProcessor>
	{
		protected override SAS112EventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new SAS112EventProcessor(eventDataObject, logger, factory);
		}

		#region testFilePreperation
		const string UniversalEventXMLTemplate_RAC = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>RAC</Code>
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
		<EventReference>{0}</EventReference>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>SPQAVICTM002</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>J07JHBL002001</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>TESTVESSELNAME</Value>
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
		#endregion

		public void TestSAS112EventProcessor_InvalidReference()
		{
			var logger = new TestErrorLogger();

			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAC, "UNK");

			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);
			var processor = new SAS112EventProcessor(xmlEvent, logger, Factory.BOFactory);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_Voyage = "009N";
			header.JPH_CarrierCode = "SPQB";
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			testVessel.RV_Code = "TESTVESSELNAME";
			header.JPH_VesselName = testVessel.RV_Code;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J07JHBL002001";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.DoNotLoad;
			bill.JPB_MessageStatus = MessageStatusList.Codes.RiskAssessmentResultReceived;
			Factory.SaveForTesting();

			processor.Process(header);
			AssertEquals(AFRBillCustomsStatusList.Codes.DoNotLoad, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.RiskAssessmentResultReceived, bill.JPB_MessageStatus);
			AssertEquals("Warning - The Event Reference 'UNK' is not recognized as a valid Risk Assessment Cancellation Code", logger.Logs);
		}

		public void TestSAS112EventProcessor_DNL()
		{
			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAC, "DNL");
			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_Voyage = "009N";
			header.JPH_CarrierCode = "SPQB";
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			testVessel.RV_Code = "TESTVESSELNAME";
			header.JPH_VesselName = testVessel.RV_Code;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J07JHBL002001";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillUpdate;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals(AFRBillCustomsStatusList.Codes.ReleasedDoNotLoad, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillUpdate, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.RiskAssessmentCancellation + " Response for " + header.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>SPQB</td></tr><tr><td>Vessel Name</td><td>TESTVESSELNAME</td></tr><tr><td>Voyage Number</td><td>009N</td></tr><tr><td>Master Bill</td><td>SPQAVICTM002</td></tr><tr><td>House Bill</td><td>J07JHBL002001</td></tr></table>", Staff1.GS_EmailAddress);
		}

		public void TestSAS112EventProcessor_DNU()
		{
			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAC, "DNU");
			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_Voyage = "009N";
			header.JPH_CarrierCode = "SPQB";
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			testVessel.RV_Code = "TESTVESSELNAME";
			header.JPH_VesselName = testVessel.RV_Code;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J07JHBL002001";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload;
			bill.JPB_MessageStatus = MessageStatusList.Codes.RiskAssessmentResultReceived;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals(AFRBillCustomsStatusList.Codes.ReleasedDoNotUnload, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.RiskAssessmentResultReceived, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.RiskAssessmentCancellation + " Response for " + header.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>SPQB</td></tr><tr><td>Vessel Name</td><td>TESTVESSELNAME</td></tr><tr><td>Voyage Number</td><td>009N</td></tr><tr><td>Master Bill</td><td>SPQAVICTM002</td></tr><tr><td>House Bill</td><td>J07JHBL002001</td></tr></table>", Staff1.GS_EmailAddress);
		}

		public void TestSAS112EventProcessor_HLD()
		{
			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAC, "HLD");
			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_Voyage = "009N";
			header.JPH_CarrierCode = "SPQB";
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			testVessel.RV_Code = "TESTVESSELNAME";
			header.JPH_VesselName = testVessel.RV_Code;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J07JHBL002001";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.ReleasedHold;
			bill.JPB_MessageStatus = string.Empty;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals(AFRBillCustomsStatusList.Codes.ReleasedHold, bill.JPB_ReleaseStatus);
			AssertEquals(string.Empty, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.RiskAssessmentCancellation + " Response for " + header.JPH_JobReference, @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>SPQB</td></tr><tr><td>Vessel Name</td><td>TESTVESSELNAME</td></tr><tr><td>Voyage Number</td><td>009N</td></tr><tr><td>Master Bill</td><td>SPQAVICTM002</td></tr><tr><td>House Bill</td><td>J07JHBL002001</td></tr></table>", Staff1.GS_EmailAddress);
		}

		public void TestSAS112EventProcessor_CodeNotMatch()
		{
			var logger = new TestErrorLogger();

			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAC, "HLD");

			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);
			var processor = new SAS112EventProcessor(xmlEvent, logger, Factory.BOFactory);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_Voyage = "009N";
			header.JPH_CarrierCode = "SPQB";
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			testVessel.RV_Code = "TESTVESSELNAME";
			header.JPH_VesselName = testVessel.RV_Code;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J07JHBL002001";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.DoNotUnload;
			bill.JPB_MessageStatus = string.Empty;
			Factory.SaveForTesting();

			processor.Process(header);
			AssertEquals(AFRBillCustomsStatusList.Codes.DoNotUnload, bill.JPB_ReleaseStatus);
			AssertEquals(string.Empty, bill.JPB_MessageStatus);
			AssertEquals("Warning - The Bill Customs Status doesn't match the Cancellation Detail. Current Bill Status is 'DNU' while the message is cancelling 'RHD'.", logger.Logs);
		}
	}
}
