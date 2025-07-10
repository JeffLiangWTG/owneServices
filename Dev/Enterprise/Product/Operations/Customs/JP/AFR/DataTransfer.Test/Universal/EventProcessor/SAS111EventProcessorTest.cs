using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	abstract class SAS111EventProcessorAbstractTest<T> : AHREventProcessorAbstractTest<T>
		where T : SAS111EventProcessor
	{
		protected override bool ExpectEmailToPostmasterOnError => false;

		public void TestSAS111EventProcessorMissingBill()
		{
			var logger = new TestErrorLogger();
			var eventDataObject = new UniversalEvent();
			eventDataObject.ContextCollection = new List<Context>()
			{
				CreateContext("HBOLNumber", "HB3242")
			};

			var processor = GetNewProcessor(eventDataObject, logger, Factory.BOFactory);
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "AFR23423";
			processor.Process(header);
			AssertEquals("Warning - Bill 'HB3242' could not be found on AFR Job 'AFR23423'.", logger.Logs);
		}
	}

	[TestedType(typeof(SAS111EventProcessor))]
	sealed class SAS111EventProcessorBaseOnlyTest : SAS111EventProcessorAbstractTest<SAS111EventProcessor>
	{
		protected override SAS111EventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new SAS111EventProcessor(eventDataObject, logger, factory);
		}

		#region testFilePreperation
		const string UniversalEventXMLTemplate_RAR = @"
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
				<Type>VesselCallSign</Type>
				<Value>xxxxxxx</Value>
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
			<Context>
				<Type>NotificationDetails</Type>
				<Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
		#endregion

		public void TestSAS111EventProcessor_InvalidReference()
		{
			var logger = new TestErrorLogger();

			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAR, "UNK");

			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);
			var processor = new SAS111EventProcessor(xmlEvent, logger, Factory.BOFactory);

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
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();

			processor.Process(header);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, bill.JPB_MessageStatus);
			AssertEquals("Warning - The Event Reference 'UNK' is not recognized as a valid Risk Assessment Result Code", logger.Logs);
		}

		public void TestSAS111EventProcessor_DNL()
		{
			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAR, "DNL");
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
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals(AFRBillCustomsStatusList.Codes.DoNotLoad, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.RiskAssessmentResultReceived, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.RiskAssessmentResult + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress);
		}

		public void TestSAS111EventProcessor_DNU()
		{
			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAR, "DNU");

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
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals(AFRBillCustomsStatusList.Codes.DoNotUnload, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.RiskAssessmentResultReceived, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.RiskAssessmentResult + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress);
		}

		public void TestSAS111EventProcessor_HLD()
		{
			var universalEventXML = string.Format(UniversalEventXMLTemplate_RAR, "HLD");

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
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			var logParent = logParents[0];
			AssertEquals("Should get best matching Header 1.", GetHumanReadableID(header), GetHumanReadableID(logParent));
			AssertEquals(AFRBillCustomsStatusList.Codes.HLD, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.RiskAssessmentResultReceived, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.RiskAssessmentResult + " Response for " + header.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress);
		}
	}
}
