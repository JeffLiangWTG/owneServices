using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PermitNumberValidation : ValidationProvider
	{
		public void ValidatePermitNumbers(IEnumerable<ZString> permitNumbers, ZPropertyInfo property)
		{
			StringBuilder invalidNumbers = new StringBuilder();
			foreach (ZString permitNumber in permitNumbers)
			{
				if (!IsValid(permitNumber.Trim()))
				{
					invalidNumbers.Append(permitNumber + ",");
				}
			}

			if (invalidNumbers.Length > 0)
			{
				property.AddWarning(invalidNumbers.ToString().Trim(',') + " are not valid.");
			}
		}

		bool IsValid(ZString permitNumber)
		{
			ZString prefix = permitNumber.Length >= 3 ? permitNumber.Substring(0, 3) : permitNumber;
			ZString suffix = prefix.Length == 3 ? permitNumber.Substring(3).Trim() : ZString.Empty;
			int suffixLen = suffix.Length;

			if (prefix.IsEmpty)
			{
				return true;
			}

			switch (prefix)
			{
				case "AHC":
					return CheckPict(suffix, "9999X");
				case "PWS":
					return CheckPict(suffix, "A999999") && (suffix[0] == 'A' || suffix[0] == 'P');
				case "OZO":
					return CheckPict(suffix, "9999A");
				case "HWA":
					return CheckPict(suffix, "999999X");
				case "WBC":
					return CheckPict(suffix, "99999X") || CheckPict(suffix, "999999X");
				case "CSH":
					return CheckPict(suffix, "99A9999X");

				case "HEA":
				case "HBE":
					return CheckPict(suffix, "9999999999");

				case "DEC":
				case "DED":
					return CheckPict(suffix, ".AAA9999999999999");

				case "MEP":
				case "CNO":
					return CheckPict(suffix, "/999/AAA/999X") || CheckPict(suffix, "/999/AAAA/999X");

				case "PIA":
				case "PIM":
				case "PIP":
				case "PID":
					return CheckPict(suffix, "9999999");

				case "DOD":
				case "PIL":
				case "PIF":
				case "PIH":
					return true;
				default:
					return false;
			}
		}

		static bool CheckPict(string suffix, string checkString)
		{
			if (suffix.Length != checkString.Length)
			{
				return false;
			}

			for (int index = 0; index < checkString.Length; index++)
			{
				char charAt = checkString[index];
				bool isValid = false;
				switch (charAt)
				{
					case 'A':
						isValid = (suffix[index] >= 'A' && suffix[index] <= 'Z');
						break;
					case '9':
						isValid = (suffix[index] >= '0' && suffix[index] <= '9');
						break;
					case '.':
						isValid = suffix[index].Equals('.');
						break;
					case '/':
						isValid = suffix[index].Equals('/');
						break;
					case 'X':
						isValid = true;
						break;
					default:
						isValid = false;
						break;
				}
				if (!isValid)
				{
					return isValid;
				}
			}
			return true;
		}
	}
}
