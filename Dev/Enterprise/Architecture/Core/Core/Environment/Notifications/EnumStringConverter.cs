using System;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Environment
{
	public static class EnumStringConverter
	{
		public static T ConvertStringToEnumEntry<T>(INotification notification, T defaultValue)
			where T : struct
		{
			Argument.NotNull(notification, "notification");
			T result = defaultValue;

			return Enum.TryParse(notification.Type.EnumValueName, out result) ? result : defaultValue;
		}
	}
}
