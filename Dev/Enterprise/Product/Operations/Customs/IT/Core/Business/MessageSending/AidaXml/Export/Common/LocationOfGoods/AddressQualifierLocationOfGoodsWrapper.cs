using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class AddressQualifierLocationOfGoodsWrapper : ILocationOfGoods
{
	public AddressQualifierLocationOfGoodsWrapper(CusGoodsLocation goodsLocation)
	{
		Argument.NotNull(goodsLocation, nameof(goodsLocation));
		var goodsLocationAddress = goodsLocation.Address;

		lazyTypeOfLocation = new Lazy<string>(() => goodsLocation.CGL_Type);
		lazyQualifier = new Lazy<string>(() => goodsLocation.CGL_Qualifier);
		lazyAddress = new Lazy<IAddress>(() => new AddressWrapper(goodsLocationAddress));
		lazyContact = new Lazy<IContact>(() => ContactWrapper.NewOrNull(goodsLocationAddress));
	}

	string ILocationOfGoods.TypeOfLocation => lazyTypeOfLocation.Value;
	readonly Lazy<string> lazyTypeOfLocation;

	string ILocationOfGoods.Qualifier => lazyQualifier.Value;
	readonly Lazy<string> lazyQualifier;

	string ILocationOfGoods.AuthorisationNumber => null;

	string ILocationOfGoods.CustomsOffice => null;

	IAddress ILocationOfGoods.Address => lazyAddress.Value;
	readonly Lazy<IAddress> lazyAddress;

	IContact ILocationOfGoods.Contact => lazyContact.Value;
	readonly Lazy<IContact> lazyContact;
}
