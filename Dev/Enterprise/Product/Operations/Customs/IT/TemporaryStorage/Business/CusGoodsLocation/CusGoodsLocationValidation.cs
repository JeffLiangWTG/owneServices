using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
	{
	}

	protected override void CheckCGL_AdditionalIdentifier()
	{
		base.CheckCGL_AdditionalIdentifier();

		var parent = Parent;

		var placeId = parent.AdditionalIdentifier;
		var placeIdInfo = parent.AdditionalIdentifierInfo;
		var organization = parent.Address.IdentificationHolderPK;

		MandatoryValidation.MessageErrorIfNotEntered(placeIdInfo);

		if (IsInvalidPlaceId(placeId))
		{
			placeIdInfo.AddMessageError(ValidationCaptions.TemporaryStorageHeader.PlaceIDMustBeNumeric);
		}
		if (!organization.IsEmpty)
		{
			ListValidation.MessageErrorIfInvalidCode(placeIdInfo);
		}
	}

	bool IsInvalidPlaceId(ZString placeId)
	{
		return !placeId.IsEmpty && !placeId.IsNumbersOnlyOrEmpty;
	}

	protected override bool IsQualifierInvalidForType(string type, string qualifier) => false;
}
