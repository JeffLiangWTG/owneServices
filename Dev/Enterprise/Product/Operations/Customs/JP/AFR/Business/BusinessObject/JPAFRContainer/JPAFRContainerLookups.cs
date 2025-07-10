//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRContainerLookups
//
//    This class should be used for overriding collections in AutoJPAFRContainerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRContainerLookups : AutoJPAFRContainerLookups
	{
		public JPAFRContainerLookups(AutoJPAFRContainer parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList CustomsConventionContainerApplicationList
		{
			get { return Factory.GetCachedValue<CustomsConventionContainerApplicationList>(); }
		}

		public ICodeDescriptionPairList ContainerOwnershipCodeList
		{
			get { return Factory.GetCachedValue<ContainerOwnershipCodeList>(); }
		}

		public ICodeDescriptionPairList VanningTypeCodeList
		{
			get { return Factory.GetCachedValue<VanningTypeCodeList>(); }
		}

		public ICodeDescriptionPairList ServiceTypeOnDeliveryCodeList
		{
			get { return Factory.GetCachedValue<ServiceTypeOnDeliveryCodeList>(); }
		}
	}
}
