using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
	[TestedType(typeof(SAS148EventProcessor))]
	class SAS148EventProcessorTest : AFREventProcessorAbstractTest<SAS148EventProcessor>
	{
		protected override SAS148EventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new SAS148EventProcessor(eventDataObject, logger, factory);
		}

		protected override bool ExpectEmailToPostmasterOnError => false;

		public void TestSAS148ETAAndETD()
		{
			var universalEventXML = @"
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
			<Context>
				<Type>TimeOfArrival</Type>
				<Value>2018-03-24 20:00</Value>
			</Context>
			<Context>
				<Type>TimeOfDeparture</Type>
				<Value>2018-03-26 20:00</Value>
			</Context>
			<Context>
				<Type>NotificationDetails</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(universalEventXML);

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQ2";
			testVessel.RV_Code = "A P MOLLER";
			var nvocc = Factory.New<JPAFRHeader>();
			nvocc.JPH_MasterBillNumber = "MB20170306";
			nvocc.JPH_Voyage = "12345678";
			nvocc.JPH_CarrierCode = "JEFF";
			nvocc.JPH_RL_NKLoading = "AUSYD";
			nvocc.JPH_LoadingPortSuffix = "1";
			nvocc.JPH_VesselName = testVessel.RV_Code;
			nvocc.JPH_VesselDetailsChanged = true;
			nvocc.JPH_ETA = new ZDateTime(2017, 11, 2);
			nvocc.JPH_ETD = new ZDateTime(2017, 11, 22);
			var nvBill = nvocc.Bills.AddNew();
			nvBill.JPB_BillNumber = "J07JHBL002001";
			nvBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			nvBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;

			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", nvocc.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, nvBill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, nvBill.JPB_MessageStatus);
			AssertEquals(@"Information - The Estimated Time Of Arrival of AFR Job 'AFR00000001' changes from '02-Nov-17 00:00:00' to '24-Mar-18 20:00:00'.
Information - The Estimated Time Of Departure of AFR Job 'AFR00000001' changes from '22-Nov-17 00:00:00' to '26-Mar-18 20:00:00'.", logger.Logs);
			AssertEquals(new ZDateTime(2018, 3, 24, 20, 0, 0), nvocc.JPH_ETA);
			AssertEquals(new ZDateTime(2018, 3, 26, 20, 0, 0), nvocc.JPH_ETD);
		}

		public void TestSAS148StatusUpdate()
		{
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "NL1", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "NL2", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "NL3", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "NL4", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "REG", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "S", true, "2013-03-25 20:00", "NL5", "NL3");

			TestSAS148StatusUpdateCore("M", "S", true, "", "", "");
			TestSAS148StatusUpdateCore("M", "S", true, "", "NL1", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "", "NL2", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "", "NL3", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "", "NL4", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "", "REG", "NL3");
			TestSAS148StatusUpdateCore("M", "S", true, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "S", true, "", "NL5", "NL3");

			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "NL1", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "NL2", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "NL3", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "NL4", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "REG", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "S", false, "2013-03-25 20:00", "NL5", "NL3");

			TestSAS148StatusUpdateCore("M", "S", false, "", "", "");
			TestSAS148StatusUpdateCore("M", "S", false, "", "NL1", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "", "NL2", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "", "NL3", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "", "NL4", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "", "REG", "NL3");
			TestSAS148StatusUpdateCore("M", "S", false, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "S", false, "", "NL5", "NL3");

			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "NL1", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "NL2", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "NL3", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "NL4", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "REG", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "", true, "2013-03-25 20:00", "NL5", "REG");

			TestSAS148StatusUpdateCore("M", "", true, "", "", "");
			TestSAS148StatusUpdateCore("M", "", true, "", "NL1", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "", "NL2", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "", "NL3", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "", "NL4", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "", "REG", "REG");
			TestSAS148StatusUpdateCore("M", "", true, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "", true, "", "NL5", "REG");

			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "NL1", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "NL2", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "NL3", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "NL4", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "REG", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "", false, "2013-03-25 20:00", "NL5", "REG");

			TestSAS148StatusUpdateCore("M", "", false, "", "", "");
			TestSAS148StatusUpdateCore("M", "", false, "", "NL1", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "", "NL2", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "", "NL3", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "", "NL4", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "", "REG", "REG");
			TestSAS148StatusUpdateCore("M", "", false, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("M", "", false, "", "NL5", "REG");

			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "NL1", "NL2");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "NL2", "NL2");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "NL3", "NL2");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "NL4", "NL2");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "REG", "NL2");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "S", true, "2013-03-25 20:00", "NL5", "NL5");

			TestSAS148StatusUpdateCore("", "S", true, "", "", "");
			TestSAS148StatusUpdateCore("", "S", true, "", "NL1", "NL1");
			TestSAS148StatusUpdateCore("", "S", true, "", "NL2", "NL2");
			TestSAS148StatusUpdateCore("", "S", true, "", "NL3", "NL3");
			TestSAS148StatusUpdateCore("", "S", true, "", "NL4", "NL4");
			TestSAS148StatusUpdateCore("", "S", true, "", "REG", "REG");
			TestSAS148StatusUpdateCore("", "S", true, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "S", true, "", "NL5", "NL5");

			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "NL1", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "NL2", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "NL3", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "NL4", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "REG", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "S", false, "2013-03-25 20:00", "NL5", "NL4");

			TestSAS148StatusUpdateCore("", "S", false, "", "", "");
			TestSAS148StatusUpdateCore("", "S", false, "", "NL1", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "", "NL2", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "", "NL3", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "", "NL4", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "", "REG", "NL4");
			TestSAS148StatusUpdateCore("", "S", false, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "S", false, "", "NL5", "NL4");

			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "NL1", "NL2");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "NL2", "NL2");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "NL3", "NL2");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "NL4", "NL2");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "REG", "NL2");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "", true, "2013-03-25 20:00", "NL5", "NL5");

			TestSAS148StatusUpdateCore("", "", true, "", "", "");
			TestSAS148StatusUpdateCore("", "", true, "", "NL1", "NL1");
			TestSAS148StatusUpdateCore("", "", true, "", "NL2", "NL2");
			TestSAS148StatusUpdateCore("", "", true, "", "NL3", "NL3");
			TestSAS148StatusUpdateCore("", "", true, "", "NL4", "NL4");
			TestSAS148StatusUpdateCore("", "", true, "", "REG", "REG");
			TestSAS148StatusUpdateCore("", "", true, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "", true, "", "NL5", "NL5");

			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "", "");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "NL1", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "NL2", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "NL3", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "NL4", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "REG", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "", false, "2013-03-25 20:00", "NL5", "NL1");

			TestSAS148StatusUpdateCore("", "", false, "", "", "");
			TestSAS148StatusUpdateCore("", "", false, "", "NL1", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "", "NL2", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "", "NL3", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "", "NL4", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "", "REG", "NL1");
			TestSAS148StatusUpdateCore("", "", false, "", "NOT", "NOT");
			TestSAS148StatusUpdateCore("", "", false, "", "NL5", "NL1");
		}

		void TestSAS148StatusUpdateCore(ZString masterBillIdentifier, ZString discrepancyCode, ZBool areAllNewVesselDetailsEmpty, ZString dateTimeOfDeletion, ZString oldReleaseStatus, ZString expectedReleaseStatus)
		{
			const string sas148XML = @"
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
			{0}
			<Context>
				<Type>MasterBillIdentifier</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>DateTimeOfDeletion</Type>
				<Value>{2}</Value>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-1</Value>
					</SubContext>
					<SubContext>
						<Type>DiscrepancyCode</Type>
						<Value>{3}</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>MB20170306-X</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(string.Format(sas148XML, areAllNewVesselDetailsEmpty ? "" : @"<Context>
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
			</Context>",
			masterBillIdentifier,
			dateTimeOfDeletion,
			discrepancyCode));

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQX";
			testVessel.RV_Code = "X P MOLLER";
			var nvocc = Factory.New<JPAFRHeader>();
			nvocc.JPH_MasterBillNumber = "MB20170306";
			nvocc.JPH_Voyage = "1234567X";
			nvocc.JPH_CarrierCode = "JEFX";
			nvocc.JPH_RL_NKLoading = "AUSYD";
			nvocc.JPH_LoadingPortSuffix = "2";
			nvocc.JPH_VesselDetailsChanged = ZBool.True;
			nvocc.JPH_VesselName = testVessel.RV_Code;
			var nvBill = nvocc.Bills.AddNew();
			nvBill.JPB_BillNumber = "MB20170306-1";

			nvBill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			var vocc = Factory.New<JPAFRHeader>();
			vocc.JPH_Voyage = "12345678";
			vocc.JPH_CarrierCode = "JEFF";
			vocc.JPH_RL_NKLoading = "AUSYD";
			vocc.JPH_LoadingPortSuffix = "1";
			vocc.JPH_IsShippingLineEntry = true;
			vocc.JPH_VesselName = testVessel.RV_Code;
			var vBill = vocc.Bills.AddNew();
			vBill.JPB_BillNumber = "MB20170306-1";
			vBill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;
			Factory.SaveForTesting();

			nvBill.JPB_ReleaseStatus = oldReleaseStatus;
			vBill.JPB_ReleaseStatus = oldReleaseStatus;
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			CombineAssertions($"'{masterBillIdentifier}'-'{discrepancyCode}'-'{areAllNewVesselDetailsEmpty}'-'{dateTimeOfDeletion}'-'{oldReleaseStatus}'->'{expectedReleaseStatus}'", () =>
			{
				AssertEquals("LogParent", 1, logParents.Length);
				AssertEquals("LogParentPK", nvocc.PK, logParents[0].PK);

				AssertEquals(expectedReleaseStatus, nvBill.JPB_ReleaseStatus);
				AssertEquals(oldReleaseStatus, vBill.JPB_ReleaseStatus);

				var sb = new ZStringBuilder();
				if ((masterBillIdentifier.Contains("M", StringComparison.OrdinalIgnoreCase) || masterBillIdentifier.IsEmpty && !areAllNewVesselDetailsEmpty) && discrepancyCode.IsEmpty)
				{
					if (areAllNewVesselDetailsEmpty)
					{
						AssertEquals("JPH_Voyage", "", nvocc.JPH_Voyage);
						AssertEquals("JPH_CarrierCode", "", nvocc.JPH_CarrierCode);
						AssertEquals("JPH_Voyage", "", nvocc.JPH_Voyage);
						AssertEquals("JPH_RL_NKLoading", "", nvocc.JPH_RL_NKLoading);
						AssertEquals("JPH_LoadingPortSuffix", "", nvocc.JPH_LoadingPortSuffix);
						AssertEquals("JPH_VesselName", "", nvocc.JPH_VesselName);
						AssertEquals("JPH_RadioCallSign", "", nvocc.JPH_RadioCallSign);

						sb.Append($"Information - The Carrier Code of AFR Job '{nvocc.JPH_JobReference}' changes from 'JEFX' to ''.");
						sb.Append($"Information - The Vessel Name of AFR Job '{nvocc.JPH_JobReference}' changes from 'X P MOLLER' to ''.");
						sb.Append($"Information - The JPAFRHeader.JPH_RadioCallSign of AFR Job '{nvocc.JPH_JobReference}' changes from 'OVYQX' to ''.");
						sb.Append($"Information - The Voyage Number of AFR Job '{nvocc.JPH_JobReference}' changes from '1234567X' to ''.");
						sb.Append($"Information - The Port Of Loading of AFR Job '{nvocc.JPH_JobReference}' changes from 'AUSYD' to ''.");
						sb.Append($"Information - The Port Of Loading Suffix of AFR Job '{nvocc.JPH_JobReference}' changes from '2' to ''.");
					}
					else
					{
						AssertEquals("JPH_Voyage", "12345678", nvocc.JPH_Voyage);
						AssertEquals("JPH_CarrierCode", "JEFF", nvocc.JPH_CarrierCode);
						AssertEquals("JPH_Voyage", "12345678", nvocc.JPH_Voyage);
						AssertEquals("JPH_RL_NKLoading", "AUSYD", nvocc.JPH_RL_NKLoading);
						AssertEquals("JPH_LoadingPortSuffix", "1", nvocc.JPH_LoadingPortSuffix);
						AssertEquals("JPH_VesselName", "A P MOLLER", nvocc.JPH_VesselName);
						AssertEquals("JPH_RadioCallSign", "OVYQX", nvocc.JPH_RadioCallSign);

						sb.Append($"Information - The Carrier Code of AFR Job '{nvocc.JPH_JobReference}' changes from 'JEFX' to 'JEFF'.");
						sb.Append("Error - Vessel name 'A P MOLLER' is not on file.");
						sb.Append($"Information - The Vessel Name of AFR Job '{nvocc.JPH_JobReference}' changes from 'X P MOLLER' to 'A P MOLLER'.");
						sb.Append($"Information - The Voyage Number of AFR Job '{nvocc.JPH_JobReference}' changes from '1234567X' to '12345678'.");
						sb.Append($"Information - The Port Of Loading Suffix of AFR Job '{nvocc.JPH_JobReference}' changes from '2' to '1'.");
					}
				}

				if (oldReleaseStatus != expectedReleaseStatus)
				{
					sb.Append($"Information - The Bill '{nvBill.JPB_BillNumber}' Customs Status of AFR Job '{nvocc.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedReleaseStatus}'.");
				}
				sb.Append($"Warning - Bill 'MB20170306-X' could not be found on AFR Job '{nvocc.JPH_JobReference}'.");
				AssertEquals(sb.ToStringWithNewLineBetweenAppends(), logger.Logs);
			});

			nvocc.Delete();
			vocc.Delete();
			testVessel.Delete();
			Factory.SaveForTesting();
		}

		public void TestSAS148EventProcessor()
		{
			var universalEventXML = @"
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
			<Context>
				<Type>NotificationDetails</Type>
				<Value>{0}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var xmlEvent = new XmlEventDeserializer().Parse(string.Format(universalEventXML, "&lt;table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"&gt;&lt;tr&gt;&lt;td&gt;Master Bill&lt;/td&gt;&lt;td&gt;MB20170306&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Master Bill Identifier&lt;/td&gt;&lt;td&gt;1&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign&lt;/td&gt;&lt;td&gt;OVYQ2&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Name&lt;/td&gt;&lt;td&gt;A P MOLLER&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number&lt;/td&gt;&lt;td&gt;12345678&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code&lt;/td&gt;&lt;td&gt;JEFF&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading&lt;/td&gt;&lt;td&gt;AUSYD&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Discharge&lt;/td&gt;&lt;td&gt;AUSYD&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Estimated Date Time of Departure&lt;/td&gt;&lt;td&gt;2012-12-25T17:00:00.0000000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Time Zone of Departure&lt;/td&gt;&lt;td&gt;+1000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Actual Date Time of Departure&lt;/td&gt;&lt;td&gt;2012-12-25T18:00:00.0000000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Date Time of Advance Cargo Information Registration&lt;/td&gt;&lt;td&gt;2013-02-25T19:00:00.0000000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Date Time of Deletion&lt;/td&gt;&lt;td&gt;2013-03-25T20:00:00.0000000&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;BR/&gt;&lt;BR/&gt;&lt;table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"&gt;&lt;tr class=\"tableheadings\"&gt;&lt;th colspan=\"2\"&gt;Bill of Lading Details&lt;/th&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;House B/L Number&lt;/td&gt;&lt;td&gt;Discrepancy Infomation&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;1234&lt;/td&gt;&lt;td&gt;S – Vessel Information discrepancy between Master B/L and House B/L.&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;4567&lt;/td&gt;&lt;td&gt;S – Vessel Information discrepancy between Master B/L and House B/L.&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;"));

			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQ2";
			testVessel.RV_Code = "A P MOLLER";
			var nvocc = Factory.New<JPAFRHeader>();
			nvocc.JPH_MasterBillNumber = "MB20170306";
			nvocc.JPH_Voyage = "12345678";
			nvocc.JPH_CarrierCode = "JEFF";
			nvocc.JPH_RL_NKLoading = "AUSYD";
			nvocc.JPH_LoadingPortSuffix = "1";
			nvocc.JPH_VesselName = testVessel.RV_Code;
			var nvBill = nvocc.Bills.AddNew();
			nvBill.JPB_BillNumber = "J07JHBL002001";
			nvBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			nvBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			var vocc = Factory.New<JPAFRHeader>();
			vocc.JPH_Voyage = "12345678";
			vocc.JPH_CarrierCode = "JEFF";
			vocc.JPH_RL_NKLoading = "AUSYD";
			vocc.JPH_LoadingPortSuffix = "1";
			vocc.JPH_IsShippingLineEntry = true;
			vocc.JPH_VesselName = testVessel.RV_Code;
			var vBill = vocc.Bills.AddNew();
			vBill.JPB_BillNumber = "J07JHBL002001";
			vBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			vBill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			Factory.SaveForTesting();
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", nvocc.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, nvBill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, nvBill.JPB_MessageStatus);
			AssertEquals(ZString.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.NotificationnofMasterBillRegistrationStatus + " Response for " + nvocc.JPH_JobReference, "<table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Master Bill</td><td>MB20170306</td></tr><tr><td>Master Bill Identifier</td><td>1</td></tr><tr><td>Vessel Call Sign</td><td>OVYQ2</td></tr><tr><td>Vessel Name</td><td>A P MOLLER</td></tr><tr><td>Voyage Number</td><td>12345678</td></tr><tr><td>Carrier Code</td><td>JEFF</td></tr><tr><td>Port of Loading</td><td>AUSYD</td></tr><tr><td>Port of Discharge</td><td>AUSYD</td></tr><tr><td>Estimated Date Time of Departure</td><td>2012-12-25T17:00:00.0000000</td></tr><tr><td>Time Zone of Departure</td><td>+1000</td></tr><tr><td>Actual Date Time of Departure</td><td>2012-12-25T18:00:00.0000000</td></tr><tr><td>Date Time of Advance Cargo Information Registration</td><td>2013-02-25T19:00:00.0000000</td></tr><tr><td>Date Time of Deletion</td><td>2013-03-25T20:00:00.0000000</td></tr></table><BR/><BR/><table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr class=\"tableheadings\"><th colspan=\"2\">Bill of Lading Details</th></tr><tr><td>House B/L Number</td><td>Discrepancy Infomation</td></tr><tr><td>1234</td><td>S – Vessel Information discrepancy between Master B/L and House B/L.</td></tr><tr><td>4567</td><td>S – Vessel Information discrepancy between Master B/L and House B/L.</td></tr></table>", Staff1.GS_EmailAddress);

			universalEventXML = @"
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
			xmlEvent = new XmlEventDeserializer().Parse(universalEventXML);
			logger.ClearLogs();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("LogParent", 1, logParents.Length);
			AssertEquals("LogParentPK", nvocc.PK, logParents[0].PK);
			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, nvBill.JPB_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, nvBill.JPB_MessageStatus);
			AssertEquals(ZString.Empty, logger.Logs);
			AssertHasEmail(MessagingTypeList.Descriptions.NotificationnofMasterBillRegistrationStatus + " Response for " + nvocc.JPH_JobReference, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th colspan=\"2\">Manifest Details</th></tr></thead><tr><td>Carrier Code</td><td>JEFF</td></tr><tr><td>Vessel Name</td><td>A P MOLLER</td></tr><tr><td>Vessel Call Sign</td><td>OVYQ2</td></tr><tr><td>Voyage Number</td><td>12345678</td></tr><tr><td>Port Of Loading</td><td>AUSYD</td></tr><tr><td>Port Of Loading Suffix</td><td>1</td></tr><tr><td>Master Bill</td><td>MB20170306</td></tr></table>", Staff1.GS_EmailAddress);
		}
	}
}
