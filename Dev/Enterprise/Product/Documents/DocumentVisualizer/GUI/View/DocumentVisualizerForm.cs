using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.DocumentVisualizer.GUI
{
	/// <summary>
	/// Form which contains View for each document in a Pack
	/// Form
	///    DocumentViewEx (for each IDocument)
	///	      Zoom
	///	      StatusBar
	///	      Toolstrip Menu
	///	      DocumentView
	/// </summary>
	[TestExcludeZWinFormHasTypedConstructor]
	partial class DocumentVisualizerForm : ZChildForm
	{
		DocumentVisualizerForm(IDocumentInfoPack pack)
		{
			_ = pack ?? throw new ArgumentNullException(nameof(pack));

			InitializeComponent();

			this.Menu = null;
			this.Text = pack.Name;

			documentViews = CreateDocumentViewInfos(pack);
#if !WINZOR
			mouseHook = new MouseHook();
#endif
		}

		readonly IReadOnlyDictionary<ZTabPage, IDocumentViewEx> documentViews;

#if !WINZOR
		readonly MouseHook mouseHook;
		bool mouseHookInitialized;
		const int ZoomFactor = 10;

		IDocumentViewEx ActiveView
		{
			get
			{
				var activeTab = tabControl.SelectedTab;

				return activeTab != null && documentViews.TryGetValue(activeTab, out var res)
					? res
					: null;
			}
		}
#endif

		#region Implementation

		public static DocumentVisualizerForm CreateView(IDocumentInfoPack pack)
		{
			var form = new DocumentVisualizerForm(pack);
			form.CreateView();

			return form;
		}

		IReadOnlyDictionary<ZTabPage, IDocumentViewEx> CreateDocumentViewInfos(IDocumentInfoPack pack)
		{
			var res = new Dictionary<ZTabPage, IDocumentViewEx>();

			foreach (var documentInfo in pack.DocumentInfos)
			{
				var host = DocumentViewExHost.Create(this, tabControl, out var tabPage);
				this.LabelCaptionRenderProvider.SetLabelCaptionVisible(tabPage, false);

				var documentView = new DocumentViewEx(host, documentInfo);
				res.Add(tabPage, documentView);
			}

			return res;
		}

		void CreateView()
		{
			foreach (var view in documentViews)
			{
				view.Value.CreateView();
			}
		}

#if !WINZOR

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!mouseHookInitialized)
			{
				mouseHook.OnMouseClick += CaptureMouseClick;
				mouseHook.OnMouseWheelUp += CaptureMouseWheelUp;
				mouseHook.OnMouseWheelDown += CaptureMouseWheelDown;
				mouseHookInitialized = true;
			}
		}

#endif

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			foreach (var view in documentViews)
			{
				view.Value.NotifyExiting();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var view in documentViews)
				{
					view.Value.Dispose();
				}

#if !WINZOR

				if (mouseHookInitialized)
				{
					mouseHook.OnMouseClick -= CaptureMouseClick;
					mouseHook.OnMouseWheelUp -= CaptureMouseWheelUp;
					mouseHook.OnMouseWheelDown -= CaptureMouseWheelDown;
				}
				mouseHook?.Dispose();

#endif
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Mouse Hook
#if !WINZOR

		void CaptureMouseClick(object sender, MouseEventArgs args)
		{
			if (HandleMouseClick(args.Location))
			{
				ActiveView?.NotifyMouseDown();
			}
		}

		void CaptureMouseWheelUp(object sender, MouseEventArgs args)
		{
			if (ContainsFocus)
			{
				ActiveView?.IncreaseZoom(ZoomFactor);
			}
		}

		void CaptureMouseWheelDown(object sender, MouseEventArgs args)
		{
			if (ContainsFocus)
			{
				ActiveView?.DecreaseZoom(ZoomFactor);
			}
		}

		bool HandleMouseClick(Point location)
		{
			var control = FindControlAt(location);

			bool IsPageView(Control ctrl) => ctrl is IPageView;

			return control != null
				&& control.TopLevelControl == this
				&& !IsControlOrAnyParent(control, IsPageView);
		}

		bool IsControlOrAnyParent(Control control, Func<Control, bool> condition)
		{
			if (control == null
				|| condition == null)
			{
				return false;
			}

			if (condition(control))
			{
				return true;
			}

			return control.Parent != null
				&& IsControlOrAnyParent(control.Parent, condition);
		}

		Control FindControlAt(Point point)
		{
			var handle = NativeMethods.WindowFromPoint(point);

			if (handle != IntPtr.Zero)
			{
				return Control.FromHandle(handle);
			}

			return null;
		}

#if DEBUG

		public void PerformClickForTest()
		{
			CaptureMouseClick(this, new MouseEventArgs(MouseButtons.Left, 1, this.Location.X, this.Location.Y, 0));
		}

#endif

#endif
		#endregion
	}
}
