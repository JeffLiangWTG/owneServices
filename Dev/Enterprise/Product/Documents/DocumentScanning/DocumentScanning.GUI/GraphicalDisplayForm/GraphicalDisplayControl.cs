using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Thumbnails;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class GraphicalDisplayControl : ZUserControl, IGraphicalDisplay, IImagePageSelectorProvider
	{
#if DEBUG

		public IPreviewableDocument PreviewableDocument => imageProvider.Document;

#endif

		internal PreviewableDocumentImageReader imageProvider;

		internal MagnifyManager MagnifyManager { get; private set; }

		internal ImagePageBufferMenu thumbnailEditMenu;

		bool ImageShowing =>
			imageHasBeenShown
			&& Height > 0
			&& Width > 0;

		bool imageHasBeenShown;
		bool readOnly;

		[DefaultValue(true), Category, Description("True to enable the rotate buttons")]
		public bool AllowRotate
		{
			get => RotateRight.Enabled && RotateLeft.Enabled;
			set => RotateRight.Enabled = RotateLeft.Enabled = value;
		}

		public GraphicalDisplayControl() : this(false)
		{ }

		public GraphicalDisplayControl(bool readOnly)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			InitializeComponent();
			SetupToolTips();
			ReadOnly = readOnly;
			SetupThumbnailContextMenu(readOnly);

			rulerPanel.ShowLines = false;
		}

		void SetupToolTips()
		{
			RotateRight.ToolTipText = Res.GetString("41f3a43c-ff28-4e17-8813-5eb0f18a18e7", "Rotate the Image Clockwise");
			RotateLeft.ToolTipText = Res.GetString("63c5fa3b-3c2c-4d6b-a725-5118fea70cfe", "Rotate the Image Anti-Clockwise");
			ToggleThumbnailToolbarButton.ToolTipText = Res.GetString("b65d3328-443e-4eaa-9d8b-69cbe4831ede", "Display the eDoc as Thumbnails");
			FullScreen.ToolTipText = Res.GetString("16e5cb9c-7d4e-4c85-ba5c-b43ff7f6e2d1", "Full Screen Display");
			ToggleRulers.ToolTipText = Res.GetString("3470B405-4739-4F8C-AB31-395DB2FD9C37", "Toggle Displaying Rulers");
		}

		void SetupThumbnailContextMenu(bool isReadOnly)
		{
			var sizingContextMenu = new ThumbNailSizeContextMenu();
			sizingContextMenu.MenuItemClicked += SizingContextMenu_MenuItemClicked;
			ThumbnailPanel.ContextMenu = sizingContextMenu;
			ThumbnailPanel.SizeChanged += ThumbnailPanel_SizeChanged;

			thumbnailEditMenu = new ImagePageBufferMenu(ThumbnailPanel.ContextMenu, isReadOnly);
			thumbnailEditMenu.CutClicked += ThumbnailEditMenu_CutClicked;
			thumbnailEditMenu.CopyClicked += ThumbnailEditMenu_CopyClicked;
			thumbnailEditMenu.PasteClicked += ThumbnailEditMenu_PasteClicked;
			thumbnailEditMenu.MoveToNewClicked += ThumbnailEditMenu_MoveToNewClicked;
			thumbnailEditMenu.DeleteClicked += ThumbnailEditMenu_DeleteClicked;
			thumbnailEditMenu.SelectAllClicked += ThumbnailEditMenu_SelectAllClicked;
		}

		internal void ThumbnailPanel_SizeChanged(object sender, EventArgs e)
		{
			if (Size != lastSize)
			{
				lastSize = Size;
				RefreshManagerThumbnails.RegisterForceRefresh();
				UpdateThumbnailView();
			}
		}
		Size lastSize;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "components", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/291")]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				CleanUp();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void CleanUp()
		{
			if (ParentForm != null)
			{
				ParentForm.Closed -= GraphicalDisplayForm_Closed;
				ParentForm.Resize -= GraphicalDisplayControl_Resize;
			}

			if (thumbNailDrawer != null)
			{
				thumbNailDrawer.SelectedPageChanged -= ThumbNailDrawer_SelectedPageChanged;
				thumbNailDrawer.ThumbnailReorderDragDrop -= ThumbNailDrawer_ThumbnailReorderDragDrop;
				thumbNailDrawer.PageDelete -= ThumbNailDrawer_PageDelete;
				thumbNailDrawer.PageDropped -= OnPageDropped;
				thumbNailDrawer.PageInsert -= OnPageInsert;
				thumbNailDrawer.Dispose();
				thumbNailDrawer = null;
			}

			if (MagnifyManager != null)
			{
				MagnifyManager.Dispose();
				MagnifyManager = null;
			}

			if (ThumbnailPanel.ContextMenu != null)
			{
				ThumbnailPanel.ContextMenu.Dispose();
			}

			if (imageProvider != null)
			{
				imageProvider.Dispose();
				imageProvider = null;
			}
			if (ThumbnailPanel != null)
			{
				ThumbnailPanel.SizeChanged -= ThumbnailPanel_SizeChanged;
			}
		}

		public bool ReadOnly
		{
			get
			{
				return readOnly;
			}
			set
			{
				if (readOnly == value)
				{
					return;
				}
				readOnly = value;
				RotateLeft.Enabled = !readOnly;
				RotateRight.Enabled = !readOnly;
				ThumbNailDrawer.ReadOnly = readOnly;
			}
		}

		#region Related Objects

		// future-proofs these Properties with a makeshift null-coalescing-assignment feature available in C# 8.0
		internal static T NCA<T>(ref T t, Func<T> tConstructor) => t == null ? t = tConstructor.Invoke() : t; // cstr should be non-nullable

		internal static T NCA<T>(ref T t) where T : new() => NCA(ref t, () => new T());

		RefreshManager RefreshManagerThumbnails => NCA(ref refreshManagerThumbnails);
		RefreshManager refreshManagerThumbnails;

		RefreshManager RefreshManagerSinglePage => NCA(ref refreshManagerSinglePage);
		RefreshManager refreshManagerSinglePage;

		internal ThumbNailDrawer ThumbNailDrawer => NCA(ref thumbNailDrawer, () =>
		{
			var tnd = GetNewThumbNailDrawer(ThumbnailPanel);
			tnd.Document = Document;
			tnd.ThumbnailReorderDragDrop += ThumbNailDrawer_ThumbnailReorderDragDrop;
			tnd.PageDelete += ThumbNailDrawer_PageDelete;
			tnd.PageDropped += OnPageDropped;
			tnd.PageInsert += OnPageInsert;
			tnd.SelectedPageChanged += ThumbNailDrawer_SelectedPageChanged;
			return tnd;
		});
		ThumbNailDrawer thumbNailDrawer;

		protected virtual ThumbNailDrawer GetNewThumbNailDrawer(Panel thumbPanel) => new ThumbNailDrawer(thumbPanel);

		SinglePagePreview SinglePagePreview => NCA(ref singlePagePreview, () => GetNewSinglePagePreview(DocumentPreviewPictureBox, ImagePanel));
		SinglePagePreview singlePagePreview;

		protected virtual SinglePagePreview GetNewSinglePagePreview(PictureBox previewPictureBox, Panel imagePanel) => new SinglePagePreview(previewPictureBox, imagePanel);

		#region Document

		public StorageDocsBase Document
		{
			get => document;
			set
			{
				document = value;
				if (ThumbNailDrawer != null)
				{
					ThumbNailDrawer.Document = value;
				}
			}
		}

		StorageDocsBase document;

		#endregion

		#endregion

		int[] SelectedIndexes => ThumbnailView
			? ThumbNailDrawer.SelectedPageIndexes
			: new[] { PageSelectorControl.CurrentPageIndex };

		#region Image Manipulation (Cut/Paste/Copy etc)

		void ThumbnailEditMenu_SelectAllClicked(object sender, EventArgs e) => ThumbNailDrawer.SelectAllPages();

		void ThumbnailEditMenu_DeleteClicked(object sender, EventArgs e) => OnPageDelete(SelectedIndexes);

		void ThumbnailEditMenu_CopyClicked(object sender, EventArgs e) => OnPageCopy(SelectedIndexes);

		void ThumbnailEditMenu_CutClicked(object sender, EventArgs e) => OnPageCut(SelectedIndexes);

		void ThumbnailEditMenu_PasteClicked(object sender, EventArgs e) => OnPagePaste(PageSelectorControl.CurrentPageIndex);

		void ThumbnailEditMenu_MoveToNewClicked(object sender, EventArgs e) => OnMoveToNew(SelectedIndexes);

		void OnPageDelete(int[] pagesToDelete)
		{
			Cursor.Current = Cursors.WaitCursor;
			PageDelete?.Invoke(this, new PageDeleteEventArgs(pagesToDelete));
			Cursor.Current = Cursors.Default;
		}

		void OnPageCopy(int[] pagesToCopy)
		{
			Cursor.Current = Cursors.WaitCursor;
			PageCopy?.Invoke(this, new PageCopyEventArgs(pagesToCopy));
			Cursor.Current = Cursors.Default;
		}

		void OnPageCut(int[] pagesToCut)
		{
			Cursor.Current = Cursors.WaitCursor;
			PageCut?.Invoke(this, new PageCutEventArgs(pagesToCut));
			Cursor.Current = Cursors.Default;
		}

		void OnPagePaste(int locationToPaste)
		{
			Cursor.Current = Cursors.WaitCursor;
			PagePaste?.Invoke(this, new PagePasteEventArgs(locationToPaste));
			Cursor.Current = Cursors.Default;
		}

#if DEBUG
		internal
#endif
		void RotateSelectedPages(bool clockwise)
		{
			Cursor.Current = Cursors.WaitCursor;
			var selectedPages = SelectedIndexes;
			try
			{
				if (Rotation != null)
				{
					foreach (var pageNumber in selectedPages)
					{
						Rotation(this, new RotateEventArgs(clockwise, pageNumber));
					}
				}

				if (selectedPages.Length > 0)
				{
					PageSelectorControl.SetPageNumbericUpDownValue(selectedPages[0] + 1);
				}
			}
			catch (ExternalException ex)
			{
				ReportGraphicsDisplayFailure(ex);
			}

			Cursor.Current = Cursors.Default;
		}

		void OnMoveToNew(int[] pagesToMove)
		{
			Cursor.Current = Cursors.WaitCursor;
			MoveToNewDocument?.Invoke(this, new PageCutEventArgs(pagesToMove, true));
			Cursor.Current = Cursors.Default;
		}

		void OnPageDropped(object sender, PageDroppedEventArgs e)
		{
			if (!ReadOnly)
			{
				PageDropped?.Invoke(this, e);
			}
		}

		void OnPageInsert(object sender, PageInsertEventArgs e)
		{
			if (!ReadOnly)
			{
				PageInsert?.Invoke(this, e);
			}
		}

		#endregion

		#region Implementation

		internal bool ThumbnailView;

		internal void UpdateSingleImageView()
		{
			if (imageProvider == null || MagnifyManager == null)
			{
				SinglePagePreview.ClearPreview();
			}
			else
			{
				if (RefreshManagerSinglePage.ForceRefresh || RefreshManagerSinglePage.PageChanged)
				{
					RefreshManagerSinglePage.RegisterViewUpdated();
					RefreshManagerSinglePage.PageChanged = false;
				}

				if (RefreshManagerSinglePage.RefreshMagnifyingGlass || RefreshManagerSinglePage.ParentResized)
				{
					SinglePagePreview.Preview(imageProvider.PageSelector);
					MagnifyManager.UpdatePictureInMagnificationWindow();
					RefreshManagerSinglePage.RefreshMagnifyingGlass = false;
					RefreshManagerSinglePage.ParentResized = false;
				}
			}
		}

		internal void UpdateThumbnailView()
		{
			if (imageProvider == null)
			{
				ThumbNailDrawer.Dispose();
				return;
			}
			else if (MagnifyManager == null)
			{
				return;
			}

			if (imageProvider.Document.IsDocumentCorrupted)
			{
				return;
			}

			if (RefreshManagerThumbnails.ForceRefresh || RefreshManagerThumbnails.ParentResized)
			{
				RefreshManagerThumbnails.RegisterViewUpdated();

				if (ThumbnailPanel.ContextMenu != null)
				{
					ThumbNailDrawer.SetRequestedNumberOfImagesPerRow(imageProvider.PageSelector.TotalPages, ((ThumbNailSizeContextMenu)ThumbnailPanel.ContextMenu).NumberOfThumbnailsPerRow);
				}

				try
				{
					ThumbNailDrawer.DrawThumbNails(imageProvider.PageSelector, MagnifyManager);
				}
				catch (CorruptedDocumentException)
				{
					Globals.Message.ShowError(Res.GetString("44a0c82d-6c1c-4aea-8959-65a40a51260d", "The document is corrupted, system preview could not be created."));
				}

				RefreshManagerThumbnails.PageChanged = true;
			}

			if (RefreshManagerThumbnails.PageChanged)
			{
				ThumbNailDrawer.SelectedPageIndex = imageProvider.PageSelector.CurrentPageIndex;
				if (RefreshManagerThumbnails.ForceThumbnailScrollIntoView)
				{
					ThumbNailDrawer.ScrollCurrentPageIntoViewIfRequired();
				}
				RefreshManagerThumbnails.PageChanged = false;
			}

			RefreshManagerThumbnails.ForceThumbnailScrollIntoView = false;

			if (RefreshManagerThumbnails.RefreshMagnifyingGlass)
			{
				if (RefreshManagerSinglePage.ForceRefresh || RefreshManagerSinglePage.PageChanged)
				{
					RefreshManagerSinglePage.PageChanged = false;
				}
				MagnifyManager.UpdatePictureInMagnificationWindow();
				RefreshManagerThumbnails.RefreshMagnifyingGlass = false;
			}
		}

		internal void UpdatePreviewImage(bool isThumbnailView)
		{
			Enabled = ImageShowing;

			if (ImageShowing)
			{
				try
				{
					if (isThumbnailView)
					{
						UpdateThumbnailView();
					}
					else
					{
						UpdateSingleImageView();
					}
				}
				catch (CorruptedDocumentException)
				{
					Globals.Message.ShowError(Res.GetString("44a0c82d-6c1c-4aea-8959-65a40a51260d", "The document is corrupted, system preview could not be created."));
				}
				catch (Exception ex)
				{
					if (ex is ImageFormatException
						|| ex is OutOfMemoryException
						|| ex is ArgumentException
						|| ex is ExternalException)
					{
						Globals.Message.Show(Res.GetString("038BC480-575C-4C75-991A-52D388CDBCF2", "There were not enough resources to show this preview. {0}", GetOutOfMemoryMessage()));
					}
					else
					{
						throw;
					}
				}
			}
		}

		string GetOutOfMemoryMessage()
		{
			var extraMessage = string.Empty;

			if (PageSelector != null && PageSelector.CurrentImage != null)
			{
				if (PageSelector.CurrentImage.HorizontalResolution > 200 || PageSelector.CurrentImage.VerticalResolution > 200)
				{
					extraMessage += "\t- " + Res.GetString("E5E00FDB-122F-4DA8-967B-6B2E304538AD", "the resolution of this image is {0} x {1}, outside the recommended 200 x 200 dpi.",
						PageSelector.CurrentImage.HorizontalResolution,
						PageSelector.CurrentImage.VerticalResolution) + System.Environment.NewLine;
				}

				if (PageSelector.CurrentImage.PixelFormat != PixelFormat.Format1bppIndexed)
				{
					extraMessage += "\t- " + Res.GetString("9FBB8F5A-ACC2-48AA-B842-FC9966B9624D", "the color depth of this image is not the recommended Black & White (1 bit per pixel).") + System.Environment.NewLine;
				}

				if (!string.IsNullOrEmpty(extraMessage))
				{
					extraMessage = Res.GetString("D9BD2BD6-9F98-4856-B5C7-9AB1F46DBE88", "The system has detected: \r\n{0}\r\nScanning your documents at 200x200 dpi with Black and White color depth will minimize system resource usage and produce faster previews.", extraMessage);
				}
			}

			return extraMessage;
		}

		void SaveThumbnailVisibleSettingIfNeeded(bool thumbNailViewActive)
		{
			var settings = Env.Registry.DMThumbnailSettings;
			if (settings.ThumbNailViewActive != thumbNailViewActive)
			{
				settings.ThumbNailViewActive = thumbNailViewActive;
				Env.Registry.DMThumbnailSettings = settings;
			}
		}

		public void SetThumbnailVisible(bool isVisible)
		{
			ImagePanel.Visible = !isVisible;
			ThumbnailPanel.Visible = isVisible;
			ThumbnailView = isVisible;
			UpdatePreviewImage(isVisible);
			SaveThumbnailVisibleSettingIfNeeded(isVisible);
			UpdateButtonIcon(ToggleThumbnailToolbarButton, isVisible);
		}

		void FullScreenView()
		{
			if (imageProvider == null)
			{
				return;
			}

			var fullScreenForm = new DocumentFullScreenPreviewForm
			{
				DocumentImage = imageProvider.PageSelector.CurrentImage
			};
			ZFormModaliser.Show(fullScreenForm, ParentForm);
		}

		#region Event Handling

		// Changed the number of thumbnails displayed in each row across the panel, so update the view
		void SizingContextMenu_MenuItemClicked(object sender, EventArgs e)
		{
			RefreshManagerThumbnails.RegisterForceRefresh();
			UpdatePreviewImage(ThumbnailView);
		}

		void ThumbNailDrawer_ThumbnailReorderDragDrop(object sender, ThumbnailsReorderEventHandlerArgs e) => PageReorder(this, new PageReorderEventArgs(e.NewOrder, e.PagesToMove, e.DestIndex));

		void ThumbNailDrawer_SelectedPageChanged(object sender, EventArgs e)
		{
			var newPageIndex = ThumbNailDrawer.SelectedPageIndex;

			var shouldRedraw =
				   newPageIndex != PageSelectorControl.CurrentPageIndex
				&& newPageIndex >= 0
				&& newPageIndex < PageSelectorControl.TotalPages
				&& newPageIndex < imageProvider.PageSelector.TotalPages;

			if (shouldRedraw)
			{
				_ = PageSelectorControl.SetCurrentPageIndex(newPageIndex);
				imageProvider.PageSelector.CurrentPageIndex = newPageIndex;

				RefreshManagerSinglePage.RefreshMagnifyingGlass = true;
				RefreshManagerThumbnails.RefreshMagnifyingGlass = true;
				RedrawGraphic();
			}
		}

		void ThumbNailDrawer_PageDelete(object sender, PageDeleteEventArgs e) => PageDelete(sender, e);

		void GraphicalDisplayControl_Resize(object sender, EventArgs e)
		{
			RefreshManagerThumbnails.ParentResized = true;
			RefreshManagerSinglePage.ParentResized = true;
			RedrawGraphic();
		}

		#endregion

		#endregion

		void RedrawGraphic() => UpdatePreviewImage(ThumbnailView);

		void RegisterForcedRedraw()
		{
			RefreshManagerSinglePage.RegisterForceRefresh();
			RefreshManagerThumbnails.RegisterForceRefresh();
		}

		void OnImagePageChanged(object sender, EventArgs e)
		{
			var curPageIndex = PageSelectorControl.CurrentPageIndex;

			if ((curPageIndex >= 0) && (curPageIndex < imageProvider.PageSelector.TotalPages))
			{
				imageProvider.PageSelector.CurrentPageIndex = curPageIndex;

				RefreshManagerThumbnails.PageChanged = true;
				RefreshManagerSinglePage.PageChanged = true;

				Cursor.Current = Cursors.WaitCursor;
				try
				{
					RefreshManagerThumbnails.ForceThumbnailScrollIntoView = true;
					RedrawGraphic();
				}
				finally
				{
					Cursor.Current = Cursors.Default;
				}
			}
		}

		void UpdateButtonIcon(ToolBarButton tbb, bool thumbnailView)
		{
			// Hardcoded image indexes are evil, but I have
			// yet to find another way 
			if (thumbnailView)
			{
				tbb.ImageIndex = 4; // single page
				tbb.ToolTipText = Res.GetString("01fe649e-d8e4-4508-9697-89620b4f2702", "Display the eDoc in Single Page View");
			}
			else
			{
				tbb.ImageIndex = 2; // thumbnail icon
				tbb.ToolTipText = Res.GetString("b65d3328-443e-4eaa-9d8b-69cbe4831ede", "Display the eDoc as Thumbnails");
			}
			tbb.Pushed = thumbnailView;
		}

		internal void ToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.Tag as string)
			{
				case "RotateLeft":
					RotateSelectedPages(false);
					break;
				case "RotateRight":
					RotateSelectedPages(true);
					break;
				case "ToggleThumbnail":
					SetThumbnailVisible(e.Button.Pushed);
					break;
				case "FullScreen":
					FullScreenView();
					break;
				case "ToggleRulers":
					rulerPanel.ShowLines = !rulerPanel.ShowLines;
					break;
			}
		}

		void GraphicalDisplayForm_Closed(object sender, EventArgs e) => CleanUp();

		void GraphicalDisplayControl_Load(object sender, EventArgs e)
		{
			Location = Point.Empty;
			Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			Size = Parent.ClientSize;

			if (ParentForm != null)
			{
				ParentForm.Closed += GraphicalDisplayForm_Closed;
				ParentForm.Resize += GraphicalDisplayControl_Resize;
			}
		}

		#region IGraphicalDisplay Implementation

		#region Page Modification Events

		public event PageDeleteEventHandler PageDelete;
		public event PageReorderEventHandler PageReorder;
		public event RotateEventHandler Rotation;
		public event PageCopyEventHandler PageCopy;
		public event PageCutEventHandler PageCut;
		public event PagePasteEventHandler PagePaste;
		public event PageDroppedEventHandler PageDropped;
		public event PageInsertEventHandler PageInsert;
		public event PageCutEventHandler MoveToNewDocument;
		public event EventHandler Closed;

		#endregion

		void EnsureMagnifyManagerCreated()
		{
			if (MagnifyManager == null)
			{
				MagnifyManager = new MagnifyManager(this, DocumentPreviewPictureBox);
			}
		}

		internal void SetThumbnailPanelContextMenuReadOnly(PreviewableDocumentType type)
		{
			thumbnailEditMenu.SetMenuItemsReadOnly(type);
		}

		public void Close()
		{
			if (MagnifyManager != null)
			{
				MagnifyManager.Dispose();
				MagnifyManager = null;
			}

			if (imageProvider != null)
			{
				imageProvider.Dispose();
				imageProvider = null;
			}

			ClearImage();

			Closed?.Invoke(this, EventArgs.Empty);
		}

		void ClearImage()
		{
			Document = null;

			PageSelectorControl.SyncTotalPages(0);
			RegisterForcedRedraw();
			RedrawGraphic();
		}

		public void ShowFile(IPreviewableDocument imageFile, StorageDocsBase storageDocument, int pageNumber = 0)
		{
			if (imageProvider == null || imageProvider.Document != imageFile)
			{
				imageProvider?.Dispose();
				imageProvider = new PreviewableDocumentImageReader(imageFile);
			}
			else
			{
				((PreviewableImagePageSelector)imageProvider.PageSelector).ClearCache();
			}

			try
			{
				imageProvider.PageSelector.CurrentPageIndex = pageNumber;

				EnsureMagnifyManagerCreated();
				SetThumbnailVisible(Env.Registry.DMThumbnailSettings.ThumbNailViewActive);
				Enabled = true;

				imageHasBeenShown = true;

				Document = storageDocument;

				PageSelectorControl.SyncTotalPages(imageProvider.PageSelector.TotalPages);
				RegisterForcedRedraw();
				RedrawGraphic();
			}
			catch (CorruptedDocumentException)
			{
				if (imageProvider != null)
				{
					imageProvider.Dispose();
				}
				throw;
			}
		}

		#endregion

		#region IImagePageSelector Implementation

		// Needed for the Magnifing Window
		public IImagePageSelector PageSelector => imageProvider?.PageSelector;

		#endregion

#if DEBUG
		internal NumericUpDown PageNumericUpDown => PageSelectorControl.PageNumericUpDown;
#endif
	}
}
