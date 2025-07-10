//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientLicenceBillingExcludeOrgLookups
//
//    This class should be used for overriding collections in AutoClientLicenceBillingExcludeOrgLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingExcludeOrgLookups : AutoClientLicenceBillingExcludeOrgLookups
	{
		public ClientLicenceBillingExcludeOrgLookups(AutoClientLicenceBillingExcludeOrg parent) : base(parent)
		{
		}

		public CodeDescriptionPairList SystemCodes
		{
			get
			{
				return Factory.GetCachedValue("ClientLicenceBillingExcludeOrgLookups.SystemCodes",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(BillingConstants.BillingSystem.AirlineMessaging, "Airline Messaging (TRAXON)");
						return result;
					});
			}
		}
	}
}

