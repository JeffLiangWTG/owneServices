using System;
using CargoWise.Common;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class TcpIpRegistryAdjuster : ITcpIpRegistryAdjuster
	{
		public TcpIpRegistryAdjuster(IWindowsRegistryAdapter registryAdapter)
		{
			this.registryAdapter = registryAdapter ?? throw new ArgumentNullException(nameof(registryAdapter));
		}

		public void Adjust()
		{
			WriteSetting(TcpipRegistryParameter_MaxUserPort, 65534);
			WriteSetting(TcpipRegistryParameter_TcpTimedWaitDelay, 30);
		}

		public void TryAdjustIfRequired(IHostLogger logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}

			TryWriteIfRequired(TcpipRegistryParameter_MaxUserPort, 65534, logger);
			TryWriteIfRequired(TcpipRegistryParameter_TcpTimedWaitDelay, 30, logger);
		}

		void WriteSetting(string name, int value)
		{
			registryAdapter.SetLocalMachineDwordRegistryValue(TcpipRegistrySubKey, name, value);
		}

		void TryWriteIfRequired(string name, int value, IHostLogger logger)
		{
			try
			{
				var currentValue = registryAdapter.ReadLocalMachineDwordRegistryValue(TcpipRegistrySubKey, name);
				if (currentValue != value)
				{
					WriteSetting(name, value);
					logger.Log(LogLevel.Information, $"Tcp/Ip Registry Settings '{name}' adjusted from [{currentValue}] to [{value}]");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Log(LogLevel.Warning, $"Tcp/Ip Registry Settings '{name}' is recommended to have the value [{value}], but it cannot be adjusted: {ex.Message}");
			}
		}

		public const string TcpipRegistrySubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
		public const string TcpipRegistryParameter_MaxUserPort = "MaxUserPort";
		public const string TcpipRegistryParameter_TcpTimedWaitDelay = "TcpTimedWaitDelay";

		readonly IWindowsRegistryAdapter registryAdapter;
	}
}
