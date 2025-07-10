using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZEditMenuItem : ZMenuItem
	{
		readonly Form parentForm;
		readonly MenuItem cutMenuItem;
		readonly MenuItem copyMenuItem;
		readonly MenuItem pasteMenuItem;

		public ZEditMenuItem(Form form) : base(ResString.GetMultilingualString("16ee8925-8e37-4e32-aad3-2b7e4e895354", "&Edit"))
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

		delegate Task AsyncEventHandler(object sender, EventArgs e);

		void CutMenuItem_Click(object sender, EventArgs e)
		{
			InvokeOnForm(parentForm, async () => await (parentForm.GetJSInterop<IClipboardJSInterop>()?.CutAsync() ?? Task.CompletedTask));
		}

		void CopyMenuItem_Click(object sender, EventArgs e)
		{
			if (parentForm.GetFrontMostActiveControl() is ZGrid zGrid)
			{
				zGrid.MenuCopy();
			}
			else
			{
				InvokeOnForm(parentForm, async () => await (parentForm.GetJSInterop<IClipboardJSInterop>()?.CopyAsync() ?? Task.CompletedTask));
			}
		}

		void PasteMenuItem_Click(object sender, EventArgs e)
		{
			InvokeOnForm(parentForm, async () => await (parentForm.GetJSInterop<IClipboardJSInterop>()?.PasteAsync() ?? Task.CompletedTask));
		}

		public static MultilingualString CutMenuItemCaption => ResString.GetMultilingualString("Edit.Cut", "Cut");

		public static MultilingualString CopyMenuItemCaption => ResString.GetMultilingualString("Edit.Copy", "Copy");

		public static MultilingualString PasteMenuItemCaption => ResString.GetMultilingualString("Edit.Paste", "Paste");
	}
}
