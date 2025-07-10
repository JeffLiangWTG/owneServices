namespace Enterprise.Registry.GUI
{
	partial class CountryDefaultLanguageControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CountryDefaultLanguageGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryDefaultLanguageGrid)).BeginInit();
			this.CountryDefaultLanguageGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CountryDefaultLanguageBusinessObjectCollection);
			// 
			// CountryDefaultLanguageGrid
			// 
			this.CountryDefaultLanguageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CountryDefaultLanguageGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CountryDefaultLanguageBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.CountryDefaultLanguageBusinessObject)(null)).CountryPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CountryDefaultLanguageBusinessObject)(null)).CountryName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CountryDefaultLanguageBusinessObject)(null)).DefaultLanguage)));
			this.CountryDefaultLanguageGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5dec9994-2625-4cc2-b2cd-f067f5b9b70f", "Country/Region Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CountryPk";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("16318eeb-de82-445d-9d76-ec7628c8aa07", "Country/Region Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CountryName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e80daa1e-67da-42b7-9e20-3a5f8069a2ce", "Default Language");
			zDropEditColumnStyleInfo1.ColumnName = "DefaultLanguage";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CountryDefaultLanguageGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CountryDefaultLanguageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CountryDefaultLanguageGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CountryDefaultLanguageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryDefaultLanguageGrid.GridId = "39D0D2CB-01D8-4FCA-8F85-7814C9BF28B1";
			this.CountryDefaultLanguageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CountryDefaultLanguageGrid.LayoutKey = "CountryDefaultLanguageGrid";
			this.CountryDefaultLanguageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CountryDefaultLanguageGrid.Name = "CountryDefaultLanguageGrid";
			this.CountryDefaultLanguageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 152, true);
			this.CountryDefaultLanguageGrid.TabIndex = 1;
			// 
			// CountryDefaultLanguageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CountryDefaultLanguageGrid);
			this.Name = "CountryDefaultLanguageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CountryDefaultLanguageGrid)).EndInit();
			this.CountryDefaultLanguageGrid.ResumeLayout(false);
			this.CountryDefaultLanguageGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CountryDefaultLanguageGrid;
	}
}
