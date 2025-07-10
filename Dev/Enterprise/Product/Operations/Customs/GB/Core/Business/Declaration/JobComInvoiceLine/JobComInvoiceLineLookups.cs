using System.Collections;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(EU.Business.Declaration.JobComInvoiceLine parent)
			: base(parent)
		{ }

		public override CodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<ValuationMethodList>();

		public ICollection CountryOfExportList => Parent.Declaration?.Lookups.GoodsOrigin ?? new CodeDescriptionPairList();
	}
}
