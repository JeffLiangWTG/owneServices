
using CargoWise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.BE.Business.Declaration;

[CodeAlive("Template")]
public class JobComInvoiceHeaderLookups : EU.Business.Declaration.JobComInvoiceHeaderLookups
{
	public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
		: base(parent)
	{
	}

	public override ICodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<BEValuationCodeList>();

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	protected new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;
}
