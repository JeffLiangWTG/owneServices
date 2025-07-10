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
	[TestedType(typeof(SAS108EventProcessor))]
	class SAS108EventProcessorTest : AFREventProcessorAbstractTest<SAS108EventProcessor>
	{
		protected override SAS108EventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new SAS108EventProcessor(eventDataObject, logger, factory);
		}

		protected override bool ExpectEmailToPostmasterOnError => false;

		public void TestSAS108EventProcessor()
		{
			var universalEventXML = @"
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
			<Context>
				<Type>NotificationDetails</Type>
				<Value>&lt;table&gt;&lt;tr&gt;&lt;td&gt;Message&lt;/td&gt;&lt;td&gt;HELLO WORLD&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;br&gt;</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "xxxxxxx";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "SPQAVICTM002";
			header.JPH_Voyage = "009N";
			header.JPH_CarrierCode = "SPQB";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_LoadingPortSuffix = "1";
			header.JPH_VesselName = testVessel.RV_Code;
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "J07JHBL002001";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			var header1 = Factory.New<JPAFRHeader>();
			header1.JPH_Voyage = "009N";
			header1.JPH_CarrierCode = "SPQB";
			header1.JPH_RL_NKLoading = "AUSYD";
			header1.JPH_LoadingPortSuffix = "1";
			header1.JPH_IsShippingLineEntry = true;
			header1.JPH_VesselName = testVessel.RV_Code;
			var bill1 = header1.Bills.AddNew();
			bill1.JPB_BillNumber = "J07JHBL002001";
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill1.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", header1.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.DiscrepancyInformationOfAdvanceFiling + " Response for " + header1.JPH_JobReference, "<table><tr><td>Message</td><td>HELLO WORLD</td></tr></table><br>", Staff1.GS_EmailAddress);

			universalEventXML = @"
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
				<Value>CALLME</Value>
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
			xmlEvent = (new XmlEventDeserializer()).Parse(universalEventXML);
			logger.ClearLogs();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 0, logParents.Length);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, bill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, bill.JPB_MessageStatus);
			AssertEquals(string.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.DiscrepancyInformationOfAdvanceFiling + " Response", @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th colspan=""2"">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>SPQB</td></tr><tr><td>Vessel Call Sign</td><td>CALLME</td></tr><tr><td>Voyage Number</td><td>009N</td></tr><tr><td>Port Of Loading</td><td>AUSYD</td></tr><tr><td>Port Of Loading Suffix</td><td>1</td></tr></table>", Staff1.GS_EmailAddress);
		}
	}
}
