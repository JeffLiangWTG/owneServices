using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return Parent; }
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<Common.BR.BRMessageStatusList>(); }
		}

		public override CodeDescriptionPairList CH_MessageTypeList => Factory.GetCachedValue<MessageTypeList>();

		public CodeDescriptionPairList RiskChannelList => Factory.GetCachedValue<RiskChannelList>();

		public CodeDescriptionPairList CargoStatusList => Factory.GetCachedValue<BRCargoStatusList>();

		public CodeDescriptionPairList AdministrativeStatusList => Factory.GetCachedValue<BRAdministrativeStatusList>();
	}
}
