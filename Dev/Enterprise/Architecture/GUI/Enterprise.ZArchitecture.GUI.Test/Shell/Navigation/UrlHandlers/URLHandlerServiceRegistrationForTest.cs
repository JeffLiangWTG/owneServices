using System;
using Enterprise.URLHandler;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class URLHandlerServiceRegistrationForTest : IDisposable
	{
		public URLHandlerServiceRegistrationForTest()
		{
			URLHandlerServiceRegistration.IsRunningUrlAuthenticationTest = true;
		}

		public bool IsRemotingServerRegistered
		{
			get { return URLHandlerServiceRegistration.IsRemotingServerRegistered; }
		}

		public string RegisteredLicenseKeyIdentifier
		{
			get { return URLHandlerServiceRegistration.RegisteredLicenseKeyIdentifier; }
		}

		public void Dispose()
		{
			URLHandlerServiceRegistration.IsRunningUrlAuthenticationTest = false;
		}
	}
}
