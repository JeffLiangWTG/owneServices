using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList JZ_IncoTerm_List => Parent.JobDeclaration is JobDeclaration ? Factory.GetCachedIncoTermListEU(true) : base.JZ_IncoTerm_List;

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;
		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;
	}
}
