using System.Security.Cryptography.X509Certificates;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class AddCertificateHandler : BaseAddCertificateHandler
	{
		readonly AzureApplicationManagement azureApplicationManagement;
		readonly ILogger logger;

		public AddCertificateHandler(AzureApplicationManagement azureApplicationManagement, ILogger logger)
		{
			this.azureApplicationManagement = azureApplicationManagement;
			this.logger = logger;
		}

		protected override void AddCertificates(EdiIdentityApplication application, X509Certificate2[] certificateData)
		{
			azureApplicationManagement.AddApplicationCertificates(application.IDA_ClientID, certificateData);

			var message = $"Added {certificateData.Length} certificate(s) to Azure Application '{application.IDA_ApplicationName}' on the tenant '{application.Tenant.IDT_Name}'";
			logger.Log(LogType.Information, message);
		}
	}
}
