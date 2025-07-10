using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	/// <summary>
	/// Extension of Document View containing the document pages (Document View) and additional controls like Zoom, Status Panel and Toolbar
	/// DocumentViewEx
	///	   Zoom
	///	   StatusBar
	///	   Toolstrip Menu
	///	   DocumentView
	/// </summary>
	partial class DocumentViewEx : ZUserControl, IDocumentViewEx
	{
		public DocumentViewEx(IDocumentViewExHost host, IDocumentInfo documentInfo)
		{
			try
			{
				InitializeComponent();

				this.host = host ?? throw new ArgumentNullException(nameof(host));
				this.presenter = new DocumentViewExPresenter(this, documentInfo);

				this.documentView = new DocumentView(documentInfo)
				{
					Dock = DockStyle.Fill,
					Name = nameof(documentView)
				};

				mainPanel.Controls.Add(documentView);
				#if !WINZOR
				toolStrip.Renderer = new BorderlessToolStripRenderer();
				#endif
				notificationPanel.Visible = false;
				notificationPanel.BackColor = ColorSchema.Notifications;
				statusPanel.BackColor = ColorSchema.StatusBarBackground;
				statusLabel.ForeColor = ColorSchema.StatusBarText;
				TypeDescriptor.AddAttributes(statusLabel, new SuppressControlRequiresTextBasherAttribute());

				settings = new DocumentSettings();
				settings.ZoomInfo.ValueChanged += (s, e) =>
				{
					documentView.OnZoomChanged(settings.Zoom);
				};

				this.SetDataBinding(settings, "");
			}
			catch (Exception ex)
			{
				var error = FormattableString.Invariant($@"
Exception message: {ex.GetFullMessage()}
StackTrace: {ex.StackTrace}");
				ErrorReporter.ReportOnce("Error initializing DocumentViewEx, which will cause the finalize() of DocumentViewEx instance will be called when Garbage collector is working.", error);
				Dispose();
				throw;
			}
		}

		readonly DocumentSettings settings;
		readonly DocumentView documentView;
		readonly DocumentViewExPresenter presenter;
		readonly IDocumentViewExHost host;

		public override void Refresh()
		{
			((IDocumentView)documentView)?.Refresh();
			base.Refresh();
		}

		#region IDocumentViewEx

		string IDocumentViewEx.Text
		{
			get => host.Text;
			set => host.Text = value;
		}

		IMenuItemCollection IDocumentViewEx.MenuItems => menuItems ?? (menuItems = new ToolStripAdapter(toolStrip));
		IMenuItemCollection menuItems;

		void IDocumentViewEx.IncreaseZoom(int factor)
		{
			settings.Zoom += factor;
		}

		void IDocumentViewEx.DecreaseZoom(int factor)
		{
			settings.Zoom -= factor;
		}

		void IDocumentViewEx.CreateView()
		{
			documentView.CreateView();
			host.ShowDocumentViewEx(this);
		}

		void IDocumentViewEx.ToggleStatusPanelVisibility(bool visible)
		{
			statusPanel.Visible = visible;
		}

		void IDocumentViewEx.SetStatusPanelCaption(string caption)
		{
			statusLabel.Text = caption;
		}

		void IDocumentViewEx.ShowLogsView(IStmALogParent logParent)
		{
			host.ShowLogsView(logParent);
		}

		void IDocumentViewEx.ShowWatermark(string text) => documentView.ShowWatermark(text);

		void IDocumentViewEx.NotifyMouseDown() => presenter.NotifyMouseDown();
		void IDocumentViewEx.NotifyExiting() => presenter.NotifyExiting();

		void IDocumentViewEx.Refresh() => Refresh();
		void IDocumentViewEx.Focus() => host.Focus();
		void IDocumentViewEx.Exit() => host.Close();

		#endregion
	}
}
