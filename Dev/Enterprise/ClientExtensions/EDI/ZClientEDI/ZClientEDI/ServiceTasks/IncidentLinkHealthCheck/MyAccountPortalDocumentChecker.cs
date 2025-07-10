using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAutoresponder;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck
{
	public interface IMyAccountPortalDocumentChecker
	{
		bool ValidELearningDocumentUrl(string url);
		IEnumerable<ZString> GetActualMessageLines(JobConversationMessage message);
		IEnumerable<JobConversationMessage> GetCandidatesForCleanup(IEnumerable<JobConversationMessage> jobConversationSystemMessages);
		void CleanMessages(IEnumerable<JobConversationMessage> messages, UrlExtractor urlExtractor);
	}
	public class MyAccountPortalDocumentChecker : IMyAccountPortalDocumentChecker
	{
		readonly HashSet<string> availableELearningUrls;
		readonly string urlsNotInitializedErrorMessage = "availableELearningUrls is not correctly initialized";
		readonly ILogger logger;

		public MyAccountPortalDocumentChecker(ILogger logger, IRecentPdfUpdatesApiClient pdfApiClient)
		{
			logger?.Log(LogType.Debug, $"{nameof(MyAccountPortalDocumentChecker)} constructor started");
			this.logger = logger;
			availableELearningUrls = FetchAvailableELearningUrls(pdfApiClient);
			if (availableELearningUrls == null)
			{
				logger?.Log(LogType.Warning, $"{nameof(MyAccountPortalDocumentChecker)} constructor, {urlsNotInitializedErrorMessage}");
			}
		}
		public bool ValidELearningDocumentUrl(string url)
		{
			logger?.Log(LogType.Debug, $"{nameof(ValidELearningDocumentUrl)} started for url:{url}");
			if (string.IsNullOrWhiteSpace(url) || availableELearningUrls == null)
			{
				return false;
			}
			return availableELearningUrls.Contains(url.Trim().ToLower());
		}
		public IEnumerable<JobConversationMessage> GetCandidatesForCleanup(IEnumerable<JobConversationMessage> jobConversationSystemMessages)
		{
			logger?.Log(LogType.Debug, $"{nameof(GetCandidatesForCleanup)} started");
			var urlExtractor = new UrlExtractor();
			var result = new List<JobConversationMessage>();
			if (jobConversationSystemMessages == null)
			{
				logger?.Log(LogType.Warning, $"{nameof(GetCandidatesForCleanup)}, {nameof(jobConversationSystemMessages)} parameter is null");
				return result;
			}
			if (availableELearningUrls == null)
			{
				logger?.Log(LogType.Warning, $"{nameof(GetCandidatesForCleanup)}, {urlsNotInitializedErrorMessage}, exiting flow without processing.");
				return result;
			}
			foreach (var jcm in jobConversationSystemMessages)
			{
				if (!ContainsUrl(jcm.JCM_Body))
				{
					logger?.Log(LogType.Debug, $"{nameof(GetCandidatesForCleanup)}, skip JCM_Body processing, no valid urls found in JCM_JCC_Conversation: {jcm.JCM_JCC_Conversation}");
					continue;
				}

				var messageLines = GetActualMessageLines(jcm);
				foreach (var message in messageLines)
				{
					if (string.IsNullOrEmpty(message))
					{
						continue;
					}

					logger?.Log(LogType.Information, $"{nameof(GetCandidatesForCleanup)}, processing message:{message}");
					var urls = urlExtractor.ExtractFromExistingMyAccountUrls(message);
					var urlNotFound = false;
					foreach (var url in urls)
					{
						if (!availableELearningUrls.Contains(url.ToLower().Trim()))
						{
							result.Add(jcm);
							urlNotFound = true;
						}
					}

					if (urlNotFound)
					{
						break;
					}
				}
			}
			return result;
		}

		public IEnumerable<ZString> GetActualMessageLines(JobConversationMessage message)
		{
			logger?.Log(LogType.Debug, $"{nameof(GetActualMessageLines)} started");
			if (message == null)
			{
				logger?.Log(LogType.Warning, $"{nameof(GetActualMessageLines)}, {nameof(message)} is null");
				return new List<ZString>();
			}
			return message.JCM_Body.Replace("\r", "")
				.TrimStart('\n')
				.Split('\n')
				.Where(line => !string.IsNullOrWhiteSpace(line));
		}
		public void CleanMessages(IEnumerable<JobConversationMessage> messages, UrlExtractor urlExtractor)
		{
			logger?.Log(LogType.Debug, $"{nameof(CleanMessages)} started");
			if (availableELearningUrls == null)
			{
				logger?.Log(LogType.Warning, $"{nameof(CleanMessages)}, {urlsNotInitializedErrorMessage}");
				return;
			}
			if (messages == null)
			{
				logger?.Log(LogType.Error, $"{nameof(CleanMessages)}, messages parameter is null");
				return;
			}
			if (urlExtractor == null)
			{
				logger?.Log(LogType.Error, $"{nameof(CleanMessages)}, urlExtractor parameter is null");
				return;
			}

			var incidentAutoresponderLogSubscriber = new IncidentAutoresponderLogSubscriber();
			BusinessObjectFactory factory = null;
			var propagateChangesBackToStorage = false;
			foreach (var message in messages)
			{
				if (message == null)
				{
					logger?.Log(LogType.Error, $"{nameof(CleanMessages)}, message is null");
					continue;
				}
				factory = message.Factory;
				var lines = GetActualMessageLines(message);
				if (lines == null)
				{
					logger?.Log(LogType.Error, $"{nameof(CleanMessages)}, lines is null");
					continue;
				}
				var interimResult = new List<ZString>();
				var prevLine = string.Empty;
				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
					{
						interimResult.Add(line);
						prevLine = line;
						continue;
					}
					var urls = urlExtractor.ExtractFromExistingMyAccountUrls(line);
					if (urls == null)
					{
						logger?.Log(LogType.Error, $"{nameof(CleanMessages)} extracted urls is null for the line:{line}");
						continue;
					}
					if (!urls.Any())
					{
						interimResult.Add(line);
						prevLine = line;
						continue;
					}

					var curLine = line;
					foreach (var url in urls)
					{
						logger?.Log(LogType.Information, $"{nameof(CleanMessages)} processing url:{url}");
						if (!availableELearningUrls.Contains(url.ToLower().Trim()))
						{
							curLine = curLine.Replace(url, string.Empty);
							logger?.Log(LogType.Information, $"{nameof(CleanMessages)}, {url} has been removed from {message}");
						}
					}
					propagateChangesBackToStorage = true;
					if (string.IsNullOrWhiteSpace(curLine.Trim()))
					{
						if (interimResult.Count > 1 && !string.IsNullOrEmpty(prevLine) && !urlExtractor.ExtractFromExistingMyAccountUrls(prevLine).Any())
						{
							interimResult.RemoveAt(interimResult.Count - 1);
						}
						prevLine = line;
						continue;
					}
					interimResult.Add(curLine);
					prevLine = line;
				}

				if (interimResult.Any())
				{
					message.JCM_Body = string.Join("\n", interimResult.ToArray());
					if (message.JCM_Body == incidentAutoresponderLogSubscriber.AutoResponderHeader.Replace("\r", ""))
					{
						message.Delete();
					}
				}
			}

			if (propagateChangesBackToStorage && factory != null)
			{
				factory.Save();
			}
		}
		HashSet<string> FetchAvailableELearningUrls(IRecentPdfUpdatesApiClient pdfApiClient)
		{
			logger?.Log(LogType.Debug, $"{nameof(FetchAvailableELearningUrls)} started");
			pdfApiClient = pdfApiClient ?? throw new ArgumentNullException(nameof(pdfApiClient));
			var data = new List<IELearningDocumentFileDescription>();
			var elearningDocs = pdfApiClient.DownloadListOfChanges(ZDateTime.MinSmallDateTimeValue);
			if (elearningDocs == null || !elearningDocs.Any())
			{
				logger?.Log(LogType.Error, $"{nameof(FetchAvailableELearningUrls)}, no data fetched from the ELearningDocumentRecentPDFUpdatesApiClientUrl endpoint");
				return null;
			}
			data.AddRange(elearningDocs);
			var updateNotes = pdfApiClient.DownloadOtherElearningDocumentChanges(ZDateTime.MinSmallDateTimeValue,
				EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl.Value);
			if (updateNotes == null || !updateNotes.Any())
			{
				logger?.Log(LogType.Error, $"{nameof(FetchAvailableELearningUrls)}, no data fetched from the UpdateNotes endpoint");
				return null;
			}
			data.AddRange(updateNotes);
			var technicalAdvisory = pdfApiClient.DownloadOtherElearningDocumentChanges(ZDateTime.MinSmallDateTimeValue,
				EDIDataRegistry.Instance.ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl.Value);
			if (technicalAdvisory == null || !technicalAdvisory.Any())
			{
				logger?.Log(LogType.Error, $"{nameof(FetchAvailableELearningUrls)},  no data fetched from the technicalAdvisory endpoint");
				return null;
			}
			data.AddRange(technicalAdvisory);
			var borderWiseDocuments = pdfApiClient.DownloadOtherElearningDocumentChanges(ZDateTime.MinSmallDateTimeValue,
				EDIDataRegistry.Instance.ELearningDocumentBorderWiseDocumentsApiClientUrl.Value);
			if (borderWiseDocuments == null || !borderWiseDocuments.Any())
			{
				logger?.Log(LogType.Error, $"{nameof(FetchAvailableELearningUrls)}, no data fetched from the borderwiseDocuments endpoint");
				return null;
			}
			data.AddRange(borderWiseDocuments);

			var result = new HashSet<string>();
			foreach (var item in data)
			{
				result.Add(item.Url.Trim().ToLower());
			}
			logger?.Log(LogType.Information, $"{nameof(FetchAvailableELearningUrls)} count return items: {result.Count}");
			return result;
		}
		public bool ContainsUrl(string content) => !string.IsNullOrWhiteSpace(content) && new UrlExtractor().ExtractFromExistingMyAccountUrls(content).Any();
	}
}
