using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegPremisesLookups : AutoCusTempStorageRegPremisesLookups
{
	public CusTempStorageRegPremisesLookups(AutoCusTempStorageRegPremises parent) : base(parent)
	{
	}

	protected new CusTempStorageRegPremises Parent => (CusTempStorageRegPremises)base.Parent;

	public virtual CodeDescriptionPairList TypeList => Factory.GetCachedValue<CusTempStorageRegPremisesTypeList>();

	public virtual OrganisationsFindBoxCollection PremisesAddressList => new(Factory);

	public virtual OrganisationsFindBoxCollection OwnerList => new(Factory);

	public virtual ICollection CustomsLocationList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
		GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
		new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType },
		ZDateTime.Today,
		System.Array.Empty<RefCusCodeListAttributeFilter>(),
		includeParentDataGroupings: false);
}
