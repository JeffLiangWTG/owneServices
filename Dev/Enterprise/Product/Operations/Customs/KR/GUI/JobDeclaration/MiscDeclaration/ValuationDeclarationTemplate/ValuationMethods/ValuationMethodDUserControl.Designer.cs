namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationMethodDUserControl
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
			this.ValuationMethodDGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValuationDeclarationMethodTwoToSixUserControl = new Enterprise.Customs.KR.GUI.ValuationDeclarationMethodTwoToSixUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValuationMethodDGroupBox.SuspendLayout();
			this.ValuationDeclarationMethodTwoToSixUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// ValuationMethodDGroupBox
			// 
			this.ValuationMethodDGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("12c19124-b133-4989-91b1-38ec17471a39", "Valuation Method D");
			this.ValuationMethodDGroupBox.Controls.Add(this.ValuationDeclarationMethodTwoToSixUserControl);
			this.ValuationMethodDGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationMethodDGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationMethodDGroupBox.Name = "ValuationMethodDGroupBox";
			this.ValuationMethodDGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1181, 589, true);
			this.ValuationMethodDGroupBox.TabIndex = 0;
			this.ValuationMethodDGroupBox.TabStop = false;
			// 
			// ValuationDeclarationMethodTwoToSixUserControl
			// 
			this.ValuationDeclarationMethodTwoToSixUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationDeclarationMethodTwoToSixUserControl, ".");
			this.ValuationDeclarationMethodTwoToSixUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationDeclarationMethodTwoToSixUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.ValuationDeclarationMethodTwoToSixUserControl.Name = "ValuationDeclarationMethodTwoToSixUserControl";
			this.ValuationDeclarationMethodTwoToSixUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1177, 564, true);
			this.ValuationDeclarationMethodTwoToSixUserControl.TabIndex = 0;
			// 
			// ValuationMethodDUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ValuationMethodDGroupBox);
			this.Name = "ValuationMethodDUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1181, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ValuationMethodDGroupBox.ResumeLayout(false);
			this.ValuationMethodDGroupBox.PerformLayout();
			this.ValuationDeclarationMethodTwoToSixUserControl.ResumeLayout(true);
			this.ValuationDeclarationMethodTwoToSixUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ValuationMethodDGroupBox;
		private ValuationDeclarationMethodTwoToSixUserControl ValuationDeclarationMethodTwoToSixUserControl;
	}
}
