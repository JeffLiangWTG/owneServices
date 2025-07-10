using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler;
using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks
{
	class AzureApplicationHandlerChainBuilder
	{
		readonly ILogger logger;
		readonly AzureApplicationManagement applicationManagement;

		public AzureApplicationHandlerChainBuilder(AzureApplicationManagement applicationManagement, ILogger logger)
		{
			this.applicationManagement = applicationManagement;
			this.logger = logger;
		}

		public IApplicationHandler BuildApplicationHandlerChain(EdiIdentityApplication application)
		{
			if (application.NeedReactivateApplication())
			{
				return new ReactivateApplicationHandler(logger);
			}

			var applicationHandler = new CreateApplicationHandler(applicationManagement, logger);
			applicationHandler
				.SetNext(new AddCertificateHandler(applicationManagement, logger))
				.SetNext(new RemoveCertificateHandler(applicationManagement, logger))
				.SetNext(new RedirectUrlHandler(applicationManagement, logger))
				.SetNext(new RollbackApplicationHandler(logger));

			return applicationHandler;
		}
	}
}
