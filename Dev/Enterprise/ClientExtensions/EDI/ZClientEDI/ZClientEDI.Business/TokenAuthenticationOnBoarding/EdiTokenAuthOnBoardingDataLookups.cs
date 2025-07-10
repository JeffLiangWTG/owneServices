//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiTokenAuthOnBoardingDataLookups
//
//    This class should be used for overriding collections in AutoEdiTokenAuthOnBoardingDataLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business
{
	public class EdiTokenAuthOnBoardingDataLookups : AutoEdiTokenAuthOnBoardingDataLookups
	{
		public static class VerificationResult
		{
			public const string NotVerified = "Not Verified";
			public const string Success = nameof(Success);
			public const string Failed = nameof(Failed);
		}

		public EdiTokenAuthOnBoardingDataLookups(AutoEdiTokenAuthOnBoardingData parent) : base(parent)
		{
		}

		public OIDCClaimMappingIdentifiers ClaimMappingIdentifiersList => new OIDCClaimMappingIdentifiers();

		public OnBoardingStatuses OnBoardingStatusList => new OnBoardingStatuses();

		public OIDCServerTypesList OIDCServerTypesList => new OIDCServerTypesList();

		public SupportIncidentCollection RelatedIncidents => new SupportIncidentCollection(Factory);

		public EdiIdentityTenantCollection Tenants =>
			Factory.GetCachedValue("EdiTokenAuthOnBoardingDataLookups.Tenants", () => new EdiIdentityTenantCollection(Factory, new ZQuery(EdiIdentityTenantSchema.IDT_Onboarding, true)));

		public LicenceEnterpriseCollectionForEntCodeFilter LicenceEnterpriseList =>
			Factory.GetCachedValue("EdiTokenAuthOnBoardingDataLookups.LicenceEnterpriseList", () => new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
	}
}
