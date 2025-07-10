using System;
using CargoWise.Common;

namespace CargoWise.Types
{
	public static class ObjectCache
	{
		public static void Initialize(IObjectCache objectCache)
		{
			Argument.NotNull(objectCache, nameof(objectCache));
			CultureProvider = objectCache.CultureProvider;
			DateTimeProvider = objectCache.DateTimeProvider;
			RoundingProvider = objectCache.RoundingProvider;
		}

#if DEBUG
		public static IDisposable OverrideDateTimeProvider(IDateTimeProvider dateTimeProvider)
		{
			var current = DateTimeProvider;
			DateTimeProvider = dateTimeProvider;
			DisposableAction action = null;
			action = new DisposableAction(() =>
			{
				DateTimeProvider = current;
				CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(action);
			});
			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(action);
			return action;
		}
#endif

		public static ICultureProvider CultureProvider
		{
			get => cultureProvider;
			private set => cultureProvider = value ?? throw new ArgumentNullException(nameof(value));
		}

		public static IDateTimeProvider DateTimeProvider
		{
			get => dateTimeProvider;
			private set => dateTimeProvider = value ?? throw new ArgumentNullException(nameof(value));
		}

		public static IRoundingProvider RoundingProvider
		{
			get => roundingProvider;
			private set => roundingProvider = value ?? throw new ArgumentNullException(nameof(value));
		}

		static ICultureProvider cultureProvider;
		static IDateTimeProvider dateTimeProvider;
		static IRoundingProvider roundingProvider;
	}
}
