using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CusGoodsLocationAddress : EU.NCTS.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public override ZString AuthorisationNumber
	{
		get => base.AuthorisationNumber;
		set
		{
			var oldValue = AuthorisationNumber;
			base.AuthorisationNumber = value;
			if (!IsCopying && oldValue != AuthorisationNumber)
			{
				UpdateGoodsLocationPlaceCodeValue();
			}
		}
	}

	public override ZGuid IdentificationHolderPK
	{
		get => base.IdentificationHolderPK;
		set
		{
			var oldValue = IdentificationHolderPK;
			base.IdentificationHolderPK = value;
			if (!IsCopying && oldValue != IdentificationHolderPK)
			{
				CusGoodsLocationAddressHelper.SetDefaultAuthorisationNumber(this);
			}
		}
	}

	public override ZBool E2_AddressOverride
	{
		get => base.E2_AddressOverride;
		set
		{
			var oldValue = E2_AddressOverride;
			base.E2_AddressOverride = value;
			if (value && !oldValue && !IsCopying)
			{
				CusGoodsLocationAddressHelper.SetValidationStatusToManIfAddressIsOverride(this);
			}
		}
	}

	protected override JobDocAddressLookups GetNewLookups() => new IT.Business.Declaration.CusGoodsLocationAddressLookups(this);

	protected override JobDocAddressValidation GetNewValidation()
	{
		return new CusGoodsLocationAddressValidation(this);
	}

	[MaxLength(Schema.E2_CityMaxLength)]
	public override ZString E2_City => base.E2_City.Left(Schema.E2_CityMaxLength);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	public new class Schema : EU.Business.CusGoodsLocationAddress.Schema
	{
		public new const int E2_CityMaxLength = 35;
	}

	#region Implementation

	void UpdateGoodsLocationPlaceCodeValue() => GoodsLocation?.UpdatePlaceCodeValue();

	#endregion
}
