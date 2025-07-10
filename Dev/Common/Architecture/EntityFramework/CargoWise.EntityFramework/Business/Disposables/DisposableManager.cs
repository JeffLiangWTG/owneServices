using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public sealed class DisposableManager : Disposable
	{
		#region Constructors

		public DisposableManager()
		: base(isListened: true)
		{
		}

		readonly Stack<IDisposable> disposables = new Stack<IDisposable>();

		#endregion

		#region API

		public T Subscribe<T>(T disposable)
			where T : IDisposable
		{
			disposables.Push(disposable);
			return disposable;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				while (disposables.Count > 0)
				{
					var disposable = disposables.Pop();

					try
					{
						disposable?.Dispose();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"Error while disposing of resource [{disposable?.ToString()}]"), e);
					}
				}
			}
		}

		#endregion
	}
}
