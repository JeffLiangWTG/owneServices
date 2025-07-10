using System;
using System.Globalization;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class NetworkActionHelper
	{
		public static string GetActionIsNotAccessibleMessage(INetworkActionAccessibility accessibility, string header = null)
		{
			if (accessibility.IsAllowed)
			{
				throw new InvalidOperationException("This method should not be called when action is accessible.");
			}

			return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
				header,
				Environment.NewLine,
				accessibility.ToString());
		}

		public static string GetTooltipForAction(string description, INetworkActionAccessibility isEnabled)
		{
			return isEnabled == null || isEnabled.IsAllowed
				? description
				: string.Format(CultureInfo.InvariantCulture, "{0}{1}{1}{2}",
					description,
					Environment.NewLine,
					GetActionIsNotAccessibleMessage(isEnabled,
						header: Res.GetString("CAFCD5D0-D186-40F9-9AB1-A22206F1656C", "This action cannot be executed for the given shape(s) due to the following reasons:")));
		}
	}
}
