using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNTransitDeclarationItemLookups : CusSupportingInfoLookups
	{
		public PBNTransitDeclarationItemLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue("IE.PBN.PBNTransitDeclarationItemLookups.CodeList", () => {
			var codeList = new CodeDescriptionPairList();
			codeList.AddPair(IEPBNDeclarationTypes.Codes.NCTS, IEPBNDeclarationTypes.Descriptions.NCTS);
			return codeList;
		});

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<PBNDeclarationReferenceStatusList>();
	}
}
