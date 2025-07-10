using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public interface IResourceCapacity
	{
		ZDateTime CalculatedTimeUtc { get; }
		string StaffCode { get; }
		decimal FullCapacity { get; }
		decimal FullCapacityForWorkInvolvingCCR { get; }
		decimal AvailableCapacity { get; }
		decimal AvailableCapacityForWorkInvolvingCCR { get; }
		decimal UtilisedCapacity { get; }
		decimal NonZoneWeightedUtilisedCapacity { get; }
		bool IsOverloaded { get; }
		decimal NonCCROverloadMultiplier { get; }

		/// <summary>
		/// Allocated Capacity represents task estimates for workflows in the relevant zones (excluding zone multipliers)
		/// </summary>
		/// <param name="zone">The zone to retreive the allocated capacity for</param>
		/// <returns>Allocated capacity for the passed in zone in decimal format</returns>
		decimal GetZoneAllocatedCapacity(int zone);

		/// <summary>
		/// Reserved Capacity is allocated capacity with the relevant zone multiplier applied to it
		/// </summary>
		/// <param name="zone">The zone to retreive the reserved capacity for</param>
		/// <returns>Reserved capacity for the passed in zone in decimal format</returns>
		decimal GetZoneReservedCapacity(int zone);
		IResourceCapacity WithRoundedBreakdown(int decimalPlaces);
		IResourceCapacity WithZoneCapacities(Dictionary<int, decimal> zonesAllocatedCapacity, Dictionary<int, decimal> zonesReservedCapacity);
		IResourceCapacity WithNonCCROverloadMultiplier(decimal nonCCROverloadMultiplier);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		IResourceCapacity Deduct(decimal utilisedCapacityHours);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		IResourceCapacity Deduct(decimal utilisedCapacityHours, int zoneId, decimal multiplier);
		IResourceCapacity Clone();
	}
}
