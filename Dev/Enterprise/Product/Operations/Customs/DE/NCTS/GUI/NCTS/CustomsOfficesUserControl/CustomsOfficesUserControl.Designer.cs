namespace Enterprise.Customs.DE.NCTS.GUI
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
			this.CustomsOfficesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
			this.CustomsOfficesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsHeader);
			// 
			// CustomsOfficesGrid
			// 
			this.CustomsOfficesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsOfficesGrid, "MovementHeader.CustomsOfficesForDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).Lookups.CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_OfficeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.DE.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_Date)));
			this.CustomsOfficesGrid.CaptionText = "Customs Offices (Departure, Destination and Transit)";
			this.CustomsOfficesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.CodeList";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("CB03FC81-EC9A-4F69-89E5-D3D0C375A959", "Office Code");
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("A52F8C5F-7C8B-4BD2-B028-8C9422F77987", "Desc.", "Office Desc.", "Office Desc.", "Office Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_OfficeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("E920D062-F5C3-4C00-B6B6-D1AC8621E279", "Office Code");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.ColumnName = "CY_Date";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.CustomsOfficesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CustomsOfficesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsOfficesGrid.GridId = "A0792307-92F1-4B5B-98BB-37AFE25CD03B";
			this.CustomsOfficesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsOfficesGrid.LayoutKey = "CustomsOfficesGrid";
			this.CustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsOfficesGrid.Name = "CustomsOfficesGrid";
			this.CustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 117, true);
			this.CustomsOfficesGrid.TabIndex = 32;
			// 
			// CustomsOfficesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficesGrid);
			this.Name = "CustomsOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 117, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
			this.CustomsOfficesGrid.ResumeLayout(false);
			this.CustomsOfficesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid CustomsOfficesGrid;
	}
}
