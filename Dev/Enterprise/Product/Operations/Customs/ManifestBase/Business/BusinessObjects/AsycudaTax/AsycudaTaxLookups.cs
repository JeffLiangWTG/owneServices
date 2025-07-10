//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaTaxLookups
//
//    This class should be used for overriding collections in AutoAsycudaTaxLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaTaxLookups : AutoAsycudaTaxLookups
	{
		public AsycudaTaxLookups(AutoAsycudaTax parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList ChargeTypeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList MethodOfPaymentList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList RateOverrideReasonCodeList
		{
			get { return new RateOverrideReasonCodeList(); }
		}
	}
}
