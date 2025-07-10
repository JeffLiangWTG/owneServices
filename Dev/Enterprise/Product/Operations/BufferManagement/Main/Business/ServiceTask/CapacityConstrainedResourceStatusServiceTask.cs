using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CapacityConstrainedResourceStatusServiceTask.Code,
	CapacityConstrainedResourceStatusServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(CapacityConstrainedResourceStatusServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class CapacityConstrainedResourceStatusServiceTask : BMSServiceTaskBase
	{
		public const string Code = "BMC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Capacity Constrained Resource Status";

		protected override string TaskDescription
		{
			get { return Description; }
		}

		[HostedServiceRequirement]
		public static string CheckCapacityCalculationsNotDisabled()
		{
			return CheckCapacityCalculationsNotDisabledCore();
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}

		protected override bool ShouldNotRunIfCapacityCalculationsDisabled => true;

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled;

		protected override void RunTaskCore(CancellationToken token)
		{
			var factory = GetNewFactory();

			var staffQuery = new ZQuery(GlbStaffSchema.GS_IsActive, true);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);

			var resources = factory.Load<GlbStaff>(staffQuery);

			var bufferQuery = new ZDBOnlyQuery(typeof(BMComponent));
			bufferQuery.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);
			bufferQuery.AddToFilter(BMComponentSchema.FC_FC_ParentComponent, null);
			bufferQuery.AddToFilter(BMComponentSchema.FC_IsActive, true);
			bufferQuery.OrderBy = FormattableString.Invariant($"{BMComponentSchema.Constants.FC_GB_AgingBranch}, {BMComponentSchema.Constants.FC_Name}");

			var subComponentSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.FC_FC_ParentComponent);
			subComponentSubQuery.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Constraint);

			bufferQuery.AddSubQuery(subComponentSubQuery, JoinCondition.And);

			var buffers = factory.Load<BMComponent>(bufferQuery);
			var buffersByBranch = buffers.GroupBy(x => x.FC_GB_AgingBranch);

			foreach (var buffersForBranch in buffersByBranch)
			{
				using (DisposableEnvironment.ForBranch(buffersForBranch.Key.ToGuid()))
				{
					foreach (var buffer in buffersForBranch)
					{
						token.ThrowIfCancellationRequested();
						var ccrDetections = new List<ConstrainedResourceDetection>();
						var nonCCRDetections = new List<ConstrainedResourceDetection>();

						int ccrThresholdMinutes = buffer.CCRThresholdMinutes;

						var capacityBreakdownInFeedingComponents = GetCapacityByResource(buffer, resources);

						foreach (var resource in resources)
						{
							var resourceComponentLink = buffer.ResourceLinks.FirstOrDefault(l => l.FD_GS_NKResource == resource.GS_Code);

							if (!capacityBreakdownInFeedingComponents.TryGetValue(resource.GS_Code, out var capacity))
							{
								capacity = ResourceCapacity.CreateForResource(resource.GS_Code, true);
							}

							var standardEstimateMinutes = capacity.NonZoneWeightedUtilisedCapacity * 60; // This is for utilised capacity in a bucket, so buffer zones are not relevant, hence using non-zone weighted capacity.

							if (buffer.IsOverCapacityConstrainedThreshold(standardEstimateMinutes))
							{
								if (resourceComponentLink == null)
								{
									resourceComponentLink = buffer.ResourceLinks.AddNew();
									resourceComponentLink.FD_GS_NKResource = resource.GS_Code;
									resourceComponentLink.FD_CapacityLimitPercent = 100;
								}

								if (resourceComponentLink.FD_CapacityConstraintDetectedUtc.IsEmpty)
								{
									ccrDetections.Add(new ConstrainedResourceDetection(resource, standardEstimateMinutes));

									resourceComponentLink.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow;
								}
							}
							else
							{
								if (resourceComponentLink != null)
								{
									if (BMSRegistry.Instance.AllowAutomaticUnmarkingOfCCRs.Value)
									{
										resourceComponentLink.FD_IsCapacityConstrained = false;
									}
									if (!resourceComponentLink.FD_CapacityConstraintDetectedUtc.IsEmpty)
									{
										nonCCRDetections.Add(new ConstrainedResourceDetection(resource, standardEstimateMinutes));
									}
									resourceComponentLink.FD_CapacityConstraintDetectedUtc = ZDateTime.Empty;
								}
							}
						}

						if (ccrDetections.Count > 0)
						{
							ServiceLogger.Log(LogType.Information, GetNonTranslatedConstraintLogMessage(ccrDetections, true, buffer, ccrThresholdMinutes));
						}

						if (nonCCRDetections.Count > 0)
						{
							ServiceLogger.Log(LogType.Information, GetNonTranslatedConstraintLogMessage(nonCCRDetections, false, buffer, ccrThresholdMinutes));
						}

						DetectPersistentlyOverloadedResources(factory, buffer);

						factory.Save();
					}
				}
			}
		}

		protected virtual BusinessObjectFactory GetNewFactory()
		{
			return new BusinessObjectFactory { NameForDebugging = "CapacityConstrainedResourceStatusServiceTask" };
		}

		static Dictionary<string, IResourceCapacity> GetCapacityByResource(BMComponent buffer, GlbStaff[] resources)
		{
			var accumulatedDictionary = new Dictionary<string, IResourceCapacity>();
			var feedingComponents = buffer.GetFeedingComponents().ToArray();

			foreach (var component in feedingComponents)
			{
				var capacityBreakdowns = CapacityCalculator.GetUtilisedCapacityBreakdown(resources, component);

				accumulatedDictionary.Merge(capacityBreakdowns.ToDictionary(p => p.Key.GS_Code.ToString(), p => p.Value),
					keyValuePair => MergeUtilisedCapacityFromMultipleFeedingComponents(accumulatedDictionary, keyValuePair));
			}
			return accumulatedDictionary;
		}

		static void MergeUtilisedCapacityFromMultipleFeedingComponents(Dictionary<string, IResourceCapacity> accumulatedDictionary, KeyValuePair<string, IResourceCapacity> otherComponentCapacity)
		{
			var existingCapacity = accumulatedDictionary[otherComponentCapacity.Key];
			var newCapacity = existingCapacity.Deduct(otherComponentCapacity.Value.NonZoneWeightedUtilisedCapacity);

			accumulatedDictionary[otherComponentCapacity.Key] = newCapacity;
		}

		void DetectPersistentlyOverloadedResources(BusinessObjectFactory factory, BMComponent buffer)
		{
			var persistentlyOverloadedResources = GetPersistentlyOverloadedResources(factory, buffer);
			if (persistentlyOverloadedResources.Length > 0)
			{
				foreach (var resource in persistentlyOverloadedResources)
				{
					factory.AddFetchHint(BMComponentResourceLinkSchema.FD_GS_NKResource, resource.GS_Code);
				}
				foreach (var resource in persistentlyOverloadedResources)
				{
					var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, buffer.PK);
					query.AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, resource.GS_Code);
					var bufferLink = factory.LoadTop1<BMComponentResourceLink>(query);
					bufferLink.FD_IsPersistentlyOverloaded = true;
				}

				var resourceNames = string.Join(System.Environment.NewLine, persistentlyOverloadedResources.Select(s => s.GS_FullName));

				var emailMessage = Res.GetString("47562b70-14b1-468f-be87-ad90d19b0cef", "The following resources have been detected as being persistently overloaded in buffer {0}:{1}{2}",
					/*0*/ buffer.FC_Name,
					/*1*/ System.Environment.NewLine,
					/*2*/ resourceNames);
				EmailBMSNotificationGroupAboutResourceDetectedAsCcr(emailMessage, buffer);

				var logMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)"The following resources have been detected as being persistently overloaded in buffer {0}:{1}{2}", // Service task logging
																																												 /*0*/ buffer.FC_Name,
					/*1*/ System.Environment.NewLine,
					/*2*/ resourceNames);
				ServiceLogger.Log(LogType.Information, logMessage);
			}
		}

		static GlbStaff[] GetPersistentlyOverloadedResources(BusinessObjectFactory factory, BMComponent buffer)
		{
			var persistentlyOverloadedMinutes = buffer.PersistentlyOverloadedMinutes;
			var targetCCRDetectionTime = ZDateTime.UtcNow.AddMinutes(-persistentlyOverloadedMinutes);

			var resourceQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			resourceQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			var componentLinkSubQuery = new ZDBOnlySubQuery(typeof(BMComponentResourceLink), BMComponentResourceLinkSchema.FD_GS_NKResource);

			var query = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, buffer.PK);
			query.AddToFilter(BMComponentResourceLinkSchema.FD_IsCapacityConstrained, false);
			query.AddToFilter(BMComponentResourceLinkSchema.FD_IsPersistentlyOverloaded, false);
			query.AddToFilter(BMComponentResourceLinkSchema.FD_CapacityConstraintDetectedUtc, SQLComparisonOperator.LessThanOrEqualTo, targetCCRDetectionTime);

			componentLinkSubQuery.AddToFilter(query);
			resourceQuery.AddSubQuery(GlbStaffSchema.GS_Code, BMComponentResourceLinkSchema.FD_GS_NKResource, componentLinkSubQuery, JoinCondition.And);

			return factory.Load<GlbStaff>(resourceQuery);
		}

		#region Log Messages

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static string GetNonTranslatedConstraintLogMessage(IEnumerable<ConstrainedResourceDetection> resourceDetections, bool areConstraints, BMComponent buffer, int ccrThresholdMinutes)
		{
			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"The following resources were detected as {0}being capacity constrained for the Release Gate into buffer {1}. The threshold to being a CCR in this buffer is {2} hours. Their total time in the Release Gate is listed next to their names below.", // Service task logging
																																																																														  /*0*/ areConstraints ? string.Empty : (NoResString)"no longer ", // Service task logging
																																																																																											  /*1*/ buffer.FC_Name,
				/*2*/ ccrThresholdMinutes / 60);

			return GetConstraintLogMessage(message, resourceDetections);
		}

		static string GetConstraintLogMessage(string message, IEnumerable<ConstrainedResourceDetection> resourceDetections)
		{
			return message
				+ System.Environment.NewLine
				+ System.Environment.NewLine
				+ string.Join(System.Environment.NewLine, resourceDetections.Select(d => string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}: {1} hours", d.Resource.GS_FullName, d.StandardEstimateMinutes / 60))); // Service task logging
		}

		static void EmailBMSNotificationGroupAboutResourceDetectedAsCcr(string message, BMComponent buffer)
		{
			var subject = Res.GetString("282986cb-05e4-42f8-945d-940ea68a6e08", "Detected Capacity Constrained Resources for {0}", buffer.FC_Name);
			var body = Res.GetString("1fb2a2f9-9b19-4613-9421-518fc455b164", @"{0}

You will need to mark the resources as designated constrained resources if you wish to use Constrained Mode logic for their capacity calculation. To do this, open any Visual Board which includes a channel for the resource, and right-click their channel heading.

Their Release Group will also need to be in Constrained Mode. You can switch a group to Constrained Mode by right-clicking a buffer section name heading on a Visual Board for that Release Group.", message);

			new BMSEmailDef(subject, body).Send();
		}

		#endregion

		class ConstrainedResourceDetection
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			internal ConstrainedResourceDetection(GlbStaff resource, decimal standardEstimateMinutes)
			{
				Resource = resource;
				StandardEstimateMinutes = standardEstimateMinutes;
			}

			internal GlbStaff Resource { get; private set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			internal decimal StandardEstimateMinutes { get; private set; }
		}
	}
}
