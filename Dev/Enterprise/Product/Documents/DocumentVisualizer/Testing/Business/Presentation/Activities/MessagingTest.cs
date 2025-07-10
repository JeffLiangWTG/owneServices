using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using DataContext = Enterprise.DocumentVisualizer.Integration.DataContext;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(Presentation.Messaging))]
	sealed class MessagingTest : CommandProviderTest
	{
		#region TestDisableMenuItems

		public void TestDisableMenuItems_AfterSendingMessage()
		{
			AssertDisableMenuItems(new MessageSentEvent(new DummyDocument()), true);
		}

		public void TestDisableMenuItems_AfterSendingMessageWithdrawal()
		{
			AssertDisableMenuItems(new MessageWithdrawalSentEvent(new DummyDocument()), true);
		}

		public void TestDisableMenuItems_AfterResettingToOriginal()
		{
			AssertDisableMenuItems(new ResetToOriginalEvent(new DummyDocument()), false);
		}

		void AssertDisableMenuItems<T>(T eventToPublish, bool expectMenuItemsToBeDisabled) where T : class
		{
			var factory = new BusinessObjectFactory();
			var messaging = new Presentation.Messaging();

			var template = new DummyStandardTemplate(DummyWorksheet.Parse(
@"#Config
#End"));
			var bizObj = (BusinessObject)factory.New<Forwarding.IForwardingShipment>();
			var documentData = bizObj.LoadOrCreateDocumentData("test");
			var services = new ServiceContainer();
			var broker = new EventBroker();
			services.Register<IEventBroker>(broker);
			var descriptor = new Mock<IDocumentDescriptor>();
			var document = new DummyDocument
			{
				Data = new object().MakeDynamic()
			};

			var info = new DocumentInfo(
			new Lazy<ITemplate>(() => template),
			new Lazy<IVisualizerDocumentData>(() => documentData),
			new Lazy<IServiceContainer>(() => services),
			new Lazy<IDocumentDescriptor>(() => descriptor.Object),
			new Lazy<IDocument>(() => document));

			messaging.OnDocumentInfosCreated(info);

			var sendMessage = messaging
				.Commands
				.Single(cmd => cmd.Id == CommandIds.SendMessage);

			var sendWithdrawal = messaging
				.Commands
				.Single(cmd => cmd.Id == CommandIds.SendWithdrawal);

			var resetToOrignal = messaging
				.Commands
				.Single(cmd => cmd.Id == CommandIds.ResetToOriginal);

			AssertEquals("Send Message is enabled before event", true, sendMessage.IsEnabled);
			AssertEquals("Send Message is visible before event", true, sendMessage.IsVisible);
			AssertEquals("Send Withdrawal is enabled before event", true, sendWithdrawal.IsEnabled);
			AssertEquals("Send Withdrawal is visible before event", true, sendWithdrawal.IsVisible);
			AssertEquals("Reset To Original is enabled before event", true, resetToOrignal.IsEnabled);
			AssertEquals("Reset To Original is visible before event", true, resetToOrignal.IsVisible);

			broker.Publish(eventToPublish);

			if (expectMenuItemsToBeDisabled)
			{
				AssertEquals("Send Message is disabled after event", false, sendMessage.IsEnabled);
				AssertEquals("Send Message is visible after event", true, sendMessage.IsVisible);
				AssertEquals("Send Withdrawal is disabled after event", false, sendWithdrawal.IsEnabled);
				AssertEquals("Send Withdrawal is visible after event", true, sendWithdrawal.IsVisible);
				AssertEquals("Reset To Original is disabled after event", false, resetToOrignal.IsEnabled);
				AssertEquals("Reset To Original is visible after event", true, resetToOrignal.IsVisible);
			}
			else
			{
				AssertEquals("Send Message is enabled after event", true, sendMessage.IsEnabled);
				AssertEquals("Send Message is visible after event", true, sendMessage.IsVisible);
				AssertEquals("Send Withdrawal is enabled after event", true, sendWithdrawal.IsEnabled);
				AssertEquals("Send Withdrawal is visible after event", true, sendWithdrawal.IsVisible);
				AssertEquals("Reset To Original is enabled after event", true, resetToOrignal.IsEnabled);
				AssertEquals("Reset To Original is visible after event", true, resetToOrignal.IsVisible);
			}
		}

		#endregion

		#region SendCopyToEDocs

		[SnailTest]
		public void TestSendCopyToEDocs()
		{
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

			var factory = new BusinessObjectFactory();
			using (factory.AddDisposableService())
			{
				var messaging = new Presentation.Messaging();

				const string documentName = "test doc";

				var documentData = SetUpModuleForSendCopyToEDocs(factory, messaging, documentName);

				var sendMessage = messaging
									.Commands
									.Single(cmd => cmd.Id == CommandIds.SendMessage);

				Assert("Send Message command executed successfully", sendMessage.Invoke());

				AssertContainsExactElementsInAnyOrder("eDocs print job has been created",
					new[]
					{
						$"{documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling}|DDS|Eagle Datamation International - BN - AUBNE - test doc"
					},
					GetPrintJobs(factory));

				var resetToOriginal = messaging
					.Commands
					.Single(cmd => cmd.Id == CommandIds.ResetToOriginal);

				Assert("Reset To Original command executed successfully",resetToOriginal.Invoke());

				AssertContainsExactElementsInAnyOrder("eDocs print job has been created",
					new[]
					{
						$"{documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling}|DDS|Eagle Datamation International - BN - AUBNE - test doc",
						$"{documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling}|DDS|Eagle Datamation International - BN - AUBNE - test doc"
					},
					GetPrintJobs(factory));

				AddEvents(documentData,
					documentName,
					new[]
					{
						Events.MessageAccepted
					});

				var sendWithdrawal = messaging
					.Commands
					.Single(cmd => cmd.Id == CommandIds.SendWithdrawal);

				Assert("Send Withdrawal command executed successfully",sendWithdrawal.Invoke());

				AssertContainsExactElementsInAnyOrder("eDocs print job has been created",
					new[]
					{
						$"{documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling}|DDS|Eagle Datamation International - BN - AUBNE - test doc",
						$"{documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling}|DDS|Eagle Datamation International - BN - AUBNE - test doc",
						$"{documentName + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling}|DDS|Eagle Datamation International - BN - AUBNE - test doc"
					},
					GetPrintJobs(factory));
			}
		}

		VisualizerDocumentData SetUpModuleForSendCopyToEDocs(BusinessObjectFactory factory, Presentation.Messaging messaging, string documentName)
		{
			var bizObj = (BusinessObject)factory.New<Forwarding.IForwardingShipment>();

			var uxml = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DataSource = new DataSource
					{
						Key = "S000001",
						Type = "ForwardingShipment"
					}
				}
			};

			var document = new DummyDocument
			{
				Data = uxml.MakeDynamic()
			};

			var template = new DummyStandardTemplate(DummyWorksheet.Parse(
@"#Config
#End"));

			var descriptor = new Mock<IDocumentDescriptor>();
			var documentData = bizObj.LoadOrCreateDocumentData(documentName);

			var services = new ServiceContainer();

			var securityService = new Mock<IDocumentSecurityService>();
			services.Register<IDocumentSecurityService>(securityService.Object);

			var notificationService = new Mock<IUserNotificationService>();
			services.Register<IUserNotificationService>(notificationService.Object);
			services.Register<IEventBroker>(new EventBroker());

			var messageInstructions = new DummyMessageInstructions
			{
				DocumentName = documentName,
				EHubClientID = "ZZZ"
			};

			var printInstructions = new DummyPrintInstructions
			{
				Title = documentName
			};

			var eDocsInstructions = new DummyEDocsInstructions
			{
				SaveCopyToEDocs = true,
				Parent = bizObj
			};

			securityService
				.Setup(s => s.CanSendMessage)
				.Returns(true);

			notificationService
				.Setup(s => s.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);

			notificationService
				.Setup(s => s.QueryUserResponse(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
				.Returns("response");

			descriptor
				.Setup(d => d.DataContext)
				.Returns(DataContext.UXML);

			descriptor
				.Setup(d => d.MessageInstructions)
				.Returns(messageInstructions);

			descriptor
				.Setup(d => d.PrintInstructions)
				.Returns(printInstructions);

			descriptor
				.Setup(d => d.EDocsInstructions)
				.Returns(eDocsInstructions);

			descriptor
				.Setup(d => d.Name)
				.Returns(documentName);

			var info = new DocumentInfo(
				new Lazy<ITemplate>(() => template),
				new Lazy<IVisualizerDocumentData>(() => documentData),
				new Lazy<IServiceContainer>(() => services),
				new Lazy<IDocumentDescriptor>(() => descriptor.Object),
				new Lazy<IDocument>(() => document));

			messaging.OnDocumentInfosCreated(info);

			return documentData;
		}

		IEnumerable<string> GetPrintJobs(BusinessObjectFactory factory)
		{
			var printJobs = factory.Load<StmPrintJob>(new ZDBOnlyQuery(typeof(StmPrintJob)));

			return printJobs
				.Select(pj => $"{pj.SP_DocumentName}|{pj.SP_JobType}|{pj.SP_EmailSubjectLine}");
		}

		void AddEvents(IStmALogParent logParent, string documentName, IEnumerable<ZArchitecture.Business.Event> events)
		{
			foreach (var @event in events)
			{
				logParent.Logs.AddNew(@event,
					ZDateTimeOffset.Now,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));
			}

			logParent.Factory.Save();
			Thread.Sleep(10);
		}

		#endregion

		#region Implementation

		IDisposable progressManagerDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			var progressManagerMock = new Mock<IProgressManager>();
			progressManagerDisposable = ObjectFactory.Substitute(progressManagerMock.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			progressManagerDisposable.Dispose();
		}

		protected override ICommandProvider CreateNewModule() => new Presentation.Messaging();

		#endregion
	}
}
