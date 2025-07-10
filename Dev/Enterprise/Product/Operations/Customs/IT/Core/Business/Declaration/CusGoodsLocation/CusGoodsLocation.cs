using System;
using System.Collections.Immutable;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.IT.ICusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	public bool IsPlaceCodeAvailable => CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && CGL_Type.In(placeCodeTypes);

	[ResourceStringData("F282C9FD-F053-48A3-9E41-8187A63A0AAC", Caption = "Place Code")]
	[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.AdditionalIdentifierList))]
	public override ZString CGL_AdditionalIdentifier { get => base.CGL_AdditionalIdentifier; set => base.CGL_AdditionalIdentifier = value; }

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

	protected override void ClearAddressFields(EU.Business.CusGoodsLocationAddress address)
	{
		address.E2_AddressOverride = GoodsLocationAddressOverride;
		base.ClearAddressFields(address);
	}

	protected override ZBool GoodsLocationAddressOverride => CGL_Qualifier != CusGoodsLocationQualifierList.Codes.Address && base.GoodsLocationAddressOverride;

	#region Implementation

	readonly ImmutableArray<ZString> placeCodeTypes = new ZString[]
	{
		CusGoodsLocationTypeList.Codes.AuthorizedPlace,
		CusGoodsLocationTypeList.Codes.ApprovedPlace,
	}.ToImmutableArray();

	#endregion
}
