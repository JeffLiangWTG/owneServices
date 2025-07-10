using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationEventParentFinderTest : TestCaseWithFactory
	{
		public void TestNoExceptionThrown()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "MBOLNumber";
			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "10137654321";
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_MasterBill = "BOL57654321";
			declaration.JE_HouseBill = "20257654321";
			consol.JK_RL_NKLoadPort = declaration.CountryCode + "ZZZ";
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "10137654321";

			Factory.Save();

			var logger = new TestErrorLogger();
			const string containerLevelEventXmlText = @"
<UniversalEvent>
	<Event>
		<EventType>CCD</EventType>
		<EventTime>10-JUL-2010 18:00</EventTime>
		<EventReference>Dummy Description</EventReference>
		<DataProvider>Dummy</DataProvider>
		<ContextCollection>
			<Context>
				<Type>MBOLNumber</Type>
				<Value>BOL57654321</Value>
			</Context>
			<Context>
				<Type>HBOLNumber</Type>
				<Value>20257654321</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>10137654321</Value>
			</Context>
			<Context>
				<Type>ContainerNumber</Type>
				<Value>10137654322</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(containerLevelEventXmlText) as UniversalEvent;
			AssertNoExceptionThrown(() =>
			{
				subscriber.GetLogParentsForEvent(xmlEvent);
			});
		}

		public void TestUpdateEntryStatus_MatchByMRN_ISW()
		{
			var messageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var declaration = createDeclaration(messageType);

			using (eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					TestUpdateEntryStatus_MatchByMRN(declaration, ZString.Empty);
				}
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TestUpdateEntryStatus_MatchByMRN(declaration, "ACK");
				}
			}
		}

		public void TestUpdateEntryStatus_MatchByMRN_LIC()
		{
			var messageType = BRJobMessageTypeList.Codes.ImportLicense;
			var declaration = createDeclaration(messageType);

			using (eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					TestUpdateEntryStatus_MatchByMRN(declaration, ZString.Empty);
				}
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TestUpdateEntryStatus_MatchByMRN(declaration, "ACK");
				}
			}
		}

		void TestUpdateEntryStatus_MatchByMRN(JobDeclaration declaration, ZString expectedStatus)
		{
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(UniversalEventXml, "B00001100", "2300000001")) as UniversalEvent;
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			var entries = logParents.Cast<CusEntryHeader>().ToArray();
			AssertEquals("Found 1 match entry by MRN", 1, entries.Length);
			AssertEquals("Information - Found 1 match using Context values.", logger.Logs);
			AssertEquals("Match Entry", "B00001100-1", entries[0].CH_BGMReference);
			AssertEquals("Matched Entry Status Updated", expectedStatus, declaration.ActiveEntryHeaders[0].CH_EntryStatus);
			AssertEquals("Nonmatched Entry Status NOT Updated", ZString.Empty, declaration.ActiveEntryHeaders[1].CH_EntryStatus);
		}

		public void TestUpdateEntryStatus_MatchByBGMReference_ISW()
		{
			var messageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var declaration = createDeclaration(messageType);

			using (eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					TestUpdateEntryStatus_MatchByBGMReference(declaration, ZString.Empty);
				}
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TestUpdateEntryStatus_MatchByBGMReference(declaration, "ACK");
				}
			}
		}

		public void TestUpdateEntryStatus_MatchByBGMReference_LIC()
		{
			var messageType = BRJobMessageTypeList.Codes.ImportLicense;
			var declaration = createDeclaration(messageType);

			using (eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_ISW.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					TestUpdateEntryStatus_MatchByBGMReference(declaration, ZString.Empty);
				}
				using (BRCustomsDataRegistry.Instance.EnableUpdateEntryStatusViaUniversalEventXML_LIC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					TestUpdateEntryStatus_MatchByBGMReference(declaration, "ACK");
				}
			}
		}

		void TestUpdateEntryStatus_MatchByBGMReference(JobDeclaration declaration, ZString expectedStatus)
		{
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(UniversalEventXml, "B00001100-1", "0")) as UniversalEvent;
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			var entries = logParents?.Cast<CusEntryHeader>().ToArray();
			AssertEquals("Found 1 match entry by BGMReference", 1, entries?.Length);
			AssertEquals("Information - Found 1 match using Context values.", logger.Logs);
			AssertEquals("Match Entry", "B00001100-1", entries[0].CH_BGMReference);
			AssertEquals("Matched Entry Status Updated", expectedStatus, declaration.ActiveEntryHeaders[0].CH_EntryStatus);
			AssertEquals("Nonmatched Entry Status NOT Updated", ZString.Empty, declaration.ActiveEntryHeaders[1].CH_EntryStatus);
		}

		JobDeclaration createDeclaration(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00001100";
			declaration.JE_MessageType = messageType;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_BGMReference = "B00001100-1";
			entry.CH_MessageType = messageType;
			entry.MovementReferenceNumberSetter("2300000001");

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "B00001100-2";
			entry2.CH_MessageType = messageType;
			entry2.MovementReferenceNumberSetter("2300000002");

			Factory.Save();
			return declaration;
		}

		JobDeclarationEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
		{
			return new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), logger);
		}

		const string UniversalEventXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventTime>2015-04-16T08:12:21.637</EventTime>
		<EventType>CES</EventType>
		<EventReference>ACK</EventReference>
		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>{0}</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>MRN</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>BR</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
	}
}
