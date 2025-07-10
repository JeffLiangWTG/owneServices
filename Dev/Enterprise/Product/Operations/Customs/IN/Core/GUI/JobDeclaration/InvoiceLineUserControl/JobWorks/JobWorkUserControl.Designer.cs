namespace Enterprise.Customs.IN.GUI;

partial class JobWorkUserControl
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
			this.JobWorkGrid = new Enterprise.ZArchitecture.ZGrid();
			this.JobWorkNotificationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.JobWorkGrid)).BeginInit();
			this.JobWorkGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobComInvoiceLine);
			// 
			// JobWorkGrid
			// 
			this.JobWorkGrid.AllowNavigation = false;
			this.JobWorkGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobWorkGrid, "JobWorks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JobWorks)));
			this.JobWorkGrid.CaptionVisible = false;
			this.JobWorkGrid.GridId = "9F24F7EB-16D2-452F-86D0-665E313767FB";
			this.JobWorkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobWorkGrid.LayoutKey = "JobWorkGrid";
			this.JobWorkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 34, true);
			this.JobWorkGrid.Name = "JobWorkGrid";
			this.JobWorkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 148, true);
			this.JobWorkGrid.TabIndex = 1;
			// 
			// JobWorkNotificationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobWorkNotificationNoTextBox, "JI_JobWorkNotificationNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(null)).JI_JobWorkNotificationNo)));
			this.JobWorkNotificationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 2, true);
			this.JobWorkNotificationNoTextBox.Name = "JobWorkNotificationNoTextBox";
			this.JobWorkNotificationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 26, true);
			this.JobWorkNotificationNoTextBox.TabIndex = 1;
			// 
			// JobWorkUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobWorkNotificationNoTextBox);
			this.Controls.Add(this.JobWorkGrid);
			this.Name = "JobWorkUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 183, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.JobWorkGrid)).EndInit();
			this.JobWorkGrid.ResumeLayout(false);
			this.JobWorkGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZGrid JobWorkGrid;
	internal ZArchitecture.ZTextBox JobWorkNotificationNoTextBox;
}
