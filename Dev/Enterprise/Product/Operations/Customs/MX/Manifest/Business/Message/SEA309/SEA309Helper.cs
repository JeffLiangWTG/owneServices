using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal static class SEA309Helper
	{
		internal static ZString WeightUnitCodeCalculator(ZString uQ)
		{
			ZString res;
			switch (uQ)
			{
				case Core.Constants.Weight.Pounds:
					res = WeightConstants.Pounds;
					break;
				case Core.Constants.Weight.Tonnes:
					res = WeightConstants.Tonnes;
					break;
				case Core.Constants.Weight.LongTons:
					res = WeightConstants.LongTons;
					break;
				case Core.Constants.Weight.ShortTons:
					res = WeightConstants.ShortTones;
					break;
				default:
					res = WeightConstants.Kilograms;
					break;
			}
			return res;
		}

		internal static ZString VolumeUnitCodeCalculator(ZString uQ)
		{
			ZString res;
			switch (uQ)
			{
				case Core.Constants.Volume.CubicMetres:
					res = VolumeConstants.CubicMetres;
					break;
				case Core.Constants.Volume.CubicDecimetres:
					res = VolumeConstants.CubicDecimetres;
					break;
				case Core.Constants.Volume.CubicFeet:
					res = VolumeConstants.CubicFeet;
					break;
				case Core.Constants.Volume.USGallons:
					res = VolumeConstants.USGallons;
					break;
				case Core.Constants.Volume.CubicInches:
					res = VolumeConstants.CubicInches;
					break;
				case Core.Constants.Volume.Litre:
					res = VolumeConstants.Litre;
					break;
				default:
					res = VolumeConstants.CubicCentimeters;
					break;
			}
			return res;
		}

		internal static ZString CalculateFlashPointNumeric(ZString flashpoint)
		{
			ZStringBuilder result = new ZStringBuilder();

			var length = flashpoint.Contains(SEA309Constants.To) ? flashpoint.IndexOf(SEA309Constants.To) : flashpoint.Length;

			for (int i = 0; i < length; i++)
			{
				if (flashpoint[i] == SEA309Constants.Minus || char.IsNumber(flashpoint[i]))
				{
					result.Append(flashpoint[i].ToString());
				}
			}

			return result.ToString();
		}

		internal static ZString CarrierCode(AsycudaManifestHeader header)
		{
			ZString result = header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Mexico)?.OK_CustomsRegNo ?? ZString.Empty;

			if (result.IsEmpty)
			{
				result = header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode && c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates)?.OK_CustomsRegNo ?? ZString.Empty;
			}

			if (result.IsEmpty)
			{
				result = header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == OrgCusCode.CodeTypes.CarrierCode)?.OK_CustomsRegNo ?? ZString.Empty;
			}

			return result;
		}

		internal static ZBool MustStateCodeBeSent(ZString partyCountry) => partyCountry == Core.Constants.CountryCodes.Canada || partyCountry == Core.Constants.CountryCodes.Mexico || partyCountry == Core.Constants.CountryCodes.UnitedStates;
	}
}
