using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder;
using Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks;
using Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.eHubOutbound
{
	[TestedType(typeof(eHubOutboundServiceTaskJob))]
	class eHubOutboundServiceTaskJobTests : OutboundEDIInterchangesServiceTaskJobTests<eHubOutboundServiceTaskJob>
	{
		public void TestAdapterOutboxLimits()
		{
			var rule = new OutboundSendLimitsRule()
			{
				SendCountLimit = 9,
				SendSizeLimit = 999999
			};
			eHubMessagingRegistry.Instance.eHubOutboundSendLimitsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertEquals(9, serviceTaskJob.Object.AdapterOutboxCountLimit);
			AssertEquals(999999 * 1024, serviceTaskJob.Object.AdapterOutboxSizeLimitInBytes);
		}

		[TestDate]
		public void TestGetCandidatePK()
		{
			TestDateAttribute.Date = DateTime.Now;

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			AssertEquals("Precondition: EDIMessage table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals("Precondition: EDIInterchange table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			const string recipient = "BLAHBLAHB";
			Assert("Precondition: Current company should have at least 2 branches", CurrentCompany.Branches.Count >= 2);
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			var eHubInterchanges = CreateTestInterchanges(recipient, interchangeNumberStrategy, messageNumberStrategy, EDIInterchange.Status.eHubQueued, EDIInterchange.Status.eHubPending, EDIInterchange.Status.Failed);
			CreateTestInterchanges(recipient, interchangeNumberStrategy, messageNumberStrategy, EDIInterchange.Status.eAdaptorQueued, EDIInterchange.Status.Sent, EDIInterchange.Status.Failed);
			AssertEquals("Precondition: 20 Interchange should be generated for testing", 20, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Object.CurrentRecipient = recipient;

			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.Branches[0].PK;
			var interchangeCandidates = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);
			AssertContainsExactElementsInAnyOrder(new[] { eHubInterchanges[0].PK, eHubInterchanges[4].PK }, interchangeCandidates.Select(c => new ZGuid(c.PK)));

			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.Branches[1].PK;
			interchangeCandidates = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);
			AssertContainsExactElementsInAnyOrder(new[] { eHubInterchanges[2].PK, eHubInterchanges[6].PK }, interchangeCandidates.Select(c => new ZGuid(c.PK)));
		}

		public void TestInterchangeFailsWithNoEDIMessage()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_IsActive = true;
			interchange.EI_ApplicationCode = "XMS";
			interchange.EI_InterchangeType = "XMS";
			interchange.EI_ReceiveTransmit = "TRX";
			interchange.EI_Status = "HQU";
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_From = "AGSWORAGS";
			interchange.EI_To = "AGSWORAGS_WWA";
			interchange.EI_InterchangeNum = "339641";
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_HeaderText = "<EDIDelivery><FileName>STATUS_S01491578_201312041056496030.gz</FileName><EmailSubject></EmailSubject></EDIDelivery>";
			interchange.EI_BodyText = XmlInterchangeHandlerTests.GetFileResourceString("Interchange.xml");

			Factory.Save();

			var mockServiceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHO"));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			mockServiceTaskJob.Object.Notifier.AssertNotificationContains("should contain at least one message, but it contains no messages");
			Factory.ReloadAll<EDIInterchange>();
			var reloadedInterchange = Factory.LoadTop1<EDIInterchange>(new CargoWise.EntityFramework.ZQuery(EDIInterchangeSchema.PK, interchange.PK));
			AssertEquals("Interchange should fail with no message", EDIMessage.Status.Failed, reloadedInterchange.EI_Status);
		}

		public void TestInterchangeFailsWithNoEDIMessageBatchedWithOtherFailingMessageBefore()
		{
			AssertInterchangeFailsWithNoEDIMessageBatchedWithOtherFailingMessage(failingMessageAfter: false);
		}

		public void TestInterchangeFailsWithNoEDIMessageBatchedWithOtherFailingMessageAfter()
		{
			AssertInterchangeFailsWithNoEDIMessageBatchedWithOtherFailingMessage(failingMessageAfter: true);
		}

		void AssertInterchangeFailsWithNoEDIMessageBatchedWithOtherFailingMessage(bool failingMessageAfter)
		{
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_IsActive = true;
			interchange1.EI_ApplicationCode = "XMS";
			interchange1.EI_InterchangeType = "XMS";
			interchange1.EI_ReceiveTransmit = "TRX";
			interchange1.EI_Status = "HQU";
			interchange1.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange1.EI_From = CurrentCompany.LicenceKeyIdentifier;
			interchange1.EI_GB = CurrentCompany.FirstActiveBranch.PK;
			interchange1.EI_To = "AGSWORAGS_WWA";
			interchange1.EI_InterchangeNum = "339641";
			interchange1.EI_SessionGUID = interchange1.PK;
			interchange1.EI_HeaderText = "<EDIDelivery><FileName>STATUS_S01491578_201312041056496030.gz</FileName><EmailSubject></EmailSubject></EDIDelivery>";
			interchange1.EI_BodyText = XmlInterchangeHandlerTests.GetFileResourceString("Interchange.xml");
			interchange1.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(failingMessageAfter ? -1 : 1);

			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			var interchange2 = CreateTestInterchange("AGSWORAGS_WWA", interchangeNumberStrategy, messageNumberStrategy);
			interchange2.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange2.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange2.EI_BodyText = "Test ABL Message";
			interchange2.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;

			Factory.Save();

			var exception = new MessageSecurityException("An unsecured or incorrectly secured fault was received from the other party. See the inner FaultException for the fault code and detail.", new FaultException("An error occurred when verifying security for the message."));
			var mockServiceTaskJob = CreateMockJob_Moq(new AdaptorFactoryMockWithOneAdaptor(new AdaptorMockThatThrowsOnSend(exception)), mockInterchangeCandidates: false);
			AssertNoExceptionThrown(() => ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHO"));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			mockServiceTaskJob.Object.Notifier.AssertNotificationContains("should contain at least one message, but it contains no messages");
			Factory.ReloadAll<EDIInterchange>();
			var reloadedInterchange = Factory.LoadTop1<EDIInterchange>(new CargoWise.EntityFramework.ZQuery(EDIInterchangeSchema.PK, interchange1.PK));
			AssertEquals("Interchange should fail with no message", EDIMessage.Status.Failed, reloadedInterchange.EI_Status);
		}

		public void TestCreateeHubMessage_AllSupportedMessageTypes()
		{
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();

			var interchange1 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange1.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange1.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange1.EI_BodyText = "Test ABL Message";
			interchange1.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;

			var interchange2 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange2.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange2.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange2.EI_BodyText = "Test CON Message";
			interchange2.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;

			var interchange3 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange3.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange3.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange3.EI_BodyText = "Test CMV Message";
			interchange3.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.ContainerMovements;

			var interchange4 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange4.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange4.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange4.EI_BodyText = "Test EVT Message";
			interchange4.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Events;

			var interchange5 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange5.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange5.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange5.EI_BodyText = "Test FTR Message";
			interchange5.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.FinancialTransactions;

			var interchange6 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange6.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange6.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange6.EI_BodyText = "Test ORD Message";
			interchange6.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Orders;

			var interchange7 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange7.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange7.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange7.EI_BodyText = "Test PRD Message";
			interchange7.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Products;

			var interchange8 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange8.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange8.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange8.EI_BodyText = "Test SHP Message";
			interchange8.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Shipments;

			var interchange9 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange9.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange9.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange9.EI_BodyText = "Test WHD Message";
			interchange9.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.WhsDockets;

			var interchange15 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange15.EI_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			interchange15.EI_HeaderText = "<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange15.EI_BodyText = "Test NDM Message";
			interchange15.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlNativeOrganization;

			var interchange10 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange10.EI_ApplicationCode = ApplicationCodeList.Codes.CIM;
			interchange10.EI_BodyText = "Test FHL Message";
			interchange10.ContainedMessages[0].EM_MessageType = EDIMessageTypeList.Codes.FHL;

			var interchange11 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange11.EI_ApplicationCode = ApplicationCodeList.Codes.CIM;
			interchange11.EI_BodyText = "Test FWB Message";
			interchange11.ContainedMessages[0].EM_MessageType = EDIMessageTypeList.Codes.FWB;

			var interchange12 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange12.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsImport;
			interchange12.EI_HeaderText = "Header";
			interchange12.EI_BodyText = "Test USI Message";
			interchange12.EI_FooterText = "Footer";
			interchange12.ContainedMessages[0].EM_MessageType = "AA";

			var interchange13 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange13.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsExport;
			interchange13.EI_HeaderText = "Header";
			interchange13.EI_BodyText = "Test USE Message";
			interchange13.EI_FooterText = "Footer";
			interchange13.ContainedMessages[0].EM_MessageType = "BB";

			var interchange14 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange14.EI_ApplicationCode = ApplicationCodeList.Codes.USAMS;
			interchange14.EI_HeaderText = "Header";
			interchange14.EI_BodyText = "Test AMS Message";
			interchange14.EI_FooterText = "Footer";
			interchange14.ContainedMessages[0].EM_MessageType = "CC";

			var interchange16 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange16.EI_ApplicationCode = ApplicationCodeList.Codes.Inttra;
			interchange16.EI_BodyText = "Test Inttra Message";

			var interchange17 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange17.EI_ApplicationCode = ApplicationCodeList.Codes.ShippingLineEHubMessaging;
			interchange17.EI_BodyText = "Test ShippingLineEHubMessaging Message";

			var interchange18 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange18.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange18.EI_HeaderText = @"<EDIDelivery><FileName>BLAH.txt</FileName><EmailSubject>Blah@BLah.com</EmailSubject></EDIDelivery>";
			interchange18.EI_BodyText = "Test ORG Message";
			interchange18.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.Organizations;

			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange1), interchange1, EDIMessageSchemaNameList.Descriptions.AgencyBillsOfLading,
				MessageSchemaType.Xml, "Test ABL Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange2), interchange2, EDIMessageSchemaNameList.Descriptions.Consols,
				MessageSchemaType.Xml, "Test CON Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange3), interchange3, EDIMessageSchemaNameList.Descriptions.ContainerMovements,
				MessageSchemaType.Xml, "Test CMV Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange4), interchange4, EDIMessageSchemaNameList.Descriptions.Events,
				MessageSchemaType.Xml, "Test EVT Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange5), interchange5, EDIMessageSchemaNameList.Descriptions.FinancialTransactions,
				MessageSchemaType.Xml, "Test FTR Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange6), interchange6, EDIMessageSchemaNameList.Descriptions.Orders,
				MessageSchemaType.Xml, "Test ORD Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange7), interchange7, EDIMessageSchemaNameList.Descriptions.Products,
				MessageSchemaType.Xml, "Test PRD Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange8), interchange8, EDIMessageSchemaNameList.Descriptions.Shipments,
				MessageSchemaType.Xml, "Test SHP Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange9), interchange9, EDIMessageSchemaNameList.Descriptions.WhsDockets,
				MessageSchemaType.Xml, "Test WHD Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange15), interchange15, EDIMessageSchemaNameList.Descriptions.NativeDataMessaging,
			MessageSchemaType.Xml, "Test NDM Message", "Blah@BLah.com", "BLAH.txt");

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange10), interchange10, EDIMessageSchemaNameList.Descriptions.FHL,
				MessageSchemaType.FlatFile, "Test FHL Message", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange11), interchange11, EDIMessageSchemaNameList.Descriptions.FWB,
				MessageSchemaType.FlatFile, "Test FWB Message", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange12), interchange12, "AA",
				MessageSchemaType.FlatFile, @"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Test USI Message]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange13), interchange13, "BB",
				MessageSchemaType.FlatFile, @"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Test USE Message]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange14), interchange14, "CC",
				MessageSchemaType.FlatFile, @"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Test AMS Message]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange16), interchange16, EDIMessageSchemaNameList.Descriptions.InttraEdifact,
				MessageSchemaType.FlatFile, "Test Inttra Message", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange17), interchange17, EDIMessageSchemaNameList.Descriptions.InttraEdifact,
				MessageSchemaType.FlatFile, "Test ShippingLineEHubMessaging Message", string.Empty, string.Empty);

			AssertEHubMessage(serviceTaskJob.Object.CreateeHubMessage(interchange18), interchange18, EDIMessageSchemaNameList.Descriptions.Organizations,
				MessageSchemaType.Xml, "Test ORG Message", "Blah@BLah.com", "BLAH.txt");
		}

		public void TestCreateeHubMessage_Errors()
		{
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();

			var interchange1 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange1.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;

			var interchange2 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange2.EI_ApplicationCode = ApplicationCodeList.Codes.CIM;
			interchange2.ContainedMessages[0].EM_MessageType = EDIMessageTypeList.Codes.XMS;

			var interchange3 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange3.EI_ApplicationCode = ApplicationCodeList.Codes.eNett;

			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);

			var serviceName = interchange1.TransportModeDescription;
			AssertNull(serviceTaskJob.Object.CreateeHubMessage(interchange1));
			var notes = interchange1.Notes.FindByDescription("eHub Error");
			AssertEquals(1, notes.Length);
			AssertEquals("Cannot create eHub message: schema name cannot be found.", notes[0].ST_NoteDataAsText);
			serviceTaskJob.Object.Notifier.AssertNotificationExists("Warning: Cannot create eHub message: schema name cannot be found.");
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			serviceName = interchange2.TransportModeDescription;
			AssertNull(serviceTaskJob.Object.CreateeHubMessage(interchange2));
			notes = interchange2.Notes.FindByDescription("eHub Error");
			AssertEquals(1, notes.Length);
			AssertEquals("Cannot create eHub message: schema name cannot be found.", notes[0].ST_NoteDataAsText);
			serviceTaskJob.Object.Notifier.AssertNotificationExists("Warning: Cannot create eHub message: schema name cannot be found.", 2);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			serviceName = interchange3.TransportModeDescription;
			AssertNull(serviceTaskJob.Object.CreateeHubMessage(interchange3));
			notes = interchange3.Notes.FindByDescription("eHub Error");
			AssertEquals(1, notes.Length);
			AssertEquals("EDI Interchange Application Code 'ENE' is not supported.", notes[0].ST_NoteDataAsText);
			serviceTaskJob.Object.Notifier.AssertNotificationExists("Warning: EDI Interchange Application Code 'ENE' is not supported.");
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestCreateeHubMessage_CartageJobs()
		{
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();

			var interchange1 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange1.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange1.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.LocalCartageBooking;

			var interchange2 = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
			interchange2.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange2.ContainedMessages[0].EM_MessageSubType = EDIMessageSubTypeList.Codes.LocalCartageStatus;

			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
			Factory.ReloadAll<EDIInterchange>();
			Factory.ReloadAll<EDIMessage>();
			serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("2 interchange(s) sent to {0}.", ServiceTaskName));
			AssertEquals(EDIInterchange.Status.eHubPending, interchange1.EI_Status);
			AssertEquals(EDIMessage.Status.Queued, interchange1.ContainedMessages[0].EM_Status);
			AssertEquals(EDIMessage.Status.Queued, interchange1.ContainedMessages[1].EM_Status);
			AssertEquals(EDIInterchange.Status.eHubPending, interchange2.EI_Status);
			AssertEquals(EDIMessage.Status.Queued, interchange2.ContainedMessages[0].EM_Status);
			AssertEquals(EDIMessage.Status.Queued, interchange2.ContainedMessages[1].EM_Status);
		}

		public void TestProcessSystemInterchanges()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			AssertEquals("PRE: EDIMessage table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals("PRE: EDIInterchange table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			Assert("PRE: Current company should have at least 2 branches", CurrentCompany.Branches.Count >= 2);

			var interchanges = new EDIInterchange[3];
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();

			interchanges[0] = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, true);
			interchanges[0].EI_InterchangeType = EDIInterchangeTypeList.Codes.SYS;
			SetInterchangeProperties(interchanges[0], CurrentCompany.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);

			Factory.Save();

			Thread.Sleep(10); // Ensure that sequential interchanges do not have identical create times

			interchanges[1] = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(interchanges[1], CurrentCompany.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);
			Factory.Save();

			Thread.Sleep(10); // Ensure that sequential interchanges do not have identical create times

			interchanges[2] = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, true);
			interchanges[2].EI_InterchangeType = EDIInterchangeTypeList.Codes.SYS;
			SetInterchangeProperties(interchanges[2], CurrentCompany.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);

			Factory.Save();

			AssertEquals("PRE: EDIInterchange table should have 3 interchanges.", 3, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);

			serviceTaskJob.Object.ProcessSystemInterchanges = true;
			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
			serviceTaskJob.Object.CurrentRecipient = "BLAHBLAHB";
			var interchangeCandidates1 = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);
			var candidatePK1 = new List<Guid>(interchangeCandidates1.Select(item => item.PK));
			CombineAssertions(() =>
			{
				AssertEquals("PRE: CandidatePK equals 2", 2, candidatePK1.Count);
				AssertEquals("1st candidate should be the 1st system type interchange.", interchanges[0].PK, candidatePK1[0]);
				AssertEquals("2nd candidate should be the 2nd system type interchange.", interchanges[2].PK, candidatePK1[1]);
			});

			serviceTaskJob.Object.ProcessSystemInterchanges = false;
			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
			serviceTaskJob.Object.CurrentRecipient = "BLAHBLAHB";
			var interchangeCandidates2 = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);
			var candidatePK2 = new List<Guid>(interchangeCandidates2.Select(item => item.PK));
			CombineAssertions(() =>
			{
				AssertEquals("PRE: CandidatePK equals 1", 1, candidatePK2.Count);
				AssertEquals("1st candidate should be another type interchange.", interchanges[1].PK, candidatePK2[0]);
			});
		}

		public void TestCreateeHubMessage_MessageOrder()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			AssertEquals("PRE: EDIMessage table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));
			AssertEquals("PRE: EDIInterchange table shoulb be empty.", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			Assert("PRE: Current company should have at least 2 branches", CurrentCompany.Branches.Count >= 2);

			var now = ZDateTime.UtcNow;
			var interchanges = new EDIInterchange[3];
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();

			interchanges[1] = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(interchanges[1], CurrentCompany.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);
			interchanges[1].EI_SystemCreateTimeUtc = now.AddSeconds(-10);
			Factory.Save();

			interchanges[0] = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(interchanges[0], CurrentCompany.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);
			interchanges[0].EI_SystemCreateTimeUtc = now.AddSeconds(-15);
			Factory.Save();

			interchanges[2] = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, true);
			SetInterchangeProperties(interchanges[2], CurrentCompany.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, EDIInterchange.Status.eHubQueued);
			interchanges[2].EI_SystemCreateTimeUtc = now.AddSeconds(-5);
			Factory.Save();

			AssertEquals("PRE: EDIInterchange table shoulb have 3 interchanges.", 3, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
			serviceTaskJob.Object.CurrentRecipient = "BLAHBLAHB";
			var interchangeCandidates = serviceTaskJob.Object.GetPendingItems(serviceTaskJob.Object.PendingItemsBatchSize);

			CombineAssertions(() =>
			{
				AssertEquals("Candidates count equals 3", 3, interchangeCandidates.Count);
				for (int i = 0; i < interchangeCandidates.Count; i++)
				{
					AssertEquals("CHK: The candidate #" + i + " should be the same as interchage #" + i, interchanges[i].PK, interchangeCandidates.ElementAt(i).PK);
				}
			});
		}

		public void TestProcessAllMessageTypesNoExceptionsNoLeaks()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);

			const string recipient = "Test";
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			var codeList = new ApplicationCodeList.Codes();
			var fields = codeList.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);

			// Iterate over all possible application message types to ensure all are behaving
			foreach (var field in fields)
			{
				var applicationCode = (string)field.GetValue(codeList);
				var interchange = CreateTestInterchange(recipient, interchangeNumberStrategy, messageNumberStrategy);
				interchange.EI_ApplicationCode = applicationCode;
				interchange.EI_SessionGUID = new ZGuid(dummySessionID);

				var notifier = new NotificationBuffer();
				var messageDirector = new EHubMessageDirector(interchange, notifier);
				var messageBuilder = messageDirector.CreateBuilder();
				if (messageBuilder == null)
				{
					interchange.Delete(); // Don't try to process this message as the current application code is not used for outbound
					continue;
				}

				// All remaining fields depend on the message type (applicatioCode). If a new message needs to be tested please add to the below

				var bodyToUse = (string)null;
				switch (applicationCode)
				{
					case ApplicationCodeList.Codes.GbCustomsDeclarationServices:
						bodyToUse = EHubMessageDirectorTests.gBCustomsInterchangeBodyContent;
						break;

					default:
						bodyToUse = $"<Root><Value>{new string('a', 1024 * 8)}</Value></Root>";
						break;
				}
				interchange.SetEI_BodyTextSource(new TextReaderSource(new MemoryStream(new UTF8Encoding(false).GetBytes(bodyToUse))));

				var interchangeTypeToUse = (string)null;
				switch (applicationCode)
				{
					case ApplicationCodeList.Codes.eHub:
						interchangeTypeToUse = "MSS";
						break;

					default:
						break;
				}
				interchange.EI_InterchangeType = interchangeTypeToUse;

				var headerTextToUse = (string)null;
				switch (applicationCode)
				{
					case ApplicationCodeList.Codes.GbCustomsDeclarationServices:
						headerTextToUse = @"<GBCustomsRequest><Provider>Direct</Provider><Service>New</Service><Credentials Key=""HYEAYA.GB123456789000.ABC"" /><JobNumber>B0001000</JobNumber></GBCustomsRequest>";
						break;

					case ApplicationCodeList.Codes.ITCustoms:
						headerTextToUse = "<HeaderValue>5</HeaderValue>";
						break;

					case ApplicationCodeList.Codes.ESCustomsMessage:
						headerTextToUse = "<ESCustoms xmlns:ns=\"http://cargowise.com/ehub/products/ESCustoms\"><Headers><BrokerCode>JRR</BrokerCode><CertificateName>test</CertificateName><CertificateThumbPrint>D36659B690BD3539E6B545594F854D69EE959BBF</CertificateThumbPrint><EntryReferenceNumber>0ESA12345678-B00180800</EntryReferenceNumber><TestMessage>N</TestMessage><Service>PreDeclaIncompletaV1Service</Service><Operation>PreDeclaIncompletaV1Service</Operation><InterchangeType>EXP</InterchangeType></Headers><Body><soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:imp=\"https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ImportacionCompletaV1Ent.xsd\"><soapenv:Header /><soapenv:Body><imp:ImportacionCompletaV1Ent endPoint=\"?\"><SegmentosDeServicio Id=\"20170224104136241208\" fecha=\"20170224\" hora=\"104136\" Test=\"\" /><TheRest>Blah</TheRest></imp:ImportacionCompletaV1Ent></soapenv:Body></soapenv:Envelope></Body></ESCustoms>";
						break;

					case ApplicationCodeList.Codes.UYCustoms:
						headerTextToUse = "<Credentials><UserName>User</UserName><Password>Pass</Password></Credentials>";
						break;

					default:
						break;
				}
				interchange.EI_HeaderText = headerTextToUse;

				var messageTypeToUse = EDIMessageTypeList.Codes.XMS;
				switch (applicationCode)
				{
					case ApplicationCodeList.Codes.CIM:
						messageTypeToUse = EDIMessageTypeList.Codes.FWB;
						break;

					default:
						break;
				}
				interchange.ContainedMessages[0].EM_MessageType = messageTypeToUse;

				var messageSubTypeToUse = (string)null;
				switch (applicationCode)
				{
					case ApplicationCodeList.Codes.XMS:
						messageSubTypeToUse = EDIMessageSubTypeList.Codes.WhsDockets;
						break;

					default:
						break;
				}
				interchange.ContainedMessages[0].EM_MessageSubType = messageSubTypeToUse;
			}
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false).Object;
			serviceTaskJob.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
			serviceTaskJob.CurrentRecipient = recipient;

			AssertNoExceptionThrown(() => serviceTaskJob.ProcessMessagesCore());
		}

		public void TestCallAdapterSendMessagesException_MultipleBatchWhenExceedingGatewayLimit()
		{
			var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			var utcNow = DateTime.UtcNow;
			for (int i = 0; i < 10; i++)
			{
				var interchange = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy);
				interchange.EI_BodyText = "A";
				interchange.EI_SystemCreateTimeUtc = utcNow.AddSeconds(i);
				interchangeList.Add(interchange);
			}
			Factory.Save();

			var adapterException = new CargoWise.eHub.Common.SerializableDictionary<Guid, string>();
			adapterException.Add(interchangeList[2].EI_SessionGUID.ToGuid(), "Error 1");

			var numberOfCalls = 0;
			adapterMock.Setup(x => x.SendMessages()).Callback(() =>
			{
				numberOfCalls++;
				if (numberOfCalls == 1)
				{
					AssertEquals(5, adapterMock.Object.Outbox.Count);
					throw new eHubAdapterException("An error occurs when sending messages", adapterException);
				}
				else
				{
					AssertEquals(5, adapterMock.Object.Outbox.Count);
				}
			});

			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new GatewayAdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
			serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
			serviceTaskJob.Object.CurrentRecipient = "BLAHBLAHB";
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);
			serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(10);
			serviceTaskJob.Setup(m => m.GatewayMaxReceivedMessageLimitInBytes).Returns(5);
			ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
			Factory.ReloadAll<EDIInterchange>();
			var processedInterchange3 = Factory.Load<EDIInterchange>(interchangeList[2].PK);
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, processedInterchange3.EI_Status);
			AssertEquals(true, processedInterchange3.Notes.HasNotes);
			AssertStartsWith("Note.ST_NoteDataAsText", $"{ServiceTaskName} Error: Error 1", (processedInterchange3.Notes.GetAllNotes().FirstOrDefault() as StmNote).ST_NoteDataAsText);

			serviceTaskJob.Object.Notifier.AssertNotificationExists($"Error: 4 interchange(s) sent to {ServiceTaskName}. 1 interchange(s) failed to send to {ServiceTaskName}.");
			serviceTaskJob.Object.Notifier.AssertNotificationExists($"5 interchange(s) sent to {ServiceTaskName}.");
		}

		public void TestSendMessages_GatewayMessageMultipleBatchWhenExceedingGatewayLimit()
		{
			using (var disposables = new DisposableList(2))
			{
				var interchangeBody = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
                     xmlns:DIS=""http://cbp.dhs.gov/DIS""
                     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://cbp.dhs.gov/DIS ../MessageEnvelope.xsd"">
  <DIS:MessageBody>
    <DIS:DocumentSubmissionPackage>
      <DIS:DocumentData>
        <DIS:DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!</DIS:DocumentObject>
      </DIS:DocumentData>
    </DIS:DocumentSubmissionPackage>
  </DIS:MessageBody>
</DIS:MessageEnvelope>";
				var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
				var interchangeList = new List<EDIInterchange>();
				var container = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonContainer>();
				var documentManager = container as IDocManagerSupport;
				var eDoc1 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes("Document1 content pdf"), disposables);
				var eDoc2 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes(new string('$', 1200)), disposables);
				documentManager.DocManagerInfo.Save();

				for (int i = 1; i <= 3; i++)
				{
					var trackingID = new Guid("00000000-0000-0000-0000-00000000000" + i);
					var message = Factory.NewWithValidTestData<EDIMessage>();
					message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
					message.EM_LinkedObject = container;

					var attachment = message.MessageAttachments.AddNew();
					attachment.EG_FileName = "Attachment" + i + ".pdf";
					attachment.EG_EdiMsgDocType = "APP";
					attachment.EG_StorageDocsGuid = i == 1 ? eDoc1.UniqueKey : eDoc2.UniqueKey;

					string messageText = interchangeBody.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", "!dOcUmEnTiMaGePlAcEHoLdEr:" + attachment.PK + "!");

					var interchange = Factory.New<EDIInterchange>();
					interchange.ContainedMessages.Add(message);
					interchange.EI_InterchangeNum = "0000000" + i;
					interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
					interchange.EI_IsActive = ZBool.True;
					interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
					interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
					interchange.EI_GB = Env.CurrentBranch.PK;
					interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISSubmission;
					interchange.EI_BodyText = messageText;
					interchange.EI_From = "ABC Corp";
					interchange.EI_To = "US DIS";
					interchange.EI_SessionGUID = trackingID;
					interchangeList.Add(interchange);
					Factory.Save();
				}

				var numberOfCalls = 0;
				adapterMock.Setup(x => x.SendMessages()).Callback(() =>
				{
					numberOfCalls++;
					if (numberOfCalls == 1)
					{
						var outboxSize = ((MessageOutboxMock)adapterMock.Object.Outbox).SizeInBytes;
						AssertEquals(2, adapterMock.Object.Outbox.Count);
						AssertLessThan("Outbox size should be less than max receive size", outboxSize, 3072);
					}
					else
					{
						var outboxSize = ((MessageOutboxMock)adapterMock.Object.Outbox).SizeInBytes;
						AssertEquals(1, adapterMock.Object.Outbox.Count);
						AssertLessThan("Outbox size should be less than max receive size", outboxSize, 3072);
					}
				});

				var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
				serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
				serviceTaskJob.Object.CurrentRecipient = "US DIS";
				serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);
				serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(2800);
				serviceTaskJob.Setup(m => m.GatewayMaxReceivedMessageLimitInBytes).Returns(3072);
				ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
				Factory.ReloadAll<EDIInterchange>();
				var processedInterchange1 = Factory.Load<EDIInterchange>(interchangeList[0].PK);
				var processedInterchange2 = Factory.Load<EDIInterchange>(interchangeList[1].PK);
				var processedInterchange3 = Factory.Load<EDIInterchange>(interchangeList[2].PK);
				AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange1.EI_Status);
				AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange2.EI_Status);
				AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange3.EI_Status);

				serviceTaskJob.Object.Notifier.AssertNotificationExists($"2 interchange(s) sent to {ServiceTaskName}.");
				serviceTaskJob.Object.Notifier.AssertNotificationExists($"1 interchange(s) sent to {ServiceTaskName}.");
			}
		}

		public void TestSendMessages_OneOutgoingMessageSizeOverGatewayMaxReceiveSize()
		{
			using (var disposables = new DisposableList(3))
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var interchangeBody = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
                     xmlns:DIS=""http://cbp.dhs.gov/DIS""
                     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://cbp.dhs.gov/DIS ../MessageEnvelope.xsd"">
  <DIS:MessageBody>
    <DIS:DocumentSubmissionPackage>
      <DIS:DocumentData>
        <DIS:DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!</DIS:DocumentObject>
      </DIS:DocumentData>
    </DIS:DocumentSubmissionPackage>
  </DIS:MessageBody>
</DIS:MessageEnvelope>";
				var adapterMock = new Mock<EHubAdapterMock>() { CallBase = true };
				var interchangeList = new List<EDIInterchange>();
				var container = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonContainer>();
				var documentManager = container as IDocManagerSupport;
				var eDoc1 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes("Document1 content pdf"), disposables);
				var eDoc2 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes(new string('$', 2000)), disposables);
				var eDoc3 = AddDISDocument(documentManager, Encoding.UTF8.GetBytes(new string('$', 1200)), disposables);
				documentManager.DocManagerInfo.Save();

				var utcNow = DateTime.UtcNow;
				var messageCount = 3;
				for (int i = 1; i <= messageCount; i++)
				{
					var trackingID = new Guid("00000000-0000-0000-0000-00000000000" + i);
					var message = Factory.NewWithValidTestData<EDIMessage>();
					message.EM_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
					message.EM_LinkedObject = container;

					var attachment = message.MessageAttachments.AddNew();
					attachment.EG_FileName = "Attachment" + i + ".pdf";
					attachment.EG_EdiMsgDocType = "APP";
					attachment.EG_StorageDocsGuid = i == 1 ? eDoc1.UniqueKey : i == 2 ? eDoc2.UniqueKey : eDoc3.UniqueKey;

					string messageText = interchangeBody.Replace("!dOcUmEnTiMaGePlAcEHoLdEr:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx!", "!dOcUmEnTiMaGePlAcEHoLdEr:" + attachment.PK + "!");

					var interchange = Factory.New<EDIInterchange>();
					interchange.ContainedMessages.Add(message);
					interchange.EI_InterchangeNum = "0000000" + i;
					interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
					interchange.EI_IsActive = ZBool.True;
					interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
					interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
					interchange.EI_GB = Env.CurrentBranch.PK;
					interchange.EI_ApplicationCode = ApplicationCodeList.Codes.USCustomsDIS;
					interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.USDISSubmission;
					interchange.EI_BodyText = messageText;
					interchange.EI_From = "ABC Corp";
					interchange.EI_To = "US DIS";
					interchange.EI_SessionGUID = trackingID;
					interchange.EI_SystemCreateTimeUtc = utcNow.AddSeconds(i);
					interchangeList.Add(interchange);
				}
				Factory.Save();

				var numberOfCalls = 0;
				adapterMock.Setup(x => x.SendMessages()).Callback(() =>
				{
					numberOfCalls++;
					if (numberOfCalls == 1)
					{
						var outboxSize = ((MessageOutboxMock)adapterMock.Object.Outbox).SizeInBytes;
						AssertEquals(1, adapterMock.Object.Outbox.Count);
						AssertLessThan("Outbox size should be less than max receive size", outboxSize, 3072);
					} else if (numberOfCalls == 2)
					{
						var outboxSize = ((MessageOutboxMock)adapterMock.Object.Outbox).SizeInBytes;
						AssertEquals(1, adapterMock.Object.Outbox.Count);
						AssertGreaterThan("Outbox size should be larger than max receive size", outboxSize, 3072);
						throw new eHubAdapterException("The maximum message size quota for incoming messages (3072) has been exceeded");
					} else if (numberOfCalls == 3)
					{
						var outboxSize = ((MessageOutboxMock)adapterMock.Object.Outbox).SizeInBytes;
						AssertEquals(1, adapterMock.Object.Outbox.Count);
						AssertLessThan("Outbox size should be less than max receive size", outboxSize, 3072);
					}
				});

				var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false, adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(adapterMock.Object));
				serviceTaskJob.Object.CurrentBranchPk = CurrentCompany.FirstActiveBranch.PK;
				serviceTaskJob.Object.CurrentRecipient = "US DIS";
				serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(10);
				serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(2560);
				serviceTaskJob.Setup(m => m.GatewayMaxReceivedMessageLimitInBytes).Returns(3072);
				ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
				Factory.ReloadAll<EDIInterchange>();
				var processedInterchange1 = Factory.Load<EDIInterchange>(interchangeList[0].PK);
				var processedInterchange2 = Factory.Load<EDIInterchange>(interchangeList[1].PK);
				var processedInterchange3 = Factory.Load<EDIInterchange>(interchangeList[2].PK);
				AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange1.EI_Status);
				AssertEquals(EDIInterchangeStatusList.Codes.Failed, processedInterchange2.EI_Status);
				AssertEquals(EDIInterchangeStatusList.Codes.eHubPending, processedInterchange3.EI_Status);

				AssertEquals(true, processedInterchange2.Notes.HasNotes);
				AssertStartsWith("Note.ST_NoteDataAsText", $"{ServiceTaskName} Error: prepared outgoing message larger than the maximum receive size for the outbound service (3072 bytes).", (processedInterchange2.Notes.GetAllNotes().FirstOrDefault() as StmNote).ST_NoteDataAsText);

				serviceTaskJob.Object.Notifier.AssertNotificationExists($"1 interchange(s) sent to {ServiceTaskName}.", 2);
				adapterMock.VerifyAll();
				serviceTaskJob.VerifyAll();
			}
		}

		public void TestSendMessages_MultipleBatchWhenExceedingGatewayLimit()
		{
			var interchangeList = new List<EDIInterchange>();
			var interchangeNumberStrategy = new TestMessageNumberStrategy();
			var messageNumberStrategy = new TestMessageNumberStrategy();
			for (int i = 0; i < 15; i++)
			{
				interchangeList.Add(CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, interchangeSize: 1));
			}
			Factory.Save();

			var serviceTaskJob = CreateMockJob_Moq(new GatewayAdaptorFactoryMockWithOneAdaptor(new EHubAdapterMock()), mockInterchangeCandidates: false);
			serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(100);
			serviceTaskJob.Setup(m => m.AdapterOutboxSizeLimitInBytes).Returns(9);
			serviceTaskJob.Setup(m => m.GatewayMaxReceivedMessageLimitInBytes).Returns(5);
			ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
			Factory.ReloadAll<EDIInterchange>();
			serviceTaskJob.Object.Notifier.AssertNotificationExists($"5 interchange(s) sent to {ServiceTaskName}.", 3);
			foreach (var item in interchangeList)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}

			var reloadFactory = new BusinessObjectFactory();
			var reloadedInterchanges = interchangeList.Select(item => reloadFactory.Load<EDIInterchange>(item.PK)).ToList();
			foreach (var item in reloadedInterchanges)
			{
				AssertEquals(GetInterchangePendingStatus(), item.EI_Status);
			}
		}

		public void TestPendingItemsBatchSize()
		{
			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertEquals("Default value was incorrect", 1000, serviceTaskJob.Object.PendingItemsBatchSize);

			eHubMessagingRegistry.Instance.eHubOutboundPendingItemsBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10000);
			AssertEquals("Value was not updated", 10000, serviceTaskJob.Object.PendingItemsBatchSize);
		}

		public void TestPendingItemsSearchLimit()
		{
			var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
			AssertEquals("Default value was incorrect", 100000, serviceTaskJob.Object.PendingItemsSearchLimit);

			eHubMessagingRegistry.Instance.eHubOutboundPendingItemsSearchLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10000);
			AssertEquals("Value was not updated", 10000, serviceTaskJob.Object.PendingItemsSearchLimit);
		}

		public void TestRetrySaveOnConcurrencyError()
		{
			var adaptorFactory = new OutboundAdaptorFactoryMock();
			var company1 = CreateCompanyWithBranch();
			var job = CreateMockJob_Moq(adaptorFactory, CreateCompanySettingsManager(), null, mockInterchangeCandidates: false);
			var interchange1 = CreateTestInterchange(Factory, GetInterchangeQueuedStatus(), company1, "RC1");
			SetInterchangeProperties(interchange1, company1.FirstActiveBranch.PK, 0, true, EDIInterchange.Direction.Transmit, GetInterchangeQueuedStatus());
			Factory.Save();
			job.Setup(x => x.CallAdapterToSendMessages(It.IsAny<IeHubAdapter>()))
				.Callback(new Action<IeHubAdapter>((a) =>
			{
				Db.Connection.TryGetLock(MutexConstants.MessageMutexPrefix + interchange1.EI_SessionGUID, out var mutex);
				job.Object.lockedInterchangePairs.Add(interchange1.PK, (interchange1, mutex));
				Db.Connection.ExecuteNonQuery($"UPDATE dbo.EDIInterchange SET EI_Status = 'SNT' WHERE EI_PK='{interchange1.PK}'");
			}));

			ExecuteJobWithServiceTaskContext(job.Object, "EHO");
			AssertEquals("Job should be scheduled again as data was processed", true, job.Object.NextExecuteIterationIsScheduled);
			AssertEquals("Wrong number of Adapters created after first execute", 1, adaptorFactory.Adapters.Length);
			Factory.ReloadAll<EDIInterchange>();
			AssertEquals("Interchange should have remained sent", "SNT", interchange1.EI_Status);
		}

		public void TestSendMessages_LockFailedAllInterchanges()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var interchangeList = new List<EDIInterchange>();
				var interchangeNumberStrategy = new TestMessageNumberStrategy();
				var messageNumberStrategy = new TestMessageNumberStrategy();
				for (int i = 0; i < 7; i++)
				{
					var interchange = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, interchangeSize: 1);
					interchange.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(i);
					interchangeList.Add(interchange);
				}
				Factory.Save();
				DisposableList mutexes = new DisposableList(5);

				for (int i = 0; i < 3; i++)
				{
					extraConnection.TryGetLock(MutexConstants.MessageMutexPrefix + interchangeList[i].EI_SessionGUID, out var mutex);
					mutexes.Add(mutex);
				}

				var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
				serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(3);
				serviceTaskJob.Setup(m => m.PendingItemsBatchSize).Returns(6);
				serviceTaskJob.Setup(m => m.InterchangeLockTimeout).Returns(TimeSpan.FromSeconds(1));
				ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
				AssertEquals("Outbound EDI Interchange Lock Timeout for eHub Tracking ID: " + interchangeList[0].EI_SessionGUID, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				Factory.ReloadAll<EDIInterchange>();
				serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("3 interchange(s) sent to {0}.", ServiceTaskName), 2);

				var reloadFactory = new BusinessObjectFactory();
				for (int i = 0; i < 7; i++)
				{
					AssertEquals(GetInterchangeQueuedStatus(), reloadFactory.Load<EDIInterchange>(interchangeList[i].PK).EI_Status);
				}

				foreach (var mutex in mutexes)
				{
					mutex.Dispose();
				}
				mutexes.Dispose();
				mutexes = null;
			}
		}

		public void TestSendMessages_LockFailedSomeInterchanges()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var interchangeList = new List<EDIInterchange>();
				var interchangeNumberStrategy = new TestMessageNumberStrategy();
				var messageNumberStrategy = new TestMessageNumberStrategy();
				for (int i = 0; i < 7; i++)
				{
					var interchange = CreateTestInterchange("BLAHBLAHB", interchangeNumberStrategy, messageNumberStrategy, interchangeSize: 1);
					interchange.EI_SystemCreateTimeUtc = DateTime.UtcNow.AddSeconds(i);
					interchangeList.Add(interchange);
				}
				Factory.Save();
				DisposableList mutexes = new DisposableList(5);

				extraConnection.TryGetLock(MutexConstants.MessageMutexPrefix + interchangeList[1].EI_SessionGUID, out var mutex);

				var serviceTaskJob = CreateMockJob_Moq(mockInterchangeCandidates: false);
				serviceTaskJob.Setup(m => m.AdapterOutboxCountLimit).Returns(3);
				serviceTaskJob.Setup(m => m.InterchangeLockTimeout).Returns(TimeSpan.FromSeconds(1));
				ExecuteJobWithServiceTaskContext(serviceTaskJob.Object, "EHO");
				AssertEquals("Outbound EDI Interchange Lock Timeout for eHub Tracking ID: " + interchangeList[1].EI_SessionGUID, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
				Factory.ReloadAll<EDIInterchange>();
				serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("3 interchange(s) sent to {0}.", ServiceTaskName), 3);
				serviceTaskJob.Object.Notifier.AssertNotificationExists(string.Format("1 interchange(s) sent to {0}.", ServiceTaskName));

				var reloadFactory = new BusinessObjectFactory();
				for (int i = 0; i < 7; i++)
				{
					if (i == 1)
					{
						AssertEquals(GetInterchangeQueuedStatus(), reloadFactory.Load<EDIInterchange>(interchangeList[i].PK).EI_Status);
					}
					else
					{
						AssertEquals(GetInterchangePendingStatus(), reloadFactory.Load<EDIInterchange>(interchangeList[i].PK).EI_Status);
					}
				}

				mutex.Dispose();
				mutexes.Dispose();
				mutexes = null;
			}
		}

		public void TestWaitsForInterchangeLocks()
		{
			using (var messageSent = new AutoResetEvent(false))
			using (var lockEvent = new AutoResetEvent(false))
			{
				var testFailedTimeoutSpan = TimeSpan.FromSeconds(30);
				var interchange = CreateTestInterchange("BLAHBLAHB", new TestMessageNumberStrategy(), new TestMessageNumberStrategy(), interchangeSize: 1);
				Factory.Save();

				var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
				mockOutbox.Setup(m => m.AddMessage(It.IsAny<IeHubMessage>()));
				mockOutbox.Setup(m => m.Count).Returns(1);
				mockOutbox.Setup(m => m.Clear());
				mockOutbox.Setup(m => m.SizeInKiloBytes).Returns(1);

				var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
				mockAdapter.Setup(m => m.Dispose());
				mockAdapter.Setup(m => m.Outbox).Returns(mockOutbox.Object);
				mockAdapter.Setup(a => a.SendMessages())
					.Callback(new Action(() => messageSent.Set()));
				var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object), mockInterchangeCandidates: false);

				var task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						Assert("Should be able to obtain lock before execute", Db.Connection.TryGetLock(MutexConstants.MessageMutexPrefix + interchange.EI_SessionGUID, out var mutex));
						using (mutex)
						{
							lockEvent.Set();
							Assert("Message was sent", !messageSent.WaitOne(TimeSpan.FromSeconds(2)));
						}
					}
				});
				Assert("Lock event didn't fire", lockEvent.WaitOne(testFailedTimeoutSpan));
				ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHO");
				Assert("lock task did not complete", task.Wait(testFailedTimeoutSpan));
				Factory.ReloadAll<EDIInterchange>();
				AssertEquals("Status should be pending, as we should have been able to obtain the lock in the end", mockServiceTaskJob.Object.InterchangeSuccessStatus, interchange.EI_Status.ToString());
				mockOutbox.VerifyAll();
				mockAdapter.VerifyAll();
			}
		}

		public void TestLocksInterchangesDuringSendNotFinalise()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var interchange = CreateTestInterchange("BLAHBLAHB", new TestMessageNumberStrategy(), new TestMessageNumberStrategy(), interchangeSize: 1);
				Factory.Save();

				var mockOutbox = new Mock<IMessageOutbox>(MockBehavior.Strict);
				mockOutbox.Setup(m => m.AddMessage(It.IsAny<IeHubMessage>()));
				mockOutbox.Setup(m => m.Count).Returns(1);
				mockOutbox.Setup(m => m.Clear());
				mockOutbox.Setup(m => m.SizeInKiloBytes).Returns(1);

				var mockAdapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
				mockAdapter.Setup(m => m.Dispose());
				mockAdapter.Setup(m => m.Outbox).Returns(mockOutbox.Object);
				mockAdapter.Setup(a => a.SendMessages())
					.Callback(new Action(() =>
					{
						SqlApplicationLock mutex = null;
						try
						{
							Assert("Should not be able to obtain lock during send", !extraConnection.TryGetLock(MutexConstants.MessageMutexPrefix + interchange.EI_SessionGUID, TimeSpan.FromSeconds(2), out mutex));
							interchange.EI_Status = "CAN";
							Factory.Save();
						}
						finally
						{
							mutex?.Dispose();
						}
					}));
				var mockServiceTaskJob = CreateMockJob_Moq(adaptorFactory: new AdaptorFactoryMockWithOneAdaptor(mockAdapter.Object), mockInterchangeCandidates: false);
				ExecuteJobWithServiceTaskContext(mockServiceTaskJob.Object, "EHO");
				Factory.ReloadAll<EDIInterchange>();
				AssertEquals("Status should remain as CAN, as the interchange was canceled during the send", "CAN", interchange.EI_Status.ToString());
				mockOutbox.VerifyAll();
				mockAdapter.VerifyAll();
			}
		}

		protected override string GetExpectedTableIndexHint()
		{
			return EDIInterchangeSchema.Constants.Indexes.NR_RX__EI_IsActive_EI_ReceiveTransmit_EI_Status_EI_To_EI_GB_HQU;
		}

		protected override string GetInterchangeQueuedStatus()
		{
			return EDIInterchange.Status.eHubQueued;
		}

		protected override string GetInterchangePendingStatus()
		{
			return EDIInterchange.Status.eHubPending;
		}

		void AssertEHubMessage(IeHubMessage message, EDIInterchange interchange, string schemaName, MessageSchemaType schemaType, string messageText, string emailSubject, string fileName)
		{
			AssertNotNull(message);
			AssertEquals(emailSubject, message.EmailSubject);
			AssertEquals(fileName, message.Filename);
			AssertEquals(CurrentCompany.LicenceKeyIdentifier, message.SenderID);
			AssertEquals("BLAHBLAHB", message.RecipientID);
			AssertEquals(interchange.EI_SessionGUID.ToGuid(), message.TrackingID);
			AssertEquals(schemaName, message.SchemaName);
			AssertEquals(schemaType, message.SchemaType);
			AssertEquals(messageText, new StreamReader(message.MessageStream).ReadToEnd());
			var notes = interchange.Notes.FindByDescription("eHub Error");
			AssertEquals(0, notes.Length);
			message.Dispose();
		}

		const string dummySessionID = "429C45C6-8968-4754-A48C-C4222C8EB32A";

		protected override eHubMessaging.ServiceTasks.AdapterType ServiceAdapterType => eHubMessaging.ServiceTasks.AdapterType.GatewayAdapter;
	}
}
