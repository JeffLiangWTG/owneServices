using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder;
using Enterprise.MasterFiles.Business.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.IncidentManager.Test
{
	internal class IncidentAutoresponderDocumentTitleTest : TestCaseWithFactory
	{
		readonly UrlExtractor urlExtractor = new UrlExtractor();
		readonly IncidentAutoresponderDocumentTitle incidentAutoresponderDocumentTitle = new IncidentAutoresponderDocumentTitle();

		void LoadELearningDocumentDescription()
		{
			using (var streamReader = new StreamReader(Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(
					"ZClientEDI.Test.IncidentManager.BatchProcessor.SupportIncident.ELearningDocumentDescriptionMockData.json")))
			{
				var eLearningDocumentDescriptionJArray = JArray.Parse(streamReader.ReadToEnd());
				foreach (JObject documentDescriptionJObject in eLearningDocumentDescriptionJArray)
				{
					var elearningDocumentDescription = Factory.New<ELearningDocumentDescription>();
					var lastEditTime = DateTime.UtcNow.AddDays(-1);

					elearningDocumentDescription.ELD_DocumentLastModified = lastEditTime;
					elearningDocumentDescription.ELD_Title = documentDescriptionJObject["ELD_Title"].ToString();
					elearningDocumentDescription.ELD_Url = documentDescriptionJObject["ELD_Url"].ToString();
					elearningDocumentDescription.ELD_DocumentType = documentDescriptionJObject["ELD_DocumentType"].ToString();
				}
			}

			Factory.Save();
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestDocumentUrlsTitleNotFound()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR008.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var expectedMessages = new List<string>
				{
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR008.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			//Act
			var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
			var messageContent = string.Join("\n", documentUrlsWithTitle);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals("Expecting Document Urls without Title", expectedContent, messageContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestDocumentUrlsTitleFound()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1SHM025.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BCA012.pdf",
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/HowTo/1BTW_How-to Creating an export air declaration.pdf",
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1ACC225.pdf"
				};

			var expectedMessages = new List<string>
				{
					"1SHM025 - How do I create a Periodic Invoice",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1SHM025.pdf",
					"1BCA012 - How do I send CBSA queries from the menu",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BCA012.pdf",
					"1BTW - Creating an export air declaration",
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/HowTo/1BTW_How-to Creating an export air declaration.pdf",
					"1ACC225 - How do I enter a Bank Fee within a Bank Reconcilation session",
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1ACC225.pdf"
				};

			//Act
			var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
			var messageContent = string.Join("\n", documentUrlsWithTitle);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals("Expecting Document Urls with Title", expectedContent, messageContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestContainsFewDocumentUrlsTitleFound()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/1AAAB009.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/1BBB087.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/HowTo/1BTW_How-to Creating an export air declaration.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BEU004.pdf"
				};

			var expectedMessages = new List<string>
				{
					"http://myaccount-portal.cargowise.com/test/Documents/UserGuides/1AAAB009.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/1BBB087.pdf",
					"1BTW - Creating an export air declaration",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/HowTo/1BTW_How-to Creating an export air declaration.pdf",
					"1BEU004 - How do I complete the Customs Valuation and Charges Grids?",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BEU004.pdf"
				};

			//Act
			var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
			var messageContent = string.Join("\n", documentUrlsWithTitle);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals("Expecting Document Urls with Title", expectedContent, messageContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestInvalidDocumentUrls()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"http://example.com",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BUS087.json",
					"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx",
					"https://example.com/en-us/Home/CargoWiseLearning.aspx"
				};

			var expectedMessages = new List<string>
				{
					"http://example.com",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BUS087.json",
					"https://myaccount.cargowise.com/en-us/Home/CargoWiseLearning.aspx",
					"https://example.com/en-us/Home/CargoWiseLearning.aspx"
				};

			//Act
			var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
			var messageContent = string.Join("\n", documentUrlsWithTitle);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals("Expecting Document Urls without Title", expectedContent, messageContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestDocumentUrlsFoundWithEmptyTitle()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BBU031.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BAU031.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR294.pdf"
				};

			var expectedMessages = new List<string>
				{
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BBU031.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BAU031.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR294.pdf"
				};

			//Act
			var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
			var messageContent = string.Join("\n", documentUrlsWithTitle);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals("Expecting Document Urls with Empty Title", expectedContent, messageContent);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestDocumentUrlIsNull()
		{
			// Arrange
			List<string> messageLines = null;
			//Assert
			incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestInvalidUrls()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"https://myaccount-portal.cargowise.com/test/Documents/UpdateNotes/ediEnterpriseUpdateNote20061117.pdf",
					"https://myaccount.cargowise.com/Home/CargoWiseLearning.aspx#search=1COR029",
					"https://myaccount-portal.cargowise.com/test/Documents/UpdateNotes/CargoWiseOneUpdateNote20190920.pdf",
					"www.exampleurl.com"
				};

			var expectedMessages = new List<string>
				{
					"https://myaccount-portal.cargowise.com/test/Documents/UpdateNotes/ediEnterpriseUpdateNote20061117.pdf",
					"https://myaccount.cargowise.com/Home/CargoWiseLearning.aspx#search=1COR029",
					"https://myaccount-portal.cargowise.com/test/Documents/UpdateNotes/CargoWiseOneUpdateNote20190920.pdf",
					"www.exampleurl.com"
				};

			//Act
			var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(messageLines);
			var messageContent = string.Join("\n", documentUrlsWithTitle);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals(expectedContent, messageContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestCompleteSystemMessage()
		{
			LoadELearningDocumentDescription();

			// Arrange
			var messageLines = new List<string>
				{
					"Content Auto-Suggester",
					"The following content may be useful in responding to this request:",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR182.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BAU050.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1RAT117.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1XDF032.pdf"
				};

			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester",
					"The following content may be useful in responding to this request:",
					"1COR182 How do I restrict access to view other staff members",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1COR182.pdf",
					"1BAU050 - How to set up an underbond movement to be sent automatically when a flight arrives.",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1BAU050.pdf",
					"1RAT117 - How do I rate overweight penalty fees and surcharges",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1RAT117.pdf",
					"https://myaccount-portal.cargowise.com/test/Documents/UserGuides/Workbooks/1XDF032.pdf"
				};

			//Act
			var constructMessages = new List<string>();
			foreach (var messageLine in messageLines)
			{
				var urls = urlExtractor.Extract(messageLine)?.ToList();
				if (urls != null && urls.Count != 0)
				{
					var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(urls);
					constructMessages.AddRange(documentUrlsWithTitle);
				}
				else
				{
					constructMessages.Add(messageLine);
				}
			}

			var messageContent = string.Join("\n", constructMessages);
			var expectedContent = string.Join("\n", expectedMessages);

			//Assert
			AssertEquals("Expecting System Message With Document Title", expectedContent, messageContent);
		}
	}
}
