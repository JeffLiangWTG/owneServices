using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck
{
	public class IncidentAutoresponderMessageFetcher
	{
		readonly ILogger logger;
		public IncidentAutoresponderMessageFetcher(ILogger logger)
		{
			this.logger = logger;
		}

		IEnumerable<JobConversationMessage> FetchJobConversationMessageRecords(IEnumerable<IncidentMainBase> incidentMainBaseRecords)
		{
			logger?.Log(LogType.Debug, $"{nameof(FetchJobConversationMessageRecords)} started");

			if (incidentMainBaseRecords == null)
			{
				return new List<JobConversationMessage>();
			}

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
			logger?.Log(LogType.Information, $"{nameof(FetchJobConversationMessageRecords)} result count:{result.Count}");
			return result;
		}

		public void RunProcessing(CancellationToken token, BusinessObjectFactory factory, IMyAccountPortalDocumentChecker checker)
		{
			logger?.Log(LogType.Debug, $"{nameof(RunProcessing)} started");
			factory = factory ?? throw new ArgumentNullException(nameof(factory));
			checker = checker ?? throw new ArgumentNullException(nameof(checker));

			var reader = new FilteredBusinessObjectReader<IncidentMainBase>(CreateFilter(), factory) { BatchSize = 100 };
			List<IncidentMainBase> candidates = null;
			BusinessObject lastBizRead = null;
			var urlExtractor = new UrlExtractor();
			while ((candidates = reader.LoadNextBatchInANewFactory(lastBizRead).Cast<IncidentMainBase>().ToList()).Any())
			{
				token.ThrowIfCancellationRequested();
				lastBizRead = candidates[candidates.Count - 1];
				var jobConversationSystemMessages = FetchJobConversationMessageRecords(candidates);
				var candidatesForCleanup = checker.GetCandidatesForCleanup(jobConversationSystemMessages);
				checker.CleanMessages(candidatesForCleanup, urlExtractor);
			}
		}

		ZQuery CreateFilter()
		{
			var daysBack = EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack.Value;
			daysBack = daysBack > 0 ? daysBack * -1 : daysBack;
			var getIncidentQuery = new ZDBOnlyQuery(typeof(IncidentMainBase));
			getIncidentQuery.AddToFilter(IncidentMainSchema.IM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddDays(daysBack));
			return getIncidentQuery;
		}
	}
}
