using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using WTG.TestHelpers;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck
{
	public static class RecentPdfUpdatesApiClientMockUtil
	{
		public static Mock<IRecentPdfUpdatesApiClient> CreateFullyPopulatedRecentPdfUpdatesApiClientClient(Mock<ILogger> mockLogger)
		{
			var recentPdfUpdatesApiClientMock = new Mock<IRecentPdfUpdatesApiClient>();
			recentPdfUpdatesApiClientMock.Setup(o => o.DownloadListOfChanges(It.IsAny<ZDateTime>()))
				.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
				.Returns(
					JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
						System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
							.GetManifestResourceStream(
								"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
							.ToByteArray())));

			recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(), EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl.Value))
				.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
				.Returns(
					JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
						System.Text.Encoding.Default.GetString(
							File.OpenRead(AssetsHelper.FetchTestAsset("ZClientEDI/content/ServiceTasks/IncidentLinkHealthCheck/ELearningDocumentUpdateNotes.json"))
							.ToByteArray())));

			recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(), EDIDataRegistry.Instance.ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl.Value))
				.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
				.Returns(
					JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
						System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
							.GetManifestResourceStream(
								"ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck.Data.ELearningDocumentTechnicalAdvisoryNotes.json")
							.ToByteArray())));

			recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(), EDIDataRegistry.Instance.ELearningDocumentBorderWiseDocumentsApiClientUrl.Value))
				.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
				.Returns(
					JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
						System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
							.GetManifestResourceStream(
								"ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck.Data.ELearningDocumentBorderWiseUserGuides.json")
							.ToByteArray())));
			return recentPdfUpdatesApiClientMock;
		}

		public static Mock<IRecentPdfUpdatesApiClient> CreateCustomPopulatedRecentPdfUpdatesApiClientClient(Mock<ILogger> mockLogger,
			bool downloadListOfChanges = true, bool downloadPdfUpdateNotes = true, bool downloadTechnicalAdvisory = true, bool downloadBorderWiseDocuments = true)
		{
			var recentPdfUpdatesApiClientMock = new Mock<IRecentPdfUpdatesApiClient>();
			if (downloadListOfChanges)
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadListOfChanges(It.IsAny<ZDateTime>()))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(
						JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
							System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
								.GetManifestResourceStream(
									"ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.ElearningDocumentDescriptionBigPayload.json")
								.ToByteArray())));
			}
			else
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadListOfChanges(It.IsAny<ZDateTime>()))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(new List<IELearningDocumentFileDescription>());
			}

			if (downloadPdfUpdateNotes)
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(),
						EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl.Value))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(
						JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
							System.Text.Encoding.Default.GetString(
								File.OpenRead(AssetsHelper.FetchTestAsset("ZClientEDI/content/ServiceTasks/IncidentLinkHealthCheck/ELearningDocumentUpdateNotes.json"))
								.ToByteArray())));
			}
			else
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(),
						EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl.Value))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(new List<IELearningDocumentFileDescription>());
			}

			if (downloadTechnicalAdvisory)
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(),
						EDIDataRegistry.Instance.ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl.Value))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(
						JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
							System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
								.GetManifestResourceStream(
									"ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck.Data.ELearningDocumentTechnicalAdvisoryNotes.json")
								.ToByteArray())));
			}
			else
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(),
						EDIDataRegistry.Instance.ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl.Value))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(new List<IELearningDocumentFileDescription>());
			}

			if (downloadBorderWiseDocuments)
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(),
						EDIDataRegistry.Instance.ELearningDocumentBorderWiseDocumentsApiClientUrl.Value))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(
						JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(
							System.Text.Encoding.Default.GetString(Assembly.GetExecutingAssembly()
								.GetManifestResourceStream(
									"ZClientEDI.Test.ServiceTasks.IncidentLinkHealthCheck.Data.ELearningDocumentBorderWiseUserGuides.json")
								.ToByteArray())));
			}
			else
			{
				recentPdfUpdatesApiClientMock.Setup(o => o.DownloadOtherElearningDocumentChanges(It.IsAny<ZDateTime>(),
						EDIDataRegistry.Instance.ELearningDocumentBorderWiseDocumentsApiClientUrl.Value))
					.Callback(() => mockLogger.Object.Log(LogType.Information, "information logging"))
					.Returns(new List<IELearningDocumentFileDescription>());
			}

			return recentPdfUpdatesApiClientMock;
		}
		public static IEnumerable<JobConversationMessage> GetConversationMessages(int daysBack, BusinessObjectFactory factory)
		{
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;
			var getIncidentQuery = new ZDBOnlyQuery(typeof(IncidentMainBase));
			getIncidentQuery.AddToFilter(IncidentMainSchema.IM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(daysBack));
			var reader = new FilteredBusinessObjectReader<IncidentMainBase>(getIncidentQuery, factory) { BatchSize = 100 };
			var result = new List<JobConversationMessage>();
			List<IncidentMainBase> candidates = null;
			BusinessObject lastBizRead = null;
			while ((candidates = reader.LoadNextBatchInANewFactory(lastBizRead).Cast<IncidentMainBase>().ToList()).Any())
			{
				lastBizRead = candidates[candidates.Count - 1];
				lastBizRead = candidates[candidates.Count - 1];
				var jobConversationSystemMessages = FetchJobConversationMessageRecords(candidates);
				result.AddRange(jobConversationSystemMessages);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public static EdiJobConversationMessage CreateIncident(ZString messageBody, BusinessObjectFactory factory, int createdDateShift = -7)
		{
			var incident = factory.New<IncidentMainBase>();
			createdDateShift = createdDateShift > 0 ? createdDateShift * -1 : createdDateShift;

			var systemCreateTime = DateTime.UtcNow.AddDays(createdDateShift);
			var lastEditTime = DateTime.UtcNow.AddDays(-1);

			incident.IM_SystemCreateTimeUtc = systemCreateTime;
			incident.IM_SystemLastEditTimeUtc = lastEditTime;
			incident.IM_Product = "ENT";
			incident.IM_ProgramArea = "CUS";
			incident.IM_Module = "USA";
			incident.IM_Priority = "CR5";
			incident.IM_RN_NKCountry = "US";
			incident.IM_Description = "Test description";

			factory.Save();

			var messageTimeOffset = 0;

			var incidentRequest = factory.New<IncidentRequest>();
			var jobConversation = factory.New<JobConversation>();
			jobConversation.JCC_ParentID = incidentRequest.PK;
			jobConversation.JCC_ParentTableCode = "INC";
			messageTimeOffset = 0;
			var jobConversationMessage = factory.New<EdiJobConversationMessage>();
			jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
			jobConversationMessage.JCM_IsSystem = true;
			jobConversationMessage.JCM_IsInternal = true;
			jobConversationMessage.JCM_PostedTimeUtc = systemCreateTime.AddMinutes(messageTimeOffset);
			jobConversationMessage.JCM_Body = messageBody;

			incident.IM_INC_Request = incidentRequest.PK;
			incident.IM_IncidentType = "INC";

			factory.Save();
			return jobConversationMessage;
		}

		static IEnumerable<JobConversationMessage> FetchJobConversationMessageRecords(IEnumerable<IncidentMainBase> incidentMainBaseRecords)
		{
			var result = new List<JobConversationMessage>();
			foreach (var incident in incidentMainBaseRecords)
			{
				var query = new ZDBOnlyQuery(typeof(JobConversationMessage));
				var subQuery = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationMessageSchema.JCM_JCC_Conversation);
				query.AddToFilter(JobConversationMessageSchema.JCM_IsSystem, SQLComparisonOperator.Equal, true);
				subQuery.AddToFilter(JobConversationSchema.JCC_ParentID, incident.IM_INC_Request);
				query.AddSubQuery(subQuery, JoinCondition.And);
				var jobConversationMessages = incident.Factory.Load<JobConversationMessage>(query).ToList();
				result.AddRange(jobConversationMessages);
			}
			return result;
		}
	}
}
