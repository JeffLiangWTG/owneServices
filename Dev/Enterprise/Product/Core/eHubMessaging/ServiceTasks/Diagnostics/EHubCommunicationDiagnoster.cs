using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using CargoWise.Common;
using Enterprise.eHubMessaging.ServiceTasks.Diagnostics;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class EHubCommunicationDiagnosterFactory : IEHubCommunicationDiagnosterFactory
	{
		public IEHubCommunicationDiagnoster Create(string serverAddress) => new EHubCommunicationDiagnoster(serverAddress);
	}

	class EHubCommunicationDiagnoster : IEHubCommunicationDiagnoster
	{
		protected readonly string siteUrl;
		protected List<DiagnosticsStep> Steps;

		public EHubCommunicationDiagnoster(string siteUrl)
		{
			if (string.IsNullOrWhiteSpace(siteUrl))
			{
				throw new ArgumentNullException(nameof(siteUrl));
			}

			this.siteUrl = siteUrl;
		}

		public string Run()
		{
			try
			{
				InitializeDiagnosticsStep();

				RunDiagnostics();
				var text = GenerateResult();
				if (string.IsNullOrWhiteSpace(text))
				{
					return string.Empty;
				}

				string caption = Res.GetString("86690561-85CA-45A7-9318-0A7990B52CE1", "Running Diagnostics to check Network Status:");
				return Res.GetString("2662C0FB-4704-494D-959A-B7DF50122B1D", "{0}\r\n\r\n{1}", caption, text);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return Res.GetString("232EA3BC-5188-4556-92B0-3EF62E26DF68", "Error while collecting communication diagnostics information\r\n{0}", ex);
			}
		}

		protected virtual string GenerateResult()
		{
			var result = new StringBuilder();

			for (int i = Steps.Count - 1; i >= 0; i--)
			{
				result.AppendLine(Steps[i].Result);
				if (!Steps[i].IsSuccessful)
				{
					result.AppendLine(Steps[i].ResolutionActions);
				}
			}

			return result.ToString();
		}

		protected virtual void RunDiagnostics()
		{
			foreach (var step in Steps)
			{
				step.Run();
			}
		}

		protected virtual void InitializeDiagnosticsStep()
		{
			if (Steps != null)
			{
				return;
			}

			Steps = new List<DiagnosticsStep>();

			var step1 = CreateNewDiagnosticsStep("http://www.wisetechglobal.com/");
			step1.AddResolutionAction(Res.GetString("DA49306A-787C-4D13-839C-E9B965EC1C27", "Check that the server [{0}] is connected to the network.", System.Environment.MachineName));
			step1.AddResolutionAction(Res.GetString("98BB75B0-2D7E-493F-AF6A-CAAD0B44925B", "Check that the user [{0}] has security rights to connect to the Internet.", WindowsIdentity.GetCurrent().Name));
			step1.AddResolutionAction(Res.GetString("CC529322-B0DA-4F51-820A-08499BFE677D", "Open a browser on the server [{0}] as the user [{1}] and try to access the site [{2}].", System.Environment.MachineName, WindowsIdentity.GetCurrent().Name, step1.Url));

			var step2 = CreateNewDiagnosticsStep(siteUrl);
			step2.AddResolutionAction(Res.GetString("B28FF2BC-D871-4400-B981-4B0D3A71DEEB", "Open a browser and try to access the site [{0}].", step2.Url));

			Steps.Add(step1);
			Steps.Add(step2);
		}

		internal virtual DiagnosticsStep CreateNewDiagnosticsStep(string url)
		{
			return new DiagnosticsStep(url);
		}
	}
}
