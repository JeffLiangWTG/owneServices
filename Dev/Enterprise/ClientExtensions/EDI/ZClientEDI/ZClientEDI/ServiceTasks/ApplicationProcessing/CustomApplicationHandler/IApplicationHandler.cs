using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	interface IApplicationHandler
	{
		void Handle(EdiIdentityApplication application);
		bool Applicable(EdiIdentityApplication application);
		IApplicationHandler SetNext(IApplicationHandler next);
	}
}
