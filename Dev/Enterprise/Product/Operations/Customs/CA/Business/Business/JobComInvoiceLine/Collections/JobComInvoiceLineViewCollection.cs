using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public partial class JobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			using (child.GetValidationSuspender())
			{
				var line = (JobComInvoiceLine)child;
				var header = line.InvoiceHeader;
				if (header != null && header.JZ_IncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid)
				{
					line.CA_CalculationMethod = CalculationMethods.Codes.DeliveredDutyPaid;
				}
			}
		}

		protected override void RebuildCore()
		{
			if (collectionToFilter is not InvoiceLineCompleteCollection completeCollection
				|| completeCollection.AllowRebuildSubCollection)
			{
				base.RebuildCore();
			}
		}
	}
}
