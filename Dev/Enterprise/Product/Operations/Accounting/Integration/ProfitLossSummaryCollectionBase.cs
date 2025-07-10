using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration
{
	public abstract class ProfitLossSummaryCollectionBase : DynamicBusinessObjectCollection<ProfitLossSummaryDetail>
	{
		public ProfitLossSummaryCollectionBase(IJobProfitLoss profitLossParent, IJobCostingPlugIn plugin) : base(profitLossParent.Factory)
		{
			this.ProfitLossParent = profitLossParent;
			this.Plugin = plugin;
		}

		public ProfitLossSummaryCollectionBase(IJobProfitLoss profitLossParent, params ZGuid[] jobPKs) : base(profitLossParent.Factory)
		{
			this.ProfitLossParent = profitLossParent;
			this.ManualJobPKs = jobPKs;
		}

		public abstract void Load();

		#region Implementation

		public readonly IJobProfitLoss ProfitLossParent;

		protected ZGuid[] ManualJobPKs;

		protected IJobCostingPlugIn Plugin;

		#endregion
	}
}
