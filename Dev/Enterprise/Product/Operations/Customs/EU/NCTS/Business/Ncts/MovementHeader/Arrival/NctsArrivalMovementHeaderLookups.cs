using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalMovementHeaderLookups : NctsCommonMovementHeaderLookups
	{
		public NctsArrivalMovementHeaderLookups(NctsArrivalMovementHeader parent)
			: base(parent)
		{
		}

		public new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

		public OrgHeaderCollection Organisations => OrganisationsCore;

		public CodeDescriptionPairList AuthorizationCodeList => AuthorizationCodeListCore;

		public CusAuthorisationHeaderCollection AuthorizationNumberList => AuthorizationNumberListCore;

		public RefCountryCollection TransportAtDepartureNationalities => TransportAtDepartureNationalitiesCore;

		public CodeDescriptionPairList ModeOfTransportList => ModeOfTransportListCore;

		public RefVesselCollection Vessels => VesselsCore;

		protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<NCTS5ArrivalCustomsStatusList>();

		protected virtual RefCountryCollection TransportAtDepartureNationalitiesCore => new RefCountryCollection(Factory);

		protected virtual CodeDescriptionPairList ModeOfTransportListCore => Factory.GetCachedValue<ModeOfTransportList>();

		protected virtual RefVesselCollection VesselsCore => new RefVesselCollection(Factory);

		protected virtual OrgHeaderCollection OrganisationsCore => new OrgHeaderCollection(Factory);

		protected virtual CodeDescriptionPairList AuthorizationCodeListCore => Factory.GetCachedValue<NctsArrivalAuthorizationCodeList>();

		protected virtual CusAuthorisationHeaderCollection AuthorizationNumberListCore
		{
			get
			{
				var numbers = new CusAuthorisationHeaderCollectionFiltered(Factory, Parent.AuthorizationCode, Parent.AuthorizationOwner);
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.Country, "Property", Parent.CountryCode, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType, "Property", Parent.AuthorizationCode, false));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber, "Property", Parent.AuthorizationNumber));
				numbers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, "Property", Parent.AuthorizationOwner, false));
				return numbers;
			}
		}

		public virtual ICollection AuthorizationRuleList => new CodeDescriptionPairList();

		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();
	}
}
