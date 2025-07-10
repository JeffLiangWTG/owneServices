using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
{
	public CusEntryHeaderLookups(CusEntryHeader parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList CH_EntryStatusList => Parent.IsExport ? Factory.GetCachedValue<ExportCustomsStatusList>() : base.CH_EntryStatusList;

	public override CodeDescriptionPairList MessageStatusList => Parent.IsExport ? Factory.GetCachedValue<ExportMessageStatusList>() : base.MessageStatusList;
}
