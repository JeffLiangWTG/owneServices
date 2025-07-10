using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class JobDeclarationLookups
{
	public override CodeDescriptionPairList SpecificCircumstanceIndicatorList => Parent.IsExport ? Factory.GetCachedValue<NLSpecificCircumstanceIndicatorList>() : base.SpecificCircumstanceIndicatorList;

	public override CodeDescriptionPairList BorderTransportMeansList
	{
		get
		{
			var result = base.BorderTransportMeansList;
			if (Parent.IsExport && Parent.JE_TransportMode.Equals(ModeOfTransportCodeList.Codes._SEA))
			{
				result.DefaultCode = TransportTypeIdList.Codes._11;
			}
			return result;
		}
	}
}
