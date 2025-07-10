using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.eHubMessaging.Tests
{
	class MessageHandlerClassTests : TestCaseWithFactory
	{
		public void TestSaveMessageShouldContinueTrue()
		{
			var message = new eHubMessage(Guid.NewGuid(), "BLASENDER", "BLARECEIVER", CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", null);
			var company = TestHelpers.ValidCompanyForTest(Factory);
			var notification = new NotificationBuffer();
			var handler = new TesteHubMessageHandler(message, company, notification, true);
			handler.SaveMessage(message, company, notification);
			AssertEquals("DoSaveWasCalledTrue", true, handler.DoSaveWasCalled);
			AssertEquals(notification.AsString, "");
		}

		public void TestSaveMessageShouldContinueFalse()
		{
			var message = new eHubMessage(Guid.NewGuid(), "BLASENDER", "BLARECEIVER", CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", null);
			var company = TestHelpers.ValidCompanyForTest(Factory);
			var notification = new NotificationBuffer();
			var handler = new TesteHubMessageHandler(message, company, notification, false);
			handler.SaveMessage(message, company, notification);
			AssertEquals("DoSaveWasCalledFalse", false, handler.DoSaveWasCalled);
			AssertEquals(notification.AsString, "");
		}

		public void TestCreateInterchange()
		{
			TestCreateInterchangeCore();
		}

		protected virtual void TestCreateInterchangeCore()
		{
			var message = new eHubMessage(Guid.NewGuid(), "BLASENDER", "BLARECEIVER", CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", null, "", string.Empty);
			var company = TestHelpers.ValidCompanyForTest(Factory);
			var notification = new NotificationBuffer();
			EDIInterchange createdInterchange = null;

			var handler = new TesteHubMessageHandler(message, company, notification, true);
			handler.InterchangeCreated += i => createdInterchange = i;

			var interchange = handler.DoSavePublic();

			AssertEquals(interchange, createdInterchange);
			AssertEquals(EDIInterchange.Status.Received, interchange.EI_ReceiveTransmit);
			AssertEquals(message.RecipientID, interchange.EI_To);
			AssertEquals(message.SenderID, interchange.EI_From);
			AssertEquals(message.TrackingID, interchange.EI_SessionGUID);
			AssertEquals(company.Branches.FirstOrDefault().PK, interchange.EI_GB);
			AssertEquals(notification.AsString, "");
			AssertEquals("Interchange shouldn't have a note of type 'File Name'", 0, interchange.Notes.FindByDescription("File Name").Length);
		}

		public void TestCreateInterchangeWithData()
		{
			var handler = HandlerFactory.GetHandler(EDIInterchangeTypeList.Descriptions.TST);
			AssertType(typeof(TestMessageHandler), handler);

			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write("test data");
				writer.Flush();

				var message = new eHubMessage(
					Guid.NewGuid(),
					"Sender",
					"Recipient",
					MessageSchemaType.Xml,
					string.Empty,
					EDIInterchangeTypeList.Descriptions.TST,
					stream,
					"",
					"FileName");

				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var notification = new NotificationBuffer();
				using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					handler.SaveMessage(message, TestHelpers.ValidCompanyForTest(((TestMessageHandler)handler).FactoryProvider.Current), notification);
				}
				AssertEquals(notification.AsString, "");

				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("test data", interchange.EI_BodyText.ToString());
				AssertEquals(EDIInterchangeTypeList.Codes.TST, interchange.EI_InterchangeType);
				var expectedNote = interchange.Notes.FindByDescription("File Name");
				AssertEquals("interchange should have a note of type 'File Name;", 1, expectedNote.Length);
				AssertEquals("File Name is stored in note", "FileName", expectedNote[0].ST_NoteDataAsText);
			}
		}

		public void TestCreateInterchangeForCompanyWithInactiveBranch()
		{
			var handler = HandlerFactory.GetHandler(EDIInterchangeTypeList.Descriptions.TST);
			AssertType(typeof(TestMessageHandler), handler);

			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write("test data");
				writer.Flush();

				var message = new eHubMessage(
					Guid.NewGuid(),
					"Sender",
					"Recipient",
					MessageSchemaType.Xml,
					string.Empty,
					EDIInterchangeTypeList.Descriptions.TST,
					stream,
					"",
					"FileName");

				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var notification = new NotificationBuffer();

				var companyWithInactiveBranch = Factory.NewWithValidTestData<GlbCompany>();
				var branch = companyWithInactiveBranch.Branches.AddNew();
				branch.GB_IsActive = false;
				Factory.Save();

				handler.SaveMessage(message, companyWithInactiveBranch, notification);
				AssertEquals(notification.AsString, "");
				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals(branch.PK, interchange.EI_GB);
			}
		}

		public void TestCreateInterchangeForCompanyWithNoBranch()
		{
			var handler = HandlerFactory.GetHandler(EDIInterchangeTypeList.Descriptions.TST);
			AssertType(typeof(TestMessageHandler), handler);

			using (var stream = new MemoryStream())
			{
				var writer = new StreamWriter(stream);
				writer.Write("test data");
				writer.Flush();

				var message = new eHubMessage(
					Guid.NewGuid(),
					"Sender",
					"Recipient",
					MessageSchemaType.Xml,
					string.Empty,
					EDIInterchangeTypeList.Descriptions.TST,
					stream,
					"",
					"FileName");

				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var notification = new NotificationBuffer();

				var companyWithInactiveBranch = Factory.NewWithValidTestData<GlbCompany>();

				AssertExceptionThrown<MessageHandlerException>(() => handler.SaveMessage(message, companyWithInactiveBranch, notification));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			}
		}

		public void TestSaveMessage_RecipientIdLength_Exception()
		{
			var notification = new NotificationBuffer();
			var handler = new TesteHubMessageHandler(notification);
			var newMessage = new eHubMessage(Guid.NewGuid(), "BLASENDER", "ABC", CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", null);
			AssertExceptionThrown(typeof(Exception), "RecipientID should be 9 characters in length", () => { handler.SaveMessageFromAdapter(newMessage); });
			AssertEquals(notification.AsString, "");
		}

		public void TestSaveMessage_CurrentCompanyAsDisposableEnvironment_NoException()
		{
			var notification = new NotificationBuffer();
			var handler = new TesteHubMessageHandler(notification);
			var newMessage = new eHubMessage(Guid.NewGuid(), "BLASENDER", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", null);
			AssertEquals("PRECONDITION", false, handler.CallPrepareToSave);
			handler.SaveMessageFromAdapter(newMessage);
			AssertEquals("Call PrepareToSave method success.", true, handler.CallPrepareToSave);

			AssertEquals(notification.AsString, "");
		}

		public void TestSaveMessage_RecordsCorrectEI_EADTransportType()
		{
			using (var stream = new MemoryStream())
			{
				var message = new eHubMessage(Guid.NewGuid(), "Sender", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", stream);
				var notification = new NotificationBuffer();
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = company.Branches.AddNew();
				Factory.Save();

				//Use EAD transport:
				var handler = HandlerFactory.GetHandler(EDIInterchangeTypeList.Descriptions.TST);
				AssertType(typeof(TestMessageHandler), handler);

				handler.SaveMessageFromAdapter(message);

				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals("Transport Type should be EAD", "EAD", interchange.EI_TransportType);
			}
		}

		public void TestSaveMessage_RecordsCorrectEI_HUBTransportType()
		{
			using (var stream = new MemoryStream())
			{
				var message = new eHubMessage(Guid.NewGuid(), "Sender", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", stream);
				var notification = new NotificationBuffer();
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = company.Branches.AddNew();
				Factory.Save();

				//Use HUB transport:
				var eHubHandler = new TesteHubMessageHandler(message, company, notification, true);

				eHubHandler.SaveMessage(message, company, notification);

				AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
				var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
				AssertEquals("Transport Type should be HUB", "HUB", interchange.EI_TransportType);
			}
		}

		public void TestSaveMessage_NoDuplicateCheckForEAdaptor()
		{
			using (var stream = new MemoryStream())
			{
				var message = new eHubMessage(Guid.NewGuid(), "Sender", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CargoWise.eHub.Common.MessageSchemaType.FlatFile, "", "", stream);
				var notification = new NotificationBuffer();
				var company = Factory.NewWithValidTestData<GlbCompany>();
				var branch = company.Branches.AddNew();
				Factory.Save();

				//Use EAD transport:
				var handler = HandlerFactory.GetHandler(EDIInterchangeTypeList.Descriptions.TST);
				AssertType(typeof(TestMessageHandler), handler);
				var interchangeBeforeCreationTimeUTC = ZDateTime.UtcNow;

				handler.SaveMessageFromAdapter(message);

				var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, interchangeBeforeCreationTimeUTC));
				AssertEquals("Should be 1 interchange", 1, interchanges.Length);
				AssertEquals("Transport Type should be EAD", "EAD", interchanges[0].EI_TransportType);

				handler.SaveMessageFromAdapter(message);

				interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, interchangeBeforeCreationTimeUTC));
				AssertEquals("Should process duplicate message", 2, interchanges.Length);
			}
		}

		class TesteHubMessageHandler : MessageHandler
		{
			public bool ShouldContinue;
			public bool CallPrepareToSave;

			public TesteHubMessageHandler(INotifications notifier)
			{
				Notifier = notifier;
			}

			public TesteHubMessageHandler(IeHubMessage message, GlbCompany company, INotifications notifier, bool shouldContinue)
			{
				Notifier = notifier;
				DoSaveWasCalled = false;
				this.Message = message;
				this.Company = company;
				ShouldContinue = shouldContinue;
			}

			public bool DoSaveWasCalled { get; set; }

			protected override bool PrepareToSave()
			{
				CallPrepareToSave = true;
				return ShouldContinue;
			}

			protected override EDIInterchange CreateInterchange()
			{
				DoSaveWasCalled = true;
				return base.CreateInterchange();
			}

			public EDIInterchange DoSavePublic()
			{
				return base.CreateInterchange();
			}
		}

		protected MockRepository MockRepository
		{
			get { return mockRepository ?? (mockRepository = new MockRepository(MockBehavior.Default)); }
		}
		MockRepository mockRepository;

		public static Stream GetResourceAsStream(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream(String.Format(@"{0}.{1}.{2}", assembly.GetName().Name, "TestFiles", resourceName));
		}
	}
}
