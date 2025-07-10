namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class PaymentBasisUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.paymentBasesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// paymentBasesButton
			// 
			this.paymentBasesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("24b62b8a-c085-4ce5-b108-15bc88fe3db9", "Calculation", "");
			this.paymentBasesButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.paymentBasesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.paymentBasesButton.Name = "paymentBasesButton";
			this.paymentBasesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.paymentBasesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.paymentBasesButton.TabIndex = 0;
			this.paymentBasesButton.ToolTipCaption = null;
			this.paymentBasesButton.UseVisualStyleBackColor = true;
			this.paymentBasesButton.Click += new System.EventHandler(this.PaymentBasesButton_Click);
			// 
			// PaymentBasisUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.paymentBasesButton);
			this.Name = "PaymentBasisUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZButton paymentBasesButton;

	}
}
