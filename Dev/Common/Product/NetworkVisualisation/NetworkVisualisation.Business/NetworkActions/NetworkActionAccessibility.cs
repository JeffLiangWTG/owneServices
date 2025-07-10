using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	[DebuggerDisplay("NetworkActionAccessibility: {ToString()}")]
	public class NetworkActionAccessibility : INetworkActionAccessibility
	{
		public NetworkActionAccessibility(bool isAllowed, INetworkEntity entity, Func<string> explanationProvider)
			: this(isAllowed, () => new NetworkActionDenialReason(entity, explanationProvider()))
		{
		}

		public NetworkActionAccessibility(bool isAllowed, Func<INetworkActionDenialReason> denialReasonProvider)
			: this(isAllowed, () => new INetworkActionDenialReason[] { denialReasonProvider() })
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public NetworkActionAccessibility(bool isAllowed, Func<IEnumerable<INetworkActionDenialReason>> denialReasonsProvider = null)
		{
			if (!isAllowed)
			{
				var reasons = denialReasonsProvider?.Invoke()?.Where(r => r != null).ToArray();

				if (reasons == null || !reasons.Any())
				{
					throw new ArgumentException("Should provide reasons of denial.", nameof(denialReasonsProvider));
				}
				DenialReasons = reasons;
			}
		}

		public NetworkActionAccessibility(INetworkEntity entity, string explanation)
			: this(new NetworkActionDenialReason(entity, explanation))
		{
		}

		public NetworkActionAccessibility(INetworkActionDenialReason denialReason)
			: this(new INetworkActionDenialReason[] { denialReason })
		{
		}

		public NetworkActionAccessibility(IEnumerable<INetworkActionDenialReason> denialReasons)
		{
			if (denialReasons != null)
			{
				DenialReasons = denialReasons.Where(r => r != null).ToArray();
			}
		}

		#region Predefined Results

		public static NetworkActionAccessibility Allowed => new NetworkActionAccessibility(true);

#if DEBUG
		public static NetworkActionAccessibility Denied_ForTesting => new NetworkActionAccessibility(new NetworkActionDenialReason(null, "Not allowed."));
#endif

		public static NetworkActionAccessibility GetCancelledByUserWithoutNotification(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(new NetworkActionDenialReason(entity, Res.GetString("E18FE5E5-EC78-4E69-B9E0-ABB3C44D97D8", "Canceled by the user."), needsNotification: false));
		}

		#endregion

		public bool IsAllowed => !DenialReasons.Any();

		public IEnumerable<INetworkActionDenialReason> DenialReasons { get; } = Array.Empty<NetworkActionDenialReason>();

		INetworkActionAccessibility INetworkActionAccessibility.Union(INetworkActionAccessibility second) => this.Union(second);

		public NetworkActionAccessibility Union(INetworkActionAccessibility second)
		{
			return new NetworkActionAccessibility(DenialReasons.Union(second.DenialReasons));
		}

		INetworkActionAccessibility INetworkActionAccessibility.UnionIfAllowed(Func<INetworkActionAccessibility> second) => this.UnionIfAllowed(second);

		public NetworkActionAccessibility UnionIfAllowed(Func<INetworkActionAccessibility> second)
		{
			return !IsAllowed ? this : new NetworkActionAccessibility(second()?.DenialReasons);
		}

		public static NetworkActionAccessibility Join(IEnumerable<INetworkActionAccessibility> applicabilities)
		{
			if (applicabilities == null || !applicabilities.Any())
			{
				return Allowed;
			}

			return new NetworkActionAccessibility(applicabilities.SelectMany(a => a.DenialReasons));
		}

		#region Object Overrides

		public override string ToString()
		{
			var builder = new StringBuilder();

			var firstLine = true;
			foreach (var reason in DenialReasons)
			{
				if (!firstLine)
				{
					builder.AppendLine();
				}
				firstLine = false;

				// the user most likely won't see this for a deleted message
				// as the network action context menu is closed straight after deletion
				// and the diagram ribbon is never activated for a deleted entity
				// but just to be on the safe side we handle this case as well
				var name = !reason.Entity.IsDeleted ? reason.Entity.Name : Res.GetString("0B73A1EC-62E4-4A1E-A53C-95345690027B", "<deleted entity>");

				builder.AppendFormat(CultureInfo.InvariantCulture, "{0}: {1}", name, reason.Explanation);
			}

			return builder.ToString();
		}

		#endregion
	}
}
