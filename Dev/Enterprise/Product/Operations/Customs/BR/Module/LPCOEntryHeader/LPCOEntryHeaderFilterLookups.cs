using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Module;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public class LPCOEntryHeaderFilterLookups : EntryHeaderFilterLookups
	{
		public LPCOEntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
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

		public override CodeDescriptionPairList EntryStatusList(ZString countryCode)
		{
			return Factory.GetCachedValue("BRLPCOFilterLookups_EntryStatusList",
				() => base.EntryStatusList(countryCode).GetEntryStatusList(true));
		}
	}
}
