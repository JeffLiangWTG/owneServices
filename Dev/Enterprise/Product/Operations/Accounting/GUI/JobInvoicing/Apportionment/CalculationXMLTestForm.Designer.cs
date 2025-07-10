namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	partial class CalculationXMLTextForm
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
		
		private ZArchitecture.GUI.ZButton copyToClipboardButton;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.ZTextBox CalculationXMLTextBox;

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.copyToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CalculationXMLTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// copyToClipboardButton
			// 
			this.copyToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyToClipboardButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("823f8c44-66d5-4caa-aee9-caea740116e9", "Copy to Clipboard");
			this.copyToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 457, true);
			this.copyToClipboardButton.Name = "copyToClipboardButton";
			this.copyToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.copyToClipboardButton.TabIndex = 2;
			this.copyToClipboardButton.UseVisualStyleBackColor = true;
			this.copyToClipboardButton.Click += new System.EventHandler(this.copyToClipboardButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b9cae165-f011-4c9c-a68b-bea128ceafbc", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 457, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// AuditLogTextBox
			// 
			this.CalculationXMLTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CalculationXMLTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("300e28d1-6f8b-4f31-a64b-164f35063cbd", "Calculation XML");
			this.CalculationXMLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.CalculationXMLTextBox.Multiline = true;
			this.CalculationXMLTextBox.Name = "AuditLogTextBox";
			this.CalculationXMLTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.CalculationXMLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 439, true);
			this.CalculationXMLTextBox.ReadOnly = true;
			this.CalculationXMLTextBox.TabIndex = 0;
			// 
			// AuditLogTextForm
			// 
			this.AcceptButton = this.closeButton;
			this.CancelButton = this.closeButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 492, true);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.copyToClipboardButton);
			this.Controls.Add(this.CalculationXMLTextBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "AuditLogTextForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}