using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using WTG.TestHelpers;
using ISVSchema = Enterprise.ZArchitecture.Schema.IncidentSimilarityTfIdfSchema;

namespace Enterprise.Client.EDI.Test
{
	public class IncidentAssociationIntegrationTests : TestCaseWithFactory
	{
		readonly Random random = new Random(314159);
		IEnumerable<dynamic> refSupportIncidents;
		readonly Dictionary<string, WorkItem> pastWorkItems = new Dictionary<string, WorkItem>();
		readonly List<(Guid, Guid)> genPivots = new List<(Guid, Guid)>();

		void ReadIncidentsFromResource()
		{
			using (var streamReader = new StreamReader(new GZipStream(
				File.OpenRead(AssetsHelper.FetchTestAsset("ZClientEDI/content/ServiceTasks/IncidentAssociation/IntegrationIncidents.json.gz")),
				CompressionMode.Decompress)))
			{
				refSupportIncidents = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(streamReader.ReadToEnd());
			}
		}

		void InsertIncidents(int count, int from = 0)
		{
			foreach (var rsi in refSupportIncidents.Skip(from).Take(count))
			{
				var incident = Factory.New<IncidentMainBase>();

				var systemCreateTime = DateTime.UtcNow.AddDays(-random.NextDouble() * 365.0);
				var lastEditTime = systemCreateTime.AddDays(random.NextDouble() * (DateTime.UtcNow - systemCreateTime).TotalDays);

				incident.IM_SystemCreateTimeUtc = systemCreateTime;
				incident.IM_SystemLastEditTimeUtc = lastEditTime;
				incident.IM_Product = rsi.Product.ToString();
				incident.IM_ProgramArea = rsi.ProgramArea.ToString();
				incident.IM_Module = rsi.Module.ToString();
				incident.IM_Priority = rsi.Priority.ToString();
				incident.IM_RN_NKCountry = rsi.Country.ToString();
				incident.IM_Description = rsi.Description.ToString();

				Factory.Save();

				var note = Factory.New<StmNote>();
				note.ST_Description = "Incident Detail";
				note.ST_NoteText = rsi.NoteText.ToString();
				note.ST_ParentID = incident.PK;
				note.ST_Table = IncidentMainSchema.Constants.TableName;

				var jobConversation = Factory.New<JobConversation>();
				jobConversation.JCC_ParentID = incident.PK;
				jobConversation.JCC_ParentTableCode = "IM";

				Factory.Save();

				var messageNumber = 0;
				foreach (var ci in rsi.ConversationItems)
				{
					var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
					jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
					jobConversationMessage.JCM_IsSystem = false;
					jobConversationMessage.JCM_PostedTimeUtc = systemCreateTime.AddSeconds(++messageNumber);
					jobConversationMessage.JCM_Body = ci.Body.ToString();
				}

				var workItemNumber = 0;
				foreach (var wi in rsi.WorkItems)
				{
					WorkItem workItem;
					var workItemId = wi.WorkItemNumber.ToString();

					if (pastWorkItems.ContainsKey(workItemId))
					{
						workItem = pastWorkItems[workItemId];
					}
					else
					{
						workItem = Factory.New<WorkItem>();
						workItem.WKI_WorkItemNumber = workItemId;
						workItem.WKI_Summary = wi.Summary.ToString();
						workItem.WKI_Details = Encoding.UTF8.GetBytes(wi.Details.ToString());
						workItem.WKI_SystemCreateTimeUtc = systemCreateTime.AddSeconds(++workItemNumber);

						Factory.Save();

						pastWorkItems.Add(workItemId, workItem);
					}

					if (!genPivots.Any(gp => gp.Item1 == workItem.PK.ToGuid() && gp.Item2 == incident.PK.ToGuid()))
					{
						var genPivot = Factory.New<GenPivot>();
						genPivot.XX_Relation1ID = workItem.PK;
						genPivot.XX_Relation2ID = incident.PK;

						Factory.Save();

						genPivots.Add((workItem.PK.ToGuid(), incident.PK.ToGuid()));
					}
				}

				Factory.Save();
			}
		}

		ZGuid UpdateOneIncident(bool withinThreshold, int version)
		{
			var descriptionOfIncidentToBeModified = "Customizing Documents"; // Short incident, add one job conversation message will update the TFIDF vector and recompute the TFIDF similarity matrix.
			if (withinThreshold)
			{
				descriptionOfIncidentToBeModified = "Drawback Entries"; // Long incident, add one job conversation message will not lead to updating the TFIDF vector.
			}

			var getIncidentByDescriptionQuery = new ZDBOnlyQuery(typeof(AutoIncidentMain));
			getIncidentByDescriptionQuery.AddToFilter(IncidentMainSchema.IM_Description, SQLComparisonOperator.Equal, descriptionOfIncidentToBeModified);
			getIncidentByDescriptionQuery.AddToFilter(IncidentMainSchema.IM_IncidentType, SQLComparisonOperator.Equal, IncidentConstants.IncidentType.SupportIncident);
			var similarityTfidfSubQuery = new ZDBOnlySubQuery(typeof(AutoIncidentSimilarityTfIdf), IncidentMainSchema.PK);
			similarityTfidfSubQuery.AddToFilter(ISVSchema.ISV_Version, SQLComparisonOperator.Equal, version);
			getIncidentByDescriptionQuery.AddSubQuery(IncidentMainSchema.PK, ISVSchema.ISV_IM_Incident, similarityTfidfSubQuery, JoinCondition.And);

			var queryResultArray = Factory.Load<IncidentMainBase>(getIncidentByDescriptionQuery);

			if (queryResultArray.Length != 1)
			{
				return ZGuid.Empty;
			}

			var incidnetToModify = queryResultArray[0];

			var incidentRequestGuid = InsertOneJobConversationMessage("Problem solved, thanks.");

			incidnetToModify.IM_INC_Request = incidentRequestGuid;
			incidnetToModify.IM_SystemLastEditTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			return incidnetToModify.PK;
		}

		ZGuid InsertOneJobConversationMessage(string message)
		{
			var incidentRequest = Factory.New<IncidentRequest>();
			var jobConversation = Factory.New<JobConversation>();
			jobConversation.JCC_ParentID = incidentRequest.PK;
			jobConversation.JCC_ParentTableCode = "INC";
			var jobConversationMessage = Factory.New<EdiJobConversationMessage>();
			jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
			jobConversationMessage.JCM_IsSystem = ZBool.False;
			jobConversationMessage.JCM_Body = message;

			Factory.Save();

			return incidentRequest.PK;
		}

		int GetCurrentVersion()
		{
			return SimilarIncidentRepository.GetLatestVersion(Factory);
		}

		int GetTokenCount(int? version = null)
		{
			return (int)new IncidentAssociationQuery()
				.QueryObjects("SELECT COUNT(*) AS Count FROM dbo.IncidentSimilarityToken" + (version.HasValue ? $" WHERE IST_Version = {version.Value}" : string.Empty))
				.Single()["Count"];
		}

		int GetMatrixCellCount(int? version = null)
		{
			return (int)new IncidentAssociationQuery()
				.QueryObjects("SELECT COUNT(*) AS Count FROM dbo.IncidentSimilarityMatrix" + (version.HasValue ? $" WHERE ISM_Version = {version.Value}" : string.Empty))
				.Single()["Count"];
		}

		[StressTest]
		[UseSnapshotProtection]
		public void TestIncidentAssociationSystem()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentMain");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentSimilarityToken");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentSimilarityTfIdf");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentSimilarityMatrix");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmData");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmNote");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.JobConversation");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.JobConversationMessage");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.WorkItem");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.GenPivot");

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

			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				ReadIncidentsFromResource();
				InsertIncidents(25, 0);

				var incidentAssociationDatasetBuilderRunner = new IncidentAssociationDatasetBuilderRunner(
					mockLogger.Object,
					Factory,
					new BootstrapperProvider(mockLogger.Object),
					new SimilarIncidentRepository());

				Assert(
					"IAD returned 'true', but should have returned 'false'",
					!incidentAssociationDatasetBuilderRunner.Run(EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value));

				AssertNull("The Incident Association System Status should be null", IncidentAssociationSystemStatus.GetStatus(Factory));
				AssertEquals("The current version should be -1, indicating no version row in StmData", -1, GetCurrentVersion());
				AssertEquals("There should be no tokens in the Tokens table", 0, GetTokenCount());
				IncidentAssociationDatasetBuilderServiceTask.InitializeIncidentAssociationSystem();
				AssertEquals("The current version should be 0", 0, GetCurrentVersion());
				AssertEquals("There should be 20948 tokens in the Tokens table", 20948, GetTokenCount());
				AssertEquals("The Incident Association System Status should be 'Ready to Bootstrap'", IncidentAssociationSystemStatus.ReadyToBootstrap, IncidentAssociationSystemStatus.GetStatus(Factory));

				var newVersion = GetCurrentVersion() + 1;

				// Need a new one now that the tokens have changed:
				incidentAssociationDatasetBuilderRunner = new IncidentAssociationDatasetBuilderRunner(
					mockLogger.Object,
					Factory,
					new BootstrapperProvider(mockLogger.Object),
					new SimilarIncidentRepository());

				Assert(
					"IAD returned 'false', but should have returned 'true'",
					incidentAssociationDatasetBuilderRunner.Run(EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value));

				AssertEquals(0, new SimilarIncidentRepository().RepositoryVersion);
				AssertEquals("There should be 41896 tokens in the Tokens table", 41896, GetTokenCount());
				AssertEquals("There should be 20948 tokens in the Tokens table for version 1", 20948, GetTokenCount(newVersion));

				var vectors = new MetaIncidentSimilarityTfIdfProvider().Load(newVersion).ToList();
				AssertEquals("There should be 25 vectors in the TF-IDF table", 25, vectors.Count);
				Assert("All vectors in the TF-IDF table should be status 'TF-IDF Computed'", vectors.All(v => v.Status == IncidentSimilarityTfIdf.Status.TfIdfComputed));

				var similarityMatrixBootstrapperRunner = new IncidentAssociationSimilarityMatrixBootstrapperRunner(mockLogger.Object, Factory);

				AssertEquals("There should be no rows in the SimilarityMatrix table", 0, GetMatrixCellCount());

				Assert(
					"IAM returned 'false', but should have returned 'true'",
					similarityMatrixBootstrapperRunner.Run(IncidentAssociationSimilarityMatrixBootstrapperRunner.StrideLength));
				AssertEquals(1, GetCurrentVersion());

				vectors = new MetaIncidentSimilarityTfIdfProvider().Load(newVersion).ToList();
				AssertEquals("There should be 25 vectors in the TF-IDF table", 25, vectors.Count);
				Assert("All vectors in the TF-IDF table should be status 'Matrix Computed'", vectors.All(v => v.Status == IncidentSimilarityTfIdf.Status.MatrixComputed));

				AssertEquals("There should be 10 rows in the SimilarityMatrix table", 10, GetMatrixCellCount(newVersion));

				InsertIncidents(10, 25);

				var incidentAssociationNewIncidentsRunner = new IncidentAssociationNewIncidentsRunner(mockLogger.Object);
				incidentAssociationNewIncidentsRunner.NewIncidentLoadStrideLength = 2;
				incidentAssociationNewIncidentsRunner.ProcessedIncidentLoadStrideLength = 2;

				incidentAssociationNewIncidentsRunner.ProcessNewIncidentVectors();

				vectors = new MetaIncidentSimilarityTfIdfProvider().Load(newVersion).ToList();
				AssertEquals("There should be 35 vectors in the TF-IDF table", 35, vectors.Count);
				AssertEquals("There should be 25 vectors in the TF-IDF table with status 'Matrix Computed'", 25, vectors.Count(v => v.Status == IncidentSimilarityTfIdf.Status.MatrixComputed));
				AssertEquals("There should be 10 vectors in the TF-IDF table should be status 'TF-IDF Computed'", 10, vectors.Count(v => v.Status == IncidentSimilarityTfIdf.Status.TfIdfComputed));

				incidentAssociationNewIncidentsRunner.ProcessNewIncidentSimilarities(EDIDataRegistry.Instance.RelatedIncidentsMaxTopToStore.Value);

				vectors = new MetaIncidentSimilarityTfIdfProvider().Load(newVersion).ToList();
				AssertEquals("There should be 35 vectors in the TF-IDF table", 35, vectors.Count);
				Assert("All vectors in the TF-IDF table should be status 'Matrix Computed'", vectors.All(v => v.Status == IncidentSimilarityTfIdf.Status.MatrixComputed));

				AssertEquals("There should be 11 rows in the SimilarityMatrix table", 11, GetMatrixCellCount(newVersion));

				var loggerForDecimalTruncationTest = new LoggerForTest();
				var incidentAssociationNewIncidentsRunnerForDecimalTruncation = new IncidentAssociationNewIncidentsRunner(loggerForDecimalTruncationTest);

				var updatedGuid = UpdateOneIncident(true, GetCurrentVersion());
				AssertNotEquals("There should be only one valid ZGuid get updated and returned", ZGuid.Empty, updatedGuid);

				incidentAssociationNewIncidentsRunnerForDecimalTruncation.ProcessNewIncidentVectors();

				var loggerEntriesList = loggerForDecimalTruncationTest.LogEntries.ToList();
				AssertEquals("There should be 4 entries in the logger", 4, loggerEntriesList.Count);
				AssertEquals("There should be 0 updated vector", "Writing 0 new or updated vectors", loggerEntriesList[3]);

				loggerForDecimalTruncationTest.ClearLog();

				updatedGuid = UpdateOneIncident(false, GetCurrentVersion());
				AssertNotEquals("There should be only one valid ZGuid get updated and returned", ZGuid.Empty, updatedGuid);

				incidentAssociationNewIncidentsRunnerForDecimalTruncation.ProcessNewIncidentVectors();

				loggerEntriesList = loggerForDecimalTruncationTest.LogEntries.ToList();
				AssertEquals("There should be 3 entries in the logger", 3, loggerEntriesList.Count);
				AssertEquals("There should be 1 updated vector writed", "Writing 1 new or updated vectors", loggerEntriesList[2]);

				var minSimilarity = (decimal)new IncidentAssociationQuery()
					.QueryObjects("SELECT MIN(ISM_Similarity) AS Similarity FROM dbo.IncidentSimilarityMatrix WHERE ISM_Version = 1")
					.Single()["Similarity"];
				Assert("Minimum similarity must not be below threshold", minSimilarity >= EDIDataRegistry.Instance.RelatedIncidentsMinimumSimilarity.Value);

				var maxSimilarity = (decimal)new IncidentAssociationQuery()
					.QueryObjects("SELECT MAX(ISM_Similarity) AS Similarity FROM dbo.IncidentSimilarityMatrix WHERE ISM_Version = 1")
					.Single()["Similarity"];
				Assert("Maximum similarity must be not greater than 1", maxSimilarity <= 1);

				var bestMatch = new IncidentAssociationQuery()
					.QueryObjects("SELECT TOP (1) ISM_IM_Incident1, ISM_IM_Incident2, ISM_Similarity FROM dbo.IncidentSimilarityMatrix WHERE ISM_Version = 1 ORDER BY ISM_Similarity DESC")
					.Single();

				var resultsA = new SimilarIncidentRepository()
					.SearchStoredIncidentSimilarities(new SimilarIncidentSearchOptions()
					{
						Count = 1,
						FromTime = new DateTime(2000, 1, 1),
						ToTime = DateTime.UtcNow,
						SourceIncidentPK = (Guid)bestMatch[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1],
					})
					.ToList();

				var resultsB = new SimilarIncidentRepository()
					.SearchStoredIncidentSimilarities(new SimilarIncidentSearchOptions()
					{
						Count = 1,
						FromTime = new DateTime(2000, 1, 1),
						ToTime = DateTime.UtcNow,
						SourceIncidentPK = (Guid)bestMatch[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2],
					})
					.ToList();

				var incident2 = resultsA.Single().GetOtherIncident((Guid)bestMatch[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1]);
				var incident1 = resultsB.Single().GetOtherIncident((Guid)bestMatch[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2]);

				AssertEquals("The guids of Incident2 and BestMatch:Incident2 should match", (Guid)bestMatch[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident2], incident2.PK.ToGuid());
				AssertEquals("The guids of Incident1 and BestMatch:Incident1 should match", (Guid)bestMatch[AutoIncidentSimilarityMatrix.Schema.ISM_IM_Incident1], incident1.PK.ToGuid());

				incidentAssociationDatasetBuilderRunner = new IncidentAssociationDatasetBuilderRunner(
					mockLogger.Object,
					Factory,
					new BootstrapperProvider(mockLogger.Object),
					new SimilarIncidentRepository());

				Assert(
					"IAD returned 'true', but should have returned 'false'",
					!incidentAssociationDatasetBuilderRunner.Run(EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value));

				var stmRow = Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, IncidentAssociationDatasetBuilderRunner.LastDateStmDataName));
				stmRow.SD_BinaryValue = IncidentAssociationDatasetBuilderRunner
					.BlobFromDateTime(DateTime.UtcNow.AddDays(-2.0 * IncidentAssociationDatasetBuilderRunner.DaysBetweenRebuilds));
				Factory.Save();

				Assert(
					"IAD returned 'false', but should have returned 'true'",
					incidentAssociationDatasetBuilderRunner.Run(EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value));

				AssertEquals("Current Version should be 1", 1, GetCurrentVersion());
				newVersion = GetCurrentVersion() + 1;

				AssertEquals("There should be 62844 tokens in the Tokens table", 62844, GetTokenCount());
				AssertEquals("There should be 20948 tokens in the Tokens table for version 2", 20948, GetTokenCount(newVersion));

				var totalVectorCount = (int)new IncidentAssociationQuery()
					.QueryObjects("SELECT COUNT(*) AS Count FROM dbo.IncidentSimilarityTfIdf")
					.Single()["Count"];

				AssertEquals("There should be 70 vectors in the TF-IDF table", 70, totalVectorCount);

				vectors = new MetaIncidentSimilarityTfIdfProvider().Load(newVersion).ToList();
				AssertEquals("There should be 35 vectors in the TF-IDF table", 35, vectors.Count);
				Assert("All vectors in the TF-IDF table should be status 'TF-IDF Computed'", vectors.All(v => v.Status == IncidentSimilarityTfIdf.Status.TfIdfComputed));

				similarityMatrixBootstrapperRunner = new IncidentAssociationSimilarityMatrixBootstrapperRunner(mockLogger.Object, Factory);

				Assert(
					"IAM returned 'false', but should have returned 'true'",
					similarityMatrixBootstrapperRunner.Run(IncidentAssociationSimilarityMatrixBootstrapperRunner.StrideLength));

				AssertEquals(2, new SimilarIncidentRepository().RepositoryVersion);

				vectors = new MetaIncidentSimilarityTfIdfProvider().Load(newVersion).ToList();
				AssertEquals("There should be 35 vectors in the TF-IDF table", 35, vectors.Count);
				Assert("All vectors in the TF-IDF table should be status 'Matrix Computed'", vectors.All(v => v.Status == IncidentSimilarityTfIdf.Status.MatrixComputed));
			}
		}
	}

	public abstract class IncidentAssociationNewIncidentsRunnerIntegrationTestCaseBase : TestCaseWithFactory
	{
		protected static void ResetRelevantTables()
		{
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentMain");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentSimilarityToken");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentSimilarityTfIdf");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.IncidentSimilarityMatrix");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmData");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmNote");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.JobConversation");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.JobConversationMessage");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.WorkItem");
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.GenPivot");
		}

		protected static void LoadIncidentSimilarityTfIdfFromEmbeddedTestResource()
		{
			// SQL script to re-generate test input:
			//
			// DECLARE @from_date date = '2024-05-01';
			// DECLARE @to_date   date = '2024-05-05';
			//
			// SELECT		ROW_NUMBER() OVER (
			// 					PARTITION BY    1
			// 					ORDER BY        ISV_IncidentLastModified ASC
			// 				)                                                   [index],
			// 				newid()                                             pk,
			// 				CONVERT(varchar(max), ISV_TF, 2)                    tf,
			// 				CONVERT(varchar(max), ISV_TFIDF, 2)                 tfidf,
			// 				ISNULL(sim_stats.num_sims, 0)                       num_sims,
			// 				ISNULL(sim_stats.max_sim, 0.0)                      max_sim
			// FROM        	dbo.IncidentSimilarityTfIdf
			// LEFT JOIN   	(
			// 					SELECT      ISV_PK                              ism_isv_pk,
			// 								SUM(num_sims) num_sims,
			// 								MAX(max_sim)  max_sim
			// 					FROM        (
			// 									SELECT      tf1.ISV_PK,
			// 												COUNT(*) num_sims,
			// 												MAX(ISM_Similarity) max_sim
			// 									FROM        dbo.IncidentSimilarityMatrix
			// 									INNER JOIN  dbo.IncidentSimilarityTfIdf tf1
			// 											ON	ISM_IM_Incident1 = tf1.ISV_IM_Incident
			// 									INNER JOIN  dbo.IncidentSimilarityTfIdf tf2
			// 											ON	ISM_IM_Incident2 = tf2.ISV_IM_Incident
			// 									WHERE       tf1.ISV_IncidentLastModified >= @from_date
			// 											AND tf1.ISV_IncidentLastModified <  @to_date
			// 											AND tf2.ISV_IncidentLastModified >= @from_date
			// 											AND tf2.ISV_IncidentLastModified <  @to_date
			// 									GROUP BY    tf1.ISV_PK
			//
			// 									UNION ALL
			// 									SELECT      tf2.ISV_PK,
			// 												COUNT(*) num_sims,
			// 												MAX(ISM_Similarity) max_sim
			// 									FROM        dbo.IncidentSimilarityMatrix
			// 									INNER JOIN  dbo.IncidentSimilarityTfIdf tf1
			// 											ON	ISM_IM_Incident1 = tf1.ISV_IM_Incident
			// 									INNER JOIN  dbo.IncidentSimilarityTfIdf tf2
			// 											ON	ISM_IM_Incident2 = tf2.ISV_IM_Incident
			// 									WHERE       tf1.ISV_IncidentLastModified >= @from_date
			// 											AND tf1.ISV_IncidentLastModified <  @to_date
			// 											AND tf2.ISV_IncidentLastModified >= @from_date
			// 											AND tf2.ISV_IncidentLastModified <  @to_date
			// 									GROUP BY    tf2.ISV_PK
			// 							)                                                               tf_sim
			// 					GROUP BY    ISV_PK
			// 				)																			sim_stats
			// 			ON 	ISV_PK = ism_isv_pk
			// WHERE       	ISV_IncidentLastModified >= @from_date
			// 		   	AND ISV_IncidentLastModified <  @to_date
			// ORDER BY    	ISV_IncidentLastModified ASC

			var fileCachedPath = AssetsHelper.FetchTestAsset("ZClientEDI/content/ServiceTasks/IncidentAssociation/incident_similarity_tfidf_excerpt.csv.gz");
			using var resourceStream = File.OpenRead(fileCachedPath);
			using var gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress);
			using var streamReader = new StreamReader(gzipStream);

			var insertQuery = "INSERT INTO dbo.IncidentSimilarityTfIdf (ISV_PK, ISV_IM_Incident, ISV_Version, ISV_TF, ISV_TFIDF, ISV_Status, ISV_IncidentLastModified) VALUES (@pk, @pk, 1, @tf, @tfidf, @status, @lastModified)";

			// Skip header line
			streamReader.ReadLine();
			var baseDateTime = new DateTime(2024, 5, 1, 0, 0, 0, kind: DateTimeKind.Utc);
			while (true)
			{
				var line = streamReader.ReadLine();
				if (line == null)
				{
					break;
				}

				var parts = line.Split(',');
				AssertEquals(4, parts.Length);

				var rowIndex = int.Parse(parts[0]);
				var pk = Guid.Parse(parts[1]);

				var tf = new byte[parts[2].Length / 2];
				for (var i = 0; i < tf.Length; ++i)
				{
					tf[i] = byte.Parse(parts[2].Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
				}

				var tfidf = new byte[parts[3].Length / 2];
				for (var i = 0; i < tfidf.Length; ++i)
				{
					tfidf[i] = byte.Parse(parts[3].Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
				}

				var status = rowIndex <= 3000 ? IncidentSimilarityTfIdf.Status.MatrixComputed : IncidentSimilarityTfIdf.Status.TfIdfComputed;
				var lastModified = baseDateTime.AddSeconds(rowIndex);

				var pkParameter = ZSqlParameter.New("@pk", pk, ISVSchema.PK);
				var tfParameter = ZSqlParameter.New("@tf", tf, ISVSchema.ISV_TF);
				var tfidfParameter = ZSqlParameter.New("@tfidf", tfidf, ISVSchema.ISV_TFIDF);
				var statusParameter = ZSqlParameter.New("@status", status, ISVSchema.ISV_Status);
				var lastModifiedParameter = ZSqlParameter.New("@lastModified", lastModified, ISVSchema.ISV_IncidentLastModified);

				Db.Connection.ExecuteNonQuery(insertQuery, command =>
				{
					command.AddParameter(pkParameter);
					command.AddParameter(tfParameter);
					command.AddParameter(tfidfParameter);
					command.AddParameter(statusParameter);
					command.AddParameter(lastModifiedParameter);
				});
			}

			var numTfIdfRowsMatrixComputed = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.IncidentSimilarityTfIdf WHERE ISV_Status = 'MCD'");
			var numTfIdfRowsTfIdfComputed = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.IncidentSimilarityTfIdf WHERE ISV_Status = 'ICD'");
			AssertEquals(3000, numTfIdfRowsMatrixComputed);
			AssertEquals(500, numTfIdfRowsTfIdfComputed);
		}

		protected static void VerifySimilarityMatrixAgainstExpectedResultsInEmbeddedTestResources()
		{
			using var expectedResultsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.IncidentAssociation.incident_similarity_matrix_expected.csv.gz");
			using var gzipDecompressStream = new GZipStream(expectedResultsStream, CompressionMode.Decompress);
			using var expectedResults = new StreamReader(gzipDecompressStream, Encoding.UTF8);
			AssertEquals("index,pk1,pk2,similarity", expectedResults.ReadLine());
			var lineNumber = 1;

			Db.Connection.ExecuteReader("SELECT ISM_IM_Incident1, ISM_IM_Incident2, ISM_Similarity FROM dbo.IncidentSimilarityMatrix ORDER BY ISM_IM_Incident1 ASC, ISM_IM_Incident2 ASC", dataRecord =>
			{
				var expectedResultsLine = expectedResults.ReadLine();
				++lineNumber;

				CombineAssertions($"At line {lineNumber}", () =>
				{
					AssertNotNull(expectedResultsLine);
					var parts = expectedResultsLine.Split(',');
					AssertEquals(4, parts.Length);
					var expectedPK1 = Guid.Parse(parts[1]);
					var expectedPK2 = Guid.Parse(parts[2]);
					var expectedSimilarity = (double)decimal.Parse(parts[3]);

					var actualPK1 = dataRecord.GetGuid(0);
					var actualPK2 = dataRecord.GetGuid(1);
					var actualSimilarity = (double)dataRecord.GetDecimal(2);

					AssertEquals("PK1 mismatch", expectedPK1, actualPK1);
					AssertEquals("PK2 mismatch", expectedPK2, actualPK2);
					AssertEquals("Similarity mismatch", expectedSimilarity, actualSimilarity, delta: 0.0001);
				});
			});

			AssertNull(expectedResults.ReadLine());
		}
	}

	public class IncidentAssociationNewIncidentsRunnerIntegrationTestCase : IncidentAssociationNewIncidentsRunnerIntegrationTestCaseBase
	{
		// Question: how does one parameterise tests with CW1's little NUnit clone?
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_DefaultCase() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: null, processedIncidentLoadStrideLength: null);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_100_100() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 100, processedIncidentLoadStrideLength: 100);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_500_100() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 500, processedIncidentLoadStrideLength: 100);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_1000_100() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 1000, processedIncidentLoadStrideLength: 100);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_100_500() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 100, processedIncidentLoadStrideLength: 500);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_500_500() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 500, processedIncidentLoadStrideLength: 500);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_1000_500() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 1000, processedIncidentLoadStrideLength: 500);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_100_10000() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 100, processedIncidentLoadStrideLength: 10000);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_500_10000() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 500, processedIncidentLoadStrideLength: 10000);

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_WithStrides_1000_10000() => TestProcessNewIncidentSimilarities(newIncidentLoadStrideLength: 1000, processedIncidentLoadStrideLength: 10000);

		protected void TestProcessNewIncidentSimilarities(int? newIncidentLoadStrideLength = null, int? processedIncidentLoadStrideLength = null)
		{
			ResetRelevantTables();

			SimilarIncidentRepository.SetLatestVersion(1, Factory);
			AssertEquals(1, SimilarIncidentRepository.GetLatestVersion(Factory));

			LoadIncidentSimilarityTfIdfFromEmbeddedTestResource();

			// ACT
			var runner = new IncidentAssociationNewIncidentsRunner(null);
			if (newIncidentLoadStrideLength.HasValue)
			{
				runner.NewIncidentLoadStrideLength = newIncidentLoadStrideLength.Value;
			}

			if (processedIncidentLoadStrideLength.HasValue)
			{
				runner.ProcessedIncidentLoadStrideLength = processedIncidentLoadStrideLength.Value;
			}

			runner.ProcessNewIncidentSimilarities(maxToStore: 10);

			// ASSERT
			var numSimilarityRows = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.IncidentSimilarityMatrix");
			AssertEquals(4555, numSimilarityRows);

			var numTfIdfRowsMatrixComputed = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.IncidentSimilarityTfIdf WHERE ISV_Status = 'MCD'");
			var numTfIdfRowsTfIdfComputed = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.IncidentSimilarityTfIdf WHERE ISV_Status = 'ICD'");
			AssertEquals(3500, numTfIdfRowsMatrixComputed);
			AssertEquals(0, numTfIdfRowsTfIdfComputed);

			VerifySimilarityMatrixAgainstExpectedResultsInEmbeddedTestResources();
		}
	}

	[NUnit.Framework.DoNotAddToTestTree]
	public class IncidentAssociationNewIncidentsRunnerIntegrationTestResultGenerator : IncidentAssociationNewIncidentsRunnerIntegrationTestCaseBase
	{
		static string IncidentAssociationTestDirectory([CallerFilePath] string sourceFilePath = null)
		{
			AssertNotNull(sourceFilePath);
			return Path.GetDirectoryName(sourceFilePath);
		}

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestProcessNewIncidentSimilarities_GenerateResultsCsv()
		{
			// ACHTUNG
			// This is a dummy test to generate the expected incident similarity matrix data for
			// IncidentAssociationNewIncidentsRunnerIntegrationTestCase

			// ARRANGE
			ResetRelevantTables();

			SimilarIncidentRepository.SetLatestVersion(1, Factory);
			AssertEquals(1, SimilarIncidentRepository.GetLatestVersion(Factory));

			LoadIncidentSimilarityTfIdfFromEmbeddedTestResource();

			// ACT
			var runner = new IncidentAssociationNewIncidentsRunner(null);
			runner.ProcessNewIncidentSimilarities(maxToStore: 10);

			// Export expected similarity matrix results
			using var expectedResultsStream = File.Create(Path.Combine(IncidentAssociationTestDirectory(), "incident_similarity_matrix_expected.csv.gz"));
			using var gzipCompressStream = new GZipStream(expectedResultsStream, CompressionMode.Compress);
			using var expectedResultsOutput = new StreamWriter(gzipCompressStream, Encoding.UTF8);
			expectedResultsOutput.WriteLine("index,pk1,pk2,similarity");
			var resultIndex = 1;
			Db.Connection.ExecuteReader("SELECT ISM_IM_Incident1, ISM_IM_Incident2, ISM_Similarity FROM dbo.IncidentSimilarityMatrix ORDER BY ISM_IM_Incident1 ASC, ISM_IM_Incident2 ASC", dataRecord => {
				expectedResultsOutput.Write(resultIndex++);
				expectedResultsOutput.Write(',');
				expectedResultsOutput.Write(dataRecord.GetGuid(0).ToString("D"));
				expectedResultsOutput.Write(',');
				expectedResultsOutput.Write(dataRecord.GetGuid(1).ToString("D"));
				expectedResultsOutput.Write(',');
				expectedResultsOutput.Write(dataRecord.GetDecimal(2).ToString());
				expectedResultsOutput.WriteLine();
			});
		}
	}

	public class IncidentAssociationNewIncidentsRunnerLoggingTest : IncidentAssociationNewIncidentsRunnerIntegrationTestCaseBase
	{
		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestInfoLoggingFromProcessNewIncidentSimilarities()
		{
			// ARRANGE
			ResetRelevantTables();

			SimilarIncidentRepository.SetLatestVersion(1, Factory);
			AssertEquals(1, SimilarIncidentRepository.GetLatestVersion(Factory));

			LoadIncidentSimilarityTfIdfFromEmbeddedTestResource();

			// ACT
			var logger = new TestLogger();
			var runner = new IncidentAssociationNewIncidentsRunner(logger);
			runner.ProcessNewIncidentSimilarities(maxToStore: 10);

			// ASSERT
			var infoLogEntries = logger.EntriesByType(LogType.Information).ToList();
			AssertEquals(3, infoLogEntries.Count);
			AssertEquals("Processing new incident similarities.", infoLogEntries[0].message);
			AssertEquals("Computing similarities for new incidents [0 - 500] out of 500.", infoLogEntries[1].message);
			Assert(new System.Text.RegularExpressions.Regex(
				"^Computed 4555 similarity scores for 500 new/updated incidents \\(out of 500\\), with peak memory usage estimated at [0-9]+[.]")
				.IsMatch(infoLogEntries[2].message));
		}

		[StressTest]
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestInfoLoggingFromProcessNewIncidentSimilaritiesWithEarlyReturn()
		{
			// ARRANGE
			ResetRelevantTables();

			SimilarIncidentRepository.SetLatestVersion(1, Factory);
			AssertEquals(1, SimilarIncidentRepository.GetLatestVersion(Factory));

			LoadIncidentSimilarityTfIdfFromEmbeddedTestResource();
			using var cts = new CancellationTokenSource();
			var logger = new TestLogger(entry =>
			{
				// Let's cancel the runner after the first iteration
				if (entry.type == LogType.Information && entry.message.StartsWith("Computing similarities for new incidents "))
				{
					cts.Cancel();
				}
			});

			// ACT
			var runner = new IncidentAssociationNewIncidentsRunner(logger);
			runner.NewIncidentLoadStrideLength = 100;
			runner.ProcessNewIncidentSimilarities(maxToStore: 10, cancellationToken: cts.Token);

			// ASSERT
			Assert(cts.Token.IsCancellationRequested);
			var infoLogEntries = logger.EntriesByType(LogType.Information).ToList();
			AssertEquals(4, infoLogEntries.Count);
			AssertEquals("Processing new incident similarities.", infoLogEntries[0].message);
			AssertEquals("Computing similarities for new incidents [0 - 100] out of 500.", infoLogEntries[1].message);
			AssertEquals("The similarity scores for 400 new/updated incidents are yet to be processed.", infoLogEntries[2].message);
			Assert(new System.Text.RegularExpressions.Regex(
				"^Computed [0-9]+ similarity scores for 100 new/updated incidents \\(out of 500\\), with peak memory usage estimated at [0-9]+[.]")
				.IsMatch(infoLogEntries[3].message));
		}

		class TestLogger : ILogger
		{
			readonly List<(LogType type, string message, Exception exception)> entries = new ();
			readonly Action<(LogType type, string message, Exception exception)> logEntryAction;
			public TestLogger(Action<(LogType type, string message, Exception exception)> logEntryAction = null)
			{
				this.logEntryAction = logEntryAction;
			}

			public void Log(LogType type, string message) => Log(type, message, null);
			public void Log(LogType type, string message, Exception exception)
			{
				var entry = (type, message, exception);
				logEntryAction?.Invoke(entry);
				entries.Add(entry);
			}
			public void Clear() => entries.Clear();
			public IEnumerable<(LogType type, string message, Exception exception)> EntriesByType(LogType type) => entries.Where(e => e.type == type);
		}
	}
}
