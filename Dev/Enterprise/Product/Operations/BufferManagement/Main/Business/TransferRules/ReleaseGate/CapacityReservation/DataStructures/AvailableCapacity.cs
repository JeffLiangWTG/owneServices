using System.Diagnostics;

namespace Enterprise.BufferManagement.Business
{
	[DebuggerDisplay("RealAvailableCapacity: {RealAvailableCapacity}, ConsideringAllowedOverload: {EffectiveAvailableCapacityConsideringAllowedOverload}")]
	public readonly struct AvailableCapacity
	{
		internal AvailableCapacity(decimal realAvailableCapacity, decimal effectiveAvailableCapacityConsideringAllowedOverload)
		{
			RealAvailableCapacity = realAvailableCapacity;
			EffectiveAvailableCapacityConsideringAllowedOverload = effectiveAvailableCapacityConsideringAllowedOverload;
		}

		public decimal RealAvailableCapacity { get; }
		public decimal EffectiveAvailableCapacityConsideringAllowedOverload { get; }

		#region Equality

		public override bool Equals(object obj)
		{
			return obj is AvailableCapacity other && this == other;
		}

		public override int GetHashCode()
		{
			return RealAvailableCapacity.GetHashCode() ^ EffectiveAvailableCapacityConsideringAllowedOverload.GetHashCode();
		}

		public static bool operator ==(AvailableCapacity lhs, AvailableCapacity rhs)
		{
			return lhs.RealAvailableCapacity == rhs.RealAvailableCapacity
				&& lhs.EffectiveAvailableCapacityConsideringAllowedOverload == rhs.EffectiveAvailableCapacityConsideringAllowedOverload;
		}

		public static bool operator !=(AvailableCapacity lhs, AvailableCapacity rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
	}
}
