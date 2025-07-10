using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class RollbackApplicationHandler : BaseRollbackApplicationHandler
	{
		readonly ILogger logger;

		public RollbackApplicationHandler(ILogger logger)
		{
			this.logger = logger;
		}

		protected override void RollbackApplication(EdiIdentityApplication application)
		{
			if (!application.IDA_IsRollback)
			{
				application.IDA_IsRollback = true;
			}

			if (!application.IDA_ClientID.IsEmpty)
			{
				var message = $"Rolled back Application '{application.IDA_ApplicationName}' from the tenant '{application.Tenant.IDT_Name}'";
				logger.Log(LogType.Information, message);
			}

			base.RollbackApplication(application);
		}

		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.NeedRollbackAzureApplication();
		}
	}
}
