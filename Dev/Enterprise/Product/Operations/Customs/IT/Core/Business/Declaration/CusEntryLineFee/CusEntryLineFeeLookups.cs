
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
{
	public CusEntryLineFeeLookups(EU.Business.Declaration.CusEntryLineFee parent) : base(parent)
	{
	}

	protected override EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon GetNewCommonLookupsHelper() => new TaxLookupsCommon(Parent);

	public override CodeDescriptionPairList RateOverrideReasonList
	{
		get
		{
			return Parent.EntryLine?.Declaration is JobDeclaration itJobDeclaration && (itJobDeclaration.IsImport || itJobDeclaration.IsExport)
				? Factory.GetCachedValue<RateOverrideReasonList>()
				: base.RateOverrideReasonList;
		}
	}
}

