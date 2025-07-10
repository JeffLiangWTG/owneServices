using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Ccsuk.Connection.Exceptions;
using Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.ShortMessage;
using Enterprise.Customs.GB.Ccsuk.ServiceTask;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers.CcsukEmailSender;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	/// <summary>
	/// Singleton to connect & login
	/// </summary>
	public class CcsukSession
	{
		public static CcsukSession GetSession(ILogger logger)
		{
			lock (padlock)
			{
				if (instance == null)
				{
					instance = new CcsukSession(logger);
				}
				return instance;
			}
		}

		public bool IsConnected { get; private set; }

		protected CcsukSession(ILogger logger)
		{
			var ipAddressHelper = new IpAddressHelper(logger);
			var pair = ipAddressHelper.GetNextAddress();
			Exception lastException = null;
			bool keepTrying = true;
			while (pair != null && keepTrying)
			{
				try
				{
					logger.Log(LogType.Information, "Attempting logon with pair " + pair.FriendlyName);
					tcpIpSenderReceiver = GetTcpIpSenderReceiver(logger, pair);
					tcpIpSenderReceiver.ConnectAndLogonAndStartReceivingInboundMessages();
					IsConnected = true;  // we assume!

					GBCustomsDataRegistry.Instance.CcsukLastConnectedProfile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, $"{pair.FullDescription}; Remote: {tcpIpSenderReceiver.RemoteHostIpAddress}");
					break;
				}
				catch (CannotLogonException ex)
				{
					lastException = ex;
					logger.Log(LogType.Information, ex.Message);
					logger.Log(LogType.Information, @"CW1 will not attempt to use any other profiles as no amount of retries will resolve the login failure.
CUK service task has been deactivated");

					DeactivateCUKServiceTask();
					SendEmailToReportLogonException(ex);
					keepTrying = false;
				}
				catch (CcsukException ex)
				{
					lastException = ex;
					logger.Log(LogType.Information, ex.Message);
				}
				pair = ipAddressHelper.GetNextAddress();
			}

			if (!IsConnected && keepTrying)
			{
				throw new Exceptions.Misc.NoMoreAcceptableProfilesLeftException(lastException);
			}
		}

		void SendEmailToReportLogonException(CannotLogonException ex)
		{
			var message = @"Could not log on to CCSUK network.  This is most likely because the password is incorrect or the host has been barred.
The CUK service task has been deactivated.";
			SendWarningEmailAboutCannotLogon(message, ex, ToWhom.CustomsGroupOnly);
		}

		public static void SendWarningEmailAboutCannotLogon(string message, Exception ex, ToWhom whom)
		{
			var body = string.Format(@"{0}

Please check service task with code CUK for details in its logs.

Exception message:

{1}", message, ex.Message);
			body = body.Replace(System.Environment.NewLine, "<BR/>" + System.Environment.NewLine);
			var subject = "CCSUK service task failed to log on to network";
			var factory = new BusinessObjectFactory();
			new CcsukEmailSender(factory, whom, null).SendEmailWithoutIndividualJobLink(subject, body, GBCustomsDataRegistry.Instance.NotificationCcsukErrors, "", Guid.Empty, Guid.Empty, Guid.Empty);
			factory.Save();
		}

		internal static void DeactivateCUKServiceTask()
		{
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskIsActive(CcsukServiceTaskConstants.CcsukServiceTaskCode, isActive: false);
		}

#if DEBUG
		protected virtual
#endif
		TcpIpSenderReceiver GetTcpIpSenderReceiver(ILogger logger, CcsukIpaddressesSetting ipAddress) => new TcpIpSenderReceiver(logger, ipAddress);

		public void SendAllWaitingInterchanges()
		{
			if (IsConnected)
			{
				int batchSize = GBCustomsDataRegistry.Instance.CcsukMaximumBatchSize.Value;
				IsConnected = tcpIpSenderReceiver.SendAllWaitingOutboundInterchanges(batchSize);
			}
		}

		public void PingIfNeeded()
		{
			if (IsConnected && ShouldPingNow)
			{
				IsConnected = tcpIpSenderReceiver.SendAndReceivePing();
				RecordLastPingTime();
			}
		}

		public void ShutdownAndDisposeInstance()
		{
			tcpIpSenderReceiver.Shutdown();
			Dispose();
		}

		public static void Dispose()
		{
			tcpIpSenderReceiver = null;
			instance = null;
		}

		public static bool ShouldPingNow
		{
			get
			{
				var pingPeriod = GBCustomsDataRegistry.Instance.CcsukPingPeriod.Value;
				if (pingPeriod < 1)
				{
					pingPeriod = 600;// 10 mins
				}
				int periodOfPingingInSecondsNegative = -1 * pingPeriod;
				return GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.Value < ZDateTime.UtcNow.AddSeconds(periodOfPingingInSecondsNegative).ToDateTime()
					||
					GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.Value > ZDateTime.UtcNow;
			}
		}

		void RecordLastPingTime()
		{
			GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
		}

		static TcpIpSenderReceiver tcpIpSenderReceiver;
		static CcsukSession instance;
		static readonly object padlock = new object();
	}
}
