namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class JPTemporaryLandingSpecificUserControl
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
			this.TemporaryLandingEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TemporaryLandingBondedTransportCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TemporaryLandingPeriodDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TemporaryLandingStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TemporaryLandingReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TemporaryLandingEndDateEdit.SuspendLayout();
			this.TemporaryLandingBondedTransportCodeDropEdit.SuspendLayout();
			this.TemporaryLandingStartDateEdit.SuspendLayout();
			this.TemporaryLandingReasonDropEdit.SuspendLayout();
			this.GoodsLocationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaBill);
			// 
			// TemporaryLandingEndDateEdit
			// 
			this.TemporaryLandingEndDateEdit.AllowDrop = true;
			this.TemporaryLandingEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TemporaryLandingEndDateEdit, "TemporaryLandingEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).TemporaryLandingEndDate)));
			this.TemporaryLandingEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 54, true);
			this.TemporaryLandingEndDateEdit.Name = "TemporaryLandingEndDateEdit";
			this.TemporaryLandingEndDateEdit.TabIndex = 9;
			// 
			// TemporaryLandingBondedTransportCodeDropEdit
			// 
			this.TemporaryLandingBondedTransportCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemporaryLandingBondedTransportCodeDropEdit, "TemporaryLandingBondedTransportCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).TemporaryLandingBondedTransportCode)));
			this.TemporaryLandingBondedTransportCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 80, true);
			this.TemporaryLandingBondedTransportCodeDropEdit.Name = "TemporaryLandingBondedTransportCodeDropEdit";
			this.TemporaryLandingBondedTransportCodeDropEdit.PreBoundMaxLength = 2;
			this.TemporaryLandingBondedTransportCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.TemporaryLandingBondedTransportCodeDropEdit.TabIndex = 11;
			// 
			// TemporaryLandingPeriodDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TemporaryLandingPeriodDaysCalcEdit, "TemporaryLandingPeriodDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).TemporaryLandingPeriodDays)));
			this.TemporaryLandingPeriodDaysCalcEdit.DecimalPlaces = 0;
			this.TemporaryLandingPeriodDaysCalcEdit.Decimals = 0;
			this.TemporaryLandingPeriodDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
			this.TemporaryLandingPeriodDaysCalcEdit.MaxValue = new decimal(new int[] {
			99,
			0,
			0,
			0});
			this.TemporaryLandingPeriodDaysCalcEdit.Name = "TemporaryLandingPeriodDaysCalcEdit";
			this.TemporaryLandingPeriodDaysCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.TemporaryLandingPeriodDaysCalcEdit.ShowGroupSeparators = false;
			this.TemporaryLandingPeriodDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.TemporaryLandingPeriodDaysCalcEdit.TabIndex = 7;
			this.TemporaryLandingPeriodDaysCalcEdit.Text = "0";
			this.TemporaryLandingPeriodDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TemporaryLandingPeriodDaysCalcEdit.TrackDisposedAccess = true;
			// 
			// TemporaryLandingStartDateEdit
			// 
			this.TemporaryLandingStartDateEdit.AllowDrop = true;
			this.TemporaryLandingStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TemporaryLandingStartDateEdit, "TemporaryLandingStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).TemporaryLandingStartDate)));
			this.TemporaryLandingStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 54, true);
			this.TemporaryLandingStartDateEdit.Name = "TemporaryLandingStartDateEdit";
			this.TemporaryLandingStartDateEdit.TabIndex = 8;
			// 
			// TemporaryLandingReasonDropEdit
			// 
			this.TemporaryLandingReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemporaryLandingReasonDropEdit, "TemporaryLandingReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).TemporaryLandingReason)));
			this.TemporaryLandingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.TemporaryLandingReasonDropEdit.Name = "TemporaryLandingReasonDropEdit";
			this.TemporaryLandingReasonDropEdit.PreBoundMaxLength = 3;
			this.TemporaryLandingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.TemporaryLandingReasonDropEdit.TabIndex = 6;
			// 
			// GoodsLocationCodeFindBox
			// 
			this.GoodsLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationCodeFindBox, "ABL_GoodsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_GoodsLocation)));
			this.GoodsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 106, true);
			this.GoodsLocationCodeFindBox.Name = "GoodsLocationCodeFindBox";
			this.GoodsLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsLocationCodeFindBox.ParentType = null;
			this.GoodsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.GoodsLocationCodeFindBox.TabIndex = 12;
			// 
			// JPTemporaryLandingSpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.GoodsLocationCodeFindBox);
			this.Controls.Add(this.TemporaryLandingEndDateEdit);
			this.Controls.Add(this.TemporaryLandingBondedTransportCodeDropEdit);
			this.Controls.Add(this.TemporaryLandingPeriodDaysCalcEdit);
			this.Controls.Add(this.TemporaryLandingStartDateEdit);
			this.Controls.Add(this.TemporaryLandingReasonDropEdit);
			this.Name = "JPTemporaryLandingSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 139, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TemporaryLandingEndDateEdit.ResumeLayout(true);
			this.TemporaryLandingEndDateEdit.PerformLayout();
			this.TemporaryLandingBondedTransportCodeDropEdit.ResumeLayout(true);
			this.TemporaryLandingBondedTransportCodeDropEdit.PerformLayout();
			this.TemporaryLandingStartDateEdit.ResumeLayout(true);
			this.TemporaryLandingStartDateEdit.PerformLayout();
			this.TemporaryLandingReasonDropEdit.ResumeLayout(true);
			this.TemporaryLandingReasonDropEdit.PerformLayout();
			this.GoodsLocationCodeFindBox.ResumeLayout(true);
			this.GoodsLocationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.GUI.ZDateEdit TemporaryLandingEndDateEdit;
		ZArchitecture.GUI.ZDropEdit TemporaryLandingBondedTransportCodeDropEdit;
		ZArchitecture.ZCalcEdit TemporaryLandingPeriodDaysCalcEdit;
		ZArchitecture.GUI.ZDateEdit TemporaryLandingStartDateEdit;
		ZArchitecture.GUI.ZDropEdit TemporaryLandingReasonDropEdit;
		ZArchitecture.GUI.ZCodeFindBox GoodsLocationCodeFindBox;
	}
}
