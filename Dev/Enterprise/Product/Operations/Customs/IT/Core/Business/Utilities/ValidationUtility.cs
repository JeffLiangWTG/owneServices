using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class ValidationUtility
{
	public static ZBool IsStartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacter(ZString authorisationNumber) => Regex.IsMatch(authorisationNumber, StartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacterRegex);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant regex pattern")]
	const string StartWithOneOrMoreDigitsAndEndWithAnUppercaseAlphabeticCharacterRegex = @"^\d+[A-Z]{1}$";
}
