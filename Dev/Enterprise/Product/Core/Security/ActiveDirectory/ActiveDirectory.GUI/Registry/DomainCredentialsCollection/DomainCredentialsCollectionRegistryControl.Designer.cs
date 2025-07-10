namespace Enterprise.Security.ActiveDirectory.GUI
{
	partial class DomainCredentialsCollectionRegistryControl
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
			domainNameTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			domainUserNameTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			domainUserPasswordTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			isDefaultDomainCheckBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			userOrganisationalUnitPickerColumnStyleInfo = new Enterprise.Security.ActiveDirectory.GUI.OrganisationalUnitPickerColumnStyleInfo();
			groupOrganisationalUnitPickerColumnStyleInfo = new Enterprise.Security.ActiveDirectory.GUI.OrganisationalUnitPickerColumnStyleInfo();
			defaultPasswordTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DomainCredentialsCollectionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DomainCredentialsCollectionGrid)).BeginInit();
			this.DomainCredentialsCollectionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Security.ActiveDirectory.DomainCredentialsCollection);
			// 
			// DomainCredentialsCollectionGrid
			// 
			this.DomainCredentialsCollectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DomainCredentialsCollectionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).DomainName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).DomainUserName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).DomainUserPassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).IsDefaultDomain)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).UserOrganisationalUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).GroupOrganisationalUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.DomainCredentials)(null)).DefaultPassword)));
			this.DomainCredentialsCollectionGrid.CaptionVisible = false;
			domainNameTextBoxColumnStyleInfo.ColumnName = "DomainName";
			domainNameTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			domainNameTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			domainUserNameTextBoxColumnStyleInfo.ColumnName = "DomainUserName";
			domainUserNameTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			domainUserPasswordTextBoxColumnStyleInfo.ColumnName = "DomainUserPassword";
			domainUserPasswordTextBoxColumnStyleInfo.PasswordChar = '*';
			domainUserPasswordTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			isDefaultDomainCheckBoxColumnStyleInfo.ColumnName = "IsDefaultDomain";
			isDefaultDomainCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			userOrganisationalUnitPickerColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			userOrganisationalUnitPickerColumnStyleInfo.ColumnName = "UserOrganisationalUnit";
			userOrganisationalUnitPickerColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			groupOrganisationalUnitPickerColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			groupOrganisationalUnitPickerColumnStyleInfo.ColumnName = "GroupOrganisationalUnit";
			groupOrganisationalUnitPickerColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			defaultPasswordTextBoxColumnStyleInfo.ColumnName = "DefaultPassword";
			defaultPasswordTextBoxColumnStyleInfo.PasswordChar = '*';
			defaultPasswordTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(domainNameTextBoxColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(domainUserNameTextBoxColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(domainUserPasswordTextBoxColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(isDefaultDomainCheckBoxColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(userOrganisationalUnitPickerColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(groupOrganisationalUnitPickerColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.ColumnStyles.Add(defaultPasswordTextBoxColumnStyleInfo);
			this.DomainCredentialsCollectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DomainCredentialsCollectionGrid.GridId = "8b7f1aed-2594-4b6e-9c53-1371f6ceeeab";
			this.DomainCredentialsCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DomainCredentialsCollectionGrid.LayoutKey = "zGrid1";
			this.DomainCredentialsCollectionGrid.LimitedColumns = null;
			this.DomainCredentialsCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DomainCredentialsCollectionGrid.Name = "DomainCredentialsCollectionGrid";
			this.DomainCredentialsCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			this.DomainCredentialsCollectionGrid.TabIndex = 0;
			// 
			// DomainCredentialsCollectionRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DomainCredentialsCollectionGrid);
			this.Name = "DomainCredentialsCollectionRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DomainCredentialsCollectionGrid)).EndInit();
			this.DomainCredentialsCollectionGrid.ResumeLayout(false);
			this.DomainCredentialsCollectionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid DomainCredentialsCollectionGrid;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo domainNameTextBoxColumnStyleInfo;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo domainUserNameTextBoxColumnStyleInfo;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo domainUserPasswordTextBoxColumnStyleInfo;
		Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo isDefaultDomainCheckBoxColumnStyleInfo;
		OrganisationalUnitPickerColumnStyleInfo userOrganisationalUnitPickerColumnStyleInfo;
		OrganisationalUnitPickerColumnStyleInfo groupOrganisationalUnitPickerColumnStyleInfo;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo defaultPasswordTextBoxColumnStyleInfo;
	}
}
