using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	class ConsolidateLVXDeclarationProcessor : IProcessor
	{
		public ConsolidateLVXDeclarationProcessor(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly JobDeclaration declaration;

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			new LVXJobsConsolidateRunner(new NotificationsLogWrapper(notifications), declaration.Factory).ConsolidateLVXJobs(new[] { declaration }, false);
		}

		#endregion

	}
}
