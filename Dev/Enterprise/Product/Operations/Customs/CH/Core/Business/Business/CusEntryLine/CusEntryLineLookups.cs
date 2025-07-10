using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CusEntryLineLookups : Customs.Business.CusEntryLineLookups
{
	public CusEntryLineLookups(CusEntryLine parent)
		: base(parent)
	{
	}

	public new CusEntryLine Parent => (CusEntryLine)base.Parent;

	public CodeDescriptionPairList WeightUQList => Parent.RandomLine.Lookups.WeightUQList;

	public CodeDescriptionPairList CustomsUQList => Parent.RandomLine.Lookups.CustomsUQList;
}
