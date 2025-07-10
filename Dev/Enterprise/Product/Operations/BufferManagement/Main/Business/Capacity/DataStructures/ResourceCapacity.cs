using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("FullCapacity: {FullCapacity} (For CCR: {FullCapacityForWorkInvolvingCCR}), AvailableCapacity: {AvailableCapacity} (For CCR: {AvailableCapacityForWorkInvolvingCCR}), UtilisedCapacity: {UtilisedCapacity}")]
	public class ResourceCapacity : IResourceCapacity
	{
		ResourceCapacity(string staffCode, ZDateTime calculatedTimeUtc, decimal fullCapacity, Dictionary<int, decimal> zonesAllocatedCapacity, Dictionary<int, decimal> zonesReservedCapacity, bool considerZoneMultipliers)
		{
			StaffCode = staffCode;
			CalculatedTimeUtc = calculatedTimeUtc;
			FullCapacity = fullCapacity;
			ConsiderZoneMultipliers = considerZoneMultipliers;

			this.zonesAllocatedCapacity = zonesAllocatedCapacity;
			this.zonesReservedCapacity = zonesReservedCapacity;

			if (considerZoneMultipliers && this.zonesAllocatedCapacity == null)
			{
				this.zonesAllocatedCapacity = new Dictionary<int, decimal>();
			}

			if (considerZoneMultipliers && this.zonesReservedCapacity == null)
			{
				this.zonesReservedCapacity = new Dictionary<int, decimal>();
			}
		}

		ResourceCapacity(string staffCode, ZDateTime calculatedTimeUtc, decimal fullCapacity, Dictionary<int, decimal> zonesAllocatedCapacity, Dictionary<int, decimal> zonesReservedCapacity, decimal nonZoneWeightedUtilisedCapacity, decimal nonCCROverloadMultiplier, int? utilisedCapacityDecimalPlaces, bool considerZoneMultipliers)
			: this(staffCode, calculatedTimeUtc, fullCapacity, zonesAllocatedCapacity, zonesReservedCapacity, considerZoneMultipliers)
		{
			NonZoneWeightedUtilisedCapacity = nonZoneWeightedUtilisedCapacity;
			NonCCROverloadMultiplier = nonCCROverloadMultiplier;
			this.utilisedCapacityDecimalPlaces = utilisedCapacityDecimalPlaces;
		}

		readonly Dictionary<int, decimal> zonesAllocatedCapacity;
		readonly Dictionary<int, decimal> zonesReservedCapacity;
		public decimal NonCCROverloadMultiplier { get; } = 1m;
		const int RoundingDecimalPlaces = CapacityCalculator.DecimalPlaces;
		readonly int? utilisedCapacityDecimalPlaces;

		public string StaffCode { get; }
		public ZDateTime CalculatedTimeUtc { get; }

		public decimal FullCapacity { get; }
		public decimal FullCapacityForWorkInvolvingCCR => Utilities.Round(FullCapacity * NonCCROverloadMultiplier, RoundingDecimalPlaces);

		// When not considering zone multipliers and zones (for new calculator), all capacity goes into NonZoneWeightedUtilisedCapacity
		public decimal AvailableCapacity => FullCapacity - ((ConsiderZoneMultipliers ? UtilisedCapacity : 0) + NonZoneWeightedUtilisedCapacity);
		public decimal AvailableCapacityForWorkInvolvingCCR => FullCapacityForWorkInvolvingCCR - (UtilisedCapacity + (ConsiderZoneMultipliers ? NonZoneWeightedUtilisedCapacity : 0));

		public decimal UtilisedCapacity
		{
			get
			{
				return ConsiderZoneMultipliers ? GetCapacity(zonesReservedCapacity).Sum(capacity => capacity.Value) : NonZoneWeightedUtilisedCapacity;
			}
		}

		public decimal NonZoneWeightedUtilisedCapacity { get; set; }

		public decimal GetZoneAllocatedCapacity(int zone) => zonesAllocatedCapacity != null && zonesAllocatedCapacity.Count != 0 && zonesAllocatedCapacity.ContainsKey(zone) ? GetCapacity(zonesAllocatedCapacity)[zone] : default;
		public decimal GetZoneReservedCapacity(int zone) => zonesReservedCapacity != null && zonesReservedCapacity.Count != 0 && zonesReservedCapacity.ContainsKey(zone) ? GetCapacity(zonesReservedCapacity)[zone] : default;
		public bool IsOverloaded => FullCapacity > 0 && UtilisedCapacity / FullCapacity >= BMConstants.ResourceCapacityUtilisationOverloadFactor;

		public IResourceCapacity Clone()
		{
			return Deduct(0m);
		}

		public IResourceCapacity WithRoundedBreakdown(int decimalPlaces)
		{
			return new ResourceCapacity(StaffCode, CalculatedTimeUtc, FullCapacity, zonesAllocatedCapacity, zonesReservedCapacity, NonZoneWeightedUtilisedCapacity, NonCCROverloadMultiplier, decimalPlaces, ConsiderZoneMultipliers);
		}

		public IResourceCapacity WithZoneCapacities(Dictionary<int, decimal> allocatedCapacity, Dictionary<int, decimal> reservedCapacity)
		{
			return new ResourceCapacity(StaffCode, CalculatedTimeUtc, FullCapacity, allocatedCapacity, reservedCapacity, NonZoneWeightedUtilisedCapacity, NonCCROverloadMultiplier, utilisedCapacityDecimalPlaces, ConsiderZoneMultipliers);
		}

		public IResourceCapacity WithNonCCROverloadMultiplier(decimal nonCCRMultiplier)
		{
			return new ResourceCapacity(StaffCode, CalculatedTimeUtc, FullCapacity, zonesAllocatedCapacity, zonesReservedCapacity, NonZoneWeightedUtilisedCapacity, nonCCRMultiplier, utilisedCapacityDecimalPlaces, ConsiderZoneMultipliers);
		}

		public static IResourceCapacity CreateForResource(string staffCode, bool considerZoneMultipliers)
		{
			return CreateForResourceWithFullCapacity(staffCode, 0, considerZoneMultipliers);
		}

		public static IResourceCapacity CreateForResourceWithFullCapacity(string staffCode, decimal fullCapacity, bool considerZoneMultipliers = true)
		{
			return new ResourceCapacity(staffCode, ZDateTime.UtcNow, fullCapacity, null, null, considerZoneMultipliers);
		}

		public static IResourceCapacity CreateResourceCapacity(string staffCode, ZDateTime calculatedTimeUtc, decimal fullCapacity, Dictionary<int, decimal> zonesAllocatedCapacity, Dictionary<int, decimal> zonesReservedCapacity, bool considerZoneMultipliers = true)
		{
			return new ResourceCapacity(staffCode, calculatedTimeUtc, fullCapacity, zonesAllocatedCapacity, zonesReservedCapacity, considerZoneMultipliers);
		}

		public static IResourceCapacity Zero(bool considerZoneMultipliers)
		{
			return new ResourceCapacity(default, ZDateTime.UtcNow, 0, null, null, considerZoneMultipliers);
		}

		Dictionary<int, decimal> GetCapacity(Dictionary<int, decimal> capacities)
		{
			if (utilisedCapacityDecimalPlaces != null)
			{
				return capacities.ToDictionary(capacity => capacity.Key, capacity => Utilities.Round(capacity.Value, utilisedCapacityDecimalPlaces.Value));
			}

			return capacities;
		}

		public IResourceCapacity Deduct(decimal utilisedCapacityToAdd)
		{
			return new ResourceCapacity(StaffCode, CalculatedTimeUtc, FullCapacity, zonesAllocatedCapacity, zonesReservedCapacity, NonZoneWeightedUtilisedCapacity + utilisedCapacityToAdd, NonCCROverloadMultiplier, utilisedCapacityDecimalPlaces, ConsiderZoneMultipliers);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public IResourceCapacity Deduct(decimal utilisedCapacityHours, int zoneId, decimal multiplier)
		{
			if (utilisedCapacityHours == 0)
			{
				return Clone();
			}

			var newZonesAllocatedCapacity = new Dictionary<int, decimal>();
			var newZonesReservedCapacity = new Dictionary<int, decimal>();
			var newNonZoneUtilisedCapacity = 0m;

			if (ConsiderZoneMultipliers)
			{
				newZonesAllocatedCapacity = CopyZonesDictionaryWithExtraUtilisedCapacity(zonesAllocatedCapacity, utilisedCapacityHours, zoneId);
				newZonesReservedCapacity = CopyZonesDictionaryWithExtraUtilisedCapacity(zonesReservedCapacity, utilisedCapacityHours * multiplier, zoneId);
			}
			else
			{
				newNonZoneUtilisedCapacity = utilisedCapacityHours;
			}

			return new ResourceCapacity(StaffCode, CalculatedTimeUtc, FullCapacity, newZonesAllocatedCapacity, newZonesReservedCapacity, NonZoneWeightedUtilisedCapacity + newNonZoneUtilisedCapacity, NonCCROverloadMultiplier, utilisedCapacityDecimalPlaces, ConsiderZoneMultipliers);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static Dictionary<int, decimal> CopyZonesDictionaryWithExtraUtilisedCapacity(Dictionary<int, decimal> sourceZonesDictionary, decimal capacityHours, int zone)
		{
			var result = sourceZonesDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			if (result.ContainsKey(zone))
			{
				result[zone] += capacityHours;
			}
			else
			{
				result.Add(zone, capacityHours);
			}

			return result;
		}

		bool ConsiderZoneMultipliers { get; }
	}
}
