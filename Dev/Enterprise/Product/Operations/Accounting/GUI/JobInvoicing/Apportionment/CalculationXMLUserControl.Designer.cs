namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	partial class CalculationXMLUserControl
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
			this.calculationXMLLogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// auditLogNoteButton
			// 
			this.calculationXMLLogButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0d51ac35-1ea8-4c21-980a-bd1276b783e4", "Calculation XML", "Autorating Calculation XML", "");
			this.calculationXMLLogButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.calculationXMLLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.calculationXMLLogButton.Name = "auditLogNoteButton";
			this.calculationXMLLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 23, true);
			this.calculationXMLLogButton.TabIndex = 0;
			this.calculationXMLLogButton.UseVisualStyleBackColor = true;
			this.calculationXMLLogButton.Click += new System.EventHandler(this.auditLogNoteButton_Click);
			// 
			// AuditLogNoteUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.calculationXMLLogButton);
			this.Name = "AuditLogNoteUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 32, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZButton calculationXMLLogButton;

	}
}
