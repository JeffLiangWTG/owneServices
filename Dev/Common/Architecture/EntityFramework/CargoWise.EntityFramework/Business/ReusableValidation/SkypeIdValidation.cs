using System.Text.RegularExpressions;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class SkypeIdValidation : ValidationProvider
	{
		public void ValidateSkypeId(ZPropertyInfo propertyInfo)
		{
			var value = (ZString)propertyInfo.Value;
			if (!IsValidSkypeName(value) && !EmailAddressValidation.IsEmailAddressValid(value))
			{
				propertyInfo.AddError(InvalidSkypeIdErrorMessage);
			}
		}

		public static string InvalidSkypeIdErrorMessage
		{
			get { return Res.GetString("3d28e086-f104-4b2c-91ce-a1d137ab972e", "Not a valid Skype name nor email address."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "regex expression")]
		bool IsValidSkypeName(ZString skypeName)
		{
			const string skypeNamePattern = @"^[a-zA-Z][a-zA-Z0-9\.,\-_]{5,31}$";

			var skypeNameRegex = new Regex(skypeNamePattern);
			return skypeNameRegex.IsMatch(skypeName);
		}
	}
}
