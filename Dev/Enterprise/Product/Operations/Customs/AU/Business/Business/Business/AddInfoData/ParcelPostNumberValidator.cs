using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ParcelPostNumberValidator
	{
		public ZString Validate(string parcelPostNumber)
		{
			if (parcelPostNumber[0] != '1' && parcelPostNumber[0] != '2')
			{
				return "The parcel post card number '" + parcelPostNumber + "' doesn't start with a '1' or '2'";
			}
			string number = ((ZString)parcelPostNumber).SubstringSafe(1);
			bool hasValidStateCode = false;
			foreach (string validStateCode in validStateCodes)
			{
				if (number.StartsWith(validStateCode))
				{
					hasValidStateCode = true;
					number = number.Substring(validStateCode.Length);
					break;
				}
			}
			if (!hasValidStateCode)
			{
				return "The parcel post card number '" + parcelPostNumber + "' must have a valid state/territory at its second character";
			}

			if (((ZString)number).Trim("1234567890".ToCharArray()).Length > 0)
			{
				return "The parcel post card number '" + parcelPostNumber + "' is invalid because it should only contain numbers after the state/territory code";
			}
			return "";
		}

		readonly string[] validStateCodes = new string[] { "NT", "N", "V", "Q", "S", "W", "T", "A" };
	}
}
