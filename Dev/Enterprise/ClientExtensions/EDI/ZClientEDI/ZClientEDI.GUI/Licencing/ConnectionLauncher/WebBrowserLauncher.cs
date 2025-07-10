using System;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public class WebBrowserLauncher : IConnectionLauncher
	{
		public WebBrowserLauncher(LicenceConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			this.connection = connection;
		}

		readonly LicenceConnection connection;

		public bool ShowProgressForm
		{
			get { return false; }
		}

		public void Launch(Progress progress)
		{
			WebUrlLauncher.Launch(connection.LK_RemoteAccessAddress);
		}
	}
}

