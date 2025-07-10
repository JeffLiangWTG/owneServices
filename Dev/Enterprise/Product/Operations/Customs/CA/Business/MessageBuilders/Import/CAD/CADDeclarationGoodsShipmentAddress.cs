using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclarationGoodsShipmentAddress : ICADMessageDeclarationGoodsShipmentAddress
{
	public CADDeclarationGoodsShipmentAddress(JobDocAddress docAddress, bool isSeller = false)
	{
		this.docAddress = docAddress;
		this.isSeller = isSeller;
	}

	readonly JobDocAddress docAddress;
	readonly bool isSeller;

	#region ICADMessageDeclarationGoodsShipmentAddress

	public string CityName => CADMessageHelper.StripOutInvalidCharacters(docAddress?.City ?? ZString.Empty).SubstringSafe(0, 40).ToUpper();

	public string CountryCode
	{
		get
		{
			if (docAddress != null && docAddress.Country != null)
			{
				return docAddress.Country.Code;
			}
			return ZString.Empty;
		}
	}

	public string CountrySubDivisionCode
	{
		get
		{
			if (isSeller && CountryCode != Core.Constants.CountryCodes.UnitedStates && CountryCode != Core.Constants.CountryCodes.Canada)
			{
				return ZString.Empty;
			}
			return docAddress?.StateCode.SubstringSafe(0, 2).ToUpper() ?? ZString.Empty;
		}
	}

	public string LineText
	{
		get
		{
			var result = ZString.Empty;
			if (docAddress != null)
			{
				result = docAddress.E2_Address1.Trim();
				var address2 = docAddress.E2_Address2.Trim();
				if (!address2.IsEmpty)
				{
					if (!result.IsEmpty)
					{
						result += " ";
					}
					result += address2;
				}
			}
			return CADMessageHelper.StripOutInvalidCharacters(result).SubstringSafe(0, 60).ToUpper();
		}
	}

	public string PostcodeID => docAddress?.Postcode ?? ZString.Empty;

	public string PostOfficeBoxID => ZString.Empty;

	public bool IsEmpty => ((ZString)CityName).IsEmpty && ((ZString)CountryCode).IsEmpty && ((ZString)CountrySubDivisionCode).IsEmpty && ((ZString)LineText).IsEmpty && ((ZString)PostcodeID).IsEmpty && ((ZString)PostOfficeBoxID).IsEmpty;

	#endregion
}
