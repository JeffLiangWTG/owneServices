using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"IUP",
	"Incident Upgrade Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.IncidentManager.IncidentBasedAutoDeploymentServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "10Minutes",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "6hours",
	ActiveByDefault = true)
]

namespace Enterprise.Client.EDI.IncidentManager
{
	[NeedsDataRefresh]
	public class IncidentBasedAutoDeploymentServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				ServiceLogger.Log(LogType.Information, "Processing incidents...");

				var incidents = GetIncidentsToSendUpgrade();
				UpgradeToClients(incidents, token, isWeekly: false);

				var delayedIncidents = GetDelayedIncidentsToSendWeeklyUpgradeIfReady();
				UpgradeToClients(delayedIncidents, token, isWeekly: true);

				int incidentsProcessed = Reporter.ProcessedIncidentCount;
				if (incidentsProcessed > 0)
				{
					// Save any log entries added to related entries, e.g., organization logs.
					// Adding to log should be immune to concurrency errors so no need for catch and retry handling.
					Factory.Save();
				}

				ServiceLogger.Log(LogType.Information, "Incidents processed: " + incidentsProcessed);

				var error = Reporter.SendReport();
				if (!string.IsNullOrEmpty(error))
				{
					ServiceLogger.Log(LogType.Error, error);
				}
			}
		}

		void UpgradeToClients(List<SupportIncident> incidents, CancellationToken token, bool isWeekly)
		{
			if (incidents.Count > 0)
			{
				var processedDatabasePKs = new List<ZGuid>();
				var reporterBuilds = new Dictionary<ZString, ReleaseBuild>();
				foreach (var buildInfo in Builds.GetAvailableBuildProductAndReleaseRings(isWeekly))
				{
					token.ThrowIfCancellationRequested();

					var buildProduct = buildInfo.Item1;
					var buildReleaseRing = buildInfo.Item2;
					var build = Builds.GetLatestAvailableBuild(buildProduct, buildReleaseRing, isWeekly);

					if (build == null)
					{
						break;
					}

					var incidentsSortedByClient = SortIncidentsByClient(incidents, buildReleaseRing, isWeekly);
					foreach (ZGuid clientPK in incidentsSortedByClient.Keys)
					{
						token.ThrowIfCancellationRequested();
						// upgrade factory just creates new records and should be immune to concurrency errors
						// It is saved inside the call below to: upgrader.PlaceUpgradesToClients()
						var upgradeFactory = new BusinessObjectFactory() { RefreshEnabled = false };

						var incidentDetails = new List<string>();
						var upgradeRequests = new UpgradeRequestCollection(upgradeFactory);

						foreach (var incident in incidentsSortedByClient[clientPK])
						{
							token.ThrowIfCancellationRequested();
							incidentDetails.Add(incident.IM_IncidentNumber + " - " + incident.IM_Description);

							var databases = GetAllHostedLicenceDatabases(incident);
							foreach (var database in databases)
							{
								if (!processedDatabasePKs.Contains(database.PK))
								{
									processedDatabasePKs.Add(database.PK);
									var upgradeRequest = new UpgradeRequest(upgradeFactory, (EDIOrgHeader)incident.Client, database);
									OrgContact contact;
									if (upgradeRequest.ClientContactPK.IsEmpty && (contact = incident.Contact) != null)
									{
										upgradeRequest.ClientContactPK = contact.PK;
										upgradeRequest.ClientContactEmailAddress = contact.OC_Email;
									}

									if (upgradeRequest.ClientContactEmailAddress.IsEmpty)
									{
										LogAndReport(LogType.Error, "Incident " + incident.IM_IncidentNumber + " was processed but the client was not notified because there was no contact email address.");
									}

									upgradeRequests.Add(upgradeRequest);
								}
							}
						}

						if (upgradeRequests.Count > 0)
						{
							UpgradeRequestCollectionContainer upgrader = GetNewUpgrader(upgradeRequests);
							upgrader.IsSendViaDefault = true;
							upgrader.SendEmailNotificationAutomatically = true;
							upgrader.ReleaseBuildPK = build.PK;
							upgrader.AdditionalNotification = GetAdditionalNotification(incidentDetails, upgrader);
							upgrader.Incidents = incidentsSortedByClient[clientPK];

							// upgradeFactory is saved here
							upgrader.PlaceUpgradesToClients();

							// Save incidents separately from upgrades and afterwards.
							// If the save fails the upgrade may be sent again next RunTask,
							// but it is better to send upgrades twice rather than not at all.
							foreach (var incident in incidentsSortedByClient[clientPK])
							{
								token.ThrowIfCancellationRequested();
								ProcessIncidentInAnotherFactory(incident, build);
							}

							reporterBuilds[buildReleaseRing] = build;
						}
					}
				}
				Reporter.AddBuilds(reporterBuilds);
			}
		}

		internal ReleaseBuild GetReleaseBuildToDeployFor(LicenceDatabase database, bool isWeekly)
		{
			ReleaseBuild build = null;
			var product = database.LD_Product;
			var releaseRing = database.LD_ReleaseRing;

			if (database.CurrentVersion != null)
			{
				// get best fit
				var installedVersion = database.CurrentVersion.VersionNumber;
				build = Builds.GetLatestAvailableBuildForInstalledVersion(product, releaseRing, installedVersion, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: isWeekly);
			}
			else
			{
				// current version not specified, get licensed.
				build = Builds.GetLatestAvailableBuild(product, releaseRing, isWeekly);
			}

			return build;
		}

		public List<SupportIncident> GetIncidentsToSendUpgrade()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			filter.AddToFilter(IncidentMainSchema.IM_Category, SQLComparisonOperator.NotEqual, SupportIncidentCategoriesList.Codes.Support);
			filter.AddToFilter(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			var incidents = LoadIncidents(filter);

			return GetIncidentsToSend(incidents);
		}

		public List<SupportIncident> GetDelayedIncidentsToSendWeeklyUpgradeIfReady()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(IncidentMainSchema.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed);
			filter.AddToFilter(IncidentMainSchema.IM_Category, SQLComparisonOperator.NotEqual, SupportIncidentCategoriesList.Codes.Support);
			filter.AddToFilter(IncidentMainSchema.IM_IncidentType, IncidentConstants.IncidentType.SupportIncident);
			var incidents = LoadIncidents(filter);

			return GetIncidentsToSend(incidents, true);
		}

		List<SupportIncident> GetIncidentsToSend(SupportIncident[] incidents, bool isWeekly = false)
		{
			List<SupportIncident> result = new List<SupportIncident>();
			incidents = incidents.Where(x => !x.ShouldStayIndependent).ToArray();
			foreach (var databaseIncidents in incidents
							.Where(x => x.Client?.OH_IsActive ?? true) // ignore inactive clients
							.GroupBy(x => x.Database))
			{
				var database = databaseIncidents.Key;
				var build = database != null ? GetReleaseBuildToDeployFor(database, isWeekly) : null;

				var completedIncidentsForDatabase = new List<SupportIncident>();

				foreach (var incident in databaseIncidents)
				{
					if (!incident.NeedUpgrade)
					{
						ClosedWithNoUpgrade(incident);
					}
					else if (database == null || incident.Client == null)
					{
						LogAndReport(LogType.Warning, "Incident " + incident.IM_IncidentNumber + " does not have a database or client.");
					}
					else if (database.LD_AvailableUpgradeMethod == UpgradeMethods.Codes.Blocked)
					{
						Reporter.AddUnprocessedIncident(incident, "Client database has an upgrade method of Blocked.");
					}
					else if (build == null)
					{
						Reporter.AddUnprocessedIncident(incident, "ReleaseBuild not available");
						if (Builds.isNeedSkipDeploy)
						{
							ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "CW1 systems cannot get CWN release build for incident {0}.", incident.IM_IncidentNumber));
						}
						else
						{
							var errorMessage = string.Format(CultureInfo.InvariantCulture, "Build Not Found for client {0} in incident# {1}.", incident.Client.OH_Code, incident.IM_IncidentNumber);
							ServiceLogger.Log(LogType.Warning, errorMessage);
						}
					}
					else if (AreAllRelatedWorkItemsComplete(incident, build))
					{
						completedIncidentsForDatabase.Add(incident);
					}
				}

				if (completedIncidentsForDatabase.Count > 0)
				{
					var incidentsToSend = ProcessIncidentsForSending(database, completedIncidentsForDatabase, build);
					result.AddRange(incidentsToSend);
				}
			}
			return result;
		}

		public List<LicenceDatabase> GetAllHostedLicenceDatabases(SupportIncident incident)
		{
			var result = new List<LicenceDatabase>();
			if (incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR1_SystemDown ||
				incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR2_ModuleDown ||
				incident.IM_Priority == Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround)
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));

				if (incident.IM_Product == ProductTypes.Codes.Enterprise)
				{
					query.AddToFilter(LicenceDatabaseSchema.LD_Product, incident.Database.LD_Product);  //Load only CW1 or CWN based on incident database product
				}
				else
				{
					query.AddToFilter(LicenceDatabaseSchema.LD_Product, incident.IM_Product);
				}

				query.AddToFilter(LicenceDatabaseSchema.LD_ReleaseRing, incident.ClientReleaseRing);
				query.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Test);
				query.AddToFilter(LicenceDatabaseSchema.LD_IsActive, true);
				query.AddToFilter(LicenceDatabaseSchema.LD_LE, incident.EnterprisePK);

				var activeCodeDescriptionPairList = EDIDataRegistry.Instance.DatabaseHostedLocations.Value.GetActiveCodeDescriptionPairList();
				query.AddToFilter(LicenceDatabaseSchema.LD_HostedLocation, activeCodeDescriptionPairList.GetAllCodes());

				var databases = Factory.Load<LicenceDatabase>(query);
				result.AddRange(databases);
			}
			if (!result.Contains(incident.Database))
			{
				result.Add(incident.Database);
			}
			return result;
		}

		void ClosedWithNoUpgrade(SupportIncident incident)
		{
			Reporter.AddProcessedIncident(incident, "Upgrade not required");

			for (int retry = 0; retry < 3; ++retry)
			{
				try
				{
					var anotherFactory = new BusinessObjectFactory();
					var incidentReloaded = anotherFactory.Load<SupportIncident>(incident.PK);
					incidentReloaded.ClosedWithNoUpgrade();
					anotherFactory.Save();
					break;
				}
				catch (ZSaveConcurrencyException)
				{
					System.Threading.Thread.Sleep(100);
				}
				catch (ZSaveException)
				{
					System.Threading.Thread.Sleep(500);
				}
			}
		}

		protected virtual SupportIncident[] LoadIncidents(ZQuery filter)
		{
			return Factory.Load<SupportIncident>(filter);
		}

		bool AreAllRelatedWorkItemsComplete(SupportIncident incident, ReleaseBuild build)
		{
			if (incident.RelatedWorkItems.OfType<NewWorkItem>()
							.Any(x => x.WorkflowItems.OfType<WorkItemProcessTask>()
								.Any(y => y.IsCheckInTaskEvenWhenAssignedToDAT)))
			{
				return AreAllRelatedWorkItemsCheckedIn(incident, build);
			}
			else
			{
				ReportNoCheckInWorkItems(incident);
				return false;
			}
		}

		void ReportNoCheckInWorkItems(SupportIncident incident)
		{
			if (incident.RelatedWorkItems.Count > 0)
			{
				Reporter.AddUnprocessedIncident(incident, "No RelatedWorkItem with check-in task");
			}
			else
			{
				Reporter.AddUnprocessedIncident(incident, "No RelatedWorkItem");
			}
		}

		bool AreAllRelatedWorkItemsCheckedIn(SupportIncident incident, ReleaseBuild build)
		{
			string releaseRing = build.HL_ReleaseStatus;

			foreach (NewWorkItem workItem in incident.RelatedWorkItems)
			{
				if (!workItem.HasOpenedShelfCheckInTasksForReleaseRing(releaseRing))
				{
					var releaseBuildContent = ReleaseBuildContent.New(Factory);

					if (workItem.HasClosedShelfCheckInTasksForReleaseRing(releaseRing))
					{
						if (!releaseBuildContent.IsPatchedTo(workItem, build) && releaseBuildContent.IsCargoWiseOneChange(workItem))
						{
							Reporter.AddUnprocessedIncident(incident, "RelatedWorkItem's changes have NOT been patched to current ReleaseBuild");
							return false;
						}
					}
					else
					{
						if (workItem.HasShelfCheckInTasks && !releaseBuildContent.IsPatchedTo(workItem, build) && releaseBuildContent.IsCargoWiseOneChange(workItem))
						{
							Reporter.AddUnprocessedIncident(incident, "RelatedWorkItem's changes are not in latest ReleaseBuild yet");
							return false;
						}
					}
				}
				else
				{
					Reporter.AddUnprocessedIncident(incident, "RelatedWorkItem has Open Shelf Check In task for client's release ring");
					return false;
				}
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<ZGuid, List<SupportIncident>> SortIncidentsByClient(List<SupportIncident> incidents, string releaseRing, bool isWeekly = false)
		{
			Dictionary<ZGuid, List<SupportIncident>> result = new Dictionary<ZGuid, List<SupportIncident>>();
			for (int i = incidents.Count - 1; i >= 0; i--)
			{
				SupportIncident incident = incidents[i];
				ReleaseBuild build = GetReleaseBuildToDeployFor(incident.Database, isWeekly);

				if (build != null && build.HL_ReleaseStatus == releaseRing)
				{
					incidents.Remove(incident);
					ZGuid clientPK = incident.Client.PK;
					List<SupportIncident> clientIncidents;
					if (!result.TryGetValue(clientPK, out clientIncidents))
					{
						result[clientPK] = clientIncidents = new List<SupportIncident>();
					}
					clientIncidents.Add(incident);
				}
			}
			return result;
		}

		public string GetAdditionalNotification(List<string> incidentDetails, UpgradeRequestCollectionContainer upgrader)
		{
			StringBuilder result = new StringBuilder("The upgrade resolves the following incidents:");
			result.AppendLine();
			result.AppendLine();
			incidentDetails.Sort();
			foreach (string incidentDetailsLine in incidentDetails)
			{
				result.AppendLine(incidentDetailsLine);
			}
			if (!upgrader.AdditionalNotification.IsEmpty)
			{
				result.AppendLine();
				result.Append(upgrader.AdditionalNotification);
			}
			return result.ToString().TrimEnd();
		}

		static string GetReleaseBuildDescription(ReleaseBuild build)
		{
			Argument.NotNull(build, nameof(build));

			if (build.HL_Product.EqualsIgnoringCase(ProductTypes.Codes.CargoWiseNext))	//Only CWN needs this logic for release ring fallback to hide release ring infomation 
			{
				return $"{build.ProductDescription} {build.ExeVersion}, {build.VersionNumberDisplayText}, Exe Date: {build.HL_ExeVersionDate.ToLongTimeString()}.";
			}
			else
			{
				return $"{build.ProductDescription} {build.ExeVersion}, {build.ReleaseDisplayText}, Exe Date: {build.HL_ExeVersionDate.ToLongTimeString()}.";
			}
		}

		public void ProcessIncidentInAnotherFactory(SupportIncident incident, ReleaseBuild selectedBuild)
		{
			var conversationMessage = @$"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: {GetReleaseBuildDescription(selectedBuild)}";
			ProcessIncidentInAnotherFactory(incident, "Upgrade sent", conversationMessage);
		}

		public void ProcessIncidentInAnotherFactory(SupportIncident incidentInMainFactory, string reason, string eConversationMessage, bool awaitingDeployment = false)
		{
			for (int retry = 0; retry < 3; ++retry)
			{
				try
				{
					ProcessIncidentInAnotherFactoryWithoutRetry(incidentInMainFactory, eConversationMessage, awaitingDeployment);
					Reporter.AddProcessedIncident(incidentInMainFactory, reason);
					break;
				}
				catch (ZSaveConcurrencyException)
				{
					System.Threading.Thread.Sleep(100);
				}
				catch (ZSaveException)
				{
					System.Threading.Thread.Sleep(500);
				}
			}
		}

		protected virtual void ProcessIncidentInAnotherFactoryWithoutRetry(SupportIncident incidentInMainFactory, string eConversationMessage, bool awaitingDeployment)
		{
			var anotherFactory = new BusinessObjectFactory();
			var incidentReloaded = anotherFactory.Load<SupportIncident>(incidentInMainFactory.PK);
			bool needsSave = false;

			if (!awaitingDeployment)
			{
				needsSave = true;
				CloseAsUpgradeDeliveredAndAddMessageToClient(incidentReloaded, eConversationMessage);
			}
			else if (incidentReloaded.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade)
			{
				needsSave = true;
				CloseAsDelayUpgrade(incidentReloaded, eConversationMessage);
			}

			if (needsSave)
			{
				anotherFactory.Save();
			}
		}

		void ProcessIncidentsNotToBeSent(
			IEnumerable<SupportIncident> incidents,
			DatabaseVersionCompatibleReason compatibleReason,
			DatabaseVersion dbVersion,
			ReleaseBuild buildToSend,
			ReleaseBuild clientBuild)
		{
			if (compatibleReason == DatabaseVersionCompatibleReason.NewerHigherRing)
			{
				string status;
				switch (dbVersion)
				{
					case DatabaseVersion.Current:
						status = "is already on";
						break;
					case DatabaseVersion.Sent:
						status = "has already been sent";
						break;
					default:
						throw new ArgumentException("invalid", nameof(dbVersion));
				}

				var reason = "Client " + status + " a higher release from a different ring";
				foreach (var incident in incidents)
				{
					Reporter.AddUnprocessedIncident(incident, reason);
				}
			}
			if (compatibleReason == DatabaseVersionCompatibleReason.Delayed)
			{
				string internalLog = @$"Upgrade will run on schedule.
Resolved in Version:{GetReleaseBuildDescription(buildToSend)}";
				string report = "Upgrade has been delayed and will run on schedule";
				string messageToCustomer = "The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.";
				foreach (var incident in incidents)
				{
					ServiceLogger.Log(LogType.Information, internalLog);
					ProcessIncidentInAnotherFactory(incident, report, messageToCustomer, true);
				}
			}
			else
			{
				var isClientNewer = compatibleReason != DatabaseVersionCompatibleReason.Equal;
				var newerTag = isClientNewer ? " newer " : " ";
				var internalVersion = isClientNewer ? "a newer version" : "the same version";

				string internalReportReason;
				string messageToCustomerReason;
				string clientBuildDescriptionHeader;

				switch (dbVersion)
				{
					case DatabaseVersion.Current:
						internalReportReason = $"is already on {internalVersion}";
						clientBuildDescriptionHeader = "Currently installed version";
						messageToCustomerReason = $"No upgrade sent. Your system is already running a{newerTag}version with changes that address this eRequest.";
						break;
					case DatabaseVersion.Sent:
						internalReportReason = $"has already been sent {internalVersion}";
						clientBuildDescriptionHeader = "Previously sent version";
						messageToCustomerReason = $"No upgrade sent. A{newerTag}version has already been sent to your system with changes that address this eRequest.";
						break;
					default:
						throw new ArgumentException("invalid", nameof(dbVersion));
				}

				var report = $"Upgrade not sent as the client {internalReportReason}";
				var messageToCustomer = @$"{messageToCustomerReason}";
				if (clientBuild != null)
				{
					messageToCustomer += $"{System.Environment.NewLine}{clientBuildDescriptionHeader}: {GetReleaseBuildDescription(clientBuild)}";
				}

				foreach (var incident in incidents)
				{
					ProcessIncidentInAnotherFactory(incident, report, messageToCustomer);
				}
			}
		}

		public void CloseAsUpgradeDeliveredAndAddMessageToClient(SupportIncident incident, string eConversationMessage)
		{
			if (!string.IsNullOrEmpty(eConversationMessage))
			{
				incident.AddPublicSystemLogMessage(eConversationMessage);
			}
			incident.SuspendPopulatingWorkflowTemplate();
			incident.SetUpgradeDelivered();
			incident.IM_BugFixDeployed = ZDateTime.Now;

			incident.SetQuoteDeliveredTimeIfEmpty();
		}

		void LogAndReport(LogType type, string message)
		{
			ServiceLogger.Log(type, message);
			Reporter.AddError(message);
		}

		void CloseAsDelayUpgrade(SupportIncident incident, string eConversationMessage)
		{
			if (!string.IsNullOrEmpty(eConversationMessage))
			{
				incident.AddPublicSystemLogMessage(eConversationMessage);
			}

			incident.SuspendPopulatingWorkflowTemplate();
			incident.DelayUpgrade();
		}

		UpgradeRequestCollectionContainer GetNewUpgrader(UpgradeRequestCollection upgradeRequests)
		{
			UpgradeRequestCollectionContainer result = new UpgradeRequestCollectionContainer(upgradeRequests.Factory, upgradeRequests);
#if DEBUG
			if (createdUpgraders == null)
			{
				createdUpgraders = new List<UpgradeRequestCollectionContainer>();
			}
			createdUpgraders.Add(result);
#endif
			return result;
		}

		#region Check Should Send Upgrade

		/// <summary>
		/// Given the list of completed incidents for a database,
		/// process those that that are not to be sent an upgrade for whatever reason.
		/// Report the reason, and close the incident if needed.
		/// Return the remaining ones that need the upgrade.
		/// </summary>
		List<SupportIncident> ProcessIncidentsForSending(LicenceDatabase database, IEnumerable<SupportIncident> completedIncidents, ReleaseBuild buildToSend)
		{
			var incidentsToSend = new List<SupportIncident>();
			var compatibleReason = CompareDatabaseVersion(buildToSend, database.CurrentVersion, onlyCompareOnSameRing: false);
			if (compatibleReason != null)
			{
				ProcessIncidentsNotToBeSent(completedIncidents, compatibleReason.Value, DatabaseVersion.Current, buildToSend, database.CurrentVersion);
			}
			else
			{
				compatibleReason = CompareDatabaseVersion(buildToSend, database.SentVersion, onlyCompareOnSameRing: true);
				if (compatibleReason != null)
				{
					ProcessIncidentsNotToBeSent(completedIncidents, compatibleReason.Value, DatabaseVersion.Sent, buildToSend, database.SentVersion);
				}
				else
				{
					var clientBuild = database.LatestSentOrCurrentVersion;
					var comparedDatabaseVersion = clientBuild == database.SentVersion ? DatabaseVersion.Sent : DatabaseVersion.Current;

					foreach (var incident in completedIncidents)
					{
						if (IsDatabaseVersionCompatibleWithCheckInTasks(incident, clientBuild))
						{
							ProcessIncidentsNotToBeSent([incident], DatabaseVersionCompatibleReason.Equal, comparedDatabaseVersion, buildToSend, clientBuild);
						}
						else
						{
							incidentsToSend.Add(incident);
						}
					}

					if (incidentsToSend.Count > 0)
					{
						var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, database.LD_LE));
						if (LicenceDatabase.IsOnWiseCloud(database.LD_HostedLocation) && !licenceEnterprise.LE_IsInternal)
						{
							var cr4IncidentsToDelayed = incidentsToSend.Where(i => i.IM_Priority == Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround
								&& i.IM_ResolutionCode == SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade).ToArray();
							foreach (var incident in cr4IncidentsToDelayed)
							{
								incidentsToSend.Remove(incident);
							}

							ProcessIncidentsNotToBeSent(cr4IncidentsToDelayed, DatabaseVersionCompatibleReason.Delayed, DatabaseVersion.Sent, buildToSend, database.SentVersion);
						}
					}
				}
			}

			return incidentsToSend;
		}

		public static StmALog[] GetLogsToCheck(LicenceDatabase database, IEnumerable<SupportIncident> incidents, ReleaseBuild build)
		{
			ZDateTime buildExeDate = build.HL_ExeVersionDate;

			ZDateTime initialDateTime = ZDateTime.Now.AddDays(-14);
			if (initialDateTime > buildExeDate)
			{
				initialDateTime = buildExeDate;
			}

			ZQuery mainFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
			mainFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, database.LD_ServerCode + " via");

			ZQuery postedTimeFilter = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, initialDateTime);
			postedTimeFilter.AddToFilter(mainFilter);

			var clientFilter = new ZQuery(StmALogSchema.SL_Table, SQLComparisonOperator.Equal, OrgHeaderSchema.Constants.TableName);
			clientFilter.AddToFilter(StmALogSchema.SL_Parent, SQLComparisonOperator.Equal, incidents.Select(x => x.Client.PK));
			postedTimeFilter.AddToFilter(clientFilter);

			var result = database.Factory.Load<StmALog>(postedTimeFilter);

			if (result.Length == 0)
			{
				var top1Filter = new ZQuery(mainFilter)
				{
					MaximumRows = 1,
					OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " DESC"
				};
				top1Filter.AddToFilter(clientFilter);
				result = database.Factory.Load<StmALog>(top1Filter);
			}

			return result;
		}

		public static bool TryGetVersionNumberFromLog(StmALog log, out VersionNumber result)
		{
			bool canGetVersion = false;
			result = new VersionNumber();

			int buildDetailsPartIndex = log.SL_Reference.IndexOf(" will be sent to this client");
			if (buildDetailsPartIndex != -1)
			{
				string buildDetailsPart = log.SL_Reference.Substring(0, buildDetailsPartIndex);
				if (buildDetailsPart.Length > 0)
				{
					Regex regex = new Regex(@"\(v[0-9]*\.[0-9]*\.[0-9]*\.[0-9]*\)", RegexOptions.IgnoreCase);

					Match match = regex.Match(buildDetailsPart);
					if (match.Success)
					{
						canGetVersion = VersionNumber.TryParse(match.Value.Substring(2, match.Value.Length - 3), out result);
					}
				}
			}

			return canGetVersion;
		}

		/// <summary>
		/// Reason the version sent/installed on the client database is such that the latest build need not be sent
		/// </summary>
		enum DatabaseVersionCompatibleReason
		{
			Equal,
			Newer,
			NewerHigherRing,
			Delayed
		}

		enum DatabaseVersion
		{
			Current,
			Sent
		}

		static DatabaseVersionCompatibleReason? CompareDatabaseVersion(ReleaseBuild build, ReleaseBuild databaseVersion, bool onlyCompareOnSameRing)
		{
			if (databaseVersion != null)
			{
				bool sameRing = build.HL_ReleaseStatus == databaseVersion.HL_ReleaseStatus;
				return CompareDatabaseVersion(build, databaseVersion.VersionNumber, sameRing, onlyCompareOnSameRing);
			}
			else
			{
				return null;
			}
		}

		static DatabaseVersionCompatibleReason? CompareDatabaseVersion(ReleaseBuild build, VersionNumber versionNumber, bool sameRing, bool onlyCompareOnSameRing)
		{
			DatabaseVersionCompatibleReason? result;

			if (versionNumber == build.VersionNumber)
			{
				result = DatabaseVersionCompatibleReason.Equal;
			}
			else if ((IsOldEnterpriseVersion(versionNumber) && IsOldEnterpriseVersion(build.VersionNumber) && versionNumber.Release > build.HL_Release) || versionNumber > build.VersionNumber)
			{
				if (sameRing)
				{
					result = DatabaseVersionCompatibleReason.Newer;
				}
				else if (!onlyCompareOnSameRing)
				{
					result = DatabaseVersionCompatibleReason.NewerHigherRing;
				}
				else
				{
					result = null;
				}
			}
			else
			{
				result = null;
			}

			return result;
		}

		static bool IsOldEnterpriseVersion(VersionNumber versionNumber)
		{
			return versionNumber.Major < 14;
		}

		/// <summary>
		/// Compare the version number from the check-in task logs (indicating what version has the fix)
		/// with the version on the database.
		/// </summary>
		bool IsDatabaseVersionCompatibleWithCheckInTasks(SupportIncident incident, ReleaseBuild databaseVersion)
		{
			if (databaseVersion == null)
			{
				return false;
			}

			bool hasCheckIns = false;
			bool isPromoted = true;

			foreach (NewWorkItem workItem in incident.RelatedWorkItems)
			{
				if (workItem.HasShelfCheckInTasks)
				{
					hasCheckIns = true;
					if (!ReleaseBuildContent.New(Factory).IsPatchedTo(workItem, databaseVersion))
					{
						isPromoted = false;
						break;
					}
				}
			}

			return hasCheckIns && isPromoted;
		}

		#endregion

		#region Properties

		public BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}

		public IncidentBatchProcessReporter Reporter
		{
			get
			{
				if (reporter == null)
				{
					reporter = new IncidentBatchProcessReporter();
				}
				return reporter;
			}
		}

		public LatestReleaseBuildsDictionary Builds
		{
			get
			{
				if (builds == null)
				{
					builds = new LatestReleaseBuildsDictionary(Factory);
					builds.Load();
				}
				return builds;
			}
		}

		BusinessObjectFactory factory;
		IncidentBatchProcessReporter reporter;
		LatestReleaseBuildsDictionary builds;

		#endregion

		#region Test
#if DEBUG
		public List<UpgradeRequestCollectionContainer> createdUpgraders;
#endif
		#endregion

		public bool isInternalMessage { get; set; }
	}
}
