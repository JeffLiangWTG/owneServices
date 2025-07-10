using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Integration
{
	public abstract class ProfitLossSummaryCollectionBaseView : NonPersistentBusinessObjectCollection<ProfitLossSummaryDetailView>
	{
		public ProfitLossSummaryCollectionBaseView(ProfitLossSummaryCollectionBase profitLossSummaryCollectionToFilter, IJobCostingPlugIn plugin)
			: base(profitLossSummaryCollectionToFilter.Factory)
		{
			this.Plugin = plugin;
			this.profitLossSummaryCollectionToFilter = profitLossSummaryCollectionToFilter;
			FilterProfitLossSummaryCollection();
		}

		protected IJobCostingPlugIn Plugin;

		public ProfitLossSummaryCollectionBase ProfitLossSummaryCollectionToFilter { get { return profitLossSummaryCollectionToFilter; } }
		readonly ProfitLossSummaryCollectionBase profitLossSummaryCollectionToFilter;

		public virtual void FilterProfitLossSummaryCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var row = new DataTable().Rows.Add(System.Array.Empty<object>());
			var profitLoss = new ProfitLossSummaryDetail(Factory, row);
			return new ProfitLossSummaryDetailView(profitLoss);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
