using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIECusFiscalReferenceLookups : CusFiscalReferenceLookups
	{
		public DeltaIECusFiscalReferenceLookups(CusFiscalReference parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<DeltaIEFiscalReferenceCodeList>();
	}
}
