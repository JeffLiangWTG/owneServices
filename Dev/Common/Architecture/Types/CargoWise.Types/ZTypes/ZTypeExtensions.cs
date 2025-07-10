using System;

namespace CargoWise.Types
{
	public static class ZTypeExtensions
	{
		public static T IfEmptyUse<T>(this T sourceValue, Func<T> getfallValue) where T : IZType
		{
			return sourceValue.IsEmpty ? getfallValue() : sourceValue;
		}
	}
}
