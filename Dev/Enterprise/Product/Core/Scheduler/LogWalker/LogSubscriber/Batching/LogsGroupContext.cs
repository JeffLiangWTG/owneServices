using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.LogWalker
{
	public sealed class LogsGroupContext : IDisposable
	{
		public LogsGroupContext(bool skipGroup, params IDisposable[] disposables)
		{
			SkipGroup = skipGroup;
			this.disposables = disposables;
		}

		public bool SkipGroup { get; }
		readonly IDisposable[] disposables;

		#region IDisposable Support

		bool disposedValue;

		public void Dispose()
		{
			if (!disposedValue)
			{
				var exceptions = new List<Exception>();
				foreach (var disposable in disposables)
				{
					try
					{
						disposable?.Dispose();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("LogsGroupContext.DisposeError", ex);
						exceptions.Add(ex);
					}
				}
				disposedValue = true;

				if (exceptions.Count > 0)
				{
					throw new AggregateException(exceptions);
				}
			}
		}
		#endregion
	}
}
