using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WTG.Foundation.Http;
using static Enterprise.Client.EDI.IssueManager.Business.StackLinesWeightsLogAutoAssigner;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class MachineLearningTeamAssignmentRetriever
	{
		public bool IsEnabled => EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.Value && !string.IsNullOrWhiteSpace(ApiUrl);
		public string ApiUrl => EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.Value?.Trim();
		public TimeSpan Timeout => TimeSpan.FromSeconds(EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout.Value);

		public HttpMessageHandler HttpMessageHandler { get; set; } = new HttpClientHandler()
		{
			PreAuthenticate = true,
			UseDefaultCredentials = true,
		};

		public async Task<IssueAssignment> GetIssueAssignmentAsync(StackLine[] stackLines, IEnumerable<AssignmentCandidate> assignmentCandidates, EdiHelpErrorLog log)
		{
			if (stackLines == null || stackLines.Length <= 0 || assignmentCandidates == null || !assignmentCandidates.Any() || log == null)
			{
				return null;
			}

			string requestJson = null;
			string responseJson = null;

			try
			{
				using var httpClient = ObjectFactory.Get<IHttpClientFactory>().CreateNew(HttpMessageHandler);

				httpClient.Timeout = Timeout;

				using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
				requestJson = JsonConvert.SerializeObject(new
				{
					StackLines = stackLines,
					AssignmentCandidates = assignmentCandidates,
					IssuePk = log.PK,
					IssueNumber = log.HE_IssueNumber,
					ExceptionType = log.HE_ExceptionType,
					ExceptionMessage = log.HE_ExceptionMessage,
					ExceptionSource = log.HE_ExceptionSource,
					FirstReported = log.HE_FirstReported.IsValid ? log.HE_FirstReported.ToDateTime() : (DateTime?)null,
					LastReported = log.HE_LastReported.IsValid ? log.HE_LastReported.ToDateTime() : (DateTime?)null,
#if !DEBUG
					ShouldLogToDb = true,
#endif
				}, JsonSerializerSettings);

				request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

				HttpResponseMessage response = null;
				try
				{
					response = await httpClient.SendAsync(request).ConfigureAwait(false);
				}
				catch (TaskCanceledException ex)
				{
					throw new TimeoutException($"Request timeout error ({Timeout.TotalSeconds} seconds).", ex);
				}

				responseJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				response.EnsureSuccessStatusCode();

				Response result;
				try
				{
					result = JsonConvert.DeserializeObject<Response>(responseJson);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new FormatException("Bad Json format of respond.", ex);
				}

				if (result.Status.ToLower() == "success" && result.Assignment != null)
				{
					if (!assignmentCandidates.Select(candidate => candidate.Assignment).Where(candidate => candidate != null)
						.Any(a => a.Equals(result.Assignment)))
					{
						var errorReporterKey = $"{ErrorReporterKeyPrefix}Assignment does not exist in candidates.";
						ErrorReporter.ReportOnce(errorReporterKey, BuildErrorReporterMessage(requestJson, responseJson, "The assignment retrieved from the API does not exist in the candidates."));

						return null;
					}

					result.Assignment.IsRetrievedFromAPI = true;

					return result.Assignment;
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var errorReporterKey = $"{ErrorReporterKeyPrefix}{ex.Message}";
				ErrorReporter.ReportOnce(errorReporterKey, BuildErrorReporterMessage(requestJson, responseJson, ex.Message), ex);

				return null;
			}
		}

		string BuildErrorReporterMessage(string requestJson, string responseJson, string message)
		{
			var messageBuilder = new StringBuilder().AppendLine(message);
			messageBuilder.AppendLine($"API URL: {ApiUrl}");
			messageBuilder.AppendLine("Request JSON:");
			messageBuilder.AppendLine(requestJson ?? "(empty)");
			messageBuilder.AppendLine("Response JSON:");
			messageBuilder.AppendLine(responseJson ?? "(empty)");

			return messageBuilder.ToString();
		}

		const string ErrorReporterKeyPrefix = "MachineLearningTeamAssignment|API|";

		JsonSerializerSettings JsonSerializerSettings => jsonSerializerSettings ??= new JsonSerializerSettings
		{
			ContractResolver = new DefaultContractResolver
			{
				NamingStrategy = new CamelCaseNamingStrategy(),
			},
			DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore,
		};

		JsonSerializerSettings jsonSerializerSettings;

		class Response
		{
			public string Status { get; set; }
			public string Message { get; set; }
			public IssueAssignment Assignment { get; set; }
		}
	}
}
