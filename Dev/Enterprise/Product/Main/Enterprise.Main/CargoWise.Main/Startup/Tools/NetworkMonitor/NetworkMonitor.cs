using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup.Tools
{
	class NetworkMonitor : IDisposable
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		internal NetworkMonitor()
		{
			Username = GlbStaff.CurrentUser?.GS_LoginName;
			pingTimer = new System.Timers.Timer(RemoteNetworkMonitorPingIntervalInMilliseconds);
			pingTimer.Elapsed += (sender, args) =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						CheckNetworkRoundLoopDelay();
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					if (e is IOException)
					{
						Globals.Message.ShowWarning(e.Message);
					}
					else
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "NetworkMonitor elapsed event throw a {0} exception", e.GetType().ToString()), e);
					}
				}
			};
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localization not required for this error message in the current context.")]
		internal const string RetrieveIPErrorMessage = "Retrieving IP address failed";
		public string Username { get; }

		internal IPAddress IPAddress
		{
			get
			{
				if (ipAddress == null)
				{
					ipAddress = IPAddress.Parse("0.0.0.0");
					var productKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (productKey != null)
					{
						try
						{
							ipAddress = IPAddress.Parse(ObjectFactory.Get<IWiseCloudSecurityClient>().GetClientIPAddress(productKey.EnterpriseCode + productKey.ServerCode, Username));
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ErrorReporter.ReportOnce(RetrieveIPErrorMessage, ex.ToString());
						}
					}
				}
				return ipAddress;
			}
		}
		IPAddress ipAddress;

		public double AverageResponseTimeThreshold { get; }
		public DateTime StartTime { get; set; }

		public bool IsRunning
		{
			get
			{
				return pingTimer.Enabled;
			}
		}

		protected virtual double RemoteNetworkMonitorPingIntervalInMilliseconds => 10000;

		public event EventHandler PingResponseEvent;
		protected System.Timers.Timer pingTimer;
		readonly List<TCPPingResponse> tcpPingResponsesList = new List<TCPPingResponse>();
		public List<TCPPingResponse> TcpPingResponsesList
		{
			get { return tcpPingResponsesList; }
		}

		void CheckNetworkRoundLoopDelay()
		{
			lock (duringRoundLoopLock)
			{
				if (timerStatus != TimerStatusOption.Idle)
				{
					return;
				}
				timerStatus = TimerStatusOption.Running;
			}
			var tcpPingResponse = GetResponse();
			if (tcpPingResponse != null)
			{
				lock (duringRoundLoopLock)
				{
					if (timerStatus == TimerStatusOption.Running)
					{
						tcpPingResponsesList.Add(tcpPingResponse);
						PingResponseEvent?.Invoke(this, new EventArgs());
						timerStatus = TimerStatusOption.Idle;
					}
				}
			}
		}
		readonly object duringRoundLoopLock = new object();
		TimerStatusOption timerStatus;

		protected virtual TCPPingResponse GetResponse()
		{
			var tcpPingResponse = new TCPPingResponse(StartTime);
			for (int i = 0; i < tcpPingResponse.RepeatTimes; i++)
			{
				if (timerStatus != TimerStatusOption.Running)
				{
					return null;
				}
				var stopwatch = new Stopwatch();

				stopwatch.Start();
				EnterpriseChannel.Instance.SendMessage<bool>(EnterpriseChannelMessageTypes.CheckNetworkRoundLoopDelay, Array.Empty<byte>());
				stopwatch.Stop();

				var t = stopwatch.Elapsed.TotalMilliseconds;
				tcpPingResponse.ResponseTimesList.Add(t);

				Thread.Sleep(1000);
			}
			return tcpPingResponse;
		}

		public void Start()
		{
			StartTime = DateTime.UtcNow;
			timerStatus = TimerStatusOption.Idle;
			pingTimer.Start();
		}

		enum TimerStatusOption
		{
			Idle,
			Running,
			Stopped
		}

		public void Stop()
		{
			lock (duringRoundLoopLock)
			{
				pingTimer.Stop();
				tcpPingResponsesList.Clear();
				timerStatus = TimerStatusOption.Stopped;
			}
		}

		#region IDisposable Support
		bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					PingResponseEvent = null;
					pingTimer.Stop();
					pingTimer.Dispose();
				}
				disposedValue = true;
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}

	public class TCPPingResponse
	{
		public TCPPingResponse(DateTime startTime)
		{
			UtcDateTime = DateTime.UtcNow;
			this.startTime = startTime;
		}

		public double ElapsedSeconds
		{
			get { return (UtcDateTime - startTime).TotalSeconds; }
		}

		public string Tooltip
		{
			get { return string.Format(CultureInfo.CurrentCulture, (NoResString)"{0:dd/MM/yy H:mm:ss}\r\n{1} ms", UtcDateTime, Average); }
		}

		public double Min
		{
			get { return responseTimesList.Min(); }
		}

		public double Max
		{
			get { return responseTimesList.Max(); }
		}

		public double Average
		{
			get { return responseTimesList.Average(); }
		}

		public int RepeatTimes
		{
			get { return repeatTimes; }
		}

		public List<double> ResponseTimesList
		{
			get { return responseTimesList; }
		}

		public DateTime UtcDateTime { get; }

		readonly List<double> responseTimesList = new List<double>(repeatTimes);
		const int repeatTimes = 4;
		readonly DateTime startTime;
	}
}
