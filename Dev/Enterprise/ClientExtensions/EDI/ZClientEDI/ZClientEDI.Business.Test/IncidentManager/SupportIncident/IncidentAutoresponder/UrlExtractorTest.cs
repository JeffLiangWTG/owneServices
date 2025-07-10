using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder.Test
{
	public partial class UrlExtractorTest : TestCaseWithFactory
	{
		readonly UrlExtractor urlExtractor = new UrlExtractor();
		readonly ELearningUrlComparer eLearningUrlComparer = new ELearningUrlComparer();

		public void TestELearningUrlsEquality()
		{
			// Arrange
			var url1 = "https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf";
			var url2 = "http://myaccount.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf";
			var url3 = "HTTPS://MYACCOUNT-PORTAL.CARGOWISE.COM/MY-ACCOUNT/DOCUMENTS/USERGUIDES/WORKBOOKS/1COR175.PDF";
			var url4 = "https://myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/1cor175.pdf";
			var illegalUrl = "ht:////foobar";
			var emptyUrl = "";

			// Act
			var result1 = UrlExtractor.IsSameELearningUrl(url1, url2);
			var result2 = UrlExtractor.IsSameELearningUrl(url1, null);
			var result3 = UrlExtractor.IsSameELearningUrl(url3, url4);
			var result4 = UrlExtractor.IsSameELearningUrl(url1, illegalUrl);
			var result5 = UrlExtractor.IsSameELearningUrl(url2, emptyUrl);

			// Assert
			Assert(result1);
			Assert(!result2);
			Assert(result3);
			Assert(!result4);
			Assert(!result5);
		}

		public void TestExtractUrlsFromNullReference()
		{
			// Assert
			AssertExceptionThrown<ArgumentNullException>(() => urlExtractor.Extract(null));
		}

		public void TestExtractUrlsFromEmptyString()
		{
			// Assert
			Assert(urlExtractor.Extract("").ToList().Count == 0);
		}

		public void TestExtractUrlsFromStrings_Canonical()
		{
			// Arrange
			var text1 = "Please refer to the WiseLearning materials on how to have your password reset.  This has to be done by a user who has admin rights, which in your system is Bob Broekman.  The steps to reset the CargoWise One password is covered in video '1COR175\r\nHow do I change or reset a user's password?' that is located in the WiseLearning portal at https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411.";
			var text2 = "The connectivity guide is located here: MyAccount -> Technical Guides -> WiseCloud -> WiseCloud Connectivity and Troubleshooting Guide (https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf)";
			var text3 = "you can navigate to https://myaccount.cargowise.com/en-us/Home/CargoWiseOne/TechnicalGuides.aspx and choose  (CargoWise One > Technical Guides > Downloads) you will be able to download Remote Desktop Services Installer. Direct Url https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneRemoteDesktopServicesSetup.exe!";
			var text4 = @"For further information on how to reset passwords please refer to the following video and workbook from the WiseLearning portal: Video - https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0
Workbook - http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf...";
			var text5 = @"Not a CR4
Whenever you cannot print, the first thing that you need to check is that if your CargoWise One WebPrint Client is running.  This and other troubleshooting advise on printing issues is covered in the 'We cannot print from our system or documents are not being printed, what should we do?' FAQ under the Printing section at https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=0BB8E74A-4BC2-4348-BF71-F305439F7F17&tab=faqs.";
			var expectedUrls1 = new List<string>
			{
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411"
			};
			var expectedUrls2 = new List<string>
			{
				"https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf"
			};
			var expectedUrls3 = new List<string>();
			var expectedUrls4 = new List<string>
			{
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			var expectedUrls5 = new List<string>
			{
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=0BB8E74A-4BC2-4348-BF71-F305439F7F17&tab=faqs"
			};
			// Act
			var actualUrls1 = urlExtractor.Extract(text1);
			var actualUrls2 = urlExtractor.Extract(text2);
			var actualUrls3 = urlExtractor.Extract(text3);
			var actualUrls4 = urlExtractor.Extract(text4);
			var actualUrls5 = urlExtractor.Extract(text5);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls1, actualUrls1);
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls2, actualUrls2);
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls3, actualUrls3);
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls4, actualUrls4);
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls5, actualUrls5);
		}

		public void TestExtractUrlsFromStrings_NonCanonical()
		{
			// Arrange
			var text = @"Url 1: http://example.org;
Url 2: https://fake.cargowise.com/en-us/Home/CargoWiseOne/TechnicalGuides.aspx;
Url 3: http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf.";
			var expectedUrls = new List<string>
			{
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			// Act
			var actualUrls = urlExtractor.Extract(text);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}

		public void TestExtractUrlsFromStrings_FilterOutTooGeneralFrontPages()
		{
			// Arrange
			var text = @"Url 1: http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf;
Url 2: https://myaccount.cargowise.com;
Url 3: https://myaccount.cargowise.com/;
Url 4: https://myaccount.cargowise.com#;
Url 5: https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx;
Url 6: https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#";
			var expectedUrls = new List<string>
			{
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			// Act
			var actualUrls = urlExtractor.Extract(text);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}

		public void TestExtractUrlsFromStrings_FilterOutEXE()
		{
			// Arrange
			var text = @"Url 1: https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneRemoteDesktopServicesSetup.exe;
Url 2: https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411;
Url 3: http://myaccount.cargowise.com/myaccount/downloads/CargoWiseOneRemoteDesktopServicesSetup.EXE;";
			var expectedUrls = new List<string>
			{
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411"
			};
			// Act
			var actualUrls = urlExtractor.Extract(text);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}

		public void TestExtractUrlsFromStrings_IncludeUpdateNotes()
		{
			// Arrange
			EDIDataRegistry.Instance.IncludeUpdateNoteUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var text = @"Url 1: http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf;
Url 2: https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20200325d.pdf;
Url 3: https://myaccount.cargowise.com/Home/CargoWise/UpdateNotes.aspx";
			var expectedUrls = new List<string>
			{
				"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20200325d.pdf",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
			};
			// Act
			var actualUrls = urlExtractor.Extract(text);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}
		public void TestExtractUrlsFromStrings_ExcludeUpdateNotes()
		{
			// Arrange
			var text = @"Url 1: http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf;
Url 2: https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20200325d.pdf;
Url 3: https://myaccount.cargowise.com/Home/CargoWise/UpdateNotes.aspx";
			var expectedUrls = new List<string>
			{
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			// Act
			var actualUrls = urlExtractor.Extract(text);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}

		public void TestExtractUrlsFromStrings_FilterOutDuplicates()
		{
			// Arrange
			var text = @"Url 1: https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf;
Url 2: http://myaccount.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf;
Url 3: HTTPS://MYACCOUNT-PORTAL.CARGOWISE.COM/MY-ACCOUNT/DOCUMENTS/USERGUIDES/WORKBOOKS/1COR175.PDF;
Url 4: https://myaccount-portal.cargowise.com/my-account/documents/userguides/workbooks/1cor175.pdf.";
			var expectedUrls = new List<string>
			{
				"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			// Act
			var actualUrls = urlExtractor.Extract(text);
			// Assert
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}

		public void TestExtractUrlsFromIncident()
		{
			// Arrange
			var messages = new List<string>
			{
				@"Please refer to the WiseLearning materials on how to have your password reset.  This has to be done by a user who has admin rights, which in your system is Bob Broekman.  The steps to reset the CargoWise One password is covered in video '1COR175\r\nHow do I change or reset a user's password?' that is located in the WiseLearning portal at https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411.",
				@"The connectivity guide is located here: MyAccount -> Technical Guides -> WiseCloud -> WiseCloud Connectivity and Troubleshooting Guide (https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf)",
				@"you can navigate to https://myaccount.cargowise.com/en-us/Home/CargoWiseOne/TechnicalGuides.aspx and choose  (CargoWise One > Technical Guides > Downloads) you will be able to download Remote Desktop Services Installer. Direct Url https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneRemoteDesktopServicesSetup.exe!",
				@"For further information on how to reset passwords please refer to the following video and workbook from the WiseLearning portal: Video - https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0
Workbook - http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf..."
			};
			var expectedUrls = new List<string>
			{
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411",
				"https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf",
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};

			var incidentWithJobConversation = Factory.New<IncidentMainBase>();
			var incidentWithJobConversationRequest = Factory.New<IncidentRequest>();
			var jobConversation = Factory.New<JobConversation>();
			jobConversation.JCC_ParentID = incidentWithJobConversationRequest.PK;
			jobConversation.JCC_ParentTableCode = "INC";
			incidentWithJobConversation.IM_INC_Request = incidentWithJobConversationRequest.PK;
			incidentWithJobConversation.IM_IncidentType = "INC";
			foreach (var message in messages)
			{
				var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
				jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
				jobConversationMessage.JCM_IsSystem = false;
				jobConversationMessage.JCM_IsInternal = true;
				jobConversationMessage.JCM_Body = message;
			}

			var incidentWithoutJobConversation = Factory.New<IncidentMainBase>();

			Factory.Save();

			// Act
			var actualUrls1 = urlExtractor.ExtractFromIncident(Factory, incidentWithJobConversation.PK);
			var actualUrls2 = urlExtractor.ExtractFromIncident(Factory, incidentWithoutJobConversation.PK);

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedUrls, actualUrls1);
			Assert(actualUrls2.ToList().Count == 0);
		}

		public void TestExtractUrlsFromIncidentWhileExcludingAutorespondedMessages()
		{
			// Arrange
			var autorespondedMessage = "Content Auto-Suggestor\nThe following content may be useful in responding to this request:\nhttps://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411";
			var messages = new List<string>
			{
				@"The connectivity guide is located here: MyAccount -> Technical Guides -> WiseCloud -> WiseCloud Connectivity and Troubleshooting Guide (https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf)",
				@"you can navigate to https://myaccount.cargowise.com/en-us/Home/CargoWiseOne/TechnicalGuides.aspx and choose  (CargoWise One > Technical Guides > Downloads) you will be able to download Remote Desktop Services Installer. Direct Url https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneRemoteDesktopServicesSetup.exe!",
				@"For further information on how to reset passwords please refer to the following video and workbook from the WiseLearning portal: Video - https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0
Workbook - http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf..."
			};
			var expectedUrls = new List<string>
			{
				"https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf",
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			var incident = Factory.New<IncidentMainBase>();
			var incidentRequest = Factory.New<IncidentRequest>();
			var jobConversation = Factory.New<JobConversation>();
			jobConversation.JCC_ParentID = incidentRequest.PK;
			jobConversation.JCC_ParentTableCode = "INC";
			incident.IM_INC_Request = incidentRequest.PK;
			incident.IM_IncidentType = "INC";
			var glbStaff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, User.ServiceUserCode)).FirstOrDefault();

			var jobConversationMessageAutoresponded = Factory.New<JobConversationMessage>();
			jobConversationMessageAutoresponded.JCM_JCC_Conversation = jobConversation.PK;
			jobConversationMessageAutoresponded.JCM_IsSystem = true;
			jobConversationMessageAutoresponded.JCM_IsInternal = true;
			jobConversationMessageAutoresponded.JCM_Body = autorespondedMessage;
			var participant = Factory.New<JobConversationParticipant>();
			participant.JCP_JCC_Conversation = jobConversation.PK;
			participant.JCP_ParticipantID = glbStaff.PK;
			participant.JCP_ParticipantTableCode = "GS";
			jobConversationMessageAutoresponded.JCM_JCP_Participant = participant.PK;

			foreach (var message in messages)
			{
				var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
				jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
				jobConversationMessage.JCM_IsSystem = false;
				jobConversationMessage.JCM_IsInternal = true;
				jobConversationMessage.JCM_Body = message;
			}
			// Act
			var actualUrls = urlExtractor.ExtractFromIncident(Factory, incident.PK);
			// Assert
			AssertNotNull(glbStaff);
			AssertContainsExactElementsInAnyOrder(eLearningUrlComparer, expectedUrls, actualUrls);
		}

		public void TestExtractUrlsFromIncidentWhileExcludingRemoteMessages()
		{
			// Arrange
			var messages = new List<string>
			{
				@"Please refer to the WiseLearning materials on how to have your password reset.  This has to be done by a user who has admin rights, which in your system is Bob Broekman.  The steps to reset the CargoWise One password is covered in video '1COR175\r\nHow do I change or reset a user's password?' that is located in the WiseLearning portal at https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=50C7EB3F-DB8A-48E4-B097-09AF0EE1C411.",
				@"The connectivity guide is located here: MyAccount -> Technical Guides -> WiseCloud -> WiseCloud Connectivity and Troubleshooting Guide (https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf)",
				@"you can navigate to https://myaccount.cargowise.com/en-us/Home/CargoWiseOne/TechnicalGuides.aspx and choose  (CargoWise One > Technical Guides > Downloads) you will be able to download Remote Desktop Services Installer. Direct Url https://myaccount-portal.cargowise.com/myaccount/downloads/CargoWiseOneRemoteDesktopServicesSetup.exe!",
				@"For further information on how to reset passwords please refer to the following video and workbook from the WiseLearning portal: Video - https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0
Workbook - http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf..."
			};
			var expectedUrls = new List<string>
			{
				"https://myaccount-portal.cargowise.com/my-account/documents/WiseCloud%20Connectivity%20and%20Troubleshooting%20Guide.pdf",
				"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx#video=11581951,488a327d09eaaad164d2b0618cd790f0",
				"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
			};
			var incident = Factory.New<IncidentMainBase>();
			var incidentRequest = Factory.New<IncidentRequest>();
			var jobConversation = Factory.New<JobConversation>();
			jobConversation.JCC_ParentID = incidentRequest.PK;
			jobConversation.JCC_ParentTableCode = "INC";
			incident.IM_INC_Request = incidentRequest.PK;
			incident.IM_IncidentType = "INC";

			for (var i = 0; i < messages.Count; i++)
			{
				var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
				jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
				jobConversationMessage.JCM_IsSystem = false;
				jobConversationMessage.JCM_IsInternal = true;
				jobConversationMessage.JCM_Body = messages[i];
				if (i == 0)
				{
					jobConversationMessage.JCM_IsLocal = false;
				}
				else
				{
					jobConversationMessage.JCM_IsLocal = true;
				}
			}

			Factory.Save();

			// ACT
			var actualUrls = urlExtractor.ExtractFromIncident(Factory, incident.PK);

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedUrls, actualUrls);
		}
	}
}