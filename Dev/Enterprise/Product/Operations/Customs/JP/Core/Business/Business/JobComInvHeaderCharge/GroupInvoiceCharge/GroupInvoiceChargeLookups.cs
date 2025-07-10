using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class GroupInvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public GroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		protected new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;

		public override CodeDescriptionPairList ChargeDistributionBy
		{
			get
			{
				var isOFT = Parent.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight;
				return Factory.GetCachedValue($"JP GroupInvoiceCharge DistributeBy {!isOFT}", delegate
				{
					var result = new CodeDescriptionPairList();
					if (!isOFT)
					{
						result.AddPair(ChargeDistributeByList.Codes.Value, ChargeDistributeByList.Descriptions.Value);
						result.AddPair(ChargeDistributeByList.Codes.Quantity, ChargeDistributeByList.Descriptions.Quantity);
					}
					result.AddPair(ChargeDistributeByList.Codes.Volume, ChargeDistributeByList.Descriptions.Volume);
					result.AddPair(ChargeDistributeByList.Codes.Weight, ChargeDistributeByList.Descriptions.Weight);
					return result;
				});
			}
		}
	}
}
