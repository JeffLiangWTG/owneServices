using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Helper class for generating tooltips related to network entities.
	/// </summary>
	public static class TooltipHelper
	{
		/// <summary>
		/// Generates a tooltip based on notifications associated with the entity.
		/// </summary>
		/// <param name="entity">The network entity.</param>
		/// <returns>A formatted string containing notifications, or null if no notifications exist.</returns>
		public static string GetTooltipWhenHasNotification(this INetworkEntity entity)
		{
			var entityNotifications = entity.EntityNotifications;
			if (entityNotifications.Any())
			{
				var groupedNotifications = entityNotifications.OrderBy(en => en.ParentName).GroupBy(en => en.ParentName);

				var stringBuilder = new StringBuilder(300);

				foreach (var notificationGroup in groupedNotifications)
				{
					stringBuilder.AppendLine(notificationGroup.Key + ":");

					foreach (var notificationLine in notificationGroup.Select(n => string.Format(CultureInfo.InvariantCulture, "    {0}: {1}", n.NotificationType.ToString(), n.Message)).Distinct())
					{
						stringBuilder.AppendLine(notificationLine);
					}
				}

				// Remove trailing new line
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Remove(stringBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
				}

				return stringBuilder.ToString();
			}
			else
			{
				return null;
			}
		}

		///<summary>
		/// Generates a tooltip for the specified entity state.
		/// </summary>
		/// <param name="entity"> The network entity.</param>
		/// <param name="state"> The entity state to generate the tooltip for.</param>
		/// <returns>A formatted string containing the tooltip based on the entity state.</returns>
		public static string GetTooltipForEntityState(this INetworkEntity entity, EntityState state)
		{
			if (state.HasFlag(EntityState.HasErrors) || state.HasFlag(EntityState.HasWarnings) || state.HasFlag(EntityState.HasMessages))
			{
				var tooltip = entity.GetTooltipWhenHasNotification();
				if (!string.IsNullOrEmpty(tooltip))
				{
					return tooltip;
				}
			}

			if (state.HasFlag(EntityState.Approved))
			{
				return Res.GetString("14209445-7e5a-428b-bcb9-64141c8946b8", "This shape has been approved");
			}
			else if (state.HasFlag(EntityState.NotApproved))
			{
				return Res.GetString("06a9d1c3-7b62-438b-a89c-2ab13a319022", "This shape has not been approved");
			}
			else if (state.HasFlag(EntityState.Fixed))
			{
				return Res.GetString("822f274f-6d1a-47b0-8eef-425fb18d8529", "This shape's position and size are fixed");
			}

			return null;
		}
	}
}
