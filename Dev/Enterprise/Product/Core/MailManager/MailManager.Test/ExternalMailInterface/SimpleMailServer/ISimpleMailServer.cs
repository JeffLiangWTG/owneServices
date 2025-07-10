using System;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public interface ISimpleMailServer : IDisposable
	{
		ushort Port { get; }

		void Start();

		void Stop();
	}
}
