using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public static class AuthorisationNumberValidationHelper
	{
		public static ZString GetAuthorisationNumberInvalidFormatMessage(this ZString number, ZString authorisationType)
		{
			if (number.Length != 17)
			{
				return Res.GetString("1A426A93-3A73-4537-BC17-8B795FE12D67", "Authorization Number must have 17 digits.");
			}

			var errors = new ZStringBuilder();
			if (!number.StartsWith(Core.Constants.CountryCodes.Germany))
			{
				errors.Append(Res.GetString("6DB22880-BC69-4E8C-9AAA-8678D1F52BC0", "Digit 01+02: Country Code 'DE' required."));
			}

			if (number.Substring(2, 3) != authorisationType)
			{
				errors.Append(Res.GetString("0B59B7CC-2A5B-4BAA-B2B9-192B36C3AE2C", "Digit 03-05: EU Authorization Type '{0}' is required.", authorisationType));
			}

			if (!number.SubstringSafe(5, 4).OnlyDigits())
			{
				errors.Append(Res.GetString("9F17D663-BFAA-4981-999F-A4F5BB23AD47", "Digit 06-09: Office Code of the issuing Main Customs Office required."));
			}

			var code = GetAuthorisationTypeCode(authorisationType);
			if (number.Substring(9, 2) != code)
			{
				errors.Append(Res.GetString("28F78999-9489-466F-8AB6-554F41FB759C", "Digit 10+11: Authorization Number of Type '{0}' requires '{1}'.", authorisationType, code));
			}

			if (!number.Substring(11, 6).OnlyDigits())
			{
				errors.Append(Res.GetString("ED9AE80F-37E2-41FA-818A-B64BD785A385", "Digit 12-17: Sequence Number of Customs Office for national Authorization Type required."));
			}

			return errors.ToStringWithNewLineBetweenAppends();
		}

		public static bool IsAuthorisationNumberValid(this ZString number, ZString authorisationType)
		{
			if (!number.IsEmpty && authorisationType.IsACEorACT())
			{
				return number.Length == 17
					&& number.StartsWith(Core.Constants.CountryCodes.Germany)
					&& number.Substring(2, 3) == authorisationType
					&& number.Substring(5, 4).OnlyDigits()
					&& number.Substring(9, 2) == GetAuthorisationTypeCode(authorisationType)
					&& number.Substring(11, 6).OnlyDigits();
			}

			return true;
		}

		public static bool IsACEorACT(this ZString authorisationType) => authorisationType == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit || authorisationType == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

		static bool OnlyDigits(this ZString str) => ((string)str).All(char.IsDigit);

		static string GetAuthorisationTypeCode(string authorisationType) => authorisationType == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit ? "ZE" : "ZT";
	}
}
