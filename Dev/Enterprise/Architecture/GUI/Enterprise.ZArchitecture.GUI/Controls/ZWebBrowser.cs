using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(true)]
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ZWebBrowser : WebBrowser, IWebNavigate
	{
		public ZWebBrowser()
			: base()
		{
			InitializeComponent();
			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this);

			DocumentCompleted += ZWebBrowser_DocumentCompleted;
			NewWindow += ZWebBrowser_NewWindow;
		}

		void ZWebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			// WebBrowser does something weird to the focus of the form on DocumentCompleted.. tell form to refocus itself
			var form = FindForm();
			if (form != null)
			{
				form.Focus();
			}
		}

		void ZWebBrowser_NewWindow(object sender, CancelEventArgs e)
		{
			e.Cancel = true;

			//open new window link in WebBrowser itself
			Navigate(StatusText);
		}

		#region New (overidden) Properties

		[Category("Properties"), Description("Indicates whether to use the context menu of the native browser.")]
		[DefaultValue(false)]
		public new bool IsWebBrowserContextMenuEnabled
		{
			get { return base.IsWebBrowserContextMenuEnabled; }
			set
			{
				if (IsWebBrowserContextMenuEnabled != value)
				{
					base.IsWebBrowserContextMenuEnabled = value;
				}
			}
		}

		[Category("Properties"), Description("Specifies whether some accelerator keys are enabled in the WebBrowser control.")]
		[DefaultValue(false)]
		public new bool WebBrowserShortcutsEnabled
		{
			get { return base.WebBrowserShortcutsEnabled; }
			set
			{
				if (WebBrowserShortcutsEnabled != value)
				{
					base.WebBrowserShortcutsEnabled = value;
				}
			}
		}

		#endregion

		#region Shortcut Keys when WebBrowserShortcutsEnabled = false
		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			var isRemoteSession = ObjectFactory.Get<TerminalService>().IsRemoteAppSession;
			if (isRemoteSession && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
			{
				e.IsInputKey = true;
			}

			if (!WebBrowserShortcutsEnabled)
			{
				switch (e.KeyCode)
				{
					case Keys.A:
						if (e.Control)
						{
							SelectAll();
						}
						break;
					case Keys.C:
						if (e.Control)
						{
							CopyToClipboard();
						}
						break;
					case Keys.Insert:
						if (e.Control)
						{
							CopyToClipboard();
						}
						break;
				}
			}
		}

		public void SelectAll()
		{
			SelectAllCore();
		}

#if DEBUG
		protected virtual
#endif
 void SelectAllCore()
		{
			Document.ExecCommand("SelectAll", false, null);
		}

		public void CopyToClipboard()
		{
			CopyToClipboardCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Command name")]
#if DEBUG
		protected virtual
#endif
 void CopyToClipboardCore()
		{
			var doc = Document;
			if (doc != null)
			{
				doc.ExecCommand("copy", false, null);
			}
		}

		void OnSelectAll_Click(object sender, EventArgs e)
		{
			SelectAll();
		}

		void OnCopy_Click(object sender, EventArgs e)
		{
			CopyToClipboard();
		}

		public override bool PreProcessMessage(ref Message msg)
		{
			return base.PreProcessMessage(ref msg) && (FindForm()?.PreProcessMessage(ref msg) ?? true);
		}

		#endregion

		#region Context Menus

		/// <summary>
		/// Resets the ContextMenuStrip of this ZWebBrowser to the default. Does not take
		/// into account any modifications to the ContextMenuStrip object itself.
		/// </summary>
		public void ResetContextMenuStripToDefault()
		{
			ContextMenuStrip = DefaultContextMenuStripWhenIsWebBrowserContextMenuEnabledIsFalse;
		}

		void CopyMenuItem_Click(object sender, EventArgs e)
		{
			CopyToClipboard();
		}

		void SelectAllMenuItem_Click(object sender, EventArgs e)
		{
			SelectAll();
		}

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZWebBrowser>()
				.Result;
		}

		#endregion

		#region Url

		public new virtual Uri Url
		{
			get { return base.Url; }
			set { base.Url = value; }
		}

		#endregion

		#region Navigate

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new virtual bool IsBusy => base.IsBusy;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1084:DoNotUseVirtualNew", Justification = "Baseline")]
		public new virtual void Navigate(string urlString)
		{
			base.Navigate(urlString);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				try
				{
					Url = null;
				}
				catch (COMException)
				{
					//we tried, continue with Dispose
				}
			}
			base.Dispose(disposing);
			if (disposing)
			{
				CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		#endregion
	}
}
