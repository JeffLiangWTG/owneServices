using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class B2AsClaimForLineViewCollection : BusinessObjectCollectionView<JobComInvoiceLine>
	{
		public B2AsClaimForLineViewCollection(JobComInvoiceLine asAccountForLine, InvoiceLineCompleteCollection completeCollection)
			: base(completeCollection)
		{
			this.asAccountForLine = asAccountForLine;
			Rebuild();
		}

		readonly JobComInvoiceLine asAccountForLine;

		protected override bool AllowNewCore => false;

		protected override bool ShouldWeAddBusinessObjectStraightToView(BusinessObject businessObject)
		{
			return true;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = false;
			if (this.asAccountForLine != null)
			{
				var invoiceLine = (JobComInvoiceLine)element;
				result = !invoiceLine.CA_IsAccountForLine && asAccountForLine.CA_IsAccountForLine && invoiceLine.JI_ParentID == this.asAccountForLine.PK;
			}
			return result;
		}
	}
}
