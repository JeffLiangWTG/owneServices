using System;
using System.Diagnostics;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	/// <summary>
	/// Document View consisting of controls representing Pages of the document
	/// </summary>
	[DebuggerDisplay("Pages = {pagesViewCollection.Count")]
	partial class DocumentView : ZUserControl, IDocumentView
	{
		public DocumentView(IDocumentInfo documentInfo)
		{
			_ = documentInfo ?? throw new ArgumentNullException(nameof(documentInfo));
			InitializeComponent();

			this.watermarkLabel.AllowOverlap(this.pagesLayoutPanel);

			this.presenter = new DocumentViewPresenter(this, documentInfo);
		}

		readonly DocumentViewPresenter presenter;

		public void CreateView()
		{
			presenter.CreateView();
			pagesLayoutPanel.NotifyPagesCreated();
		}

		public void OnZoomChanged(int zoom)
		{
			presenter.NotifyZoomChanged(zoom);
			pagesLayoutPanel.NotifyZoomChanged();
		}

		#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			const int WM_MOUSEWHEEL = 0x020A;

			if (m.Msg == WM_MOUSEWHEEL && !isInTheMiddleOfSendingMessage)
			{
				try
				{
					isInTheMiddleOfSendingMessage = true;
					NativeMethods.SendMessage(pagesLayoutPanel.Handle, (uint)m.Msg, m.WParam, m.LParam);
				}
				finally
				{
					isInTheMiddleOfSendingMessage = false;
				}
			}

			base.WndProc(ref m);
		}

		bool isInTheMiddleOfSendingMessage;

		#endif

		#region IDocumentView members

		public IViewCollection<IPageView> PageViews => pagesViewCollection ?? (pagesViewCollection = new PagesViewCollection(this));
		IViewCollection<IPageView> pagesViewCollection;

		public IViewCollection<INotificationView> NotificationViews => notificationViews ?? (notificationViews = new NotificationsViewCollection(this));
		IViewCollection<INotificationView> notificationViews;

		public void ShowWatermark(string text)
		{
			watermarkLabel.Text = text;
			watermarkLabel.Visible = !string.IsNullOrWhiteSpace(text);
		}

		void IDocumentView.Focus()
		{
			TopLevelControl?.Focus();
		}

		void IDocumentView.Refresh()
		{
			SuspendLayout();

			foreach (var page in PageViews)
			{
				page.Refresh();
			}

			ResumeLayout();
		}

		void IDocumentView.Scale(float scale)
		{
			SuspendLayout();

			foreach (var page in PageViews)
			{
				page.Scale(scale);
			}

			ResumeLayout();
		}

		#endregion

		#region IDisposable implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "presenter")]
		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (!isNotFinalizing)
			{
				pagesViewCollection?.Clear();
				notificationViews?.Clear();
				presenter?.Dispose();
			}
		}

		#endregion
	}
}
