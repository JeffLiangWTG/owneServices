using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class UserInterface
	{
		public UserInterface(IServiceContainer services)
		{
			Argument.NotNull(services, nameof(services));

			this.services = services;
		}

		readonly IServiceContainer services;

		#region Menu Items

		[MacroInvokable]
		public IMenuItemDescriptor CreateMenuItem(string caption, object image, object[] subItems)
		{
			if (string.IsNullOrEmpty(caption))
			{
				throw new MacroRuntimeException("You must specify menu caption.");
			}

			if (subItems == null)
			{
				throw new MacroRuntimeException("You must specify sub menu items.");
			}

			return new MenuItemDescriptor(caption, image, GetMenuItems(subItems).ToArray());
		}

		[MacroInvokable]
		public IMenuItemDescriptor CreateMenuItem(ICommand command)
		{
			if (command == null)
			{
				throw new MacroRuntimeException("You must specify command.");
			}

			return new MenuItemDescriptorFromCommand(services.Resolve<IResourceAccessor>(), command);
		}

		[MacroInvokable]
		public MenuItemDescriptor CreateMenuItem(string caption, object image, MacroClosure action, MacroClosure isActive)
		{
			if (string.IsNullOrEmpty(caption))
			{
				throw new MacroRuntimeException("You must specify menu caption.");
			}

			if (action == null)
			{
				throw new MacroRuntimeException("You must specify click action.");
			}

			if (isActive == null)
			{
				throw new MacroRuntimeException("You must specify active handler.");
			}

			Action invoker = () =>
			{
				using (var scope = new MacroScope())
				{
					action.Invoke(scope);
				}
			};

			Func<bool> isEnabledRule = () =>
			{
				using (var scope = new MacroScope())
				{
					return Convert.ToBoolean(isActive.Invoke(scope), CultureInfo.InvariantCulture);
				}
			};

			return new MenuItemDescriptor(caption, image, invoker, isEnabledRule);
		}

		[MacroInvokable]
		public void SetMenu(IEnumerable<object> items)
		{
			if (items == null)
			{
				throw new MacroRuntimeException("You pass menu items you want to set.");
			}

			var descriptors = GetMenuItems(items).ToArray();

			if (descriptors.Any())
			{
				services.Resolve<IEventBroker>().Publish(new CreateMenuEvent(descriptors));
			}
		}

		IEnumerable<IMenuItemDescriptor> GetMenuItems(IEnumerable<object> items)
		{
			foreach (var item in items)
			{
				switch (item)
				{
					case IMenuItemDescriptor desc:
						yield return desc;
						break;

					case ICommand command:
						yield return CreateMenuItem(command);
						break;
				}
			}
		}

		#endregion

		#region User Messaging

		[MacroInvokable]
		public void ShowMessage(string message, string caption = "")
		{
			var notifications = services.Resolve<IUserNotificationService>();

			notifications.ShowMessage(message, caption);
		}

		[MacroInvokable]
		public bool ShowConfirmation(string message, string caption = "")
		{
			var notifications = services.Resolve<IUserNotificationService>();

			return notifications.ShowConfirmation(message, caption);
		}

		#endregion
	}
}
