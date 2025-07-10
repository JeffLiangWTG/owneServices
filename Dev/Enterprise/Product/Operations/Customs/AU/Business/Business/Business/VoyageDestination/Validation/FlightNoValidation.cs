using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FlightNoValidation : ValidationProvider
	{
		public static string ValidateFlightNo(ZPropertyInfo flightNoInfo)
		{
			string warningMessage = "";
			ZString flightNo = ((ZString)flightNoInfo.Value).Replace(" ", "").Trim();

			if (flightNo.Length < 3)
			{
				warningMessage = "Flight Number must be greater than 3 characters long";
			}
			else if (flightNo.Length > 6)
			{
				warningMessage = "Flight Number must be less than 6 characters long";
			}
			else
			{
				string theFirstTwoChars = flightNo.Left(2);
				bool flightNumberValid = false;
				if (Char.IsLetter(theFirstTwoChars[0]))
				{
					flightNumberValid = true;
					flightNumberValid &= Char.IsLetterOrDigit(theFirstTwoChars[1]);
				}
				else if (Char.IsDigit(theFirstTwoChars[0]))
				{
					flightNumberValid = true;
					flightNumberValid &= Char.IsLetter(theFirstTwoChars[1]);
				}

				if (flightNumberValid)
				{
					foreach (char aCharacter in flightNo.Substring(2))
					{
						flightNumberValid &= Char.IsDigit(aCharacter);
					}
				}
				if (!flightNumberValid)
				{
					warningMessage = "Flight Numbers should be in a format of AN, NA or AA followed by up to four numbers(A: Alpha, N:Numeric).";
				}
			}

			return warningMessage;
		}
	}
}
