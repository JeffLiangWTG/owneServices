using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class MessageExtensionsTest : TestCaseWithFactory
	{
		#region TestSubmissionVersion

		public void TestSubmissionVersion()
		{
			var dummy = Factory.New<DummyWithLogs>();

			AssertEquals("no messages sent", 1, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("no messages sent", 1, dummy.CalculateSubmissionVersion("bbb"));

			dummy.Logs.AddNew(Events.MessageSent,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "bbb"));

			AssertEquals("one sent", 1, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("bbb"));

			dummy.Logs.AddNew(Events.InterchangeRejected);

			AssertEquals("one sent", 1, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("bbb"));

			dummy.Logs.AddNew(Events.MessageSent);

			AssertEquals("one sent", 1, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("bbb"));

			dummy.Logs.AddNew(Events.MessageSent,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "aaa"));

			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("bbb"));

			dummy.Logs.AddNew(Events.MessageSent,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "bbb"));

			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("one sent", 3, dummy.CalculateSubmissionVersion("bbb"));

			dummy.Logs.AddNew(Events.MessageAccepted);

			AssertEquals("one sent", 2, dummy.CalculateSubmissionVersion("aaa"));
			AssertEquals("one sent", 3, dummy.CalculateSubmissionVersion("bbb"));
		}

		#endregion

		#region HasSentDocument

		public void TestHasSentDocument()
		{
			var dummy = Factory.New<DummyWithLogs>();

			const string document1Name = "test1";
			const string document2Name = "test2";

			Assert(!dummy.HasSentDocument(document1Name));
			Assert(!dummy.HasSentDocument(document2Name));

			CreateLogs(dummy, document1Name, false, Events.MessageSent);

			Assert(dummy.HasSentDocument(document1Name));
			Assert(!dummy.HasSentDocument(document2Name));
		}

		#endregion

		#region TestDataVersion

		public void TestDataVersion()
		{
			AssertDataVersion("no sent messages",
					Enumerable.Empty<LogTemplate>(),
					"Shipping Instruction", 1);

			AssertDataVersion("one sent message",
				new[] { NewLog("MSN", "Shipping Instruction"), NewLog("DEX") },
				"Shipping Instruction", 2);

			AssertDataVersion("sent message, but interchange got rejected",
				new[] { NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("IRJ", "Shipping Instruction") },
				"Shipping Instruction", 1);

			AssertDataVersion("one sent message for another document",
				new[] { NewLog("MSN", "Booking Request"), NewLog("DEX") },
				"Shipping Instruction", 1);

			AssertDataVersion("three sent messages",
				new[]
				{
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction")
				},
				"Shipping Instruction", 4);

			AssertDataVersion("three sent messages of which one is sent for another document",
				new[]
				{
						NewLog("MSN", "Booking Request"), NewLog("DEX"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction")
				},
				"Shipping Instruction", 3);

			AssertDataVersion("two messages sent last one has been cancelled",
				new[]
				{
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MWR", "Shipping Instruction"), NewLog("MWA", "Shipping Instruction")
				},
				"Shipping Instruction", 1);

			AssertDataVersion("two messages sent last one has been cancelled for another document",
				new[]
				{
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MWR", "Booking Request"), NewLog("MWA", "Booking Request")
				},
				"Shipping Instruction", 3);

			AssertDataVersion("two messages sent first one has been cancelled",
				new[]
				{
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MWR", "Shipping Instruction"), NewLog("MWA", "Shipping Instruction"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction")
				},
				"Shipping Instruction", 2);

			AssertDataVersion("two messages sent for different documents",
				new[]
				{
						NewLog("MSN", "Booking Request"), NewLog("DEX"), NewLog("MAA", "Booking Request"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction")
				},
				"Shipping Instruction", 2);

			AssertDataVersion("reset to original for Booking Request",
				new[]
				{
						NewLog("MSN", "Booking Request"), NewLog("DEX"), NewLog("STU", "Booking Request"),
						NewLog("MSN", "Shipping Instruction"), NewLog("DEX"), NewLog("MAA", "Shipping Instruction")
				},
				"Booking Request", 1);

			AssertDataVersion("send Bill Of Lading",
				new[]
				{
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX")
				},
				"Bill Of Lading", 2);

			AssertDataVersion("send Bill Of Lading, accepted",
				new[]
				{
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATH", "Bill Of Lading")
				},
				"Bill Of Lading", 2);

			AssertDataVersion("send three Bill Of Ladings, all three accepted",
				new[]
				{
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATH", "Bill Of Lading"),
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATH", "Bill Of Lading"),
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATH", "Bill Of Lading"),
				},
				"Bill Of Lading", 4);

			AssertDataVersion("send Bill Of Lading, rejected",
				new[]
				{
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATR", "Bill Of Lading")
				},
				"Bill Of Lading", 1);

			AssertDataVersion("send three Bill Of Ladings, two first got rejected and last one is accepted",
				new[]
				{
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATR", "Bill Of Lading"),
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATR", "Bill Of Lading"),
					NewLog("MSN", "Bill Of Lading"), NewLog("DEX"), NewLog("ATH", "Bill Of Lading"),
				},
				"Bill Of Lading", 2);
		}

		public void TestDataVersion_OrderByEventTime()
		{
			var documentData = Factory.New<DummyWithLogs>();
			var documentName = "Test Document";

			CreateLog(documentData, documentName, false, Events.MessageSent, DateTime.Now.AddHours(-2));
			CreateLog(documentData, documentName, false, Events.MessageAccepted, DateTime.Now);

			AssertEquals(2, documentData.CalculateDataVersion(documentName, true));
		}

		void AssertDataVersion(string message, IEnumerable<LogTemplate> logTemplates, string documentName, int expected)
		{
			var dummy = Factory.New<DummyWithLogs>();

			foreach (var template in logTemplates)
			{
				template.CreateLog(dummy);
				Factory.Save();
			}

			AssertEquals(message, expected, dummy.CalculateDataVersion(documentName, false));
		}

		#endregion

		#region GetCurrentMessageStatus

		public void TestGetCurrentMessageStatus_StatusUsesLatestMessage()
		{
			var documentData = Factory.New<DummyWithLogs>();

			CreateLogs(documentData, "Test Document", false, Events.DataExport, Events.MessageSent);
			CreateLogs(documentData, "Test Document", false, Events.MessageAccepted);

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = "Test Document"
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = false
			};

			AssertEquals("Test Document Message Accepted", documentData.GetCurrentMessageStatus(document, messageInstructions));
		}

		public void TestGetCurrentMessageStatus_Authorised()
		{
			var documentData = Factory.New<DummyWithLogs>();

			CreateLogs(documentData, "Test Document", false, Events.DataExport, Events.MessageSent);
			CreateLogs(documentData, "Test Document", false, Events.Authorised);

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = "Test Document"
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = false
			};

			AssertEquals("Test Document  Authorized", documentData.GetCurrentMessageStatus(document, messageInstructions));
		}

		public void TestGetCurrentMessageStatus_OnlyShowStatusForCurrentDocument()
		{
			var documentData = Factory.New<DummyWithLogs>();

			const string document1Name = "Test Document 1";
			const string document2Name = "Test Document 2";

			CreateLogs(documentData, document1Name, false, Events.DataExport, Events.MessageSent);
			CreateLogs(documentData, document2Name, false, Events.MessageAccepted);

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = document1Name
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = false
			};

			AssertEquals("Test Document 1 Message Sent", documentData.GetCurrentMessageStatus(document, messageInstructions));
		}

		public void TestGetCurrentMessageStatus_NoMessageSent()
		{
			var documentData = Factory.New<DummyWithLogs>();

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = "Document 1"
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = false
			};

			AssertEquals("No Document 1 Messages Have Been Sent.", documentData.GetCurrentMessageStatus(document, messageInstructions));
		}

		public void TestGetCurrentMessageStatus_NoMessageSent_TranslatedDocumentName()
		{
			var documentData = Factory.New<DummyWithLogs>();

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = "Document 1"
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = false,
				TranslatedDocumentName = "Translated Document 1"
			};

			AssertEquals("No Translated Document 1 Messages Have Been Sent.", documentData.GetCurrentMessageStatus(document, messageInstructions));
		}

		public void TestGetCurrentMessageStatus_OrderByEventTime()
		{
			var documentData = Factory.New<DummyWithLogs>();
			var documentName = "Test Document";

			CreateLog(documentData, documentName, false, Events.MessageSent, DateTime.Now);
			CreateLog(documentData, documentName, false, Events.MessagePendingProcessing, DateTime.Now.AddHours(-1));

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = "Test Document"
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = true
			};

			AssertEquals("Test Document Message Sent", documentData.GetCurrentMessageStatus(document, messageInstructions));
		}

		public void TestGetCurrentMessageStatus_DocumentSpecificEventDisplay()
		{
			var dummy = Factory.New<DummyWithUXmlSupport>();
			var documentData = VisualizerDocumentDataExtensions.LoadOrCreateDocumentData(dummy, "aaa");

			var document = new DummyDocument
			{
				DataContext = "zzz",
				Name = "Test Document"
			};

			var messageInstructions = new DummyMessageInstructions
			{
				OrderLogsByLocalTime = true
			};

			var messagingExtensions = new Mock<IMessagingExtensions>();

			dummy.MessagingExtensions = messagingExtensions.Object;

			const string messageStatus = "message status";

			messagingExtensions
				.Setup(ext => ext.GetMessageStatus())
				.Returns(messageStatus);

			var status = documentData.GetCurrentMessageStatus(document, messageInstructions);
			AssertEquals("custom message status", messageStatus, status);

			messagingExtensions.Verify(ext => ext.GetMessageStatus(), Times.Once);
		}

		#endregion

		#region MessageStatusEventCodes

		public void TestMessageStatusEventCodes()
		{
			var expectedCodes = new[]
			{
				"MSN", "MWR", "MRR", "MPP", "STU", "ISN", "IRA", "MAA", "MWA", "MRJ", "IRJ", "IRA", "ATH", "ATR"
			};

			AssertContainsExactElementsInAnyOrder("If you want to add or remove any codes, please confirm it with a product staff at first.",
				expectedCodes,
				MessageEventCodes.MessageStatusEventCodes);
		}

		#endregion

		#region TestGetDialogs

		public void TestGetDialogs()
		{
			var dummy = Factory.New<DummyWithLogs>();

			const string document1Name = "test1";
			const string document2Name = "test2";
			const string document3Name = "test3";
			const string document4Name = "test4";

			CreateLogs(dummy, document1Name, false, Events.DataExport, Events.MessageSent);
			CreateLogs(dummy, document1Name, false, Events.MessageAccepted);

			CreateLogs(dummy, document1Name, false, Events.DataExport, Events.MessageSent);
			CreateLogs(dummy, document1Name, false, Events.MessageAccepted);

			CreateLogs(dummy, document2Name, false, Events.DataExport, Events.MessageSent);

			CreateLogs(dummy, document3Name, true, Events.DataExport, Events.MessageSent);

			CreateLogs(dummy, document4Name, false, Events.DataExport, Events.MessageSent);
			CreateLogs(dummy, document4Name, false, Events.AuthorisationRejected);

			CreateLogs(dummy, document4Name, false, Events.DataExport, Events.MessageSent);
			CreateLogs(dummy, document4Name, false, Events.Authorised);

			var dialogs = dummy.GetDialogs(document1Name, false);
			AssertEquals("dialogs for 1st doc (MSN->MAA and MSN->MAA)", 2, dialogs.Count());

			dialogs = dummy.GetDialogs(document2Name, false);
			AssertEquals("dialogs for 2nd doc (MSN)", 1, dialogs.Count());

			dialogs = dummy.GetDialogs(document3Name, false);
			AssertEquals("dialogs for 3rd doc (MSN cancelled)", 0, dialogs.Count());

			dialogs = dummy.GetDialogs(document4Name, false);
			AssertEquals("dialogs for 4th doc (MSN->ATR and MSN->ATH", 2, dialogs.Count());
		}

		public void TestGetDialogs_OrderByEventTime()
		{
			var dummy = Factory.New<DummyWithLogs>();

			var documentName = "Test Document";

			CreateLog(dummy, documentName, false, Events.MessageSent, DateTime.Now.AddHours(-4));
			CreateLog(dummy, documentName, false, Events.MessagePendingProcessing, DateTime.Now.AddHours(-3));
			CreateLog(dummy, documentName, false, Events.MessageAccepted, DateTime.Now.AddHours(-1));
			CreateLog(dummy, documentName, false, Events.MessageSent, DateTime.Now.AddHours(-2));
			CreateLog(dummy, documentName, false, Events.MessageRejected, DateTime.Now);

			var dialogs = dummy.GetDialogs(documentName, true);
			AssertEquals("no confirmation event in current dialog.", "", dialogs.First().ResponseCode);
			AssertEquals("MRJ", dialogs.ElementAt(1).ResponseCode);
		}

		public void TestGetDialogs_IsDialogInitiatingEvent()
		{
			var dummy = Factory.New<DummyWithLogs>();

			var documentName = "Test Document";

			CreateLogs(dummy, documentName, false, Events.DataExport, Events.MessageSent);
			CreateLog(dummy, documentName, false, Events.StatusUpdated, DateTime.Now, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			CreateLogs(dummy, documentName, false, Events.DataExport, Events.MessageWithdrawCancelRequest);
			CreateLog(dummy, documentName, false, Events.StatusUpdated, DateTime.Now, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			CreateLogs(dummy, documentName, false, Events.DataExport, Events.MessageSent);
			CreateLogs(dummy, documentName, false, Events.AuthorisationRejected);

			CreateLog(dummy, documentName, false, Events.StatusUpdated, DateTime.Now, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Enterprise.Core.Constants.EventReferenceMessageTypes.ResetToOriginal));

			CreateLogs(dummy, documentName, false, Events.DataExport, Events.MessageSent);
			CreateLog(dummy, documentName, false, Events.StatusUpdated, DateTime.Now, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			var dialogs = dummy.GetDialogs(documentName, false, (StmALog log) => MessageEventCodes.SentMessagesEventCodes.Contains(log.SL_SE_NKEvent.ToString())
					|| (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type) == Enterprise.Core.Constants.EventReferenceMessageTypes.ResetToOriginal));
			AssertEquals("(MSN->STU, MSN->STU, MSN->ATR, Reset STU, MSN->STU)", 5, dialogs.Count());
		}

		#endregion

		#region TestGetUXml

		public void TestGetUXml_IForwardingDocDataObjectUXmlWriter()
		{
			var document = new EmptyDocument("Test Document Name", "Test Data Context");
			var expectedShipment = new UniversalDataBuss.DataObjects.Universal.Shipment();
			expectedShipment.GoodsDescription = "Expected Shipment";

			var nonExpectedShipment = new UniversalDataBuss.DataObjects.Universal.Shipment();
			nonExpectedShipment.GoodsDescription = "Non-Expected Shipment";

			Assert(!(document.Data.Value is UniversalDataBuss.Integration.IDataObject));

			var forwardingDocDataObjectUXmlWriter = new Mock<IForwardingDocDataObjectUXmlWriter>();
			forwardingDocDataObjectUXmlWriter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, MessageType.Unspecified)).Returns(expectedShipment);
			ObjectFactory.Substitute("IForwardingDocDataObjectUXmlWriter", forwardingDocDataObjectUXmlWriter.Object);

			var agencyDocDataObjectUXmlWriter = new Mock<IAgencyDocDataObjectUXmlWriter>();
			agencyDocDataObjectUXmlWriter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, MessageType.Unspecified)).Returns(nonExpectedShipment);
			ObjectFactory.Substitute("IAgencyDocDataObjectUXmlWriter", agencyDocDataObjectUXmlWriter.Object);

			var result = document.GetUXml("", "Test Data Context");

			AssertNotNull(result);
			AssertEquals(@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <GoodsDescription>Expected Shipment</GoodsDescription>
  </Shipment>
</UniversalShipment>", result.ToString());
		}

		public void TestGetUXml_IAgencyDocDataObjectUXmlWriter()
		{
			var document = new EmptyDocument("Test Document Name", "Test Data Context");
			var expectedShipment = new UniversalDataBuss.DataObjects.Universal.Shipment();
			expectedShipment.GoodsDescription = "Expected Shipment";

			Assert(!(document.Data.Value is UniversalDataBuss.Integration.IDataObject));

			var forwardingDocDataObjectUXmlWriter = new Mock<IForwardingDocDataObjectUXmlWriter>();
			forwardingDocDataObjectUXmlWriter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, MessageType.Unspecified)).Returns((UniversalDataBuss.DataObjects.Universal.Shipment)null);
			ObjectFactory.Substitute("IForwardingDocDataObjectUXmlWriter", forwardingDocDataObjectUXmlWriter.Object);

			var agencyDocDataObjectUXmlWriter = new Mock<IAgencyDocDataObjectUXmlWriter>();
			agencyDocDataObjectUXmlWriter.Setup(d => d.GetDataObject(DefaultDataObjectWriterStrategy.Instance, document, MessageType.Unspecified)).Returns(expectedShipment);
			ObjectFactory.Substitute("IAgencyDocDataObjectUXmlWriter", agencyDocDataObjectUXmlWriter.Object);

			var result = document.GetUXml("", "Test Data Context");

			AssertNotNull(result);
			AssertEquals(@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <GoodsDescription>Expected Shipment</GoodsDescription>
  </Shipment>
</UniversalShipment>", result.ToString());
		}

		#endregion

		#region MatchesDocumentName

		public void TestMatchesDocumentName()
		{
			var dummy = Factory.New<DummyWithLogs>();

			var log1 = dummy.Logs.AddNew(Events.MessageSent, "|MST=test doc");
			Assert("expected to match MSN document name via MST parameter", log1.MatchesDocumentName("test doc"));
			Assert("expected not to match MSN document name", !log1.MatchesDocumentName("some other doc name"));

			var log2 = dummy.Logs.AddNew(Events.Authorised, "|TYP=test doc");
			Assert("expected to match ATH document name via TYP parameter", log2.MatchesDocumentName("test doc"));
			Assert("expected not to match ATH document name", !log2.MatchesDocumentName("some other doc name"));

			var log3 = dummy.Logs.AddNew(Events.InterchangeRejected, "|MST=test doc");
			Assert("expected to match IRJ document name via MST parameter", log3.MatchesDocumentName("test doc"));
			Assert("expected not to match IRJ document name", !log3.MatchesDocumentName("some other doc name"));

			var log4 = dummy.Logs.AddNew(Events.AuthorisationRejected, "|TYP=test doc");
			Assert("expected to match ATR document name via TYP parameter", log4.MatchesDocumentName("test doc"));
			Assert("expected not to match ATR document name", !log4.MatchesDocumentName("some other doc name"));
		}

		#endregion

		#region Implementation

		LogTemplate NewLog(string code, string documentName = null)
		{
			return new LogTemplate(code, documentName);
		}

		sealed class LogTemplate
		{
			public LogTemplate(string code, string documentName)
			{
				this.code = code;
				this.documentName = documentName;
			}

			readonly string code;
			readonly string documentName;

			public void CreateLog(IStmALogParent parent)
			{
				var log = parent.Logs.AddNew();

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_SE_NKEvent = code;

					if (!string.IsNullOrWhiteSpace(documentName))
					{
						var parameterName = log.GetParameterNameWhichStoresDocumentName();

						log.Parameters.Add(
							new KeyValuePair<string, string>(parameterName,
								documentName));
					}
				}
			}
		}

		void CreateLogs(IStmALogParent parent, string documentName, bool isCancelled, params Event[] events)
		{
			foreach (var @event in events)
			{
				var log = parent.Logs.AddNew(@event);

				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					var parameterName = log.GetParameterNameWhichStoresDocumentName();
					log.Parameters.Add(
						new KeyValuePair<string, string>(parameterName,
							documentName));
				}

				if (isCancelled)
				{
					log.Cancel();
				}
			}

			Factory.Save();
			Thread.Sleep(10);
		}

		void CreateLog(IStmALogParent parent, string documentName, bool isCancelled, Event messageEvent, DateTime eventTime, params KeyValuePair<string, string>[] parameters)
		{
			var log = parent.Logs.AddNew(messageEvent, eventTime);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				var parameterName = log.GetParameterNameWhichStoresDocumentName();
				log.Parameters.Add(
					new KeyValuePair<string, string>(parameterName,
						documentName));

				foreach (var parameter in parameters)
				{
					log.Parameters.Add(parameter);
				}
			}

			if (isCancelled)
			{
				log.Cancel();
			}

			Factory.Save();
			Thread.Sleep(10);
		}

		#endregion
	}
}
