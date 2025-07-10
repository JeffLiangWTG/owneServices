using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryHeaderLookups : EU.Business.Declaration.CusEntryHeaderLookups
{
	public CusEntryHeaderLookups(EU.Business.Declaration.CusEntryHeader parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<ITMessageStatusList>();

	public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<ITEntryStatusList>();

	public override CodeDescriptionPairList CH_MessageTypeList => Factory.GetCachedValue<Customs.Business.DeclarationApplicationCodeList>();

	public CodeDescriptionPairList CustomsChannelCodeList => Factory.GetCachedValue<CustomsChannelCodeList>();
}
