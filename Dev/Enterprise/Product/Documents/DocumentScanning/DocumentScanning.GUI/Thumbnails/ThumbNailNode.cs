using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace Enterprise.DocumentScanning.Thumbnails
{
	/// <summary>
	/// Basic struct class that represents a single thumbnail node printed out in the thumbnail view.
	/// </summary>
	public class ThumbNailNode : IDisposable
	{
		// The node will be responsible for Disposing the passed image.
		public ThumbNailNode(Panel reqPanel, PictureBox reqPictureBox, Label reqLabel,
			Panel reqMasterPanel, Panel reqDragDropPanel, Label reqPositionLine, Image imageToDisplay)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			fThePanel = reqPanel;
			fThePictureBox = reqPictureBox;
			fTheLabel = reqLabel;
			fTheMasterPanel = reqMasterPanel;
			fSelected = false;
			fThePositionLine = reqPositionLine;
			fTheDragDropPanel = reqDragDropPanel;
			fPositionLineVisible = false;
			fImage = imageToDisplay;
		}

		Panel fThePanel;
		PictureBox fThePictureBox;
		Label fTheLabel;
		Panel fTheMasterPanel;
		Panel fTheDragDropPanel;
		Label fThePositionLine;
		bool fSelected;
		bool fPositionLineVisible;
		Image fImage;

		public Panel ThePanel
		{
			get { return fThePanel; }
		}

		public PictureBox ThePictureBox
		{
			get { return fThePictureBox; }
		}

		public Label TheLabel
		{
			get { return fTheLabel; }
		}

		public Panel TheMasterPanel
		{
			get { return fTheMasterPanel; }
		}

		public Panel TheDragDropPanel
		{
			get { return fTheDragDropPanel; }
		}

		public Label ThePositionLine
		{
			get { return fThePositionLine; }
		}

		public Image TheImage
		{
			get { return fImage; }
		}

		public bool Selected
		{
			get { return fSelected; }
			set { fSelected = value; }
		}

		public bool PositionLineVisible
		{
			get { return fPositionLineVisible; }
			set { fPositionLineVisible = value; }
		}

		public void Dispose()
		{
			if (!HasBeenDisposed)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				if (fThePictureBox != null)
				{
					fThePictureBox.Image = null;
					fThePictureBox = null;
				}

				if (fImage != null)
				{
					fImage.Dispose();
					fImage = null;
				}

				fThePanel = null;
				fTheLabel = null;
				fTheMasterPanel = null;
				fTheDragDropPanel = null;
				fThePositionLine = null;

				fHasBeenDisposed = true;
			}
		}

		public bool HasBeenDisposed
		{
			get { return fHasBeenDisposed; }
		}
		bool fHasBeenDisposed;
	}
}
