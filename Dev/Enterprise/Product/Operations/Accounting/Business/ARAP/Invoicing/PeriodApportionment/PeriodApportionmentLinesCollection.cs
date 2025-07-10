using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodApportionmentLinesCollection : NonPersistentBusinessObjectCollection<PeriodApportionmentLine>
	{
		public PeriodApportionmentLinesCollection(InvoicingLineBase parent)
		{
			this.parent = parent;
		}

		readonly InvoicingLineBase parent;

		protected override BusinessObject CreateNonPersistentBusinessObject() => new PeriodApportionmentLine(parent, 0, 0);

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
		protected override bool AllowSort => false;
	}
}
