using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CargoWise.Interop.DataObjects;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.DocumentScanning.GUI.Res;

namespace Enterprise.DocumentScanning.Thumbnails
{
	public class ThumbNailDrawer : IDisposable
	{
		public event EventHandler SelectedPageChanged;
		public event ThumbnailsReorderEventHandler ThumbnailReorderDragDrop;
		public event PageDroppedEventHandler PageDropped;
		public event PageDeleteEventHandler PageDelete;
		public event PageInsertEventHandler PageInsert;

		public ThumbNailDrawer(Panel parentPanel)
		{
			this.parentPanel = parentPanel;
		}

		#region Properties

		#region Document

		public StorageDocsBase Document
		{
			get { return document; }
			set { document = value; }
		}

		StorageDocsBase document;

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get { return readOnly; }
			set { readOnly = value; }
		}

		bool readOnly;

		#endregion

		#endregion

		// 0 means auto-size.
		public void SetRequestedNumberOfImagesPerRow(int totalPages, int imagesPerRow)
		{
			if (totalPages > 0)
			{
				Size newSize;

				if (imagesPerRow == 0)
				{
					imagesPerRow = CalculateOptimalNumberOfPagesPerRow(totalPages, out newSize);
				}
				else
				{
					newSize = GetTotalBoxSizeByNumberOfImagesPerRow(imagesPerRow);
				}

				numberOfPagesPerRow = imagesPerRow;
				SizeManager.TotalBoxSizeForOneThumbnail = newSize;
			}
		}

		public void Dispose()
		{
			selectedPageIndex = -1;
			ThumbnailList.Dispose();
			EraseAllThumbnailControls();
		}

		public void DrawThumbNails(IImagePageSelector pageSelector, MagnifyManager magManager)
		{
			selectedPageIndex = -1;
			ThumbnailList.ClearAll();
			int minimumPanelLength = 5;
			EraseAllThumbnailControls();

			if (pageSelector.TotalPages > 0 && SizeManager.ImagePanelSize.Height > minimumPanelLength && SizeManager.ImagePanelSize.Width > minimumPanelLength && numberOfPagesPerRow > 0)
			{
				int savedPageIndex = pageSelector.CurrentPageIndex;
				bool isLastInRow;

				for (int ii = 0; ii < pageSelector.TotalPages; ii++)
				{
					Point curPos = GetThumbnailStartPoint(numberOfPagesPerRow, ii, pageSelector.TotalPages, out isLastInRow);

					pageSelector.CurrentPageIndex = ii;
					Bitmap curBitmap = GetThumbnailFromImage(pageSelector.CurrentImage, SizeManager.ImagePanelSize);
					ThumbNailNode curNode = DrawOneImage(curBitmap, ii, curPos, parentPanel, SizeManager, isLastInRow);
					ThumbnailList.Add(curNode);

					if (magManager != null)
					{
						magManager.EnablePictureBox(curNode.ThePictureBox);
					}
				}
				pageSelector.CurrentPageIndex = savedPageIndex;
			}
		}

		public int SelectedPageIndex
		{
			get
			{
				return selectedPageIndex;
			}
			set
			{
				UnselectAllPages();
				selectedPageIndex = value;
				HighlightOnePage(selectedPageIndex, true, true);
			}
		}

		public int[] SelectedPageIndexes
		{
			get { return ThumbnailList.SelectedPageIndexes; }
		}

		public void ScrollCurrentPageIntoViewIfRequired()
		{
			ScrollPageIntoViewIfRequired(SelectedPageIndex);
		}

		public void SelectAllPages()
		{
			ChangeAllPageSelections(true);
		}

		#region Implementation

		int numberOfPagesPerRow;
		int selectedPageIndex = -1;
		bool possibleLeftClick;
		bool dragDropStarted;
		int lastPositionLineIndex = -2;
		readonly Panel parentPanel;

		#region ThumbNailList

#if DEBUG
		public
#endif
		ThumbNailList ThumbnailList
		{
			get
			{
				if (thumbNailList == null)
				{
					thumbNailList = new ThumbNailList();
				}
				return thumbNailList;
			}
		}

		ThumbNailList thumbNailList;

		#endregion

		#region Size Manager (ThumbNailDrawerSizeMgr)

#if DEBUG
		public
#endif
 ThumbNailDrawerSizeMgr SizeManager
		{
			get
			{
				if (sizeManager == null)
				{
					sizeManager = new ThumbNailDrawerSizeMgr();
				}
				return sizeManager;
			}
		}

		ThumbNailDrawerSizeMgr sizeManager;

		#endregion

		internal Size ParentPanelUsableSize
		{
			get { return ControlDpiScalingHelper.NewScaledSize(ParentPanelUsableWidth, parentPanel.Height, false); }
		}

		int ParentPanelUsableWidth
		{
			get
			{
				return parentPanel.Width - SystemInformation.VerticalScrollBarWidth -
					ControlDpiScalingHelper.ScaleToCurrentDpiX(SizeManager.WhitespaceAtRowStart + SizeManager.WhitespaceAtRowEnd);
			}
		}

		internal Size GetTotalBoxSizeByNumberOfImagesPerRow(int imagesPerRow)
		{
			int proposedWidth = ParentPanelUsableWidth / imagesPerRow;
			int proposedHeight = (int)(proposedWidth * 1.33);
			return ControlDpiScalingHelper.NewScaledSize(proposedWidth, proposedHeight, false);
		}

		internal void EraseAllThumbnailControls()
		{
			DisposeChildControls(parentPanel, false);
		}

		void DisposeChildControls(Control parent, bool clearSelf)
		{
			for (int i = parent.Controls.Count - 1; i >= 0; i--)
			{
				if (parent.Controls.Count > i)
				{
					DisposeChildControls(parent.Controls[i], true);
				}
			}

			if (clearSelf)
			{
				parent.Dispose();
			}
		}

		enum DropPanelPositionLineAlignment
		{
			Left, Centre, Right
		}

		Panel DrawDropPanel(Control parentControl, [DpiState(DpiState.ScaledVariant)] Point location, [DpiState(DpiState.ScaledVariant)] Size aSize, DropPanelPositionLineAlignment alignment, int pageIndex, out Label positionLine)
		{
			Panel dropPanel = new Panel();
			dropPanel.SuspendLayout();

			dropPanel.Size = aSize;
			dropPanel.Location = location;
			dropPanel.BorderStyle = BorderStyle.None;
			dropPanel.Tag = pageIndex;
			dropPanel.DragEnter += new DragEventHandler(this.LabelDragEnter);
			dropPanel.AllowDrop = true;
			dropPanel.DragDrop += new DragEventHandler(this.LabelDragDrop);
			dropPanel.DragLeave += new EventHandler(this.LabelDragLeave);
			dropPanel.MouseUp += new MouseEventHandler(MasterPanel_MouseUp);

			parentControl.Controls.Add(dropPanel);

			positionLine = new Label();
			positionLine.SuspendLayout();

			int positionLineWidth = 3;
			positionLine.Size = ControlDpiScalingHelper.NewScaledSize(positionLineWidth, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(dropPanel.Height));
			switch (alignment)
			{
				case DropPanelPositionLineAlignment.Left:
					positionLine.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
					break;

				case DropPanelPositionLineAlignment.Right:
					positionLine.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(dropPanel.Width) - positionLineWidth, 0);
					break;

				case DropPanelPositionLineAlignment.Centre:
					positionLine.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(dropPanel.Width / 2) - (positionLineWidth / 2), 0);
					break;
			}

			positionLine.BorderStyle = BorderStyle.None;
			positionLine.BackColor = SystemColors.ControlLightLight; // System.Drawing.SystemColors.ControlDark; 
			positionLine.Visible = false;
			positionLine.Text = "";
			positionLine.AutoSize = false;
			positionLine.Tag = pageIndex;

			dropPanel.Controls.Add(positionLine);
			positionLine.ResumeLayout(false);
			dropPanel.ResumeLayout(false);

			return dropPanel;
		}

		/// <summary>
		/// Draws the passed image as a thumbnail on screen.
		/// Precondition : CurImage should already have been sized to make sure it fits within
		/// the bounds of ReqPanelSize.
		/// </summary>
		ThumbNailNode DrawOneImage(Image imageToDisplay, int pageIndex, Point startPos, Control parentControl, ThumbNailDrawerSizeMgr sizeMgr, bool isLastInRow)
		{
			Panel masterPanel = new Panel();

			masterPanel.SuspendLayout();
			masterPanel.Size = sizeMgr.DrawableAreaSize;
			masterPanel.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(startPos.X + sizeMgr.DrawableAreaOffset.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(startPos.Y + sizeMgr.DrawableAreaOffset.Y));
			masterPanel.BorderStyle = BorderStyle.FixedSingle;
			masterPanel.Tag = pageIndex;
			masterPanel.MouseDown += new MouseEventHandler(MasterPanel_MouseDown);
			masterPanel.MouseUp += new MouseEventHandler(MasterPanel_MouseUp);

			parentControl.Controls.Add(masterPanel);

			Panel newPanel = new Panel();
			newPanel.SuspendLayout();
			newPanel.Size = sizeMgr.ImagePanelSize;
			newPanel.Location = sizeMgr.ImagePanelOffset;
			newPanel.BorderStyle = BorderStyle.None;
			newPanel.Tag = pageIndex;
			newPanel.MouseDown += new MouseEventHandler(MasterPanel_MouseDown);
			newPanel.MouseUp += new MouseEventHandler(MasterPanel_MouseUp);

			masterPanel.Controls.Add(newPanel);

			Size aSize;
			Point location;
			Panel dragDropPanel;
			Label positionLine;

			location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(masterPanel.Location.X + masterPanel.Size.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(masterPanel.Location.Y));

			if (isLastInRow)
			{
				aSize = sizeMgr.DropPanelLastInRowSize;
				dragDropPanel = DrawDropPanel(parentControl, location, aSize, DropPanelPositionLineAlignment.Right, pageIndex, out positionLine);
			}
			else
			{
				aSize = sizeMgr.DropPanelSize;
				dragDropPanel = DrawDropPanel(parentControl, location, aSize, DropPanelPositionLineAlignment.Centre, pageIndex, out positionLine);
			}

			if (pageIndex == 0)
			{
				// Special case for very first page.
				aSize = sizeMgr.DropPanelFirstInRowSize;
				location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(masterPanel.Location.X - aSize.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(masterPanel.Location.Y));

				Label beforeFirstPositionLine;
				Panel beforeFirstDragDropPanel;
				beforeFirstDragDropPanel = DrawDropPanel(parentControl, location, aSize, DropPanelPositionLineAlignment.Left, -1, out beforeFirstPositionLine);
				ThumbnailList.BeforeFirstPageNode = new ThumbNailNode(null, null, null, null, beforeFirstDragDropPanel, beforeFirstPositionLine, null);
			}

			PictureBox newPictureBox = new PictureBox();
			newPictureBox.SuspendLayout();
			newPictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			newPictureBox.Size = imageToDisplay.Size;
			newPictureBox.SizeMode = PictureBoxSizeMode.Normal;
			newPictureBox.Image = imageToDisplay;
			newPictureBox.Tag = pageIndex;
			newPictureBox.MouseDown += new MouseEventHandler(this.MasterPanel_MouseDown);
			newPictureBox.MouseUp += new MouseEventHandler(MasterPanel_MouseUp);
			newPictureBox.BackColor = SystemColors.Control;
			newPictureBox.BorderStyle = BorderStyle.None;

			newPanel.Controls.Add(newPictureBox);

			Label newLabel = new Label();
			newLabel.SuspendLayout();
			newLabel.Text = Res.GetString("ed8fdf1a-5b30-4e50-85db-2bb39ef80fae", "Page {0}", pageIndex + 1);
			newLabel.Size = sizeMgr.LabelSize;
			newLabel.Location = sizeMgr.LabelOffset;

			newLabel.TextAlign = ContentAlignment.MiddleCenter;
			newLabel.Font = new Font("Tahoma", 8);
			newLabel.BorderStyle = BorderStyle.None;
			newLabel.BackColor = SystemColors.Control;
			newLabel.ForeColor = SystemColors.ControlText;
			newLabel.AutoSize = false;
			newLabel.Tag = pageIndex;
			newLabel.Cursor = Cursors.Default;
			newLabel.ContextMenu = parentPanel.ContextMenu;
			newLabel.MouseDown += new MouseEventHandler(this.MasterPanel_MouseDown);
			newLabel.MouseUp += new MouseEventHandler(MasterPanel_MouseUp);
			newLabel.Paint += new PaintEventHandler(this.LabelPaint);
			newLabel.MouseMove += new MouseEventHandler(this.LabelMouseMove);
			newLabel.DragEnter += new DragEventHandler(this.LabelDragEnter);
			newLabel.AllowDrop = true;
			newLabel.DragDrop += new DragEventHandler(this.LabelDragDrop);
			newLabel.DragLeave += new EventHandler(this.LabelDragLeave);

			masterPanel.Controls.Add(newLabel);

			newLabel.ResumeLayout(false);
			newPictureBox.ResumeLayout(false);
			newPanel.ResumeLayout(false);

			masterPanel.ResumeLayout(false);

			ThumbNailNode newNode = new ThumbNailNode(newPanel, newPictureBox, newLabel, masterPanel, dragDropPanel, positionLine, imageToDisplay);
			return newNode;
		}

		void LabelPaint(object sender, PaintEventArgs e)
		{
			#if !WINZOR
			var label = (Label)sender;
			var yPos = label.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
			e.Graphics.DrawLine(Pens.Black, 0, yPos, label.Width, yPos);
			#endif
		}

		/// <summary>
		/// Returns an absolute point to start painting the thumbnail (Label & Image) from.
		/// The point is absolute from within the parent control that is holding the thumbnails.
		/// </summary>
		/// <param name="currentIndex">Starting from 0-index</param>
		internal Point GetThumbnailStartPoint(int columnsPerRow, int currentIndex, int totalPages, out bool isLastInRow)
		{
			int rowPosition = currentIndex / columnsPerRow;
			int columnPosition = currentIndex % columnsPerRow;
			isLastInRow = (columnPosition == columnsPerRow - 1 || currentIndex == totalPages - 1);

			int newXPosition = (columnPosition * SizeManager.TotalBoxSizeForOneThumbnail.Width);
			int newYPosition = (rowPosition * SizeManager.TotalBoxSizeForOneThumbnail.Height);
			newXPosition += ControlDpiScalingHelper.ScaleToCurrentDpiX(SizeManager.WhitespaceAtRowStart);
			return ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(newXPosition), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(newYPosition));
		}

		protected virtual Bitmap GetThumbnailFromImage(Image image, Size boundingSize)
		{
			if (boundingSize.Width > 5 || boundingSize.Height > 5)
			{
				DocumentImageSizer docSizer = new DocumentImageSizer(image, ResizeOption.FitCompletely);
				try
				{
					return docSizer.CreateThumbnail(boundingSize);
				}
				catch (OutOfMemoryException) { } //GDI+ error is wrapped as an OOM
			}
			return new Bitmap(boundingSize.Width, boundingSize.Height);
		}

		internal void ColorLabel(Label label, bool selectIt, bool updateImmediately)
		{
			label.ForeColor = (selectIt) ? SystemColors.HighlightText : SystemColors.ControlText;
			label.BackColor = (selectIt) ? SystemColors.Highlight : SystemColors.Control;

			if (updateImmediately)
			{
				label.Update();
			}
		}

		void MasterPanel_MouseDown(object sender, MouseEventArgs e)
		{
			lastPositionLineIndex = -2;
			possibleLeftClick = (e.Button == MouseButtons.Left);
			dragDropStarted = false;

			if (!possibleLeftClick)
			{
				return;
			}

			int pageIndexClicked = (int)((Control)sender).Tag;

			if (pageIndexClicked >= 0 && pageIndexClicked < ThumbnailList.Count)
			{
				ThumbNailNode nodeClicked = ThumbnailList[pageIndexClicked];

				if (nodeClicked != null)
				{
					if (Control.ModifierKeys == Keys.Control)
					{
						if (nodeClicked.Selected)
						{
							if (SelectedPageIndex != pageIndexClicked)  // not allowed to deselect page if its the current page
							{
								HighlightOnePage(pageIndexClicked, false, true);
							}
						}
						else
						{
							HighlightOnePage(pageIndexClicked, true, true);
						}
					}
					else
					{
						SelectedPageIndex = pageIndexClicked;
					}

					OnSelectedPageChanged();
				}
			}
		}

		void LabelMouseMove(object sender, MouseEventArgs e)
		{
			if (possibleLeftClick && !dragDropStarted && !ReadOnly)
			{
				Point newPoint = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y));
				Label label = (Label)sender;

				if (!label.ClientRectangle.Contains(newPoint))
				{
					dragDropStarted = true;
					string tempFile = string.Empty;
					string outputFile = string.Empty;

					try
					{
						tempFile = Document.SaveToTempFile();
						outputFile = DocumentUtilities.GetTempFilename();

						DocumentUtilities.CopySelectedPages(tempFile, outputFile, SelectedPageIndexes);

						DocManagerDataObject dataObject = new DocManagerDataObject(new SerializableEDoc(Document, DocumentUtilities.GetFileAsBytes(outputFile)));
						DragDropEffects result = label.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move);

						if (result == DragDropEffects.Move)
						{
							PageDelete(this, new PageDeleteEventArgs(SelectedPageIndexes));
						}
					}
					finally
					{
						if (File.Exists(outputFile))
						{
							File.Delete(outputFile);
						}

						if (File.Exists(tempFile))
						{
							File.Delete(tempFile);
						}
					}
				}
			}
		}

		void LabelDragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;
			if (!ReadOnly)
			{
				ThumbNailNode node = null;
				bool allowDragOnElement = false;
				int destinationIndex = (int)((Control)sender).Tag;

				if (e.Data.GetDataPresent(DocManagerDataObject.DataFormatType))
				{
					SerializableEDocCollection data = (SerializableEDocCollection)(e.Data.GetData(DocManagerDataObject.DataFormatType));
					allowDragOnElement = !data.ContainsPK(Document.PK);
				}
				else if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					allowDragOnElement = true;
				}

				if (destinationIndex == -1)
				{
					node = ThumbnailList.BeforeFirstPageNode;
				}
				else if (!ThumbnailList[destinationIndex].Selected || allowDragOnElement)
				{
					node = ThumbnailList[destinationIndex];
				}

				if (node != null && ThumbnailList.Count > 0)
				{
					if (!node.PositionLineVisible)
					{
						node.ThePositionLine.Visible = true;
						node.PositionLineVisible = true;
					}

					lastPositionLineIndex = destinationIndex;
					e.Effect = ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) ? DragDropEffects.Copy : DragDropEffects.Move;
				}
			}
		}

		void LabelDragDrop(object sender, DragEventArgs e)
		{
			try
			{
				// drag drop from one form to another.
				int destinationIndex = (int)((Control)sender).Tag;

				if (e.Data.GetDataPresent(DocManagerDataObject.DataFormatType) && !ReadOnly)
				{
					SerializableEDocCollection data = (SerializableEDocCollection)e.Data.GetData(DocManagerDataObject.DataFormatType);

					if (data.ContainsPK(Document.PK))
					{
						if (ThumbnailReorderDragDrop != null)
						{
							int[] pagesToMove = SelectedPageIndexes;
							int[] newOrder = GetNewOrder(ThumbnailList.Count, pagesToMove, destinationIndex);

							if (ThumbnailReorderDragDrop != null)
							{
								ThumbnailReorderDragDrop(sender, new ThumbnailsReorderEventHandlerArgs(newOrder, pagesToMove, destinationIndex));
							}
							e.Effect = DragDropEffects.Copy;
						}
					}
					else
					{
						if (!IsDraggingToDifferentViewOfSameDocument(data))
						{
							if (PageDropped != null)
							{
								PageDropped(this, new PageDroppedEventArgs(data, destinationIndex));
							}
							e.Effect = DragDropEffects.Move;
						}
						else
						{
							Globals.Message.ShowError(Res.GetString("f3c4e013-8f5e-4724-a10d-6560e4019fa1", "You are attempting to drag an eDoc inside itself. Please drag the eDoc to inside a different eDoc."));
						}
					}
				}
				else if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					if (PageInsert != null)
					{
						PageInsert(this, new PageInsertEventArgs((string[])e.Data.GetData(DataFormats.FileDrop), destinationIndex));
						e.Effect = ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) ? DragDropEffects.Copy : DragDropEffects.Move;
					}
				}
				else
				{
					if (PageInsert != null)
					{
						using (ZDataObject insertableData = ZDataObject.FromData(e.Data))
						{
							if (insertableData.FileDropCount > 0)
							{
								PageInsert(this, new PageInsertEventArgs((string[])insertableData.GetData(DataFormats.FileDrop), destinationIndex));
								e.Effect = ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) ? DragDropEffects.Copy : DragDropEffects.Move;
							}
						}
					}
				}
			}
			finally
			{
				HidePositionLine();
			}
		}

		bool IsDraggingToDifferentViewOfSameDocument(SerializableEDocCollection data)
		{
			return data.ContainsPK(Document.PK);
		}

		void HidePositionLine()
		{
			ThumbNailNode tempNode = null;

			if (lastPositionLineIndex >= 0)
			{
				if (lastPositionLineIndex < ThumbnailList.Count)
				{
					tempNode = ThumbnailList[lastPositionLineIndex];
				}
			}
			else if (lastPositionLineIndex == -1)
			{
				tempNode = ThumbnailList.BeforeFirstPageNode;
			}

			if (tempNode != null)
			{
				if (tempNode.PositionLineVisible)
				{
					tempNode.ThePositionLine.Visible = false;
					tempNode.PositionLineVisible = false;
				}
				lastPositionLineIndex = -2;
			}
		}

		/// <summary>
		/// Returns the new order of pages, given the total number of pages, and the selected pages
		/// to move to, just after the DestIndex page.
		/// DestIndex is set to -1 if the pages should be moved to the beginning.
		/// </summary>
		internal int[] GetNewOrder(int totalPages, int[] selectedPages, int destIndex)
		{
			int[] inverseList = DocumentUtilities.GetInverseList(selectedPages, totalPages);

			ArrayList result = new ArrayList();

			// Add pages to add before (or equal to) DestIndex
			for (int i = 0; i < inverseList.Length; i++)
			{
				if (inverseList[i] <= destIndex)
				{
					result.Add(inverseList[i]);
				}
			}

			// Add SelectedPages.
			for (int i = 0; i < selectedPages.Length; i++)
			{
				result.Add(selectedPages[i]);
			}

			// Add the rest of the pages
			for (int i = 0; i < inverseList.Length; i++)
			{
				if (inverseList[i] > destIndex)
				{
					result.Add(inverseList[i]);
				}
			}

			int[] newOrder = (int[])result.ToArray(typeof(int));
			return newOrder;
		}

		void LabelDragLeave(object sender, EventArgs e)
		{
			HidePositionLine();
		}

		void MasterPanel_MouseUp(object sender, MouseEventArgs e)
		{
			possibleLeftClick = false;
			dragDropStarted = false;
		}

		void ScrollPageIntoViewIfRequired(int pageNumIndex)
		{
			if ((pageNumIndex >= 0) && (pageNumIndex < ThumbnailList.Count))
			{
				ThumbNailNode node = ThumbnailList[pageNumIndex];
				Panel masterPanel = node.TheMasterPanel;
				masterPanel.Focus();
			}
		}

		void OnSelectedPageChanged()
		{
			if (SelectedPageChanged != null)
			{
				SelectedPageChanged(this, EventArgs.Empty);
			}
		}

		void UnselectAllPages()
		{
			ChangeAllPageSelections(false);
		}

		void ChangeAllPageSelections(bool select)
		{
			for (int ii = 0; ii < ThumbnailList.Count; ii++)
			{
				HighlightOnePage(ii, select, false);
			}
		}

		void HighlightOnePage(int pageIndex, bool selectIt, bool updateImmediately)
		{
			if (pageIndex >= 0 && pageIndex < ThumbnailList.Count)
			{
				ThumbNailNode nodeToHighlight = ThumbnailList[pageIndex];
				nodeToHighlight.Selected = selectIt;
				ColorLabel(nodeToHighlight.TheLabel, selectIt, updateImmediately);
			}
		}

		/// <summary>
		/// the Autofit option will try to fit all the thumbnails on the one panel so 
		/// that you don't have to scroll. This method works out what the size of the 
		/// image should be when you supply the number of images per row and the
		/// number of pages.
		/// </summary>
		internal Size GetMaximumTotalBoxSizeToAutoFit(double imagesPerRow, double totalPages)
		{
			Size returnSize = Size.Empty;
			int numRows = (int)Math.Ceiling(totalPages / imagesPerRow);

			if (numRows > 0)
			{
				var xSum = 1.0 * (double)imagesPerRow;
				var ySum = 1.33 * numRows;

				// N.B. used size is NOT the size of the whole panel, it's the portion of the
				// panel that we will use to actually draw (using the whole panel doesn't
				// always provide the right dimensions for the images)
				Size usedSize = DocumentImageSizer.AdjustImageSize(ResizeOption.FitCompletely, ParentPanelUsableSize, xSum / ySum);

				var oneImageWidth = Convert.ToInt32(usedSize.Width / (double)imagesPerRow);
				var oneImageHeight = Convert.ToInt32(usedSize.Height / (double)numRows);
				returnSize = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(oneImageWidth), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(oneImageHeight));
			}
			return returnSize;
		}

		int GetMostNumberOfPagesPerRowWhichExceedsSize(Size reqMinimumSize, out Size foundSize)
		{
			for (int i = 10; i >= 1; i--)
			{
				Size curSize = this.GetTotalBoxSizeByNumberOfImagesPerRow(i);
				if ((curSize.Width >= reqMinimumSize.Width) && (curSize.Height >= reqMinimumSize.Height))
				{
					foundSize = curSize;
					return i;
				}
			}

			// else not found. Just go for one.
			foundSize = this.GetTotalBoxSizeByNumberOfImagesPerRow(1);
			return 1;
		}

		internal int CalculateOptimalNumberOfPagesPerRow(int reqTotalPages, out Size reqTotalBoxSize)
		{
			Size biggestBoxSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0);
			int pagesPerRowForBiggestBoxSize = 0;

			for (int pagesPerRow = 1; pagesPerRow <= 10; pagesPerRow++)
			{
				Size currentTotalBoxSize = GetMaximumTotalBoxSizeToAutoFit(pagesPerRow, reqTotalPages);

				if ((pagesPerRowForBiggestBoxSize == 0) ||
					((currentTotalBoxSize.Height >= biggestBoxSize.Height) && (currentTotalBoxSize.Width >= biggestBoxSize.Width)))
				{
					// If pages are the same size as the previous biggest, then favour having more pages per row.
					biggestBoxSize = currentTotalBoxSize;
					pagesPerRowForBiggestBoxSize = pagesPerRow;
				}
			}

			Size minimumSize = SizeManager.MinimumTotalBoxSizeForOneThumbnail;
			if (!(biggestBoxSize.Height >= minimumSize.Height && biggestBoxSize.Width >= minimumSize.Width))
			{
				// Biggest found is not big enough. Go for scrolling.
				pagesPerRowForBiggestBoxSize = GetMostNumberOfPagesPerRowWhichExceedsSize(minimumSize, out biggestBoxSize);
			}

			reqTotalBoxSize = biggestBoxSize;
			return pagesPerRowForBiggestBoxSize;
		}

#endregion
	}
}
