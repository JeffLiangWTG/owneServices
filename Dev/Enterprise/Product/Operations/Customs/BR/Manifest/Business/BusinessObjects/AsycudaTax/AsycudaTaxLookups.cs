using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaTaxLookups : ASYCUDA.Business.AsycudaTaxLookups
	{
		public AsycudaTaxLookups(AsycudaTax parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<ChargeCodeList>();

		public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.PaymentType);
	}
}

