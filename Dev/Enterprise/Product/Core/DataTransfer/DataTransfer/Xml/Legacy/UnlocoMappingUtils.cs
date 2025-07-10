using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class UnlocoMappingUtils
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static string GetPortCodeFromAustralianState(string stateCode)
		{
			string result = "";

			switch (stateCode.ToUpper().Replace(".", ""))
			{
				case "NSW":
				case "NEW SOUTH WALES":
					result = "AUSYD";
					break;
				case "VIC":
				case "VICTORIA":
					result = "AUMEL";
					break;
				case "QLD":
				case "QUEENSLAND":
					result = "AUBNE";
					break;
				case "WA":
				case "WESTERN AUSTRALIA":
					result = "AUPER";
					break;
				case "SA":
				case "SOUTH AUSTRALIA":
					result = "AUADL";
					break;
				case "TAS":
				case "TASMANIA":
					result = "AUHBA";
					break;
				case "NT":
				case "NORTHERN TERRITORY":
					result = "AUDRW";
					break;
				default:
					result = "ZZZZZ";
					break;
			}

			return result;
		}

		public static ZString GetPortCodeFromCountry(ZString countryCode)
		{
			ZString result = countryCode.Left(2).PadRight(5, 'Z');
			return result;
		}
	}
}
