using System;
using CargoWise.Common;
using WTG.Foundation.FrameworkExtensions.Functional;

namespace CargoWise.Types
{
	public static class OptionExtensions
	{
		public static Option<TResult> MapTo<TResult>(this Option<ZDate> option, Func<ZDate, TResult> mapping)
			=> option.MapZType(mapping);
		public static Option<TResult> MapTo<TResult>(this Option<ZDateTime> option, Func<ZDateTime, TResult> mapping)
			=> option.MapZType(mapping);
		public static Option<TResult> MapTo<TResult>(this Option<ZDateTimeOffset> option, Func<ZDateTimeOffset, TResult> mapping)
			=> option.MapZType(mapping);

		public static Option<TResult> MapTo<TResult>(this Option<ZGeography> option, Func<ZGeography, TResult> mapping)
			=> option.MapZType(mapping);

		public static Option<TResult> MapTo<TResult>(this Option<ZGuid> option, Func<ZGuid, TResult> mapping)
			=> option.MapZType(mapping);

		static Option<TResult> MapZType<T, TResult>(this Option<T> option, Func<T, TResult> mapping)
			where T : struct, IZType
		{
			Argument.NotNull(mapping, nameof(mapping));

			if (!option.TryGet(out var value)
				|| !value.IsValid)
			{
				return Option.None<TResult>();
			}
			else
			{
				return mapping(value).ToOption();
			}
		}
	}
}
