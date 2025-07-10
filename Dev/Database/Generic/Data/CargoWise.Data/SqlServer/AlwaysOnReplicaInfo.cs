using System;

namespace CargoWise.Data.SqlServer
{
	public class AlwaysOnReplicaInfo : IEquatable<AlwaysOnReplicaInfo>
	{
		public string ReplicaServerName { get; set; }
		public int AvailabilityMode { get; set; }

		public bool Equals(AlwaysOnReplicaInfo other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return string.Equals(ReplicaServerName, other.ReplicaServerName, StringComparison.OrdinalIgnoreCase) && AvailabilityMode == other.AvailabilityMode;
		}

		public override bool Equals(object obj) => obj is AlwaysOnReplicaInfo other && Equals(other);

		public override int GetHashCode()
		{
			return (ReplicaServerName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(ReplicaServerName) : 0) ^ AvailabilityMode;
		}

		public static bool operator ==(AlwaysOnReplicaInfo left, AlwaysOnReplicaInfo right)
			=> Equals(left, right);

		public static bool operator !=(AlwaysOnReplicaInfo left, AlwaysOnReplicaInfo right)
			=> !Equals(left, right);
	}
}
