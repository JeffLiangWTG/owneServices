using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Module;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class CACusRulingFilterLookups : ZZRefCusRulingFilterLookups
	{
		public CACusRulingFilterLookups(CACusRulingFilterStripBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList RulingTypeList => CACusRulingLookupsExtension.GetRulingTypeList(Factory, base.RulingTypeList);
	}
}
