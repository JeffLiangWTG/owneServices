using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class CustomsOfficeQualifierLocationOfGoodsWrapper : ILocationOfGoods
{
	public CustomsOfficeQualifierLocationOfGoodsWrapper(CusGoodsLocation goodsLocation)
	{
		Argument.NotNull(goodsLocation, nameof(goodsLocation));

		lazyTypeOfLocation = new Lazy<string>(() => goodsLocation.CGL_Type);
		lazyQualifier = new Lazy<string>(() => goodsLocation.CGL_Qualifier);
		lazyCustomsOffice = new Lazy<string>(() => goodsLocation.CGL_CustomsOffice);
	}

	string ILocationOfGoods.TypeOfLocation => lazyTypeOfLocation.Value;
	readonly Lazy<string> lazyTypeOfLocation;

	string ILocationOfGoods.Qualifier => lazyQualifier.Value;
	readonly Lazy<string> lazyQualifier;

	string ILocationOfGoods.AuthorisationNumber => null;

	string ILocationOfGoods.CustomsOffice => lazyCustomsOffice.Value;
	readonly Lazy<string> lazyCustomsOffice;

	IAddress ILocationOfGoods.Address => null;

	IContact ILocationOfGoods.Contact => null;
}
