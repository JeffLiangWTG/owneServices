using System;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public class BaseDbEnvironment : IDbEnvironment
	{
		#region IDbEnvironment Members

		public virtual int ConnectionTimeout => 60;

		public virtual IConnectionPooling ConnectionPooling => noPooling;

		public virtual bool IsServingWebBasedApp => false;

		public virtual IDbConnectionGuiPlugin ConnectionGuiPlugin => new BaseDbConnectionGuiPlugin();

		public IDisposable DisableTimerDuringDbUpgrade()
		{
			disableTimerDuringDbUpgradeCount++;

			return new DisposableAction(() => disableTimerDuringDbUpgradeCount--);
		}

		int disableTimerDuringDbUpgradeCount;

		public bool IsTimerDisabledDuringDbUpgrade => disableTimerDuringDbUpgradeCount > 0;

		public int DeadlockPriority { get; set; }

		public IDbRecoveryModelManager DbRecoveryModelManager => GlobalServiceProvider.Instance.GetRequiredService<IDbRecoveryModelManager>();

		public IDedicatedSqlServerInstanceDefinition DedicatedSqlServerInstanceDefinition => GlobalServiceProvider.Instance.GetRequiredService<IDedicatedSqlServerInstanceDefinition>();

		#endregion

		readonly IConnectionPooling noPooling = new NoConnectionPooling();
	}
}
