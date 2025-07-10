using System;

namespace Enterprise.Customs.DE.Business
{
	internal static class ComparerHelper
	{
		internal static bool Compare<T>(T x, T y, Func<T, T, bool> action)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}
			return action(x, y);
		}
	}
}
