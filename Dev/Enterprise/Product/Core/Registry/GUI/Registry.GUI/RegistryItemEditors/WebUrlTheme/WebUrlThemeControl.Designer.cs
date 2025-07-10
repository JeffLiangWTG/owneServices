namespace Enterprise.Registry.GUI
{
	partial class WebUrlThemeControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.UrlThemeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UrlThemeGrid)).BeginInit();
			this.UrlThemeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.WebThemeUrlCollection);
			// 
			// UrlThemeGrid
			// 
			this.UrlThemeGrid.AllowNavigation = false;
			this.UrlThemeGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.UrlThemeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.WebThemeUrl)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WebThemeUrl)(null)).ThemeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WebThemeUrl)(null)).Url)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.WebThemeUrl)(null)).CompanyCode)));
			this.UrlThemeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ce3e75bd-d7dd-4c31-bc45-5f870ade4050", "Theme Name");
			zDropEditColumnStyleInfo1.ColumnName = "ThemeName";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.IsSortable = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0530d396-7de4-40d6-92f3-30c61906f5f1", "URL");
			zTextBoxColumnStyleInfo1.ColumnName = "Url";
			zTextBoxColumnStyleInfo1.IsSortable = false;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("290aab45-8748-430a-a457-6d4598101258", "Company");
			zDropEditColumnStyleInfo2.ColumnName = "CompanyCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UrlThemeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.UrlThemeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UrlThemeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.UrlThemeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UrlThemeGrid.GridId = "03476ca6-9065-45df-b5a4-62d88de3d033";
			this.UrlThemeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UrlThemeGrid.LayoutKey = "webUrlThemeGrid";
			this.UrlThemeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UrlThemeGrid.Name = "UrlThemeGrid";
			this.UrlThemeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.UrlThemeGrid.TabIndex = 1;
			// 
			// WebUrlThemeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UrlThemeGrid);
			this.Name = "WebUrlThemeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UrlThemeGrid)).EndInit();
			this.UrlThemeGrid.ResumeLayout(false);
			this.UrlThemeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid UrlThemeGrid;
	}
}
