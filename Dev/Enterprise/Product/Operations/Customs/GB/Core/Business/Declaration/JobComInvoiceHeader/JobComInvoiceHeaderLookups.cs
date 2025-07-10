using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
		protected new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public override CodeDescriptionPairList JZ_IncoTerm_List
		{
			get
			{
				return Invoice.JobDeclaration?.Lookups.IncoTermList ?? new CodeDescriptionPairList();
			}
		}
	}
}
