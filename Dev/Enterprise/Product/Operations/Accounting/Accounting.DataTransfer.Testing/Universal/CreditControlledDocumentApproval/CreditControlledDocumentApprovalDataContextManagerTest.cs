using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.CreditControlledDocumentApproval;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.CreditControlledDocumentApproval
{
	[TestedType(typeof(CreditControlledDocumentApprovalDataContextManager))]
	public class CreditControlledDocumentApprovalDataContextManagerTest : DataContextManagerTestCase<CreditControlledDocumentApprovalDataContextManager, CreditControlledDocumentsApproval>
	{
		protected override CreditControlledDocumentsApproval GetNewBusinessObjectForTesting()
		{
			return Factory.NewWithValidTestData<CreditControlledDocumentsApproval>();
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("NettingReceivableTransaction doesn't support IJobNumber", true);
		}

		public void TestApprovedEvent()
		{
			SetupAnddAssertImportOfEvent(EventText.ApprovedEvent, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
		}

		public void TestRejectedEvent()
		{
			SetupAnddAssertImportOfEvent(EventText.RejectionEvent, Core.Constants.GenApprovalRequestApprovalStatus.Rejected);
		}

		void SetupAnddAssertImportOfEvent(string eventXml, string approvalStatus)
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var request1 = factory.New<CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);

			factory.Save();

			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(eventXml);
			var parent = finder.GetLogParentsForEvent(xmlEvent).First();
			manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, null, parent);
			factory.Save();

			var newFactory = new BusinessObjectFactory();
			var approvalRequest = newFactory.Load<CreditControlledDocumentsApproval>(request1.PK);
			AssertEquals(approvalStatus, approvalRequest.XP_ApprovalStatus);
			AssertEquals(new DateTime(2018, 01, 09), approvalRequest.XP_ApprovalDate);
			AssertEquals("SPE", approvalRequest.XP_GS_NKApprovingUser1);
			approvalRequest.Job.Logs.Find(x => x.SL_SE_NKEvent == Events.CreditApprovalResponse.Code);
		}

		public void TestRetrieveEventUser()
		{
			var xmlEvent = new XmlEventDeserializer().Parse(EventText.ApprovedEvent);
			var userCodeAndName = xmlEvent.DataContext.ContextKeyValuePairs.FirstOrDefault(x => x.Key == "Data Source Trigger Event User");
			AssertNotNull("'Data Source Trigger Event User' retrieves user from the event, if the string changes then this unit test will fail. So will other unit tests in this class.", userCodeAndName);
		}

		readonly BusinessObjectFactory factory = new BusinessObjectFactory();
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(factory));
		TestObjectCreator testObjectCreator;
	}

	public class CreditControlledDocumentApprovalParentFinderTest : TestCaseWithFactory
	{
		public void TestCreditControlledDocumentApprovalParentFinder_FindsCorrectly()
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var request1 = Factory.New<CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);

			Factory.Save();

			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(Factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(EventText.RejectionEvent);
			var parent = finder.GetLogParentsForEvent(xmlEvent);

			AssertNotNull(parent);

			CreditControlledDocumentsApproval approvalRequest = parent[0] as CreditControlledDocumentsApproval;

			AssertNotNull(approvalRequest);
			AssertEquals(shipmentNumber, approvalRequest.JobNumber);
			AssertEquals(request1.XP_RequestID, approvalRequest.XP_RequestID);
		}

		public void TestCreditControlledDocumentApprovalParentFinder_EventWellFormedButApprovalRequestStatusNotREQ()
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var request1 = Factory.New<CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);
			request1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, false);

			Factory.Save();

			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(Factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(EventText.RejectionEvent);
			var parent = finder.GetLogParentsForEvent(xmlEvent);

			AssertEquals("No parent should be found", 0, parent.Length);
		}

		public void TestCreditControlledDocumentApprovalParentFinder_ApprovalRequestNotPresent()
		{
			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(Factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(EventText.IncompleteApprovedEvent_ContextCollectionMissing);
			var parent = finder.GetLogParentsForEvent(xmlEvent);

			AssertNull(parent);
			Assert("Error should be found.", logger.HasErrors);
			AssertNotNull(((ISimpleLogResult)logger).Logs.ToList().Where(x => x.Message == "ContextCollection is missing in DataContext"));
		}

		public void TestCreditControlledDocumentApprovalParentFinder_EventNotWellFormed_CompanyMissing()
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var request1 = Factory.New<CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);

			Factory.Save();

			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(Factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(EventText.IncompleteApprovedEvent_CompanyMissing);
			var parent = finder.GetLogParentsForEvent(xmlEvent);

			AssertNull(parent);
			Assert("Error should be found.", logger.HasErrors);
			AssertNotNull(((ISimpleLogResult)logger).Logs.ToList().Where(x => x.Message == "Invalid company code."));
		}

		public void TestCreditControlledDocumentApprovalParentFinder_EventNotWellFormed_KeyMissingJobContext()
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var request1 = Factory.New<CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);

			Factory.Save();

			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(Factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(EventText.IncompleteApprovedEvent_MissingJobContext);
			var parent = finder.GetLogParentsForEvent(xmlEvent);

			AssertNull(parent);
			Assert("Error should be found.", logger.HasErrors);
			AssertNotNull(((ISimpleLogResult)logger).Logs.ToList().Where(x => x.Message == "Context: Type=Job is missing in ContextCollection"));
		}

		public void TestCreditControlledDocumentApprovalParentFinder_EventNotWellFormed_KeyMissingJobNumber()
		{
			var shipmentNumber = "S001001";
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var request1 = Factory.New<CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);

			Factory.Save();

			var manager = new CreditControlledDocumentApprovalDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var finder = new CreditControlledDocumentApprovalParentFinder(Factory, manager, logger);

			var xmlEvent = new XmlEventDeserializer().Parse(EventText.IncompleteApprovedEvent_MissingValueInJobContext);
			var parent = finder.GetLogParentsForEvent(xmlEvent);

			AssertNull(parent);
			Assert("Error should be found.", logger.HasErrors);
			AssertNotNull(((ISimpleLogResult)logger).Logs.ToList().Where(x => x.Message == "Context: Value is missing for Type=Job in ContextCollection"));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}

	internal class EventText
	{
		internal const string RejectionEvent = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>00001000</Key>
					<Type>CreditControlledApprovalRequest</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>EDI</Code>
			</Company>
			<EventUser>
				<Code>SPE</Code>
				<Name>Special User</Name>
			</EventUser>
		</DataContext>
		<EventTime>2018-01-09T00:00:00</EventTime>
		<EventType>CDR</EventType>
		<EventParameters>
			<Type>REJ</Type>
			<Reason>Here comes the reason for rejecting the request.</Reason>
		</EventParameters>
		<ContextCollection>
          <Context>
            <Type>Job</Type>
            <Value>S001001</Value>
          </Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		internal const string ApprovedEvent = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>00001000</Key>
					<Type>CreditControlledApprovalRequest</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>EDI</Code>
			</Company>
			<EventUser>
				<Code>SPE</Code>
				<Name>Special User</Name>
			</EventUser>
		</DataContext>
		<EventTime>2018-01-09T00:00:00</EventTime>
		<EventType>CDR</EventType>
		<EventParameters>
			<Type>APP</Type>
		</EventParameters>
		<ContextCollection>
          <Context>
            <Type>Job</Type>
            <Value>S001001</Value>
          </Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		internal const string IncompleteApprovedEvent_CompanyMissing = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>00001000</Key>
					<Type>CreditControlledApprovalRequest</Type>
				</DataTarget>
			</DataTargetCollection>
			<EventUser>
				<Code>SPE</Code>
			</EventUser>
		</DataContext>
		<EventTime>2018-01-09T00:00:00</EventTime>
		<EventType>CDR</EventType>
		<EventParameters>
			<Type>APP</Type>
		</EventParameters>
		<ContextCollection>
          <Context>
            <Type>Job</Type>
            <Value>S001001</Value>
          </Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		internal const string IncompleteApprovedEvent_ContextCollectionMissing = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>00001000</Key>
					<Type>CreditControlledApprovalRequest</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>EDI</Code>
			</Company>
			<EventUser>
				<Code>SPE</Code>
			</EventUser>
		</DataContext>
		<EventTime>2018-01-09T00:00:00</EventTime>
		<EventType>CDR</EventType>
		<EventParameters>
			<Type>APP</Type>
		</EventParameters>
	</Event>
</UniversalEvent>";

		internal const string IncompleteApprovedEvent_MissingJobContext = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>00001000</Key>
					<Type>CreditControlledApprovalRequest</Type>
				</DataTarget>
			</DataTargetCollection>
			<EventUser>
				<Code>SPE</Code>
			</EventUser>
		</DataContext>
		<EventTime>2018-01-09T00:00:00</EventTime>
		<EventType>CDR</EventType>
		<EventParameters>
			<Type>APP</Type>
		</EventParameters>
		<ContextCollection>
          <Context>
            <Type>SomethingElse</Type>
            <Value>S001001</Value>
          </Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		internal const string IncompleteApprovedEvent_MissingValueInJobContext = @"<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>00001000</Key>
					<Type>CreditControlledApprovalRequest</Type>
				</DataTarget>
			</DataTargetCollection>
			<EventUser>
				<Code>SPE</Code>
			</EventUser>
		</DataContext>
		<EventTime>2018-01-09T00:00:00</EventTime>
		<EventType>CDR</EventType>
		<EventParameters>
			<Type>APP</Type>
		</EventParameters>
		<ContextCollection>
          <Context>
            <Type>Job</Type>
          </Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
	}
}
