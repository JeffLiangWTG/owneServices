using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	partial class DebugControlInfoForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugControlInfoForm));
			this.PictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.messageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dataFieldMapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.copyContentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.flowLayoutPanel1 = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.mcrDataFieldMapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// PictureBox
			// 
			this.PictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.PictureBox.TabIndex = 1;
			this.PictureBox.TabStop = false;
			this.PictureBox.Image = System.Drawing.SystemIcons.Information.ToBitmap();
			// 
			// messageTextBox
			// 
			this.messageTextBox.AcceptsReturn = true;
			this.messageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageTextBox.BackColor = System.Drawing.Color.White;
			this.messageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.messageTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.messageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 6, true);
			this.messageTextBox.Multiline = true;
			this.messageTextBox.Name = "messageTextBox";
			this.messageTextBox.ReadOnly = true;
			this.messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.messageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 375, true);
			this.messageTextBox.TabIndex = 6;
			this.messageTextBox.TabStop = false;
			// 
			// dataFieldMapButton
			// 
			this.dataFieldMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.dataFieldMapButton.IsCaptionOverridden = true;
			this.dataFieldMapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.dataFieldMapButton.Name = "dataFieldMapButton";
			this.dataFieldMapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
			this.dataFieldMapButton.TabIndex = 7;
			this.dataFieldMapButton.Text = Enterprise.ZArchitecture.GUI.Res.GetString("DebugControlInfoForm|3207da01-274d-4c27-8d3d-50a6d1f29555", "Data Field Map");
			this.dataFieldMapButton.ToolTipCaption = null;
			this.dataFieldMapButton.Click += new System.EventHandler(this.DataFieldMapButton_Click);
			// 
			// copyContentButton
			// 
			this.copyContentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.copyContentButton.IsCaptionOverridden = true;
			this.copyContentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 1, true);
			this.copyContentButton.Name = "copyContentButton";
			this.copyContentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
			this.copyContentButton.TabIndex = 9;
			this.copyContentButton.Text = Enterprise.ZArchitecture.GUI.Res.GetString("DebugControlInfoForm|7ba6bad2-05bb-47d2-9868-4d23caca99f7", "Copy List Content");
			this.copyContentButton.ToolTipCaption = null;
			this.copyContentButton.Click += new System.EventHandler(this.CopyContentButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.IsCaptionOverridden = true;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 390, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 23, true);
			this.okButton.TabIndex = 10;
			this.okButton.Text = Enterprise.ZArchitecture.GUI.Res.GetString("DebugControlInfoForm|6096ed7d-1b9b-42c3-8d5f-41f8596cd4d0", "OK");
			this.okButton.ToolTipCaption = null;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.flowLayoutPanel1.Controls.Add(this.dataFieldMapButton);
			this.flowLayoutPanel1.Controls.Add(this.mcrDataFieldMapButton);
			this.flowLayoutPanel1.Controls.Add(this.copyContentButton);
			this.flowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 387, true);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 28, true);
			this.flowLayoutPanel1.TabIndex = 8;
			// 
			// mcrDataFieldMapButton
			// 
			this.mcrDataFieldMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.mcrDataFieldMapButton.IsCaptionOverridden = true;
			this.mcrDataFieldMapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 1, true);
			this.mcrDataFieldMapButton.Name = "mcrDataFieldMapButton";
			this.mcrDataFieldMapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
			this.mcrDataFieldMapButton.TabIndex = 8;
			this.mcrDataFieldMapButton.Text = Enterprise.ZArchitecture.GUI.Res.GetString("DebugControlInfoForm|60be9e3c-b063-47b0-a644-20d6a35c7416", "MCR Data Field Map");
			this.mcrDataFieldMapButton.ToolTipCaption = null;
			this.mcrDataFieldMapButton.Click += new System.EventHandler(this.mcrDataFieldMapButton_Click);
			// 
			// DebugControlInfoForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 419, true);
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.messageTextBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.PictureBox);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DebugControlInfoForm";
			this.ShowIcon = false;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZPictureBox PictureBox;
		protected internal ZTextBox messageTextBox;
		protected internal ZButton dataFieldMapButton;
		protected internal ZButton copyContentButton;
		protected ZButton okButton;
		CargoWise.Windows.UI.KFlowLayoutPanel flowLayoutPanel1;
		protected internal ZButton mcrDataFieldMapButton;
	}
}
