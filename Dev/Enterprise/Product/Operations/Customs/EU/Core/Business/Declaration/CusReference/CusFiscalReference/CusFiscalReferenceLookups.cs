using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusFiscalReferenceLookups : CommonCusReferenceLookups
	{
		public CusFiscalReferenceLookups(CusFiscalReference parent) : base(parent)
		{
		}

		public new CusFiscalReference Parent
		{
			get { return (CusFiscalReference)base.Parent; }
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<FiscalReferenceCodeList>();
	}
}
