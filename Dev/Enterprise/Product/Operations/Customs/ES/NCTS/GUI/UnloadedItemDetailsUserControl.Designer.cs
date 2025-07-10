namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class UnloadedItemDetailsUserControl
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
			UnhookEvents();
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
			this.UnloadedStatisticalValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalStatisticalValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BillOfLadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemDetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedContainersGrid)).BeginInit();
			this.UnloadedContainersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalContainersGrid)).BeginInit();
			this.OriginalContainersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedPackagesGrid)).BeginInit();
			this.UnloadedPackagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalPackagesGrid)).BeginInit();
			this.OriginalPackagesGrid.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedSupportingDocumentsGrid)).BeginInit();
			this.UnloadedSupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalSupportingDocumentsGrid)).BeginInit();
			this.OriginalSupportingDocumentsGrid.SuspendLayout();
			this.ItemAdditionalInfosTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ItemDetailsTabPage
			// 
			this.ItemDetailsTabPage.Controls.Add(this.BillOfLadingTextBox);
			this.ItemDetailsTabPage.Controls.Add(this.OriginalStatisticalValueCalcEdit);
			this.ItemDetailsTabPage.Controls.Add(this.UnloadedStatisticalValueCalcEdit);
			this.ItemDetailsTabPage.Controls.SetChildIndex(this.UnloadedStatisticalValueCalcEdit, 0);
			this.ItemDetailsTabPage.Controls.SetChildIndex(this.OriginalStatisticalValueCalcEdit, 0);
			this.ItemDetailsTabPage.Controls.SetChildIndex(this.BillOfLadingTextBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// UnloadedStatisticalValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UnloadedStatisticalValueCalcEdit, "UnloadingMovementHeader.GoodsItems.BY_MonetaryValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_MonetaryValue)));
			this.UnloadedStatisticalValueCalcEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("BC3D829F-BA19-49D9-9858-B783040AE075", "[46] Statistical Value");
			this.UnloadedStatisticalValueCalcEdit.DecimalPlaces = 2;
			this.UnloadedStatisticalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 193, true);
			this.UnloadedStatisticalValueCalcEdit.Name = "UnloadedStatisticalValueCalcEdit";
			this.UnloadedStatisticalValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.UnloadedStatisticalValueCalcEdit.TabIndex = 12;
			this.UnloadedStatisticalValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalStatisticalValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalStatisticalValueCalcEdit, "ArrivalMovementHeader.GoodsItems.BY_MonetaryValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_MonetaryValue)));
			this.OriginalStatisticalValueCalcEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("7F07F4FC-74F9-478E-9B1F-B6B4924366AD", "[46] Statistical Value");
			this.OriginalStatisticalValueCalcEdit.DecimalPlaces = 2;
			this.OriginalStatisticalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 193, true);
			this.OriginalStatisticalValueCalcEdit.Name = "OriginalStatisticalValueCalcEdit";
			this.OriginalStatisticalValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.OriginalStatisticalValueCalcEdit.TabIndex = 13;
			this.OriginalStatisticalValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BillOfLadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillOfLadingTextBox, "UnloadingMovementHeader.GoodsItems.BillOfLadingItem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BillOfLadingItem)));
			this.BillOfLadingTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("D4C46075-A2F9-4848-B10D-767F483781A9", "Bill of Lading/Item");
			this.BillOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 194, true);
			this.BillOfLadingTextBox.Name = "BillOfLadingTextBox";
			this.BillOfLadingTextBox.ReadOnly = true;
			this.BillOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.BillOfLadingTextBox.TabIndex = 14;
			this.BillOfLadingTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnloadedItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "UnloadedItemDetailsUserControl";
			this.ItemDetailsTabPage.ResumeLayout(false);
			this.ItemDetailsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedContainersGrid)).EndInit();
			this.UnloadedContainersGrid.ResumeLayout(false);
			this.UnloadedContainersGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalContainersGrid)).EndInit();
			this.OriginalContainersGrid.ResumeLayout(false);
			this.OriginalContainersGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedPackagesGrid)).EndInit();
			this.UnloadedPackagesGrid.ResumeLayout(false);
			this.UnloadedPackagesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalPackagesGrid)).EndInit();
			this.OriginalPackagesGrid.ResumeLayout(false);
			this.OriginalPackagesGrid.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedSupportingDocumentsGrid)).EndInit();
			this.UnloadedSupportingDocumentsGrid.ResumeLayout(false);
			this.UnloadedSupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalSupportingDocumentsGrid)).EndInit();
			this.OriginalSupportingDocumentsGrid.ResumeLayout(false);
			this.OriginalSupportingDocumentsGrid.PerformLayout();
			this.ItemAdditionalInfosTabPage.ResumeLayout(false);
			this.ItemAdditionalInfosTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit UnloadedStatisticalValueCalcEdit;
		private ZArchitecture.ZCalcEdit OriginalStatisticalValueCalcEdit;
		private ZArchitecture.ZTextBox BillOfLadingTextBox;
	}
}
