namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EdiOrgMembershipControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo statusTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.MembershipGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MembershipGrid)).BeginInit();
			this.MembershipGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader);
			// 
			// MembershipGrid
			// 
			this.MembershipGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MembershipGrid, "Memberships");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgMembership)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)).SyncRoot)).EOR_MembershipType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgMembership)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)).SyncRoot)).EOR_ValidFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgMembership)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)).SyncRoot)).EOR_ValidTo))); 
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgMembership)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)).SyncRoot)).EOR_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgMembership)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)).SyncRoot)).EOR_OH_Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Client.EDI.MasterFiles.Business.EdiOrgMembership)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Memberships)).SyncRoot)).EOR_AgreementVersion)));
			this.MembershipGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EOR_MembershipType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "EOR_ValidFrom";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "EOR_ValidTo";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "EOR_AgreementVersion";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			statusTextBoxColumnStyleInfo.ColumnName = "EOR_Status";
			statusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);

			zDropEditColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("77b5a2bd-1ed9-4585-b549-0ede694ba5b7", "Organization");
			zDropEditColumnStyleInfo2.ColumnName = "EOR_OH_Organisation";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			this.MembershipGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MembershipGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MembershipGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MembershipGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MembershipGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MembershipGrid.ColumnStyles.Add(statusTextBoxColumnStyleInfo);
			this.MembershipGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MembershipGrid.GridId = "d8fd462f-ac37-4cd7-92dd-bc486bedd093";
			this.MembershipGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MembershipGrid.LayoutKey = "BorderWiseLicencesGrid";
			this.MembershipGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MembershipGrid.Name = "MembershipGrid";
			this.MembershipGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 257, true);
			this.MembershipGrid.TabIndex = 12;
			// 
			// EdiOrgMembershipControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MembershipGrid);
			this.Name = "EdiOrgMembershipControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 257, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MembershipGrid)).EndInit();
			this.MembershipGrid.ResumeLayout(false);
			this.MembershipGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private ZArchitecture.ZGrid MembershipGrid;
	}
}
