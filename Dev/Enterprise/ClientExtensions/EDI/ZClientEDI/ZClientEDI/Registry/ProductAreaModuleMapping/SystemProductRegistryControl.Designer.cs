namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class SystemProductRegistryControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
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
			this.BindingSource.SetBindingMember(this.ModuleGrid, "ModuleMappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).ModuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).ModuleDescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).IsInternal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).IsEnabled)));
			this.ModuleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("a8338e9a-a11d-4916-baca-e390d8b5a88d", "Module Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ModuleCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("d7d8f847-4775-40a4-9a31-5b93fc94cf78", "Module Desc.");
			zTextBoxColumnStyleInfo2.ColumnName = "ModuleDescriptionMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("b4280cc8-69a2-4b15-b665-6dc9a52e51ce", "Product Area");
			zDropEditColumnStyleInfo1.ColumnName = "ProductArea";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("45ba81ef-5062-40d1-8a3f-b0cac7c5427b", "Internal");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsInternal";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "IsEnabled";
			zCheckBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("c8fa9480-26f5-471c-a7fa-3c422d212e22", "Enabled");
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ModuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
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
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("0f09d0fb-851a-4fd2-8686-703b8c3e1510", "Product Code");
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("80fbbad4-2e2a-4d66-8810-c51af1023f6a", "Product Code");
			zDropEditColumnStyleInfo2.ColumnName = "CodeExisting";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("367c8c99-fa4f-483e-8658-e10f5ab7781a", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("e4fea9b0-0e2a-41e8-b4a3-741d8848784a", "Enabled");
			zCheckBoxColumnStyleInfo2.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("96e50432-1409-4804-a9af-7ba6bb7dfd7e", "Internal");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsInternal";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
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
			this.ProductsLabel.CaptionResourceString = ZClientEDI.Res.GetData("930C12DF-4B89-45DB-A1CF-A1078C1F6214", "Products");
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
			this.ModuleMappingsLabel.CaptionResourceString = ZClientEDI.Res.GetData("1392D0D9-1E23-4EA8-BB83-9ADA3DA4D004", "Module Mappings");
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
			this.BindingSource.SetBindingMember(this.SourceModuleGrid, "ModuleMappings.SourceModuleMappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).SourceModuleMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaSourceModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).SourceModuleMappings)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaSourceModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).SourceModuleMappings)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaSourceModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).SourceModuleMappings)).SyncRoot)).ModuleTreePath)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.ProductAreaSourceModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.ProductAreaModuleMapping)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.SystemProduct)(null)).ModuleMappings)).SyncRoot)).SourceModuleMappings)).SyncRoot)).ProductArea)));
			this.SourceModuleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("8a2a74ca-ab0c-4026-adc1-88ed13d4e9fa", "Menu Item Code");
			zDropEditColumnStyleInfo3.ColumnName = "Code";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("343335ba-d3a6-4bc1-bb1f-8650dbcd37e0", "Menu Item Description");
			zTextBoxColumnStyleInfo5.ColumnName = "Description";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("53976358-5d8c-4e80-9f26-c95ee4265382", "Module Tree Path");
			zTextBoxColumnStyleInfo6.ColumnName = "ModuleTreePath";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("643968cc-ca32-4a54-b564-121f67e5b317", "Product Area");
			zDropEditColumnStyleInfo4.ColumnName = "ProductArea";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SourceModuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SourceModuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SourceModuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.SourceModuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SourceModuleGrid.GridId = "568ae9e6-71f2-40df-9cc8-00d29ad48975";
			this.SourceModuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SourceModuleGrid.LayoutKey = "SourceModuleGrid";
			this.SourceModuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.SourceModuleGrid.Name = "SourceModuleGrid";
			this.SourceModuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 135, true);
			this.SourceModuleGrid.TabIndex = 1;
			// 
			// SourceModuleMappingsLabel
			// 
			this.SourceModuleMappingsLabel.CaptionResourceString = ZClientEDI.Res.GetData("9015802E-CC05-4737-98D2-1DD8CD842E0D", "Menu Item Mappings");
			this.SourceModuleMappingsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SourceModuleMappingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SourceModuleMappingsLabel.IsFontBold = true;
			this.SourceModuleMappingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SourceModuleMappingsLabel.Name = "SourceModuleMappingsLabel";
			this.SourceModuleMappingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 23, true);
			this.SourceModuleMappingsLabel.TabIndex = 0;
			// 
			// SystemProductRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer);
			this.Name = "SystemProductRegistryControl";
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
