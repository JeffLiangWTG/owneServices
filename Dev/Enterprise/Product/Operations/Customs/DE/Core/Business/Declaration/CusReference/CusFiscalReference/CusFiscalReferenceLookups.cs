using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusFiscalReferenceLookups : EU.Business.Declaration.CusFiscalReferenceLookups
	{
		public CusFiscalReferenceLookups(EU.Business.Declaration.CusFiscalReference parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<FiscalReferenceCodeList>();
	}
}
