using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using GoodsLocationAddressSchema = CargoWise.Customs.IT.MessageContracts.Declaration.Import.ImportMessageSchema.Address;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobDeclarationGoodsLocationAddressValidation
{
	public JobDeclarationGoodsLocationAddressValidation(JobDeclaration jobDeclaration)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
	}

	readonly JobDeclaration declaration;

	internal void ValidateOrganisation(ZPropertyInfo targetInfo)
	{
		var address = declaration.GoodsLocationAddress;
		if (!IsGoodsAreOutOfCustomsCircuit || !address.IsEmpty)
		{
			return;
		}
		CheckGoodsLocationAddressWhenEmpty(targetInfo);
	}

	internal void ValidateAddress(ZPropertyInfo targetInfo)
	{
		var address = declaration.GoodsLocationAddress;
		if (!IsGoodsAreOutOfCustomsCircuit || address.IsEmpty)
		{
			return;
		}
		CheckGoodsLocationAddressWhenFilled(targetInfo, address);
	}

	#region Implementation

	void CheckGoodsLocationAddressWhenEmpty(ZPropertyInfo targetInfo)
	{
		if (declaration.ZG_AuthorisationNumber.IsEmpty)
		{
			targetInfo.AddMessageError(ValidationCaptions.JobDeclaration.YouHaveNotEnteredAValueForGoodsLocationAddress);
		}
	}

	void CheckGoodsLocationAddressWhenFilled(ZPropertyInfo targetInfo, JobDocAddress address)
	{
		ValidateAddressLength(targetInfo, address);
		ValidatePostcodeLength(targetInfo, address);
		ValidateCityLength(targetInfo, address);
	}

	void ValidateAddressLength(ZPropertyInfo targetInfo, JobDocAddress address)
	{
		if (address.E2_Address1AndE2_Address2.Length > GoodsLocationAddressSchema.StreetAndNumberMaxLength)
		{
			targetInfo.AddWarning(ValidationCaptions.JobDeclaration.GoodsLocationAddressLongerThan70);
		}
	}

	void ValidatePostcodeLength(ZPropertyInfo targetInfo, JobDocAddress address)
	{
		if (address.E2_Postcode.Length > GoodsLocationAddressSchema.ZipCodeMaxLength)
		{
			targetInfo.AddWarning(ValidationCaptions.JobDeclaration.GoodsLocationAddressPostcodeLongerThan9);
		}
	}

	void ValidateCityLength(ZPropertyInfo targetInfo, JobDocAddress address)
	{
		if (address.E2_City.Length > GoodsLocationAddressSchema.CityMaxLength)
		{
			targetInfo.AddWarning(ValidationCaptions.JobDeclaration.GoodsLocationAddressCityLongerThan35);
		}
	}

	#endregion

	bool IsGoodsAreOutOfCustomsCircuit => declaration.JE_LocationQualifier == GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit;
}
