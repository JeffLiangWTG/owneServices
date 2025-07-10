using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert;

public interface ISqlExecutionPlanRetriever
{
	Task<string> RetrieveExecutionPlan(string searchLink);
}

public class SqlExecutionPlanRetriever : ISqlExecutionPlanRetriever
{
	readonly HttpClient _httpClient;

	public SqlExecutionPlanRetriever() : this(new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
	{
	}

	public SqlExecutionPlanRetriever(HttpClient httpClient = null)
	{
		_httpClient = httpClient ?? new HttpClient();
	}

	public async Task<string> RetrieveExecutionPlan(string searchLink)
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, searchLink);
		HttpResponseMessage response;
		try
		{
			response = await _httpClient.SendAsync(request).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			throw new HttpRequestException($"Failed to retrieve execution plan from {searchLink}.", e);
		}

		if (!response.IsSuccessStatusCode)
		{
			throw new HttpRequestException($"Failed to retrieve execution plan from {searchLink}. Status code: {response.StatusCode}");
		}

		return await response.Content.ReadAsStringAsync();
	}
}
