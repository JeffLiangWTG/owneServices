//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRHeaderLookups
//
//    This class should be used for overriding collections in AutoJPAFRHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderLookups : AutoJPAFRHeaderLookups
	{
		public JPAFRHeaderLookups(AutoJPAFRHeader parent)
			: base(parent)
		{
		}

		protected new JPAFRHeader Parent => (JPAFRHeader)base.Parent;

		public SeaShippingProviderCollection Carriers
		{
			get { return new SeaShippingProviderCollection(Factory); }
		}

		public MessageStatusList MessageStatusList
		{
			get { return Factory.GetCachedValue<MessageStatusList>(); }
		}

		public RefVesselCollection Vessels => Parent.VesselCombination.Vessels;
	}
}
