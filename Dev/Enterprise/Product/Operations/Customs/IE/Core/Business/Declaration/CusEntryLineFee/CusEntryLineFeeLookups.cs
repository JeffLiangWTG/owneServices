using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration;

public class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
{
	public CusEntryLineFeeLookups(EU.Business.Declaration.CusEntryLineFee parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<PaymentMethodList>();

	public override CodeDescriptionPairList NationalFeeTypeCodeList => new TaxLookupsCommon(EntryLineFee).TypeList;

	protected override EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon GetNewCommonLookupsHelper() => new TaxLookupsCommon(EntryLineFee);
}
