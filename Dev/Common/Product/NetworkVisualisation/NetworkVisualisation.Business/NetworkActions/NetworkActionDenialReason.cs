using System.Globalization;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NetworkActionDenialReason : INetworkActionDenialReason
	{
		public NetworkActionDenialReason(INetworkEntity entity, string explanation, bool needsNotification = true)
		{
			Entity = entity;
			Explanation = explanation;
			NeedsNotification = needsNotification;
		}

		public INetworkEntity Entity { get; }

		public string Explanation { get; }

		public bool NeedsNotification { get; } = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A string representation of an object")]
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Entity: {0}, Explanation: {1}, NeedsNotification: {2}", Entity, Explanation, NeedsNotification);
		}

		public override bool Equals(object obj)
		{
			return obj is NetworkActionDenialReason denialReason && Equals(denialReason);
		}

		public bool Equals(NetworkActionDenialReason anotherDeason)
		{
			return Entity == anotherDeason.Entity &&
				Explanation == anotherDeason.Explanation &&
				NeedsNotification == anotherDeason.NeedsNotification;
		}

		public override int GetHashCode() => ToString().GetHashCode();
	}
}
