using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class GroupInvoiceChargeLookups : EU.Business.Declaration.GroupInvoiceChargeLookups
	{
		public GroupInvoiceChargeLookups(EU.Business.Declaration.GroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var result = base.ChargeTypeList;
				var declaration = Parent.GroupInvoice.JobDeclaration;
				if (declaration?.IsImport ?? false)
				{
					result = Factory.GetCachedValue("DEGroupInvoiceChargeLookups.ChargeTypeList_NoAirType", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(base.ChargeTypeList);
						list.RemoveCode(ImportChargeCodeList.Codes.AIR);
						return list;
					});
				}
				return result;
			}
		}
	}
}
