using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class DiacriticsValidation : ValidationProvider
	{
		public static void ErrorIfContainsAnyDiacritics(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddError(GetNotificationMessage(info));
			}
		}

		public static void MessageErrorIfContainsAnyDiacritics(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddMessageError(GetNotificationMessage(info));
			}
		}

		public static void WarnIfContainsAnyDiacritics(ZPropertyInfo info)
		{
			if (!IsValid(info))
			{
				info.AddWarning(GetNotificationMessage(info));
			}
		}

		static bool IsValid(ZPropertyInfo info)
		{
			return !((ZString)info.Value.ToString()).ContainsAnyDiacritics;
		}

		public static ZString GetNotificationMessage(ZPropertyInfo info)
		{
			return info.HumanReadableName + " " + Res.GetString("2cce08d8-9ac1-4825-8a21-62733b0cb213", "should not contain any characters with diacritics.");
		}
	}
}
