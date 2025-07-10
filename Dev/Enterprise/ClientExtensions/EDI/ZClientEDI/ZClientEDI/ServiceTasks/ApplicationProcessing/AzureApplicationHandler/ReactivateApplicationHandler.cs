using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Integration;
using ILogger = Enterprise.Integration.ILogger;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class ReactivateApplicationHandler : BaseApplicationHandler
	{
		readonly ILogger logger;

		public ReactivateApplicationHandler(ILogger logger)
		{
			this.logger = logger;
		}

		protected override void HandleCore(EdiIdentityApplication application)
		{
			application.IDA_IsActive = true;
			application.IDA_IsRollback = false;
			application.Factory.Save();
			logger.Log(LogType.Information, $"Activate Application '{application.IDA_ApplicationName}' on the tenant '{application.Tenant.IDT_Name}'");
		}
		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.NeedReactivateApplication();
		}
	}
}
