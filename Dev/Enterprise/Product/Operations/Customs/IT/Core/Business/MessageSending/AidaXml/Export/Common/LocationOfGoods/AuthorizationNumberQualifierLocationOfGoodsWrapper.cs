using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration.Export;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class AuthorizationNumberQualifierLocationOfGoodsWrapper : ILocationOfGoods
{
	public AuthorizationNumberQualifierLocationOfGoodsWrapper(CusGoodsLocation goodsLocation)
	{
		Argument.NotNull(goodsLocation, nameof(goodsLocation));

		lazyTypeOfLocation = new Lazy<string>(() => goodsLocation.CGL_Type);
		lazyQualifier = new Lazy<string>(() => goodsLocation.CGL_Qualifier);
		lazyAuthorisationNumber = new Lazy<string>(() => GetAuthorisationNumber(goodsLocation));
		lazyContact = new Lazy<IContact>(() => ContactWrapper.NewOrNull(goodsLocation.Address));
	}

	string ILocationOfGoods.TypeOfLocation => TypeOfLocation;
	readonly Lazy<string> lazyTypeOfLocation;

	string TypeOfLocation => lazyTypeOfLocation.Value;

	string ILocationOfGoods.Qualifier => lazyQualifier.Value;
	readonly Lazy<string> lazyQualifier;

	string ILocationOfGoods.AuthorisationNumber => lazyAuthorisationNumber.Value;
	readonly Lazy<string> lazyAuthorisationNumber;

	string ILocationOfGoods.CustomsOffice => null;

	IAddress ILocationOfGoods.Address => null;

	IContact ILocationOfGoods.Contact => lazyContact.Value;
	readonly Lazy<IContact> lazyContact;

	string GetAuthorisationNumber(CusGoodsLocation goodsLocation)
	{
		var authorisationNumber = goodsLocation.Address.AuthorisationNumber;
		switch (TypeOfLocation)
		{
			case Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace:
			case Customs.Business.CusGoodsLocationTypeList.Codes.ApprovedPlace:
				return FormattableString.Invariant($"{authorisationNumber}.{goodsLocation.CGL_AdditionalIdentifier}");

			default:
				return authorisationNumber;
		}
	}
}
