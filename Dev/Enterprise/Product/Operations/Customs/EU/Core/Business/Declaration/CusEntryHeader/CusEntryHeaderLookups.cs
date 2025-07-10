using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public CusEntryHeader EntryHeader => Parent;

		protected new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<Common.EU.MessageStatusList>();

		public CodeDescriptionPairList UQList
		{
			get
			{
				return Factory.GetCachedValue("UQList", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Weight.Kilograms, Weight.GetDescription(Weight.Kilograms, PluralState.Plural));
					return list;
				});
			}
		}

		public CodeDescriptionPairList ExportExitStatusList => Factory.GetCachedValue<ExportExitStatus>();
	}
}
