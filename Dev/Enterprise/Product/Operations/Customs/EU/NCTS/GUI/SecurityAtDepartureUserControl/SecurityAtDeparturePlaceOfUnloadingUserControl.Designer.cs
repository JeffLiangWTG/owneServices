namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed partial class SecurityAtDeparturePlaceOfUnloadingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecurityAtDeparturePlaceOfUnloadingUserControl));
			this.PlaceOfUnloadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PlaceOfUnloadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlaceOfUnloadingCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// PlaceOfUnloadingCodeFindBox
			// 
			this.PlaceOfUnloadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingCodeFindBox, "BM_ForeignDestPortKCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_ForeignDestPortKCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfUnloadingCodeFindBox, false);
			this.PlaceOfUnloadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceOfUnloadingCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PlaceOfUnloadingCodeFindBox.Name = "PlaceOfUnloadingCodeFindBox";
			this.PlaceOfUnloadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfUnloadingCodeFindBox.ParentType = null;
			this.PlaceOfUnloadingCodeFindBox.PreBoundMaxLength = 5;
			this.PlaceOfUnloadingCodeFindBox.ShowDescriptionBox = false;
			this.PlaceOfUnloadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PlaceOfUnloadingCodeFindBox.TabIndex = 0;
			// 
			// PlaceOfUnloadingTextBox
			// 
			this.PlaceOfUnloadingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingTextBox, "BM_PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_PlaceOfUnloading)));
			this.PlaceOfUnloadingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PlaceOfUnloadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 0, true);
			this.PlaceOfUnloadingTextBox.Name = "PlaceOfUnloadingTextBox";
			this.PlaceOfUnloadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.PlaceOfUnloadingTextBox.TabIndex = 1;
			// 
			// SecurityAtDeparturePlaceOfUnloadingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PlaceOfUnloadingTextBox);
			this.Controls.Add(this.PlaceOfUnloadingCodeFindBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "SecurityAtDeparturePlaceOfUnloadingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlaceOfUnloadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingCodeFindBox;
		internal ZArchitecture.ZTextBox PlaceOfUnloadingTextBox;
	}
}
