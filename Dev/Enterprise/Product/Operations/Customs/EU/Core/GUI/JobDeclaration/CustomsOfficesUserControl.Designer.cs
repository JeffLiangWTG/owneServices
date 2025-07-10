namespace Enterprise.Customs.EU.GUI
{
	partial class CustomsOfficesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.OfficesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsOfficesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OfficesGroupBox.SuspendLayout();
			this.CustomsOfficeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
			this.CustomsOfficesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// OfficesGroupBox
			// 
			this.OfficesGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c4063858-4430-4784-9e54-b8b0281f25de", "Customs Offices");
			this.OfficesGroupBox.Controls.Add(this.CustomsOfficeFindBox);
			this.OfficesGroupBox.Controls.Add(this.CustomsOfficesGrid);
			this.OfficesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OfficesGroupBox.Name = "OfficesGroupBox";
			this.OfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 140, true);
			this.OfficesGroupBox.TabIndex = 0;
			this.OfficesGroupBox.TabStop = false;
			// 
			// CustomsOfficeFindBox
			// 
			this.CustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_CustomsOffice)));
			this.CustomsOfficeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("6941437f-1e1c-4d4d-ad5b-c1111452e41b", "Customs Office");
			this.CustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 19, true);
			this.CustomsOfficeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeFindBox.Name = "CustomsOfficeFindBox";
			this.CustomsOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.CustomsOfficeFindBox.TabIndex = 1;
			// 
			// CustomsOfficesGrid
			// 
			this.CustomsOfficesGrid.AllowNavigation = false;
			this.CustomsOfficesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomsOfficesGrid, "CustomsOffices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsOffices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsOffices)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsOffices)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsOffices)).SyncRoot)).CY_OfficeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsOffices)).SyncRoot)).CY_Date)));
			this.CustomsOfficesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("29cd1848-9506-4489-b510-1bb2fc5019cd", "Purpose");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.Caption = "";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("64400e51-3cf3-4b0d-ac7d-6913c11bc446", "Office");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("a712074c-597e-4b1b-9c28-de42afc289b5", "Office Code");
			zCodeFindBoxColumnStyleInfo1.IsCustomColumn = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("fd1e9031-8505-4499-83ba-5e52db010103", "Office Desc.");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_OfficeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("a712074c-597e-4b1b-9c28-de42afc289b5", "Office Code");
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("6699bf22-10e0-4766-9d9a-fac91232b0b3", "Time");
			zDateEditColumnStyleInfo1.ColumnName = "CY_Date";
			zDateEditColumnStyleInfo1.IsCustomColumn = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CustomsOfficesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CustomsOfficesGrid.GridId = "bb0318e1-f790-4aa5-9481-2d8bda7fbf3c";
			this.CustomsOfficesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsOfficesGrid.LayoutKey = "CustomsOfficesGrid";
			this.CustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.CustomsOfficesGrid.Name = "CustomsOfficesGrid";
			this.CustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 83, true);
			this.CustomsOfficesGrid.TabIndex = 2;
			// 
			// CustomsOfficesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OfficesGroupBox);
			this.Name = "CustomsOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OfficesGroupBox.ResumeLayout(false);
			this.OfficesGroupBox.PerformLayout();
			this.CustomsOfficeFindBox.ResumeLayout(true);
			this.CustomsOfficeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
			this.CustomsOfficesGrid.ResumeLayout(false);
			this.CustomsOfficesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox OfficesGroupBox;
		protected ZArchitecture.ZGrid CustomsOfficesGrid;
		protected ZArchitecture.GUI.ZCodeFindBox CustomsOfficeFindBox;
	}
}
