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
	[TestedType(typeof(SAS157EventProcessor))]
	class SAS157EventProcessorTest : AFREventProcessorAbstractTest<SAS157EventProcessor>
	{
		protected override SAS157EventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new SAS157EventProcessor(eventDataObject, logger, factory);
		}

		protected override bool ExpectEmailToPostmasterOnError => false;

		public void TestSAS157StatusUpdate()
		{
			var sAS157XML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NHS</Code>
				<Description>SAS157</Description>
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
			<Context>
				<Type>NotificationDetails</Type>
				<Value>{0}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(string.Format(sAS157XML, "&lt;table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"&gt;&lt;tr&gt;&lt;td&gt;Master Bill&lt;/td&gt;&lt;td&gt;1234567890&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign&lt;/td&gt;&lt;td&gt;ABCDE&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Name&lt;/td&gt;&lt;td&gt;ABCDE&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number&lt;/td&gt;&lt;td&gt;123E&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code&lt;/td&gt;&lt;td&gt;NACC&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading&lt;/td&gt;&lt;td&gt;USLAX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading Suffix&lt;/td&gt;&lt;td&gt;1&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Date Time of Notification&lt;/td&gt;&lt;td&gt;2013-03-25 20:00&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;"));

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQ2";
			testVessel.RV_Code = "A P MOLLER";
			var nvocc = Factory.New<JPAFRHeader>();
			nvocc.JPH_JobReference = "novcc";
			nvocc.JPH_MasterBillNumber = "MB20170306";
			nvocc.JPH_Voyage = "12345678";
			nvocc.JPH_CarrierCode = "JEFF";
			nvocc.JPH_RL_NKLoading = "AUSYD";
			nvocc.JPH_LoadingPortSuffix = "1";
			nvocc.JPH_VesselName = testVessel.RV_Code;
			var nvBill = nvocc.Bills.AddNew();
			nvBill.JPB_BillNumber = "MB20170306";
			nvBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NL2;
			nvBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			var vocc = Factory.New<JPAFRHeader>();
			vocc.JPH_JobReference = "vocc";
			vocc.JPH_Voyage = "12345678";
			vocc.JPH_CarrierCode = "JEFF";
			vocc.JPH_RL_NKLoading = "AUSYD";
			vocc.JPH_LoadingPortSuffix = "1";
			vocc.JPH_IsShippingLineEntry = true;
			vocc.JPH_VesselName = testVessel.RV_Code;
			var vBill = vocc.Bills.AddNew();
			vBill.JPB_BillNumber = "MB20170306";
			vBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NL2;
			vBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", vocc.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, vBill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, vBill.JPB_MessageStatus);
			AssertEquals("Information - The Bill Customs Status of AFR Job 'vocc' changes from 'NL2' to 'REG'.", logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.NotificationnofHouseBillRegistrationStatus + " Response for " + vocc.JPH_JobReference, "<table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Master Bill</td><td>1234567890</td></tr><tr><td>Vessel Call Sign</td><td>ABCDE</td></tr><tr><td>Vessel Name</td><td>ABCDE</td></tr><tr><td>Voyage Number</td><td>123E</td></tr><tr><td>Carrier Code</td><td>NACC</td></tr><tr><td>Port of Loading</td><td>USLAX</td></tr><tr><td>Port of Loading Suffix</td><td>1</td></tr><tr><td>Date Time of Notification</td><td>2013-03-25 20:00</td></tr></table>", Staff1.GS_EmailAddress);
		}

		public void TestSAS157EventProcessor()
		{
			var universalEventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NHS</Code>
				<Description>SAS157</Description>
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
			<Context>
				<Type>NotificationDetails</Type>
				<Value>{0}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(string.Format(universalEventXML, "&lt;table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"&gt;&lt;tr&gt;&lt;td&gt;Master Bill&lt;/td&gt;&lt;td&gt;1234567890&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign&lt;/td&gt;&lt;td&gt;ABCDE&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Name&lt;/td&gt;&lt;td&gt;ABCDE&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number&lt;/td&gt;&lt;td&gt;123E&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code&lt;/td&gt;&lt;td&gt;NACC&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading&lt;/td&gt;&lt;td&gt;USLAX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading Suffix&lt;/td&gt;&lt;td&gt;1&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Date Time of Notification&lt;/td&gt;&lt;td&gt;2013-03-25 20:00&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;"));

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQ2";
			testVessel.RV_Code = "A P MOLLER";
			var nvocc = Factory.New<JPAFRHeader>();
			nvocc.JPH_JobReference = "novcc";
			nvocc.JPH_MasterBillNumber = "MB20170306";
			nvocc.JPH_Voyage = "12345678";
			nvocc.JPH_CarrierCode = "JEFF";
			nvocc.JPH_RL_NKLoading = "AUSYD";
			nvocc.JPH_LoadingPortSuffix = "1";
			nvocc.JPH_VesselName = testVessel.RV_Code;
			var nvBill = nvocc.Bills.AddNew();
			nvBill.JPB_BillNumber = "MB20170306";
			nvBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			nvBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			var vocc = Factory.New<JPAFRHeader>();
			vocc.JPH_JobReference = "vocc";
			vocc.JPH_Voyage = "12345678";
			vocc.JPH_CarrierCode = "JEFF";
			vocc.JPH_RL_NKLoading = "AUSYD";
			vocc.JPH_LoadingPortSuffix = "1";
			vocc.JPH_IsShippingLineEntry = true;
			vocc.JPH_VesselName = testVessel.RV_Code;
			var vBill = vocc.Bills.AddNew();
			vBill.JPB_BillNumber = "MB20170306";
			vBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			vBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", vocc.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, vBill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, vBill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.NotificationnofHouseBillRegistrationStatus + " Response for " + vocc.JPH_JobReference, "<table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Master Bill</td><td>1234567890</td></tr><tr><td>Vessel Call Sign</td><td>ABCDE</td></tr><tr><td>Vessel Name</td><td>ABCDE</td></tr><tr><td>Voyage Number</td><td>123E</td></tr><tr><td>Carrier Code</td><td>NACC</td></tr><tr><td>Port of Loading</td><td>USLAX</td></tr><tr><td>Port of Loading Suffix</td><td>1</td></tr><tr><td>Date Time of Notification</td><td>2013-03-25 20:00</td></tr></table>", Staff1.GS_EmailAddress);

			universalEventXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>NHS</Code>
				<Description>SAS157</Description>
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
			xmlEvent = new XmlEventDeserializer().Parse(universalEventXML);
			logger.ClearLogs();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", vocc.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, vBill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, vBill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.NotificationnofHouseBillRegistrationStatus + " Response for " + vocc.JPH_JobReference, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th colspan=\"2\">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>JEFF</td></tr><tr><td>Vessel Name</td><td>A P MOLLER</td></tr><tr><td>Vessel Call Sign</td><td>OVYQ2</td></tr><tr><td>Voyage Number</td><td>12345678</td></tr><tr><td>Port Of Loading</td><td>AUSYD</td></tr><tr><td>Port Of Loading Suffix</td><td>1</td></tr><tr><td>Master Bill</td><td>MB20170306</td></tr></table>", Staff1.GS_EmailAddress);
		}
	}
}
