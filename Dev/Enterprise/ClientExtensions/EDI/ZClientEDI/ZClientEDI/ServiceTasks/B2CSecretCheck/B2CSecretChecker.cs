using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.Client.EDI.ServiceTasks.GraphAPI;
using Enterprise.Registry.Business;
using WTG.AzureApplicationIntegration;

namespace Enterprise.Client.EDI.ServiceTasks.B2CSecretCheck
{
	class B2CSecretChecker : IB2CSecretChecker
	{
		internal B2CSecretChecker()
		{
		}

		internal B2CSecretChecker(IGraphService graphService)
		{
			this.graphService = graphService;
		}

		public bool IsCheckerReady(out string notReadyMessage)
		{
			var emptyRegistryValueLocation = new List<string>();

			if (string.IsNullOrEmpty(EDIDataRegistry.Instance.B2CSecretCheckManagementTenantID.Value))
			{
				emptyRegistryValueLocation.Add(EDIDataRegistry.Instance.B2CSecretCheckManagementTenantID.GetLocationInEnglish());
			}

			if (string.IsNullOrEmpty(EDIDataRegistry.Instance.B2CSecretCheckManagementClientID.Value))
			{
				emptyRegistryValueLocation.Add(EDIDataRegistry.Instance.B2CSecretCheckManagementClientID.GetLocationInEnglish());
			}

			if (string.IsNullOrEmpty(EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.Value))
			{
				emptyRegistryValueLocation.Add(EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.GetLocationInEnglish());
			}

			var systemToSystemTrustInfo = SystemDataRegistry.Instance.SystemToSystemCertificate.Value;

			var notReadyMessageBuilder = new StringBuilder();

			if (systemToSystemTrustInfo.CertificateBytes.Length == 0)
			{
				notReadyMessageBuilder.AppendLine("The system does not have the private key for the System to System Trust token authentication.");
			}

			if (emptyRegistryValueLocation.Count > 0)
			{
				notReadyMessageBuilder.AppendLine("Following registries don't have valid value.");
				notReadyMessageBuilder.AppendLine(emptyRegistryValueLocation.Aggregate((a, b) => a + ";\r\n" + b));
			}

			if (notReadyMessageBuilder.Length > 0)
			{
				notReadyMessage = notReadyMessageBuilder.ToString();
				return false;
			}

			notReadyMessage = string.Empty;
			return true;
		}

		public DateTime GetExpiryDate()
		{
			var endDateTimeOffset = GraphService
				.GetMaxExpirationDateAsync(EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.Value)
				.GetAwaiter()
				.GetResult();

			return endDateTimeOffset?.UtcDateTime ?? DateTime.MinValue;
		}

		IGraphService GraphService => graphService ??= GraphServiceFactory.Instance.CreateGraphService(EDIDataRegistry.Instance.B2CSecretCheckManagementTenantID.Value, EDIDataRegistry.Instance.B2CSecretCheckManagementClientID.Value);
		IGraphService graphService;
	}
}
