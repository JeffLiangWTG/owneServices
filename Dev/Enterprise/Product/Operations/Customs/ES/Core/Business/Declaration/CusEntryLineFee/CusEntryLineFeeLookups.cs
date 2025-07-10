using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public sealed class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
		{
		}

		new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		public override CodeDescriptionPairList MethodOfPaymentList
		{
			get
			{
				var baseMethodOfPaymentList = base.MethodOfPaymentList;
				if (Parent.EntryLine?.Declaration?.IsImport ?? false)
				{
					var isVat = Parent.IsVAT;
					return Factory.GetCachedValue("ES.CusEntryLineFeeLookups.MethodOfPaymentList.IsImport&&IsVAT" + isVat, () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(baseMethodOfPaymentList);
						if (isVat)
						{
							result.AddPair(UniversalReferenceConstants.FeeMethodOfPayment.Deferred, VATDeferredDescription);
						}
						result.AddPair(UniversalReferenceConstants.FeeMethodOfPayment.NonBillableTax, NonBillableTaxDescription);
						return result;
					});
				}
				return baseMethodOfPaymentList;
			}
		}

		static string VATDeferredDescription => Res.GetString("BAA227A8-6384-426D-9F37-5376294DE1D9", "VAT Deferred");
		static string NonBillableTaxDescription => Res.GetString("77A629A5-0F75-46B6-B625-915326E5B01E", "Non-billable Tax");
	}
}
