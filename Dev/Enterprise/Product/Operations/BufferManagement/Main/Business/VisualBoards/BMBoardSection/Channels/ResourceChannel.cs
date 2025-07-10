using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Cache;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.BufferManagement.Business.BMBoardSectionViewModel;

namespace Enterprise.BufferManagement.Business
{
	class ResourceChannel : VisualBoardChannel
	{
		internal ResourceChannel(GlbStaff resource, ChannelFactoryParameters parameters)
			: base(resource.PK, resource.GS_Code, ChannelTypeList.Codes.Resource, () => parameters.Cache, () => parameters.FactoryProvider)
		{
			resource.AddBMSHolidayFetchHint();
			Parameters = parameters;
		}

		ChannelFactoryParameters Parameters { get; }

		#region Status

		internal static void SeedCacheCapacity(BusinessObjectFactory factory, PropertyCache propertyCache, IEnumerable<ZGuid> componentPKs, IEnumerable<ResourceChannel> channels)
		{
			var resourcesPKs = channels.Select(channel => channel.EntityPK).Distinct();
			var resources = factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, resourcesPKs));
			var components = factory.Load<BMComponent>(new ZQuery(BMComponentSchema.PK, componentPKs));

			foreach (var component in components)
			{
				SeedCacheCapacity(propertyCache, channels, resources, component);
			}
		}

		static string GetChannelCapacityPropertyName(BMComponent component) => "ChannelCapacity" + component.PK;

		static void SeedCacheCapacity(PropertyCache propertyCache, IEnumerable<ResourceChannel> channels, GlbStaff[] resources, BMComponent component)
		{
			var calculateIfNotInCache = BMSRegistry.Instance.BoardsAutoCalculateCapacityWhenNotInCache.Value;
			var capacities = CapacityCalculator.GetUtilisedCapacityBreakdown(resources, component, calculateIfNotInCache: calculateIfNotInCache).ToDictionary(k => k.Key.PK, v => v.Value);

			foreach (var capacity in capacities)
			{
				propertyCache.OverwriteCachedValue(capacity.Key, GetChannelCapacityPropertyName(component), capacity.Value);
			}

			foreach (var channel in channels)
			{
				if (capacities.TryGetValue(channel.EntityPK, out var capacity))
				{
					channel.Cache.OverwriteCachedValue(channel.EntityPK, GetChannelCapacityPropertyName(component), capacity);
				}
			}
		}

		internal static void SeedCacheWithStatus(BusinessObjectFactory factory, PropertyCache propertyCache, CardAllocationMap allocationMap, IEnumerable<ResourceChannel> channels)
		{
			foreach (var channel in channels)
			{
				var status = channel.GetStatus(factory, channel.Cache, allocationMap ?? channel.Parameters.GetComponentGrid().CardAllocationMap);
				propertyCache.OverwriteCachedValue(channel.EntityPK, ChannelStatusCachePropertyName, status);
			}
		}

		internal static IResourceCapacity GetCapacity(PropertyCache propertyCache, GlbStaff resource, BMComponent component)
		{
			var capacityBreakdown = propertyCache.GetCachedValue<IResourceCapacity>(resource.PK, GetChannelCapacityPropertyName(component));

			if (capacityBreakdown == null)
			{
				var calculateIfNotInCache = BMSRegistry.Instance.BoardsAutoCalculateCapacityWhenNotInCache.Value;
				capacityBreakdown = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, component, calculateIfNotInCache: calculateIfNotInCache);
			}

			return capacityBreakdown;
		}

		protected override ChannelStatus GetChannelStatusCore(BusinessObjectFactory factory, PropertyCache propertyCache)
		{
			if (Parameters.IsPreview)
			{
				return new ChannelStatus
				{
					Status = WorkingStatusText,
					ToolTipStatus = WorkingStatusText,
					IsHighRisk = false,
				};
			}

			return GetStatus(factory, Cache, Parameters.GetComponentGrid().CardAllocationMap);
		}

		#region GET Status

		ChannelStatus GetStatus(BusinessObjectFactory factory, PropertyCache propertyCache, CardAllocationMap allocationMap)
		{
			Thread.CurrentThread.RequireNotOnVisualBoardFormThread();
			var staff = factory.Load<GlbStaff>(this.EntityPK);

			if (staff == null)
			{
				return new ChannelStatus { Status = Res.GetString("e34fdc48-cfa2-41d0-9474-4d824b2c8b30", "Load failed."), ToolTipStatus = Res.GetString("e34fdc48-cfa2-41d0-9474-4d824b2c8b30", "Load failed.") };
			}

			var roadRunnerDetails = propertyCache.GetCachedValue(staff.PK, CacheConstants.ChannelRoadRunnerStatus, () => Parameters.GetAndCacheSingleResourceRoadRunnerDetails(staff, factory));
			var status = GetChannelStatus(factory, roadRunnerDetails, allocationMap);

			if (roadRunnerDetails.Status == RoadRunnerStatus.Away)
			{
				var awayUntilTime = AppendAwayUntilTime(roadRunnerDetails.AwayUntil);

				if (status.Status == status.ToolTipStatus)
				{
					status.ToolTipStatus += awayUntilTime;
				}
				status.Status += awayUntilTime;
			}
			else
			{
				string relevantTimeText;
				RoadRunnerStatusCacheService service = factory.ServiceContainer.GetService<RoadRunnerStatusCacheService>();
				if (service != null)
				{
					relevantTimeText = service.GetOrCacheValue(staff, "AppendRelevantTimeText", () => AppendRelevantTimeText(roadRunnerDetails, roadRunnerDetails.RelevantActivityTime));
				}
				else
				{
					relevantTimeText = AppendRelevantTimeText(roadRunnerDetails, roadRunnerDetails.RelevantActivityTime);
				}

				if (status.Status == status.ToolTipStatus)
				{
					status.ToolTipStatus += relevantTimeText;
				}
				status.Status += relevantTimeText;
			}
			return status;
		}

		string AppendRelevantTimeText(RoadRunnerDetails roadRunnerDetails, TimeSpan relevantTime)
		{
			var relevantTimeText = string.Empty;
			if (relevantTime > TimeSpan.Zero)
			{
				var friendlyTimeString = relevantTime.ToFriendlyTimeString(maxUnitRollup: roadRunnerDetails.Status == RoadRunnerStatus.FullSpeed ? FriendlyMaxUnitRollup.Hour : FriendlyMaxUnitRollup.Year);
				if (!string.IsNullOrEmpty(friendlyTimeString))
				{
					relevantTimeText = " " + Res.GetString("8a6f6e0c-305c-42ec-80a2-4724b8d41f71", "for {0}", friendlyTimeString);
				}
			}
			return relevantTimeText;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		string AppendAwayUntilTime(ZDateTime awayUntil)
		{
			var awayUntilTime = " " + Res.GetString("90045b51-cfc4-441b-a200-623d494bfd10", "until") + " ";
			if (awayUntil.Year != ZDateTime.Now.Year)
			{
				awayUntilTime += awayUntil.ToString(@"ddd d-MMM yyyy");
			}
			else
			{
				awayUntilTime += awayUntil.ToString(@"ddd d-MMM");
			}
			return awayUntilTime;
		}

		ChannelStatus GetChannelStatus(BusinessObjectFactory factory, RoadRunnerDetails roadRunnerDetails, CardAllocationMap allocationMap)
		{
			ChannelStatus status;
			if (Parameters.IsBuffer)
			{
				status = GetChannelLoadingStatus(factory, roadRunnerDetails, allocationMap);
				if (!status.Status.IsNullOrEmpty())
				{
					if (status.Status == status.ToolTipStatus)
					{
						status.ToolTipStatus += ", ";
					}
					status.Status += ", ";
				}
			}
			else
			{
				status = GetResourceStatus(roadRunnerDetails);
			}
			var statusText = GetStatusText(roadRunnerDetails);
			if (status.Status == status.ToolTipStatus)
			{
				status.ToolTipStatus += statusText;
			}
			status.Status += statusText;
			return status;
		}

		static string WorkingStatusText => Res.GetString("654a1775-81aa-4a72-89b1-b8a6874a7a96", "Working");

		static string GetStatusText(RoadRunnerDetails roadRunnerDetails)
		{
			switch (roadRunnerDetails.Status)
			{
				case RoadRunnerStatus.FullSpeed:
				case RoadRunnerStatus.WorkingInOtherComponent:
					return WorkingStatusText;

				case RoadRunnerStatus.Alert:
					return Res.GetString("4a64747f-4f7c-46e0-8f59-472c78bc9a41", "Alert");

				case RoadRunnerStatus.WorkingOnStandbyTask:
				case RoadRunnerStatus.OnStandby:
					return Res.GetString("80b971ef-be62-46b0-89e7-42f315e00e6e", "On Standby");

				case RoadRunnerStatus.Stopped:
					return Res.GetString("206c8284-a940-40fb-8a15-b8442fd8d982", "Idle");

				case RoadRunnerStatus.Away:
					return Res.GetString("e2d1c1d1-88aa-4a29-8d43-1d3d13d4414d", "Away");

				default:
					return string.Empty;
			}
		}

		ChannelStatus GetResourceStatus(RoadRunnerDetails roadRunnerDetails)
		{
			var result = new ChannelStatus
			{
				IsOvertime = roadRunnerDetails.IsOvertime,
				StatusImage = GetStatusImage(roadRunnerDetails),
			};
			return result;
		}

		#region ImageWithTooltip 
		ImageWithTooltip GetStatusImage(RoadRunnerDetails roadRunnerDetails)
		{
			if (Parameters.IsBuffer)
			{
				var image = GetRoadRunnerStatusIcon(roadRunnerDetails.Status);
				var tooltip = GetRoadRunnerTooltip(roadRunnerDetails);

				return new ImageWithTooltip(image, tooltip);
			}

			return default(ImageWithTooltip);
		}

		[ThreadSafe]
		readonly static Lazy<MemoryCache> roadRunnerStatusIconCache = new Lazy<MemoryCache>(() => new MemoryCache("roadRunnerStatusIconCache"), true);

		static MemoryCache RoadRunnerStatusIconCache => roadRunnerStatusIconCache.Value;

		static Image GetRoadRunnerStatusIcon(RoadRunnerStatus status)
		{
			var cacheItemPolicy = new CacheItemPolicy();

			switch (status)
			{
				case RoadRunnerStatus.FullSpeed:
				case RoadRunnerStatus.WorkingInOtherComponent:
					return RoadRunnerStatusIconCache.GetOrAdd("resource_active", () => Properties.Resources.resource_active, cacheItemPolicy);

				case RoadRunnerStatus.Stopped:
				case RoadRunnerStatus.WorkingOnStandbyTask:
				case RoadRunnerStatus.OnStandby:
					return RoadRunnerStatusIconCache.GetOrAdd("resource_inactive", () => Properties.Resources.resource_inactive, cacheItemPolicy);

				case RoadRunnerStatus.Away:
					return RoadRunnerStatusIconCache.GetOrAdd("resource_away", () => Properties.Resources.resource_away, cacheItemPolicy);

				case RoadRunnerStatus.Alert:
					return RoadRunnerStatusIconCache.GetOrAdd("resource_alert", () => Properties.Resources.resource_alert, cacheItemPolicy);

				default:
					return null;
			}
		}

		static string GetRoadRunnerTooltip(RoadRunnerDetails roadRunnerDetail)
		{
			switch (roadRunnerDetail.Status)
			{
				case RoadRunnerStatus.FullSpeed:
					return Res.GetString("24bfb8e7-ed88-4e69-aa61-65798f672d7d", "Full Speed: Working on a task within a buffer");

				case RoadRunnerStatus.Stopped:
					return Res.GetString("d91deaca-f7a4-4ab4-9a26-5afd0ca7ee88", "Stopped: Not working on any task");

				case RoadRunnerStatus.Alert:
					return GetTooltipTextForClickableRoadRunner(roadRunnerDetail, Res.GetString("4d78ff1f-2a47-431f-b59a-f987327ff807", "ALERT: there is a buffer task pending, but the resource is working on a task outside the buffer."));

				case RoadRunnerStatus.WorkingOnStandbyTask:
					return GetRoadRunnerStatusWithWorkingTask(Res.GetString("00122362-f3f4-472e-8348-8f7cea918327", "Working on Standby Task. A standby task is a task with a workflow or a job marked as 'Standby'"), roadRunnerDetail.RelevantTaskDescription);

				case RoadRunnerStatus.OnStandby:
					return Res.GetString("00a22362-f4f4-471e-8348-8f7cea918327", "Standby: Not working on any task, and is a target of a critical handover");

				case RoadRunnerStatus.WorkingInOtherComponent:
					return GetTooltipTextForClickableRoadRunner(roadRunnerDetail, Res.GetString("7057215f-39a7-4542-b3f7-32cfd57e5f92", "Working on a task outside the buffer"));

				case RoadRunnerStatus.Away:
					return Res.GetString("e1e68a24-0c5c-47a6-9a02-d4aab267fdb1", "Away");
			}

			return null;
		}

		static string GetTooltipTextForClickableRoadRunner(RoadRunnerDetails roadRunnerDetail, string statusMessage)
		{
			var result = new StringBuilder();
			var status = GetRoadRunnerStatusWithWorkingTask(statusMessage, roadRunnerDetail.RelevantTaskDescription);
			result.Append(status);
			result.AppendLine();
			result.Append(Res.GetString("314f682f-26ec-4095-a73a-4881b64a033e", "Click to open the task's job."));

			return result.ToString();
		}

		static string GetRoadRunnerStatusWithWorkingTask(string status, ZString taskDescription)
		{
			var result = new StringBuilder();
			result.Append(status);
			result.AppendLine();
			result.Append(Res.GetString("4d78ff1f-2a47-431f-b29a-f987327fa807", "Working task: "));
			result.Append(taskDescription);

			return result.ToString();
		}

		#endregion

		ChannelStatus GetChannelLoadingStatus(BusinessObjectFactory factory, RoadRunnerDetails roadRunnerDetails, CardAllocationMap cardAllocationMap)
		{
			var channelStatus = GetResourceStatus(roadRunnerDetails);

			if (BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				return channelStatus;
			}

			var section = BMBoardSection.Load(factory, Parameters.SectionPK);

			var channelCell = (
				from cell in Parameters.GetComponentGrid().Cells
				where cell.ContentType == CellContentType.ChannelHeading
				where cell.HasSameChannel(this)
				select cell).FirstOrDefault();

			if (channelCell != null)
			{
				var resource = factory.Load<GlbStaff>(EntityPK);
				var component = section.Component != null ? factory.Load<BMComponent>(section.Component.PK) : null;
				var capacity = GetCapacity(Cache, resource, component);

				if (capacity.IsOverloaded)
				{
					channelStatus.Status = Res.GetString("a068d526-b414-451d-ba73-80cc9fd0b681", "Overloaded");
					channelStatus.ToolTipStatus = GetToolTipStatus(section, cardAllocationMap, channelStatus);
					channelStatus.RiskComponentPK = GetRiskComponent(section, cardAllocationMap);
				}
				else if (this.IsCCRChannel(section, Parameters.IsInConstrainedMode))
				{
					var isHighRisk = true;
					var statusText = GetStatusTextForCCR(section, cardAllocationMap, resource);

					if (statusText.IsNullOrEmpty())
					{
						isHighRisk = false;
					}

					channelStatus.IsHighRisk = isHighRisk;
					channelStatus.Status = statusText;
					channelStatus.ToolTipStatus = statusText;
				}
				else
				{
					var workingZoneStatus = Parameters.GetComponentGrid().GetWorkingZoneStatus(this, section, cardAllocationMap);
					if (!workingZoneStatus.IsNullOrEmpty())
					{
						channelStatus.Status = workingZoneStatus;
						channelStatus.ToolTipStatus = GetToolTipStatus(section, cardAllocationMap, channelStatus);
						channelStatus.RiskComponentPK = GetRiskComponent(section, cardAllocationMap);
					}
				}
			}

			return channelStatus;
		}

		string GetToolTipStatus(BMBoardSection section, CardAllocationMap cardAllocationMap, ChannelStatus channelStatus)
		{
			var toolTipStatus = Parameters.GetComponentGrid().GetSubComponentTooltipStatus(this, section, cardAllocationMap);
			return !toolTipStatus.IsNullOrEmpty() ? toolTipStatus : channelStatus.Status;
		}

		string GetStatusTextForCCR(BMBoardSection section, CardAllocationMap cardAllocationMap, GlbStaff resource)
		{
			var componentGrid = Parameters.GetComponentGrid();

			if (ComponentGrid.IsInOutsideTargetZoneRiskState(ChannelEntityCode, componentGrid))
			{
				return Res.GetString("8C60B4B3-3B2E-4188-B845-91BC8D6468B4", "Outside target zone");
			}

			if (ComponentGrid.IsInNotEnoughWorkRiskState(this, section, cardAllocationMap) && !resource.IsOnLeave)
			{
				return Res.GetString("3D3FE898-5D1D-4DF7-B664-3CB2F6481EA2", "Not enough work");
			}

			return string.Empty;
		}

		ZGuid GetRiskComponent(BMBoardSection section, CardAllocationMap cardAllocationMap)
		{
			var result = ZGuid.Empty;
			var subComponentPK = Parameters.GetComponentGrid().GetSubComponentPK(this, section, cardAllocationMap);
			if (subComponentPK != null)
			{
				var riskComponent = section.AllComponents.SelectMany(c => c.ChildComponents).SingleOrDefault(c => c.PK == subComponentPK);
				if (riskComponent != null)
				{
					result = riskComponent.PK;
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region Descriptor

		protected override ChannelDescriptor GetChannelDescriptorCore(BusinessObjectFactory factory)
		{
			var staff = factory.Load<GlbStaff>(EntityPK);

			if (staff != null)
			{
				return new ChannelDescriptor
				{
					ChannelNames = new ChannelNameBuilder(staff.GS_FullName, staff.GS_FriendlyName, staff.GS_Code).Build(),
					DisplayImage = staff.ProfileImage,
				};
			}
			else
			{
				return new ChannelDescriptor();
			}
		}

		#endregion

		#region Matcher

		protected override ChannelMatcher GetChannelMatcherCore(BusinessObjectFactory factory)
		{
			if (!Parameters.IsPreview && this.TryGetCapabilityPKs(factory, out var capabilityPKs))
			{
				return new ChannelMatcher(task => IsInChannel(task, capabilityPKs));
			}
			else
			{
				return ChannelMatcher.Empty;
			}
		}

		bool IsInChannel(IProcessTask task, IEnumerable<ZGuid> capabilities)
		{
			var concreteTask = (ProcessTask)task;

			if (string.Equals(concreteTask.P9_GS_NKAssignedStaffMember, ChannelEntityCode, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}

			if (!concreteTask.RequiresResourceWithCapability)
			{
				return false;
			}

			if (capabilities == null || !capabilities.Any(capabilityPK => concreteTask.P9_G4_RequiredCapability == capabilityPK))
			{
				return false;
			}

			if (ShouldHideTaskFromCapability(concreteTask.P9_G4_RequiredCapability))
			{
				return false;
			}

			if (concreteTask.RequiredCapability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope)
			{
				var assignedGroup = concreteTask.AssignedGroup;

				if (assignedGroup == null)
				{
					var processHeader = task.GetProcessHeader();

					if (processHeader.ReleaseGroup != null && !processHeader.ReleaseGroup.Staff.Any(staff => staff.PK == EntityPK))
					{
						return false;
					}
				}
				else if (!assignedGroup.Staff.Any(staff => staff.PK == EntityPK))
				{
					return false;
				}
			}

			return true;
		}

		bool ShouldHideTaskFromCapability(ZGuid capabilityPk)
		{
			if (Parameters.HideCapabilityTasksFromResourceChannels && Parameters.CapabilityChannelEntityPKs != null)
			{
				return Parameters.CapabilityChannelEntityPKs.Contains(capabilityPk);
			}

			return false;
		}

		#endregion

		#region ClearCache

		protected override void ClearChannelCacheCore()
		{
			base.ClearChannelCacheCore();
			Cache.Remove(EntityPK, CacheConstants.ChannelRoadRunnerStatus);
		}

		#endregion

		#region Tests
#if DEBUG
		public string GetStatusInCacheForTest() => (Cache.GetDumpOfCacheForTest()[EntityPK + ChannelStatusCachePropertyName] as ChannelStatus)?.Status;
#endif
		#endregion
	}
}
