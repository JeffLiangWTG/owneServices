namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class WebSecurityMappingControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo webSecurityDropDownColumnStyle = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MappingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MappingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.WebSecurityMappingCollection);
			// 
			// MappingGrid
			// 
			this.MappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MappingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.WebSecurityMapping)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.WebSecurityMapping)(null)).WebSecurity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.WebSecurityMapping)(null)).ProductMapping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.WebSecurityMapping)(null)).ProductMappingDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.WebSecurityMapping)(null)).ModuleMapping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.WebSecurityMapping)(null)).ModuleMappingDescription)));
			this.MappingGrid.CaptionVisible = false;
			webSecurityDropDownColumnStyle.Caption = "";
			webSecurityDropDownColumnStyle.CaptionResourceString = ZClientEDI.Res.GetData("0E355950-4DA9-4C9C-ABA3-DEBB93B080AC", "Web Security", "Web Security", "");
			webSecurityDropDownColumnStyle.ColumnName = "WebSecurity";
			webSecurityDropDownColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("4302CF8A-5F82-42CF-B2DB-E4634A91A183", "Product", "Product Mapping", "");
			zDropEditColumnStyleInfo1.ColumnName = "ProductMapping";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("A1D2E0A4-1E86-4D47-800C-6EA5B760DE82", "Product Description", "Product Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "ProductMappingDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("E44C1303-EEFA-4616-BA26-35D7821C2944", "Module", "Module Mapping", "");
			zDropEditColumnStyleInfo2.ColumnName = "ModuleMapping";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("2DA1E31C-3FAC-4765-B9A2-9CB6CF2EF163", "Module Description", "Module Description", "");
			zTextBoxColumnStyleInfo3.ColumnName = "ModuleMappingDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.MappingGrid.ColumnStyles.Add(webSecurityDropDownColumnStyle);
			this.MappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MappingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MappingGrid.CopySelectedRowsAllowed = true;
			this.MappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MappingGrid.GridId = "05B49E24-42B3-484D-9B59-CC01CD38FD8E";
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
			this.Name = "WebSecurityMappingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MappingGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid MappingGrid;
	}
}
