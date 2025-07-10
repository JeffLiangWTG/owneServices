using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
{
	public CusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
	{
	}

	protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new NLTaxLookupsCommon(EntryLineFee);
}
