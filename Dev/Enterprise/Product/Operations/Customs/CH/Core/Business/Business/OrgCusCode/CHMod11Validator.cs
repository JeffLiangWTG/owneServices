using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CHMod11Validator
{
	public void Validate(ZString codeType, ZPropertyInfo codeInfo)
	{
		ZString code = ((ZString)codeInfo.Value);

		if (!IsValidPattern(code))
		{
			AddMessage(codeInfo, Res.GetString("526334B6-D9FF-4247-A7DA-7F10E07CF893", @"CH {0} number should start with ""CHE"" or ""E"" followed by 9 numeric digits. (CHENNNNNNNNN, CHE-NNN.NNN.NNN, ENNNNNNNNN or E-NNN.NNN.NNN)", codeType));
		}
		else if (!IsValidCheckDigit(code))
		{
			AddMessage(codeInfo, Res.GetString("175B3E86-CC3B-4790-8D21-2BBCCA5C2B32", "CH {0} code is incorrect (checksum error). Please check input.", codeType));
		}
	}

	void AddMessage(ZPropertyInfo codeInfo, string message)
	{
		codeInfo.AddMessageError(message);
	}

	public static bool IsValidPattern(ZString code)
	{
		return Regex.IsMatch(code, "^(CHE-|CHE|E-|E)([0-9]{3}[.][0-9]{3}.[0-9]{3}|[0-9]{9})(?= MWST|MWST| TVA|TVA| IVA|IVA|$)", RegexOptions.IgnoreCase);
	}

	public static bool IsValidCheckDigit(ZString code)
	{
		bool result = false;

		code = code.Replace("CHE-", "X").Replace("CHE", "X").Replace("E-", "X").Replace("E", "X").Replace("X", "CHE").Replace("MWST", "").Replace("TVA", "").Replace("IVA", "").Replace(".", "").Trim();

		if (code.Length == 12)
		{
			int productMod11 = ((int.Parse(code[3].ToString(), CultureInfo.InvariantCulture) * 5) +
								(int.Parse(code[4].ToString(), CultureInfo.InvariantCulture) * 4) +
								(int.Parse(code[5].ToString(), CultureInfo.InvariantCulture) * 3) +
								(int.Parse(code[6].ToString(), CultureInfo.InvariantCulture) * 2) +
								(int.Parse(code[7].ToString(), CultureInfo.InvariantCulture) * 7) +
								(int.Parse(code[8].ToString(), CultureInfo.InvariantCulture) * 6) +
								(int.Parse(code[9].ToString(), CultureInfo.InvariantCulture) * 5) +
								(int.Parse(code[10].ToString(), CultureInfo.InvariantCulture) * 4)) % 11;

			int checksum = (productMod11 == 0) ? productMod11 : 11 - productMod11;

			result = int.Parse(code[11].ToString(), CultureInfo.InvariantCulture) == checksum;
		}

		return result;
	}

	public static bool IsValid(ZString code) => IsValidPattern(code) && IsValidCheckDigit(code);
}
