using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public OrganisationsFindBoxCollection DeclarantList => new OrganisationsFindBoxCollection(Factory);

		public OrganisationsFindBoxCollection RepresentativeList => new BrokerCollection(Factory);

		public OrganisationsFindBoxCollection PresenterList => PresenterListCore;

		protected virtual OrganisationsFindBoxCollection PresenterListCore => new OrganisationsFindBoxCollection(Factory);

		public override CodeDescriptionPairList AgentTypeList => Factory.GetCachedValue<EUH7AgentTypes>();

		protected override ICollection CustomsOfficesCore => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
			Parent.AMA_RN_NKCountry,
			Constants.RefCusCodeListTypes.Codes.CustomsOffice,
			ZDateTime.UtcToday);
	}
}
