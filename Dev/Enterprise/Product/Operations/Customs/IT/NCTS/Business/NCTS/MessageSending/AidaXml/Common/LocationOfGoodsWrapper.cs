using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Argument = CargoWise.Common.Argument;
using IAddress = CargoWise.Customs.IT.MessageContracts.IAddress;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class LocationOfGoodsWrapper : LocationOfGoodsTypeDataProviderAbstractClass, ILocationOfGoods
{
	public LocationOfGoodsWrapper(CusGoodsLocation goodsLocation)
	{
		Argument.NotNull(goodsLocation, nameof(goodsLocation));
		var address = goodsLocation.Address;
		lazyTypeOfLocation = new Lazy<string>(() => goodsLocation.CGL_Type);
		lazyQualifier = new Lazy<string>(() => goodsLocation.CGL_Qualifier);
		lazyAuthorisationNumber = new Lazy<string>(() => GetAuthorisationNumber(goodsLocation)?.Trim());
		lazyCustomsOffice = new Lazy<string>(() => GetCustomsOffice(goodsLocation));
		lazyAddress = new Lazy<IAddress>(() => GetAddress(address));
		lazyContact = new Lazy<IContact>(() => GetContact(address));
		lazyAddressType = new Lazy<AddressTypeDataProviderAbstractClass>(() => GetAddressType(address));
		lazyContactPersonType = new Lazy<ContactPersonTypeDataProviderAbstractClass>(() => GetContactPersonType(address));
	}

	#region ILocationOfGoods Member

	string ILocationOfGoods.TypeOfLocation => lazyTypeOfLocation.Value;

	readonly Lazy<string> lazyTypeOfLocation;

	string ILocationOfGoods.Qualifier => lazyQualifier.Value;
	readonly Lazy<string> lazyQualifier;

	string ILocationOfGoods.AuthorisationNumber => lazyAuthorisationNumber.Value?.Trim();
	readonly Lazy<string> lazyAuthorisationNumber;

	string ILocationOfGoods.AdditionalIdentifier => null;

	string ILocationOfGoods.CustomsOffice => lazyCustomsOffice.Value;
	readonly Lazy<string> lazyCustomsOffice;

	IAddress ILocationOfGoods.Address => lazyAddress.Value;
	readonly Lazy<IAddress> lazyAddress;

	IContact ILocationOfGoods.Contact => lazyContact.Value;
	readonly Lazy<IContact> lazyContact;

	#endregion

	#region Implemtation

	IContact GetContact(CusGoodsLocationAddress address)
	{
		var qualifier = Qualifier;
		return (qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber || qualifier == CusGoodsLocationQualifierList.Codes.Address)
		? ContactWrapper.NewOrNull(address)
		: null;
	}

	IAddress GetAddress(CusGoodsLocationAddress address)
	{
		return Qualifier == CusGoodsLocationQualifierList.Codes.Address
			? new AddressWrapper(address)
			: null;
	}

	string GetCustomsOffice(CusGoodsLocation goodsLocation)
	{
		return Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
			? goodsLocation.CGL_CustomsOffice
			: null;
	}

	string GetAuthorisationNumber(CusGoodsLocation goodsLocation)
	{
		if (Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber)
		{
			return null;
		}
		var typeOfLocation = lazyTypeOfLocation.Value;
		var authorisationNumber = goodsLocation.Address.AuthorisationNumber;
		var additionalIdentifier = goodsLocation.CGL_AdditionalIdentifier;

		if (typeOfLocation.In(CusGoodsLocationTypeList.Codes.AuthorizedPlace, CusGoodsLocationTypeList.Codes.ApprovedPlace))
		{
			return new ZStringBuilder()
				.AppendIfNotEmpty(authorisationNumber)
				.AppendIfNotEmpty(additionalIdentifier)
				.ToStringWithDelimiterBetweenAppends(".");
		}

		return authorisationNumber;
	}

	string Qualifier => lazyQualifier.Value;

	AddressTypeDataProviderAbstractClass GetAddressType(CusGoodsLocationAddress address)
	{
		var addressContract = GetAddress(address);
		return addressContract != null ? new AddressTypeWrapper(addressContract) : null;
	}

	ContactPersonTypeDataProviderAbstractClass GetContactPersonType(CusGoodsLocationAddress address)
	{
		var contact = GetContact(address);
		return contact != null ? new ContactPersonWrapper(contact) : null;
	}

	#endregion

	#region ILocationOfGoodsType

	public override string TypeOfLocation => lazyTypeOfLocation.Value;

	public override string QualifierOfIdentification => lazyQualifier.Value;

	public override string UNLOCODE => null;

	public override string AuthorisationNumber => lazyAuthorisationNumber.Value;

	public override string AdditionalIdentifier => null;

	public override string CustomsOfficeReferenceNumber => lazyCustomsOffice.Value;

	public override GNSSTypeDataProviderAbstractClass GNSS => null;

	public override string EconomicOperatorIdentificationNumber => null;

	public override AddressTypeDataProviderAbstractClass Address => lazyAddressType.Value;

	readonly Lazy<AddressTypeDataProviderAbstractClass> lazyAddressType;

	public override PostcodeAddressTypeDataProviderAbstractClass PostcodeAddress => null;

	public override ContactPersonTypeDataProviderAbstractClass ContactPerson => lazyContactPersonType.Value;

	readonly Lazy<ContactPersonTypeDataProviderAbstractClass> lazyContactPersonType;

	#endregion
}
