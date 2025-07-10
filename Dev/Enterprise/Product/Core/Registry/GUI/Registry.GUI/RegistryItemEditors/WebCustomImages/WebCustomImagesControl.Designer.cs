using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebCustomImagesControl : RegistryZUserControl
	{
		internal KListView ImageList;
		internal ZButton ImportButton;
		internal ZButton DeleteButton;
		internal KPictureBox PreviewPictureBox;
		internal KLabel PreviewLabel;
		internal KComboBox UrlComboBox;
		internal ZButton ExportButton;

		void InitializeComponent()
		{
			this.ImageList = new CargoWise.Windows.UI.KListView();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.PreviewLabel = new CargoWise.Windows.UI.KLabel();
			this.UrlComboBox = new CargoWise.Windows.UI.KComboBox();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// ImageList
			// 
			this.ImageList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ImageList.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.ImageList.Name = "ImageList";
			this.ImageList.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 237, true);
			this.ImageList.TabIndex = 2;
			this.ImageList.UseCompatibleStateImageBehavior = false;
			this.ImageList.View = System.Windows.Forms.View.Details;
			this.ImageList.SelectedIndexChanged += new System.EventHandler(this.RefreshPreviewPictureBox);
			this.ImageList.SizeChanged += new System.EventHandler(this.RefreshPreviewPictureBox);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0354479a-c5e9-4990-b118-22441826e9c3", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 274, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 4;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeleteButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5222510c-3006-48f2-83a5-99401a0d50a5", "Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 274, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeleteButton.TabIndex = 5;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// PreviewPictureBox
			// 
			this.PreviewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PreviewPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 31, true);
			this.PreviewPictureBox.Name = "PreviewPictureBox";
			this.PreviewPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 237, true);
			this.PreviewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.PreviewPictureBox.TabIndex = 3;
			this.PreviewPictureBox.TabStop = false;
			// 
			// PreviewLabel
			// 
			this.PreviewLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewLabel.AutoSize = true;
			this.PreviewLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 274, true);
			this.PreviewLabel.Name = "PreviewLabel";
			this.PreviewLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.PreviewLabel.TabIndex = 7;
			this.PreviewLabel.Text = "Label";
			// 
			// UrlComboBox
			// 
			this.UrlComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.UrlComboBox.FormattingEnabled = true;
			this.UrlComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.UrlComboBox.Name = "UrlComboBox";
			this.UrlComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 21, true);
			this.UrlComboBox.TabIndex = 1;
			this.UrlComboBox.SelectedIndexChanged += new System.EventHandler(this.UrlComboBox_SelectedIndexChanged);
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExportButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7dede977-a7da-4dca-8811-654b67b81a85", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 274, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExportButton.TabIndex = 6;
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// WebCustomImagesControl
			// 
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.UrlComboBox);
			this.Controls.Add(this.PreviewLabel);
			this.Controls.Add(this.PreviewPictureBox);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.ImageList);
			this.CaptionRenderingEnabled = true;
			this.Name = "WebCustomImagesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
