using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class DocumentFullScreenPreviewForm : ZChildForm
	{
		public DocumentFullScreenPreviewForm()
		{
#if DEBUG
			TypeDescriptor.AddAttributes(CloseUnboundButton, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		public Image DocumentImage
		{
			get { return DocumentPreviewPictureBox.Image; }
			set
			{
				try
				{
					DocumentPreviewPictureBox.Image = value;
					ErrorLabel.Visible = false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleError(ex);
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.FitToHeightMenuItem.Caption = ResString.GetMultilingualString("3b2989a0-9e01-4c44-a20c-8e94c095a895", "Fit To &Height");
			this.FitToWidthMenuItem.Caption = ResString.GetMultilingualString("a632df90-2a69-4c0e-adc4-087e81f11c2e", "Fit To &Width");
			DocumentPreviewPictureBox.Owner = this;

			CloseUnboundButton.AllowOverlap(ImagePanel);
		}

		#region DocumentPictureBox

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		protected class DocumentPictureBox : PictureBox
		{
			internal DocumentFullScreenPreviewForm Owner
			{
				get { return owner; }
				set { owner = value; }
			}
			DocumentFullScreenPreviewForm owner;

			protected override void OnPaint(PaintEventArgs pe)
			{
				try
				{
					base.OnPaint(pe);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Owner.HandleError(ex);
				}
			}
		}

		#endregion

		#region Implementation

		void HandleError(Exception ex)
		{
			DocumentPreviewPictureBox.Image = null;
			ErrorLabel.Text = Res.GetString("54534bbe-7eae-4440-b39a-ac1ec7050d4f", "Full screen mode is not available. The image may be corrupt. ({0})", ex.Message);
			ErrorLabel.Visible = true;
			ErrorLabel.BringToFront();
		}

		ZLabel ErrorLabel
		{
			get
			{
				if (errorLabel == null)
				{
					errorLabel = new ZLabel();
					errorLabel.TextAlign = ContentAlignment.MiddleCenter;
					errorLabel.Dock = DockStyle.Fill;
					Controls.Add(errorLabel);
				}
				return errorLabel;
			}
		}
		ZLabel errorLabel;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			var initialResizeOption = ResizeOption.FitToWidth;
			if (this.DocumentPreviewPictureBox.Image != null)
			{
				initialResizeOption = this.DocumentPreviewPictureBox.Image.Width > this.DocumentPreviewPictureBox.Image.Height ? ResizeOption.FitToWidth : ResizeOption.FitToHeight;
			}
			DoPreview(initialResizeOption);
			ImagePanel.Select();
		}

		protected void DoPreview(ResizeOption selectedOption)
		{
			CheckMenuItem(selectedOption);
			if (DocumentPreviewPictureBox.Image != null)
			{
				DocumentImageSizer imageSizer = new DocumentImageSizer(DocumentPreviewPictureBox.Image, selectedOption);
				DocumentPreviewPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
				DocumentPreviewPictureBox.Size = imageSizer.AdjustImageSize(Size, out selectedOption);

				DocumentPreviewPictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX((Size.Width - DocumentPreviewPictureBox.Width) / 2),
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY((Size.Height - DocumentPreviewPictureBox.Height) / 2));

				if (DocumentPreviewPictureBox.Left < 0)
				{
					ControlDpiScalingHelper.SetLeft(ref DocumentPreviewPictureBox, 0, true);
				}
				if (DocumentPreviewPictureBox.Top < 0)
				{
					ControlDpiScalingHelper.SetTop(ref DocumentPreviewPictureBox, 0, true);
				}
			}
		}

		void CheckMenuItem(ResizeOption option)
		{
			foreach (MenuItem menuItem in this.ImageFullScreenMenu.MenuItems)
			{
				menuItem.Checked = false;
			}

			switch (option)
			{
				case ResizeOption.FitToHeight:
					FitToHeightMenuItem.Checked = true;
					break;

				case ResizeOption.FitToWidth:
					FitToWidthMenuItem.Checked = true;
					break;
			}
		}

		void FitToHeightMenuItem_Click(object sender, EventArgs e)
		{
			DoPreview(ResizeOption.FitToHeight);
		}

		void FitToWidthMenuItem_Click(object sender, EventArgs e)
		{
			DoPreview(ResizeOption.FitToWidth);
		}

		void CloseUnboundButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void HandleClientSizeChanged(object sender, EventArgs e)
		{
			DoPreview(FitToHeightMenuItem.Checked ? ResizeOption.FitToHeight : ResizeOption.FitToWidth);
		}

		#endregion
	}
}
