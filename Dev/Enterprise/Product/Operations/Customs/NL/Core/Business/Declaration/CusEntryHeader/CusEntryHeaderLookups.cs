using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusEntryHeaderLookups : EU.Business.Declaration.CusEntryHeaderLookups
{
	public CusEntryHeaderLookups(CusEntryHeader parent) : base(parent)
	{
	}

	public new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

	public CodeDescriptionPairList EntryPhaseStatusList => Factory.GetCachedValue<CustomsEntryPhaseStatusList>();

	public override CodeDescriptionPairList MessageStatusList
	{
		get
		{
			if (!Parent.CH_StatusInfo.ReadOnly)
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(CustomsEntryMessageStatusList.Codes.ACC, CustomsEntryMessageStatusList.Descriptions.ACC);
				return result;
			}
			else
			{
				return Factory.GetCachedValue<CustomsEntryMessageStatusList>();
			}
		}
	}

	public override CodeDescriptionPairList CH_EntryStatusList
	{
		get
		{
			var entryStatusReadOnly = Parent.CH_EntryStatusInfo.ReadOnly;
			return Factory.GetCachedValue($"NL.CusEntryHeaderLookups.CH_EntryStatusList|{entryStatusReadOnly}", () =>
			{
				var result = base.CH_EntryStatusList;
				if (!entryStatusReadOnly)
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(EntryStatusNew.PhysicalInspection, result.GetDescriptionFromCode(EntryStatusNew.PhysicalInspection));
					list.AddPair(EntryStatusNew.Released, result.GetDescriptionFromCode(EntryStatusNew.Released));
					result = list;
				}
				return result;
			});
		}
	}
}
