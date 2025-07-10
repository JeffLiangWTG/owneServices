using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;
using NotificationsHandler = Enterprise.DocumentVisualizer.Business.NotificationsHandler;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class NoUIMessageSenderTest : TestCaseWithFactory
	{
		[TestDate(2020, 7, 25)]
		public void TestSendUXml()
		{
			using (Factory.AddDisposableService())
			{
				var messageInstructions = new DummyMessageInstructions
				{
					DocumentName = "De revolutionibus orbium coelestium",
					EHubClientID = "I_AM_YOUR_CLIENT",
					XmlNamespace = "XXX/1"
				};

				var dummy = Factory.New<DummyWithUXmlSupport>();
				var documentDataStorage = dummy.LoadOrCreateDocumentData("xxx");

				var dataObject = new DummyDocDataObject();
				var notifications = new NotificationsHandler();

				var res = NoUIMessageSender.SendUXml(messageInstructions, documentDataStorage, dataObject, notifications);

				Assert("message has been sent", res);

				var logs = documentDataStorage
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
						|| l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", "|MST=De revolutionibus orbium coelestium", msn.SL_Reference);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
				AssertEquals("Send to eHub", EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, message.Message.Interchange.EI_TransportType);
				AssertEquals("eHub client name", "I_AM_YOUR_CLIENT", message.Message.Interchange.EI_To);
				AssertIsXml("UXml", message.Message.EM_MessageText)
					.HavingExactlyOneChildNode("Shipment/DataContext/DocumentaryOverride/DocumentName",
						dn => dn.WithValue(messageInstructions.DocumentName)
					).HavingExactlyOneChildNode("Shipment/WayBillNumber",
						wbn => wbn.WithValue("12345")
					);
			}
		}

		[TestDate(2024, 11, 15)]
		public void TestSendUXmlToDirectXT()
		{
			using (Factory.AddDisposableService())
			{
				var messageInstructions = new DummyMessageInstructions
				{
					DocumentName = "De revolutionibus orbium coelestium",
					EHubClientID = "I_AM_YOUR_CLIENT",
					DirectXTClientID = "DIRECT_XT_CLIENT_5",
					XmlNamespace = "XXX/1"
				};

				var dummy = Factory.New<DummyWithUXmlSupport>();
				dummy.MessageBroker = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

				var documentDataStorage = dummy.LoadOrCreateDocumentData("xxx");

				var dataObject = new DummyDocDataObject();
				var notifications = new NotificationsHandler();

				var res = NoUIMessageSender.SendUXml(messageInstructions, documentDataStorage, dataObject, notifications);

				Assert("message has been sent", res);

				var logs = documentDataStorage
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
						|| l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", "|MST=De revolutionibus orbium coelestium", msn.SL_Reference);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
				AssertEquals("Send to Direct XT", EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, message.Message.Interchange.EI_TransportType);
				AssertEquals("Direct XT client name", "DIRECT_XT_CLIENT_5", message.Message.Interchange.EI_To);
				AssertIsXml("UXml", message.Message.EM_MessageText)
					.HavingExactlyOneChildNode("Shipment/DataContext/DocumentaryOverride/DocumentName",
						dn => dn.WithValue(messageInstructions.DocumentName)
					).HavingExactlyOneChildNode("Shipment/WayBillNumber",
						wbn => wbn.WithValue("12345")
					);
			}
		}

		[TestDate(2020, 7, 25)]
		public void TestSendUXmlWithDocumentInfo()
		{
			using (Factory.AddDisposableService())
			{
				var messageInstructions = new DummyMessageInstructions
				{
					DocumentName = "De revolutionibus orbium coelestium",
					EHubClientID = "I_AM_YOUR_CLIENT",
					XmlNamespace = "XXX/1"
				};

				var dummy = Factory.New<DummyWithUXmlSupport>();
				var documentDataStorage = dummy.LoadOrCreateDocumentData("xxx");
				var dataObject = new DummyDocDataObject();

				var messagingExtensions = new Mock<IMessagingExtensions>();
				messagingExtensions.Setup(m => m.GetXmlNamespace()).Returns("YYY/1");

				dummy.MessagingExtensions = messagingExtensions.Object;

				var notifications = new NotificationsHandler();
				var services = new ServiceContainer();

				var document = new Mock<IDocument>();
				var documentInfo = new Mock<IDocumentInfo>();

				documentInfo.Setup(i => i.Document).Returns(document.Object);
				documentInfo.Setup(i => i.DocumentData).Returns(documentDataStorage);
				documentInfo.Setup(i => i.Services).Returns(services);
				document.Setup(d => d.Data).Returns(dataObject.MakeDynamic());

				var res = NoUIMessageSender.SendUXml(messageInstructions, documentInfo.Object, notifications);

				Assert("message has been sent", res);

				var logs = documentDataStorage
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
						|| l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", "|MST=De revolutionibus orbium coelestium", msn.SL_Reference);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);

				AssertIsXml("UXml's namespace had beed replaced from the related message extension", message.Message.EM_MessageText)
					.HavingExactlyOneChildNode("Shipment/DataContext/DocumentaryOverride/DocumentName",
						dn => dn.WithValue(messageInstructions.DocumentName)
					).HavingExactlyOneChildNode("Shipment/WayBillNumber",
						wbn => wbn.WithValue("12345")
					);
			}
		}
	}
}
