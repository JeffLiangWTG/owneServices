using System;
using System.Net.Http;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class RatesServiceUrlControl : ServiceUrlControl
	{
		protected override void buttonTestConnection_Click(object sender, EventArgs e)
		{
			if (!Uri.TryCreate(textBoxUrl.Text, UriKind.Absolute, out Uri serviceUri))
			{
				Globals.Message.ShowError(Res.GetString("139E1506-F033-4EC6-8F7A-77C7BF7C89A3", "The URL is of invalid format."));
				return;
			}
			try
			{
				var urlGetRequest = new Uri($"{textBoxUrl.Text}/wtg/status");
				using (var client = new HttpClient())
				{
					var response = client.GetAsync(urlGetRequest).ConfigureAwait(false).GetAwaiter().GetResult();
					if (response.IsSuccessStatusCode)
					{
						Globals.Message.Show(Res.GetString("2204e40c-70c9-405b-8ab1-664560c9a5f1", "Test successfully."));
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("5c3732e6-70ba-45fc-92e7-8e073095b6ed", "Test unsuccessful. Please ensure that the Rates Service URL is valid and the service is available."));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(Res.GetString("3ce37cad-468c-4566-8b97-a5ada59d1365", "The Rates Service URL is invalid. Please ensure that it's a valid URL and the service is available."));
				return;
			}
		}
	}
}
