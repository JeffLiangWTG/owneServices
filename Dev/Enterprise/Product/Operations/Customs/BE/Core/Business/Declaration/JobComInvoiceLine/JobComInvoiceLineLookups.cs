using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	public CodeDescriptionPairList RegionOfDispatchList => Factory.GetCachedValue<BERegionList>();

	protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override ICodeDescriptionPairList PrimaryPreferenceList => (Parent?.InvoiceHeader?.JobDeclaration?.IsUCC6AndIsImport ?? false) ? Factory.GetCachedValue<PreferenceCodesList>() : base.PrimaryPreferenceList;

	public OrgHeaderCollection ExporterList => new OrgHeaderCollection(Factory);
}
