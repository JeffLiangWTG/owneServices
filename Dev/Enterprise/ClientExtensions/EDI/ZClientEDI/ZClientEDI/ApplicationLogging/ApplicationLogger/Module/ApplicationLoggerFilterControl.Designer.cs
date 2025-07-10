using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	partial class ApplicationLoggerFilterControl
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationLogger)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationLogger)(null)).ALG_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationLogger)(null)).ALG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationLogger)(null)).ALG_Product)));
			nameTextBox.ColumnName = "ALG_Name";
			nameTextBox.CaptionResourceString = ZClientEDI.Res.GetData("0C09A9AA-6B5D-4C29-A029-928448E42A74", "Name");
			nameTextBox.DefaultCollectionIndex = 0;
			nameTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			descriptionTextBox.ColumnName = "ALG_Description";
			descriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("7BA47663-5011-4C16-8EF7-8F8A4BAF80CE", "Description");
			descriptionTextBox.DefaultCollectionIndex = 0;
			descriptionTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			productTextBox.ColumnName = "ALG_Product";
			productTextBox.CaptionResourceString = ZClientEDI.Res.GetData("C57EDB4C-3E81-465C-8432-879824CB9C91", "Product");
			productTextBox.DefaultCollectionIndex = 0;
			productTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.grid.ColumnStyles.Add(nameTextBox);
			this.grid.ColumnStyles.Add(productTextBox);
			this.grid.ColumnStyles.Add(descriptionTextBox);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 413, true);
			this.grid.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.ApplicationLogging.Business.ApplicationLogger);
			// 
			// ApplicationLoggingFilterControl
			// 
			this.Name = "ApplicationLoggingFilterControl";
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
