using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusGoodsLocation : EU.Business.CusTempStorage.CusGoodsLocation, Integration.Customs.IT.ITemporaryStorageCusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	[ResourceStringData("EF591827-1517-4410-A5E9-5CC8B1E49865", Caption = "Place ID")]
	public override ZString CGL_AdditionalIdentifier { get => base.CGL_AdditionalIdentifier; set => base.CGL_AdditionalIdentifier = value; }

	[ResourceStringData("EF591827-1517-4410-A5E9-5CC8B1E49865", Caption = "Place ID")]
	[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.AdditionalIdentifierList))]
	public override ZString AdditionalIdentifier { get => base.AdditionalIdentifier; set => base.AdditionalIdentifier = value; }

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

	protected override int AdditionalIdentifierMaxLength => 6;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	protected override void SetDefaultsForNew()
	{
		base.SetDefaultsForNew();
		CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		CGL_Type = CusGoodsLocationTypeList.Codes.ApprovedPlace;
	}
}
