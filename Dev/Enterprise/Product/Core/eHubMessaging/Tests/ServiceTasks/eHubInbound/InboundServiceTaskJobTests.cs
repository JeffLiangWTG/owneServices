using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.Tests.Business.DownloadHandler;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.eHubInbound
{
	[TestedType(typeof(InboundServiceTaskJob))]
	class InboundServiceTaskJobTests : ServiceTaskJobWithAdapterTests<InboundServiceTaskJob>
	{
		protected override void TestExecuteLockCore(DbConnection extraConnection)
		{
			var company1 = CreateCompanyWithBranch();
			var company2 = CreateCompanyWithBranch();
			var companySettingsManager = eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company1, company2 });

			var serviceTaskJob1 = CreateMockJob_Moq(companySettingsManager.Object, company1);
			serviceTaskJob1.Setup(x => x.DbConnection).Returns(Db.Connection);

			var serviceTaskJob2 = CreateMockJob_Moq(companySettingsManager.Object, company2);
			serviceTaskJob2.Setup(x => x.DbConnection).Returns(extraConnection);
			serviceTaskJob2.Setup(x => x.ProcessMessagesCore())
				.Callback(new Action(() =>
				{
					AssertEquals(company2, serviceTaskJob2.Object.CurrentCompany);
					Task.Delay(20).Wait();
				}));

			serviceTaskJob1.Setup(x => x.ProcessMessagesCore())
				.Callback(new Action(() =>
				{
					AssertEquals(company1, serviceTaskJob1.Object.CurrentCompany);
					ExecuteJobWithServiceTaskContext(serviceTaskJob2.Object, "EHI");
					serviceTaskJob1.Setup(m => m.ProcessMessagesCore()).CallBase();
				}));

			ExecuteJobWithServiceTaskContext(serviceTaskJob1.Object, "EHI");
			serviceTaskJob1.VerifyAll();
			serviceTaskJob2.VerifyAll();
		}

		public void TestReceiveInterchangeNoMessages()
		{
			var mockServiceTaskJob = CreateMockJob_Moq();
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Adapter download completed and read.");
		}

		IeHubMessage CreateDEAMessage()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream);
			var headerContent = @"<SenderID>DECUSTOMSTEST</SenderID><RecipientID>HYEZNTCMT</RecipientID><InterchangeType>DEA</InterchangeType><InterchangeNumber>0000003115962</InterchangeNumber>";
			var bodyContent = @"<ns0:SomeRootElement xmlns:ns0=""http://SomeSchema/Test""><Description>This is a dummy message that is added to the body of the generic message interchange</Description></ns0:SomeRootElement>";
			var xmlContent = $@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header>{headerContent}</Header><Body>{bodyContent}</Body></ns0:GenericMessageInterchange>";
			writer.Write(xmlContent);
			writer.Flush();

			var trackingID = Guid.NewGuid();

			var message = new Mock<IeHubMessage>();
			message.Setup(m => m.MessageStream).Returns(messageStream);
			message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.GenericMessageDelivery);
			message.Setup(m => m.SenderID).Returns("HYEZNTCMT");
			message.Setup(m => m.RecipientID).Returns("HYEZNTCMT");
			message.Setup(m => m.TrackingID).Returns(trackingID);
			message.Setup(m => m.Filename).Returns("FileName");
			message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.GenericMessageDelivery);
			return message.Object;
		}

		public void TestReceiveInterchangeSuccess_DuplicateDEAMessages_WI00261419()
		{
			var dea1 = CreateDEAMessage();
			var dea2 = CreateDEAMessage();
			var interchanges = new List<IeHubMessage> { dea1, dea2 };

			var mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.SetupSequence(m => m.Count).Returns(1).Returns(1).Returns(0);
			mockInbox.Setup(m => m.GetEnumerator()).Returns(interchanges.GetEnumerator());
			mockInbox.Setup(m => m.MarkAsRead());

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			var results = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, new[] { dea1.TrackingID, dea2.TrackingID }));
			CombineAssertions(() =>
			{
				AssertEquals("Interchange not created", 2, results.Length);

				var first = results.Single(x => x.EI_SessionGUID == dea1.TrackingID);
				AssertEquals("First.IsInDatabase", true, first.IsInDatabase);
				AssertEquals("First.EI_Status", "QUE", first.EI_Status);
				AssertEquals("First.EI_InterchangeNum", "0000003115962", first.EI_InterchangeNum);
				AssertEquals("First.EI_From", "HYEZNTCMT", first.EI_From);
				AssertEquals("First.EI_To", "HYEZNTCMT", first.EI_To);

				var duplicate = results.Single(x => x.EI_SessionGUID == dea2.TrackingID);
				AssertEquals("Duplicate.IsInDatabase", true, duplicate.IsInDatabase);
				AssertEquals("Duplicate.EI_Status", "FAL", duplicate.EI_Status);
				AssertNotEquals("Duplicate.EI_InterchangeNum", first.EI_InterchangeNum, duplicate.EI_InterchangeNum);
				AssertEquals("Duplicate.EI_From", "HYEZNTCMT", duplicate.EI_From);
				AssertEquals("Duplicate.EI_To", "HYEZNTCMT", duplicate.EI_To);
				AssertEquals("Duplicate.ST_NoteDataAsText"
					, @"The value of Interchange Number + From + To must be unique on EDIInterchange. The duplicate value(s) are: (0000003115962, HYEZNTCMT, HYEZNTCMT)."
					, duplicate.Notes.GetAllNotes().Cast<StmNote>().Single(x => x.ST_Description == "eHub: Duplication Detected").ST_NoteDataAsText);
			});

			mockInbox.VerifyAll();
			mockAdapter.VerifyAll();

			mockInbox.Verify(m => m.MarkAsRead(), Times.Exactly(2));
			mockAdapter.Verify(m => m.RetrieveMessages(), Times.Exactly(2));
			mockAdapter.Verify(m => m.Dispose(), Times.Exactly(1));
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(6));
		}

		public void TestReceiveInterchangeSuccess_DuplicateEHubMessages_SenderRecipientModifiedDuringProcessing()
		{
			var messageCreationFunc = new Func<Guid, string, string, IeHubMessage>((trackingId, sender, recipient) =>
			{
				var messageStream = new MemoryStream();
				var writer = new StreamWriter(messageStream);
				writer.Write(@"<ns0:NZCustomsReply xmlns:ns0=""http://cargowise.com/ehub/products/""><ns0:Reference>00009908C</ns0:Reference><ns0:Content>PERvY3VtZW50TWV0YWRhdGEgeG1sbnM9J3Vybjp3Y286ZGF0YW1vZGVsOldDTzpETToxJyB4bWxuczp4c2k9J2h0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlJz4KICA8V0NPRGF0YU1vZGVsVmVyc2lvbj4zLjI8L1dDT0RhdGFNb2RlbFZlcnNpb24+CiAgPFdDT0RvY3VtZW50TmFtZT5SRVM8L1dDT0RvY3VtZW50TmFtZT4KICA8Q291bnRyeUNvZGU+Tlo8L0NvdW50cnlDb2RlPgogIDxBZ2VuY3lBc3NpZ25lZEN1c3RvbWl6ZWREb2N1bWVudE5hbWU+UkVTT0NSPC9BZ2VuY3lBc3NpZ25lZEN1c3RvbWl6ZWREb2N1bWVudE5hbWU+CiAgPEFnZW5jeUFzc2lnbmVkQ3VzdG9taXplZERvY3VtZW50VmVyc2lvbj5WMS4wPC9BZ2VuY3lBc3NpZ25lZEN1c3RvbWl6ZWREb2N1bWVudFZlcnNpb24+CiAgPFJlc3BvbnNlPgogICAgPElzc3VlRGF0ZVRpbWUgZm9ybWF0Q29kZT0iMjA0IiA+MjAxMzAzMTQxNDA2NDE8L0lzc3VlRGF0ZVRpbWU+CiAgICA8RnVuY3Rpb25hbFJlZmVyZW5jZUlEPjY8L0Z1bmN0aW9uYWxSZWZlcmVuY2VJRD4KICAgIDxGdW5jdGlvbkNvZGU+MjQ8L0Z1bmN0aW9uQ29kZT4KICAgIDxPdmVyYWxsRGVjbGFyYXRpb24+CiAgICAgIDxEZWNsYXJhdGlvbj4KICAgICAgICA8SUQ+OTQ5NTA0NTU8L0lEPgogICAgICAgIDxBY2NlcHRhbmNlRGF0ZVRpbWUgZm9ybWF0Q29kZT0iMjA0IiA+MjAxMzAzMTQxNDA2NDE8L0FjY2VwdGFuY2VEYXRlVGltZT4KICAgICAgICA8RnVuY3Rpb25hbFJlZmVyZW5jZUlEPkMwMDAwMTAzMjwvRnVuY3Rpb25hbFJlZmVyZW5jZUlEPgogICAgICAgIDxWZXJzaW9uSUQvPgogICAgICAgIDxTdWJtaXR0ZXI+CiAgICAgICAgICA8SUQ+MDAwMDk5MDhDPC9JRD4KICAgICAgICA8L1N1Ym1pdHRlcj4KICAgICAgICA8UmVzcG9uc2libGVHb3Zlcm5tZW50QWdlbmN5PgogICAgICAgICAgPElEPk5aQ1M8L0lEPgogICAgICAgIDwvUmVzcG9uc2libGVHb3Zlcm5tZW50QWdlbmN5PgogICAgICA8L0RlY2xhcmF0aW9uPgogICAgPC9PdmVyYWxsRGVjbGFyYXRpb24+CiAgICA8U3RhdHVzPgogICAgICA8RWZmZWN0aXZlRGF0ZVRpbWUgZm9ybWF0Q29kZT0iMjA0IiA+MjAxMzAzMTQxNDA2NDE8L0VmZmVjdGl2ZURhdGVUaW1lPgogICAgICA8TmFtZUNvZGU+ODQ3PC9OYW1lQ29kZT4KICAgICAgPFJlbGVhc2VEYXRlVGltZSBmb3JtYXRDb2RlPSIyMDQiID4yMDEzMDMxNDE0MDY0MTwvUmVsZWFzZURhdGVUaW1lPgogICAgICA8UG9pbnRlcj4KICAgICAgICA8U2VxdWVuY2VOdW1lcmljPjE8L1NlcXVlbmNlTnVtZXJpYz4KICAgICAgICA8RG9jdW1lbnRTZWN0aW9uQ29kZT4wN0I8L0RvY3VtZW50U2VjdGlvbkNvZGU+CiAgICAgIDwvUG9pbnRlcj4KICAgICAgPFBvaW50ZXI+CiAgICAgICAgPFNlcXVlbmNlTnVtZXJpYz4xPC9TZXF1ZW5jZU51bWVyaWM+CiAgICAgICAgPERvY3VtZW50U2VjdGlvbkNvZGU+NDJBPC9Eb2N1bWVudFNlY3Rpb25Db2RlPgogICAgICA8L1BvaW50ZXI+CiAgICAgIDxQb2ludGVyPgogICAgICAgIDxTZXF1ZW5jZU51bWVyaWM+MTwvU2VxdWVuY2VOdW1lcmljPgogICAgICAgIDxEb2N1bWVudFNlY3Rpb25Db2RlPjA4QjwvRG9jdW1lbnRTZWN0aW9uQ29kZT4KICAgICAgICA8VGFnSUQ+RzAwNzwvVGFnSUQ+CiAgICAgIDwvUG9pbnRlcj4KICAgIDwvU3RhdHVzPgogIDwvUmVzcG9uc2U+CjwvRG9jdW1lbnRNZXRhZGF0YT4K</ns0:Content></ns0:NZCustomsReply>");
				writer.Flush();
				var message = new Mock<IeHubMessage>();
				message.Setup(m => m.MessageStream).Returns(messageStream);
				message.Setup(m => m.ApplicationCode).Returns(EDIInterchange.ApplicationCodes.NewZealandCustoms);
				message.Setup(m => m.RecipientID).Returns(recipient);
				message.Setup(m => m.SenderID).Returns(sender);
				message.Setup(m => m.TrackingID).Returns(trackingId);
				message.Setup(m => m.Filename).Returns("FileName");
				message.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.NZCustoms);
				return message.Object;
			});

			AssertReceiveInterchangeSuccess_DuplicateEHubMessages(messageCreationFunc, expectedSizeForTwoMessagesKB: 4, expectedSender: "CUSMOD", expectedRecipient: "00009908C", checkDifferentSender: false);
		}

		public void TestReceiveInterchangeSuccess_DuplicateEHubMessages()
		{
			const string messageBody = "<InboundMessage><Header><![CDATA[A             041114                                                            ]]></Header><Body><![CDATA[B064101D99RR                                                                    R14101D99 532273690195-268096500400144716MSCUMSC CHARLESTON      FG410041514    R4            ED685946    SHA400144716            00000102CT   MSCUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 R14101D99 532279970195-268096500400175690EGLVEVER DECENT         07230041514    R4            149400520639ZNV14030346             00000615PCE  EGLVJASF         R5041114001405PAPERLESS                                                         R5041114001422RELEASE DATE UPDATE                     04151402                  R6FDA    041114001401FDA REVIEW                                                 R14101D99 532304700195-268096500400170831CMDUXIN TAI CANG        0222E041514    R4            XMPC381045  XMN400170831            00001004CT   CMDUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 Y064101D99RR00015                                                               ]]></Body><Footer><![CDATA[Z             041114                                                            ]]></Footer></InboundMessage>";
			var messageCreationFunc = new Func<Guid, string, string, IeHubMessage>((trackingId, sender, recipient) =>
			{
				return new eHubMessage(trackingId, sender, recipient, MessageSchemaType.Xml, EDIInterchangeTypeList.Descriptions.USCustomsImport, "USCustoms Import", new MemoryStream(Encoding.ASCII.GetBytes(messageBody)));
			});
			AssertReceiveInterchangeSuccess_DuplicateEHubMessages(messageCreationFunc, expectedSizeForTwoMessagesKB: 3);
		}

		void AssertReceiveInterchangeSuccess_DuplicateEHubMessages(Func<Guid, string, string, IeHubMessage> messageCreationFunc, int expectedSizeForTwoMessagesKB, string expectedSender = null, string expectedRecipient = null, bool checkDifferentSender = true)
		{
			var eHubMessages = new List<IeHubMessage>();
			for (int i = 0; i < 2; i++)
			{
				eHubMessages.Add(messageCreationFunc.Invoke(Guid.NewGuid(), "SENDER", "RECIPIENT"));
			}

			var mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.Setup(m => m.Count).Returns(eHubMessages.Count);
			mockInbox.Setup(m => m.GetEnumerator()).Returns(eHubMessages.GetEnumerator());
			mockInbox.Setup(m => m.MarkAsRead()).Throws(new Exception("Communication with the server has timed out - The service task will reattempt the transfer on the next run"));

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			AssertExceptionThrown(typeof(Exception), "Communication with the server has timed out - The service task will reattempt the transfer on the next run", () => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			string receivedTwoMessagesNotification = string.Format("Retrieved 2 eHub Messages for company EDI. Total size(KB): {0}.", expectedSizeForTwoMessagesKB);
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists(receivedTwoMessagesNotification);
			var queryString = string.Join(", ", eHubMessages.Select(_ => $"'{_.TrackingID}'"));
			var query = new ZQuery();
			var builder = new SqlBuilder();
			builder.Append($"{EDIInterchange.Schema.EI_SessionGUID} IN ({queryString})");
			query.AddFilterString(builder);
			query.AddToFilter(EDIInterchangeSchema.EI_From, expectedSender ?? "SENDER");
			query.AddToFilter(EDIInterchangeSchema.EI_To, expectedRecipient ?? "RECIPIENT");
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			var interchanges = Factory.Load<EDIInterchange>(query).OrderBy(x => x.EI_InterchangeDateTime).ToArray();
			AssertEquals("Interchanges created", 2, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("QUE", interchange.EI_Status);
			}

			mockInbox.VerifyAll();
			mockInbox.Verify(m => m.Count, Times.Exactly(2));

			mockAdapter.VerifyAll();
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(4));

			mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.Setup(m => m.Count).Returns(eHubMessages.Count);
			mockInbox.Setup(m => m.GetEnumerator()).Returns(eHubMessages.GetEnumerator());
			mockInbox.Setup(m => m.MarkAsRead()).Throws(new Exception("Communication with the server has timed out - The service task will reattempt the transfer on the next run"));

			mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			AssertExceptionThrown(typeof(Exception), "Communication with the server has timed out - The service task will reattempt the transfer on the next run", () => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists(receivedTwoMessagesNotification);
			interchanges = Factory.Load<EDIInterchange>(query).OrderBy(x => x.EI_InterchangeNum).ToArray();
			AssertEquals("Don't create duplicated interchanges", 2, interchanges.Length);
			AssertEquals(2, interchanges.Count(_ => _.EI_Status == EDIInterchangeStatusList.Codes.Queued));
			AssertEquals(0, interchanges.Count(_ => _.EI_Status == EDIInterchangeStatusList.Codes.Failed));
			AssertNotEquals("Duplicate.EI_InterchangeNum", interchanges[0].EI_InterchangeNum, interchanges[1].EI_InterchangeNum);
			AssertDuplicateNotificationExists(mockServiceTaskJob.Object.Notifier, eHubMessages[0], expectedSender, expectedRecipient);
			AssertDuplicateNotificationExists(mockServiceTaskJob.Object.Notifier, eHubMessages[1], expectedSender, expectedRecipient);

			mockInbox.VerifyAll();
			mockInbox.Verify(m => m.Count, Times.Exactly(2));

			mockAdapter.VerifyAll();
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(4));

			if (!checkDifferentSender)
			{
				return;
			}

			eHubMessages.Add(messageCreationFunc.Invoke(eHubMessages[0].TrackingID, "SENDER1", "RECIPIENT"));
			mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.SetupSequence(m => m.Count)
				.Returns(eHubMessages.Count)
				.Returns(eHubMessages.Count)
				.Returns(0);
			mockInbox.Setup(m => m.GetEnumerator()).Returns(eHubMessages.GetEnumerator());
			mockInbox.Setup(m => m.MarkAsRead());

			mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Retrieved 3 eHub Messages for company EDI. Total size(KB): 4.");

			interchanges = Factory.Load<EDIInterchange>(query).OrderBy(x => x.EI_InterchangeNum).ToArray();
			AssertEquals("Don't create failed duplicated interchanges", 2, interchanges.Length);
			AssertEquals(2, interchanges.Count(_ => _.EI_Status == EDIInterchangeStatusList.Codes.Queued));
			AssertEquals(0, interchanges.Count(_ => _.EI_Status == EDIInterchangeStatusList.Codes.Failed));
			AssertNotEquals("Duplicate.EI_InterchangeNum", interchanges[0].EI_InterchangeNum, interchanges[1].EI_InterchangeNum);
			AssertDuplicateNotificationExists(mockServiceTaskJob.Object.Notifier, eHubMessages[0], expectedSender, expectedRecipient);
			AssertDuplicateNotificationExists(mockServiceTaskJob.Object.Notifier, eHubMessages[1], expectedSender, expectedRecipient);

			query = new ZQuery();
			query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, eHubMessages.Last().TrackingID);
			query.AddToFilter(EDIInterchangeSchema.EI_From, "SENDER1");
			query.AddToFilter(EDIInterchangeSchema.EI_To, "RECIPIENT");
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Queued);
			var interchanges_SENDER1 = Factory.Load<EDIInterchange>(query);
			AssertEquals("Created interchange having same tracking ID with existing interchange but different sender", 1, interchanges_SENDER1.Length);
			AssertNotEquals("Duplicate.EI_InterchangeNum", interchanges[0].EI_InterchangeNum, interchanges_SENDER1.First().EI_InterchangeNum);
			AssertNotEquals("Duplicate.EI_InterchangeNum", interchanges[1].EI_InterchangeNum, interchanges_SENDER1.First().EI_InterchangeNum);

			mockInbox.VerifyAll();
			mockInbox.Verify(m => m.Count, Times.Exactly(3));
			mockInbox.Verify(m => m.MarkAsRead(), Times.Exactly(2));

			mockAdapter.VerifyAll();
			mockAdapter.Verify(m => m.RetrieveMessages(), Times.Exactly(2));
			mockAdapter.Verify(m => m.Dispose());
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(6));
		}

		void AssertDuplicateNotificationExists(INotifications notifier, IeHubMessage message, string expectedSender = null, string expectedRecipient = null)
		{
			notifier.AssertNotificationExists($"Duplicate eHub Message received and discarded.  This was most likely caused by the connection to eHub being lost during the download process.  The values of the eHub Tracking ID, From, and To must be unique. The duplicate value(s) are: ({message.TrackingID}, {expectedSender ?? message.SenderID}, {expectedRecipient ?? message.RecipientID}).");
		}

		public void TestReceiveInterchangeSuccess()
		{
			var interchanges = new List<IeHubMessage>();
			const string messageBody = "<InboundMessage><Header><![CDATA[A             041114                                                            ]]></Header><Body><![CDATA[B064101D99RR                                                                    R14101D99 532273690195-268096500400144716MSCUMSC CHARLESTON      FG410041514    R4            ED685946    SHA400144716            00000102CT   MSCUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 R14101D99 532279970195-268096500400175690EGLVEVER DECENT         07230041514    R4            149400520639ZNV14030346             00000615PCE  EGLVJASF         R5041114001405PAPERLESS                                                         R5041114001422RELEASE DATE UPDATE                     04151402                  R6FDA    041114001401FDA REVIEW                                                 R14101D99 532304700195-268096500400170831CMDUXIN TAI CANG        0222E041514    R4            XMPC381045  XMN400170831            00001004CT   CMDUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 Y064101D99RR00015                                                               ]]></Body><Footer><![CDATA[Z             041114                                                            ]]></Footer></InboundMessage>";
			var trackingId = Guid.NewGuid();
			interchanges.Add(new eHubMessage(trackingId, "SENDER", "RECIPIENT", MessageSchemaType.Xml, EDIInterchangeTypeList.Descriptions.USCustomsImport, "USCustoms Import", new MemoryStream(Encoding.ASCII.GetBytes(messageBody))));

			var mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.SetupSequence(m => m.Count).Returns(1).Returns(1).Returns(0);
			mockInbox.Setup(m => m.GetEnumerator()).Returns(interchanges.GetEnumerator());
			mockInbox.Setup(m => m.MarkAsRead());

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Retrieved 1 eHub Messages for company EDI. Total size(KB): 1.");
			var results = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId));
			AssertEquals("Interchange not created", 1, results.Length);
			AssertEquals("QUE", results[0].EI_Status);

			mockInbox.VerifyAll();
			mockAdapter.VerifyAll();
			mockInbox.Verify(m => m.Count, Times.Exactly(3));
			mockInbox.Verify(m => m.MarkAsRead(), Times.Exactly(2));
			mockAdapter.Verify(m => m.RetrieveMessages(), Times.Exactly(2));
			mockAdapter.Verify(m => m.Dispose(), Times.Exactly(1));
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(6));
		}

		public void TestReceiveInterchange_EmptyMessageContent()
		{
			var interchanges = new List<IeHubMessage>();
			const string messageBody = "";
			var trackingId = Guid.NewGuid();
			interchanges.Add(new eHubMessage(trackingId, "SENDER", "RECIPIENT", MessageSchemaType.Xml, EDIInterchangeTypeList.Descriptions.XMS, "http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange", new MemoryStream(Encoding.ASCII.GetBytes(messageBody))));

			var mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.SetupSequence(m => m.Count).Returns(1).Returns(1).Returns(0);
			mockInbox.Setup(m => m.GetEnumerator()).Returns(interchanges.GetEnumerator());
			mockInbox.Setup(m => m.MarkAsRead());

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Retrieved 1 eHub Messages for company EDI. Total size(KB): 0.");
			var results = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId));
			var failedMessage = string.Format(
				@"Failed to create message - Interchange Session GUID - {0}, Sender - SENDER, Recipient - RECIPIENT, Schema - http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange, Message content - The message content is empty, Failure Log - Length of the message content is 0 before Get Payload Sub Type
System.Xml.XmlException: Root element is missing.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
   at System.Xml.XmlTextReaderImpl.ParseDocumentContent()".Trim(), trackingId);

			CombineAssertions(() =>
			{
				AssertEquals("Interchange created", 1, results.Length);
				AssertEquals("FAL", results[0].EI_Status);
			});

			ErrorReporter.Clear();
			mockInbox.VerifyAll();
			mockAdapter.VerifyAll();
			mockInbox.Verify(m => m.Count, Times.Exactly(3));
			mockInbox.Verify(m => m.MarkAsRead(), Times.Exactly(2));
			mockAdapter.Verify(m => m.RetrieveMessages(), Times.Exactly(2));
			mockAdapter.Verify(m => m.Dispose(), Times.Exactly(1));
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(6));
		}

		public void TestReceiveInterchangeTimeLimitSuccess()
		{
			// setup mailbox for first call
			var interchanges1 = new List<IeHubMessage>();
			const string messageBody = "<InboundMessage><Header><![CDATA[A             041114                                                            ]]></Header><Body><![CDATA[B064101D99RR                                                                    R14101D99 532273690195-268096500400144716MSCUMSC CHARLESTON      FG410041514    R4            ED685946    SHA400144716            00000102CT   MSCUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 R14101D99 532279970195-268096500400175690EGLVEVER DECENT         07230041514    R4            149400520639ZNV14030346             00000615PCE  EGLVJASF         R5041114001405PAPERLESS                                                         R5041114001422RELEASE DATE UPDATE                     04151402                  R6FDA    041114001401FDA REVIEW                                                 R14101D99 532304700195-268096500400170831CMDUXIN TAI CANG        0222E041514    R4            XMPC381045  XMN400170831            00001004CT   CMDUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 Y064101D99RR00015                                                               ]]></Body><Footer><![CDATA[Z             041114                                                            ]]></Footer></InboundMessage>";
			var trackingId1 = Guid.NewGuid();
			interchanges1.Add(new eHubMessage(trackingId1, "SENDER", "RECIPIENT", MessageSchemaType.Xml, EDIInterchangeTypeList.Descriptions.USCustomsImport, "USCustoms Import", new MemoryStream(Encoding.ASCII.GetBytes(messageBody))));

			var mockInbox1 = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox1.Setup(m => m.Count).Returns(1);
			mockInbox1.Setup(m => m.GetEnumerator()).Returns(interchanges1.GetEnumerator());
			mockInbox1.Setup(m => m.MarkAsRead());

			// setup adapter
			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox1.Object);

			// setup service task job
			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			mockServiceTaskJob.Setup(m => m.RetrieveForOneCompanyTimeLimitInSeconds).Returns(0);// set to zero so it exits after one retrieve.

			// execute service task job
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists("Retrieved 1 eHub Messages for company EDI. Total size(KB): 1.");

			// check outcome
			var results = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId1));
			AssertEquals("Interchange not created", 1, results.Length);
			AssertEquals("QUE", results[0].EI_Status);

			mockInbox1.VerifyAll();
			mockAdapter.VerifyAll();

			mockInbox1.Verify(m => m.Count, Times.Exactly(2));
			mockInbox1.Verify(m => m.GetEnumerator(), Times.Exactly(1));
			mockInbox1.Verify(m => m.MarkAsRead(), Times.Exactly(1));

			mockAdapter.Verify(m => m.RetrieveMessages(), Times.Exactly(1));
			mockAdapter.Verify(m => m.Dispose(), Times.Exactly(1));
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(4));

			mockServiceTaskJob.Verify(m => m.RetrieveForOneCompanyTimeLimitInSeconds);
		}

		[UseSnapshotProtection]
		public void TestStatusMessage_OutgoingInterchangeNotFound()
		{
			// setup a mock test to receive a status message.
			// the status message does not match any outgoing interchange and it should be deleted.

			var testInbox = new TestMessageInbox();
			var statusMessage = new Mock<IeHubMessage>();
			statusMessage.Setup(m => m.TrackingID).Returns(Guid.NewGuid());
			statusMessage.Setup(m => m.SenderID).Returns("eHub");
			statusMessage.Setup(m => m.RecipientID).Returns("ENT1");
			statusMessage.Setup(m => m.SchemaName).Returns(EDIInterchangeTypeList.Descriptions.MSS);
			statusMessage.Setup(m => m.MessageStream).Returns(new VirtualMemoryStream());
			statusMessage.Setup(m => m.ApplicationCode).Returns(string.Empty);
			testInbox.AddMessage(statusMessage.Object);

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(x => x.RetrieveMessages());
			mockAdapter.Setup(x => x.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(testInbox);

			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));

			ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI");
			mockServiceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("Failed to acknowledge message - Interchange with Session GUID '{0}' could not be found", statusMessage.Object.TrackingID));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			var queryForStatusIncoming = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, statusMessage.Object.TrackingID);
			queryForStatusIncoming.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			var statusInterchange = Factory.LoadTop1<EDIInterchange>(queryForStatusIncoming);
			AssertNull("incoming status interchange should not be found", statusInterchange);
		}

		[UseSnapshotProtection]
		public void TestStatusMessage_SaveStatusMessageToStatusInterchange_IsLocked()
		{
			AssertStatusMessage_SaveStatusMessageToStatusInterchange_IsLocked(EDIInterchangeTypeList.Descriptions.MSS, EDIInterchange.Status.Sent);
			AssertStatusMessage_SaveStatusMessageToStatusInterchange_IsLocked(EDIInterchangeTypeList.Descriptions.MSF, EDIInterchange.Status.Failed);
			AssertStatusMessage_SaveStatusMessageToStatusInterchange_IsLocked(EDIInterchangeTypeList.Descriptions.MessageStatusAcknowledgment, EDIInterchange.Status.Acknowledged);
		}

		static IeHubMessage CreateStatusMessage(Guid trackingId, string schemaName)
		{
			var statusMessage = new Mock<IeHubMessage>();
			statusMessage.Setup(m => m.TrackingID).Returns(trackingId);
			statusMessage.Setup(m => m.SenderID).Returns("eHub");
			statusMessage.Setup(m => m.RecipientID).Returns("ENT1");
			statusMessage.Setup(m => m.SchemaName).Returns(schemaName);
			statusMessage.Setup(m => m.MessageStream).Returns(new VirtualMemoryStream());
			statusMessage.Setup(m => m.ApplicationCode).Returns(string.Empty);
			return statusMessage.Object;
		}

		void AssertNoStatusInterchange(EDIInterchange outgoingInterchange)
		{
			var queryForStatusIncoming = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, outgoingInterchange.EI_SessionGUID);
			queryForStatusIncoming.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			var incomingStatusInterchange = Factory.LoadTop1<EDIInterchange>(queryForStatusIncoming);
			AssertNull(incomingStatusInterchange);
		}

		void AssertStatusMessage_SaveStatusMessageToStatusInterchange_IsLocked(string schemaName, string expectedOutboundStatus)
		{
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, "ENT1", "ENT1", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchange.Status.eHubPending;
			Factory.Save();

			var testInbox = new TestMessageInbox();
			testInbox.AddMessage(CreateStatusMessage(outgoingInterchange.EI_SessionGUID.ToGuid(), schemaName));

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(x => x.RetrieveMessages());
			mockAdapter.Setup(x => x.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(testInbox);

			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			var messageHandler = HandlerFactory.GetHandlerForTest(schemaName, TimeSpan.FromSeconds(1));
			mockServiceTaskJob.Setup(m => m.GetMessageHandler(It.IsAny<string>())).Returns(messageHandler);

			using (var mutexConnection = Db.NewExtraConnectionToMainDb())
			{
				mutexConnection.BeginTransaction();
				Assert("PRE: A lock is acquired", mutexConnection.TryGetLock(MutexConstants.MessageMutexPrefix + outgoingInterchange.EI_SessionGUID, out var mutex));

				try
				{
					ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI");
					Fail("No exception Thrown when interchange is locked");
				}
				catch (TimeoutException ex)
				{
					AssertEquals("Wrong exception message", $"MessageStatusHandler failed to acquire interchange lock for TrackingID = '{outgoingInterchange.EI_SessionGUID.ToGuid()}'", ex.Message);
				}
				finally
				{
					mutex.Dispose();
					mutexConnection.RollbackTransaction();
				}
			}

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals(EDIInterchange.Status.eHubPending, outgoingInterchange.EI_Status);
			AssertEquals("The inbox should not be marked as read as the message could not be processed", false, testInbox.MarkedAsRead);
			AssertNoStatusInterchange(outgoingInterchange);

			ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals(expectedOutboundStatus, outgoingInterchange.EI_Status);
			AssertNoStatusInterchange(outgoingInterchange);
			AssertEquals("The inbox should be marked as read as the message was processed", true, testInbox.MarkedAsRead);
		}

		[UseSnapshotProtection]
		public void TestStatusMessage_SaveStatusMessageToStatusInterchange_NotLocked()
		{
			AssertStatusMessage_SaveStatusMessageToStatusInterchange_NotLocked(EDIInterchangeTypeList.Descriptions.MSS, EDIInterchange.Status.Sent);
			AssertStatusMessage_SaveStatusMessageToStatusInterchange_NotLocked(EDIInterchangeTypeList.Descriptions.MSF, EDIInterchange.Status.Failed);
			AssertStatusMessage_SaveStatusMessageToStatusInterchange_NotLocked(EDIInterchangeTypeList.Descriptions.MessageStatusAcknowledgment, EDIInterchange.Status.Acknowledged);
		}

		void AssertStatusMessage_SaveStatusMessageToStatusInterchange_NotLocked(string schemaName, string expectedOutboundStatus)
		{
			var outgoingInterchange = TestInterchangeMessage.GetNew(CurrentBranchPk, "ENT1", "ENT1", EDIInterchange.Direction.Transmit, Factory, true);
			outgoingInterchange.EI_Status = EDIInterchange.Status.eHubPending;
			Factory.Save();

			var testInbox = new TestMessageInbox();
			testInbox.AddMessage(CreateStatusMessage(outgoingInterchange.EI_SessionGUID.ToGuid(), schemaName));

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(x => x.RetrieveMessages());
			mockAdapter.Setup(x => x.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(testInbox);

			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI");
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			Factory.ReloadAll<EDIInterchange>();
			AssertEquals(expectedOutboundStatus, outgoingInterchange.EI_Status);
			AssertNoStatusInterchange(outgoingInterchange);
			AssertEquals("The inbox should be marked as read as the message was processed", true, testInbox.MarkedAsRead);
		}

		public void TestReceiveInterchangeZSaveException()
		{
			var mockInbox = new Mock<IMessageInbox>(MockBehavior.Strict);
			mockInbox.Setup(m => m.Count).Returns(1);
			var interchanges = new List<IeHubMessage>();
			const string messageBody = "<InboundMessage><Header><![CDATA[A             041114                                                            ]]></Header><Body><![CDATA[B064101D99RR                                                                    R14101D99 532273690195-268096500400144716MSCUMSC CHARLESTON      FG410041514    R4            ED685946    SHA400144716            00000102CT   MSCUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 R14101D99 532279970195-268096500400175690EGLVEVER DECENT         07230041514    R4            149400520639ZNV14030346             00000615PCE  EGLVJASF         R5041114001405PAPERLESS                                                         R5041114001422RELEASE DATE UPDATE                     04151402                  R6FDA    041114001401FDA REVIEW                                                 R14101D99 532304700195-268096500400170831CMDUXIN TAI CANG        0222E041514    R4            XMPC381045  XMN400170831            00001004CT   CMDUJASF         R5041114001505PAPERLESS                                                         R5041114001522RELEASE DATE UPDATE                     04151402                  R6FDA    041114001501FDA REVIEW                                                 Y064101D99RR00015                                                               ]]></Body><Footer><![CDATA[Z             041114                                                            ]]></Footer></InboundMessage>";
			var trackingId = Guid.NewGuid();
			interchanges.Add(new eHubMessage(trackingId, "SENDER", "RECIPIENT", MessageSchemaType.Xml, EDIInterchangeTypeList.Descriptions.USCustomsImport, "USCustoms Import", new MemoryStream(Encoding.ASCII.GetBytes(messageBody))));
			mockInbox.Setup(m => m.GetEnumerator()).Returns(interchanges.GetEnumerator());

			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages());
			mockAdapter.Setup(m => m.Dispose());
			mockAdapter.Setup(m => m.Inbox).Returns(mockInbox.Object);

			var mockMessageHandler = new Mock<USCustomsImportMessageHandler>() { CallBase = true };
			mockMessageHandler.Setup(m => m.SaveFactory()).Throws(new ZSaveException(new ZDataException(GetSqlTimeoutException(), null, null), null));//.Repeat.Times(4);

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			mockServiceTaskJob.Setup(m => m.GetMessageHandler(It.IsAny<string>())).Returns(mockMessageHandler.Object);
			AssertExceptionThrown<ZSaveException>(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			mockServiceTaskJob.Object.Notifier.AssertNotificationDoesNotExists("Retrieved 1 eHub Messages for company EDI. Total size(KB): 1.");
			var results = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingId));
			AssertEquals("Interchange should not have been created", 0, results.Length);

			mockInbox.Verify(m => m.MarkAsRead(), Times.Never()); // this is the critical check. MarkAsRead will not occur if there is an exception while saving.
			mockInbox.VerifyAll();
			mockAdapter.VerifyAll();
			mockAdapter.Verify(m => m.Inbox, Times.Exactly(2));
			mockServiceTaskJob.VerifyAll();
		}

		public void TestAdapterRetrieveMessagesExceptionRethrown()
		{
			TestAdapterRetrieveMessagesExceptionRethrown(new CommunicationException("timeout", new WebException("The underlying connection was closed")));
			TestAdapterRetrieveMessagesExceptionRethrown(new CommunicationException("The socket connection was aborted", new IOException("Unable to write data to the transport connection", new SocketException(10054))));
			TestAdapterRetrieveMessagesExceptionRethrown(new CommunicationException("The socket connection was aborted", new IOException("Unable to read data from the transport connection: The connection was closed.")));
			TestAdapterRetrieveMessagesExceptionRethrown(new CommunicationException("The socket connection was aborted", new SocketException(10054)));
			TestAdapterRetrieveMessagesExceptionRethrown(new EndpointNotFoundException("There was no endpoint listening at https://ehub-ausyd.cargowise.net/eHubGateway/eHubStreamedService.svc that could accept the message. This is often caused by an incorrect address or SOAP action. See InnerException, if present, for more details.", new WebException()));
			TestAdapterRetrieveMessagesExceptionRethrown(new ProtocolException("The content type text/html of the response message does not match the content type of the binding (text/xml; charset=utf-8)", new WebException("(413) Request Entity Too Large")));
			TestAdapterRetrieveMessagesExceptionRethrown(new ProtocolViolationException("Chunked encoding upload is not supported on the HTTP/1.0 protocol."));
			TestAdapterRetrieveMessagesExceptionRethrown(new ServerTooBusyException("The HTTP service located at https://ehub-ausyd.cargowise.net/eHubGateway/eHubStreamedService.svc is unavailable. This could be because the service is too busy or because no endpoint was found listening at the specified address. Please ensure that the address is correct and try accessing the service again later.", new WebException()));
			TestAdapterRetrieveMessagesExceptionRethrown(new TimeoutException());
			TestAdapterRetrieveMessagesExceptionRethrown(new eHubAdapterException("some error on eHub"));
		}

		public void TestCompanyShouldBeServiced()
		{
			var ehi = new InboundServiceTaskJobForTest(eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockServiceTaskSupport("EHI").Object, new NotificationBuffer());
			var company = Factory.NewWithValidTestData<GlbCompany>();
			AssertEquals(false, ehi.CompanyShouldBeServiced(company));
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			AssertEquals(true, ehi.CompanyShouldBeServiced(company));
		}

		[UseSnapshotProtection]
		public void TestMessagesAreDownloadedOncePerCompanyAcrossMultipleJobs()
		{
			var totalCompanies = 15;
			var companiesToExecuteBeforePause = 10;
			var currentCompanies = new eHubMessagingCompanySettingsManager().Companies.Length;
			var companies = new List<GlbCompany>();

			Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					for (int i = 0; i < totalCompanies - currentCompanies; i++)
					{
						companies.Add(CreateCompanyWithBranch(i.ToString(), factory));
					}
					factory.Save();
				}
			}).Wait();

			var secondJobCompleted = new AutoResetEvent(false);
			var pauseOfFirstJob = new AutoResetEvent(false);
			var readCount = 0;
			using (secondJobCompleted)
			using (pauseOfFirstJob)
			{
				var mockAdapter = new EHubAdapterMock();
				mockAdapter.OnRetrieve += (o, e) =>
				{
					var currentCount = Interlocked.Increment(ref readCount);
					if (currentCount == companiesToExecuteBeforePause)
					{
						pauseOfFirstJob.Set();
						Assert("secondJobCompleted event was not signalled in time", secondJobCompleted.WaitOne(TimeSpan.FromMinutes(1)));
					}
				};

				var mockAdapterFactory = new Mock<IAdaptorFactory>();
				mockAdapterFactory.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<INotifications>(), It.IsAny<string>())).Returns(mockAdapter);//.Repeat.AtLeastOnce()

				var job1 = CreateMockJob_Moq(mockAdapterFactory.Object, new eHubMessagingCompanySettingsManager(), company: null, mockInterchangeCandidates: false);
				var job1Task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						ExecuteJobWithServiceTaskContext(job1.Object, "EHI");
					}
				});

				var job2 = CreateMockJob_Moq(mockAdapterFactory.Object, new eHubMessagingCompanySettingsManager(), company: null, mockInterchangeCandidates: false);
				var job2Task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						Assert("PauseOfFirstJob event was not signalled in time", pauseOfFirstJob.WaitOne(TimeSpan.FromMinutes(1)));
						ExecuteJobWithServiceTaskContext(job2.Object, "EHI");
						secondJobCompleted.Set();
					}
				});

				Task.WaitAll(job1Task, job2Task);

				AssertEquals("Wrong number of companies read", totalCompanies + (totalCompanies - companiesToExecuteBeforePause), readCount);
			}
		}

		[UseSnapshotProtection]
		public void TestLockLostWithMultipleCompanies()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var companies = new List<GlbCompany>();
				for (int i = 0; i < 2; i++)
				{
					companies.Add(CreateCompanyWithBranch(i.ToString()));
				}
				var mockSettings = new Mock<ICompanySettings>(MockBehavior.Strict);
				var mockSettingsManager = new Mock<ICompanySettingsManager>(MockBehavior.Strict);
				mockSettingsManager.Setup(m => m.Companies).Returns(companies.ToArray());
				mockSettingsManager.Setup(m => m.GetSetting(It.IsAny<GlbCompany>())).Returns(mockSettings.Object);
				var mockServiceTaskJob = CreateMockJob_Moq(new Mock<IAdaptorFactory>(MockBehavior.Strict).Object, mockSettingsManager.Object, company: null, mockInterchangeCandidates: false);
				mockServiceTaskJob.Setup(m => m.DbConnection).Returns(extraConnection);
				mockServiceTaskJob.Setup(x => x.ProcessMessagesCore())
					.Callback(() =>
					{
						extraConnection.CloseConnection();
						new BusinessObjectFactory(extraConnection).LoadTop1<DummyBusinessObject>(new ZQuery());
					});

				AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
				mockServiceTaskJob.Object.Notifier.AssertNotificationContains("Exclusive lock for company was lost.");
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestAllExceptionsReportedWhenAdapterDisposeThrows()
		{
			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages()).Throws(new InvalidOperationException("Retrieve"));
			mockAdapter.Setup(m => m.Dispose()).Throws(new InvalidOperationException("Dispose"));

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));

			var exceptionThrown = AssertExceptionThrown<AggregateException>(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			AssertEquals("The aggregate exception should contain two exceptions", 2, exceptionThrown.InnerExceptions.Count);
			Assert("Contains retrieve exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Retrieve") != null);
			Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "Dispose") != null);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		[UseSnapshotProtection]
		public void TestAllExceptionsReportedWhenCompanyLockReleaseThrows()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var companies = new List<GlbCompany>();
				for (int i = 0; i < 2; i++)
				{
					companies.Add(CreateCompanyWithBranch(i.ToString()));
				}
				var mockSettings = new Mock<ICompanySettings>(MockBehavior.Strict);
				var mockSettingsManager = new Mock<ICompanySettingsManager>(MockBehavior.Strict);
				mockSettingsManager.Setup(m => m.Companies).Returns(companies.ToArray());
				mockSettingsManager.Setup(m => m.GetSetting(It.IsAny<GlbCompany>())).Returns(mockSettings.Object);
				var mockServiceTaskJob = CreateMockJob_Moq(new Mock<IAdaptorFactory>(MockBehavior.Strict).Object, mockSettingsManager.Object, company: null, mockInterchangeCandidates: false);
				mockServiceTaskJob.Setup(m => m.DbConnection).Returns(extraConnection);
				mockServiceTaskJob.Setup(x => x.ProcessMessagesCore())
					.Callback(() =>
					{
						var cmd = extraConnection.Command("SELECT 1");
						var reader = cmd.ExecuteReader();
						throw new InvalidOperationException("ProcessMessagesCore");
					});

				var exceptionThrown = AssertExceptionThrown<AggregateException>(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
				AssertEquals("The aggregate exception should contain two exceptions", 2, exceptionThrown.InnerExceptions.Count);
				Assert("Contains retrieve exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is InvalidOperationException && e.Message == "ProcessMessagesCore") != null);
				Assert("Contains dispose exception", exceptionThrown.InnerExceptions.FirstOrDefault(e => e is CommunicationException && e.Message == "Unable to execute command on the database") != null);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		protected override Mock<InboundServiceTaskJob> CreateMockJob_Moq(ICompanySettingsManager companySettingsManager, GlbCompany company, INotifications notifications = null, bool mockInterchangeCandidates = true)
		{
			var actualCompanySettingsManager = companySettingsManager ?? eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockCompanySettingsManager(new[] { company }).Object;
			var mockServiceTaskJob = new Mock<InboundServiceTaskJob>(CreateMockServiceTaskSupport(ServiceTaskName, actualCompanySettingsManager), notifications ?? new NotificationBuffer(), new AdaptorFactoryMockWithOneAdaptor(new EHubAdapterMock()));
			mockServiceTaskJob.CallBase = true;
			AdditionalServiceTaskJobSetup_Moq(mockServiceTaskJob, company, mockInterchangeCandidates);
			return mockServiceTaskJob;
		}

		class InboundServiceTaskJobForTest : InboundServiceTaskJob
		{
			public InboundServiceTaskJobForTest(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier) : base(serviceTaskSupport, notifier, new AdaptorFactoryMockWithOneAdaptor(new EHubAdapterMock())) { }

			public new bool CompanyShouldBeServiced(GlbCompany company)
			{
				return base.CompanyShouldBeServiced(company);
			}
		}

		void TestAdapterRetrieveMessagesExceptionRethrown<TException>(TException exception)
			where TException : Exception
		{
			var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			mockAdapter.Setup(m => m.RetrieveMessages()).Throws(exception);
			mockAdapter.Setup(x => x.Dispose());

			var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object));
			var exceptionThrown = AssertExceptionThrown<TException>(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHI"));
			AssertEquals(exception, exceptionThrown);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		protected override eHubMessaging.ServiceTasks.AdapterType ServiceAdapterType => eHubMessaging.ServiceTasks.AdapterType.GatewayAdapter;

		static SqlException GetSqlTimeoutException()
		{
			var error = SqlExceptionBuilder.CreateSqlError(-2, 2, 3, "server name", "error message", "proc", 100);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			return SqlExceptionBuilder.CreateSqlException(errorCollection);
		}

		static ZGuid CurrentBranchPk => GlbCompany.CurrentCompany.Branches[0].PK;

		class TestMessageInbox : IMessageInbox
		{
			readonly List<IeHubMessage> messages = new List<IeHubMessage>();

			public bool MarkedAsRead { get; set; }

			public IEnumerator<IeHubMessage> GetEnumerator()
			{
				return messages.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Clear()
			{
				messages.Clear();
			}

			public long SizeInKiloBytes
			{
				get { return Count; }
			}

			public int Count
			{
				get { return messages.Count; }
			}

			public void AddMessage(IeHubMessage message)
			{
				messages.Add(message);
			}

			public bool CanRetrieveMessages
			{
				get
				{
					return true;
				}
			}

			public void ReadMessageBatch(Guid batchID, eHubGatewayMessage[] messages)
			{
			}

			public void MarkAsRead()
			{
				MarkedAsRead = true;
				Clear();
			}
		}
	}
}
