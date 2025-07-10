using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class LocationOfGoodsProvider : ILocationOfGoods
{
	public LocationOfGoodsProvider(CusGoodsLocation goodsLocation, bool isTransitionPeriodAES30 = false)
	{
		this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
		isTransitionPeriod = isTransitionPeriodAES30;
	}
	readonly CusGoodsLocation goodsLocation;
	readonly bool isTransitionPeriod;

	public string TypeOfLocation => goodsLocation.CGL_Type;

	public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

	public string AuthorisationNumber => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber ? goodsLocation.CGL_AdditionalIdentifier : null;

	public string AdditionalIdentifier => goodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber
		&& goodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.UnLocode
		&& goodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
		&& goodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.PostcodeAddress
		&& goodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.GnssCoordinates
		? goodsLocation.CGL_AdditionalIdentifier : null;

	public string UNLocode => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode ? goodsLocation.Unlocode : null;

	public string CustomsOfficeReferenceNumber => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? goodsLocation.CGL_CustomsOffice : null;

	public string GNSSLatitute => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? goodsLocation.Address.E2_Latitude.ToString() : null;

	public string GNSSLongitude => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? goodsLocation.Address.E2_Longitude.ToString() : null;

	public string EconomicOperatorIdentificationNumber => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber ? goodsLocation.Address.E2_GovRegNum : null;

	public CargoWise.Customs.BE.MessageContracts.Interfaces.IAddress Address => CachedValueHelper.GetValue(ref address, () => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address ? new AddressProvider(goodsLocation.Address.Address, isTransitionPeriodAES30: isTransitionPeriod) : null);
	CachedValue<CargoWise.Customs.BE.MessageContracts.Interfaces.IAddress> address;

	public IPostCodeAddress PostCodeAddress => CachedValueHelper.GetValue(ref postCodeAddress, () => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress ? new PostCodeAddressProvider(goodsLocation) : null);
	CachedValue<IPostCodeAddress> postCodeAddress;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address ? ContactPersonProvider.NewOrNull(goodsLocation.Address.E2_Contact, goodsLocation.Address.E2_Phone, goodsLocation.Address.E2_Email) : null);
	CachedValue<IContactPerson> contactPerson;
}
