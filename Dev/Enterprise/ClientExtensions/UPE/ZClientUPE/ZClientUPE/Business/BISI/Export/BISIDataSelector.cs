using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.BISI
{
	public abstract class BISIDataSelector
	{
		public BISIDataSelector(BusinessObjectFactory factory, ZDateTime startDate, ZDateTime endDate)
		{
			this.Factory = factory;
			this.StartDate = startDate;
			this.EndDate = endDate;
		}

		protected ZQuery TimeStampFilter
		{
			get
			{
				if (fTimeStampFilter == null)
				{
					fTimeStampFilter = new ZQuery(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, StartDate);
					fTimeStampFilter.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.LessThan, EndDate);
					return fTimeStampFilter;
				}
				return fTimeStampFilter;
			}
		}

		public readonly BusinessObjectFactory Factory;
		readonly ZDateTime StartDate;
		readonly ZDateTime EndDate;
		ZQuery fTimeStampFilter;
	}
}
