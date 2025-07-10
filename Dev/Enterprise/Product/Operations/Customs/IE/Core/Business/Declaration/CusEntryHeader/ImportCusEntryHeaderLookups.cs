using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportCusEntryHeaderLookups : CusEntryHeaderLookups
	{
		public ImportCusEntryHeaderLookups(CusEntryHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<AISEntryStatusList>();

		public override CodeDescriptionPairList CH_MessageTypeList => Factory.GetCachedValue<ImportDeclarationTypeList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<IELogicalStatusList>();
	}
}
