using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public static class CADMessageHelper
	{
		public static ZString StripOutInvalidCharacters(string input)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(input))
			{
				result = Regex.Replace(input, RegexValidationExtensions.DisallowedCharactersPattern, "");
			}
			return result;
		}
	}
}
