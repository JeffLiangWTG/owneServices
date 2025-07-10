using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CargoWise.Common
{
	public static class ApplicationDispatcher
	{
		public static SynchronizationContext Current
		{
			get => current;
			set
			{
				current = value;
				MainThread = value == null ? null : Thread.CurrentThread;
			}
		}

		public static Exception ThreadException { get => threadException.Value; }

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static SynchronizationContext current;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		public static Thread MainThread { get; private set; }

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly ThreadLocal<Exception> threadException = new();

		public static IDisposable RecordThreadException(Exception ex)
		{
			var originalValue = threadException.Value;
			threadException.Value = ex;
			return new DisposableAction(() => threadException.Value = originalValue);
		}
	}
}
