using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler;

namespace Enterprise.Client.EDI.ServiceTasks
{
	class CustomerApplicationHandlerChainBuilder
	{
		public IApplicationHandler BuildApplicationHandlerChain()
		{
			var applicationHandler = new BaseAddCertificateHandler();
			applicationHandler.SetNext(new BaseRemoveCertificateHandler())
				.SetNext(new BaseRollbackApplicationHandler());
			return applicationHandler;
		} 
	}
}
