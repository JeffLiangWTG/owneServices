using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class BaseRollbackApplicationHandler : BaseApplicationHandler
	{
		protected override void HandleCore(EdiIdentityApplication application)
		{
			RollbackApplication(application);
		}

		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.IDA_IsRollback;
		}

		protected virtual void RollbackApplication(EdiIdentityApplication application)
		{
			application.IDA_IsActive = false;
			application.Factory.Save();
		}
	}
}
