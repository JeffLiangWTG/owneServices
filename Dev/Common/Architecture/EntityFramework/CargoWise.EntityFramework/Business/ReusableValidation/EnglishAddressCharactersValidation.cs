using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class EnglishAddressCharactersValidation : ValidationProvider
	{
		public static void MessageErrorIfNotEnglish(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddMessageError(GetNotificationMessage(info));
			}
		}

		static bool IsValid(ZPropertyInfo info)
		{
			var stringValue = (ZString)info.Value.ToString();
			return stringValue.IsEnglishOnlyOrEmpty || stringValue.RemoveDiacritics().IsEnglishOnlyOrEmpty;
		}

		public static ZString GetNotificationMessage(ZPropertyInfo info)
		{
			return info.HumanReadableName + " " + Res.GetString("8e285650-56f0-4e05-ba71-56c4d50e4040", "only accepts English language characters.");
		}
	}
}
