using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<BRMessageStatusList>();

		public CodeDescriptionPairList AdministrativeStatusList => Factory.GetCachedValue<BRAdministrativeStatusList>();

		public CodeDescriptionPairList CargoStatusList => Factory.GetCachedValue<BRCargoStatusList>();

		public override CodeDescriptionPairList EntryStatusList()
		{
			return Factory.GetCachedValue("BRJobDeclarationFilterLookups_EntryStatusList",
				() => base.EntryStatusList().GetEntryStatusList(false));
		}

		public CodeDescriptionPairList RiskChannelList => Factory.GetCachedValue<RiskChannelList>();
	}
}
