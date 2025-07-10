using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS;

public class CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : EU.NCTS.Business.CusGoodsLocation(factory, row)
{
	public bool IsAuthorizedPlaceWithAuthorization => Parent is NctsCommonMovementHeader { IsPhase4: false } && CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace;

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	public override ZString CGL_Qualifier
	{
		get => base.CGL_Qualifier;
		set
		{
			var oldValue = CGL_Qualifier;
			base.CGL_Qualifier = value;
			if (!IsCopying && oldValue != CGL_Qualifier)
			{
				UpdateAdressAuthorizationNumber();
			}
		}
	}

	public override ZString CGL_Type
	{
		get => base.CGL_Type;
		set
		{
			var oldValue = CGL_Type;
			base.CGL_Type = value;
			if (!IsCopying && oldValue != CGL_Type)
			{
				UpdateAdressAuthorizationNumber();
			}
		}
	}

	protected void UpdateAdressAuthorizationNumber()
	{
		if (!( Header?.IsPhase4 ?? false ) && (Header?.IsArrivalMovement ?? false) && CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace)
		{
			var authorisationNumberList = (CodeDescriptionPairList)Address.Lookups.AuthorisationNumberList;
			if (authorisationNumberList.Count == 1)
			{
				Address.AuthorisationNumber = authorisationNumberList.GetAllCodes().Single();
			}
			else
			{
				Address.AuthorisationNumber = ZString.Empty;
			}
		}
	}
}
