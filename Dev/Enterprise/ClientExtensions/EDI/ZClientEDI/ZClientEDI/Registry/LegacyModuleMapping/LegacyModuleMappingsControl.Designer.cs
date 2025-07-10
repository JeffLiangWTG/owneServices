namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class LegacyModuleMappingsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MappingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MappingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.LegacyModuleMappingCollection);
			// 
			// MappingGrid
			// 
			this.MappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MappingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)).CriticalityMapping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)).ModuleMapping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)).ModuleMappingDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.LegacyModuleMapping)(null)).CountryMapping)));
			this.MappingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("23cb2991-a77a-4203-a223-f0f5b09422f4", "Code", "Legacy Code", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("36d97bec-c10c-44a1-bd59-11858977b96d", "Description", "Legacy Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("682af0a7-4d73-4641-bd0e-2c5139a1bd72", "Criticality Mapping", "Criticality Mapping", "");
			zDropEditColumnStyleInfo1.ColumnName = "CriticalityMapping";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("c3c93b87-dbd2-4979-ab6a-608443e9a58e", "Mod. Mapping", "Module Mapping", "");
			zDropEditColumnStyleInfo2.ColumnName = "ModuleMapping";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("9dc931ef-7b99-4aff-8bcc-7709d03db42a", "Mod. Mapping Desc.", "Module Mapping Description", "");
			zTextBoxColumnStyleInfo3.ColumnName = "ModuleMappingDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("60a262bf-38e2-4843-a9dd-311f838dc20c", "Country/Region Mapping");
			zDropEditColumnStyleInfo3.ColumnName = "CountryMapping";
			this.MappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.MappingGrid.CopySelectedRowsAllowed = true;
			this.MappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MappingGrid.GridId = "2af63adc-ca9f-4468-b49d-22f95e27cbe9";
			this.MappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MappingGrid.LayoutKey = "zGrid1";
			this.MappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MappingGrid.Name = "MappingGrid";
			this.MappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.MappingGrid.TabIndex = 0;
			// 
			// LegacyModuleMappingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MappingGrid);
			this.Name = "LegacyModuleMappingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MappingGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid MappingGrid;
	}
}
