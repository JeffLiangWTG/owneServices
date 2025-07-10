using System;
using System.Collections;
using System.Configuration.Install;

namespace Enterprise.ServiceManager.Host
{
	public class TcpIpRegistrySettingsInstaller : Installer
	{
		public TcpIpRegistrySettingsInstaller()
			: this(new TcpIpRegistryAdjuster(new WindowsRegistryAdapter()))
		{
		}

		internal TcpIpRegistrySettingsInstaller(ITcpIpRegistryAdjuster tcpIpRegistryAdjuster)
		{
			this.tcpIpRegistryAdjuster = tcpIpRegistryAdjuster ?? throw new ArgumentNullException(nameof(tcpIpRegistryAdjuster));
		}

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			tcpIpRegistryAdjuster.Adjust();
		}

		readonly ITcpIpRegistryAdjuster tcpIpRegistryAdjuster;
	}
}
