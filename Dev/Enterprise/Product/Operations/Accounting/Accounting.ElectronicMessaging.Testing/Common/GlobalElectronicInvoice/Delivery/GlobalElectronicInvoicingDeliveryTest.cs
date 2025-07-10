using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	class GlobalElectronicInvoicingDeliveryTest : TestCaseWithFactory
	{
		public void TestDelivery()
		{
			using (Factory.AddDisposableService())
			{
				var arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR0001", ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
				Factory.Save();

				var context = EInvoicingTestHelper.GetDeliveryContext(Factory, arInvoice, "APP", "TST", "STT", new Logger());
				var mode = EInvoicingTestHelper.GetEHubMode();
				var eInvoice = EInvoicingTestHelper.GetEInvoice();
				var provider = EInvoicingTestHelper.GetProvider(context, false, mode);
				var interchanges = new GlobalElectronicInvoicingDelivery(provider).Deliver(eInvoice);
				Factory.Save();
				AssertNotNull("Interchange", interchanges);
				AssertEquals("TransportType", "HUB", interchanges[0].EI_TransportType);

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
				AssertEquals("Messages", 1, messages.Length);
				AssertEquals("Messages", EDIMessageStatusList.Codes.Sent, messages[0].EM_Status);
				var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(messages[0].EM_MessageText);
				AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("MessageType", "GEN", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("Payload", "Test Payload", deserializedObj.Payload);
			}
		}

		public void TestDeliveryOfErroneousInvoice()
		{
			using (Factory.AddDisposableService())
			{
				var arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR0001", ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
				Factory.Save();

				var context = EInvoicingTestHelper.GetDeliveryContext(Factory, arInvoice, "APP", "TST", "STT", new Logger());
				var mode = EInvoicingTestHelper.GetEHubMode();
				var eInvoice = EInvoicingTestHelper.GetEInvoice();
				var provider = EInvoicingTestHelper.GetProvider(context, false, mode);
				var postman = new GlobalElectronicInvoicingDelivery(provider);
				var interchanges = postman.Deliver(eInvoice, true);
				Factory.Save();

				AssertNotNull("Interchange", interchanges);
				AssertEquals("TransportType", "HUB", interchanges[0].EI_TransportType);

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
				AssertEquals("Messages", 1, messages.Length);
				AssertEquals("Messages", EDIMessageStatusList.Codes.Failed, messages[0].EM_Status);
				var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(messages[0].EM_MessageText);
				AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("MessageType", "GEN", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("payload", "Test Payload", deserializedObj.Payload);
			}
		}

		public void TestDeliveryForCustomizedModes()
		{
			using (Factory.AddDisposableService())
			{
				var arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR0001", ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
				Factory.Save();

				var context = EInvoicingTestHelper.GetDeliveryContext(Factory, arInvoice, "APP", "TST", "STT", new Logger());
				var mode = EInvoicingTestHelper.GetEHubMode();
				var eInvoice = EInvoicingTestHelper.GetEInvoice();

				var mode1 = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, "TSTDestination");
				var mode2 = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "TSTDestination2");
				var mode3 = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, "TSTDestination3");

				var provider = EInvoicingTestHelper.GetProvider(context, false, mode1, mode2, mode3);
				var interchanges = new GlobalElectronicInvoicingDelivery(provider).Deliver(eInvoice);
				Factory.Save();
				AssertEquals("Interchanges", 3, interchanges.Length);

				AssertEquals("TransportType", "EAD", interchanges[0].EI_TransportType);
				AssertEquals("Destination", "TSTDestination", interchanges[0].EI_To);
				AssertMessagesWithPayloadData(interchanges[0].PK, EDIMessageStatusList.Codes.Sent);

				AssertEquals("TransportType", "HUB", interchanges[1].EI_TransportType);
				AssertEquals("Destination", "TSTDestination2", interchanges[1].EI_To);
				AssertMessagesWithPayloadData(interchanges[1].PK, EDIMessageStatusList.Codes.Sent);

				AssertEquals("TransportType", "EAD", interchanges[2].EI_TransportType);
				AssertEquals("Destination", "TSTDestination3", interchanges[2].EI_To);
				AssertMessagesWithPayloadData(interchanges[2].PK, EDIMessageStatusList.Codes.Sent);
			}
		}

		public void TestNotificationIsSentWhenExceptionOccursDuringDelivery()
		{
			SetupEmail();

			var arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("AR0001", ObjectCreator.AUD, 1.0M, ObjectCreator.Debtor);
			Factory.Save();

			var notifications = new Logger();
			var context = EInvoicingTestHelper.GetDeliveryContext(Factory, arInvoice, "APP", "TST", "STT", notifications);
			var mode = EInvoicingTestHelper.GetEHubMode();
			var eInvoice = EInvoicingTestHelper.GetEInvoice();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(false, notifications.HasErrors);

			var mode1 = EInvoicingTestHelper.CreateCommunicaitonMode(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, "TSTDestination");
			var provider = EInvoicingTestHelper.GetProvider(context, false, mode1);
			var interchanges = new MockGlobalElectronicInvoicingDeliveryForExceptionHandlingTest(provider).Deliver(eInvoice);
			Factory.Save();

			AssertEquals("1 error notification email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(true, notifications.HasErrors);
			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("No EDI Interchange is saved", 0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Message is saved", 0, ediMessages.Length);
		}

		void AssertMessagesWithPayloadData(ZGuid interchnagePK, string expectedStatus)
		{
			var messages = Factory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchnagePK));
			AssertEquals("Messages", 1, messages.Length);
			AssertEquals("Messages", expectedStatus, messages[0].EM_Status);

			var deserializedObj = DataObjectSerializer.Deserialize<GlobalElectronicInvoicing>(messages[0].EM_MessageText);
			AssertEquals("MessagingSystem", "Test Electronic Invoicing System", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("BatchNumber", "125896", deserializedObj.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("MessageType", "GEN", deserializedObj.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("Payload", "Test Payload", deserializedObj.Payload);
		}

		GlbGroup SetupEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "mickey.mouse@cargowise.com";
			staff.GS_Code = "ZAC";
			NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();
			return group;
		}

		TestObjectCreator ObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#region Inner Classes

		class MockGlobalElectronicInvoicingDeliveryForExceptionHandlingTest : GlobalElectronicInvoicingDelivery
		{
			public MockGlobalElectronicInvoicingDeliveryForExceptionHandlingTest(GEIDeliveryModeAndContextProvider provider)
				: base(provider)
			{ }

			protected override IGEIDeliveryModeDecider DeliveryDecider => new MockGEIDeliveryModeDecider();
		}

		public class MockGEIDeliveryModeDecider : IGEIDeliveryModeDecider
		{
			public IDelivery GetDeliveryMode(IEDICommunicationsMode ediCommunicationMode, bool hasValidationError)
				=> new MockFaultyEServicesDelivery() { ErrorNotifier = new EmailNotifier() };

			public IXmlEDIInterchange GetCreatedInterchange(IDelivery delivery)
				=> null;
		}

		public class MockFaultyEServicesDelivery : EServicesDelivery
		{
			protected override string InterchangeQueuedStatus => throw new NotImplementedException();

			protected override string TransportType => throw new NotImplementedException();

			protected override string GetRecipientID(IEDICommunicationsMode mode) => throw new NotImplementedException();
		}

		#endregion
	}
}