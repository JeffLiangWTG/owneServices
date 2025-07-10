using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class UCC6ImportCusFiscalReferenceLookups : CusFiscalReferenceLookups
	{
		public UCC6ImportCusFiscalReferenceLookups(CusFiscalReference parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<UCC6IMPFiscalReferenceCodeList>();
	}
}
