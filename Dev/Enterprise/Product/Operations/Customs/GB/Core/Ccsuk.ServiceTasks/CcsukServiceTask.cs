using System;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.Connection;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers.CcsukEmailSender;

[assembly: HostedService(Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukServiceTask.Code,
	"UK CCSUK sender/receiver",
	"GBC",
	typeof(Enterprise.Customs.GB.Ccsuk.ServiceTasks.CcsukServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom,
	MinimumPeriod = "10Minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "10minutes"
	)]

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks
{
	/// <summary>
	/// Spools waiting EdiMessages into interchanges, then spool them as emails; then pulls relevant inbound emails out into interchange/messgae pairs; then processes said messages and update entries, etc. 
	/// Should have one connection per enterprise, not per company. Agnostic of branch/company as registry options are retrieved at enterprise level.
	/// </summary>
	public class CcsukServiceTask : CustomsServiceTask
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override void RunTaskCore(CancellationToken token)
		{
			if (IsCcsukConnectionLive)
			{
				ServiceLogger.Log(Integration.LogType.Information, "~#~#~#~#~#~#~# TASK STARTING UP #~#~#~#~#~#~#~");
				var randomUkBranch = GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.UnitedKingdom);
				if (randomUkBranch == null)
				{
					ServiceLogger.Log(Integration.LogType.Warning, "Task cannot run, there is no active GB branch. Task is deactivated");
					CcsukSession.DeactivateCUKServiceTask();
					return;
				}

				using (DisposableEnvironment.ForBranch(randomUkBranch.PK.ToGuid()))
				{
					CcsukSession session;
					try
					{
						session = CcsukSession.GetSession(ServiceLogger);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var message = string.Format(@"Could not connect to or could not log on to CCSUK network. The most likely reasons for this are:
a) Incorrect configuration in the {0} registry, in particular IP address details.  This is particularly likely for a brand-new installation of CCSUK for a given {0} instance. 
b) Incorrect networking rules (e.g. network address translation) outside of {0}.  This is unlikely other than for a brand-new installation of CCSUK for a given {0} instance. 
c) Outage of CCSUK host. 
d) (If using IPConnect) Outage of a VPN or other network link between the {0} process controller and the CCSUK IPConnect endpoint (CCSUK router).  In the case of WiseCloud-hosted service, this may include a VPN between WiseCloud data centres. 
e) (If using IPConnect) Outage of the CCSUK IPConnect service, including the CCSUK router. 
f) (If using CCSUK VPN) Outage of the CCSUK VPN between the {0} process controller and the CCSUK host. 
g) Unavailability of the correct {0} process controller(s), such that the only available process controllers do not have access to the local IP addresses detailed in the {0} registry. The correct process controllers, and only those, must be allocated. 
h) IP address conflict. More than one process controller server has been allocated the IP address that CW1 is trying to use, and so this address is not always available or not always usable.  Typical symptom is intermittent connectivity errors not affecting all hosted clients.

Please raise a new eRequest, and be sure to include not only this message but any text which follows in this service task log entry AND all the previous and subsequent log entries (for all available process controllers). It will not be possible to diagnose connectivity problems without complete log details over all servers.",
							BrandingFactory.Instance.ProductName);
						CcsukSession.SendWarningEmailAboutCannotLogon(message, ex, ToWhom.ItDepartmentAndCustomsGroup);
						throw new HostedServiceException(message, ex);
					}

					try
					{
						while (true)
						{
							token.ThrowIfCancellationRequested();
							session.SendAllWaitingInterchanges();
							int spoolPeriod = GBCustomsDataRegistry.Instance.CcsukSpoolPeriodInSeconds.Value > 0 ? GBCustomsDataRegistry.Instance.CcsukSpoolPeriodInSeconds.Value : 1;
							token.WaitHandle.WaitOne(spoolPeriod * 1000);
							session.PingIfNeeded();
							if (!session.IsConnected)
							{
								break;
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ServiceLogger.Log(Integration.LogType.Error, string.Format("Top-level exception handler on main thread. Clean shutdown will be attempted but may not succeed. " +
							"The most likely explanation for this is that a shutdown signal was issued by the operating system, such as stop command for service, " +
							"which may have been issued as part of a {0} upgrade. Further detail: {1}", Core.Constants.ProductName, ex.Message));
						throw;
					}
					finally
					{
						ServiceLogger.Log(Integration.LogType.Information, "~#~#~#~#~#~#~# TASK SHUTTING DOWN #~#~#~#~#~#~#~");
						session.ShutdownAndDisposeInstance();
						ServiceLogger.Log(Integration.LogType.Information, "~#~#~#~#~#~#~# TASK ENDS #~#~#~#~#~#~#~");
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		bool IsCcsukConnectionLive
		{
			get
			{
				bool isLive = false;
				var goLiveDate = GBCustomsDataRegistry.Instance.CcsukGoLiveDate.Value;

				if (goLiveDate == ZDateTime.MinSmallDateTimeValue)
				{
					isLive = true;
				}
				else if (goLiveDate <= DateTime.UtcNow)
				{
					isLive = true;
				}
				else if (goLiveDate > DateTime.UtcNow)
				{
					ServiceLogger.Log(Integration.LogType.Warning, $"CCSUK go live date is not yet reached, this task will not run.  It will run at {goLiveDate}.");
				}
				return isLive;
			}
		}

		public const string Code = ApplicationCodeList.Codes.GbCcsuk;
	}
}
