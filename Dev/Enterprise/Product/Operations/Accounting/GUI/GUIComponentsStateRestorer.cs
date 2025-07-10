using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;
using MenuItem = System.Windows.Forms.MenuItem;

namespace Enterprise.Accounting.GUI
{
	public interface IGUIComponentsStateRestorer
	{
		void DisableControlsAndMenuItems(ZButton[] buttons, MenuItem[] menuItems);
		void RestoreControlsAndMenuItems(ZButton[] buttons, MenuItem[] menuItems);
		void DisableControlIfApplicableAndUpdateCurrentState(ZButton button, bool newCurrentState);
		void DisableMenuItemIfApplicableAndUpdateCurrentState(MenuItem menuItem, bool newCurrentState);
	}

	public class GUIComponentsStateRestorer : IGUIComponentsStateRestorer
	{
		void IGUIComponentsStateRestorer.DisableControlsAndMenuItems(ZButton[] buttons, MenuItem[] menuItems)
		{
			if (buttons != null)
			{
				foreach (var button in buttons)
				{
					if (button != null && !ComponentStates.ContainsKey(button))
					{
						ComponentStates.Add(button, button.Enabled);
						button.Enabled = false;
					}
				}
			}

			if (menuItems != null)
			{
				foreach (var menuItem in menuItems)
				{
					if (menuItem != null && !ComponentStates.ContainsKey(menuItem))
					{
						ComponentStates.Add(menuItem, menuItem.Enabled);
						menuItem.Enabled = false;
					}
				}
			}
		}

		void IGUIComponentsStateRestorer.RestoreControlsAndMenuItems(ZButton[] buttons, MenuItem[] menuItems)
		{
			if (buttons != null)
			{
				foreach (var button in buttons)
				{
					if (button != null && ComponentStates.ContainsKey(button))
					{
						button.Enabled = ComponentStates.GetValueSafe(button);
						ComponentStates.Remove(button);
					}
				}
			}

			if (menuItems != null)
			{
				foreach (var menuItem in menuItems)
				{
					if (menuItem != null && ComponentStates.ContainsKey(menuItem))
					{
						menuItem.Enabled = ComponentStates.GetValueSafe(menuItem);
						ComponentStates.Remove(menuItem);
					}
				}
			}
		}

		void IGUIComponentsStateRestorer.DisableControlIfApplicableAndUpdateCurrentState(ZButton button, bool newCurrentState)
		{
			if (button != null)
			{
				if (ComponentStates.ContainsKey(button))
				{
					button.Enabled = false;
					ComponentStates.Remove(button);
					ComponentStates.Add(button, newCurrentState);
				}
				else
				{
					button.Enabled = newCurrentState;
				}
			}
		}

		void IGUIComponentsStateRestorer.DisableMenuItemIfApplicableAndUpdateCurrentState(MenuItem menuItem, bool newCurrentState)
		{
			if (menuItem != null)
			{
				if (ComponentStates.ContainsKey(menuItem))
				{
					menuItem.Enabled = false;
					ComponentStates.Remove(menuItem);
					ComponentStates.Add(menuItem, newCurrentState);
				}
				else
				{
					menuItem.Enabled = newCurrentState;
				}
			}
		}

		Dictionary<IComponent, bool> ComponentStates
		{
			get
			{
				if (componentStates == null)
				{
					componentStates = new Dictionary<IComponent, bool>();
				}
				return componentStates;
			}
		}
		Dictionary<IComponent, bool> componentStates;
	}
}
