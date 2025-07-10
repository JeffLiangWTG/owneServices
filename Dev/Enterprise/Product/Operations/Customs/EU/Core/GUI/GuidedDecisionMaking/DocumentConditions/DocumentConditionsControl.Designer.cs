namespace Enterprise.Customs.EU.GUI
{
	partial class DocumentConditionsControl
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
			this.DocumentConditionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingCondition);
			// 
			// DocumentConditionDetailsGroupBox
			// 
			this.DocumentConditionDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DocumentConditionDetailsGroupBox.AutoSize = true;
			this.DocumentConditionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.DocumentConditionDetailsGroupBox.Name = "DocumentConditionDetailsGroupBox";
			this.DocumentConditionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 10, true);
			this.DocumentConditionDetailsGroupBox.TabIndex = 1;
			this.DocumentConditionDetailsGroupBox.TabStop = false;
			// 
			// DocumentConditionsControl
			// 
			this.AutoSize = true;
			this.Controls.Add(this.DocumentConditionDetailsGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 5, true);
			this.Name = "DocumentConditionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 10, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox DocumentConditionDetailsGroupBox;
	}
}
