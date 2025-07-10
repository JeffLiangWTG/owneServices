using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class StmALogAddedToQueueOnly : StmALog, IStmALog
	{
		public StmALogAddedToQueueOnly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			scopeManager = HookupStmALogToScopeManager();
			scopeManager.AddStmALogQueueRowToScope(this);
		}

		public override bool IsSavedByFactory => false;

		protected override bool ShouldCheckIsInDatabaseCore => false;

		protected override void OnDeleteStmALog()
		{
			base.OnDeleteStmALog();
			scopeManager.RemoveStmALogQueueRowFromScope(this);
		}

		readonly StmALogAddedToQueueOnlyScopeManager scopeManager;

		StmALogAddedToQueueOnlyScopeManager HookupStmALogToScopeManager()
		{
			var nonPersistedStmAlogService = Factory?.ServiceContainer.GetAfterOnSavingService<StmALogAddedToQueueOnlyScopeManager>();
			if (nonPersistedStmAlogService == null)
			{
				nonPersistedStmAlogService = new StmALogAddedToQueueOnlyScopeManager(Factory);
				Factory?.ServiceContainer.AddAfterOnSavingService(nonPersistedStmAlogService);
			}
			return nonPersistedStmAlogService;
		}
	}
}
