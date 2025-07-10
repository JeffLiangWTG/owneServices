using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	partial class NextRunTimeEstimatorControl
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
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.NextRunTimeEstimatorGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NextRunTimeEstimatorGrid)).BeginInit();
			this.NextRunTimeEstimatorGrid.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // BindingSource
            //
            this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.NextRunTimeEstimator);
            //
            // MainGroupBox
            //
            this.MainGroupBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("NextRunTimeEstimatorControl|54508F79-7ED3-4115-BF48-D929610939DD", "Estimated Run Times");
            this.MainGroupBox.Controls.Add(this.NextRunTimeEstimatorGrid);
            this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 378);
			this.MainGroupBox.TabIndex = 2;
			this.MainGroupBox.TabStop = false;
			//
			// NextRunTimeEstimatorGrid
			//
			this.NextRunTimeEstimatorGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NextRunTimeEstimatorGrid, "NextRunTimeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.NextRunTimeEstimator)(null)).NextRunTimeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ServiceManager.Business.NextRunTimeRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.NextRunTimeEstimator)(null)).NextRunTimeList)).SyncRoot)).NextRunTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ServiceManager.Business.NextRunTimeRecord)(((System.Collections.IList)(((Enterprise.ServiceManager.Business.NextRunTimeEstimator)(null)).NextRunTimeList)).SyncRoot)).NextRunTimeLocal)));
			this.NextRunTimeEstimatorGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("NextRunTimeEstimatorControl|D23CC1A6-2B40-4414-B736-F008FE2C3B0B", "Run Time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "NextRunTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("NextRunTimeEstimatorControl|E9E9A3DF-2E0C-4369-BC0F-C08835896CB3", "Run Time (Local)");
			zDateEditColumnStyleInfo2.ColumnName = "NextRunTimeLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.NextRunTimeEstimatorGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.NextRunTimeEstimatorGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.NextRunTimeEstimatorGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NextRunTimeEstimatorGrid.GridId = "D23CC1A6-2B40-4414-B736-F008FE2C3B0B";
			this.NextRunTimeEstimatorGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NextRunTimeEstimatorGrid.LayoutKey = "NextRunTimeEstimatorGrid";
			this.NextRunTimeEstimatorGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NextRunTimeEstimatorGrid.Name = "NextRunTimeEstimatorGrid";
			this.NextRunTimeEstimatorGrid.ReadOnly = true;
			this.NextRunTimeEstimatorGrid.ShouldSetErrorsOnTabPage = false;
			this.NextRunTimeEstimatorGrid.TabIndex = 1;
			this.NextRunTimeEstimatorGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 340, true);
			//
			// NextRunTimeEstimatorControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MainGroupBox);
            this.Name = "NextRunTimeEstimatorControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NextRunTimeEstimatorGrid)).EndInit();
			this.NextRunTimeEstimatorGrid.ResumeLayout(false);
			this.NextRunTimeEstimatorGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDisplayGrid NextRunTimeEstimatorGrid;
	}
}
