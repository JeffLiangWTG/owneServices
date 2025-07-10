namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class SecurityAtDepartureUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PlaceOfUnloadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SpecificCircumstanceIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PlaceOfLoadingUserControl = new Enterprise.Customs.EU.NCTS.GUI.SecurityAtDeparturePlaceOfLoadingUserControl();
			this.PlaceOfUnloadingUserControl = new Enterprise.Customs.EU.NCTS.GUI.SecurityAtDeparturePlaceOfUnloadingUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlaceOfUnloadingCodeFindBox.SuspendLayout();
			this.SpecificCircumstanceIndicatorDropEdit.SuspendLayout();
			this.PlaceOfLoadingUserControl.SuspendLayout();
			this.PlaceOfUnloadingUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// PlaceOfUnloadingCodeFindBox
			// 
			this.PlaceOfUnloadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingCodeFindBox, "MovementHeader.BM_PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PlaceOfUnloading)));
			this.PlaceOfUnloadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 61, true);
			this.PlaceOfUnloadingCodeFindBox.Name = "PlaceOfUnloadingCodeFindBox";
			this.PlaceOfUnloadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfUnloadingCodeFindBox.ParentType = null;
			this.PlaceOfUnloadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.PlaceOfUnloadingCodeFindBox.TabIndex = 2;
			// 
			// SpecificCircumstanceIndicatorDropEdit
			// 
			this.SpecificCircumstanceIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificCircumstanceIndicatorDropEdit, "MovementHeader.BM_SpecificCircumstance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_SpecificCircumstance)));
			this.SpecificCircumstanceIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 91, true);
			this.SpecificCircumstanceIndicatorDropEdit.Name = "SpecificCircumstanceIndicatorDropEdit";
			this.SpecificCircumstanceIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.SpecificCircumstanceIndicatorDropEdit.TabIndex = 3;
			// 
			// PlaceOfLoadingUserControl
			// 
			this.PlaceOfLoadingUserControl.AllowDrop = true;
			this.PlaceOfLoadingUserControl.AutoSize = true;
			this.PlaceOfLoadingUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.PlaceOfLoadingUserControl, "MovementHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader)));
			this.PlaceOfLoadingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 9, true);
			this.PlaceOfLoadingUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PlaceOfLoadingUserControl.Name = "PlaceOfLoadingUserControl";
			this.PlaceOfLoadingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 23, true);
			this.PlaceOfLoadingUserControl.TabIndex = 0;
			// 
			// PlaceOfUnloadingUserControl
			// 
			this.PlaceOfUnloadingUserControl.AllowDrop = true;
			this.PlaceOfUnloadingUserControl.AutoSize = true;
			this.PlaceOfUnloadingUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingUserControl, "MovementHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader)));
			this.PlaceOfUnloadingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 35, true);
			this.PlaceOfUnloadingUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PlaceOfUnloadingUserControl.Name = "PlaceOfUnloadingUserControl";
			this.PlaceOfUnloadingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 23, true);
			this.PlaceOfUnloadingUserControl.TabIndex = 1;
			// 
			// SecurityAtDepartureUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PlaceOfUnloadingUserControl);
			this.Controls.Add(this.PlaceOfLoadingUserControl);
			this.Controls.Add(this.PlaceOfUnloadingCodeFindBox);
			this.Controls.Add(this.SpecificCircumstanceIndicatorDropEdit);
			this.Name = "SecurityAtDepartureUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 198, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlaceOfUnloadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingCodeFindBox.PerformLayout();
			this.SpecificCircumstanceIndicatorDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceIndicatorDropEdit.PerformLayout();
			this.PlaceOfLoadingUserControl.ResumeLayout(true);
			this.PlaceOfLoadingUserControl.PerformLayout();
			this.PlaceOfUnloadingUserControl.ResumeLayout(true);
			this.PlaceOfUnloadingUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit SpecificCircumstanceIndicatorDropEdit;
		internal SecurityAtDeparturePlaceOfLoadingUserControl PlaceOfLoadingUserControl;
		internal SecurityAtDeparturePlaceOfUnloadingUserControl PlaceOfUnloadingUserControl;
	}
}
