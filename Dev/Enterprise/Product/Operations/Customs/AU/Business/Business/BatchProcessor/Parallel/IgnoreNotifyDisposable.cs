using System;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class IgnoreNotifyDisposable : INotifiedDisposable
	{
		readonly IDisposable disposable;

		public IgnoreNotifyDisposable(IDisposable disposable)
		{
			this.disposable = disposable;
		}

		public void Dispose()
		{
			disposable.Dispose();
		}

		public void Notify()
		{
			// notify is ignored by this class
		}
	}
}
