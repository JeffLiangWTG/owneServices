using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Business
{
	/// <summary>
	/// Saves the state of objects used in compliance risk status calculation.
	/// The class is immutable, its properties cannot be changed after creation.
	/// The class overrides the equality operators and methods to compare objects by value.
	/// See <see href="https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type">MS documentation</see> for details.
	/// </summary>
	/// <param name="parties">Parties used in party risk status calculation.</param>
	/// <param name="locations">Locations used in location risk status calculation.</param>
	/// <param name="commodities">Commodities used in commodity risk status calculation.</param>
	/// <param name="isInternational">Flag that shows if the job is international.</param>
	/// <param name="subProvidersCommodityRiskStatus">Commodity risk status of commodity risk sub providers.</param>
	public class ComplianceSnapShot(
		List<(ZGuid PK, ZString ScreeningStatus)> parties,
		List<(ZGuid PK, bool IsSanctioned)> locations,
		List<(ZString HarmonizedCode, ZString RiskStatus, CommodityType CommodityType)> commodities,
		bool isInternational,
		ZString subProvidersCommodityRiskStatus)
	{
		/// <summary>
		/// Parties used in party risk status calculation.
		/// </summary>
		public List<(ZGuid PK, ZString ScreeningStatus)> Parties { get; } = parties;

		/// <summary>
		/// Locations used in location risk status calculation.
		/// </summary>
		public List<(ZGuid PK, bool IsSanctioned)> Locations { get; } = locations;

		/// <summary>
		/// Commodities used in commodity risk status calculation.
		/// </summary>
		public List<(ZString HarmonizedCode, ZString RiskStatus, CommodityType CommodityType)> Commodities { get; } = commodities;

		/// <summary>
		/// Flag that shows if the job is international.
		/// </summary>
		public bool IsInternational { get; } = isInternational;

		/// <summary>
		/// Commodity risk status of commodity risk sub providers.
		/// </summary>
		public ZString SubProvidersCommodityRiskStatus { get; } = subProvidersCommodityRiskStatus;

		/// <summary>
		/// Overloads the equality operator to compare objects by value.
		/// </summary>
		/// <param name="lhs">Left hand side argument.</param>
		/// <param name="rhs">Right hand side argument.</param>
		/// <returns>true if the objects are equal.</returns>
		public static bool operator == (ComplianceSnapShot lhs, ComplianceSnapShot rhs) => lhs is null ? rhs is null : lhs.Equals(rhs);

		/// <summary>
		/// Overloads the inequality operator to compare objects by value.
		/// </summary>
		/// <param name="lhs">Left hand side argument.</param>
		/// <param name="rhs">Right hand side argument.</param>
		/// <returns>true if the objects are not equal.</returns>
		public static bool operator != (ComplianceSnapShot lhs, ComplianceSnapShot rhs) => !(lhs == rhs);

		/// <summary>
		/// Overrides the GetHashCode method to compare objects by value.
		/// It is required to define value equality for a class.
		/// </summary>
		/// <returns>Instance hash code.</returns>
		public override int GetHashCode() => (Parties, Locations, Commodities, IsInternational, SubProvidersCommodityRiskStatus).GetHashCode();

		/// <summary>
		/// Overrides the Equals method to compare objects by value.
		/// It is required to define value equality for a class.
		/// </summary>
		/// <param name="obj">Another object to compare the instance with.</param>
		/// <returns>true if the provided object is equal to the instance.</returns>
		public override bool Equals(object obj) => obj is ComplianceSnapShot other && Equals(other);

		/// <summary>
		/// The Equals method to compare ComplianceSnapShot objects by value.
		/// </summary>
		/// <param name="other">Another ComplianceSnapShot object to compare the instance with.</param>
		/// <returns>true if the provided object is equal to the instance.</returns>
		public bool Equals(ComplianceSnapShot other) =>
			ReferenceEquals(this, other) || (other is not null
				&& IsInternational == other.IsInternational
				&& Parties.Count.Equals(other.Parties.Count)
				&& Locations.Count.Equals(other.Locations.Count)
				&& Commodities.Count.Equals(other.Commodities.Count)
				&& SubProvidersCommodityRiskStatus == other.SubProvidersCommodityRiskStatus
				&& Locations.All(u => other.Locations.Any(v => v.PK == u.PK && v.IsSanctioned == u.IsSanctioned))
				&& Parties.All(u => other.Parties.Any(v => v.PK == u.PK && v.ScreeningStatus == u.ScreeningStatus))
				&& Commodities.All(u => other.Commodities.Any(v => v.HarmonizedCode == u.HarmonizedCode && v.RiskStatus == u.RiskStatus)));

		public bool Contains(ComplianceSnapShot other) =>
			ReferenceEquals(this, other) || (other is not null
				&& IsInternational == other.IsInternational
				&& Parties.Count >= other.Parties.Count
				&& Locations.Count >= other.Locations.Count
				&& Commodities.Count >= other.Commodities.Count
				&& SubProvidersCommodityRiskStatus == other.SubProvidersCommodityRiskStatus
				&& other.Locations.All(u => Locations.Any(v => v.PK == u.PK && v.IsSanctioned == u.IsSanctioned))
				&& other.Parties.All(u => Parties.Any(v => v.PK == u.PK && v.ScreeningStatus == u.ScreeningStatus))
				&& other.Commodities.All(u => Commodities.Any(v => v.HarmonizedCode == u.HarmonizedCode && v.RiskStatus == u.RiskStatus)));
	}
}
