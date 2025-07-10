using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck;

namespace Enterprise.Client.EDI.Test
{
	public class MyAccountPortalDocumentCheckerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidELearningDocumentUrlUppercaseNonSanitizedInputExists()
		{
			// Arrange
			// Act
			var actual = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.ValidELearningDocumentUrl("HTTPS://MYACCOUNT-PORTAL.CARGOWISE.COM/MY-ACCOUNT/DOCUMENTS/USERGUIDES/WORKBOOKS/1BZA024.PDF");

			// Assert
			Assert("uppercase url should be present in the set", actual);
		}

		[ExpectNoExceptions]
		public void TestValidELearningDocumentLowercaseNonSanitizedInputExists()
		{
			// Arrange
			// Act
			var actual = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.ValidELearningDocumentUrl("https://myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/1bza024.pdf");
			// Assert
			Assert("lowercase url should be present in the set", actual);
		}

		[ExpectNoExceptions]
		public void TestValidELearningDocumentLowercaseNonSanitizedInputNotExists()
		{
			// Arrange
			// Act
			var actual = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.ValidELearningDocumentUrl("https://myaccount-portal/non-existing-document.pdf");
			// Assert
			Assert("lowercase url should not be present in the set", !actual);
		}

		[ExpectNoExceptions]
		public void TestNotValidELearningDocumentInputNotExists()
		{
			// Arrange
			// Act
			var actual = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.ValidELearningDocumentUrl(string.Empty);
			// Assert
			Assert("lowercase url should not be present in the set", !actual);
		}

		[ExpectNoExceptions]
		public void TestNullELearningDocumentInputNotExists()
		{
			// Arrange
			// Act
			var actual = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.ValidELearningDocumentUrl(null);
			// Assert
			Assert("lowercase url should not be present in the set", !actual);
		}

		[ExpectNoExceptions]
		public void TestCleanupJobConversationMessageIsNull()
		{
			// Arrange
			// Act
			// Assert
			new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.CleanMessages(null, new UrlExtractor());
		}

		[ExpectNoExceptions]
		public void TestCleanupUrlExtractorIsNull()
		{
			// Arrange
			// Act
			// Assert
			new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object)
				.CleanMessages(new List<JobConversationMessage>(), null);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupNothingChange()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident("test message", Factory) };
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Act
			checker.CleanMessages(message, new UrlExtractor());
			// Assert
			AssertEquals("test message", actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupUntouchIrrelevantLinks()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageContent = "test message\nhttps://randomurl.com";
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };

			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			AssertEquals(messageContent, actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupRemoveSingleUrl()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageContent = "Content Auto-Suggester. The following content may be useful in responding to this request:\ntest message\nhttps://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/not-exists1.pdf\nhttps://randomurl.com";
			var expectedContent = "Content Auto-Suggester. The following content may be useful in responding to this request:\nhttps://randomurl.com";
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };

			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			AssertEquals(expectedContent, actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupRemoveAllStaleLinksUrlCase1()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };

			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			AssertEquals(expectedContent, actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupRemoveAllStaleLinksUrlCase2WithTitles()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };
			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			AssertEquals(expectedContent, actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupRemoveAllStaleLinksUrlCase3WithTitles()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR00901.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS08702.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR17503.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR11304.pdf"
				};

			var expectedMessages = new List<string>
			{
			};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };

			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			//AssertEquals(expectedContent, message[0].JCM_Body);
			AssertEquals(0, actualMessages.Count);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupRemoveAllStaleLinksUrlCase4WithTitles()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR00901.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS08702.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR17503.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR11304.pdf"
				};
			var messageContent = string.Join("\n", messageLines);
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };
			messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"1BOR006 - How do I change or retrieve my password?",
					"https://myaccount-portal.cargowise.com/my-account/Documents/BorderWise/UserGuides/Workbooks/1BOR006.pdf"
				};
			messageContent = string.Join("\n", messageLines);
			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"1BOR006 - How do I change or retrieve my password?",
					"https://myaccount-portal.cargowise.com/my-account/Documents/BorderWise/UserGuides/Workbooks/1BOR006.pdf"
				};
			var expectedContent = string.Join("\n", expectedMessages);
			message.Add(RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory));

			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			AssertEquals(1, actualMessages.Count);
			AssertEquals(expectedContent, actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCleanupRemoveAllStaleLinksUrlCase2()
		{
			// Arrange
			var checker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object,
				GetMockOfIRecentPdfUpdatesApiClient().Object);
			var messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			var message = new List<JobConversationMessage> { RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory) };
			// Act
			checker.CleanMessages(message, new UrlExtractor());
			var actualMessages =
				RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(
					EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value, Factory).ToList();
			// Assert
			AssertEquals(expectedContent, actualMessages[0].JCM_Body);
		}

		[ExpectNoExceptions]
		public void TestGetActualMessageLinesNullMessageParameter()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			// Assert
			documentChecker.GetActualMessageLines(null);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetActualMessageLinesEmptyJCM_BodyParameter()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
			jobConversationMessage.JCM_IsSystem = true;
			jobConversationMessage.JCM_IsInternal = true;
			jobConversationMessage.JCM_Body = string.Empty;
			// Act
			var actual = documentChecker.GetActualMessageLines(jobConversationMessage);
			// Assert
			Assert(!actual.Any());
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetActualMessageLinesSingleLineJCM_BodyParameter()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
			jobConversationMessage.JCM_IsSystem = true;
			jobConversationMessage.JCM_IsInternal = true;
			jobConversationMessage.JCM_Body = "single line";
			// Act
			var actual = documentChecker.GetActualMessageLines(jobConversationMessage);
			// Assert
			Assert(actual.ToList().Count == 1);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetActualMessageLinesMultiLineJCM_BodyParameter()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
			jobConversationMessage.JCM_IsSystem = true;
			jobConversationMessage.JCM_IsInternal = true;
			jobConversationMessage.JCM_Body = "Content Auto-Suggester. The following content may be useful in responding to this request:\nhttp://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf\nhttp://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf";
			// Act
			var actual = documentChecker.GetActualMessageLines(jobConversationMessage);
			// Assert
			Assert(actual.ToList().Count == 3);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlNoUrlFoundCase1()
		{
			// Arrange
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck.Data.MessageWithoutLinks.txt";
			var lines = ReadLines(() => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName), Encoding.UTF8).ToList();

			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			Assert(lines.Any());
			// Act
			foreach (var line in lines)
			{
				if (!string.IsNullOrEmpty(line))
				{
					// Assert
					Assert(!documentChecker.ContainsUrl(line));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestContainsUrlNullPayload()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(null);
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlEmptyPayload()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl("");
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlWhiteSpacesPayload()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(" ");
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase1()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Dear Ronnie, 

I hope you are well. 

Thank you for your eRequest. 

Kindly note that due to maintenance being conducted on our eHub messaging infrastructure > 
- There were some delays being experienced. 

I can confirm that the maintenance has been completed however > 
- Could you please check that the messages are flowing through again on your end? 

In future, you are welcome to view scheduled maintenance on the link below > 
https://myaccount.cargowise.com/Home/CargoWise/ServiceIssuesMaintenanceNotices.aspx 

We look forward to your response. 

Many Thanks & Kind Regards, 
Melissa");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase2()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@" Content Auto-Suggestor
The following content may be useful in responding to this request:
https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1BAU%20How-to%20Resend%20or%20Retransmit%20the%20Customs%20Interchange%20Message%20in%20ICS.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/BAU_RetransmitCustomsInterchangeMessageinICS.pdf
http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/BAU004.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BAU011.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20160216.pdf");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase3()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Hi Luciana, 

You are able to check the security rights for modules using the Security Checkpoint Monitor

https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20191127a.pdf

Gretel");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase4()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Good day Stephen,

The copy procedure is now complete. Users can log back in to the TRN system with their ABQ username (eg ABQ.Firstname.Lastname). When users log into the newly restored system for the first time, they will be required to reset their password. To reset the password, they will need to enter their username, click 'Login', then click the 'Email Temp Password' button that will appear.

A temporary password can only be sent when the following conditions are met:

 · There is a valid email address entered on the Staff record
 · The email address is not used by multiple Staff records

For further information please refer to the following update note

https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20191112d.pdf

Regards");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase5()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@" Content Auto-Suggestor
The following content may be useful in responding to this request:
https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20191112d.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20150119.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/edienterpriseUpdateNote20071102.pdf");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase6()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl("https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20191112d.pdf");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase7()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Hi Kevin,

Thank you for raising this incident. 

We are currently conducting maintenance of our eHub messaging infrastructure. You can view scheduled maintenance on the link below. 

https://myaccount.cargowise.com/Home/CargoWise/ServiceIssuesMaintenanceNotices.aspx

A notice will be added to the page when completed. I will also update you via this incident. 

Gretel");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlUrlsFoundCase8()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"There is no standard integration with any other program, apart from MYOB.

The expectation if you're using the Sales Ledger in Sapphire is that the Invoices are generated in Sapphire rather than another system.

Having said that, there is a Sales Ledger Import available.

** ANY TESTING OF THE SALES LEDGER IMPORT MUST BE DONE IN YOUR TEST COMPANY FIRST TO ENSURE IT IS WORKING CORRECTLY BEFORE USING IT IN YOUR PRODUCTION DATABASE **

Before proceeding, you MUST make a copy of the standard release import and make changes only to your copy. Any changes made to a standard release import will be reverted to the standard release the next time your system is updated. 

The name of the import is SLINVIM1 - Sales Ledger Import

The structure of the import is:
ref_no         up to 12 characters
acc_code     up to 8 characters
date            format DD/MM/YYYY
description   up to 20 characters
GST_code    up to 8 characters 
NETT          amount - $/c value to 2 decimals
GST            amount - $/c value to 2 decimals
GROSS        amount - $/c value to 2 decimals
GL_CODE     up to 24 characters

The ref_no must be unique, because this is the Invoice number in Sapphire. 

The import has a standard release CSV format, but you can change the file format and use XML if you choose.

If you require hands-on assistance with the configuration and testing of Sapphire for this import, please engage directly with one of our WiseService Partners. https://myaccount.cargowise.com/Home/SapphireOdyssey/WiseServicePartners.aspx
");
			// Assert
			AssertEquals(true, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlNoMyAccountUrlsFoundCase1()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl("https://google.com");
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlNoMyAccountUrlsFoundCase2()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(" This incident was reported by Fabio Crosilla (fcrosilla@aitworldwide.com) and approved by Fabio Crosilla (fcrosilla@aitworldwide.com)");
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlNoMyAccountUrlsFoundCase3()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(" Module changed from RAT (Contract Management Console) to RMQ (RM Client Query)");
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlNoMyAccountUrlsFoundCase4()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Hi

Screenshot of the error


Kind Regards,

Tracey Spreckley
Independent IT Consultant

M: 0431 000 620
E: werwerwer@barrbusiness.com.au<mailto:werwerwer@barrbusiness.com.au>");
			// Assert
			AssertEquals(false, actual);
		}
		[ExpectNoExceptions]
		public void TestContainsUrlNoMyAccountUrlsFoundCase5()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Hi,

eAdaptor is activated. 

For MAOPRD system, Inbound URL: https://MAOPRDservices.wisegrid.net
For MAOTRN system, Inbound URL: https://maotrnservices.wisegrid.net/

Thanks");
			// Assert
			AssertEquals(false, actual);
		}
		[ExpectNoExceptions]
		public void TestContainsUrlNoMyAccountUrlsFoundCase6()
		{
			// Arrange
			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			// Act
			var actual = documentChecker.ContainsUrl(@"Hi Nonku,

Sorted and submitted, thanks.

Dayalan Naidoo
Access World
151 South Coast Road | Warehouse No 5 | Bayhead
Durban | South Africa
T: +27 31 451 9200
F: +27 31 451 9210
M: +27 82 573 9990
dayalan.naidoo@accessworld.com<mailto:dayalan.naidoo@accessworld.com>
link 1#KEY0000 link 2#KEY0001 >


[logo-max-quality]

[https://urldefense.com/v3/__http://accessworld.com/signature/linkedin.png__;!!Na5NE8kfbMIR6Ys!5MVwKGDRP0AKWGF6KSyV5hvUdRTO5WL7jAY-4pKVt2fzWzt7JgfFfgSX2apiISBqV5tzxQ$ ]<https://urldefense.com/v3/__https://www.linkedin.com/company/22294303/admin/updates/__;!!Na5NE8kfbMIR6Ys!5MVwKGDRP0AKWGF6KSyV5hvUdRTO5WL7jAY-4pKVt2fzWzt7JgfFfgSX2apiISDTOnRkLA$ >  [https://urldefense.com/v3/__http://accessworld.com/signature/facebook.png__;!!Na5NE8kfbMIR6Ys!5MVwKGDRP0AKWGF6KSyV5hvUdRTO5WL7jAY-4pKVt2fzWzt7JgfFfgSX2apiISBJksDQPQ$ ] link 3#KEY0002 >   [https://urldefense.com/v3/__http://accessworld.com/signature/twitter.png__;!!Na5NE8kfbMIR6Ys!5MVwKGDRP0AKWGF6KSyV5hvUdRTO5WL7jAY-4pKVt2fzWzt7JgfFfgSX2apiISC0CCcMVA$ ] link 4#KEY0003 >   [cid:image005.png@01D486A1.E07752C0] link 5#KEY0004 >");
			// Assert
			AssertEquals(false, actual);
		}

		[ExpectNoExceptions]
		public void TestContainsUrlLinksFoundCase1()
		{
			// Arrange
			var lines = new List<string>()
			{
				"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20191112d.pdf",
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=605fec47-c4e1-4317-b816-3a29288c7fb9&video=35292519,bbf1dd541201f7919531811dbb07349d",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR193.pdf",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/ediEnterpriseUpdateNote20071102.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/ediEnterpriseUpdateNote20071102.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20160211a.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_SecuritySettings.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1BUS_How%20do%20I%20set%20up%20my%20system%20for%20US%20AMS%20Messaging.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20201212.pdf",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR194.pdf",
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=0BB8E74A-4BC2-4348-BF71-F305439F7F17",
				"http://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#search=email+groups&video=11582138,12ac714833ef928879203f71139aa5bd",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/ediEnterpriseupdatenote20110824c.pdf"
			};

			var documentChecker = new MyAccountPortalDocumentChecker(GetMockOfILogger().Object, GetMockOfIRecentPdfUpdatesApiClient().Object);
			Assert(lines.Any());
			// Act
			foreach (var line in lines)
			{
				// Assert
				Assert(documentChecker.ContainsUrl(line));
			}
		}

		[ExpectNoExceptions]
		public void TestExtractFromExistingMyAccountUrlsCase1()
		{
			// Arrange
			var line = @"[Internal System Message]  Content Auto-Suggestor
The following content may be useful in responding to this request:
https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_How%20to%20Create%20a%20Staff%20record%20enabled%20for%20Single%20Sign%20On%20login.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1EDU_How%20To%20My%20Account%20Portal.pdf
https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_TroubleshootDocBuilderErrors.pdf
https://myaccount-portal.cargowise.com/my-account/documents/CargoWise%20One%20Remote%20Printing%20Guide.pdf
http://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20141021a.pdf";

			var expectedUrls = new HashSet<string>
			{
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_How%20to%20Create%20a%20Staff%20record%20enabled%20for%20Single%20Sign%20On%20login.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1EDU_How%20To%20My%20Account%20Portal.pdf",
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_TroubleshootDocBuilderErrors.pdf",
				"https://myaccount-portal.cargowise.com/my-account/documents/CargoWise%20One%20Remote%20Printing%20Guide.pdf",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20141021a.pdf"
			};
			var extractor = new UrlExtractor();
			// Act
			var actualUrls = extractor.ExtractFromExistingMyAccountUrls(line).ToList();
			// Assert
			Assert(actualUrls.Count == expectedUrls.Count);
			foreach (var url in actualUrls)
			{
				Assert(expectedUrls.Contains(url));
			}
		}

		[ExpectNoExceptions]
		public void TestExtractFromExistingMyAccountUrlsCase2()
		{
			// Arrange
			var line = string.Empty;
			var extractor = new UrlExtractor();
			// Act
			var actualUrls = extractor.ExtractFromExistingMyAccountUrls(line);
			// Assert
			Assert(!actualUrls.Any());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExtractFromExistingMyAccountUrlsCase3()
		{
			// Arrange
			string line = null;
			var extractor = new UrlExtractor();
			// Act
			// Assert
			var actualUrls = extractor.ExtractFromExistingMyAccountUrls(line);
		}

		IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
		{
			using (var stream = streamProvider())
			{
				using (var reader = new StreamReader(stream, encoding))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						yield return line;
					}
				}
			}
		}

		Mock<IRecentPdfUpdatesApiClient> GetMockOfIRecentPdfUpdatesApiClient()
		{
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			return RecentPdfUpdatesApiClientMockUtil.CreateFullyPopulatedRecentPdfUpdatesApiClientClient(mockLogger);
		}

		Mock<ILogger> GetMockOfILogger()
		{
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});
			return mockLogger;
		}
	}
}
