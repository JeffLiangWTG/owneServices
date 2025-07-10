using System;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public static class MessageBuilderHelper
	{
		internal static T MapCodeToEnumWithDefault<T>(this ZString itemValue) where T : struct
		{
			var succeeded = Enum.TryParse<T>(itemValue, true, out var result) && Enum.IsDefined(typeof(T), result);
			if (!succeeded)
			{
				result = default;
			}
			return result;
		}
	}
}
