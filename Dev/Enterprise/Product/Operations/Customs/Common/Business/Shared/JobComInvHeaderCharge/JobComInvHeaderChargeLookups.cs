//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvHeaderChargeLookups
//
//    This class should be used for overriding collections in AutoJobComInvHeaderChargeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class JobComInvHeaderChargeLookups : AutoJobComInvHeaderChargeLookups
	{
		public JobComInvHeaderChargeLookups(AutoJobComInvHeaderCharge parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList ChargeTypeList
		{
			get { return Factory.GetCachedValue<CustomsChargeTypeList>(); }
		}

		public new JobComInvCharge Parent
		{
			get { return (JobComInvCharge)base.Parent; }
		}

		public CodeDescriptionPairList PrepaidCollectList
		{
			get { return Factory.GetCachedValue(OLookUpEditType.PaymentType.GetType().FullName, delegate { return new CodeDescriptionPairList(OLookUpEditType.PaymentType); }); }
		}

		public virtual CodeDescriptionPairList ChargeDistributionBy
		{
			get { return Factory.GetCachedValue<ChargeDistributeByList>(); }
		}

		public CodeDescriptionPairList ApportionmentTypeList
		{
			get { return Factory.GetCachedValue<ApportionmentTypeList>(); }
		}
	}
}

