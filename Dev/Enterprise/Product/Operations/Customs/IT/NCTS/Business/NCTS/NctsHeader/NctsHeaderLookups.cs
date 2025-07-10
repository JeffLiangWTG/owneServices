using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
{
	public NctsHeaderLookups(NctsHeader parent) : base(parent)
	{
	}

	public new NctsHeader Parent => (NctsHeader)base.Parent;

	public OrganisationsFindBoxCollection OrganizationsFindBoxList => new OrganisationsFindBoxCollection(Factory);

	public CodeDescriptionPairList RepresentationTypeList => Factory.GetCachedValue<RepresentationTypeList>();

	public GlbStaffCollection Subscribers => SubscribersProvider.GetSubscribers();

	public CodeDescriptionPairList ProfileList => GetCustomsProfileListProvider().GetAccountDetails(fetchAllIfNoneFound: Parent.IsPhase5Departure);

	public CodeDescriptionPairList AuthorisationNumberList => AuthorizationsProvider.GetAuthorizations(Parent);

	public override CodeDescriptionPairList NctsTransitStatusList => Factory.GetCachedValue<NctsTransitStatusList>();

	#region Implementation

	SubscriberListProvider SubscribersProvider => subscribersProvider ?? (subscribersProvider = new SubscriberListProvider(Factory, Parent));
	SubscriberListProvider subscribersProvider;

	ICustomsProfileListProvider GetCustomsProfileListProvider()
	{
		if (Parent.IsPhase5)
		{
			return new AccountCustomsProfileListProvider(Factory, new NctsHeaderCustomsProfileListLoaderSupportingDataAdapter(Parent));
		}

		return new NodeCustomsProfileListProvider(Factory, new NctsHeaderCustomsProfileListLoaderSupportingDataAdapter(Parent));
	}

	AuthorizationListProvider AuthorizationsProvider => authorizationsProvider ?? (authorizationsProvider = new AuthorizationListProvider(Factory));
	AuthorizationListProvider authorizationsProvider;

	#endregion
}
