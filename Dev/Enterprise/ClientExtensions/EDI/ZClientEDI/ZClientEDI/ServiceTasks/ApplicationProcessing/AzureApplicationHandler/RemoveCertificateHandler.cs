using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class RemoveCertificateHandler : BaseRemoveCertificateHandler
	{
		readonly AzureApplicationManagement azureApplicationManagement;
		readonly ILogger logger;

		public RemoveCertificateHandler(AzureApplicationManagement azureApplicationManagement, ILogger logger)
		{
			this.azureApplicationManagement = azureApplicationManagement;
			this.logger = logger;
		}

		protected override void RemoveCertificates(EdiIdentityApplication application, string[] thumbprints)
		{
			azureApplicationManagement.RemoveCertificatesFromApplication(application.IDA_ClientID, thumbprints);

			var message = $"Removed {thumbprints.Length} certificate(s) from Azure Application '{application.IDA_ApplicationName}' on the tenant '{application.Tenant.IDT_Name}'";
			logger.Log(LogType.Information, message);
		}
	}
}
