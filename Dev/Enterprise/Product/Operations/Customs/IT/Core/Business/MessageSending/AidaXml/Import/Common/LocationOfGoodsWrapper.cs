using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class LocationOfGoodsWrapper : ILocationOfGoods
{
	LocationOfGoodsWrapper(JobDeclaration declaration)
	{
		this.declaration = declaration;

		lazyAdditionalCode = new Lazy<string>(GetAdditionalCode);
		lazyAddress = new Lazy<string>(GetAddress);
		lazyCity = new Lazy<string>(GetCity);
		lazyCode = new Lazy<string>(GetCode);
		lazyCountryCode = new Lazy<string>(GetCountryCode);
		lazyQualifier = new Lazy<string>(GetQualifier);
		lazyRole = new Lazy<string>(GetRole);
		lazyZipCode = new Lazy<string>(GetZipCode);
	}

	readonly JobDeclaration declaration;

	public static LocationOfGoodsWrapper NewOrNull(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, nameof(declaration));

		if (declaration.IsImport && !declaration.JE_LocationQualifier.IsEmpty)
		{
			return new LocationOfGoodsWrapper(declaration);
		}
		return null;
	}

	#region ILocationOfGoods

	string ILocationOfGoods.AdditionalCode => lazyAdditionalCode.Value;
	readonly Lazy<string> lazyAdditionalCode;

	string ILocationOfGoods.Address => lazyAddress.Value;
	readonly Lazy<string> lazyAddress;

	string ILocationOfGoods.City => lazyCity.Value;
	readonly Lazy<string> lazyCity;

	string ILocationOfGoods.Code => lazyCode.Value;
	readonly Lazy<string> lazyCode;

	string ILocationOfGoods.CountryCode => lazyCountryCode.Value;
	readonly Lazy<string> lazyCountryCode;

	string ILocationOfGoods.Qualifier => lazyQualifier.Value;
	readonly Lazy<string> lazyQualifier;

	string ILocationOfGoods.Role => lazyRole.Value;
	readonly Lazy<string> lazyRole;

	string ILocationOfGoods.ZipCode => lazyZipCode.Value;
	readonly Lazy<string> lazyZipCode;

	#endregion

	#region Implementation

	string GetAdditionalCode()
	{
		switch (LocationQualifier)
		{
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				return declaration.ImportJE_LocationOtherInformation;

			default:
				return ZString.Empty;
		}
	}

	string GetAddress()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit:
				return SelectedAddress?.Address1 + SelectedAddress?.Address2;

			default:
				return ZString.Empty;
		}
	}

	string GetCity()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit:
				return SelectedAddress?.City ?? ZString.Empty;

			default:
				return ZString.Empty;
		}
	}

	string GetCode()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection:
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation:
				return declaration.JE_LocationOfGoods;

			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				var sb = new ZStringBuilder();
				sb.AppendIfNotEmpty(declaration.ZG_AuthorisationNumber);
				sb.AppendIfNotEmpty(declaration.JE_LocationOfGoods);
				return sb.ToStringWithDelimiterBetweenAppends(".");

			default:
				return ZString.Empty;
		}
	}

	string GetCountryCode()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection:
				return declaration.JE_LocationOfGoods.Left(2);

			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit:
				return Core.Constants.CountryCodes.Italy;

			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				return declaration.ImportJE_SubLocationOfGoods;

			default:
				return ZString.Empty;
		}
	}

	string GetQualifier()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection:
				return Ucc6XmlConstants.LocationOfGoods.Qualifier.CustomsOffice;

			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit:
				return Ucc6XmlConstants.LocationOfGoods.Qualifier.Address;

			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				return Ucc6XmlConstants.LocationOfGoods.Qualifier.AuthorizationNumber;

			default:
				return ZString.Empty;
		}
	}

	string GetRole()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection:
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit:
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuitAtAnAuthorizedLocation:
				return Ucc6XmlConstants.LocationOfGoods.Role.OtherPlace;

			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace:
				return Ucc6XmlConstants.LocationOfGoods.Role.AuthorizedPlace;

			case ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace:
				return Ucc6XmlConstants.LocationOfGoods.Role.ApprovedPlace;

			default:
				return ZString.Empty;
		}
	}

	string GetZipCode()
	{
		switch (LocationQualifier)
		{
			case GoodsLocationQualifierList.Codes.GoodsAreOutOfCustomsCircuit:
				return SelectedAddress?.Postcode ?? ZString.Empty;

			default:
				return ZString.Empty;
		}
	}

	ISupportWebAddressValidation SelectedAddress => declaration.GoodsLocationAddress?.Address?.SelectedTranslatedAddress;
	ZString LocationQualifier => declaration.JE_LocationQualifier;

	#endregion
}
