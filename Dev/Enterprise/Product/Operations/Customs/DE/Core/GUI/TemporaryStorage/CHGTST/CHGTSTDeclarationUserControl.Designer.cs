namespace Enterprise.Customs.DE.GUI
{
	partial class CHGTSTDeclarationUserControl
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
			this.AWBDeclarationUserControl = new Enterprise.Customs.DE.GUI.CHGTSTAWBDeclarationUserControl();
			this.REGDeclarationUserControl = new Enterprise.Customs.DE.GUI.CHGTSTREGDeclarationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.DecsGrid)).BeginInit();
			this.DecsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.declarationAndLinesSplitContainer)).BeginInit();
			this.declarationAndLinesSplitContainer.Panel1.SuspendLayout();
			this.declarationAndLinesSplitContainer.Panel2.SuspendLayout();
			this.declarationAndLinesSplitContainer.SuspendLayout();
			this.LinesTabControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AWBDeclarationUserControl.SuspendLayout();
			this.REGDeclarationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// DecsGrid
			// 
			this.BindingSource.SetBindingMember(this.DecsGrid, "CHGTSTCusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).STH_IdentificationIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).STH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).STH_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).STH_OwnerReferenceNumber)));
			// 
			// LinesGrid
			// 
			this.BindingSource.SetBindingMember(this.LinesGrid, "CHGTSTCusTempStorageDecs.CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).CustodianOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).Lookups.OrganizationsFindBoxList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).GoodsOwnerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).Lookups.OrganizationsFindBoxList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 239, true);
			// 
			// declarationAndLinesSplitContainer
			// 
			this.declarationAndLinesSplitContainer.TabIndex = 0;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.Controls.Add(this.AWBDeclarationUserControl);
			this.LinesTabPage.Controls.Add(this.REGDeclarationUserControl);
			this.LinesTabPage.Controls.SetChildIndex(this.REGDeclarationUserControl, 0);
			this.LinesTabPage.Controls.SetChildIndex(this.AWBDeclarationUserControl, 0);
			this.LinesTabPage.Controls.SetChildIndex(this.LinesGrid, 0);
			// 
			// MessagesUserControl
			// 
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "CHGTSTCusTempStorageDecs.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).Messages)));
			// 
			// AWBDeclarationUserControl
			// 
			this.AWBDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AWBDeclarationUserControl, ".");
			this.AWBDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AWBDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 239, true);
			this.AWBDeclarationUserControl.Name = "AWBDeclarationUserControl";
			this.AWBDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 135, true);
			this.AWBDeclarationUserControl.TabIndex = 1;
			// 
			// REGDeclarationUserControl
			// 
			this.REGDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REGDeclarationUserControl, ".");
			this.REGDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.REGDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 374, true);
			this.REGDeclarationUserControl.Name = "REGDeclarationUserControl";
			this.REGDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 45, true);
			this.REGDeclarationUserControl.TabIndex = 2;
			// 
			// CHGTSTDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CHGTSTDeclarationUserControl";
			((System.ComponentModel.ISupportInitialize)(this.DecsGrid)).EndInit();
			this.DecsGrid.ResumeLayout(false);
			this.DecsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.declarationAndLinesSplitContainer.Panel1.ResumeLayout(false);
			this.declarationAndLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.declarationAndLinesSplitContainer)).EndInit();
			this.declarationAndLinesSplitContainer.ResumeLayout(false);
			this.declarationAndLinesSplitContainer.PerformLayout();
			this.LinesTabControl.ResumeLayout(false);
			this.LinesTabControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AWBDeclarationUserControl.ResumeLayout(true);
			this.AWBDeclarationUserControl.PerformLayout();
			this.REGDeclarationUserControl.ResumeLayout(true);
			this.REGDeclarationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CHGTSTAWBDeclarationUserControl AWBDeclarationUserControl;
		internal CHGTSTREGDeclarationUserControl REGDeclarationUserControl;
	}
}
