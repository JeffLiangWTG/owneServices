using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GroupInvoiceChargeLookups : EU.Business.Declaration.GroupInvoiceChargeLookups
	{
		public GroupInvoiceChargeLookups(GroupInvoiceCharge charge) : base(charge)
		{
		}

		public override CodeDescriptionPairList ChargeDistributionBy
		{
			get
			{
				return Factory.GetCachedValue("GB GroupInvoiceCharge DistributeBy", delegate
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
