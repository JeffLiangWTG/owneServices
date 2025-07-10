using System.Windows.Forms;

namespace Enterprise.DocumentVisualizer.GUI
{
	partial class PopupDynamicContentControl
	{
		private void InitializeComponent()
		{
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cornerCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.captionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contentPanel = new Enterprise.DocumentVisualizer.GUI.ContentPanel();
			this.nothingToEditLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.contentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.closeButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 66, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 31, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("c7b779dc-f2e6-4860-b3ed-3f9110b8b1fe", "Close");
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 3, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 0;
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.Color.Black;
			this.topPanel.Controls.Add(this.cornerCloseButton);
			this.topPanel.Controls.Add(this.captionLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 24, true);
			this.topPanel.TabIndex = 2;
			// 
			// cornerCloseButton
			// 
			this.cornerCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cornerCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.cornerCloseButton.FlatAppearance.BorderSize = 0;
			this.cornerCloseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkRed;
			this.cornerCloseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
			this.cornerCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cornerCloseButton.ForeColor = System.Drawing.Color.White;
			this.cornerCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 0, true);
			this.cornerCloseButton.Name = "cornerCloseButton";
			this.cornerCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.cornerCloseButton.TabIndex = 1;
			this.cornerCloseButton.Text = "X";
			this.cornerCloseButton.UseVisualStyleBackColor = false;
			// 
			// captionLabel
			// 
			this.captionLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("6a554ff6-ccd1-458a-adb8-5f36b51b4748", "Edit values");
			this.captionLabel.ForeColor = System.Drawing.Color.White;
			this.captionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.captionLabel.Name = "captionLabel";
			this.captionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.captionLabel.TabIndex = 0;
			// 
			// contentPanel
			// 
			this.contentPanel.Controls.Add(this.nothingToEditLabel);
			this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 42, true);
			this.contentPanel.TabIndex = 3;
			// 
			// nothingToEditLabel
			// 
			this.nothingToEditLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.nothingToEditLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("aaf812e4-9acb-40ed-b9cd-0663908f7f3e", "There are no editable properties");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.nothingToEditLabel, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.nothingToEditLabel, false);
			this.nothingToEditLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.nothingToEditLabel.Name = "nothingToEditLabel";
			this.nothingToEditLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 40, true);
			this.nothingToEditLabel.TabIndex = 0;
			this.nothingToEditLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// PopupDynamicContentEditor
			// 
			this.BackColor = System.Drawing.Color.White;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.contentPanel);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.bottomPanel);
			this.Name = "PopupDynamicContentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 97, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.topPanel.ResumeLayout(false);
			this.contentPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ContentPanel contentPanel;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.ZLabel captionLabel;
		private ZArchitecture.ZLabel nothingToEditLabel;
		private ZArchitecture.GUI.ZButton cornerCloseButton;
	}
}