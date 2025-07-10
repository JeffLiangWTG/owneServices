using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	partial class UPEGlbGroupsControl : RegistryZUserControl
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
			this.UPEGlbGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.UPEGlbGroupsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// UPEGlbGroupsGrid
			// 
			this.UPEGlbGroupsGrid.AllowNavigation = false;
			this.UPEGlbGroupsGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.UPEGlbGroupsRegistryObject)(null)))));
			this.UPEGlbGroupsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "GroupCollection";
			zGuidFindBoxColumnStyleInfo1.Caption = "Group";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Group";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			zTextBoxColumnStyleInfo1.Caption = "Group Description";
			zTextBoxColumnStyleInfo1.ColumnName = "GroupDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.UPEGlbGroupsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UPEGlbGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UPEGlbGroupsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UPEGlbGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UPEGlbGroupsGrid.LayoutKey = "UPEGlbGroupsGrid";
			this.UPEGlbGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UPEGlbGroupsGrid.Name = "UPEGlbGroupsGrid";
			this.UPEGlbGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 280, true);
			this.UPEGlbGroupsGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Registry.Business.UPEGlbGroupsRegistryObject)(null)).Group)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.UPEGlbGroupsRegistryObject)(null)).GroupInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Registry.Business.UPEGlbGroupsRegistryObject)(null)).GroupCollection)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Registry.Business.UPEGlbGroupsRegistryObject)(null)).GroupDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Registry.Business.UPEGlbGroupsRegistryObject)(null)).GroupDescription)));
			// 
			// UPEGlbGroupsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.UPEGlbGroupsGrid);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.UPEGlbGroupsRegistryObject";
			this.Name = "UPEGlbGroupsControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 280, true);
			((System.ComponentModel.ISupportInitialize)(this.UPEGlbGroupsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid UPEGlbGroupsGrid;		
	}
}
