namespace Enterprise.Accounting.GUI.eNett
{
	partial class ContainerStoragePaymentOrgSelectionControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrganisationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource);
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStoragePaymentOrgSelectionControl|94b49a6b-f591-4d2e-8396-2b0ef45d90e8", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 3, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 23, true);
			this.FindButton.TabIndex = 0;
			this.FindButton.UseVisualStyleBackColor = true;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// SelectButton
			// 
			this.SelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStoragePaymentOrgSelectionControl|4b9718f2-626f-449e-b32e-4a4bb9a79768", "Select");
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(629, 386, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectButton.TabIndex = 2;
			this.SelectButton.UseVisualStyleBackColor = true;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStoragePaymentOrgSelectionControl|de8bd656-78c0-47dc-918a-8670f21b587d", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 386, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OrganisationGrid
			// 
			this.OrganisationGrid.AllowNavigation = false;
			this.OrganisationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrganisationGrid, "ComPayRegisteredOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).ClientName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).RelatedOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).TerminalCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).ABN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).ECN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Suburb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisation)(((System.Collections.IList)(((Enterprise.Accounting.Business.eNett.ComPayRegisteredOrganisationDataSource)(null)).ComPayRegisteredOrganisations)).SyncRoot)).RegistrationDate)));
			this.OrganisationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo2.ColumnName = "RelatedOrganisations";
			zTextBoxColumnStyleInfo3.ColumnName = "TerminalCode";
			zTextBoxColumnStyleInfo4.ColumnName = "ABN";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ECN";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo5.ColumnName = "Address1";
			zTextBoxColumnStyleInfo6.ColumnName = "Address2";
			zTextBoxColumnStyleInfo7.ColumnName = "Suburb";
			zTextBoxColumnStyleInfo8.ColumnName = "Postcode";
			zTextBoxColumnStyleInfo9.ColumnName = "State";
			zTextBoxColumnStyleInfo10.ColumnName = "Country";
			zTextBoxColumnStyleInfo11.ColumnName = "Phone";
			zTextBoxColumnStyleInfo12.ColumnName = "Fax";
			zDateEditColumnStyleInfo1.ColumnName = "RegistrationDate";
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrganisationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.OrganisationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.OrganisationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrganisationGrid.GridId = "7bb11bad-b1f4-4c07-9af3-6af094dd10fe";
			this.OrganisationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationGrid.IsWholeRowSelectedOnClick = true;
			this.OrganisationGrid.LayoutKey = "zGrid1";
			this.OrganisationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 32, true);
			this.OrganisationGrid.Name = "OrganisationGrid";
			this.OrganisationGrid.ReadOnly = true;
			this.OrganisationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 348, true);
			this.OrganisationGrid.TabIndex = 1;
			this.OrganisationGrid.DoubleClick += new System.EventHandler(this.OrganisationGrid_DoubleClick);
			// 
			// ContainerStoragePaymentOrgSelectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SelectButton);
			this.Controls.Add(this.OrganisationGrid);
			this.Controls.Add(this.FindButton);
			this.Name = "ContainerStoragePaymentOrgSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 412, true);
			this.Load += new System.EventHandler(this.ContainerStoragePaymentOrgSelectionControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton FindButton;
		public Enterprise.ZArchitecture.ZGrid OrganisationGrid;
		public Enterprise.ZArchitecture.GUI.ZButton SelectButton;
		public Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}
