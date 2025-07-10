using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZWebBrowser
	{
		#region Windows Forms Designer generated code

		ContextMenuStrip DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse;
		ZToolStripMenuItem CopyMenuItem;
		ZToolStripMenuItem SelectAllMenuItem;
		IContainer components;

		void InitializeComponent()
		{
			this.components = new Container();
			this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.CopyMenuItem = new ZToolStripMenuItem();
			this.SelectAllMenuItem = new ZToolStripMenuItem();
			this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse.SuspendLayout();
			this.SuspendLayout();
			//
			// DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse
			//
			this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse.Items.AddRange(new ToolStripItem[] {
			this.CopyMenuItem,
			this.SelectAllMenuItem });
			this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse.Name = "DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse";
			this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 48, true);
			//
			// CopyMenuItem
			//
			this.CopyMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("1182797F-09F1-4DFA-AA1B-C9CBEE04C6FD", "Copy");
			this.CopyMenuItem.Name = "CopyMenuItem";
			this.CopyMenuItem.ShortcutKeys = (System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C);
			this.CopyMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 22, true);
			this.CopyMenuItem.Click += new EventHandler(this.CopyMenuItem_Click);
			//
			// SelectAllMenuItem
			//
			this.SelectAllMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("039ED84D-699B-4192-96B0-12A4F815959F", "Select All");
			this.SelectAllMenuItem.Name = "SelectAllMenuItem";
			this.SelectAllMenuItem.ShortcutKeys = (System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A);
			this.SelectAllMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 22, true);
			this.SelectAllMenuItem.Click += new EventHandler(this.SelectAllMenuItem_Click);
			//
			// ZWebBrowser
			//
			this.ContextMenuStrip = this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse;
			this.IsWebBrowserContextMenuEnabled = false;
			this.WebBrowserShortcutsEnabled = false;
			this.DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion
	}
}
