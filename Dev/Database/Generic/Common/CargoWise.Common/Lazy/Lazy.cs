using System;

namespace CargoWise.Common
{
	public static class Lazy
	{
		public static Lazy<T> Create<T>(Func<T> creator, bool isThreadSafe = false)
		{
			Argument.NotNull(creator, nameof(creator));

			return new Lazy<T>(creator, isThreadSafe);
		}
	}
}
