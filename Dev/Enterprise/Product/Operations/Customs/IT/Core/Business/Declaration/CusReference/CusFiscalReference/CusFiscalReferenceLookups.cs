using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusFiscalReferenceLookups : EU.Business.Declaration.CusFiscalReferenceLookups
{
	public CusFiscalReferenceLookups(CusFiscalReference parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<FiscalReferenceCodeList>();
}
