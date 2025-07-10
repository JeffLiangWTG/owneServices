using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
		IgnoreValidationStatusError = true;
	}

	[MaxLength(Schema.E2_CityMaxLength)]
	public override ZString E2_City => base.E2_City.Left(Schema.E2_CityMaxLength);

	public override ZString E2_Address1AndE2_Address2 => base.E2_Address1AndE2_Address2.Left(E2_Address1AndE2_Address2Info.MaxLength);

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

	public override ZString AuthorisationNumber
	{
		get => base.AuthorisationNumber;
		set
		{
			var oldValue = AuthorisationNumber;
			base.AuthorisationNumber = value;
			if (!IsCopying && oldValue != AuthorisationNumber)
			{
				DefaultAdditionalIdentifier();
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

	protected override JobDocAddressLookups GetNewLookups() => new CusGoodsLocationAddressLookups(this);

	protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	public new class Schema : EU.Business.CusGoodsLocationAddress.Schema
	{
		public new const int E2_CityMaxLength = 35;
	}

	public new CusGoodsLocationAddressLookups Lookups => (CusGoodsLocationAddressLookups)base.Lookups;

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public new EU.Business.CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

	#region Implementation

	void DefaultAdditionalIdentifier()
	{
		var additionalIdentifierList = GoodsLocation?.Lookups.AdditionalIdentifierList ?? new CodeDescriptionPairList();
		if (additionalIdentifierList.Count == 1)
		{
			GoodsLocation.CGL_AdditionalIdentifier = additionalIdentifierList[0].Code;
		}
	}

	#endregion
}
