using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	abstract class BaseApplicationHandler : IApplicationHandler
	{
#if DEBUG
		internal
#endif
		IApplicationHandler nextHandler;
		public void Handle(EdiIdentityApplication application)
		{
			if (Applicable(application))
			{
				HandleCore(application);
			}

			nextHandler?.Handle(application);
		}

		public IApplicationHandler SetNext(IApplicationHandler next)
		{
			nextHandler = next;
			return next;
		}

		public abstract bool Applicable(EdiIdentityApplication application);
		protected abstract void HandleCore(EdiIdentityApplication application);
	}
}
