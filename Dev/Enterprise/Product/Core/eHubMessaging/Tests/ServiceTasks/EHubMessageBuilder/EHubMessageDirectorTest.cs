using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.ServiceTasks.EHubMessageBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;

namespace Enterprise.eHubMessaging.Tests
{
	public class EHubMessageDirectorTests : TestCaseWithFactory
	{
		#region TestCreateeHubMessageFail

		public void TestCreateeHubMessageFail_NotSupportedApplicationCode()
		{
			var expectedInterchangeNotes = "EDI Interchange Application Code '~NA' is not supported.";
			var expectedNotifications = "Warning: " + expectedInterchangeNotes;
			CreateAndAsserteHubBuilderFail("~NA", "TST", "Header", "Body", "Footer", "", "", expectedInterchangeNotes, expectedNotifications);
		}

		public void TestCreateeHubMessageFail_EmptyContent()
		{
			var expectedInterchangeNotes = "Cannot create eAdaptor message: message content is empty.";
			var expectedNotifications = "Warning: " + expectedInterchangeNotes;
			var header = "<EDIDelivery><FileName>Blah.txt</FileName><EmailSubject>blah@blah.com</EmailSubject></EDIDelivery>";
			CreateAndAsserteHubBuilderFail(ApplicationCodeList.Codes.UniversalDataMessaging, EDIMessageTypeList.Codes.XDC, header, "", "Footer", "", "", expectedInterchangeNotes, expectedNotifications);
		}

		public void TestCreateeHubMessageFail_HandleBuildException()
		{
			ErrorReporter.Clear();
			var interchange = CreateInterchange("XMS", "TST", "Header", "Body", "Footer");
			var message = CreateMessage(interchange);
			var notifier = new NotificationBuffer();

			var director = new Mock<EHubMessageDirector>(interchange, notifier) { CallBase = true };
			var builder = new Mock<EHubMessageBuilderForXMS>(interchange, notifier);
			var exceptionMessage = "Exception thrown during build.";
			builder.Protected().Setup("BuildCore").Throws(new Exception(exceptionMessage));
			director.Setup(m => m.CreateBuilder()).Returns(builder.Object);

			var diagnosticDetails = interchange.DiagnosticDetails;
			var eHubMessage = director.Object.CreateMessage();

			AssertNull("eHubMessage should be null", eHubMessage);
			AssertEquals("Interchange Number: " + dummyInterchangeNum + ". eHub Id: " + interchange.eHubID + ". Diagnostic Details: " + diagnosticDetails, ErrorReporter.LastMessageReported);

			director.VerifyAll();
			builder.VerifyAll();
			ErrorReporter.Clear();
		}

		#endregion

		#region TestCreateXMSeHubMessage

		public void TestCreateXMSeHubMessage()
		{
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.AgencyBillsOfLading, EDIMessageSchemaNameList.Descriptions.AgencyBillsOfLading);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Consols, EDIMessageSchemaNameList.Descriptions.Consols);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.ContainerMovements, EDIMessageSchemaNameList.Descriptions.ContainerMovements);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Events, EDIMessageSchemaNameList.Descriptions.Events);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.FinancialTransactions, EDIMessageSchemaNameList.Descriptions.FinancialTransactions);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Orders, EDIMessageSchemaNameList.Descriptions.Orders);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Invoices, EDIMessageSchemaNameList.Descriptions.Invoices);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Products, EDIMessageSchemaNameList.Descriptions.Products);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Shipments, EDIMessageSchemaNameList.Descriptions.Shipments);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.WhsDockets, EDIMessageSchemaNameList.Descriptions.WhsDockets);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.LocalCartageBooking, EDIMessageSchemaNameList.Descriptions.LocalCartageBooking);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.LocalCartageStatus, EDIMessageSchemaNameList.Descriptions.LocalCartageStatus);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.DocumentMessages, EDIMessageSchemaNameList.Descriptions.DocumentMessages);
			CreateXMSeHubMessageSucceed(EDIMessageSubTypeList.Codes.Organizations, EDIMessageSchemaNameList.Descriptions.Organizations);

			CreateAndAsserteHubMessageFailDueToMessageRequired(ApplicationCodeList.Codes.XMS);
			CreateXMSeHubMessageFailDueToNotSupportedMessageSubType();
		}

		void CreateXMSeHubMessageSucceed(string messageSubType, string expectedSchemaName)
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.XMS,
				"",
				"DummyTo",
				@"<EDIDelivery><FileName>Blah.txt</FileName><EmailSubject>blah@blah.com</EmailSubject></EDIDelivery>",
				"Hello world",
				"",
				"",
				messageSubType,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.XMS,
				expectedSchemaName,
				"Hello world",
				"blah@blah.com",
				"Blah.txt");
		}

		void CreateXMSeHubMessageFailDueToNotSupportedMessageSubType()
		{
			var expectedInterchangeNotes = "Cannot create eAdaptor message: schema name cannot be found.";
			var expectedNotifications = "Warning: " + expectedInterchangeNotes;
			var notSupportedMessageSubType = "~NA";
			CreateAndAsserteHubMessageFail(ApplicationCodeList.Codes.XMS, "", "Header", "Body", "Footer", "", notSupportedMessageSubType, expectedInterchangeNotes, expectedNotifications);
		}

		#endregion

		#region TestCreateXMLeHubMessage

		public void TestCreateXMLeHubMessage()
		{
			CreateXMLeHubMessageSucceed(ApplicationCodeList.Codes.NativeDataMessaging, EDIMessageSchemaNameList.Descriptions.NativeDataMessaging);
			CreateXMLeHubMessageSucceed(ApplicationCodeList.Codes.UniversalDataMessaging, EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging);
			CreateXMLeHubMessageSucceed(ApplicationCodeList.Codes.CustomsWare, EDIMessageSchemaNameList.Descriptions.CustomsWare);
			CreateXMLeHubMessageSucceed(ApplicationCodeList.Codes.USeBond, EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging);

			CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.CustomsWare, EDIMessageSchemaNameList.Descriptions.CustomsWare);
			CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.ChinaInterfaceMapping, EDIMessageSchemaNameList.Descriptions.ChinaInterface);
			CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USeBond, EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging);
			CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.AirCargoAdvanceScreening, EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging);
			ErrorReporter.Clear();
		}

		void CreateXMLeHubMessageSucceed(string applicationCode, string expectedSchemaName)
		{
			CreateAndAsserteHubMessageSucceed(
				applicationCode,
				"",
				"DummyTo",
				@"<EDIDelivery><FileName>Blah.txt</FileName><EmailSubject>blah@blah.com</EmailSubject></EDIDelivery>",
				"Hello world",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				applicationCode,
				expectedSchemaName,
				"Hello world",
				"blah@blah.com",
				"Blah.txt");
		}

		void CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(string applicationCode, string expectedSchemaName)
		{
			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndDiagnosticInfoNotReported(
				applicationCode,
				"",
				"DummyTo",
				@"<EDIDelivery><FileName>Blah.txt</FileName><EmailSubject>blah@blah.com</EmailSubject></EDIDelivery>",
				"Hello world",
				"",
				MessageSchemaType.Xml,
				applicationCode,
				expectedSchemaName,
				"Hello world",
				"blah@blah.com",
				"Blah.txt");
		}

		#endregion

		#region TestCreateSYSeHubMessage

		public void TestCreateSYSeHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.SYS,
				"",
				"DummyTo",
				@"<EDIDelivery><FileName>Blah.txt</FileName><EmailSubject>blah@blah.com</EmailSubject></EDIDelivery>",
				"Hello world",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.SYS,
				EDIMessageSchemaNameList.Descriptions.UniversalDataMessaging,
				"Hello world",
				"blah@blah.com",
				"Blah.txt");

			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndNoDiagnosticInfoReported(
				ApplicationCodeList.Codes.SYS,
				"",
				@"<EDIDelivery><FileName>Blah.txt</FileName><EmailSubject>blah@blah.com</EmailSubject></EDIDelivery>",
				"Hello world",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.SYS,
				Enterprise.eHubMessaging.Business.SystemMessage.SchemaName,
				"Hello world",
				"blah@blah.com",
				"Blah.txt");
		}

		#endregion

		#region TestCreateCIMeHubMessage

		public void TestCreateCIMeHubMessage()
		{
			CreateCIMeHubMessageSucceed(ApplicationCodeList.Codes.CIM, EDIMessageTypeList.Codes.FHL, EDIMessageSchemaNameList.Descriptions.FHL);
			CreateCIMeHubMessageSucceed(ApplicationCodeList.Codes.CIM, EDIMessageTypeList.Codes.FWB, EDIMessageSchemaNameList.Descriptions.FWB);
			CreateCIMeHubMessageSucceed(ApplicationCodeList.Codes.SGCustomsCMD, EDIMessageTypeList.Codes.CMD, EDIMessageSchemaNameList.Descriptions.SGCustomsCMD);

			CreateCIMeHubMessageFailDueToNotSupportedMessageType();
			CreateAndAsserteHubMessageFailDueToMessageRequired(ApplicationCodeList.Codes.CIM);
		}

		void CreateCIMeHubMessageSucceed(string applicationCode, string messageType, string expectedSchemaName)
		{
			CreateAndAsserteHubMessageSucceed(
				applicationCode,
				"",
				"DummyTo",
				"Header",
				"Body",
				"Footer",
				messageType,
				"",
				MessageSchemaType.FlatFile,
				applicationCode,
				expectedSchemaName,
				"HeaderBodyFooter");
		}

		void CreateCIMeHubMessageFailDueToNotSupportedMessageType()
		{
			var expectedInterchangeNotes = "Cannot create eAdaptor message: schema name cannot be found.";
			var expectedNotifications = "Warning: " + expectedInterchangeNotes;
			CreateAndAsserteHubMessageFail(ApplicationCodeList.Codes.CIM, "", "Header", "Body", "Footer", "~NA", "", expectedInterchangeNotes, expectedNotifications);
		}

		#endregion

		#region TestCreateEdifacteHubMessage

		public void TestCreateEdifacteHubMessage()
		{
			CreateEdifacteHubMessageSucceed(ApplicationCodeList.Codes.Inttra);
			CreateEdifacteHubMessageSucceed(ApplicationCodeList.Codes.ShippingLineEHubMessaging);

			CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.Inttra);
			CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.ShippingLineEHubMessaging);
		}

		void CreateEdifacteHubMessageSucceed(string applicationCode)
		{
			CreateAndAsserteHubMessageSucceed(
				applicationCode,
				"",
				"DummyTo",
				"Header",
				"Body",
				"Footer",
				"",
				"",
				MessageSchemaType.FlatFile,
				applicationCode,
				EDIMessageSchemaNameList.Descriptions.InttraEdifact,
				"HeaderBodyFooter");
		}

		void CreateXMLeHubMessageFromInterchangeWithNoMessageSucceed(string applicationCode)
		{
			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndDiagnosticInfoNotReported(
							applicationCode,
							"",
							"DummyTo",
							"Header",
							"Body",
							"Footer",
							MessageSchemaType.FlatFile,
							applicationCode,
							EDIMessageSchemaNameList.Descriptions.InttraEdifact,
							"HeaderBodyFooter");
		}

		#endregion

		#region TestCreateNZCustomseHubMessage

		public void TestCreateNZCustomseHubMessage()
		{
			CreateNZCustomseHubMessageSucceed(
				ApplicationCodeList.Codes.NZMAFeBACCa,
				EDIInterchange.InterchangePartyIDs.NZCustomsLiveMailbox,
				@"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Type>Xml</Type><Authentication /><Content>PGJvZHk+VGVzdDwvYm9keT4=</Content></NZCustoms>",
				"NZCustoms");

			CreateNZCustomseHubMessageFromInterchangeWithNoMessageSucceed(
				ApplicationCodeList.Codes.NZMAFeBACCa,
				EDIInterchange.InterchangePartyIDs.NZMAFeBACCaLiveMailbox,
				@"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Type>Xml</Type><Authentication /><Content>PGJvZHk+VGVzdDwvYm9keT4=</Content></NZCustoms>",
				"NZCustoms");

			CreateNZCustomseHubMessageSucceed(
				ApplicationCodeList.Codes.NZCustoms,
				EDIInterchange.InterchangePartyIDs.NZCustomsTestMailbox,
				@"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Type>Text</Type><Authentication /><Content>PGhlYWRlcj48Ym9keT5UZXN0PC9ib2R5Pjxmb290ZXI+</Content></NZCustoms>",
				"NZCustomsTest");

			CreateNZCustomseHubMessageFromInterchangeWithNoMessageSucceed(
				ApplicationCodeList.Codes.NZCustoms,
				EDIInterchange.InterchangePartyIDs.NZMAFeBACCaTestMailbox,
				@"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Type>Text</Type><Authentication /><Content>PGhlYWRlcj48Ym9keT5UZXN0PC9ib2R5Pjxmb290ZXI+</Content></NZCustoms>",
				"NZCustomsTest");

			Array.ForEach(new string[] { "ANA", "AND", "I10", "I11", "I51", "I52", "I53", "IPI", "E40", "E41", "EXC", "ICR", "CRE", "OCR" },
				interchangeType =>
				{
					CreateWCONZCustomseHubMessageSucceed(interchangeType);
					CreateAndAsserteHubMessageFailDueToMessageRequired(ApplicationCodeList.Codes.NZCustoms, interchangeType);
				}
			);
		}

		public void TestGenericMessageDeliveryForRefDataRepoMessage()
		{
			Array.ForEach(new string[] { "CMR", "CAC", "CFG", "CWS", "EHU", "RDM" },
			interchangeType =>
			{
				var interchange = CreateInterchange(ApplicationCodeList.Codes.GenericMessageDelivery, interchangeType, "Header", "Body", "Footer");
				var notifier = new NotificationBuffer();
				var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

				if (interchangeType != "RDM")
				{
					AssertNull(eHubMessage);
				}
				else
				{
					AssertNotNull(eHubMessage);
				}

				AssertEquals("Diagnostic report should not exist", true, ErrorReporter.LastMessageReported.Length == 0);
			});
		}

		void CreateNZCustomseHubMessageSucceed(string applicationCode, string interchangeTo, string expectedStreamContent, string expectedRecipientID)
		{
			CreateAndAsserteHubMessageSucceed(
				applicationCode,
				"",
				interchangeTo,
				"<header>",
				"<body>Test</body>",
				"<footer>",
				"",
				"",
				MessageSchemaType.Xml,
				applicationCode,
				EDIMessageSchemaNameList.Descriptions.NZCustoms,
				expectedStreamContent,
				expectedRecipientID: expectedRecipientID);
		}

		void CreateNZCustomseHubMessageFromInterchangeWithNoMessageSucceed(string applicationCode, string interchangeTo, string expectedStreamContent, string expectedRecipientID)
		{
			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndNoDiagnosticInfoReportedIfApplicationDoesNotRequireMessage(
				applicationCode,
				"",
				interchangeTo,
				"<header>",
				"<body>Test</body>",
				"<footer>",
				MessageSchemaType.Xml,
				applicationCode,
				EDIMessageSchemaNameList.Descriptions.NZCustoms,
				expectedStreamContent,
				expectedRecipientID: expectedRecipientID);
		}

		void CreateWCONZCustomseHubMessageSucceed(string interchangeType)
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.NZCustoms, interchangeType, "<header>", "<body>Declaration content</body>", "<footer>");
			var message = CreateMessage(interchange);
			CreateMessageAttachment(message, "Attachment1.pdf", "APP", "Document1", "CUS", "Document1 content pdf");
			CreateMessageAttachment(message, "Attachment2.xml", "DAC", "Document2", "MSC", "Document2 content xml");
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage, MessageSchemaType.Xml, ApplicationCodeList.Codes.NZCustoms, EDIMessageSchemaNameList.Descriptions.NZCustoms, @"<NZCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Type>XmlWithAttachments</Type><Authentication>&lt;footer&gt;</Authentication><Content>77u/PERvY3VtZW50cyB4bWxucz0iaHR0cDovL2Nhcmdvd2lzZS5jb20vZWh1Yi9wcm9kdWN0cy94bWx3aXRoYXR0YWNobWVudHMiPjxEb2N1bWVudD48RG9jdW1lbnRUeXBlPkRFQzwvRG9jdW1lbnRUeXBlPjxDb250ZW50VHlwZT5YbWw8L0NvbnRlbnRUeXBlPjxGaWxlTmFtZT5EZWNsYXJhdGlvbi54bWw8L0ZpbGVOYW1lPjxDb250ZW50Pkg0c0lBQUFBQUFBRUFMTkp5aytwdEhOSlRjNUpMRW9zeWN6UFUwak96eXRKelN1eDBRZkxBQUNVdUxJYklBQUFBQT09PC9Db250ZW50PjwvRG9jdW1lbnQ+PERvY3VtZW50PjxEb2N1bWVudFR5cGU+QVBQPC9Eb2N1bWVudFR5cGU+PENvbnRlbnRUeXBlIC8+PEZpbGVOYW1lPkF0dGFjaG1lbnQxLnBkZjwvRmlsZU5hbWU+PENvbnRlbnQ+SDRzSUFBQUFBQUFFQUhQSlR5N05UYzByTVZSSXpzOHJBVElVQ2xMU0FJOURIRTRWQUFBQTwvQ29udGVudD48L0RvY3VtZW50PjxEb2N1bWVudD48RG9jdW1lbnRUeXBlPkRBQzwvRG9jdW1lbnRUeXBlPjxDb250ZW50VHlwZSAvPjxGaWxlTmFtZT5BdHRhY2htZW50Mi54bWw8L0ZpbGVOYW1lPjxDb250ZW50Pkg0c0lBQUFBQUFBRUFIUEpUeTdOVGMwck1WSkl6czhyQVRJVUtuSnpBSzRzMHN3VkFBQUE8L0NvbnRlbnQ+PC9Eb2N1bWVudD48L0RvY3VtZW50cz4=</Content></NZCustoms>", expectedRecipientID: "NZCustoms");
		}

		#endregion

		#region TestCreateTWCustomsMessageBuilder

		public void TestCreateTWCustomsMessage()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.TWCustoms, "ADM", @"
<TWMessageInfo>
	<CompanyID>TST</CompanyID>
	<MailBox>mailbox</MailBox>
	<MessageType>ICD</MessageType>
	<PasswordType>TVA</PasswordType>
	<EntryNumber>0899900001</EntryNumber>
	<EntryNumberType>IMP</EntryNumberType>
	<StaffCode>JLU</StaffCode>
</TWMessageInfo>
", @"<body><Declaration xsi:schemaLocation=""urn:wco:datamodel:TW:NX5901:R-01-00 ../maindoc/NX5901.xsd"" xmlns=""urn:wco:datamodel:TW:NX5901:R-01-00"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""/></body>", "<footer>");
			var message = CreateMessage(interchange);
			CreateMessageAttachment(message, "Attachment1.pdf", "APP", "Document1", "CUS", "Document1 content pdf");
			CreateMessageAttachment(message, "Attachment2.xml", "DAC", "Document2", "MSC", "Document2 content xml");
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.TWCustoms,
				"http://cargowise.com/ehub/products/TWCPluginRequest#TWCPluginServiceSendRequest",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TWCPluginServiceSendRequest xmlns=\"http://cargowise.com/ehub/products/TWCPluginRequest\">\r\n  <systemId>Dumrom</systemId>\r\n  <companyId>TST</companyId>\r\n  <staffCode>JLU</staffCode>\r\n  <mailbox>mailbox</mailbox>\r\n  <passwordType>TVA</passwordType>\r\n  <messageType>ICD</messageType>\r\n  <entryNumber>0899900001</entryNumber>\r\n  <interchangeNum>~BLAH00000000009999</interchangeNum>\r\n  <entryNumberType>IMP</entryNumberType>\r\n  <messageFormat>NX5901</messageFormat>\r\n  <messageId />\r\n  <messageBodyBase64>PGJvZHk+PERlY2xhcmF0aW9uIHhzaTpzY2hlbWFMb2NhdGlvbj0idXJuOndjbzpkYXRhbW9kZWw6VFc6Tlg1OTAxOlItMDEtMDAgLi4vbWFpbmRvYy9OWDU5MDEueHNkIiB4bWxucz0idXJuOndjbzpkYXRhbW9kZWw6VFc6Tlg1OTAxOlItMDEtMDAiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiLz48L2JvZHk+</messageBodyBase64>\r\n  <attachments>\r\n    <attachment>\r\n      <attachmentName>Attachment1</attachmentName>\r\n            \t\t\t\t\t<attachmentDataBase64>RG9jdW1lbnQxIGNvbnRlbnQgcGRm</attachmentDataBase64>\r\n            \t\t\t\t\t<attachmentFileType>pdf</attachmentFileType></attachment>\r\n    <attachment>\r\n      <attachmentName>Attachment2</attachmentName>\r\n            \t\t\t\t\t<attachmentDataBase64>RG9jdW1lbnQyIGNvbnRlbnQgeG1s</attachmentDataBase64>\r\n            \t\t\t\t\t<attachmentFileType>xml</attachmentFileType></attachment>\r\n  </attachments>\r\n  <clientRegistrationId />\r\n  <registrationConfiguration />\r\n</TWCPluginServiceSendRequest>"
			);
		}

		public void TestCreateTWCustomsForwarderManifestMessage()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.TWCustoms, "FHM", @"
<TWMessageInfo>
	<CompanyID>TST</CompanyID>
	<MailBox>mailbox</MailBox>
	<MessageType>FHM</MessageType>
	<PasswordType>TVA</PasswordType>
	<EntryNumber>0899900001</EntryNumber>
	<EntryNumberType>FHM</EntryNumberType>
</TWMessageInfo>
", @"<body><Declaration xsi:schemaLocation=""urn:wco:datamodel:TW:NX5901:R-01-00 ../maindoc/NX5901.xsd"" xmlns=""urn:wco:datamodel:TW:NX5901:R-01-00"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""/></body>", "<footer>");
			var message = CreateMessage(interchange);
			CreateMessageAttachment(message, "Attachment1.pdf", "APP", "Document1", "CUS", "Document1 content pdf");
			CreateMessageAttachment(message, "Attachment2.xml", "DAC", "Document2", "MSC", "Document2 content xml");
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.TWCustoms,
				"http://cargowise.com/ehub/products/TWCPluginRequest#TWCPluginServiceSendRequest",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TWCPluginServiceSendRequest xmlns=\"http://cargowise.com/ehub/products/TWCPluginRequest\">\r\n  <systemId>Dumrom</systemId>\r\n  <companyId>TST</companyId>\r\n  <staffCode></staffCode>\r\n  <mailbox>mailbox</mailbox>\r\n  <passwordType>TVA</passwordType>\r\n  <messageType>FHM</messageType>\r\n  <entryNumber>0899900001</entryNumber>\r\n  <interchangeNum>~BLAH00000000009999</interchangeNum>\r\n  <entryNumberType>FHM</entryNumberType>\r\n  <messageFormat>NX5901</messageFormat>\r\n  <messageId />\r\n  <messageBodyBase64>PGJvZHk+PERlY2xhcmF0aW9uIHhzaTpzY2hlbWFMb2NhdGlvbj0idXJuOndjbzpkYXRhbW9kZWw6VFc6Tlg1OTAxOlItMDEtMDAgLi4vbWFpbmRvYy9OWDU5MDEueHNkIiB4bWxucz0idXJuOndjbzpkYXRhbW9kZWw6VFc6Tlg1OTAxOlItMDEtMDAiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiLz48L2JvZHk+</messageBodyBase64>\r\n  <attachments>\r\n    <attachment>\r\n      <attachmentName>Attachment1</attachmentName>\r\n            \t\t\t\t\t<attachmentDataBase64>RG9jdW1lbnQxIGNvbnRlbnQgcGRm</attachmentDataBase64>\r\n            \t\t\t\t\t<attachmentFileType>pdf</attachmentFileType></attachment>\r\n    <attachment>\r\n      <attachmentName>Attachment2</attachmentName>\r\n            \t\t\t\t\t<attachmentDataBase64>RG9jdW1lbnQyIGNvbnRlbnQgeG1s</attachmentDataBase64>\r\n            \t\t\t\t\t<attachmentFileType>xml</attachmentFileType></attachment>\r\n  </attachments>\r\n  <clientRegistrationId />\r\n  <registrationConfiguration />\r\n</TWCPluginServiceSendRequest>"
			);
		}

		public void TestCreateTWCustomsLicensingMessage()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.TWCustoms, "NXM", @"
<TWMessageInfo>
	<CompanyID>TST</CompanyID>
	<MailBox>mailbox</MailBox>
	<MessageType>NXM</MessageType>
	<PasswordType>NXM</PasswordType>
	<EntryNumber>0899900001</EntryNumber>
	<EntryNumberType>NXM</EntryNumberType>
</TWMessageInfo>
", @"<body><Declaration xsi:schemaLocation=""urn:wco:datamodel:TW:NX903:R-01-00 ../maindoc/NX903.xsd"" xmlns=""urn:wco:datamodel:TW:NX903:R-01-00"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""/></body>", "<footer>");
			var message = CreateMessage(interchange);
			CreateMessageAttachment(message, "Attachment1.pdf", "APP", "Document1", "CUS", "Document1 content pdf");
			CreateMessageAttachment(message, "Attachment2.xml", "DAC", "Document2", "MSC", "Document2 content xml");
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.TWCustoms,
				"http://cargowise.com/ehub/products/TWCPluginRequest#TWCPluginServiceSendRequest",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TWCPluginServiceSendRequest xmlns=\"http://cargowise.com/ehub/products/TWCPluginRequest\">\r\n  <systemId>Dumrom</systemId>\r\n  <companyId>TST</companyId>\r\n  <staffCode></staffCode>\r\n  <mailbox>mailbox</mailbox>\r\n  <passwordType>NXM</passwordType>\r\n  <messageType>NXM</messageType>\r\n  <entryNumber>0899900001</entryNumber>\r\n  <interchangeNum>~BLAH00000000009999</interchangeNum>\r\n  <entryNumberType>NXM</entryNumberType>\r\n  <messageFormat>NX903</messageFormat>\r\n  <messageId />\r\n  <messageBodyBase64>PGJvZHk+PERlY2xhcmF0aW9uIHhzaTpzY2hlbWFMb2NhdGlvbj0idXJuOndjbzpkYXRhbW9kZWw6VFc6Tlg5MDM6Ui0wMS0wMCAuLi9tYWluZG9jL05YOTAzLnhzZCIgeG1sbnM9InVybjp3Y286ZGF0YW1vZGVsOlRXOk5YOTAzOlItMDEtMDAiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiLz48L2JvZHk+</messageBodyBase64>\r\n  <attachments>\r\n    <attachment>\r\n      <attachmentName>Attachment1</attachmentName>\r\n            \t\t\t\t\t<attachmentDataBase64>RG9jdW1lbnQxIGNvbnRlbnQgcGRm</attachmentDataBase64>\r\n            \t\t\t\t\t<attachmentFileType>pdf</attachmentFileType></attachment>\r\n    <attachment>\r\n      <attachmentName>Attachment2</attachmentName>\r\n            \t\t\t\t\t<attachmentDataBase64>RG9jdW1lbnQyIGNvbnRlbnQgeG1s</attachmentDataBase64>\r\n            \t\t\t\t\t<attachmentFileType>xml</attachmentFileType></attachment>\r\n  </attachments>\r\n  <clientRegistrationId />\r\n  <registrationConfiguration />\r\n</TWCPluginServiceSendRequest>"
			);
		}

		#endregion

		#region TestCreateUYCustomsMessageBuilder

		public void TestCreateUYCustomsMessage()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.UYCustoms, "UYC", @"
<Credentials>
	<UserName>6081</UserName>
	<Password>EncryptedPassword</Password>
</Credentials>
", @"<DAE xmlns=""http://www.aduanas.gub.uy/LUCIA/DAE""/>", "<footer>");
			CreateMessage(interchange);
			var notifier = new NotificationBuffer();
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.UYCustoms,
				"http://cargowise.com/xhub/products/UYCustoms#UYCustomsEnvelope",
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<UYCustomsEnvelope xmlns=\"http://cargowise.com/xhub/products/UYCustoms\">\r\n  <Credentials>\r\n    <UserName>6081</UserName>\r\n    <Password>EncryptedPassword</Password>\r\n  </Credentials>\r\n  <InboxPK />\r\n  <OutboxPK />\r\n  <MessageTrackingID />\r\n  <Sender>DummyFrom</Sender>\r\n  <Recipient>DummyTo</Recipient>\r\n  <MessageBodyBase64>PERBRSB4bWxucz0iaHR0cDovL3d3dy5hZHVhbmFzLmd1Yi51eS9MVUNJQS9EQUUiLz4=</MessageBodyBase64>\r\n</UYCustomsEnvelope>"
			);
		}

		#endregion

		#region TestCreateAUCustomseHubMessage

		public void TestCreateAUCustomseHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
					ApplicationCodeList.Codes.AUCMR,
					"",
					"DummyTo",
					"<header>",
					"VGhpcyBpcyBhIHRlc3QgbWVzc2FnZQ==",
					"<footer>",
					"",
					"",
					MessageSchemaType.Xml,
					ApplicationCodeList.Codes.AUCMR,
					EDIMessageSchemaNameList.Descriptions.AUCustoms,
					@"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Content>VkdocGN5QnBjeUJoSUhSbGMzUWdiV1Z6YzJGblpRPT0=</Content></AUCustoms>",
					expectedRecipientID: "AUCustoms");

			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndDiagnosticInfoNotReported(
					ApplicationCodeList.Codes.AUCMR,
					"",
					"DummyTo",
					"<header>",
					"VGhpcyBpcyBhIHRlc3QgbWVzc2FnZQ==",
					"<footer>",
					MessageSchemaType.Xml,
					ApplicationCodeList.Codes.AUCMR,
					EDIMessageSchemaNameList.Descriptions.AUCustoms,
					@"<AUCustoms xmlns=""http://cargowise.com/ehub/products/""><Reference>DummyFrom</Reference><Content>VkdocGN5QnBjeUJoSUhSbGMzUWdiV1Z6YzJGblpRPT0=</Content></AUCustoms>",
					expectedRecipientID: "AUCustoms");
		}

		#endregion

		public void TestCreateNEXeHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.AUCustomsNEXDOC,
				"NEX",
				"NEXDOCSTest",
				"",
				@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""><Header>Header</Header><Body><RexForwardOwnership><identification><rexNumber>REX0000028928<rexNumber></identification></RexForwardOwnership></Body></UniversalInterchange>",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.AUCustomsNEXDOC,
				EDIInterchangeTypeList.Descriptions.AUCustomsNEXDOC,
				@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""><Header>Header</Header><Body><RexForwardOwnership><identification><rexNumber>REX0000028928<rexNumber></identification></RexForwardOwnership></Body></UniversalInterchange>",
				expectedRecipientID: "NEXDOCSTest-REX");
		}

		#region TestCreateCanadianCustomseHubMessage

		public void TestCreateCanadianCustomsEHubMessage()
		{
			var interchange = CreateInterchange(
				applicationCode: ApplicationCodeList.Codes.CACustoms,
				type: "CAD",
				header: "<header>",
				body: "EdiFact message",
				footer: "<footer>",
				to: "INETCECPT");

			CreateMessage(interchange: interchange, type: "", subType: "");

			var notifier = new NotificationBuffer();
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(
				eHubMessage: eHubMessage,
				expectedSchemaType: MessageSchemaType.Xml,
				expectedApplicationCode: ApplicationCodeList.Codes.CACustoms,
				expectedSchemaName: EDIMessageSchemaNameList.Descriptions.CanadianCustoms,
				expectedStreamContent: string.Format(@"<CACustoms xmlns=""http://cargowise.com/ehub/products/canadiancustoms""><Reference>DummyFrom - INETCECPT</Reference><MessageId>{0}</MessageId><Content>PGhlYWRlcj5FZGlGYWN0IG1lc3NhZ2U8Zm9vdGVyPg==</Content></CACustoms>", interchange.EI_SessionGUID),
				expectedRecipientID: "CACustomsTest",
				expectedEmailSubject: "CAD",
				epxectedFileName: null);
		}

		public void TestCreateCanadianCustomsEHubMessage_InterchangeWithoutMessages()
		{
			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndDiagnosticInfoNotReported(
				applicationCode: ApplicationCodeList.Codes.CACustoms,
				interchangeType: "",
				interchangeTo: "INETCECPP",
				interchangeHeader: "<header>",
				interchangeBody: "EdiFact message",
				interchangeFooter: "<footer>",
				expectedSchemaType: MessageSchemaType.Xml,
				expectedApplicationCode: ApplicationCodeList.Codes.CACustoms,
				expectedSchemaName: EDIMessageSchemaNameList.Descriptions.CanadianCustoms,
				expectedStreamContent: "",
				expectedRecipientID: "CACustoms",
				expectedEmailSubject: "",
				expectedFileName: null
				);
		}

		#endregion

		#region TestCreateUSCustomseHubMessage

		public void TestCreateUSCustomseHubMessage()
		{
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USeManifest, "TST");
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USAMA, "TST");
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USAMS, "TST");
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USCustomsImport, "TST");
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USCustomsExport, "TST");
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USExportManifest, "TST");
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.StowPlan, "TST");

			var emptyMessageType = string.Empty;
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USeManifest, emptyMessageType);
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USAMA, emptyMessageType);
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USAMS, emptyMessageType);
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USCustomsImport, emptyMessageType);
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USCustomsExport, emptyMessageType);
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.USExportManifest, emptyMessageType);
			CreateUSCustomseHubMessageSucceed(ApplicationCodeList.Codes.StowPlan, emptyMessageType);

			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USeManifest);
			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USAMS);
			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USAMA);
			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USCustomsImport);
			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USCustomsExport);
			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USExportManifest);
			CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.StowPlan);
		}

		void CreateUSCustomseHubMessageSucceed(string applicationCode, string messageType)
		{
			CreateAndAsserteHubMessageSucceed(
				applicationCode,
				"",
				"DummyTo",
				"Header",
				"Body",
				"Footer",
				messageType,
				"",
				MessageSchemaType.FlatFile,
				applicationCode,
				messageType,
				"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Body]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>");
		}

		void CreateUSCustomseHubMessageFromInterchangeWithNoMessageSucceed(string applicationCode)
		{
			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndDiagnosticInfoNotReported(
				applicationCode,
				"",
				"DummyTo",
				"Header",
				"Body",
				"Footer",
				MessageSchemaType.FlatFile,
				applicationCode,
				"",
				"<USCustoms><Header><![CDATA[Header]]></Header><Body><![CDATA[Body]]></Body><Footer><![CDATA[Footer]]></Footer></USCustoms>");
		}

		#endregion

		#region TestCreateGBCustomseHubMessage

		public void TestCreateGBCustomseHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.GbCustomsDeclarationServices,
				"",
				"GBCustoms",
				@"<GBCustomsRequest><Provider>Direct</Provider><Service>New</Service><Credentials Key=""HYEAYA.GB123456789000.ABC"" /><JobNumber>B0001000</JobNumber></GBCustomsRequest>",
				gBCustomsInterchangeBodyContent,
				"Footer",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.GbCustomsDeclarationServices,
				EDIInterchangeTypeList.Descriptions.GBCustoms,
				expectedGBCustomseHubMessageContent,
				expectedRecipientID: "GBCustoms");
		}
		public void TestCreateGBCustomseHubMessageForJsonForGVMS()
		{
			string someJson = "{ some json here blah}";
			GvmsRunner(someJson, someJson);
		}
		public void TestCreateGBCustomseHubMessageForJsonForGVMSWithDirtyJson()
		{
			string someJson = "{ some json here blah that includes <xml> characters}";
			string someJsonEscaped = "{ some json here blah that includes &lt;xml&gt; characters}";
			GvmsRunner(someJson, someJsonEscaped);
		}

		void GvmsRunner(string someJsonIn, string someEscapedJsonExpected)
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.GbCustomsDeclarationServices,
				"",
				"GBCustoms",
				@"<GBCustomsRequest><Provider>GVMS</Provider><Service>Create</Service><Credentials Key=""HYEAYA.GB123456789000.ABC"" /><JobNumber>MAN00001</JobNumber></GBCustomsRequest>",
				someJsonIn,
				"Footer",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.GbCustomsDeclarationServices,
				EDIInterchangeTypeList.Descriptions.GBCustoms,
				 @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""><Header><GBCustomsRequest><Provider>GVMS</Provider><Service>Create</Service><Credentials Key=""HYEAYA.GB123456789000.ABC"" /><JobNumber>MAN00001</JobNumber></GBCustomsRequest></Header><Body>"
						+ someEscapedJsonExpected + "</Body></ns0:GBCustoms>",
				expectedRecipientID: "GBCustoms");
		}

		public const string gBCustomsInterchangeBodyContent = @"<md:MetaData xmlns:md=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:udt=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2""><md:WCODataModelVersionCode>3.6</md:WCODataModelVersionCode><md:WCOTypeName>DEC-DMS</md:WCOTypeName><md:ResponsibleCountryCode>GB</md:ResponsibleCountryCode><md:ResponsibleAgencyName>Agency ABC</md:ResponsibleAgencyName><md:AgencyAssignedCustomizationVersionCode>v1.2</md:AgencyAssignedCustomizationVersionCode><!--
		Import Declaration including:
		- DV1 elements
		- Quota / Preference (Add.Info with type ""TRR"", DutyTaxFee)
		- VAT transfer
		- Additional costs (DutyTaxFee)
		- Direct representation
		- Arrival transport means
		- Payer / Surety
		- UCR
		- Warehouse reference
		- CDIU document with quantity/amount
		- Special mention (Add.Info with type ""CUS"")
		- National classification
		- Relief amount (DutyTaxFee)
		- Method of payment
		- Supplementary units
		- Additional calculation units
		- Previous document
		--><Declaration><AcceptanceDateTime><udt:DateTimeString formatCode=""304"">20161207010101Z</udt:DateTimeString></AcceptanceDateTime><FunctionCode>9</FunctionCode><FunctionalReferenceID>DemoUK20161207_010</FunctionalReferenceID><TypeCode>IMZ</TypeCode><DeclarationOfficeID>0051</DeclarationOfficeID><TotalPackageQuantity>1</TotalPackageQuantity><Agent><ID>ZZ123456789001</ID><FunctionCode>2</FunctionCode></Agent><CurrencyExchange><!-- 1094 new section--><RateNumeric>1.234</RateNumeric></CurrencyExchange><Declarant><ID>ZZ123456789000</ID></Declarant><GoodsShipment><ExitDateTime><udt:DateTimeString formatCode=""304"">20161207010101Z</udt:DateTimeString></ExitDateTime><!-- 1094 --><TransactionNatureCode>1</TransactionNatureCode><Buyer><Name>Buyer name Part1Buyername Part2</Name><Address><CityName>Buyer City name</CityName><CountryCode>NL</CountryCode><Line>Buyerstreet Part1BuyerStreet Part2 7C</Line><PostcodeID>8603 AV</PostcodeID></Address></Buyer><Consignee><ID>ZZ123456789002</ID></Consignee><Consignment><ArrivalTransportMeans><Name>Titanic II</Name><TypeCode>1</TypeCode></ArrivalTransportMeans><GoodsLocation><Name>3016 DR, Loods 5</Name></GoodsLocation><LoadingLocation><!-- 1094 --><Name>Neverland</Name><ID>1234</ID></LoadingLocation><TransportEquipment><SequenceNumeric>1</SequenceNumeric><ID>CONTAINERNUMBER17</ID></TransportEquipment><TransportEquipment><SequenceNumeric>2</SequenceNumeric><ID>CONTAINERNUMBER22</ID></TransportEquipment></Consignment><DomesticDutyTaxParty><ID>ZZ123456789003</ID></DomesticDutyTaxParty><ExportCountry><ID>CA</ID></ExportCountry><GovernmentAgencyGoodsItem><SequenceNumeric>1</SequenceNumeric><StatisticalValueAmount>1234567</StatisticalValueAmount><AdditionalDocument><CategoryCode>I</CategoryCode><EffectiveDateTime><udt:DateTimeString formatCode=""304"">20130812091112Z</udt:DateTimeString></EffectiveDateTime><!-- 1094 --><ID>I003INVOERVERGEU</ID><!-- CDD-1094 an..70 --><Name>NAME_HERE</Name><!-- CDD-1094 ADDED --><TypeCode>003</TypeCode><LPCOExemptionCode>123</LPCOExemptionCode><!-- 1094 --></AdditionalDocument><AdditionalDocument><CategoryCode>N</CategoryCode><ID>N861UnivCertOrigin</ID><TypeCode>861</TypeCode></AdditionalDocument><AdditionalInformation><StatementCode>1</StatementCode><!-- not affiliated --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>3</StatementCode><!-- no price influence --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>5</StatementCode><!-- no approximate value --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>8</StatementCode><!-- special restrictions --><StatementDescription>Special Restrictions</StatementDescription><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>9</StatementCode><!-- no price conditions --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>11</StatementCode><!-- no royalties or license fees --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>13</StatementCode><!-- no other revenue --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>16</StatementCode><!-- customs decisions --><StatementDescription>11NL12345678901234</StatementDescription><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>17</StatementCode><!-- contract information --><StatementDescription>Contract 12123, 24-11-2011</StatementDescription><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>90010</StatementCode><StatementDescription>VERVOERDER: InterTrans</StatementDescription><StatementTypeCode>CUS</StatementTypeCode></AdditionalInformation><Commodity><Description>Inertial navigation systems</Description><Classification><ID>901420209000000000</ID><IdentificationTypeCode>SRZ</IdentificationTypeCode></Classification><Classification><ID>9002</ID><IdentificationTypeCode>GN</IdentificationTypeCode><!--Not representative for this commodity--></Classification><DutyTaxFee><AdValoremTaxBaseAmount currencyID=""EUR"">900</AdValoremTaxBaseAmount><DutyRegimeCode>
								100<!--Specified a Tariff Quota preference (not representative)--></DutyRegimeCode><TypeCode>B00</TypeCode><Payment><MethodCode>M</MethodCode></Payment></DutyTaxFee></Commodity><GovernmentProcedure><CurrentCode>40</CurrentCode><PreviousCode>91</PreviousCode></GovernmentProcedure><GovernmentProcedure><CurrentCode>C30</CurrentCode></GovernmentProcedure><Origin><CountryCode>US</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><MarksNumbersID>SHIPPING MARKS PART1 SHIPPING</MarksNumbersID><QuantityQuantity>9</QuantityQuantity><TypeCode>CT</TypeCode></Packaging><PreviousDocument><ID>X355ID13</ID><!-- 1094 --><LineNumeric>1</LineNumeric></PreviousDocument><ValuationAdjustment><AdditionCode>155</AdditionCode></ValuationAdjustment></GovernmentAgencyGoodsItem><Invoice><ID>INVOICENUMBER</ID><IssueDateTime><udt:DateTimeString formatCode=""304"">20130812091112Z</udt:DateTimeString></IssueDateTime><!-- 1094 --></Invoice><Payer><ID>ZZ123456789003</ID></Payer><Seller><Name>Seller name Part1Sellername Part2</Name><Address><CityName>Seller City name</CityName><CountryCode>MX</CountryCode><Line>Sellerstreet Part1SellerStreet Part2 7C</Line><PostcodeID>8603 AV</PostcodeID></Address></Seller><Surety><ID>ZZ123456789003</ID></Surety><TradeTerms><ConditionCode>CIP</ConditionCode><CountryRelationshipCode>158</CountryRelationshipCode><LocationName>Rotterdam</LocationName></TradeTerms><UCR><ID>UN1234567893123456789</ID><TraderAssignedReferenceID>P198R65Q29</TraderAssignedReferenceID></UCR><Warehouse><ID>A123456ZZ</ID></Warehouse></GoodsShipment></Declaration></md:MetaData>";

		const string expectedGBCustomseHubMessageContent = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""><Header><GBCustomsRequest><Provider>Direct</Provider><Service>New</Service><Credentials Key=""HYEAYA.GB123456789000.ABC"" /><JobNumber>B0001000</JobNumber></GBCustomsRequest></Header><Body><md:MetaData xmlns:md=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:udt=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2""><md:WCODataModelVersionCode>3.6</md:WCODataModelVersionCode><md:WCOTypeName>DEC-DMS</md:WCOTypeName><md:ResponsibleCountryCode>GB</md:ResponsibleCountryCode><md:ResponsibleAgencyName>Agency ABC</md:ResponsibleAgencyName><md:AgencyAssignedCustomizationVersionCode>v1.2</md:AgencyAssignedCustomizationVersionCode><!--
		Import Declaration including:
		- DV1 elements
		- Quota / Preference (Add.Info with type ""TRR"", DutyTaxFee)
		- VAT transfer
		- Additional costs (DutyTaxFee)
		- Direct representation
		- Arrival transport means
		- Payer / Surety
		- UCR
		- Warehouse reference
		- CDIU document with quantity/amount
		- Special mention (Add.Info with type ""CUS"")
		- National classification
		- Relief amount (DutyTaxFee)
		- Method of payment
		- Supplementary units
		- Additional calculation units
		- Previous document
		--><Declaration><AcceptanceDateTime><udt:DateTimeString formatCode=""304"">20161207010101Z</udt:DateTimeString></AcceptanceDateTime><FunctionCode>9</FunctionCode><FunctionalReferenceID>DemoUK20161207_010</FunctionalReferenceID><TypeCode>IMZ</TypeCode><DeclarationOfficeID>0051</DeclarationOfficeID><TotalPackageQuantity>1</TotalPackageQuantity><Agent><ID>ZZ123456789001</ID><FunctionCode>2</FunctionCode></Agent><CurrencyExchange><!-- 1094 new section--><RateNumeric>1.234</RateNumeric></CurrencyExchange><Declarant><ID>ZZ123456789000</ID></Declarant><GoodsShipment><ExitDateTime><udt:DateTimeString formatCode=""304"">20161207010101Z</udt:DateTimeString></ExitDateTime><!-- 1094 --><TransactionNatureCode>1</TransactionNatureCode><Buyer><Name>Buyer name Part1Buyername Part2</Name><Address><CityName>Buyer City name</CityName><CountryCode>NL</CountryCode><Line>Buyerstreet Part1BuyerStreet Part2 7C</Line><PostcodeID>8603 AV</PostcodeID></Address></Buyer><Consignee><ID>ZZ123456789002</ID></Consignee><Consignment><ArrivalTransportMeans><Name>Titanic II</Name><TypeCode>1</TypeCode></ArrivalTransportMeans><GoodsLocation><Name>3016 DR, Loods 5</Name></GoodsLocation><LoadingLocation><!-- 1094 --><Name>Neverland</Name><ID>1234</ID></LoadingLocation><TransportEquipment><SequenceNumeric>1</SequenceNumeric><ID>CONTAINERNUMBER17</ID></TransportEquipment><TransportEquipment><SequenceNumeric>2</SequenceNumeric><ID>CONTAINERNUMBER22</ID></TransportEquipment></Consignment><DomesticDutyTaxParty><ID>ZZ123456789003</ID></DomesticDutyTaxParty><ExportCountry><ID>CA</ID></ExportCountry><GovernmentAgencyGoodsItem><SequenceNumeric>1</SequenceNumeric><StatisticalValueAmount>1234567</StatisticalValueAmount><AdditionalDocument><CategoryCode>I</CategoryCode><EffectiveDateTime><udt:DateTimeString formatCode=""304"">20130812091112Z</udt:DateTimeString></EffectiveDateTime><!-- 1094 --><ID>I003INVOERVERGEU</ID><!-- CDD-1094 an..70 --><Name>NAME_HERE</Name><!-- CDD-1094 ADDED --><TypeCode>003</TypeCode><LPCOExemptionCode>123</LPCOExemptionCode><!-- 1094 --></AdditionalDocument><AdditionalDocument><CategoryCode>N</CategoryCode><ID>N861UnivCertOrigin</ID><TypeCode>861</TypeCode></AdditionalDocument><AdditionalInformation><StatementCode>1</StatementCode><!-- not affiliated --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>3</StatementCode><!-- no price influence --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>5</StatementCode><!-- no approximate value --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>8</StatementCode><!-- special restrictions --><StatementDescription>Special Restrictions</StatementDescription><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>9</StatementCode><!-- no price conditions --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>11</StatementCode><!-- no royalties or license fees --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>13</StatementCode><!-- no other revenue --><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>16</StatementCode><!-- customs decisions --><StatementDescription>11NL12345678901234</StatementDescription><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>17</StatementCode><!-- contract information --><StatementDescription>Contract 12123, 24-11-2011</StatementDescription><StatementTypeCode>ABC</StatementTypeCode></AdditionalInformation><AdditionalInformation><StatementCode>90010</StatementCode><StatementDescription>VERVOERDER: InterTrans</StatementDescription><StatementTypeCode>CUS</StatementTypeCode></AdditionalInformation><Commodity><Description>Inertial navigation systems</Description><Classification><ID>901420209000000000</ID><IdentificationTypeCode>SRZ</IdentificationTypeCode></Classification><Classification><ID>9002</ID><IdentificationTypeCode>GN</IdentificationTypeCode><!--Not representative for this commodity--></Classification><DutyTaxFee><AdValoremTaxBaseAmount currencyID=""EUR"">900</AdValoremTaxBaseAmount><DutyRegimeCode>
								100<!--Specified a Tariff Quota preference (not representative)--></DutyRegimeCode><TypeCode>B00</TypeCode><Payment><MethodCode>M</MethodCode></Payment></DutyTaxFee></Commodity><GovernmentProcedure><CurrentCode>40</CurrentCode><PreviousCode>91</PreviousCode></GovernmentProcedure><GovernmentProcedure><CurrentCode>C30</CurrentCode></GovernmentProcedure><Origin><CountryCode>US</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><MarksNumbersID>SHIPPING MARKS PART1 SHIPPING</MarksNumbersID><QuantityQuantity>9</QuantityQuantity><TypeCode>CT</TypeCode></Packaging><PreviousDocument><ID>X355ID13</ID><!-- 1094 --><LineNumeric>1</LineNumeric></PreviousDocument><ValuationAdjustment><AdditionCode>155</AdditionCode></ValuationAdjustment></GovernmentAgencyGoodsItem><Invoice><ID>INVOICENUMBER</ID><IssueDateTime><udt:DateTimeString formatCode=""304"">20130812091112Z</udt:DateTimeString></IssueDateTime><!-- 1094 --></Invoice><Payer><ID>ZZ123456789003</ID></Payer><Seller><Name>Seller name Part1Sellername Part2</Name><Address><CityName>Seller City name</CityName><CountryCode>MX</CountryCode><Line>Sellerstreet Part1SellerStreet Part2 7C</Line><PostcodeID>8603 AV</PostcodeID></Address></Seller><Surety><ID>ZZ123456789003</ID></Surety><TradeTerms><ConditionCode>CIP</ConditionCode><CountryRelationshipCode>158</CountryRelationshipCode><LocationName>Rotterdam</LocationName></TradeTerms><UCR><ID>UN1234567893123456789</ID><TraderAssignedReferenceID>P198R65Q29</TraderAssignedReferenceID></UCR><Warehouse><ID>A123456ZZ</ID></Warehouse></GoodsShipment></Declaration></md:MetaData></Body></ns0:GBCustoms>";

		#endregion

		#region TestCreateGEIeHubMessage

		public void TestCreateGEIeHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.GlobalElectronicInvoice,
				"",
				"GLOBAL_ELECTRONIC_INVOICING",
				"",
				@"<GlobalElectronicInvoicing xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing""><Header>Header</Header><Payload>Body</Payload></GlobalElectronicInvoicing>",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.GlobalElectronicInvoice,
				EDIInterchangeTypeList.Descriptions.GlobalElectronicInvoice,
				@"<GlobalElectronicInvoicing xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing""><Header>Header</Header><Payload>Body</Payload></GlobalElectronicInvoicing>",
				expectedRecipientID: "GLOBAL_ELECTRONIC_INVOICING");
		}

		#endregion

		#region TestCreateGEPeHubMessage

		public void TestCreateGEPeHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.GlobalElectronicPayment,
				"",
				"GLOBAL_ELECTRONIC_PAYMENT",
				"",
				@"<GlobalElectronicPayment xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicPayment""><Header>Header</Header><Payload>Body</Payload></GlobalElectronicPayment>",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.GlobalElectronicPayment,
				EDIInterchangeTypeList.Descriptions.GlobalElectronicPayment,
				@"<GlobalElectronicPayment xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicPayment""><Header>Header</Header><Payload>Body</Payload></GlobalElectronicPayment>",
				expectedRecipientID: "GLOBAL_ELECTRONIC_PAYMENT");
		}

		#endregion

		#region TestCreateUSDISeHubMessage

		public void TestCreateUSDISeHubMessage()
		{
			CreateUSDISeHubMessageSucceed(ApplicationCodeList.Codes.USCustomsDIS);
			CreateUSDISeHubMessageFromInterchangeWithNoMessageSucceed(ApplicationCodeList.Codes.USCustomsDIS);

			CreateUSDISSubmissioneHubMessageIncludingAttachmentsSucceed();
			CreateUSDISSubmissioneHubMessageIncludingAttachmentsFailed();
			CreateAndAsserteHubMessageFailDueToMessageRequired(ApplicationCodeList.Codes.USCustomsDIS, EDIInterchangeTypeList.Codes.USDISSubmission);
		}

		void CreateUSDISeHubMessageSucceed(string applicationCode)
		{
			CreateAndAsserteHubMessageSucceed(
				applicationCode,
				"",
				"DummyTo",
				"Header",
				"Body",
				"Footer",
				"",
				"",
				MessageSchemaType.Xml,
				applicationCode,
				EDIMessageSchemaNameList.Descriptions.USDDIS,
				"Body",
				expectedRecipientID: "USDIS");
		}

		void CreateUSDISeHubMessageFromInterchangeWithNoMessageSucceed(string applicationCode)
		{
			CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndNoDiagnosticInfoReportedIfApplicationDoesNotRequireMessage(
				applicationCode,
				"",
				"DummyTo",
				"Header",
				"Body",
				"Footer",
				MessageSchemaType.Xml,
				applicationCode,
				EDIMessageSchemaNameList.Descriptions.USDDIS,
				"Body",
				expectedRecipientID: "USDIS");
		}

		void CreateUSDISSubmissioneHubMessageIncludingAttachmentsSucceed()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.USCustomsDIS, EDIInterchangeTypeList.Codes.USDISSubmission, "<header>", "", "<footer>");
			var message = CreateMessage(interchange);
			var attachment1 = CreateMessageAttachment(message, "Attachment1.pdf", "APP", "Document1", "DIS", "Document1 content pdf");
			var attachment2 = CreateMessageAttachment(message, "Attachment2.pdf", "APP", "Document2", "DIS", "Document2 content pdf");
			interchange.EI_BodyText = WrapAttachmentPalceHolder(attachment1.PK.ToString()) + System.Environment.NewLine + WrapAttachmentPalceHolder(attachment2.PK.ToString());
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			var expectedMessageStreamContent = "RG9jdW1lbnQxIGNvbnRlbnQgcGRm" + System.Environment.NewLine + "RG9jdW1lbnQyIGNvbnRlbnQgcGRm" + System.Environment.NewLine;
			AsserteHubMessageContent(eHubMessage, MessageSchemaType.Xml, ApplicationCodeList.Codes.USCustomsDIS, EDIMessageSchemaNameList.Descriptions.USDDIS, expectedMessageStreamContent, expectedRecipientID: "USDIS");
		}

		void CreateUSDISSubmissioneHubMessageIncludingAttachmentsFailed()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.USCustomsDIS, EDIInterchangeTypeList.Codes.USDISSubmission, "<header>", "", "<footer>");
			var message = CreateMessage(interchange);
			var attachment1 = CreateMessageAttachmentWithoutStorage(message, "AttachmentFoobar.pdf", "APP");

			interchange.EI_BodyText = WrapAttachmentPalceHolder(attachment1.PK.ToString()) + System.Environment.NewLine;
			var notifier = new NotificationBuffer();
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);
			AssertEquals(eHubMessage, null);
		}

		string WrapAttachmentPalceHolder(string input)
		{
			return EHubMessageBuilderForUSDSubmission.AttachmentPlaceholderStart + input + EHubMessageBuilderForUSDSubmission.AttachmentPlaceholderEnd;
		}

		#endregion

		#region TestCreateTELeHubMessage

		public void TestCreateTELeHubMessage()
		{
			var messageData = new byte[] { 0x40, 0xFB, 0x2F, 0x30, 0xE7, 0x5C, 0x4D, 0x21, 0xB3, 0x9B, 0x7C, 0x91, 0x2D, 0x16, 0x96, 0xE0 };
			var messageBody = "<ProtobufData>" + Convert.ToBase64String(messageData) + "</ProtobufData>";
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.Telematics,
				string.Empty,
				"DummyTo",
				string.Empty,
				messageBody,
				string.Empty,
				EDIInterchangeTypeList.Codes.Telematics,
				TelematicsMessageList.Codes.ProtobufData,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.Telematics,
				string.Empty,
				@"
<?xml version=""1.0"" encoding=""utf-8""?>
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>DummyFrom</SenderID>
    <RecipientID>DummyTo</RecipientID>
  </Header>
  <Body>
    <ProtobufData>QPsvMOdcTSGzm3yRLRaW4A==</ProtobufData>
  </Body>
</ns0:TelematicsInterchange>".TrimStart());
		}

		public void TestCreateTELXmleHubMessage()
		{
			// Arrange
			var interchange = CreateInterchange(
				ApplicationCodeList.Codes.Telematics,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty);

			var messages = new[]
			{
				"<TelematicsXmlData>\r\n      <DataFlowReadyMessage CargoWiseOneLicense=\"MyEdiLicence\" />\r\n    </TelematicsXmlData>",
				"<TelematicsXmlData>\r\n      <ServerRegistrationRequestMessage EHubId=\"TELMIDSERV\" />\r\n    </TelematicsXmlData>",
			};
			foreach (var message in messages)
			{
				CreateMessage(interchange, EDIInterchangeTypeList.Codes.Telematics, TelematicsMessageList.Codes.TelematicsXmlData)
					.EM_MessageText = message;
			}

			var notifier = new NotificationBuffer();

			// Act
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			// Assert
			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(
				eHubMessage,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.Telematics,
				string.Empty,
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>DummyFrom</SenderID>
    <RecipientID>DummyTo</RecipientID>
  </Header>
  <Body>
    <TelematicsXmlData>
      <DataFlowReadyMessage CargoWiseOneLicense=""MyEdiLicence"" />
    </TelematicsXmlData>
    <TelematicsXmlData>
      <ServerRegistrationRequestMessage EHubId=""TELMIDSERV"" />
    </TelematicsXmlData>
  </Body>
</ns0:TelematicsInterchange>");
		}
		#endregion

		#region TestCreateEHINudgeURLUpdateHubMessage

		public void TestCreateEHINudgeURLUpdateHubMessage()
		{
			var url = "<eHubRegistryUpdate xmlns=\"http://www.cargowise.com/ehub/Schemas/eHubRegistryUpdate\"><EHINudgeURL>http://test.url/NudgeServiceTask</EHINudgeURL></eHubRegistryUpdate>";
			var interchange = CreateInterchange(ApplicationCodeList.Codes.eHub, EDIInterchangeTypeList.Codes.EHubRegistryUpdate, string.Empty, url);
			var notifier = new NotificationBuffer();
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);
			AsserteHubMessageContent(eHubMessage, MessageSchemaType.Xml, ApplicationCodeList.Codes.eHub, EDIInterchangeTypeList.Descriptions.EHubRegistryUpdate, url);
		}

		#endregion

		#region TestCreateFCFeHubMessage

		public void TestCreateFCFeHubMessage()
		{
			var interchange = CreateInterchange(
				ApplicationCodeList.Codes.eHub,
				EDIInterchangeTypeList.Codes.ForwarderConfiguration,
				string.Empty,
				@"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TWCustomsSubscribers"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""WTLCTU"">
    <Group Type=""Company"" Reference=""DTW"">
      <Group Type=""MailBoxID"" Reference=""TBK1079"" Status=""VAL"">
        <Item Name=""Platform"">FHM</Item>
        <Item Name=""ReceiveAutomatically"">1</Item>
        <Credential>
          <UserName>TVCBBKTWTPE01079-TEST</UserName>
          <Password>…</Password>
        </Credential>
        <Certificate Name=""Certificate"">
          <File>…<File>
          <Passphrase>…</Passphrase>
        </Certificate>
      </Group>
    </Group>
  </Group>
</Configuration>",
				string.Empty);

			var notifier = new NotificationBuffer();
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(
				eHubMessage,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.eHub,
				"http://www.wisetechglobal.com/Schemas/Configuration#Configuration",
				@"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""TWCustomsSubscribers"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""WTLCTU"">
    <Group Type=""Company"" Reference=""DTW"">
      <Group Type=""MailBoxID"" Reference=""TBK1079"" Status=""VAL"">
        <Item Name=""Platform"">FHM</Item>
        <Item Name=""ReceiveAutomatically"">1</Item>
        <Credential>
          <UserName>TVCBBKTWTPE01079-TEST</UserName>
          <Password>…</Password>
        </Credential>
        <Certificate Name=""Certificate"">
          <File>…<File>
          <Passphrase>…</Passphrase>
        </Certificate>
      </Group>
    </Group>
  </Group>
</Configuration>");
		}

		#endregion

		#region TestCreateGenericMessageDeliveryeHubMessage

		public void TestCreateGenericMessageDeliveryeHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.GenericMessageDelivery,
				"XXX",
				"DummyTo",
				string.Empty,
				"<ns0:SomeRootElement xmlns:ns0=\"http://SomeSchema/Test\"><Description>This is a dummy message that will be added to the body of the generic message interchange</Description></ns0:SomeRootElement>",
				string.Empty,
				EDIInterchangeTypeList.Codes.GenericMessageDelivery,
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.GenericMessageDelivery,
				EDIInterchangeTypeList.Descriptions.GenericMessageDelivery,
				@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header><SenderID>DummyFrom</SenderID><RecipientID>DummyTo</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber>~BLAH00000000009999</InterchangeNumber></Header><Body><ns0:SomeRootElement xmlns:ns0=""http://SomeSchema/Test""><Description>This is a dummy message that will be added to the body of the generic message interchange</Description></ns0:SomeRootElement></Body></ns0:GenericMessageInterchange>".TrimStart());
		}

		public void TestCreateGenericMessageDeliveryeHubMessageForNonXmlBody()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.GenericMessageDelivery,
				"XXX",
				"DummyTo",
				string.Empty,
				"This text is an unformatted string not in XML format",
				string.Empty,
				EDIInterchangeTypeList.Codes.GenericMessageDelivery,
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.GenericMessageDelivery,
				EDIInterchangeTypeList.Descriptions.GenericMessageDelivery,
				@"<ns0:GenericMessageInterchange xmlns:ns0=""http://cargowise.com/ehub/core/genericmessagedelivery""><Header><SenderID>DummyFrom</SenderID><RecipientID>DummyTo</RecipientID><InterchangeType>XXX</InterchangeType><InterchangeNumber>~BLAH00000000009999</InterchangeNumber></Header><Body>This text is an unformatted string not in XML format</Body></ns0:GenericMessageInterchange>".TrimStart());
		}

		#endregion

		#region TestCreateITCeHubMessage

		public void TestCreateITCeHubMessage()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.ITCustoms,
				"",
				"ITCustoms",
				@"<ITMessage>
					<MessageType>U</MessageType>
					<FileName>200119.ULR</FileName>
					<eHubTrackingIDFromSentInterchange>abcd</eHubTrackingIDFromSentInterchange>
					<Header> first line from the response message </Header>
				</ITMessage>",
				"Body<",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.ITCustoms,
				EDIInterchangeTypeList.Descriptions.ITCustoms,
				@"<ITMessage xmlns:ns=""http://cargowise.com/ehub/products/ITCustoms""><MessageType>U</MessageType><FileName>200119.ULR</FileName><eHubTrackingIDFromSentInterchange>abcd</eHubTrackingIDFromSentInterchange><Header> first line from the response message </Header><Body>Body&lt;</Body></ITMessage>",
				expectedRecipientID: "ITCustoms");
		}

		public void TestCreateITCeHubMessage_MessageBodyIsXml()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.ITCustoms,
				"",
				"ITCustoms",
				@"<ITMessage>
					<MessageType>U</MessageType>
					<FileName>200119.ULR</FileName>
					<eHubTrackingIDFromSentInterchange>abcd</eHubTrackingIDFromSentInterchange>
					<Header> first line from the response message </Header>
				</ITMessage>",
				"<richiesta_esito><dichiarazione><num_reg>1111</num_reg><cod_uff_dog>279100</cod_uff_dog><cod_reg>4</cod_reg><anno_reg>2021</anno_reg></dichiarazione></richiesta_esito>",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.ITCustoms,
				EDIInterchangeTypeList.Descriptions.ITCustoms,
				@"<ITMessage xmlns:ns=""http://cargowise.com/ehub/products/ITCustoms""><MessageType>U</MessageType><FileName>200119.ULR</FileName><eHubTrackingIDFromSentInterchange>abcd</eHubTrackingIDFromSentInterchange><Header> first line from the response message </Header><Body><richiesta_esito><dichiarazione><num_reg>1111</num_reg><cod_uff_dog>279100</cod_uff_dog><cod_reg>4</cod_reg><anno_reg>2021</anno_reg></dichiarazione></richiesta_esito></Body></ITMessage>",
				expectedRecipientID: "ITCustoms");
		}

		#endregion

		#region TestCreateESCeHubMessage

		public void TestCreateESCeHubMessageXML()
		{
			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.ESCustomsMessage,
				"EXP",
				"ESCustomsSOAP",
				@"<?xml version='1.0' encoding='utf - 8'?>
<Headers>
	<BrokerCode>JRR</BrokerCode>
	<CertificateName>test</CertificateName>
	<CertificateThumbPrint>D36659B690BD3539E6B545594F854D69EE959BBF</CertificateThumbPrint>
	<EntryReferenceNumber>0ESA12345678-B00180800</EntryReferenceNumber>
	<TestMessage>N</TestMessage>
	<Service>PreDeclaIncompletaV1Service</Service>
	<Operation>PreDeclaIncompletaV1Service</Operation>
</Headers>",
				@"<?xml version='1.0' encoding='utf-8'?>
<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:imp='https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ImportacionCompletaV1Ent.xsd'>
<soapenv:Header />
	<soapenv:Body>
		<imp:ImportacionCompletaV1Ent endPoint = '?'>
			<SegmentosDeServicio Id = '20170224104136241208' fecha = '20170224' hora = '104136' Test = ''/>
			<TheRest>Blah</TheRest>
		</imp:ImportacionCompletaV1Ent>
	</soapenv:Body>
</soapenv:Envelope>",
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.ESCustomsMessage,
				EDIInterchangeTypeList.Descriptions.ESCustoms,
				@"<ns0:ESCustoms xmlns:ns0=""http://cargowise.com/ehub/products/ESCustoms""><Headers><BrokerCode>JRR</BrokerCode><CertificateName>test</CertificateName><CertificateThumbPrint>D36659B690BD3539E6B545594F854D69EE959BBF</CertificateThumbPrint><EntryReferenceNumber>0ESA12345678-B00180800</EntryReferenceNumber><TestMessage>N</TestMessage><Service>PreDeclaIncompletaV1Service</Service><Operation>PreDeclaIncompletaV1Service</Operation><InterchangeType>EXP</InterchangeType></Headers><Body><soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:imp=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ImportacionCompletaV1Ent.xsd""><soapenv:Header /><soapenv:Body><imp:ImportacionCompletaV1Ent endPoint=""?""><SegmentosDeServicio Id=""20170224104136241208"" fecha=""20170224"" hora=""104136"" Test="""" /><TheRest>Blah</TheRest></imp:ImportacionCompletaV1Ent></soapenv:Body></soapenv:Envelope></Body></ns0:ESCustoms>",
				expectedRecipientID: "ESCustomsSOAP");
		}

		public void TestCreateESCeHubMessageEDIFACT()
		{
			var edifactMessageContent = @"VIA=WTGCW1ES&DAT=UNB%2BUNOA:1%2B:ZZ%2BAEATADUE:ZZ%2B210105:1949%2B%2B%2B54%2B%2B%26EE'UNH%2B54%2BCUSDEC:1:96B:UN:AVI'BGM%2BAVI%2BAPCREF%2B9'CST%2B%2B:149:141%2B:113:141'LOC%2B8%2B9998::148%2B000002::148'GIS%2B0:109:141'GIS%2BA:42:148'GIS%2B0:181:148'RFF%2BAAE:21E000999912345678'NAD%2BDT%2BFR12345678A::148%2B%2BSPAINS'MOA%2BZZZ::EUR'UNS%2BD'UNS%2BS'CNT%2B5:0'CNT%2B11:0'UNT%2B15%2B54'UNZ%2B1%2B54'&FIR=MIIFSQYJKoZIhvcNAQcCoIIFOjCCBTYCAQExDzANBglghkgBZQMEAgEFADALBgkqhkiG9w0BBwGgggPrMIID5zCCAs%2BgAwIBAgIISC6QKk2pE8kwDQYJKoZIhvcNAQELBQAwbDELMAkGA1UEBhMCSVQxHTAbBgNVBAoTFEFnZW56aWEgZGVsbGUgRG9nYW5lMRwwGgYDVQQLExNTZXJ2aXppbyBUZWxlbWF0aWNvMSAwHgYDVQQDExdDQSBBZ2VuemlhIGRlbGxlIERvZ2FuZTAeFw0xNzA3MjEwODE0MTVaFw0yMDA3MjEwODI0MDBaMGcxCzAJBgNVBAYTAklUMR0wGwYDVQQKDBRBZ2VuemlhIGRlbGxlIERvZ2FuZTEfMB0GA1UECwwWU2Vydml6aSBBdXRlbnRpY2F6aW9uZTEYMBYGA1UEAwwPMTMxNDk2MDAxNTAtMDAzMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCDc98IEYWAhDgTuzEV1dMAo2RMQMTEoP8P%2FIGumnrEOd8hA0IuYANVkQUnQbXiBR0SHDQsLrcC92sJ%2BUX2IWD22ea9vRhY8TBq4MEBdPoadEICPP1IOAr4DioTKkUz3soY%2Fd5ySKgmmJqhu%2Fv%2BI37aoDrJrQmDTUhl%2BoItgRUkLwIDAQABo4IBFDCCARAwHwYDVR0jBBgwFoAUKdSjQ3uS8RNyA93kg8fUVOrBFXcwEwYDVR0lBAwwCgYIKwYBBQUHAwIwgagGA1UdHwSBoDCBnTCBmqCBl6CBlIaBkWxkYXA6Ly9jYWRzLmRvZ2FuZS5maW5hbnplLml0L0NOPUNBJTIwQWdlbnppYSUyMGRlbGxlJTIwRG9nYW5lLE9VPVNlcnZpemlvJTIwVGVsZW1hdGljbyxPPUFnZW56aWElMjBkZWxsZSUyMERvZ2FuZSxDPWl0P2NlcnRpZmljYXRlUmV2b2NhdGlvbkxpc3QwHQYDVR0OBBYEFFcCfVI3YXOW9zK3ZTvxWSmxyUTWMA4GA1UdDwEB%2FwQEAwIFoDANBgkqhkiG9w0BAQsFAAOCAQEAMmSp00P3r0CFjv%2BcSJ%2Ffj2p%2Fl8XoFoj7oQZlygHIOSy50%2FdEFEQ5B%2B0P%2BFuIHo4zp8tAlJS0z5y9NQzSqlJpms4bw9L9v2v429N23LSXC1Gxh8pd%2BnFTs%2Bpw%2BLh7SLputtsdUAWnJ8Sdp7R47wvG2IDsTFEeIkRD4IRfwgUyGdaTuOaKj4TR%2Bx6lBCnjFeNbxS05KwQ7s7RaJP5MYBvPjEMEs9SuA7gmkBE7v0JK4PvXN4N8%2BEsJqt4z2VHInsDQ3b1ZksfOzqCRqiDfuoDObb8EH14NSlyFs9kBaH6uFG%2Fego2lp8LeiwEz%2F8EqX7NHr9km8sJ06CbvalGv6JFmVTGCASIwggEeAgEBMHgwbDELMAkGA1UEBhMCSVQxHTAbBgNVBAoTFEFnZW56aWEgZGVsbGUgRG9nYW5lMRwwGgYDVQQLExNTZXJ2aXppbyBUZWxlbWF0aWNvMSAwHgYDVQQDExdDQSBBZ2VuemlhIGRlbGxlIERvZ2FuZQIISC6QKk2pE8kwDQYJYIZIAWUDBAIBBQAwDQYJKoZIhvcNAQEBBQAEgYB9wsynPbtLSq1ZPHGPBiIavvuE0gD8htLhiW%2Bm7DYaiX55gX%2FUDQgtOW7FIrEcKcAeNjfBjJk0f%2Fww6PilhFyXsn%2B5su3k6K50MTTkG2vHR%2FRw2tpXmMEfB16HrRyherqN9ZUGZ9QJvU7j6kU0aezIjLQyyhg0Nkn7N7TwlFIl1w==";
			var encodedMessageContent = edifactMessageContent.Replace("&", "&amp;");

			CreateAndAsserteHubMessageSucceed(
				ApplicationCodeList.Codes.ESCustomsMessage,
				"EXP",
				"ESCustomsEDIFACT",
				@"<?xml version='1.0' encoding='utf - 8'?>
<Headers>
	<BrokerCode>JRR</BrokerCode>
	<CertificateName>test</CertificateName>
	<CertificateThumbPrint>D36659B690BD3539E6B545594F854D69EE959BBF</CertificateThumbPrint>
	<EntryReferenceNumber>0ESA12345678-B00180800</EntryReferenceNumber>
	<TestMessage>N</TestMessage>
	<Service>PreDeclaIncompletaV1Service</Service>
	<Operation>PreDeclaIncompletaV1Service</Operation>
</Headers>",
				edifactMessageContent,
				"",
				"",
				"",
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.ESCustomsMessage,
				EDIInterchangeTypeList.Descriptions.ESCustoms,
				$@"<ns0:ESCustoms xmlns:ns0=""http://cargowise.com/ehub/products/ESCustoms""><Headers><BrokerCode>JRR</BrokerCode><CertificateName>test</CertificateName><CertificateThumbPrint>D36659B690BD3539E6B545594F854D69EE959BBF</CertificateThumbPrint><EntryReferenceNumber>0ESA12345678-B00180800</EntryReferenceNumber><TestMessage>N</TestMessage><Service>PreDeclaIncompletaV1Service</Service><Operation>PreDeclaIncompletaV1Service</Operation><InterchangeType>EXP</InterchangeType></Headers><Body>{encodedMessageContent}</Body></ns0:ESCustoms>",
				expectedRecipientID: "ESCustomsEDIFACT");
		}

		#endregion

		#region TestCreateTReHubMessage

		public void TestCreateTReHubMessage()
		{
			byte[] signedMessageData = new byte[] { 0x00, 0x01, 0xFF, 0x30, 0x80, 0x01, 0x02 };
			CreateAndAsserteHubMessageSucceed
			(
				ApplicationCodeList.Codes.TRCustoms,
				"TRO",
				"TROCustoms",
				string.Empty,
				signedMessageData,
				string.Empty,
				string.Empty,
				string.Empty,
				MessageSchemaType.Xml,
				ApplicationCodeList.Codes.TRCustoms,
				EDIInterchangeTypeList.Descriptions.TRCustomsGlobalManifest,
				FormattableString.Invariant($@"
<?xml version=""1.0"" encoding=""utf-8""?>
<TRCustomsEnvelope xmlns=""http://cargowise.com/xhub/products/TRCustoms"">
  <MessageBodyBase64>{Convert.ToBase64String(signedMessageData)}</MessageBodyBase64>
</TRCustomsEnvelope>
				").Trim(),
				expectedRecipientID: "TROCustoms"
			);
		}

		#endregion

		public void TestCreateEDIMessageSupportAttachmentNullRefference()
		{
			var interchange = CreateInterchange(ApplicationCodeList.Codes.USCustomsDIS,
				EDIInterchangeTypeList.Codes.USDISSubmission,
				"Header", "Body", "Footer", to: "DummyTo");

			var message = CreateMessage(interchange, "", "");
			message.EM_LinkedObject = (BusinessObject)Factory.New<Integration.Freight.ICommonShipment>();

			var docManager = message.EM_LinkedObject as IDocManagerSupport;
			var documentBytes = Encoding.UTF8.GetBytes("Document1 content pdf");
			var eDoc1 = docManager.DocManagerInfo.AddFileOrDocument(documentBytes, "Document1", "DIS");

			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_FileName = "Attachment1.pdf";
			attachment.EG_EdiMsgDocType = "APP";
			attachment.EG_StorageDocsGuid = eDoc1.UniqueKey;

			interchange.EI_BodyText = WrapAttachmentPalceHolder(attachment.PK.ToString());

			message.EM_LinkedObject = Factory.New<JobRequiredDocumentAddInfo>();

			var notifier = new NotificationBuffer();

			AssertNoExceptionThrown(() => { EHubMessageDirector.CreateMessage(interchange, notifier); });
		}

		#region Succeed

		void CreateAndAsserteHubMessageSucceed(string applicationCode, string interchangeType, string interchangeTo, string interchangeHeader, string interchangeBody, string interchangeFooter, string messageType, string messageSubType, MessageSchemaType expectedSchemaType, string expectedApplicationCode, string expectedSchemaName, string expectedStreamContent, string expectedEmailSubject = "", string expectedFileName = "", string expectedRecipientID = "")
		{
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter, to: interchangeTo);
			CreateMessage(interchange, messageType, messageSubType);
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage, expectedSchemaType, expectedApplicationCode, expectedSchemaName, expectedStreamContent, expectedEmailSubject, expectedFileName, expectedRecipientID);
		}

		void CreateAndAsserteHubMessageSucceed(string applicationCode, string interchangeType, string interchangeTo, string interchangeHeader, byte[] interchangeBody, string interchangeFooter, string messageType, string messageSubType, MessageSchemaType expectedSchemaType, string expectedApplicationCode, string expectedSchemaName, string expectedStreamContent, string expectedEmailSubject = "", string expectedFileName = "", string expectedRecipientID = "")
		{
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter, to: interchangeTo);
			CreateMessage(interchange, messageType, messageSubType);
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage, expectedSchemaType, expectedApplicationCode, expectedSchemaName, expectedStreamContent, expectedEmailSubject, expectedFileName, expectedRecipientID);
		}

		void CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndDiagnosticInfoNotReported(string applicationCode, string interchangeType, string interchangeTo, string interchangeHeader, string interchangeBody, string interchangeFooter, MessageSchemaType expectedSchemaType, string expectedApplicationCode, string expectedSchemaName, string expectedStreamContent, string expectedEmailSubject = "", string expectedFileName = "", string expectedRecipientID = "")
		{
			ErrorReporter.Clear();
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter, to: interchangeTo);
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			var serviceName = interchange.TransportModeDescription;
			var expectedInterchangeNotes = string.Format("Cannot create {0} message: Interchange(Number: {1}, Tracking ID: {2}, Application Code: {3}) should contain at least one message, but it contains no messages", serviceName, dummyInterchangeNum, dummySessionID, applicationCode);
			var expectedNotifications = "Warning: " + expectedInterchangeNotes;
			AssertInterchangeNotesAndNotifications(interchange, notifier, expectedInterchangeNotes, expectedNotifications);
			AssertNull(eHubMessage);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		void CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndNoDiagnosticInfoReportedIfApplicationDoesNotRequireMessage(string applicationCode, string interchangeType, string interchangeTo, string interchangeHeader, string interchangeBody, string interchangeFooter, MessageSchemaType expectedSchemaType, string expectedApplicationCode, string expectedSchemaName, string expectedStreamContent, string expectedEmailSubject = "", string expectedFileName = "", string expectedRecipientID = "")
		{
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter, to: interchangeTo);
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage, expectedSchemaType, expectedApplicationCode, expectedSchemaName, expectedStreamContent, expectedEmailSubject, expectedFileName, expectedRecipientID);
		}

		void CreateAndAsserteHubMessageFromInterchangeWithNoMessageSucceedAndNoDiagnosticInfoReported(string applicationCode, string interchangeType, string interchangeHeader, string interchangeBody, string interchangeFooter, MessageSchemaType expectedSchemaType, string expectedApplicationCode, string expectedSchemaName, string expectedStreamContent, string expectedEmailSubject = "", string expectedFileName = "", string expectedRecipientID = "")
		{
			ErrorReporter.Clear();
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter);
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier);
			AsserteHubMessageContent(eHubMessage, expectedSchemaType, expectedApplicationCode, expectedSchemaName, expectedStreamContent, expectedEmailSubject, expectedFileName, expectedRecipientID);
			AssertEquals("ErrorReporter.LastMessageReported", "", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Fail

		void CreateAndAsserteHubMessageFail(string applicationCode, string interchangeType, string interchangeHeader, string interchangeBody, string interchangeFooter, string messageType, string messageSubType, string expectedInterchangeNotes = "", string expectedNotifications = "")
		{
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter);
			CreateMessage(interchange, messageType, messageSubType);
			var notifier = new NotificationBuffer();
			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier, expectedInterchangeNotes, expectedNotifications);
			AssertNull("eHubMessage", eHubMessage);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		void CreateAndAsserteHubBuilderFail(string applicationCode, string interchangeType, string interchangeHeader, string interchangeBody, string interchangeFooter, string messageType, string messageSubType, string expectedInterchangeNotes = "", string expectedNotifications = "")
		{
			var interchange = CreateInterchange(applicationCode, interchangeType, interchangeHeader, interchangeBody, interchangeFooter);
			var message = CreateMessage(interchange, messageType, messageSubType);
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);

			AssertInterchangeNotesAndNotifications(interchange, notifier, expectedInterchangeNotes, expectedNotifications);
			AssertNull("eHubMessage", eHubMessage);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		void CreateAndAsserteHubMessageFailDueToMessageRequired(string applicationCode, string interchangeType = "")
		{
			var interchange = CreateInterchange(applicationCode, interchangeType, "Header", "Body", "Footer");
			var notifier = new NotificationBuffer();

			var eHubMessage = EHubMessageDirector.CreateMessage(interchange, notifier);
			var serviceName = interchange.TransportModeDescription;
			var expectedInterchangeNotes = string.Format("Cannot create {0} message: Interchange(Number: {1}, Tracking ID: {2}, Application Code: {3}) should contain at least one message, but it contains no messages", serviceName, dummyInterchangeNum, dummySessionID, applicationCode);
			var expectedNotifications = "Warning: " + expectedInterchangeNotes;
			AssertInterchangeNotesAndNotifications(interchange, notifier, expectedInterchangeNotes, expectedNotifications);
			AssertNull("eHubMessage", eHubMessage);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		#endregion

		#region Assert

		void AssertInterchangeNotesAndNotifications(EDIInterchange interchange, NotificationBuffer notifier, string expectedInterchangeNotes = "", string expectedNotifications = "")
		{
			var errorNotes = interchange.Notes.FindByDescription("eHub Error");
			AssertEquals("interchange notes with error", expectedInterchangeNotes, string.Join(System.Environment.NewLine, errorNotes.Select(n => n.ST_NoteDataAsText).ToArray()));
			AssertEquals("notifications", expectedNotifications, notifier.AsString.Trim());
		}

		void AsserteHubMessageContent(IeHubMessage eHubMessage, MessageSchemaType expectedSchemaType, string expectedApplicationCode, string expectedSchemaName, string expectedStreamContent, string expectedEmailSubject = "", string epxectedFileName = "", string expectedRecipientID = "")
		{
			AssertNotNull("eHubMessage should not be null", eHubMessage);
			AssertEquals("eHubMessage.TrackingID", dummySessionID, eHubMessage.TrackingID.ToString());
			AssertEquals("eHubMessage.SenderID", dummyFrom, eHubMessage.SenderID);
			AssertEquals("eHubMessage.RecipientID", string.IsNullOrEmpty(expectedRecipientID) ? dummyTo : expectedRecipientID, eHubMessage.RecipientID);
			AssertEquals("eHubMessage.SchemaType", expectedSchemaType, eHubMessage.SchemaType);
			AssertEquals("eHubMessage.ApplicationCode", expectedApplicationCode, eHubMessage.ApplicationCode);
			AssertEquals("eHubMessage.SchemaName", expectedSchemaName, eHubMessage.SchemaName);
			AssertEquals("eHubMessage.MessageStream", expectedStreamContent, new StreamReader(eHubMessage.MessageStream).ReadToEnd());
			AssertEquals("eHubMessage.EmailSubject", expectedEmailSubject, eHubMessage.EmailSubject);
			AssertEquals("eHubMessage.Filename", epxectedFileName, eHubMessage.Filename);
			eHubMessage.Dispose();
		}

		#endregion

		#region Create test data

		EDIInterchange CreateInterchange(string applicationCode, string type, string header, string body, string footer = "", string to = "DummyTo")
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_SessionGUID = new ZGuid(dummySessionID);
			interchange.EI_From = dummyFrom;
			interchange.EI_To = to;
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_InterchangeType = type;
			interchange.EI_HeaderText = header;
			interchange.EI_BodyText = body;
			interchange.EI_FooterText = footer;
			interchange.EI_InterchangeNum = dummyInterchangeNum;
			return interchange;
		}

		EDIInterchange CreateInterchange(string applicationCode, string type, string header, byte[] body, string footer = "", string to = "DummyTo")
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_SessionGUID = new ZGuid(dummySessionID);
			interchange.EI_From = dummyFrom;
			interchange.EI_To = to;
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_InterchangeType = type;
			interchange.EI_HeaderText = header;
			interchange.EI_BodyData = body;
			interchange.EI_FooterText = footer;
			interchange.EI_InterchangeNum = dummyInterchangeNum;
			return interchange;
		}

		EDIMessage CreateMessage(EDIInterchange interchange, string type = "", string subType = "")
		{
			var message = interchange.ContainedMessages.AddNew();
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			return message;
		}

		EDIMessageAttach CreateMessageAttachment(EDIMessage message, string name, string type, string docFilename, string docType, string docContent)
		{
			if (message.EM_LinkedObject == null)
			{
				message.EM_LinkedObject = (BusinessObject)Factory.New<Integration.Freight.ICommonShipment>();
			}

			var docManager = message.EM_LinkedObject as IDocManagerSupport;
			var documentBytes = Encoding.UTF8.GetBytes(docContent);
			var eDoc1 = docManager.DocManagerInfo.AddFileOrDocument(documentBytes, docFilename, docType);

			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_FileName = name;
			attachment.EG_EdiMsgDocType = type;
			attachment.EG_StorageDocsGuid = eDoc1.UniqueKey;

			return attachment;
		}

		EDIMessageAttach CreateMessageAttachmentWithoutStorage(EDIMessage message, string name, string type)
		{
			if (message.EM_LinkedObject == null)
			{
				message.EM_LinkedObject = (BusinessObject)Factory.New<Integration.Freight.ICommonShipment>();
			}
			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_FileName = name;
			attachment.EG_EdiMsgDocType = type;
			attachment.EG_StorageDocsGuid = Guid.NewGuid();
			return attachment;
		}

		#endregion

		const string dummyInterchangeNum = "~BLAH00000000009999";
		const string dummySessionID = "166f827b-5daa-49bb-b364-42a22b4f438a";
		const string dummyFrom = "DummyFrom";
		const string dummyTo = "DummyTo";
	}
}
