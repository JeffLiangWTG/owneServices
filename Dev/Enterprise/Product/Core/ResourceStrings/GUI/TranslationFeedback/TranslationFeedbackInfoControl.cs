using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI.TranslationFeedback
{
	public partial class TranslationFeedbackInfoControl : ZUserControl
	{
		public TranslationFeedbackInfoControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem == null)
			{
				currentImage = null;
			}
			else
			{
				using (var reader = ((StmTranslationFeedback)CurrentDataItem).GetXT_ScreenshotReader())
				{
					try
					{
						currentImage = Image.FromStream(reader);
					}
					catch (ArgumentException)
					{
						currentImage = null;
					}
				}
			}
			if (currentImage == null)
			{
				screenShotPictureBox.Image = null;
				screenShotPictureBox.Cursor = Cursors.Default;
			}
			else
			{
				screenShotPictureBox.Image = currentImage.GetThumbnailImage(screenShotPictureBox.Width, screenShotPictureBox.Height, null, IntPtr.Zero);
				screenShotPictureBox.Cursor = Cursors.Hand;
			}
		}

		Image currentImage;

		void screenShotPictureBox_Click(object sender, EventArgs e)
		{
			if (currentImage != null)
			{
				var tempFile = TempFile.NewWithExtension("png");
				currentImage.Save(tempFile.Filename);
				FileOpener.Open(tempFile.Filename);
			}
		}
	}
}
