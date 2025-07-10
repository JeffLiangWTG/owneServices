using System;

namespace Enterprise.Messaging.Business
{
	public interface INotifiedDisposable : IDisposable
	{
		void Notify();
	}
}
