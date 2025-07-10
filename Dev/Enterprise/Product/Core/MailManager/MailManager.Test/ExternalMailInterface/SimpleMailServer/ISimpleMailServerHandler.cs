using System;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public interface ISimpleMailServerHandler : IDisposable
	{
		void Handle();
	}
}
