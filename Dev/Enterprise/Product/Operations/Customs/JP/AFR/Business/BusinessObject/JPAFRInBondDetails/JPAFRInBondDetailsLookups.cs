//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRInBondDetailsLookups
//
//    This class should be used for overriding collections in AutoJPAFRInBondDetailsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRInBondDetailsLookups : AutoJPAFRInBondDetailsLookups
	{
		public JPAFRInBondDetailsLookups(AutoJPAFRInBondDetails parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList TemporaryLandingReasonCodeList
		{
			get { return Factory.GetCachedValue<TemporaryLandingReasonCodeList>(); }
		}

		public ICodeDescriptionPairList TransportModeList
		{
			get { return Factory.GetCachedValue<TransportModeList>(); }
		}
	}
}
