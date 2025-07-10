using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class EnglishStrictCharactersValidation : ValidationProvider
	{
		public static void ErrorIfNotEnglish(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddError(GetNotificationMessage(info));
			}
		}

		public static void MessageErrorIfNotEnglish(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddMessageError(GetNotificationMessage(info));
			}
		}

		public static void WarnIfNotEnglish(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddWarning(GetNotificationMessage(info));
			}
		}

		static bool IsValid(ZPropertyInfo info)
		{
			return ((ZString)info.Value.ToString()).IsEnglishOnlyOrEmpty;
		}

		public static ZString GetNotificationMessage(ZPropertyInfo info)
		{
			return info.HumanReadableName + " " + Res.GetString("b08c0eb2-8402-4c3e-8c45-b0611ebe7e00", "only accepts English language characters.");
		}
	}
}
