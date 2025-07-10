namespace Enterprise.BufferManagement.GUI
{
	partial class CancelWorkflowForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
		protected new void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.cancellationReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 24, true);
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("41795d60-e62b-4385-b366-d47d6158e859", "Please Enter a Cancellation Reason");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 5, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 23, true);
			this.zLabel1.TabIndex = 1;
			// 
			// cancellationReasonTextBox
			// 
			this.cancellationReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cancellationReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cancellationReasonTextBox, false);
			this.cancellationReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 31, true);
			this.cancellationReasonTextBox.Multiline = true;
			this.cancellationReasonTextBox.Name = "cancellationReasonTextBox";
			this.cancellationReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 44, true);
			this.cancellationReasonTextBox.TabIndex = 2;
			// 
			// zButton1
			// 
			this.zButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButton1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ba73dbd8-2991-4d8e-8e68-e137db8d9576", "OK");
			this.zButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.zButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 80, true);
			this.zButton1.Name = "zButton1";
			this.zButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton1.TabIndex = 3;
			this.zButton1.UseVisualStyleBackColor = true;
			// 
			// zButton2
			// 
			this.zButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButton2.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b19832e4-dab4-492e-b636-18efa38b4c56", "Cancel");
			this.zButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 80, true);
			this.zButton2.Name = "zButton2";
			this.zButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton2.TabIndex = 4;
			this.zButton2.UseVisualStyleBackColor = true;
			// 
			// CancelWorkflowForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.zButton2;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("95b3a199-b291-40b3-beec-8a7bee59aa2a", "Cancel a Workflow");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 127, true);
			this.Controls.Add(this.zButton2);
			this.Controls.Add(this.zButton1);
			this.Controls.Add(this.cancellationReasonTextBox);
			this.Controls.Add(this.zLabel1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 150, true);
			this.Name = "CancelWorkflowForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.cancellationReasonTextBox, 0);
			this.Controls.SetChildIndex(this.zButton1, 0);
			this.Controls.SetChildIndex(this.zButton2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZTextBox cancellationReasonTextBox;
		private ZArchitecture.GUI.ZButton zButton1;
		private ZArchitecture.GUI.ZButton zButton2;
	}
}