using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.IncidentManager.Test
{
	[TestedType(typeof(IncidentAutoresponderLogSubscriber))]
	class IncidentAutoresponderIntegrationTest : LogSubscriberTest<IncidentAutoresponderLogSubscriber>
	{
		readonly Mock<ILogger> mockLogger = new Mock<ILogger>();
		readonly List<ZGuid> incidentGuids = new List<ZGuid>();

		void LoadIncidents()
		{
			using (var streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ZClientEDI.Test.IncidentManager.BatchProcessor.SupportIncident.IntegrationIncidents.json")))
			{
				var incidentsJArray = JArray.Parse(streamReader.ReadToEnd());
				foreach (JObject incidentJObject in incidentsJArray)
				{
					var incident = Factory.New<IncidentMainBase>();
					incidentGuids.Add(incident.PK.ToGuid());

					var systemCreateTime = DateTime.UtcNow.AddDays(-7);
					var lastEditTime = DateTime.UtcNow.AddDays(-1);

					incident.IM_SystemCreateTimeUtc = systemCreateTime;
					incident.IM_SystemLastEditTimeUtc = lastEditTime;
					incident.IM_Product = incidentJObject["Product"].ToString();
					incident.IM_ProgramArea = incidentJObject["ProgramArea"].ToString();
					incident.IM_Module = incidentJObject["Module"].ToString();
					incident.IM_Priority = incidentJObject["Priority"].ToString();
					incident.IM_RN_NKCountry = incidentJObject["Country"].ToString();
					incident.IM_Description = incidentJObject["Description"].ToString();

					Factory.Save();

					var note = Factory.New<StmNote>();
					note.ST_Description = "Incident Detail";
					note.ST_NoteText = incidentJObject["NoteText"].ToString();
					note.ST_ParentID = incident.PK;
					note.ST_Table = IncidentMainSchema.Constants.TableName;

					var jobConversation = Factory.New<JobConversation>();
					jobConversation.JCC_ParentID = incident.PK;
					jobConversation.JCC_ParentTableCode = "IM";

					Factory.Save();

					var messageTimeOffset = 0;
					foreach (JObject ci in (JArray)incidentJObject["ConversationItems"])
					{
						var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
						jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
						jobConversationMessage.JCM_IsSystem = false;
						jobConversationMessage.JCM_PostedTimeUtc = systemCreateTime.AddMinutes(messageTimeOffset);
						jobConversationMessage.JCM_Body = ci["Body"].ToString();
					}

					var incidentRequest = Factory.New<IncidentRequest>();
					jobConversation = Factory.New<JobConversation>();
					jobConversation.JCC_ParentID = incidentRequest.PK;
					jobConversation.JCC_ParentTableCode = "INC";
					messageTimeOffset = 0;
					foreach (JObject ci in (JArray)incidentJObject["ConversationItems"])
					{
						var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
						jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
						jobConversationMessage.JCM_IsSystem = false;
						jobConversationMessage.JCM_IsInternal = true;
						jobConversationMessage.JCM_PostedTimeUtc = systemCreateTime.AddMinutes(messageTimeOffset);
						jobConversationMessage.JCM_Body = ci["Body"].ToString();
					}
					incident.IM_INC_Request = incidentRequest.PK;
					incident.IM_IncidentType = "INC";

					Factory.Save();
				}
			}
		}

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
					elearningDocumentDescription.ELD_DocumentType =
						documentDescriptionJObject["ELD_DocumentType"].ToString();
				}
			}

			Factory.Save();
		}
		void RunIncidentAssociationService()
		{
			IncidentAssociationDatasetBuilderServiceTask.InitializeIncidentAssociationSystem();

			var incidentAssociationDatasetBuilderRunner = new IncidentAssociationDatasetBuilderRunner(
				mockLogger.Object,
				Factory,
				new BootstrapperProvider(mockLogger.Object),
				new SimilarIncidentRepository());

			if (!incidentAssociationDatasetBuilderRunner.Run(EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value))
			{
				Assert("IAD returned 'false', but should have returned 'true'", false);
			}

			var similarityMatrixBootstrapperRunner = new IncidentAssociationSimilarityMatrixBootstrapperRunner(mockLogger.Object, Factory);

			if (!similarityMatrixBootstrapperRunner.Run(IncidentAssociationSimilarityMatrixBootstrapperRunner.StrideLength))
			{
				Assert("IAM returned 'false', but should have returned 'true'", false);
			}

			var incidentAssociationNewIncidentsRunner = new IncidentAssociationNewIncidentsRunner(new LoggerForTest());
			incidentAssociationNewIncidentsRunner.ProcessNewIncidentVectors();
			incidentAssociationNewIncidentsRunner.ProcessNewIncidentSimilarities(EDIDataRegistry.Instance.RelatedIncidentsMaxTopToStore.Value);
		}

		[StressTest]
		[UseSnapshotProtection]
		public void TestIncidentAutoresponderSystem()
		{
			EDIDataRegistry.Instance.EnableAutoresponder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			LoadIncidents();
			LoadELearningDocumentDescription();

			RunIncidentAssociationService();

			RunLogWalkerCycleForTest();

			var incidentsExpectingAutorespondedMessages = incidentGuids.Take(3).Select(pk => Factory.Load<IncidentMainBase>(pk));
			var expectedMessages = new List<string>
				{
					"Content Auto-Suggester. The following content may be useful in responding to this request:",
					"1COR009 - How do I create a new staff record",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR009.pdf",
					"1COR175 - How do I change or reset a user's password",
					"http://myaccount-portal.cargowise.com/my-account/Documents/UserGuides/Workbooks/1COR175.pdf"
				};
			foreach (var incident in incidentsExpectingAutorespondedMessages)
			{
				var query = new ZQuery(JobConversationSchema.JCC_ParentID, incident.IM_INC_Request);
				var jobConversation = Factory.Load<JobConversation>(query).FirstOrDefault();
				var jobConversationSystemMessages = Factory.Load<JobConversationMessage>(new ZQuery(JobConversationMessageSchema.JCM_JCC_Conversation, jobConversation.PK))
					.Where(jcm => jcm.JCM_IsSystem)
					.ToList();

				Assert("Expecting at least one message that is from system", jobConversationSystemMessages.Count != 0);

				foreach (var jcm in jobConversationSystemMessages)
				{
					var actualMessages = jcm.JCM_Body.Replace("\r", "")
							.TrimStart('\n')
							.Split('\n')
							.Where(line => !string.IsNullOrWhiteSpace(line));
					AssertContainsExactElementsInAnyOrder(expectedMessages, actualMessages);
				}
			}

			var incidentsExpectingNoAutorespondedMessages = incidentGuids.Skip(3).Select(pk => Factory.Load<IncidentMainBase>(pk));
			foreach (var incident in incidentsExpectingNoAutorespondedMessages)
			{
				var query = new ZQuery(JobConversationSchema.JCC_ParentID, incident.IM_INC_Request);
				var jobConversation = Factory.Load<JobConversation>(query).FirstOrDefault();
				var jobConversationMessages = Factory.Load<JobConversationMessage>(new ZQuery(JobConversationMessageSchema.JCM_JCC_Conversation, jobConversation.PK));

				foreach (var jobConversationMessage in jobConversationMessages)
				{
					Assert("Expecting no autoresponded message", !jobConversationMessage.JCM_IsSystem);
				}
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestHandleSjParentIdSafely()
		{
			// Arrange
			var newFactory = new BusinessObjectFactory();
			var mock = new Mock<IQueuedLog>();
			mock.SetupGet(x => x.Factory).Returns(newFactory);
			mock.SetupGet(x => x.SJ_ParentID).Returns(ZGuid.NewZGuid);
			var inputData = new[] { mock.Object };
			var wrapper = new IncidentAutoresponderLogSubscriberForTest();
			// Act
			// Assert
			wrapper.ProcessLogQueueItemsExposed(inputData);
		}
	}
}
