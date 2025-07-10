using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<CustomsStatusList>();

		public CodeDescriptionPairList PhaseList => Factory.GetCachedValue<CustomsDeclarationPhases>();
	}
}
