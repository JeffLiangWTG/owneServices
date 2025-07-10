using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common.Testing;

namespace CargoWise.Common
{
	public abstract class Disposable : IDisposable
	{
		#region Constructors

		protected Disposable(bool isListened = true)
		{
			IsListened = isListened;
			if (isListened)
			{
				DisposableLeakListener.Instance.RegisterDisposable(this);
			}
		}

		#endregion

		#region Properties

		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Just put true or false to enable or disable DisposableLeakListener upon this object. Do not implement instance-level logics to avoid unexpected consequences.
		/// </summary>
		public bool IsListened { get; private set; }

		#endregion

		#region Dispose

		[SuppressMessage("Microsoft.Design", "CA1063:Implement IDisposable correctly")]
		public void Dispose()
		{
			if (!IsDisposed)
			{
				try
				{
					Dispose(true);
				}
				finally
				{
					IsDisposed = true;
					if (IsListened)
					{
						DisposableLeakListener.Instance.UnRegisterDisposable(this);
					}
					GC.SuppressFinalize(this);
				}
			}
		}

		/// <summary>
		/// Called to actually dispose the current object.
		/// </summary>
		/// <param name="isDisposing">Whether the call comes from Dispose() or the finalizer.</param>
		protected abstract void Dispose(bool isDisposing);

		#endregion
	}
}
