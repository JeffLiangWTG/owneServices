using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Module;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Module;

public sealed class EntryHeaderMessageNumberFilterGenerator : ModuleFilterGenerator
{
	protected override ModuleFilter GenerateInternal(ZString description)
	{
		var filter = new ModuleTextFilter(description, EDIMessageSchema.EM_MessageNum)
		{
			SubGroup = new MessageNumberFilterSubGroup()
		};
		return filter;
	}
}

sealed class MessageNumberFilterSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var entryHeaderMainQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
		var messageSubQuery = new ZDBOnlySubQuery(typeof(ITEDIMessage), EDIMessageSchema.EM_LinkUniqueID);
		messageSubQuery.AddToFilter(filter);
		entryHeaderMainQuery.AddSubQuery(messageSubQuery, JoinCondition.And);
		return entryHeaderMainQuery;
	}
}
