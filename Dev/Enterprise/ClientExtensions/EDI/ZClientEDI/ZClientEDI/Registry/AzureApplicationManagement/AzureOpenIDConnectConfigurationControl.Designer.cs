namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class AzureOpenIDConnectConfigurationControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.AzureOpenIDConnectConfigurationCollection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.AzureOpenIDConnectConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.AzureOpenIDConnectConfiguration)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.AzureOpenIDConnectConfiguration)(null)).AuthorityUrl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.AzureOpenIDConnectConfiguration)(null)).ClientID)));
			this.Grid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("B34DCFA1-29C1-4801-A962-537BA2C44AEF", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("1C7E5657-1EFF-4211-9BB3-D9BF09FEB6A7", "Authority URL");
			zTextBoxColumnStyleInfo2.ColumnName = "AuthorityUrl";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("F4D06FC9-3A94-48BB-8244-E14E353567A8", "Client ID");
			zTextBoxColumnStyleInfo4.ColumnName = "ClientID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Grid.GridId = "7F05AE59-E0F9-4B04-A4C3-A7E5766B02A2";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)))));
			this.Grid.LayoutKey = "zGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.Grid.TabIndex = 0;
			// 
			// AzureOpenIDConnectConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Grid);
			this.Name = "AzureOpenIDConnectConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZGrid Grid;
	}
}
