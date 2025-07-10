using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class MenuItemsExtensions
	{
		#region CreateDefaultMessagingMenuItem

		public static IMenuItemDescriptor CreateDefaultMessagingMenuItem(this IMenuItemBuilder menuItemBuilder, IResourceAccessor res, IMessageInstructions messageInstructions)
		{
			if (menuItemBuilder == null
				|| res == null
				|| messageInstructions == null)
			{
				return null;
			}

			var subs = new List<IMenuItemDescriptor>();

			if (messageInstructions.AllowSendMessage)
			{
				subs.Add(menuItemBuilder.Build(CommandIds.SendMessage));
			}

			if (messageInstructions.AllowSendMessageWithdrawal)
			{
				subs.Add(menuItemBuilder.Build(CommandIds.SendWithdrawal));
			}

			if (messageInstructions.AllowResetToOriginal)
			{
				subs.Add(menuItemBuilder.Build(CommandIds.ResetToOriginal));
			}

			switch (subs.Count)
			{
				case 0:
					return null;

				case 1:
					return subs.First();

				default:
					return menuItemBuilder.Build(
						Res.GetString("cada86c9-546f-4f9b-90a0-44a725abc241", "Send Message"),
						res.Get(new Uri(ImageUri.SendMessage)),
						subs.ToArray());
			}
		}

		#endregion

		#region CreateDefaultToolsMenuItem

		public static IMenuItemDescriptor CreateDefaultToolsMenuItem(this IMenuItemBuilder menuItemBuilder, IResourceAccessor res)
		{
			if (menuItemBuilder == null
				|| res == null)
			{
				return null;
			}

			var subs = new IMenuItemDescriptor[]
			{
				menuItemBuilder.Build(CommandIds.ShowMacroEvaluator),
				menuItemBuilder.Build(CommandIds.ShowMessagingData),
				menuItemBuilder.Build(CommandIds.ShowDocumentData),
				menuItemBuilder.Build(CommandIds.ShowOverriddenData)
			}
			.Where(mi => mi != null)
			.ToArray();

			switch (subs.Length)
			{
				case 0:
					return null;

				case 1:
					return subs.First();

				default:
					return menuItemBuilder.Build(
						Res.GetString("f1dbe394-8e58-4372-bc88-6da9410136bd", "Tools"),
						res.Get(new Uri(ImageUri.Tools)),
						subs.ToArray());
			}
		}

		#endregion
	}
}
