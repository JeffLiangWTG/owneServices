using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.BE.Business;

public class PNTSLocationOfGoodsProvider : IPNTSLocationOfGoods
{
	public PNTSLocationOfGoodsProvider(CusGoodsLocation locationOfGoods)
	{
		this.locationOfGoods = Argument.NotNull(locationOfGoods, nameof(locationOfGoods));
	}

	readonly CusGoodsLocation locationOfGoods;

	public string TypeOfLocation => locationOfGoods.CGL_Type;

	public string QualifierOfIdentification => locationOfGoods.CGL_Qualifier;

	public string UnLoCode => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? locationOfGoods.CGL_AdditionalIdentifier : ZString.Empty;

	public string AuthorisationNumber => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.AuthorizationNumber ? locationOfGoods.Address?.E2_GovRegNum ?? ZString.Empty : ZString.Empty;

	public string AdditionalIdentifier => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.AuthorizationNumber ? locationOfGoods.CGL_AdditionalIdentifier : ZString.Empty;

	public string CustomsOfficeReferenceNumber => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? locationOfGoods.CGL_CustomsOffice : ZString.Empty;

	public string GNSSLongitude => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? locationOfGoods.Address?.E2_Longitude.ToString() ?? ZString.Empty : ZString.Empty;

	public string GNSSLatitude => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? locationOfGoods.Address?.E2_Latitude.ToString() ?? ZString.Empty : ZString.Empty;

	public string EconomicOperatorIdentificationNumber => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.EoriNumber ? GetEconomicOperatorIdentificationNumber() : ZString.Empty;

	string GetEconomicOperatorIdentificationNumber()
	{
		var orgAddress = locationOfGoods.Address;
		if (!orgAddress.IsNull)
		{
			var regNum = orgAddress.E2_GovRegNum;
			if (regNum.IsEmpty)
			{
				regNum = orgAddress.IdentificationHolder.GetConcatenatedSingleOrgCusCode(EuropeanUnionSharedCodeTypes.Eori);
			}
			else
			{
				var country = orgAddress.CountryCodeList.FirstOrDefault(x => x.Code == regNum.SubstringSafe(0, 2))?.Code ?? ZString.Empty;
				if (country.IsEmpty)
				{
					country = orgAddress.IdentificationHolder.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CustomsRegNo == regNum)?.OK_RN_NKCodeCountry ?? ZString.Empty;
					if (country.IsEmpty)
					{
						country = orgAddress.IdentificationHolder.CountryCode;
					}
					regNum = country + regNum;
				}
			}
			return regNum;
		}
		return ZString.Empty;
	}

	public IPNTSAddressStreetAndNumber Address => null;
}
