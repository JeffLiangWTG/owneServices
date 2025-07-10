using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CusGoodsLocation : EU.NCTS.Business.CusGoodsLocation
	, Integration.Customs.IT.INctsCusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocationLookups Lookups => new CusGoodsLocationLookups(this);

	public new CusGoodsLocationAddress Address => (CusGoodsLocationAddress)base.Address;

	public override ZGuid CGL_ParentID
	{
		get => base.CGL_ParentID;
		set
		{
			var oldValue = CGL_ParentID;
			base.CGL_ParentID = value;

			if (!IsCopying && oldValue != value)
			{
				Address.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_ParentTableCode
	{
		get => base.CGL_ParentTableCode;
		set
		{
			var oldValue = CGL_ParentTableCode;
			base.CGL_ParentTableCode = value;
			if (!IsCopying && oldValue != value)
			{
				Address.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CGL_Qualifier
	{
		get => base.CGL_Qualifier;
		set
		{
			var oldValue = CGL_Qualifier;
			base.CGL_Qualifier = value;
			if (!IsCopying && oldValue != value)
			{
				Address.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("080fd9c9-983f-45d2-bbaa-7c147491a02f", Caption = "Place Code")]
	[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.AdditionalIdentifierList))]
	public override ZString CGL_AdditionalIdentifier { get => base.CGL_AdditionalIdentifier; set => base.CGL_AdditionalIdentifier = value; }

	protected override ZString AdditionalIdentifierDescriptionCore => Lookups.AdditionalIdentifierList.GetDescriptionFromCode(CGL_AdditionalIdentifier) ?? ZString.Empty;

	protected override void ClearAddressFields(EU.Business.CusGoodsLocationAddress address)
	{
		address.E2_AddressOverride = GoodsLocationAddressOverride;
		base.ClearAddressFields(address);
		address.OrganisationPK = ZGuid.Empty;
	}

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation()
		=> new CusGoodsLocationValidation(this);

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	protected override void AddressIdentificationHolderPKChanged(object sender, EventArgs e)
	{
		base.AddressIdentificationHolderPKChanged(sender, e);
		UpdatePlaceCodeValue();
	}

	protected override ZBool GoodsLocationAddressOverride
		=> CGL_Qualifier != CusGoodsLocationQualifierList.Codes.Address && base.GoodsLocationAddressOverride;

	internal void UpdatePlaceCodeValue()
	{
		if (CGL_Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber
			&& CGL_Qualifier != CusGoodsLocationQualifierList.Codes.Address)
		{
			CGL_AdditionalIdentifier = ZString.Empty;
			return;
		}

		var placeCodes = Lookups.AdditionalIdentifierList;
		CGL_AdditionalIdentifier = placeCodes.Count == 1
			? (ZString)placeCodes[0].Code
			: ZString.Empty;
	}
}
