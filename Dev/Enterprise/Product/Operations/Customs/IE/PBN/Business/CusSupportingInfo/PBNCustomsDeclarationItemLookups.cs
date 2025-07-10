using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class PBNCustomsDeclarationItemLookups : CusSupportingInfoLookups
	{
		public PBNCustomsDeclarationItemLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue("IE.PBN.PBNCustomsDeclarationItemLookups.CodeList", () => {
			var codeList = new IEPBNDeclarationTypes();
			codeList.RemoveCode(IEPBNDeclarationTypes.Codes.NCTS);
			return codeList;
		});

		public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<PBNDeclarationReferenceStatusList>();
	}
}
