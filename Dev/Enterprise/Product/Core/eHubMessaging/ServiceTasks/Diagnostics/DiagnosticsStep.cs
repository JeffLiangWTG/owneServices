using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using CargoWise.Common;

namespace Enterprise.eHubMessaging.ServiceTasks.Diagnostics
{
	class DiagnosticsStep
	{
		public DiagnosticsStep(string url)
		{
			Url = new Uri(url);
			resolutionActions = new List<string>();
		}

		public void AddResolutionAction(string resolutionAction)
		{
			resolutionActions.Add(resolutionAction);
		}

		public void Run()
		{
			using (HttpClient httpClient = new HttpClient())
			using (var request = new HttpRequestMessage(HttpMethod.Get, Url))
			{
				IsSuccessful = false;
				httpClient.Timeout = TimeSpan.FromMilliseconds(TimeoutInMilliseconds);
				request.Headers.ConnectionClose = true;

				try
				{
					using (var response = GetResponse(request, httpClient))
					{
						if (response.IsSuccessStatusCode)
						{
							Result = Res.GetString("D42D5346-7D5A-4740-AA6D-29C5943AD141", "Successfully connected to [{0}].", Url.AbsoluteUri);
							IsSuccessful = true;
						}
						else
						{
							Result = Res.GetString("6EAF2767-DA06-4C38-A915-64AAC5BAACBA", "Error connecting to [{0}].\r\nError Status: {1} - {2} ", Url.AbsoluteUri, (int)response.StatusCode, response.ReasonPhrase);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Result = Res.GetString("B93DDF35-5521-4FED-879B-26B63DD9E020", "Error connecting to [{0}].\r\nException Message: {1}", Url.AbsoluteUri, ex.Message);
				}
			}
		}

		internal virtual HttpResponseMessage GetResponse(HttpRequestMessage request, HttpClient httpClient)
		{
			return httpClient.SendAsync(request).Result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		internal virtual int TimeoutInMilliseconds
		{
			get { return 5000; }
		}

		public Uri Url { get; private set; }
		public bool IsSuccessful { get; protected set; }
		public string Result { get; protected set; }
		readonly List<string> resolutionActions;

		public string ResolutionActions
		{
			get
			{
				var text = new StringBuilder();
				text.AppendLine(Res.GetString("bb384480-68b1-4536-bf2b-04070402c445", "To Diagnose Further:"));
				foreach (var resolutionAction in resolutionActions)
				{
					text.AppendLine(resolutionAction);
				}

				return text.ToString();
			}
		}
	}
}
