using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalFiscalReferenceLookups : EU.Business.Declaration.CusFiscalReferenceLookups
	{
		public AdditionalFiscalReferenceLookups(AdditionalFiscalReference parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<EUICS2AdditionalFiscalReferenceTypes>();
	}
}
