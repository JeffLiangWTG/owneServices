using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonLocationOfGoodsWrapper : ICommonLocationOfGoods
{
	public CommonLocationOfGoodsWrapper(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		goodsLocation = this.entryInstruction.GoodsLocation;
	}
	protected readonly CusEntryInstruction entryInstruction;
	protected readonly EU.Business.CusGoodsLocation goodsLocation;

	public ZString LocationType => goodsLocation.CGL_Type;

	public ZString LocationQualifier => goodsLocation.CGL_Qualifier;

	public ZString LocationId => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
									? IsTypeBAndAutorizationNumberLonger10AndStartsWithES() ? goodsLocation.Address.AuthorisationNumber.SubstringSafe(4) : goodsLocation.Address.AuthorisationNumber
									: null;

	ZBool IsTypeBAndAutorizationNumberLonger10AndStartsWithES() => goodsLocation.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace && goodsLocation.Address.AuthorisationNumber.Length > 10 && goodsLocation.Address.AuthorisationNumber.StartsWith("ES");

	public ZString LocationAdditionalId => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber
											|| goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
										? goodsLocation.CGL_AdditionalIdentifier
										: ZString.Empty;

	public ZString LocationUNloCode => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode ? goodsLocation.Unlocode : ZString.Empty;

	public ZString LocationCustomOffice => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? goodsLocation.CGL_CustomsOffice : ZString.Empty;

	public ICommonGNSS LocationGNSS => locationGNSS ?? (locationGNSS = goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates
																				? new CommonGNSSWrapper(goodsLocation.Address.E2_Latitude, goodsLocation.Address.E2_Longitude)
																				: null);
	CommonGNSSWrapper locationGNSS;

	public ZString LocationEconomicOperatorId => goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber ? goodsLocation.Address.E2_GovRegNum : ZString.Empty;

	public IPartyAddressProvider LocationAddress => locationAddress ?? (locationAddress = goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address
																							? AESCommonAddressWrapper.New(goodsLocation.Address.E2_Address1,
																															goodsLocation.Address.E2_City,
																															goodsLocation.Address.E2_Postcode,
																															goodsLocation.Address.E2_RN_NKCountryCode)
																							: null);
	AESCommonAddressWrapper locationAddress;

	public ICommonPostcodeAddress LocationPostcodeAddress => locationPostcodeAddress ?? (locationPostcodeAddress = goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress
																										? new CommonPostcodeAddressWrapper(goodsLocation.CGL_AdditionalIdentifier,
																																				goodsLocation.Address.E2_Postcode,
																																				goodsLocation.Address.E2_RN_NKCountryCode)
																										: null);
	CommonPostcodeAddressWrapper locationPostcodeAddress;
}
