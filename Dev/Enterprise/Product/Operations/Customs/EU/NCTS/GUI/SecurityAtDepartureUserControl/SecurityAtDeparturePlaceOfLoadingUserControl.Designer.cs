namespace Enterprise.Customs.EU.NCTS.GUI
{
	sealed partial class SecurityAtDeparturePlaceOfLoadingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PlaceOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PlaceOfLoadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlaceOfLoadingCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// PlaceOfLoadingCodeFindBox
			// 
			this.PlaceOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfLoadingCodeFindBox, "BM_PortOfPresentationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_PortOfPresentationCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceOfLoadingCodeFindBox, false);
			this.PlaceOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceOfLoadingCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PlaceOfLoadingCodeFindBox.Name = "PlaceOfLoadingCodeFindBox";
			this.PlaceOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PlaceOfLoadingCodeFindBox.ParentType = null;
			this.PlaceOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PlaceOfLoadingCodeFindBox.ShowDescriptionBox = false;
			this.PlaceOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.PlaceOfLoadingCodeFindBox.TabIndex = 0;
			// 
			// PlaceOfLoadingTextBox
			// 
			this.PlaceOfLoadingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PlaceOfLoadingTextBox, "BM_PlaceOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_PlaceOfLoading)));
			this.PlaceOfLoadingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PlaceOfLoadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 0, true);
			this.PlaceOfLoadingTextBox.Name = "PlaceOfLoadingTextBox";
			this.PlaceOfLoadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.PlaceOfLoadingTextBox.TabIndex = 1;
			// 
			// SecurityAtDeparturePlaceOfLoadingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PlaceOfLoadingTextBox);
			this.Controls.Add(this.PlaceOfLoadingCodeFindBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "SecurityAtDeparturePlaceOfLoadingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlaceOfLoadingCodeFindBox.ResumeLayout(true);
			this.PlaceOfLoadingCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfLoadingCodeFindBox;
		internal ZArchitecture.ZTextBox PlaceOfLoadingTextBox;
	}
}
