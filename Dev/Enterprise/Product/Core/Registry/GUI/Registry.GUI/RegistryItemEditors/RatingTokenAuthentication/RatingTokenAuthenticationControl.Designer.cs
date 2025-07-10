namespace Enterprise.Registry.GUI
{
	public partial class RatingTokenAuthenticationControl : RegistryZUserControl
	{
		ZArchitecture.ZGrid TokenAuthenticationGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.TokenAuthenticationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TokenAuthenticationGrid)).BeginInit();
			this.TokenAuthenticationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.RatingTokenAuthenticationCollection);
			// 
			// TokenAuthenticationGrid
			// 
			this.TokenAuthenticationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TokenAuthenticationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RatingTokenAuthentication)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RatingTokenAuthentication)(null)).Endpoint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RatingTokenAuthentication)(null)).ClientId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RatingTokenAuthentication)(null)).StaffCode)));
			this.TokenAuthenticationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ClientId";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("B2602A09-D043-4529-8E31-4A3F8CF3292A", "Client Id");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo2.ColumnName = "Endpoint";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("BB714E74-BC26-4D6E-AAA0-82BAE6613578", "Token endpoint");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCodeFindBoxColumnStyleInfo.ColumnName = "StaffCode";
			zCodeFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7542D85C-DD77-4817-8BFE-B7B890032982", "Staff Code");
			zCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TokenAuthenticationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TokenAuthenticationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TokenAuthenticationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo);
			this.TokenAuthenticationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TokenAuthenticationGrid.GridId = "D3EEE596-755A-4A54-98C1-97647F7C1C9C";
			this.TokenAuthenticationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TokenAuthenticationGrid.LayoutKey = "TokenAuthenticationGrid";
			this.TokenAuthenticationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TokenAuthenticationGrid.Name = "TokenAuthenticationGrid";
			this.TokenAuthenticationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			this.TokenAuthenticationGrid.TabIndex = 0;
			// 
			// RatingTokenAuthenticationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TokenAuthenticationGrid);
			this.Name = "RatingTokenAuthenticationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 257, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TokenAuthenticationGrid)).EndInit();
			this.TokenAuthenticationGrid.ResumeLayout(false);
			this.TokenAuthenticationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
