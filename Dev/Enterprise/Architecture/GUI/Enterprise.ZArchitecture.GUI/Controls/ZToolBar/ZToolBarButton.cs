using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZToolBarButton : ToolBarButton
	{
		public ZToolBarButton()
		{
		}

		public ZToolBarButton(string caption, IconTypes icon)
		{
			this.Text = caption;
			ImageIndex = Icons.GetImageIndex(icon);
		}

		public ZToolBarButton(string caption, IconTypes icon, ZToolBar.ClickHandler handler)
			: this(caption, icon)
		{
			Click += delegate
			{ handler(); };
		}

		public ZToolBarButton(string caption, IconTypes icon, EventHandler handler)
			: this(caption, icon)
		{
			Click += handler;
		}

		IconTypes activeIconType = IconTypes.None;
		[DefaultValue(IconTypes.None)]
		public IconTypes ActiveIconType
		{
			get => activeIconType;
			set
			{
				activeIconType = value;
				OnActiveIconTypeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler Click;
		public event EventHandler DropDown;
		public event EventHandler OnVisibleChanged;
		public event EventHandler OnActiveIconTypeChanged;

		public void PerformClick()
		{
			OnClick(EventArgs.Empty);
		}

		internal void PerformDropDown()
		{
			OnDropDown(EventArgs.Empty);
		}

		protected virtual void OnClick(EventArgs e)
		{
			if (Click != null)
			{
				Click(this, e);
			}
		}

		protected virtual void OnDropDown(EventArgs e)
		{
			if (DropDownMenu != null)
			{
				// the Popup event is not fired for MenuItems on a ToolBarButton
				FireMenuItemsPopupEvent(DropDownMenu.MenuItems);
			}

			if (DropDown != null)
			{
				DropDown(this, e);
			}
		}

		public new bool Visible
		{
			get => base.Visible;
			set
			{
				if (Visible != value)
				{
					base.Visible = value;
					if (OnVisibleChanged != null)
					{
						OnVisibleChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		#region Implementation

		void FireMenuItemsPopupEvent(IEnumerable menuItems)
		{
			foreach (MenuItem menuItem in menuItems)
			{
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { EventArgs.Empty });
				FireMenuItemsPopupEvent(menuItem.MenuItems);
			}
		}

		#endregion
	}
}

