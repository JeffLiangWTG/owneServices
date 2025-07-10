using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionLineGroupingFetchStrategy<TGrouping, TLine> : BusinessObjectFetchStrategy
		where TGrouping : CommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		public CommissionLineGroupingFetchStrategy(CommissionLineGrouping<TGrouping, TLine> grouping)
			: base(grouping)
		{
		}

		new TGrouping BusinessObject
		{
			get { return (TGrouping)base.BusinessObject; }
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(col => col.ColumnName == CommissionLineGrouping<TGrouping, TLine>.Schema.SourceNumber))
			{
				if (BusinessObject.SourceTableCode == JobHeaderSchema.Constants.Prefix)
				{
					Factory.AddFetchHint(JobHeaderSchema.PK, BusinessObject.SourceId);
				}
				else if (BusinessObject.SourceTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					Factory.AddFetchHint(AccTransactionHeaderSchema.PK, BusinessObject.SourceId);
				}
			}
		}

		#endregion
	}
}
