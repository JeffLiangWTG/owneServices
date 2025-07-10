using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList JZ_IncoTerm_List => Factory.GetCachedValue("JPIncotermList", () =>
		{
			var result = new IncoTermsCodeDescriptionPairList(IncoTermsListType.Default);
			result.AddPairIfNotExist(IncoTermList.Codes.CostAndInsurance, IncoTermList.Descriptions.CostAndInsurance);
			result.AddPairIfNotExist(IncoTermList.Codes.CostAndFreight, IncoTermList.Descriptions.CostAndFreight);
			result.Sort();

			return result;
		});

		public ICodeDescriptionPairList ValuationTypeCodeList => Factory.GetCachedValue<ValuationTypeCodeList>();

		public CodeDescriptionPairList InvoiceTypes => Factory.GetCachedValue<RepresentativeInvoiceTypes>();

		public CodeDescriptionPairList InvoiceAmountType => Factory.GetCachedValue<InvoicePriceClassificationList>();

		public CodeDescriptionPairList InsuranceTypes => Factory.GetCachedValue<InsuranceTypes>();

		public CodeDescriptionPairList FreightTypes => Factory.GetCachedValue<FreightRatesTypes>();
	}
}
