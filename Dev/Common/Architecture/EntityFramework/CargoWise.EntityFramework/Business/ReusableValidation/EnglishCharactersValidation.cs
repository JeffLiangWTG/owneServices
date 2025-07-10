using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class EnglishCharactersValidation : ValidationProvider
	{
		public static void ErrorIfNotWesternEuropean(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddError(GetNotificationMessage(info));
			}
		}

		public static void MessageErrorIfNotWesternEuropean(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddMessageError(GetNotificationMessage(info));
			}
		}

		public static void WarnIfNotWesternEuropean(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddWarning(GetNotificationMessage(info));
			}
		}

		static bool IsValid(ZPropertyInfo info)
		{
			return ((ZString)info.Value.ToString()).IsWesternEuropeanOrEmpty;
		}

		public static ZString GetNotificationMessage(ZPropertyInfo info)
		{
			return info.HumanReadableName + " " + Res.GetString("4d817f2e-33d7-423b-8646-d0f6e92aeb8c", "only accepts Western European languages characters.");
		}
	}
}
