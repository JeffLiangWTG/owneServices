using System;
using System.Collections.Generic;

namespace CargoWise.Common.Enumeration
{
	public static class ZEnumerable
	{
		public static IEnumerable<T> Iterate<T>(T source, Func<T, T> generator)
		{
			Argument.NotNull(generator, nameof(generator));

			for (;;)
			{
				yield return source;
				source = generator(source);
			}
		}

		public static IEnumerable<T> Iterate<T>(T source, Func<T, T> generator, T stoppingPoint)
		{
			Argument.NotNull(generator, nameof(generator));

			return IterateUntil(source, generator, s => Equals(s, stoppingPoint));
		}

		public static IEnumerable<T> IterateUntil<T>(T source, Func<T, T> generator, Predicate<T> shouldStop)
		{
			Argument.NotNull(generator, nameof(generator));

			for (; !shouldStop(source); source = generator(source))
			{
				yield return source;
			}
		}
	}
}
