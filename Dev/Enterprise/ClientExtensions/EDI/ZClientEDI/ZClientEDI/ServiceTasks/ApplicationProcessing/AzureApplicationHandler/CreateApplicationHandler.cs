using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class CreateApplicationHandler : BaseApplicationHandler
	{
		readonly AzureApplicationManagement azureApplicationManagement;
		readonly ILogger logger;

		public CreateApplicationHandler(AzureApplicationManagement azureApplicationManagement, ILogger logger)
		{
			this.azureApplicationManagement = azureApplicationManagement;
			this.logger = logger;
		}

		protected override void HandleCore(EdiIdentityApplication application)
		{
			var appId = azureApplicationManagement.CreateApplication(application.IDA_ApplicationName);
			var message = $"Created the Azure Application '{application.IDA_ApplicationName}' with ClientId '{appId}' on the tenant '{application.Tenant.IDT_Name}'";
			logger.Log(LogType.Information, message);

			application.IDA_ClientID = appId;
			application.Factory.Save();
		}

		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.NeedCreateAzureApplication();
		}
	}
}
