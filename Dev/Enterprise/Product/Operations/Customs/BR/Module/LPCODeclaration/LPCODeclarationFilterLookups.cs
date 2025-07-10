using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class LPCODeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public LPCODeclarationFilterLookups(Customs.Module.JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageTypeList
		{
			get
			{
				var declaration = Factory.GetNull<JobDeclaration>();
				declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
				return declaration.Lookups.MessageTypeList;
			}
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<BRMessageStatusList>();

		public override CodeDescriptionPairList EntryStatusList() => Factory.GetCachedValue<LPCOEntryStatusList>();
	}
}
