using System;
using System.Net.Http;
using Enterprise.Dash.Business.Extensions;
using Enterprise.Dash.Integration.Services;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.Dash.Business.Services
{
	public class DashGlowService : IDashGlowService
	{
		readonly string OrganizationMatchingServiceRelativeUri = (NoResString)"api/dash/organization/match/{0}";
		readonly string ProductCodeMatchingServiceRelativeUri = (NoResString)"api/dash/document/{0}/productcode/match";

		readonly IGlowServiceClientFactory clientFactory;
		readonly IDashErrorReporter dashErrorReporter;

		public DashGlowService(IGlowServiceClientFactory clientFactory, IDashErrorReporter dashErrorReporter)
		{
			this.clientFactory = clientFactory;
			this.dashErrorReporter = dashErrorReporter;
		}

		public HttpResponseMessage MatchOrganisations(Guid docPk)
		{
			return CallGlowEndpointAsync(string.Format(OrganizationMatchingServiceRelativeUri, docPk));
		}

		public HttpResponseMessage MatchProductCodes(Guid docPk)
		{
			return CallGlowEndpointAsync(string.Format(ProductCodeMatchingServiceRelativeUri, docPk));
		}

		HttpResponseMessage CallGlowEndpointAsync(string apiPath)
		{
			using (dashErrorReporter.GatherAdditionalInformation(apiPath))
			{
				using var httpClient = clientFactory.Create(new Uri(GlowRegistry.Instance.GlowServiceUri));

				var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, apiPath);

				var response = httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead)
					.ConfigureAwait(false)
					.GetAwaiter()
					.GetResult();

				if (!response.IsSuccessStatusCode)
				{
					var responseContent = response.Content.ReadAsString();

					if (response.StatusCode.CanRetry())
					{
						throw new HttpRequestException($"Request failed with HttpStatusCode: {response.StatusCode}: {responseContent}");
					}
				}

				return response;
			}
		}
	}
}
