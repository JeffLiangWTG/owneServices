using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public RefCountryCollection CountryList => new RefCountryCollection(Factory);

		public CodeDescriptionPairList InvoiceTypeList => Factory.GetCachedValue<InvoiceTypeList>();

		public CodeDescriptionPairList PreferenceDocumentTypeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.IL.Business.JobComInvoiceHeaderLookups.PreferenceDocumentTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					var loader = new RefCusTradeGroup.Loader(Factory);
					var tradeGroups = loader.Load(Core.Constants.CountryCodes.Israel, ZDateTime.Today, ZString.Empty);
					foreach (var tradeGroup in tradeGroups)
					{
						result.AddPair(tradeGroup.ZZA_TradeGroup, tradeGroup.ZZA_Description);
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList PaymentTermsList => Factory.GetCachedValue<PaymentTermsList>();
	}
}
