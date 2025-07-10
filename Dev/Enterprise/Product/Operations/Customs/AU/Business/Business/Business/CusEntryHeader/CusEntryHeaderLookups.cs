using Enterprise.Customs.Common.AU;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public CodeDescriptionPairList CPDecQuestionViewTypeList
		{
			get { return Factory.GetCachedValue<CPDecQuestionViewTypeList>(); }
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				if (Parent.Declaration != null)
				{
					return Parent.Declaration.IsImportCMR ? Parent.CMRStatusProvider.MessageStatusList : new LegacyCustomsEntryStatusList();
				}
				return new CodeDescriptionPairList();
			}
		}

		public override CodeDescriptionPairList CH_MessageTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList(base.CH_MessageTypeList);
				result.Add(CANType.CustomsAuthorityNumber);
				return result;
			}
		}
	}
}
