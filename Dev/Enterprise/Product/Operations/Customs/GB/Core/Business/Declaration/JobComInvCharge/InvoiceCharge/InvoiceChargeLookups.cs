using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceChargeLookups
		: EU.Business.Declaration.InvoiceChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		protected new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		public override CodeDescriptionPairList ChargeDistributionBy
		{
			get
			{
				return Factory.GetCachedValue("GB InvoiceCharge DistributeBy", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value);
					result.AddPair(ChargeDistributeByList.Codes.Weight, ChargeDistributeByList.Descriptions.Weight);
					return result;
				});
			}
		}
	}
}
