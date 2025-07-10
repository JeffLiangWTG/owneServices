using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Integration
{
	public abstract class ProfitLossDetailCollectionBaseView : NonPersistentBusinessObjectCollection<ProfitLossDetailView>
	{
		public ProfitLossDetailCollectionBaseView(ProfitLossCollectionBase profitLossCollectionToFilter, IJobCostingPlugIn plugin)
			: base(profitLossCollectionToFilter.Factory)
		{
			this.Plugin = plugin;
			this.profitLossCollectionToFilter = profitLossCollectionToFilter;
			FilterProfitLossCollection();
		}

		protected IJobCostingPlugIn Plugin;

		public ProfitLossCollectionBase ProfitLossCollectionToFilter { get { return profitLossCollectionToFilter; } }
		protected ProfitLossCollectionBase profitLossCollectionToFilter;

		public virtual void FilterProfitLossCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var row = new DataTable().Rows.Add(System.Array.Empty<object>());
			var profitLoss = new ProfitLossDetail(Factory, row);
			return new ProfitLossDetailView(profitLoss);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
