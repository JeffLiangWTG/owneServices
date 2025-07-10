using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.NCTS.GUI
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
			this.PlaceOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PlaceOfUnloadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlaceOfLoadingCodeFindBox.SuspendLayout();
			this.PlaceOfUnloadingCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// PlaceOfLoadingCodeFindBox
			// 
			this.PlaceOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfLoadingCodeFindBox, "MovementHeader.BM_PlaceOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PlaceOfLoading)));
			this.PlaceOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 15, true);
			this.PlaceOfLoadingCodeFindBox.Name = "PlaceOfLoadingCodeFindBox";
			this.PlaceOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfLoadingCodeFindBox.ParentType = null;
			this.PlaceOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.PlaceOfLoadingCodeFindBox.TabIndex = 0;
			this.PlaceOfLoadingCodeFindBox.EmptyDescriptionIfCodeNotFound = true;
			// 
			// PlaceOfUnloadingCodeFindBox
			// 
			this.PlaceOfUnloadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingCodeFindBox, "MovementHeader.BM_PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PlaceOfUnloading)));
			this.PlaceOfUnloadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 41, true);
			this.PlaceOfUnloadingCodeFindBox.Name = "PlaceOfUnloadingCodeFindBox";
			this.PlaceOfUnloadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfUnloadingCodeFindBox.ParentType = null;
			this.PlaceOfUnloadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.PlaceOfUnloadingCodeFindBox.TabIndex = 1;
			this.PlaceOfUnloadingCodeFindBox.EmptyDescriptionIfCodeNotFound = true;
			// 
			// SecurityAtDepartureUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PlaceOfLoadingCodeFindBox);
			this.Controls.Add(this.PlaceOfUnloadingCodeFindBox);
			this.Name = "SecurityAtDepartureUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlaceOfLoadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfLoadingCodeFindBox.PerformLayout();
			this.PlaceOfUnloadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfLoadingCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingCodeFindBox;
	}
}
