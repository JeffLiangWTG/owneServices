using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using IAddress = CargoWise.Customs.IT.MessageContracts.IAddress;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class H7LocationOfGoodsWrapper : IH7LocationOfGoods
{
	public H7LocationOfGoodsWrapper(CusGoodsLocation goodsLocation)
	{
		this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
	}

	readonly CusGoodsLocation goodsLocation;

	public string Type => goodsLocation.CGL_Type;

	public string Qualifier => goodsLocation.CGL_Qualifier;

	public string Unlocode => null;

	public string AuthorizationNumber
	{
		get
		{
			if (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber)
			{
				return goodsLocation.CGL_Authorization;
			}

			return null;
		}
	}

	public string LocationIdentifier => goodsLocation.CGL_AdditionalIdentifier;

	public string CustomsOfficeCode
	{
		get
		{
			if (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier)
			{
				return goodsLocation.CGL_CustomsOffice;
			}

			return null;
		}
	} 

	public string Latitude => null;

	public string Longitude => null;

	public string RegistrationNumberOfParty
	{
		get
		{
			if (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber)
			{
				return goodsLocation.Address.E2_GovRegNum;
			}

			return null;
		}
	}

	public IAddress Address
	{
		get
		{
			if (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address)
			{
				return CachedValueHelper.GetValue(ref address, () =>
					new AddressWrapper(
						name: goodsLocation.Address.E2_CompanyName,
						streetAndNumber: goodsLocation.Address.E2_Address1 + " " + goodsLocation.Address.E2_Address2,
						country: goodsLocation.Address.E2_RN_NKCountryCode,
						zipCode: goodsLocation.Address.Postcode,
						city: goodsLocation.Address.City
					)
				);
			}
			return null;
		}
	}
	CachedValue<IAddress> address;

	public IAddress MailingAddress => null;
}
