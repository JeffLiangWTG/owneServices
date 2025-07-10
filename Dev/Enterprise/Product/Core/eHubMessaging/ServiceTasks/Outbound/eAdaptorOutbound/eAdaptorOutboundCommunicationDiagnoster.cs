using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.eHubMessaging.ServiceTasks.Diagnostics;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eAdaptorOutboundCommunicationDiagnosterFactory : IEHubCommunicationDiagnosterFactory
	{
		public IEHubCommunicationDiagnoster Create(string serverAddress) => new eAdaptorOutboundCommunicationDiagnoster(serverAddress);
	}

	class eAdaptorOutboundCommunicationDiagnoster : EHubCommunicationDiagnoster
	{
		public eAdaptorOutboundCommunicationDiagnoster(string serverUrl) : base(serverUrl) { }

		protected override string GenerateResult()
		{
			var result = new StringBuilder();

			foreach (var step in Steps)
			{
				result.AppendLine(step.Result);
				if (step.IsSuccessful)
				{
					break;
				}
			}

			return result.ToString();
		}

		protected override void RunDiagnostics()
		{
			foreach (var step in Steps)
			{
				step.Run();
				if (step.IsSuccessful)
				{
					return;
				}
			}
		}

		protected override void InitializeDiagnosticsStep()
		{
			if (Steps != null)
			{
				return;
			}

			Steps = new List<DiagnosticsStep>
			{
				CreateNewDiagnosticsStep(siteUrl),
				CreateNewDiagnosticsStep(eAdaptorRegistry.Instance.OutboundAdaptorConnectivityTestUrl.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			};
		}
	}
}
