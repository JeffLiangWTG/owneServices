namespace Enterprise.Customs.CN.GUI
{
	partial class ContractNumbersUserControl
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
			this.ContractNumbersEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContractNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobComInvoiceHeader);
			// 
			// ContractNumbersEditButton
			// 
			this.ContractNumbersEditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ContractNumbersEditButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("1f6313eb-a9b7-47d0-be42-01162aba5d02", "More ...");
			this.ContractNumbersEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 0, true);
			this.ContractNumbersEditButton.Name = "ContractNumbersEditButton";
			this.ContractNumbersEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ContractNumbersEditButton.TabIndex = 20;
			this.ContractNumbersEditButton.Click += new System.EventHandler(this.ContractNumbersEditButton_Click);
			// 
			// ContractNumbersTextBox
			// 
			this.ContractNumbersTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContractNumbersTextBox, "ContractNumbersAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceHeader)(null)).ContractNumbersAsString)));
			this.ContractNumbersTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("4962a983-1f89-4cc1-9c2a-994279c86ed4", "CTR No.", "Contract No.", "Contract Numbers", "");
			this.ContractNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 0, true);
			this.ContractNumbersTextBox.Name = "ContractNumbersTextBox";
			this.ContractNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 20, true);
			this.ContractNumbersTextBox.TabIndex = 19;
			// 
			// ContractNumbersUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.ContractNumbersEditButton);
			this.Controls.Add(this.ContractNumbersTextBox);
			this.Name = "ContractNumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton ContractNumbersEditButton;
		private ZArchitecture.ZTextBox ContractNumbersTextBox;
	}
}
