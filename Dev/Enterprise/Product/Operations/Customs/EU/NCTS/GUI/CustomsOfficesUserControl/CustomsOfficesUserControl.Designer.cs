namespace Enterprise.Customs.EU.NCTS.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// CustomsOfficesGrid
			// 
			this.CustomsOfficesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsOfficesGrid, "CustomsOfficesForDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CustomsOfficesForDeparture)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CustomsOfficesForDeparture)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CustomsOfficesForDeparture)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CustomsOfficesForDeparture)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CustomsOfficesForDeparture)).SyncRoot)).CY_OfficeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CustomsOfficesForDeparture)).SyncRoot)).CY_Date)));
			this.CustomsOfficesGrid.CaptionText = "Customs Offices (Departure, Destination and Transit)";
			this.CustomsOfficesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Order";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.CodeList";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f88fbdf9-a7e0-4bf5-81a7-5ed84543f9de", "Office Code");
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2a681460-de50-452d-8e34-614973ad795c", "Desc.", "Office Desc.", "Office Desc.", "Office Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_OfficeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f88fbdf9-a7e0-4bf5-81a7-5ed84543f9de", "Office Code");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.ColumnName = "CY_Date";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsOfficesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsOfficesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CustomsOfficesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsOfficesGrid.GridId = "582a4d3d-a4fa-4344-8383-f88a8645b21e";
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
