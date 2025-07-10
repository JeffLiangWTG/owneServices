using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using CargoWise.Windows.UI.Interop;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Various states and options of a system menu item.
	/// </summary>
	[Flags]
	public enum SystemMenuItemOptions
	{
		/// <summary>
		/// Not checked
		/// </summary>
		Unchecked = 0x00000000,

		/// <summary>
		/// Contains a string as label
		/// </summary>
		String = 0x00000000,

		/// <summary>
		/// Item is disabled
		/// </summary>
		Disabled = 0x00000002,

		/// <summary>
		/// Item is grayed
		/// </summary>
		Grayed = 0x00000001,

		/// <summary>
		/// Item is checked
		/// </summary>
		Checked = 0x00000008,

		/// <summary>
		/// Is a popup menu. Pass the menu handle 
		/// of the popup menu into the ID parameter
		/// </summary>	
		Popup = 0x00000010,

		/// <summary>
		/// Is a bar break
		/// </summary>	
		BarBreak = 0x00000020,

		/// <summary>
		/// Is a break
		/// </summary>	
		Break = 0x00000040,

		/// <summary>
		/// Is identified by the position
		/// </summary>	
		ByPosition = 0x00000400,

		/// <summary>
		/// Is identified by its ID
		/// </summary>	
		ByCommand = 0x00000000,

		/// <summary>
		/// Separator (String and ID parameters are ignored)
		/// </summary>
		Separator = 0x00000800
	}

	/// <summary>
	/// A class that helps to manipulate the system menu
	/// of a passed form.
	/// 
	/// Authors:
	///		Written by Florian Stinglmayr
	///		Extended by Yevhen (mainly usability, relilabilty and conformance to our coding standards)
	/// </summary>
	public class WindowSystemMenu
	{
		public WindowSystemMenu(Form form)
		{
			this.Form = form;
			form.Activated += Form_Activated;
		}

		/// <summary>
		/// Get the form that the system menu is attached to.
		/// </summary>
		public Form Form { get; private set; }

		/// <summary>
		/// When the system menu becomes available to add menu items to.
		/// </summary>
		public event EventHandler Available;

		/// <summary>
		/// Get whether the system menu is available to have menu items added.
		/// Use the Available event to be notified when it becomes available.
		/// </summary>
		public bool IsAvailable()
		{
			return
				Form.IsHandleCreated &&
				Form.ControlBox &&
				GetMenuHandle() != IntPtr.Zero;
		}

		/// <summary>
		/// Resets the system menu to initial state.
		/// </summary>
		public void Reset()
		{
			CheckAvailable();
			GetSystemMenu(Form.Handle, 1);
			clickHandlers.Clear();
		}

		/// <summary>
		/// Inserts the separator at the given position index starting at zero.
		/// </summary>
		/// <param name="position">The pos.</param>
		/// <returns></returns>
		public void InsertSeparator(int position)
		{
			CheckAvailable();
			InsertMenu(position, "", SystemMenuItemOptions.Separator | SystemMenuItemOptions.ByPosition, null);
		}

		/// <summary>
		/// Appends the separator.
		/// </summary>
		/// <returns></returns>
		public void AppendSeparator()
		{
			CheckAvailable();
			AppendMenu("", SystemMenuItemOptions.Separator, null);
		}

		/// <summary>
		/// Inserts the menu assuming that position is relative.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="text">The text.</param>
		/// <param name="handler">The click handler.</param>
		/// <returns><c>true</c> if successuful</returns>
		public void InsertMenu(int position, string text, EventHandler handler)
		{
			CheckAvailable();
			InsertMenu(position, text, SystemMenuItemOptions.ByPosition | SystemMenuItemOptions.String, handler);
		}

		/// <summary>
		/// Inserts the menu according to specified item options.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="options">The options.</param>
		/// <param name="text">The text.</param>
		/// <param name="handler">The click handler.</param>
		/// <returns><c>true</c> if successuful</returns>
		public void InsertMenu(int position, string text, SystemMenuItemOptions options, EventHandler handler)
		{
			CheckAvailable();
			InsertMenu(GetMenuHandle(), position, (int)options, GetID(handler), text);
		}

		/// <summary>
		/// Appends the text menu item.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="handler">The click handler.</param>
		public void AppendMenu(string text, EventHandler handler)
		{
			CheckAvailable();
			AppendMenu(text, SystemMenuItemOptions.String, handler);
		}

		/// <summary>
		/// Appends the menu item according to specified item options.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="options">The options.</param>
		/// <param name="handler">The click handler.</param>
		public void AppendMenu(string text, SystemMenuItemOptions options, EventHandler handler)
		{
			CheckAvailable();
			AppendMenu(GetMenuHandle(), (int)options, GetID(handler), text);
		}

		/// <summary>
		/// Get the number of menu items.
		/// </summary>
		public int MenuItemCount
		{
			get { return GetMenuItemCount(GetMenuHandle()); }
		}

		/// <summary>
		/// Call this method from host form WndProc in order to have click handlers work.
		/// </summary>
		/// <param name="msg">The message.</param>
		public void ProcessWndProc(ref Message msg)
		{
			if (msg.Msg == WindowMessages.WM_SYSCOMMAND)
			{
				int id;
				unchecked
				{
					id = msg.WParam.ToInt32();
				}

				EventHandler handler;

				if (clickHandlers.TryGetValue(id, out handler))
				{
					handler.Invoke(this, EventArgs.Empty);
				}
			}
		}

		#region Interop

		[DllImport("USER32", EntryPoint = "GetSystemMenu", SetLastError = true,
			CharSet = CharSet.Unicode, ExactSpelling = true,
			CallingConvention = CallingConvention.Winapi)]
		static extern IntPtr GetSystemMenu(IntPtr windowHandle, int bReset);

		[DllImport("USER32", EntryPoint = "AppendMenuW", SetLastError = true,
			CharSet = CharSet.Unicode, ExactSpelling = true,
			CallingConvention = CallingConvention.Winapi)]
		static extern int AppendMenu(IntPtr menuHandle, int flags, int newID, string item);

		[DllImport("USER32", EntryPoint = "InsertMenuW", SetLastError = true,
			CharSet = CharSet.Unicode, ExactSpelling = true,
			CallingConvention = CallingConvention.Winapi)]
		static extern int InsertMenu(IntPtr hMenu, int position, int flags, int newId, string item);

		[DllImport("USER32", EntryPoint = "GetMenuItemCount", SetLastError = true,
			CharSet = CharSet.Unicode, ExactSpelling = true,
			CallingConvention = CallingConvention.Winapi)]
		static extern int GetMenuItemCount(IntPtr menuHandle);

		#endregion

		#region Implementation

		readonly Dictionary<int, EventHandler> clickHandlers = new Dictionary<int, EventHandler>();
		int idGenerator = 0x1000;

		int GetID(EventHandler handler)
		{
			idGenerator++;

			if (idGenerator >= 0xF000)
			{
				throw new Exception("Has reached limit of menu items");
			}

			if (handler != null)
			{
				clickHandlers.Add(idGenerator, handler);
			}

			return idGenerator;
		}

		void Form_Activated(object sender, EventArgs e)
		{
			if (IsAvailable())
			{
				if (Available != null)
				{
					Available(this, EventArgs.Empty);
				}
			}
		}

		IntPtr GetMenuHandle()
		{
			return GetSystemMenu(Form.Handle, 0);
		}

		void CheckAvailable()
		{
			if (!IsAvailable())
			{
				throw new Exception("System menu is not available for this form");
			}
		}

		#endregion
	}
}
