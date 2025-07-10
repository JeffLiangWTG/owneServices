using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class PreviousExpDecLineLookups : CusSupportingInfoLookups
	{
		public PreviousExpDecLineLookups(PreviousExpDecLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList UnitOfQuantityList => Factory.GetCachedValue<InvoiceUnitQuantityCodeList>();
	}
}
