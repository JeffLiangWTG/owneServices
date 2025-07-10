namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class IssueWorkItemCreationThresholdControl
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
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.IssueWorkItemCreationThresholdGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IssueWorkItemCreationThresholdGrid)).BeginInit();
            this.IssueWorkItemCreationThresholdGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IssueWorkItemCreationThreshold);
            // 
            // IssueWorkItemCreationThresholdGrid
            // 
            this.IssueWorkItemCreationThresholdGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.IssueWorkItemCreationThresholdGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IssueWorkItemCreationThreshold)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IssueWorkItemCreationThreshold)(null)).IssueOccurrenceThreshold)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IssueWorkItemCreationThreshold)(null)).ThresholdTimespan)));
            this.IssueWorkItemCreationThresholdGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("118f7f30-b2c0-49e3-b26e-f166363c7f30", "Issue Occurrence Threshold");
            zCalcEditColumnStyleInfo1.ColumnName = "IssueOccurrenceThreshold";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("67a65829-e50e-4b0f-b766-a9e3d58874da", "Threshold Timespan");
            zCalcEditColumnStyleInfo2.ColumnName = "ThresholdTimespan";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            this.IssueWorkItemCreationThresholdGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.IssueWorkItemCreationThresholdGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.IssueWorkItemCreationThresholdGrid.GridId = "1B845ED5-3042-407B-A04E-456C20C7FFCD";
            this.IssueWorkItemCreationThresholdGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.IssueWorkItemCreationThresholdGrid.LayoutKey = "IssueWorkItemCreationThresholdGrid";
            this.IssueWorkItemCreationThresholdGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.IssueWorkItemCreationThresholdGrid.Name = "IssueWorkItemCreationThresholdGrid";
            this.IssueWorkItemCreationThresholdGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
            this.IssueWorkItemCreationThresholdGrid.TabIndex = 0;
            // 
            // IssueWorkItemCreationThresholdControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.IssueWorkItemCreationThresholdGrid);
            this.Name = "IssueWorkItemCreationThresholdControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IssueWorkItemCreationThresholdGrid)).EndInit();
            this.IssueWorkItemCreationThresholdGrid.ResumeLayout(false);
            this.IssueWorkItemCreationThresholdGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid IssueWorkItemCreationThresholdGrid;
	}
}
