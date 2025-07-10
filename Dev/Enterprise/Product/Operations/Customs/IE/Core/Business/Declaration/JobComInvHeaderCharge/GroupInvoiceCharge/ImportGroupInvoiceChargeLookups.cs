using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class ImportGroupInvoiceChargeLookups : EU.Business.Declaration.GroupInvoiceChargeLookups
	{
		public ImportGroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		public new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<AISChargeCodeList>();
	}
}
