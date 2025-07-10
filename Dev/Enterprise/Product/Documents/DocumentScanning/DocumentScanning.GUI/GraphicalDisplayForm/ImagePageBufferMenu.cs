using System;
using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public class ImagePageBufferMenu : ContextMenu
	{
		public ImagePageBufferMenu(Menu parentMenu, bool readOnly)
		{
			AddDividerItem(parentMenu);

			cutMenu = new ZMenuItem(ResString.GetMultilingualString("ImagePageBufferMenu|Cut", "Cu&t"));
			cutMenu.Click += new EventHandler(cutMenu_Click);
			cutMenu.Enabled = !readOnly;
			parentMenu.MenuItems.Add(cutMenu);

			copyMenu = new ZMenuItem(ResString.GetMultilingualString("ImagePageBufferMenu|Copy", "&Copy"));
			copyMenu.Click += new EventHandler(copyMenu_Click);
			parentMenu.MenuItems.Add(copyMenu);

			pasteMenu = new ZMenuItem(ResString.GetMultilingualString("ImagePageBufferMenu|Paste", "&Paste"));
			pasteMenu.Click += new EventHandler(pasteMenu_Click);
			pasteMenu.Enabled = !readOnly;
			parentMenu.MenuItems.Add(pasteMenu);

			moveToNewMenu = new ZMenuItem(ResString.GetMultilingualString("ImagePageBufferMenu|MoveToNewDocument", "Move to &New Document"));
			moveToNewMenu.Click += new EventHandler(moveToNewMenu_Click);
			moveToNewMenu.Enabled = !readOnly;
			parentMenu.MenuItems.Add(moveToNewMenu);

			deleteMenu = new ZMenuItem(ResString.GetMultilingualString("ImagePageBufferMenu|Delete","&Delete"));
			deleteMenu.Click += new EventHandler(deleteMenu_Click);
			deleteMenu.Enabled = !readOnly;
			parentMenu.MenuItems.Add(deleteMenu);

			AddDividerItem(parentMenu);

			selectAllMenu = new ZMenuItem(ResString.GetMultilingualString("ImagePageBufferMenu|SelectAll", "Select &All"));
			selectAllMenu.Click += new EventHandler(selectAllMenu_Click);
			parentMenu.MenuItems.Add(selectAllMenu);
		}

		internal void SetMenuItemsReadOnly(PreviewableDocumentType type)
		{
			switch (type)
			{
				case PreviewableDocumentType.NoneFile:
					cutMenu.Enabled = false;
					copyMenu.Enabled = false;
					pasteMenu.Enabled = false;
					moveToNewMenu.Enabled = false;
					deleteMenu.Enabled = false;
					selectAllMenu.Enabled = false;
					break;
				case PreviewableDocumentType.TempFile:
					copyMenu.Enabled = true;
					cutMenu.Enabled = false;
					pasteMenu.Enabled = false;
					moveToNewMenu.Enabled = false;
					deleteMenu.Enabled = false;
					selectAllMenu.Enabled = true;
					break;
				default:
					copyMenu.Enabled = true;
					cutMenu.Enabled = true;
					pasteMenu.Enabled = true;
					moveToNewMenu.Enabled = true;
					deleteMenu.Enabled = true;
					selectAllMenu.Enabled = true;
					break;
			}
		}

		public event EventHandler CutClicked;
		public event EventHandler CopyClicked;
		public event EventHandler PasteClicked;
		public event EventHandler MoveToNewClicked;
		public event EventHandler DeleteClicked;
		public event EventHandler SelectAllClicked;

		#region Implementation
		readonly MenuItem cutMenu;
		readonly MenuItem copyMenu;
		readonly MenuItem pasteMenu;
		readonly MenuItem moveToNewMenu;
		readonly MenuItem deleteMenu;
		readonly MenuItem selectAllMenu;

		void AddDividerItem(Menu parentMenu)
		{
			MenuItem item = new ZMenuItem();
			item.Text = "-";
			parentMenu.MenuItems.Add(item);
		}

		void cutMenu_Click(object sender, EventArgs e)
		{
			if (CutClicked != null)
			{
				CutClicked(sender, e);
			}
		}

		void copyMenu_Click(object sender, EventArgs e)
		{
			if (CopyClicked != null)
			{
				CopyClicked(sender, e);
			}
		}

		void pasteMenu_Click(object sender, EventArgs e)
		{
			if (PasteClicked != null)
			{
				PasteClicked(sender, e);
			}
		}

		void moveToNewMenu_Click(object sender, EventArgs e)
		{
			if (MoveToNewClicked != null)
			{
				MoveToNewClicked(sender, e);
			}
		}

		void deleteMenu_Click(object sender, EventArgs e)
		{
			if (DeleteClicked != null)
			{
				DeleteClicked(sender, e);
			}
		}

		void selectAllMenu_Click(object sender, EventArgs e)
		{
			if (SelectAllClicked != null)
			{
				SelectAllClicked(sender, e);
			}
		}

		#endregion
	}
}
