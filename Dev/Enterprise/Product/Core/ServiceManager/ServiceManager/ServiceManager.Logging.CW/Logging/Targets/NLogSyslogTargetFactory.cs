using Enterprise.ServiceManager.Shared.Interfaces;
using NLog;
using NLog.Targets;
using NLog.Targets.Syslog;
using NLog.Targets.Syslog.Settings;
using NLog.Targets.Wrappers;
using ServiceManager.Shared.CW;

namespace ServiceManager.Logging.CW
{
	class NLogSyslogTargetFactory : INLogTargetFactory
	{
		public Target? GetOrCreateTarget()
		{
			var registry = (ILoggerRegistrySettings)SharedRegistry.Instance;
			if (!registry.SyslogLoggingEnabled)
			{
				return null;
			}

			const string targetName = "syslog";

			var existingBufferingTarget = LogManager.Configuration.FindTargetByName<BufferingTargetWrapper>(targetName);
			if (existingBufferingTarget != null)
			{
				return  existingBufferingTarget;
			}

			var syslogTarget = new SyslogTarget
			{
				MessageCreation = new MessageBuilderConfig()
				{
					Facility = Facility.User,
					Rfc = registry.ProcessControllerSyslogProtocolVersion == "RFC5424" ? RfcNumber.Rfc5424 : RfcNumber.Rfc3164,
				},
				Name = targetName,
			};

			var messageSend = new MessageTransmitterConfig();

			if (registry.ProcessControllerSyslogNetworkProtocol == "TCP")
			{
				messageSend.Protocol = ProtocolType.Tcp;

				messageSend.Tcp = new TcpConfig()
				{
					Port = registry.ProcessControllerSyslogServerPort,
					Server = registry.ProcessControllerSyslogServerHostname,
					Tls = new TlsConfig()
					{
						Enabled = registry.ProcessControllerSyslogSslEnable,
					}
				};
			}
			else
			{
				messageSend.Protocol = ProtocolType.Udp;

				messageSend.Udp = new UdpConfig()
				{
					Port = registry.ProcessControllerSyslogServerPort,
					Server = registry.ProcessControllerSyslogServerHostname,
				};
			}

			syslogTarget.MessageSend = messageSend;

			var syslogBufferWrapper = new BufferingTargetWrapper
			{
				WrappedTarget = syslogTarget,
				BufferSize = 50,
				FlushTimeout = 2000,
				SlidingTimeout = true,
				Name = targetName,
			};
			return syslogBufferWrapper;
		}
	}
}
