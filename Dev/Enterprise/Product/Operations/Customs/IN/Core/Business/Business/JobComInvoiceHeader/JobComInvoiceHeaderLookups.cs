using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
{
	public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
		: base(parent)
	{
	}

	protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	public override OrgHeaderCollection Exporters => Factory.GetCachedValue("IN|JobComInvoiceHeaderLookups|Exporters", () =>
	{
		var exporters = new ConsignorCollection(Factory);
		exporters.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country/Region", "Property", new ZString(Core.Constants.CountryCodes.India)));
		return exporters;
	});

	public CodeDescriptionPairList IGSTPaymentStatusCodeList => Factory.GetCachedValue<IGSTPaymentStatusCodeList>();

	public OrgHeaderCollection AuthorizedEconomicOperatorsList => authorizedEconomicOperatorsList ??= new OrgHeaderCollection(Factory);
	OrgHeaderCollection authorizedEconomicOperatorsList;

	public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue<INIncoTermList>();

	protected override CodeDescriptionPairList PaymentMethodListCore => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IndiaNatureOfPayment, Parent.EffectiveValuationDate);

	public override OrgHeaderCollection Buyers => buyers ??= new ConsigneeCollection(Factory);
	OrgHeaderCollection buyers;
}
