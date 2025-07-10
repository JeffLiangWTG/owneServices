namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class IncidentDetailsUserControl
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
			this.IncidentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EndorsementDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EndorsementAuthorityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EndorsementCountryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EndorsementPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocationOfGoodsUserControl = new Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl();
			this.EventCountryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportMeansGroupUserControl = new Phase5TransportMeansGroupUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncidentCodeDropEdit.SuspendLayout();
			this.EndorsementDateEdit.SuspendLayout();
			this.EndorsementCountryCodeDropEdit.SuspendLayout();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.EventCountryCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.EnRouteIncident);
			// 
			// IncidentCodeDropEdit
			// 
			this.IncidentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncidentCodeDropEdit, "BN_IncidentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_IncidentCode)));
			this.IncidentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 8, true);
			this.IncidentCodeDropEdit.Name = "IncidentCodeDropEdit";
			this.IncidentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.IncidentCodeDropEdit.TabIndex = 0;
			// 
			// InformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.InformationTextBox, "BN_Information");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_Information)));
			this.InformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InformationTextBox.CaptionResourceString = null;
			this.InformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 34, true);
			this.InformationTextBox.Multiline = true;
			this.InformationTextBox.Name = "InformationTextBox";
			this.InformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 62, true);
			this.InformationTextBox.TabIndex = 1;
			// 
			// EndorsementDateEdit
			// 
			this.EndorsementDateEdit.AllowDrop = true;
			this.EndorsementDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EndorsementDateEdit, "BN_EndorsementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementDate)));
			this.EndorsementDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 167, true);
			this.EndorsementDateEdit.Name = "EndorsementDateEdit";
			this.EndorsementDateEdit.TabIndex = 2;
			// 
			// EndorsementAuthorityTextBox
			// 
			this.BindingSource.SetBindingMember(this.EndorsementAuthorityTextBox, "BN_EndorsementAuthority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementAuthority)));
			this.EndorsementAuthorityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EndorsementAuthorityTextBox.CaptionResourceString = null;
			this.EndorsementAuthorityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 193, true);
			this.EndorsementAuthorityTextBox.Name = "EndorsementAuthorityTextBox";
			this.EndorsementAuthorityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.EndorsementAuthorityTextBox.TabIndex = 3;
			// 
			// EndorsementCountryCodeDropEdit
			// 
			this.EndorsementCountryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EndorsementCountryCodeDropEdit, "BN_EndorsementCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementCountryCode)));
			this.EndorsementCountryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 219, true);
			this.EndorsementCountryCodeDropEdit.Name = "EndorsementCountryCodeDropEdit";
			this.EndorsementCountryCodeDropEdit.ShowDescriptionBox = false;
			this.EndorsementCountryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.EndorsementCountryCodeDropEdit.TabIndex = 4;
			// 
			// EndorsementPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.EndorsementPlaceTextBox, "BN_EndorsementPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EndorsementPlace)));
			this.EndorsementPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EndorsementPlaceTextBox.CaptionResourceString = null;
			this.EndorsementPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 248, true);
			this.EndorsementPlaceTextBox.Name = "EndorsementPlaceTextBox";
			this.EndorsementPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.EndorsementPlaceTextBox.TabIndex = 5;
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)))));
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 8, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 25, true);
			this.LocationOfGoodsUserControl.TabIndex = 6;
			// 
			// EventCountryCodeDropEdit
			// 
			this.EventCountryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EventCountryCodeDropEdit, "BN_EventCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).BN_EventCountryCode)));
			this.EventCountryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 34, true);
			this.EventCountryCodeDropEdit.Name = "EventCountryCodeDropEdit";
			this.EventCountryCodeDropEdit.ShowDescriptionBox = false;
			this.EventCountryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.EventCountryCodeDropEdit.TabIndex = 7;
			// 
			// TransportMeansDynamicLayoutPanel
			//
			this.BindingSource.SetBindingMember(this.TransportMeansGroupUserControl, ".");
			this.TransportMeansGroupUserControl.AllowDrop = true;
			this.TransportMeansGroupUserControl.Name = "TransportMeansGroupUserControl";
			this.TransportMeansGroupUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 100, true);
			this.TransportMeansGroupUserControl.TabIndex = 8;
			// 
			// IncidentDetailsUserControl
			// 
			this.AccessibleDescription = " ";
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EventCountryCodeDropEdit);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.EndorsementPlaceTextBox);
			this.Controls.Add(this.EndorsementCountryCodeDropEdit);
			this.Controls.Add(this.EndorsementAuthorityTextBox);
			this.Controls.Add(this.EndorsementDateEdit);
			this.Controls.Add(this.InformationTextBox);
			this.Controls.Add(this.IncidentCodeDropEdit);
			this.Controls.Add(this.TransportMeansGroupUserControl);
			this.Name = "IncidentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 271, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncidentCodeDropEdit.ResumeLayout(true);
			this.IncidentCodeDropEdit.PerformLayout();
			this.EndorsementDateEdit.ResumeLayout(true);
			this.EndorsementDateEdit.PerformLayout();
			this.EndorsementCountryCodeDropEdit.ResumeLayout(true);
			this.EndorsementCountryCodeDropEdit.PerformLayout();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.EventCountryCodeDropEdit.ResumeLayout(true);
			this.EventCountryCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit IncidentCodeDropEdit;
		internal ZArchitecture.ZTextBox InformationTextBox;
		internal ZArchitecture.GUI.ZDateEdit EndorsementDateEdit;
		internal ZArchitecture.ZTextBox EndorsementAuthorityTextBox;
		internal ZArchitecture.GUI.ZDropEdit EndorsementCountryCodeDropEdit;
		internal ZArchitecture.ZTextBox EndorsementPlaceTextBox;
		internal EU.GUI.LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal ZArchitecture.GUI.ZDropEdit EventCountryCodeDropEdit;
		internal Phase5TransportMeansGroupUserControl TransportMeansGroupUserControl;
	}
}
