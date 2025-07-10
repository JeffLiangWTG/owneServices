using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CH_MessageTypeList => Factory.GetCachedValue<EntryTypeList>();

		public CusEntryHeader EntryHeader => Parent;

		protected new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<JobMessageStatusList>();
	}
}
