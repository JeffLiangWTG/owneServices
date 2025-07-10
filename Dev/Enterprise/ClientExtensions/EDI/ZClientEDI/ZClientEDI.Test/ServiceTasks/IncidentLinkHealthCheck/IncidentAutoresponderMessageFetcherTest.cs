using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck;
using Enterprise.Integration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TestHelpers;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;
using ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck;

namespace Enterprise.Client.EDI.Test
{
	public class IncidentAutoresponderMessageFetcherTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingCase1()
		{
			// Arrange
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
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateFullyPopulatedRecentPdfUpdatesApiClientClient(mockLogger);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoListOfChangesCase1()
		{
			// Arrange
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
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: false, downloadPdfUpdateNotes: true, downloadTechnicalAdvisory: true, downloadBorderWiseDocuments: true);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoListOfChangesWithTitlesCase1()
		{
			// Arrange
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
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: false, downloadPdfUpdateNotes: true, downloadTechnicalAdvisory: true, downloadBorderWiseDocuments: true);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoDownloadPdfUpdateNotesCase1()
		{
			// Arrange
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
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: true, downloadPdfUpdateNotes: false, downloadTechnicalAdvisory: true, downloadBorderWiseDocuments: true);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoDownloadPdfUpdateNotesWithTitlesCase1()
		{
			// Arrange
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
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: true, downloadPdfUpdateNotes: false, downloadTechnicalAdvisory: true, downloadBorderWiseDocuments: true);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoDownloadTechnicalAdvisoryCase1()
		{
			// Arrange
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
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: true, downloadPdfUpdateNotes: true, downloadTechnicalAdvisory: false, downloadBorderWiseDocuments: true);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoDownloadTechnicalAdvisoryWithTitlesCase1()
		{
			// Arrange
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
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: true, downloadPdfUpdateNotes: true, downloadTechnicalAdvisory: false, downloadBorderWiseDocuments: true);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoDownloadBorderWiseDocumentsCase1()
		{
			// Arrange
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
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: true, downloadPdfUpdateNotes: true, downloadTechnicalAdvisory: true, downloadBorderWiseDocuments: false);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingNoDownloadBorderWiseDocumentsWithTitlesCase1()
		{
			// Arrange
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
					"Title 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"Title 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Title 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"Title 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateCustomPopulatedRecentPdfUpdatesApiClientClient(mockLogger: mockLogger,
				downloadListOfChanges: true, downloadPdfUpdateNotes: true, downloadTechnicalAdvisory: true, downloadBorderWiseDocuments: false);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingCase2()
		{
			// Arrange
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
			var messageLines = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"Topic 1",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"Topic 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Topic 3",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf",
					"Topic 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};
			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"Topic 2",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087.pdf",
					"Topic 4",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateFullyPopulatedRecentPdfUpdatesApiClientClient(mockLogger);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingPartiallyPopulatedTitlesCase1()
		{
			// Arrange
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
			var messageLines = new List<string>
				{
					"[Internal System Message]  Content Auto-Suggester. The following content may be useful in responding to this request:",
					"http://myaccount.cargowise.com/Home/WiseLearningediEnterprise.aspx#item=711d7d1a-206c-46c8-aace-67d1c97e297f",
					"1COR - How do multiple companies share one database?",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_MultipleCompanyDatabase.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1ACC_Implementing%20best%20practices%20and%20troubleshooting%20Job%20Profit%20reports.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20170327b.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20171019.pdf"
				};
			var expectedMessages = new List<string>
				{
					"[Internal System Message]  Content Auto-Suggester. The following content may be useful in responding to this request:",
					"1COR - How do multiple companies share one database?",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/HowTo/1COR_MultipleCompanyDatabase.pdf"
				};

			var messageContent = string.Join("\n", messageLines);
			var expectedContent = string.Join("\n", expectedMessages);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateFullyPopulatedRecentPdfUpdatesApiClientClient(mockLogger);

			recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(), EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl.Value))
				.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
				.Returns(
					JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
						System.Text.Encoding.Default.GetString(
							File.OpenRead(AssetsHelper.FetchTestAsset("ZClientEDI/content/ServiceTasks/IncidentLinkHealthCheck/ELearningDocumentUpdateNotes.json"))
							.ToByteArray())));

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains url in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRunProcessingCase3OneOutOfDateRange()
		{
			// Arrange
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
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);
			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, --daysBack);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateFullyPopulatedRecentPdfUpdatesApiClientClient(mockLogger);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("One record should exists", result.Count == 1);
			Assert("Record contains two urls in the message", result[result.Count - 1].JCM_Body == expectedContent);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void RunProcessingCase1OneOutOfDateRangeBackwardCompatibility()
		{
			// Arrange
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
			var messageLines = new List<string>
				{
					"Content Auto-Suggestor",
					"The following content may be useful in responding to this request:",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009_non_existing.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1BUS087_non_existing.pdf",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175_non_existing.pdf",
					"https://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR113_non_existing.pdf"
				};
			var messageContent = string.Join("\n", messageLines);
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, -1);
			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;
			RecentPdfUpdatesApiClientMockUtil.CreateIncident(messageContent, Factory, --daysBack);

			var recentPdfUpdatesApiClientMock = RecentPdfUpdatesApiClientMockUtil.CreateFullyPopulatedRecentPdfUpdatesApiClientClient(mockLogger);

			var incidentAutoresponderMessageFetcher = new IncidentAutoresponderMessageFetcher(mockLogger.Object);
			var checker = new MyAccountPortalDocumentChecker(mockLogger.Object, recentPdfUpdatesApiClientMock.Object) as IMyAccountPortalDocumentChecker;
			var token = new CancellationToken();
			incidentAutoresponderMessageFetcher.RunProcessing(token, Factory, checker);

			daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;

			var result = RecentPdfUpdatesApiClientMockUtil.GetConversationMessages(daysBack, Factory).ToList();
			Assert("No records should exists", !result.Any());
		}
	}
}
