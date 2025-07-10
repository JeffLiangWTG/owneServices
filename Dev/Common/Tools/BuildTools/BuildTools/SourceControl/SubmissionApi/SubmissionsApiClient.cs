using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CargoWise.BuildTools
{
	public class SubmissionsApiClient : ISubmissionsApiClient
	{
		public SubmissionsApiClient()
			: this(CreateClient())
		{
		}

		public SubmissionsApiClient(HttpClient httpClient)
		{
			this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			if (httpClient.BaseAddress is null)
			{
				throw new ArgumentException(nameof(httpClient.BaseAddress), "httpClient.BaseAddress must be specified");
			}
		}

		readonly HttpClient httpClient;

		static HttpClient CreateClient()
		{
			var handler = new HttpClientHandler
			{
				PreAuthenticate = true,
				UseDefaultCredentials = true,
			};
			return new HttpClient(handler) { BaseAddress = new Uri("https://crikey.wtg.zone") };
		}

		public void CreateNewSubmission(SubmissionType submissionType, string userName, string taskComments, Guid processTaskPK, string criticality = "")
		{
			var body = new
			{
				UserName = userName,
				Criticality = criticality,
				ActionType = GetActionType(submissionType),
				ProcessTaskPK = processTaskPK,
				Comments = taskComments,
			};
			var json = JsonConvert.SerializeObject(body);
			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = httpClient.PostAsync("/api/submisions/new", content).GetAwaiter().GetResult();
			response.EnsureSuccessStatusCode();
		}

		public Guid GetLatestBuild(string repositoryUrl, int pullRequestId)
		{
			var escapedRepositoryUrl = Uri.EscapeDataString(repositoryUrl);
			var response = httpClient.GetAsync($"/api/submisions/latest-build?targetRepository={escapedRepositoryUrl}&pullRequestId={pullRequestId}").GetAwaiter().GetResult();
			response.EnsureSuccessStatusCode();

			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			if (content is not null && content.StartsWith("\"") && content.EndsWith("\""))
			{
				content = content.Substring(1, content.Length - 2);
			}
			if (Guid.TryParse(content, out var guid))
			{
				return guid;
			}
			throw new InvalidOperationException($"Invalid GUID response received from server: {response.StatusCode}, {content}");
		}

		public async Task<Stream> DownloadLatestTestMethodsFileAsync(string repositoryUrl, string branch = "master", string path = "/", string buildConfiguration = "DEBUG")
		{
			var escapedRepositoryUrl = Uri.EscapeDataString(repositoryUrl);
			var escapedBranch = Uri.EscapeDataString(branch);
			var escapedPath = Uri.EscapeDataString(path);
			var escapedBuildConfiguration = Uri.EscapeDataString(buildConfiguration);

			var response = await httpClient.GetAsync($"/api/submisions/latest-test-methods?targetRepository={escapedRepositoryUrl}&branch={escapedBranch}&path={escapedPath}&buildConfiguration={escapedBuildConfiguration}").ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		}

		public async Task<string> UploadTestFailureDataAndGetUrlAsync(Stream stream, string contentType = "text/plain", string fileExtension = ".txt")
		{
			var url = $"/api/submisions/test-failure-data?contentType={contentType}&fileExtension={fileExtension}";
			using var content = new StreamContent(stream);
			using var response = await httpClient.PostAsync(url, content).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		}

		static string GetActionType(SubmissionType submissionType)
		{
			return submissionType switch
			{
				SubmissionType.TestRun => "SHV",
				SubmissionType.Checkin => "SCH",
				_ => throw new ArgumentException("Invalid submissionType: " + submissionType, nameof(submissionType))
			};
		}
	}
}
