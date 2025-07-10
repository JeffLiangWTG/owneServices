using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using Newtonsoft.Json;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ELearningDocument
{
	public interface IRecentPdfUpdatesApiClient
	{
		IEnumerable<IELearningDocumentFileDescription> DownloadListOfChanges(ZDateTime date);

		IEnumerable<IELearningDocumentFileDescription> DownloadOtherElearningDocumentChanges(ZDateTime date, string eLearningUrl);
	}

	public class RecentPdfUpdatesApiClient : IRecentPdfUpdatesApiClient
	{
		readonly int timeout;
		readonly ILogger logger;

		public RecentPdfUpdatesApiClient(ILogger logger)
		{
			timeout = EDIDataRegistry.Instance.ELearningDocumentMyAccountCallTimeout.Value;
			this.logger = logger;
		}

		public IEnumerable<IELearningDocumentFileDescription> DownloadListOfChanges(ZDateTime date)
		{
			logger?.Log(LogType.Information, $"started download list of changes for {date}");
			var minDate = new DateTime(2000, 1, 1);
			if (date < minDate)
			{
				logger?.Log(LogType.Information, $"passed date value: {date},changing to minimal allowed date value: {minDate}");
				date = minDate;
			}
			var url = EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesApiClientUrl.Value;
			var request = CreateHttpWebRequest(url + $"{date.Year}-{date.Month}-{date.Day}T00:00:00Z");
			request.Timeout = timeout;
			logger?.Log(LogType.Information, $"perform http call {url}, timeout: {timeout}");
			var response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode != HttpStatusCode.OK)
			{
				logger?.Log(LogType.Warning, $"download list of changes return status code:{response.StatusCode}, response: {response}");
				return new List<IELearningDocumentFileDescription>();
			}

			using (var receiveStream = response.GetResponseStream())
			{
				using (var readStream = string.IsNullOrWhiteSpace(response.CharacterSet)
					? new StreamReader(receiveStream ?? throw new InvalidOperationException())
					: new StreamReader(receiveStream ?? throw new InvalidOperationException(),
						Encoding.GetEncoding(response.CharacterSet)))
				{
					var responseData = readStream.ReadToEnd();
					logger?.Log(LogType.Information, $"response data: {responseData}");
					return JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(responseData);
				}
			}
		}

		public virtual HttpWebRequest CreateHttpWebRequest(string requestUriString)
		{
#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			return (HttpWebRequest)WebRequest.Create(requestUriString);
#pragma warning restore SYSLIB0014
		}

		public IEnumerable<IELearningDocumentFileDescription> DownloadOtherElearningDocumentChanges(ZDateTime date, string eLearningUrl)
		{
			logger?.Log(LogType.Debug, $"started download list of changes for {date}");
			if (string.IsNullOrWhiteSpace(eLearningUrl))
			{
				logger?.Log(LogType.Error, $"eLearningUrl is null or empty, stop processing");
				return new List<IELearningDocumentFileDescription>();
			}

			var minDate = new DateTime(2000, 1, 1);
			if (date < minDate)
			{
				logger?.Log(LogType.Information, $"passed date value: {date},changing to minimal allowed date value: {minDate}");
				date = minDate;
			}

#pragma warning disable SYSLIB0014 // WebRequest.Create(Uri) is obsolete: WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			var request = (HttpWebRequest)WebRequest.Create(eLearningUrl + $"{date.Year}-{date.Month}-{date.Day}T00:00:00Z");
#pragma warning restore SYSLIB0014
			request.Timeout = timeout;
			logger?.Log(LogType.Information, $"perform http call {eLearningUrl}, timeout: {timeout}");
			var response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode != HttpStatusCode.OK)
			{
				logger?.Log(LogType.Warning, $"download list of changes return status code:{response.StatusCode}, response: {response}");
				return new List<IELearningDocumentFileDescription>();
			}

			using (var receiveStream = response.GetResponseStream())
			{
				using (var readStream = string.IsNullOrWhiteSpace(response.CharacterSet)
					? new StreamReader(receiveStream ?? throw new InvalidOperationException())
					: new StreamReader(receiveStream ?? throw new InvalidOperationException(),
						Encoding.GetEncoding(response.CharacterSet)))
				{
					var responseData = readStream.ReadToEnd();
					logger?.Log(LogType.Information, $"response data: {responseData}");
					return JsonConvert.DeserializeObject<List<ELearningDocumentFileDescription>>(responseData);
				}
			}
		}
	}
}
