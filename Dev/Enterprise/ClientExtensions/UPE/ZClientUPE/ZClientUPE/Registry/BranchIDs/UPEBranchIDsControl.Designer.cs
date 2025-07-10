
namespace Enterprise.Client.UPE
{
	using Enterprise.Registry.GUI;

    partial class UPEBranchIDsControl : RegistryZUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UPEBranchIDsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UPEBranchIDsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.UPEBranchIDsRegistryObject);
			// 
			// UPEBranchIDsGrid
			// 
			this.UPEBranchIDsGrid.AllowNavigation = false;
			this.UPEBranchIDsGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.UPEBranchIDsRegistryObject)(null)))));
			this.UPEBranchIDsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "UNLOCO_List";
			zGuidFindBoxColumnStyleInfo1.Caption = "First Arrival Port";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "FirstArrivalPort";
			zTextBoxColumnStyleInfo1.Caption = "Building ID";
			zTextBoxColumnStyleInfo1.ColumnName = "BuildingID";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.UPEBranchIDsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UPEBranchIDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UPEBranchIDsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UPEBranchIDsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UPEBranchIDsGrid.LayoutKey = "UPEBranchIDsGrid";
			this.UPEBranchIDsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UPEBranchIDsGrid.Name = "UPEBranchIDsGrid";
			this.UPEBranchIDsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 534, true);
			this.UPEBranchIDsGrid.TabIndex = 2;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.UPEBranchIDsRegistryObject)(null)).FirstArrivalPort)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.UPEBranchIDsRegistryObject)(null)).FirstArrivalPortInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.UPEBranchIDsRegistryObject)(null)).UNLOCO_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.UPEBranchIDsRegistryObject)(null)).BuildingID)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.UPEBranchIDsRegistryObject)(null)).BuildingIDInfo)));
			// 
			// UPEBranchIDsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.UPEBranchIDsGrid);
			this.Name = "UPEBranchIDsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 534, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UPEBranchIDsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

        private Enterprise.ZArchitecture.ZGrid UPEBranchIDsGrid;
	}
}
