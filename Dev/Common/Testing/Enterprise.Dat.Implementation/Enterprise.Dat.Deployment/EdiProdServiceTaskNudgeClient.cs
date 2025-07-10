using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dat.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Dat.Implementation
{
	public class EdiProdServiceTaskNudgeClient
	{
		readonly HttpClient client;

		public EdiProdServiceTaskNudgeClient()
			: this(new HttpClient { BaseAddress = new Uri("http://svc-ediprod.corporate.cargowise.com") })
		{
			if (Globals.IsTest)
			{
				throw new InvalidOperationException("Must setup nudge client with fake handler");
			}
		}

		public EdiProdServiceTaskNudgeClient(HttpClient client)
		{
			this.client = client ?? throw new ArgumentNullException(nameof(client));
		}

		public async Task NudgeServiceTaskAsync(string serviceTaskCode, ITaskLogger logger)
		{
			var url = $"Services/NudgeServiceTask?code={serviceTaskCode}&key=9183AC7A-59C3-47DC-A0C4-D96874BA5D6F";
			HttpResponseMessage response;
			try
			{
				response = await client.GetAsync(url).ConfigureAwait(false);
			}
			catch (HttpRequestException ex)
			{
				logger.RecordInfo($"Exception from NudgeServiceTask: {ex}");
				return;
			}
			if (response.IsSuccessStatusCode)
			{
				return;
			}

			var responseContent = response.Content == null ? default : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			logger.RecordInfo($"Unsuccessful response from NudgeServiceTask: {response.StatusCode}, {responseContent}");
		}
	}
}
