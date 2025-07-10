namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class MyAccountHostingSiteLandingPageUrlUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.SiteLandingPageUrlGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SiteLandingPageUrlGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.MyAccountHostingSiteLandingPageUrlCollection);
			// 
			// SiteLandingPageUrlGrid
			// 
			this.SiteLandingPageUrlGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SiteLandingPageUrlGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.MyAccountHostingSiteLandingPageUrl)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.MyAccountHostingSiteLandingPageUrl)(null)).LandingPageAbsoluteUrl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.MyAccountHostingSiteLandingPageUrl)(null)).ProductCode)));
			this.SiteLandingPageUrlGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "LandingPageAbsoluteUrl";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo1.ColumnName = "ProductCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			this.SiteLandingPageUrlGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SiteLandingPageUrlGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SiteLandingPageUrlGrid.CopySelectedRowsAllowed = true;
			this.SiteLandingPageUrlGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SiteLandingPageUrlGrid.LayoutKey = "zGrid1";
			this.SiteLandingPageUrlGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SiteLandingPageUrlGrid.Name = "SiteLandingPageUrlGrid";
			this.SiteLandingPageUrlGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 223, true);
			this.SiteLandingPageUrlGrid.TabIndex = 0;
			this.SiteLandingPageUrlGrid.GridId = "a25eb280-55da-468d-abe5-6ab1931c35b1";
			// 
			// MyAccountHostingSiteLandingPageUrlUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SiteLandingPageUrlGrid);
			this.Name = "MyAccountHostingSiteLandingPageUrlUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SiteLandingPageUrlGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.ZGrid SiteLandingPageUrlGrid;
	}
}
