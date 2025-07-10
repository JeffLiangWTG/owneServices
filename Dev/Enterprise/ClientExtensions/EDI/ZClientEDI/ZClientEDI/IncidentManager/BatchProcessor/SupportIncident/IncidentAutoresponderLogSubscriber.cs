using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.Business.IncidentManager.SupportIncident.IncidentAutoresponder;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	[Serializable]
	public class IncidentAutoresponderLogSubscriber : LogSubscriber
	{
		public string AutoResponderHeader
		{
			get
			{
				return Res.GetString("7e89e969-81a1-4dca-9baf-c2f85f8be20d", "Content Auto-Suggester. The following content may be useful in responding to this request:");
			}
		}

		[NonSerialized]
		readonly UrlExtractor urlExtractor = new UrlExtractor();

		[NonSerialized]
		readonly IncidentAutoresponderDocumentTitle incidentAutoresponderDocumentTitle = new IncidentAutoresponderDocumentTitle();

		readonly string incidentLogMessage = "IncidentContentAutoSuggestor response message created";

		#region LogSubscriber Overrides

		public override string Name => "IncidentAutoresponderLogSubscriber";

		public override string[] TableNames => new string[] { IncidentSimilarityTfIdfSchema.Constants.TableName };

		public override string FriendlyName => "Log Subscriber for IncidentAutoresponder.";

		public override string[] EventTypes => new string[] { Events.AddedARecordToTheSystem.Code };

		public override bool IsClientSpecificSubscriber => true;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (EDIDataRegistry.Instance.EnableAutoresponder.Value && queuedLogs is object && queuedLogs.Length > 0)
			{
				var respondedIncidentPks = new HashSet<ZGuid>();

				foreach (var queuedLog in queuedLogs)
				{
					var factory = queuedLog.Factory;
					var incidentSimilarityTfIdfObj = factory.Load<IncidentSimilarityTfIdf>(queuedLog.SJ_ParentID);
					if (incidentSimilarityTfIdfObj == null)
					{
						DefaultLogger?.Log(LogType.Warning, $"IncidentSimilarityTfIdf object not found for SJ_ParentID ={queuedLog.SJ_ParentID}");
						continue;
					}
					var incidentPk = incidentSimilarityTfIdfObj.ISV_IM_Incident;
					var incident = factory.Load<SupportIncident>(incidentPk);
					var incidentRequest = factory.Load<EdiIncidentRequest>(incident.IM_INC_Request);

					if (IsValidIncidentToRespond(factory, incident, incidentRequest, respondedIncidentPks))
					{
						var similarRecords = GetSimilarIncidentsUrls(factory, incidentPk);
						var message = GetMessage(similarRecords);
						if (string.IsNullOrEmpty(message)) { continue; }

						CreateResponse(factory, incidentRequest, message);

						var incidentLogs = (IncidentLogs)incident.GetLogs();
						incidentLogs.AddLog(Events.EditedARecord, incidentLogMessage);
					}
				}
			}
		}

		#endregion

		ZBool IsValidIncidentToRespond(BusinessObjectFactory factory, SupportIncident incident, EdiIncidentRequest request, HashSet<ZGuid> respondedIncidentPks)
		{
			if (respondedIncidentPks.Contains(incident.PK))
			{
				return ZBool.False;
			}

			if (!(incident.IM_Product.ToString().Equals("ENT", StringComparison.OrdinalIgnoreCase)))
			{
				return ZBool.False;
			}

			var daysToScan = EDIDataRegistry.Instance.NewIncidentsDaysToReply.Value;
			if (incident.IM_SystemCreateTimeUtc < ZDateTime.UtcNow.AddDays(-daysToScan))
			{
				return ZBool.False;
			}

			if (request.INC_Status.ToString().Equals("CLS", StringComparison.OrdinalIgnoreCase)
				|| request.INC_Status.ToString().Equals("CAN", StringComparison.OrdinalIgnoreCase))
			{
				return ZBool.False;
			}

			if (incident.IM_Module == Cr9ModuleList.Codes.ReopenClosedGlPeriod)
			{
				return ZBool.False;
			}

			if (incident.IM_Category == SupportIncidentCategoriesList.Codes.Defect)
			{
				return ZBool.False;
			}

			if (incident.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse)
			{
				return ZBool.False;
			}

			var getRespondedLogFilter = new ZQuery(StmALogSchema.SL_Parent, incident.PK);
			getRespondedLogFilter.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, incidentLogMessage));
			var autorespondLogs = factory.Load<StmALog>(getRespondedLogFilter);
			if (autorespondLogs.Any())
			{
				respondedIncidentPks.Add(incident.PK);
				return ZBool.False;
			}

			return ZBool.True;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<MetaSimilarIncidentUrls> GetSimilarIncidentsUrls(BusinessObjectFactory factory, ZGuid incidentPk)
		{
			var maxResultsCount = EDIDataRegistry.Instance.GetSimilarIncidentMaxCount.Value;
			var query = string.Format(
				CultureInfo.InvariantCulture,
				@"WITH results AS
(
    SELECT {0} AS 'SimilarIncidentPk', {1} AS Similarity
    FROM {2}
    WHERE {3} = @SourceIncidentPk
    UNION
    SELECT {3} AS 'SimilarIncidentPk', {1} AS Similarity
    FROM {2}
    WHERE {0} = @SourceIncidentPk
)
SELECT DISTINCT TOP {4} SimilarIncidentPk, Similarity
FROM results
JOIN {5} ON ({6} = SimilarIncidentPk)
JOIN {7} ON ({8} = {9})
ORDER BY Similarity DESC",
				IncidentSimilarityMatrix.Schema.ISM_IM_Incident1,
				IncidentSimilarityMatrix.Schema.ISM_Similarity,
				IncidentSimilarityMatrix.Schema.TableName,
				IncidentSimilarityMatrix.Schema.ISM_IM_Incident2,
				maxResultsCount,
				IncidentMainBase.Schema.TableName,
				IncidentMainBase.Schema.PK,
				IncidentRequest.Schema.TableName,
				IncidentMainBase.Schema.IM_INC_Request,
				IncidentRequest.Schema.PK);
			var candidates = new List<MetaSimilarIncidentUrls>(maxResultsCount);

			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@SourceIncidentPk", SqlDbType.UniqueIdentifier, incidentPk.ToGuid());
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var similarIncidentPk = new ZGuid(reader.GetGuid(0));
						candidates.Add(
							new MetaSimilarIncidentUrls(
								similarIncidentPk,
								similarityScore: reader.GetDecimal(1))
							);
					}
				}
			}

			foreach (var similarIncidentUrls in candidates)
			{
				var urls = urlExtractor.ExtractFromIncident(factory, similarIncidentUrls.SimilarIncidentPk);
				if (!urls.IsNullOrEmpty())
				{
					var documentUrlsWithTitle = incidentAutoresponderDocumentTitle.GetDocumentUrlsWithTitle(urls);
					similarIncidentUrls.Urls = documentUrlsWithTitle;
					yield return similarIncidentUrls;
				}
			}
		}

		void CreateResponse(BusinessObjectFactory factory, BusinessObject incidentRequest, string messageBody)
		{
			var conversation = JobConversation.GetOrCreate(incidentRequest);
			var serviceUser = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, User.ServiceUserCode)).FirstOrDefault();
			JobConversationParticipant participant;

			if (serviceUser is null)
			{
				DefaultLogger.Log(LogType.Error, "Service User is unavailable in the database, unable to create content autosuggestor response");
				return;
			}

			participant = FetchJobConversationParticipant(factory, serviceUser.PK);

			if (participant == null)
			{
				participant = factory.New<JobConversationParticipant>();
				participant.JCP_JCC_Conversation = conversation.PK;
				participant.JCP_ParticipantID = serviceUser.PK;
				participant.JCP_ParticipantTableCode = "GS";
			}

			var message = factory.New<JobConversationMessage>();
			message.JCM_Body = messageBody;
			message.JCM_JCC_Conversation = conversation.PK;
			message.JCM_IsSystem = true;
			message.JCM_IsInternal = true;
			message.JCM_JCP_Participant = participant.PK;
		}

		protected JobConversationParticipant FetchJobConversationParticipant(BusinessObjectFactory factory, ZGuid participantId)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory), $"{nameof(factory)} cannot be null");
			}

			if (participantId == ZGuid.Empty)
			{
				throw new ArgumentException($"{nameof(participantId)} cannot be empty", nameof(participantId));
			}

			return factory.LoadTop1<JobConversationParticipant>(new ZQuery(JobConversationParticipantSchema.JCP_ParticipantID, participantId));
		}

		string GetMessage(IEnumerable<MetaSimilarIncidentUrls> records)
		{
			var urlsLimit = EDIDataRegistry.Instance.RespondedUrlsLimit.Value;
			var urlsAggregator = new Dictionary<string, Dictionary<string, decimal>>(urlsLimit, new ELearningUrlComparer());

			foreach (var record in records)
			{
				if (urlsAggregator.Count == urlsLimit)
				{
					break;
				}

				foreach (string url in record.Urls)
				{
					if (urlsAggregator.ContainsKey(url))
					{
						urlsAggregator[url]["Votes"] += 1;
					}
					else if (urlsAggregator.Count < urlsLimit)
					{
						var urlsInfo = new Dictionary<string, decimal>(2)
						{
							{ "Score", record.SimilarityScore },
							{ "Votes", 1 },
							{ "FinalScore", 0 }
						};
						urlsAggregator.Add(url, urlsInfo);
					}
				}
			}

			foreach (string url in urlsAggregator.Keys)
			{
				var urlInfo = urlsAggregator[url];
				urlInfo["FinalScore"] = urlInfo["Score"] * urlInfo["Votes"];
			}
			var sortedUrls = urlsAggregator
				.OrderByDescending(item => item.Value["FinalScore"])
				.Select(item => item.Key).Distinct()
				.ToList();

			if (sortedUrls.Count == 0)
			{
				return "";
			}

			var msgBuilder = new ZStringBuilder();
			msgBuilder.Append($"{AutoResponderHeader}\r\n");
			sortedUrls.ForEach(url => msgBuilder.Append(url + "\r\n"));
			return msgBuilder.ToString().TrimEnd('\n').TrimEnd('\r');
		}
	}
}
