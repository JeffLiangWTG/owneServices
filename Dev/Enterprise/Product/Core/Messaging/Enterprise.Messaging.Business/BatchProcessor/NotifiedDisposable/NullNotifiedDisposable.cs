using System;
using CargoWise.Common;

namespace Enterprise.Messaging.Business
{
	public sealed class NullNotifiedDisposable : Disposable, INotifiedDisposable
	{
		public NullNotifiedDisposable(IDisposable disposable)
		{
			this.disposable = disposable;
		}

		readonly IDisposable disposable;

		public void Notify()
		{
			// Do nothing.
		}

		#region Disposable Support

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				disposable.Dispose();
			}
		}

		#endregion
	}
}
