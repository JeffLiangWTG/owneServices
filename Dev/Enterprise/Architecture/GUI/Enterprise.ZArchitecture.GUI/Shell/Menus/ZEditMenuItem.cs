using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.Core.Forms
{
	public class ZEditMenuItem : ZMenuItem
	{
		public ZEditMenuItem(Form form)
			: base(ResString.GetMultilingualString("16ee8925-8e37-4e32-aad3-2b7e4e895354", "&Edit"))
		{
			parentForm = form;

			cutMenuItem = new ZMenuItem(CutMenuItemCaption);
			cutMenuItem.Shortcut = Shortcut.CtrlX;
			cutMenuItem.Click += new EventHandler(CutMenuItem_Click);
			MenuItems.Add(cutMenuItem);

			copyMenuItem = new ZMenuItem(CopyMenuItemCaption);
			copyMenuItem.Shortcut = Shortcut.CtrlC;
			copyMenuItem.Click += new EventHandler(CopyMenuItem_Click);
			MenuItems.Add(copyMenuItem);

			pasteMenuItem = new ZMenuItem(PasteMenuItemCaption);
			pasteMenuItem.Shortcut = Shortcut.CtrlP;
			pasteMenuItem.Click += new EventHandler(PasteMenuItem_Click);
			MenuItems.Add(pasteMenuItem);
		}

		#region Menu Item Captions

		public static MultilingualString CutMenuItemCaption
		{
			get { return ResString.GetMultilingualString("Edit.Cut", "Cut"); }
		}

		public static MultilingualString CopyMenuItemCaption
		{
			get { return ResString.GetMultilingualString("Edit.Copy", "Copy"); }
		}

		public static MultilingualString PasteMenuItemCaption
		{
			get { return ResString.GetMultilingualString("Edit.Paste", "Paste"); }
		}

		#endregion

		#region Click Event Handlers

		void SendMessageToActiveChildControl(Form form, int message)
		{
			var activeChildControl = form.GetFrontMostActiveControl();
			if (activeChildControl != null && !activeChildControl.IsDisposed)
			{
				UnsafeNativeMethods.PostMessage(new HandleRef(activeChildControl, activeChildControl.Handle), message, IntPtr.Zero, IntPtr.Zero);
			}
		}

		void CutMenuItem_Click(object sender, EventArgs e)
		{
			SendMessageToActiveChildControl(parentForm, WindowsMessage.WM_CUT);
		}

		void CopyMenuItem_Click(object sender, EventArgs e)
		{
			SendMessageToActiveChildControl(parentForm, WindowsMessage.WM_COPY);
		}

		void PasteMenuItem_Click(object sender, EventArgs e)
		{
			SendMessageToActiveChildControl(parentForm, WindowsMessage.WM_PASTE);
		}

		readonly Form parentForm;
#if DEBUG
		internal
#endif
		readonly MenuItem cutMenuItem;
#if DEBUG
		internal
#endif
		readonly MenuItem copyMenuItem;
#if DEBUG
		internal
#endif
		readonly MenuItem pasteMenuItem;

		#endregion
	}
}
