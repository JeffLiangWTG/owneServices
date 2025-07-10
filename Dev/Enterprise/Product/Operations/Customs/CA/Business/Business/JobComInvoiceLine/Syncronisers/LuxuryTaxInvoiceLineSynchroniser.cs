using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	class LuxuryTaxInvoiceLineSynchroniser : BusinessObjectSynchroniser
	{
		public LuxuryTaxInvoiceLineSynchroniser(JobComInvoiceLine destination, JobComInvoiceLine source)
			: base(destination, source)
		{
		}

		public new JobComInvoiceLine Source => (JobComInvoiceLine)base.Source;

		public new JobComInvoiceLine Destination => (JobComInvoiceLine)base.Destination;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source != null && !Source.IsDeleted && Destination != null && !Destination.IsDeleted && Destination.IsLuxuryTaxInvoiceLine)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_InvoiceQuantityInfo, Source.JI_InvoiceQuantityInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.JI_InvoiceUQInfo, Source.JI_InvoiceUQInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.CA_PageNumberInfo, Source.CA_PageNumberInfo));
			}
		}
	}
}
