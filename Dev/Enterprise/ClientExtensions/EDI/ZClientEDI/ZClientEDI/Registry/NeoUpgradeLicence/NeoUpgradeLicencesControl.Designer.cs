namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class NeoUpgradeLicencesControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo licenceEnterpriseFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo licenceEnterpriseCodeTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.NeoUpgradeLicencesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NeoUpgradeLicencesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.NeoUpgradeLicenceCollection);
			// 
			// AssignmentGrid
			// 
			this.NeoUpgradeLicencesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NeoUpgradeLicencesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.NeoUpgradeLicence)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.NeoUpgradeLicence)(null)).LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.NeoUpgradeLicence)(null)).EnterpriseCode)));
			this.NeoUpgradeLicencesGrid.CaptionVisible = false;
			licenceEnterpriseFindBoxColumnStyleInfo.CaptionResourceString = ZClientEDI.Res.GetData("ce68f480-c498-4a3c-9619-56d7ae7d5c42", "License ID");
			licenceEnterpriseFindBoxColumnStyleInfo.ColumnName = "LicencePK";
			licenceEnterpriseCodeTextBoxColumnStyleInfo.CaptionResourceString = ZClientEDI.Res.GetData("d58ead6b-3035-4b44-99b6-1c7577206acb", "Enterprise Code");
			licenceEnterpriseCodeTextBoxColumnStyleInfo.IsReadOnly = true;
			licenceEnterpriseCodeTextBoxColumnStyleInfo.ColumnName = "EnterpriseCode";
			this.NeoUpgradeLicencesGrid.ColumnStyles.Add(licenceEnterpriseFindBoxColumnStyleInfo);
			this.NeoUpgradeLicencesGrid.ColumnStyles.Add(licenceEnterpriseCodeTextBoxColumnStyleInfo);
			this.NeoUpgradeLicencesGrid.CopySelectedRowsAllowed = true;
			this.NeoUpgradeLicencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NeoUpgradeLicencesGrid.GridId = "dd1d21d1-e850-4b9a-a8bc-87f0c71cc5fc";
			this.NeoUpgradeLicencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NeoUpgradeLicencesGrid.LayoutKey = "ENeoUpgradeLicencesGrid";
			this.NeoUpgradeLicencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NeoUpgradeLicencesGrid.Name = "NeoUpgradeLicencesGrid";
			this.NeoUpgradeLicencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 263, true);
			this.NeoUpgradeLicencesGrid.TabIndex = 0;
			// 
			// ProductAreaAssignmentsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.NeoUpgradeLicencesGrid);
			this.Name = "NeoUpgradeLicencesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 263, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NeoUpgradeLicencesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid NeoUpgradeLicencesGrid;
	}
}
