using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Module
{
	public class CommercialInvoiceFilterLookups : Customs.Module.CommercialInvoiceFilterLookups
	{
		public CommercialInvoiceFilterLookups(CommercialInvoiceFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageTypes => Factory.GetCachedValue("AUCommercialInvoiceFilterMessageTypes", () =>
		{
			var result = new CodeDescriptionPairList();

			result.AddRange(base.MessageTypes);
			result.AddPair(AUJobMessageTypeList.Codes.Quarantine, AUJobMessageTypeList.Descriptions.Quarantine);

			return result;
		});
	}
}
