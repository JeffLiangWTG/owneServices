using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public static class CapacityCalculator
	{
		public const int DecimalPlaces = 2;

		#region API

		public static IDictionary<GlbStaff, IResourceCapacity> GetUtilisedCapacityBreakdown(
			IEnumerable<GlbStaff> staffs,
			BMComponent buffer,
			bool useCache = true,
			bool calculateIfNotInCache = true)
		{
			if (staffs == null)
			{
				return null;
			}

			var staffCapacities = new Dictionary<GlbStaff, IResourceCapacity>();
			var staffsToCalculate = new List<GlbStaff>();
			var considerZoneMultipliers = !ExperimentalSettingsProvider.ZoneMultipliersDisabled(buffer);

			foreach (var resource in staffs)
			{
				var cachedResult = useCache ? GetCapacityFromPersistentCache(buffer.Factory, resource.GS_Code, buffer, clearIfStale: calculateIfNotInCache) : null;

				if (cachedResult == null)
				{
					if (calculateIfNotInCache)
					{
						staffsToCalculate.Add(resource);
					}
					else
					{
						staffCapacities.Add(resource, ResourceCapacity.Zero(considerZoneMultipliers));
					}
				}
				else
				{
					staffCapacities.Add(resource, cachedResult);
				}
			}

			if (staffsToCalculate.Count > 0)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("CapacityCalculator.GetUtilisedCapacityBreakdown", buffer.FC_Name))
				{
					var activeStaffs = staffsToCalculate.Where(x => x.GS_IsActive).ToArray();
					var breakdowns = activeStaffs.Any() ? CapacityCalculatorImpl.GetUtilisedCapacityBreakdowns(activeStaffs, buffer, considerZoneMultipliers) : new Dictionary<string, IResourceCapacity>();

					foreach (var resource in staffsToCalculate)
					{
						breakdowns.TryGetValue(resource.GS_Code, out var capacity);
						staffCapacities.Add(resource, capacity ?? ResourceCapacity.CreateForResource(resource.GS_Code, considerZoneMultipliers));
					}

					if (useCache)
					{
						BufferCapacityCache.MergeWithLocalCache(buffer.PK, breakdowns);
					}
				}
			}

			return staffCapacities;
		}

		public static IResourceCapacity GetUtilisedCapacityBreakdown(GlbStaff resource, BMComponent buffer, bool useCache = true, bool calculateIfNotInCache = true)
		{
			return GetUtilisedCapacityBreakdown(new[] { resource }, buffer, useCache, calculateIfNotInCache: calculateIfNotInCache).Values.First();
		}

		static IResourceCapacity GetCapacityFromPersistentCache(BusinessObjectFactory factory, ZString resourceCode, BMComponent buffer, bool clearIfStale)
		{
			if (!BMSRegistry.Instance.CacheCalculatedCapacity.Value)
			{
				return null;
			}

			return BufferCapacityCache.Get(buffer.PK).GetCapacity(resourceCode, factory, clearIfStale);
		}

		#endregion

		#region For Test
#if DEBUG

		public static IResourceCapacity GetFullCapacity_ForTest(GlbStaff resource, BMComponent buffer, WorkingTimeContext workingTimeContext)
		{
			if (resource != null)
			{
				var cachedResult = GetCapacityFromPersistentCache(buffer.Factory, resource.GS_Code, buffer, clearIfStale: true);

				if (cachedResult != null)
				{
					return cachedResult;
				}
			}

			using (PerformanceStatisticsCollector.StartMonitoring("CapacityCalculator.GetFullCapacity", buffer.FC_Name))
			{
				return CapacityCalculatorImpl.GetFullCapacity(resource, buffer, workingTimeContext, true);
			}
		}

		public static decimal GetFullCapacity_ForTest(GlbStaff resource, BMComponent buffer)
		{
			if (!Globals.IsTest)
			{
				throw new InvalidOperationException("Only call this in unit tests. Production code should pass in the appropriate WorkingTimeContext.");
			}

			return GetFullCapacity_ForTest(resource, buffer, WorkingTimeContext.Create(buffer, resource)).FullCapacity;
		}

		public static decimal GetUtilisedCapacity_ForTest(GlbStaff resource, BMComponent buffer, WorkingTimeContext workingTimeContext)
		{
			if (resource != null)
			{
				var cachedResult = GetCapacityFromPersistentCache(buffer.Factory, resource.GS_Code, buffer, clearIfStale: true);

				if (cachedResult != null)
				{
					return cachedResult.UtilisedCapacity;
				}
			}

			using (PerformanceStatisticsCollector.StartMonitoring("CapacityCalculator.GetUtilisedCapacity", buffer.FC_Name))
			{
				return CapacityCalculatorImpl.GetUtilisedCapacity(resource, buffer, workingTimeContext);
			}
		}

		public static decimal GetUtilisedCapacity_ForTest(GlbStaff resource, BMComponent buffer)
		{
			if (!Globals.IsTest)
			{
				throw new InvalidOperationException("Only call this in unit tests. Production code should pass in the appropriate WorkingTimeContext.");
			}

			return GetUtilisedCapacity_ForTest(resource, buffer, WorkingTimeContext.Create(buffer, resource));
		}

		public static IEnumerable<WorkflowDTO> GetComponentContents_ForTest(BMComponent component, IEnumerable<GlbStaff> resources, ILogger logger = null)
		{
			return CapacityCalculatorImpl.GetComponentContents(component, resources, logger);
		}

#endif
		#endregion
	}
}
