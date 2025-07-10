using System.ComponentModel;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class ElectronicProcessingChargeConfigurationControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		IContainer components = null;

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

		internal Enterprise.ZArchitecture.ZGrid ElectronicProcessingChargeConfigurationGrid;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ElectronicProcessingChargeConfigurationGrid.ReadOnly = readOnly;
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo jobTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditStartDate = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditEndDate = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();

			this.ElectronicProcessingChargeConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicProcessingChargeConfigurationGrid)).BeginInit();
			this.ElectronicProcessingChargeConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeConfiguration);
			// 
			// ElectronicProcessingChargeDescriptionOverrideGrid
			// 
			this.ElectronicProcessingChargeConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ElectronicProcessingChargeConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeConfiguration)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeConfiguration)(null)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Registry.Business.ElectronicProcessingChargeConfiguration)(null)).EndDate)));

			jobTypeDropEditColumnStyleInfo.ColumnName = "JobType";
			jobTypeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9DEC1CA9-0116-4939-ACFB-7C46882D2AAF", "Job Type");
			jobTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zDateEditStartDate.ColumnName = "StartDate";
			zDateEditStartDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C3250C15-47CF-45DB-BEDD-F33C267C6D68", "Start Date");
			zDateEditStartDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditStartDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;

			zDateEditEndDate.ColumnName = "EndDate";
			zDateEditEndDate.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("E5671CC7-3CA1-4D3F-9E11-427FE403B728", "End Date");
			zDateEditEndDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditEndDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;

			this.ElectronicProcessingChargeConfigurationGrid.ColumnStyles.Add(jobTypeDropEditColumnStyleInfo);
			this.ElectronicProcessingChargeConfigurationGrid.ColumnStyles.Add(zDateEditStartDate);
			this.ElectronicProcessingChargeConfigurationGrid.ColumnStyles.Add(zDateEditEndDate);
			
			this.ElectronicProcessingChargeConfigurationGrid.CaptionVisible = false;
			this.ElectronicProcessingChargeConfigurationGrid.GridId = "E0B3C000-E158-41F6-BE98-C10088E434F6";
			this.ElectronicProcessingChargeConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ElectronicProcessingChargeConfigurationGrid.LayoutKey = "ElectronicProcessingChargeDescriptionOverrideGrid";
			this.ElectronicProcessingChargeConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ElectronicProcessingChargeConfigurationGrid.Name = "ElectronicProcessingChargeConfigurationGrid";
			this.ElectronicProcessingChargeConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 372, true);
			this.ElectronicProcessingChargeConfigurationGrid.TabIndex = 0;
			this.ElectronicProcessingChargeConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// ElectronicProcessingChargeDescriptionOverrideControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.ElectronicProcessingChargeConfigurationGrid);
			this.Name = "ElectronicProcessingChargeDescriptionOverrideControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ElectronicProcessingChargeConfigurationGrid)).EndInit();
			this.ElectronicProcessingChargeConfigurationGrid.ResumeLayout(false);
			this.ElectronicProcessingChargeConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}

