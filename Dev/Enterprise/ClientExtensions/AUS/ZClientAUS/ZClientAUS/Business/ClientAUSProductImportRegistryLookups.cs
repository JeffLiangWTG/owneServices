//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientAUSProductImportRegistryLookups
//
//    This class should be used for overriding collections in AutoClientAUSProductImportRegistryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSProductImportRegistryLookups : AutoClientAUSProductImportRegistryLookups
	{
		public ClientAUSProductImportRegistryLookups(AutoClientAUSProductImportRegistry parent) : base(parent)
		{
		}

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}

		OrgHeaderCollection fOrganisations;
	}
}
