using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusGoodsLocationAddressLookups : EU.Business.CusGoodsLocationAddressLookups
{
	public CusGoodsLocationAddressLookups(EU.Business.CusGoodsLocationAddress parent) : base(parent)
	{
	}

	public override ICollection AuthorisationNumberList => GetCachedAuthorisationNumberList();

	#region Implementation

	ICollection GetCachedAuthorisationNumberList()
	{
		var identificationHolderPK = Parent.IdentificationHolderPK;
		var cacheKey = FormattableString.Invariant($"IT|AuthorisationNumberList|{identificationHolderPK}");
		return Factory.GetCachedValue(cacheKey, () => GetAuthorisationNumberList(identificationHolderPK));
	}

	ICollection GetAuthorisationNumberList(ZGuid identificationHolderPK)
	{
		var authorisationHeaderCollectionFiltered = new ApprovedLocationAuthorisationHeaderCollection(Factory, identificationHolderPK);

		var filterBusinessObjectDefaults = authorisationHeaderCollectionFiltered.FilterBusinessObjectDefaults;
		filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder, PropertyName, new ZGuid(identificationHolderPK), false));
		filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber, PropertyName, Parent.E2_GovRegNum));

		return authorisationHeaderCollectionFiltered;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "constant property")]
	const string PropertyName = "Property";

	#endregion
}
