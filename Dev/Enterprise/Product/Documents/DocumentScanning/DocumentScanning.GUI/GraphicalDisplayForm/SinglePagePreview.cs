using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Imaging;

namespace Enterprise.DocumentScanning.GUI
{
	public class SinglePagePreview
	{
		public SinglePagePreview(PictureBox documentPreviewPictureBox, Panel parentPanel)
		{
			this.parentPanel = parentPanel;
			this.documentPreviewPictureBox = documentPreviewPictureBox;
		}

		// Display the current page for the current image on the picture box.
		public void Preview(IImagePageSelector imagePageSelector)
		{
			try
			{
				var image = imagePageSelector.CurrentImage;
				if (image == null)
				{
					ClearPreview();
				}
				else
				{
					documentPreviewPictureBox.SuspendLayout();
					try
					{
						documentPreviewPictureBox.SizeMode = PictureBoxSizeMode.Normal;
						documentPreviewPictureBox.Image = GetThumbnailFromImage(image, GetAdjustedPicureBoxSize(image));
						documentPreviewPictureBox.Size = documentPreviewPictureBox.Image.Size;
						documentPreviewPictureBox.Location = GetAdjustedPictureBoxLocation(documentPreviewPictureBox.Size);
						documentPreviewPictureBox.Visible = true;
					}
					finally
					{
						documentPreviewPictureBox.ResumeLayout(false);
					}
				}
			}
			catch (Exception)
			{
				documentPreviewPictureBox.Image = null;
				throw;
			}
		}

		#region Implementation

		public void ClearPreview()
		{
			documentPreviewPictureBox.Image = null;
			documentPreviewPictureBox.Size = Size.Empty;
			documentPreviewPictureBox.SizeMode = PictureBoxSizeMode.Normal;
			documentPreviewPictureBox.Visible = false;
		}

		readonly Panel parentPanel;
		readonly PictureBox documentPreviewPictureBox;

		[return:DpiState(DpiState.ScaledVariant)]
		protected Point GetAdjustedPictureBoxLocation(Size newSize)
		{
			Size available = parentPanel.ClientSize;
			// leave an appropriate margin around width and height of client panel before drawing picture box contents
			int newLeft = ControlDpiScalingHelper.UnscaleFromCurrentDpiX((available.Width - newSize.Width) / 2);
			int newTop = ControlDpiScalingHelper.UnscaleFromCurrentDpiY((available.Height - newSize.Height) / 2);
			return ControlDpiScalingHelper.NewScaledPoint(newLeft, newTop);
		}

		protected Size GetAdjustedPicureBoxSize(Image image)
		{
			DocumentImageSizer imageSizer = new DocumentImageSizer(image, ResizeOption.FitCompletely);
			Size useableSize = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(parentPanel.ClientSize.Width) - 10,
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(parentPanel.ClientSize.Height) - 10);
			Size newSize = imageSizer.AdjustImageSize(useableSize);
			return newSize;
		}

		protected virtual Bitmap GetThumbnailFromImage(Image imageToThumbnail, Size boundingSize)
		{
			DocumentImageSizer docSizer = new DocumentImageSizer(imageToThumbnail, ResizeOption.FitCompletely);
			return docSizer.CreateThumbnail(boundingSize);
		}

		#endregion
	}
}
