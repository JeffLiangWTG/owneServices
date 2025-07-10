namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class UnloadedItemNewUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.UnloadedStatisticalValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BillOfLadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewItemTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.NewItemTabPage4.SuspendLayout();
			this.NewItemTabPage5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// NewItemTabPage1
			// 
			this.NewItemTabPage1.Controls.Add(this.BillOfLadingTextBox);
			this.NewItemTabPage1.Controls.Add(this.UnloadedStatisticalValueCalcEdit);
			this.NewItemTabPage1.Controls.SetChildIndex(this.UnloadedStatisticalValueCalcEdit, 0);
			this.NewItemTabPage1.Controls.SetChildIndex(this.BillOfLadingTextBox, 0);
			// 
			// UnloadedStatisticalValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UnloadedStatisticalValueCalcEdit, "UnloadingMovementHeader.GoodsItems.BY_MonetaryValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_MonetaryValue)));
			this.UnloadedStatisticalValueCalcEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("7F07F4FC-74F9-478E-9B1F-B6B4924366AD", "[46] Statistical Value");
			this.UnloadedStatisticalValueCalcEdit.DecimalPlaces = 2;
			this.UnloadedStatisticalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 192, true);
			this.UnloadedStatisticalValueCalcEdit.Name = "UnloadedStatisticalValueCalcEdit";
			this.UnloadedStatisticalValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.UnloadedStatisticalValueCalcEdit.TabIndex = 12;
			this.UnloadedStatisticalValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BillOfLadingItem
			// 
			this.BindingSource.SetBindingMember(this.BillOfLadingTextBox, "UnloadingMovementHeader.GoodsItems.BillOfLadingItem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BillOfLadingItem)));
			this.BillOfLadingTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("D4C46075-A2F9-4848-B10D-767F483781A9", "Bill of Lading/Item");
			this.BillOfLadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 192, true);
			this.BillOfLadingTextBox.MaxLength = 21;
			this.BillOfLadingTextBox.Name = "BillOfLadingTextBox";
			this.BillOfLadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.BillOfLadingTextBox.TabIndex = 13;
			this.BillOfLadingTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnloadedItemNewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "UnloadedItemNewUserControl";
			this.NewItemTabPage1.ResumeLayout(false);
			this.NewItemTabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.NewItemTabPage4.ResumeLayout(false);
			this.NewItemTabPage4.PerformLayout();
			this.NewItemTabPage5.ResumeLayout(false);
			this.NewItemTabPage5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit UnloadedStatisticalValueCalcEdit;
		private ZArchitecture.ZTextBox BillOfLadingTextBox;
	}
}
