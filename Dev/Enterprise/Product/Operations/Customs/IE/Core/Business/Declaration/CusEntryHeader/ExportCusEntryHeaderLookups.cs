using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportCusEntryHeaderLookups : CusEntryHeaderLookups
	{
		public ExportCusEntryHeaderLookups(CusEntryHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<AESEntryStatusList>();

		public override CodeDescriptionPairList CH_MessageTypeList => Factory.GetCachedValue<ExportDeclarationTypeList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<IELogicalStatusList>();
	}
}
