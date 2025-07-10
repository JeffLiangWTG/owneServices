namespace Enterprise.Client.EDI.ApplicationLogging
{
	partial class ApplicationActiveLoggerFilterControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo nameTextBox = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionTextBox = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo productTextBox = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo environmentTextBox = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo activeUntilDateTimeBox = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowBeginDrag = false;
			this.grid.AllowDragDropWithChanges = false;
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).ApplicationLogger.ALG_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).ApplicationLogger.ALG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).ApplicationLogger.ALG_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).AAL_Environment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger)(null)).AAL_ActiveUntil)));
			nameTextBox.ColumnName = "ApplicationLogger+ALG_Name";
			nameTextBox.CaptionResourceString = ZClientEDI.Res.GetData("68B1FE4F-AFF0-4A23-A3FE-B7246869B4CF", "Name");
			nameTextBox.DefaultCollectionIndex = 0;
			nameTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			descriptionTextBox.ColumnName = "ApplicationLogger+ALG_Description";
			descriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("953B90FA-E650-4018-AF60-385621876392", "Description");
			descriptionTextBox.DefaultCollectionIndex = 0;
			descriptionTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			productTextBox.ColumnName = "ApplicationLogger+ALG_Product";
			productTextBox.CaptionResourceString = ZClientEDI.Res.GetData("7C697D44-6BDF-4731-9465-683FFE326B3A", "Product");
			productTextBox.DefaultCollectionIndex = 0;
			productTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			environmentTextBox.ColumnName = "AAL_Environment";
			environmentTextBox.CaptionResourceString = ZClientEDI.Res.GetData("AC66D8B9-B678-4C0C-9D53-02FBA1AE4472", "Environment");
			environmentTextBox.DefaultCollectionIndex = 0;
			environmentTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			activeUntilDateTimeBox.ColumnName = "AAL_ActiveUntil";
			activeUntilDateTimeBox.CaptionResourceString = ZClientEDI.Res.GetData("C2976749-6AB8-435D-8738-E9F093091A90", "Active Until");
			activeUntilDateTimeBox.DefaultCollectionIndex = 0;
			activeUntilDateTimeBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.grid.ColumnStyles.Add(nameTextBox);
			this.grid.ColumnStyles.Add(descriptionTextBox);
			this.grid.ColumnStyles.Add(productTextBox);
			this.grid.ColumnStyles.Add(environmentTextBox);
			this.grid.ColumnStyles.Add(activeUntilDateTimeBox);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 413, true);
			this.grid.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationActiveLogger);
			// 
			// ApplicationLoggingFilterControl
			// 
			this.Name = "ApplicationActiveLoggingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
