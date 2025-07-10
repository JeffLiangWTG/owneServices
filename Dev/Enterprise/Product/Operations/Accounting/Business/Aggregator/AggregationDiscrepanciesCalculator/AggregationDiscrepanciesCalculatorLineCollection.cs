using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AggregationDiscrepanciesCalculatorLineCollection : NonPersistentBusinessObjectCollection<AggregationDiscrepanciesCalculatorLine>
	{
		public AggregationDiscrepanciesCalculatorLineCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AggregationDiscrepanciesCalculatorLine();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}
	}
}
