using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoCusEntryHeaderLookups : EU.Business.Declaration.AddInfoCusEntryHeaderLookups
{
	public AddInfoCusEntryHeaderLookups(EU.Business.Declaration.AddInfoCusEntryHeader parent) : base(parent)
	{
	}

	public CodeDescriptionPairList AmendmentStatusList => Factory.GetCachedValue<AmendmentStatusList>();
}
