using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceChargeLookups : EU.Business.Declaration.InvoiceChargeLookups
	{
		public InvoiceChargeLookups(EU.Business.Declaration.InvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				CodeDescriptionPairList result;
				var invoice = Parent.Invoice;
				if (invoice?.IsImport ?? false)
				{
					result = Factory.GetCachedValue("DEInvoiceChargeLookups.ChargeTypeList_NoAirType", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(base.ChargeTypeList);
						list.RemoveCode(ImportChargeCodeList.Codes.AIR);
						return list;
					});
				}
				else if (invoice?.IsExport ?? false)
				{
					result = Factory.GetCachedValue<ChargeCodeList>();
				}
				else
				{
					result = base.ChargeTypeList;
				}

				return result;
			}
		}
	}
}
