namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class ServiceTypeRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ModuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.ProductsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProductsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModuleMappingsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SourceModuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SourceModuleMappingsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ModuleGrid)).BeginInit();
			this.ModuleGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).BeginInit();
			this.SplitContainer2.Panel1.SuspendLayout();
			this.SplitContainer2.Panel2.SuspendLayout();
			this.SplitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).BeginInit();
			this.ProductsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SourceModuleGrid)).BeginInit();
			this.SourceModuleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.SystemProductCollection);
			// 
			// ModuleGrid
			// 
			this.ModuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ModuleGrid, "ServiceTypeModuleMappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).ModuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).CalculatedModuleDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).IsInternal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).IsEnabled)));
			this.ModuleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("2437e770-2e9f-4d01-a663-073fcd7ac253", "Product Area");
			zDropEditColumnStyleInfo1.ColumnName = "ProductArea";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.Caption = "";
			zDropEditColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("06dacca3-79cc-4f5a-9aa8-086aa00054ec", "Module Code");
			zDropEditColumnStyleInfo4.ColumnName = "ModuleCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("52e5949c-66bf-446c-98d3-fa18df54dcd3", "Module Desc.");
			zTextBoxColumnStyleInfo2.ColumnName = "CalculatedModuleDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("3a71b323-4713-4d96-8929-8f68edbf4a99", "Internal");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsInternal";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "IsEnabled";
			zCheckBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("2317ad69-350d-42c3-84db-7d75670fc094", "Enabled");
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ModuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ModuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ModuleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ModuleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.ModuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleGrid.GridId = "2af63adc-ca9f-4468-b49d-22f95e27cbe9";
			this.ModuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ModuleGrid.LayoutKey = "zGrid1";
			this.ModuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ModuleGrid.Name = "ModuleGrid";
			this.ModuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 135, true);
			this.ModuleGrid.TabIndex = 2;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SplitContainer2);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.SourceModuleGrid);
			this.SplitContainer.Panel2.Controls.Add(this.SourceModuleMappingsLabel);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 480, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(320);
			this.SplitContainer.TabIndex = 0;
			// 
			// SplitContainer2
			// 
			this.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer2.Name = "SplitContainer2";
			this.SplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer2.Panel1
			// 
			this.SplitContainer2.Panel1.Controls.Add(this.ProductsGrid);
			this.SplitContainer2.Panel1.Controls.Add(this.ProductsLabel);
			// 
			// SplitContainer2.Panel2
			// 
			this.SplitContainer2.Panel2.Controls.Add(this.ModuleGrid);
			this.SplitContainer2.Panel2.Controls.Add(this.ModuleMappingsLabel);
			this.SplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 320, true);
			this.SplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			this.SplitContainer2.TabIndex = 0;
			// 
			// ProductsGrid
			// 
			this.ProductsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).CodeExisting)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).Enabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).IsInternal)));
			this.ProductsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("aaf7645c-b9c5-4e90-8d48-de2cb08961f7", "Product Code");
			zDropEditColumnStyleInfo2.ColumnName = "CodeExisting";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("64ccf237-90e4-4037-b9dc-dde7933269e7", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("b0ba7073-80da-47fa-bc0a-015553630d2f", "Enabled");
			zCheckBoxColumnStyleInfo2.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("6afce97a-2ce4-451b-b2c7-4dec2a7b736d", "Internal");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsInternal";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProductsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ProductsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ProductsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductsGrid.GridId = "2af63adc-ca9f-4468-b49d-22f95e27cbe9";
			this.ProductsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductsGrid.LayoutKey = "zGrid1";
			this.ProductsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ProductsGrid.Name = "ProductsGrid";
			this.ProductsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 137, true);
			this.ProductsGrid.TabIndex = 1;
			// 
			// ProductsLabel
			// 
			this.ProductsLabel.CaptionResourceString = ZClientEDI.Res.GetData("15ebfaa0-a14c-413a-9e66-5cff59e156e5", "Products");
			this.ProductsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProductsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ProductsLabel.IsFontBold = true;
			this.ProductsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductsLabel.Name = "ProductsLabel";
			this.ProductsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 23, true);
			this.ProductsLabel.TabIndex = 0;
			// 
			// ModuleMappingsLabel
			// 
			this.ModuleMappingsLabel.CaptionResourceString = ZClientEDI.Res.GetData("2425cef4-c38c-4621-a529-44875c246db8", "Module Mappings");
			this.ModuleMappingsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ModuleMappingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ModuleMappingsLabel.IsFontBold = true;
			this.ModuleMappingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModuleMappingsLabel.Name = "ModuleMappingsLabel";
			this.ModuleMappingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 23, true);
			this.ModuleMappingsLabel.TabIndex = 0;
			// 
			// SourceModuleGrid
			// 
			this.SourceModuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SourceModuleGrid, "ServiceTypeModuleMappings.ServiceTypeMappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).ServiceTypeMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).ServiceTypeMappings)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ServiceTypeProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ServiceTypeModuleMappings)).SyncRoot)).ServiceTypeMappings)).SyncRoot)).Description)));
			this.SourceModuleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("04025fa4-1339-4099-a380-065fd046b176", "Service Type Code");
			zDropEditColumnStyleInfo3.ColumnName = "Code";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("70bf6a24-c879-4f82-92d1-f53957a9a10b", "Service Type Description");
			zTextBoxColumnStyleInfo5.ColumnName = "Description";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.SourceModuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SourceModuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SourceModuleGrid.GridId = "0a794411-ef1a-4447-a1c9-50c2d81b66c4";
			this.SourceModuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SourceModuleGrid.LayoutKey = "SourceModuleGrid";
			this.SourceModuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.SourceModuleGrid.Name = "SourceModuleGrid";
			this.SourceModuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 135, true);
			this.SourceModuleGrid.TabIndex = 1;
			// 
			// SourceModuleMappingsLabel
			// 
			this.SourceModuleMappingsLabel.CaptionResourceString = ZClientEDI.Res.GetData("0f26614e-9648-42cc-91dc-892530722522", "Service Type");
			this.SourceModuleMappingsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SourceModuleMappingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SourceModuleMappingsLabel.IsFontBold = true;
			this.SourceModuleMappingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SourceModuleMappingsLabel.Name = "SourceModuleMappingsLabel";
			this.SourceModuleMappingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 23, true);
			this.SourceModuleMappingsLabel.TabIndex = 0;
			// 
			// ServiceTypeRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer);
			this.Name = "ServiceTypeRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 480, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ModuleGrid)).EndInit();
			this.ModuleGrid.ResumeLayout(false);
			this.ModuleGrid.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.SplitContainer2.Panel1.ResumeLayout(false);
			this.SplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).EndInit();
			this.SplitContainer2.ResumeLayout(false);
			this.SplitContainer2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).EndInit();
			this.ProductsGrid.ResumeLayout(false);
			this.ProductsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SourceModuleGrid)).EndInit();
			this.SourceModuleGrid.ResumeLayout(false);
			this.SourceModuleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer2;
		internal ZArchitecture.ZGrid SourceModuleGrid;
		internal ZArchitecture.ZGrid ModuleGrid;
		private ZArchitecture.ZLabel SourceModuleMappingsLabel;
		internal ZArchitecture.ZLabel ModuleMappingsLabel;
		internal ZArchitecture.ZLabel ProductsLabel;
		internal ZArchitecture.ZGrid ProductsGrid;
	}
}
