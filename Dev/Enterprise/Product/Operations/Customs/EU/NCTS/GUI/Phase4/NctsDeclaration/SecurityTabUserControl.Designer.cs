namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class SecurityTabUserControl
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
			this.SecurityTradersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierAddressControl = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SecurityConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SecurityConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SecurityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PlaceOfUnloadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PlaceOfUnloadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportChargesMoPDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConveyanceReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommercialReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SpecificCircumstanceIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ItineraryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.itineraryUserControl1 = new Enterprise.Customs.EU.NCTS.GUI.ItineraryUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SecurityTradersGroupBox.SuspendLayout();
			this.CarrierAddressControl.SuspendLayout();
			this.SecurityConsigneeDocAddressControl.SuspendLayout();
			this.SecurityConsignorDocAddressControl.SuspendLayout();
			this.SecurityDetailsGroupBox.SuspendLayout();
			this.PlaceOfUnloadingFindBox.SuspendLayout();
			this.TransportChargesMoPDropEdit.SuspendLayout();
			this.SpecificCircumstanceIndicatorDropEdit.SuspendLayout();
			this.ItineraryGroupBox.SuspendLayout();
			this.itineraryUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// SecurityTradersGroupBox
			// 
			this.SecurityTradersGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9C47282B-4E62-29F0-E459-6F704281C95A", "Security Traders");
			this.SecurityTradersGroupBox.Controls.Add(this.CarrierAddressControl);
			this.SecurityTradersGroupBox.Controls.Add(this.SecurityConsigneeDocAddressControl);
			this.SecurityTradersGroupBox.Controls.Add(this.SecurityConsignorDocAddressControl);
			this.SecurityTradersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.SecurityTradersGroupBox.Name = "SecurityTradersGroupBox";
			this.SecurityTradersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 588, true);
			this.SecurityTradersGroupBox.TabIndex = 0;
			this.SecurityTradersGroupBox.TabStop = false;
			// 
			// CarrierAddressControl
			// 
			this.CarrierAddressControl.AllowDrop = true;
			this.CarrierAddressControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CarrierAddressControl, "BH_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_OH_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Lookups.Carriers)));
			this.CarrierAddressControl.BindToList = "Lookups.Carriers";
			this.CarrierAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b4cfad7c-e74f-471b-9d84-71b4dd73ec32", "Carrier");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CarrierAddressControl, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.CarrierAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 409, true);
			this.CarrierAddressControl.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CarrierAddressControl.Name = "CarrierAddressControl";
			this.CarrierAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.CarrierAddressControl.TabIndex = 2;
			// 
			// SecurityConsigneeDocAddressControl
			// 
			this.SecurityConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityConsigneeDocAddressControl, "SecurityConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).SecurityConsignee)));
			this.SecurityConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.SecurityConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2d100622-52f7-452d-a954-9bbcc52cd2e5", "Security Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SecurityConsigneeDocAddressControl, false);
			this.SecurityConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 207, true);
			this.SecurityConsigneeDocAddressControl.Name = "SecurityConsigneeDocAddressControl";
			this.SecurityConsigneeDocAddressControl.ReadOnly = false;
			this.SecurityConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SecurityConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SecurityConsigneeDocAddressControl.TabIndex = 1;
			this.SecurityConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// SecurityConsignorDocAddressControl
			// 
			this.SecurityConsignorDocAddressControl.AllowDrop = true;
			this.SecurityConsignorDocAddressControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SecurityConsignorDocAddressControl, "SecurityConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).SecurityConsignor)));
			this.SecurityConsignorDocAddressControl.BindToOrganisations = "Lookups.Consignors";
			this.SecurityConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("159bcbaf-f737-4129-8519-fc6db02d7056", "Security Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SecurityConsignorDocAddressControl, false);
			this.SecurityConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SecurityConsignorDocAddressControl.Name = "SecurityConsignorDocAddressControl";
			this.SecurityConsignorDocAddressControl.ReadOnly = false;
			this.SecurityConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.SecurityConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.SecurityConsignorDocAddressControl.TabIndex = 0;
			this.SecurityConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// SecurityDetailsGroupBox
			// 
			this.SecurityDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("38C20631-9B31-15EB-E85D-C192E18AC6A0", "Safety and Security Details");
			this.SecurityDetailsGroupBox.Controls.Add(this.PlaceOfUnloadingTextBox);
			this.SecurityDetailsGroupBox.Controls.Add(this.PlaceOfUnloadingFindBox);
			this.SecurityDetailsGroupBox.Controls.Add(this.TransportChargesMoPDropEdit);
			this.SecurityDetailsGroupBox.Controls.Add(this.ConveyanceReferenceNumberTextBox);
			this.SecurityDetailsGroupBox.Controls.Add(this.CommercialReferenceNumberTextBox);
			this.SecurityDetailsGroupBox.Controls.Add(this.SpecificCircumstanceIndicatorDropEdit);
			this.SecurityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 0, true);
			this.SecurityDetailsGroupBox.Name = "SecurityDetailsGroupBox";
			this.SecurityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 588, true);
			this.SecurityDetailsGroupBox.TabIndex = 1;
			this.SecurityDetailsGroupBox.TabStop = false;
			// 
			// PlaceOfUnloadingTextBox
			// 
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingTextBox, "MovementHeader.BM_PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PlaceOfUnloading)));
			this.PlaceOfUnloadingTextBox.CaptionResourceString = null;
			this.PlaceOfUnloadingTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PlaceOfUnloadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 70, true);
			this.PlaceOfUnloadingTextBox.Name = "PlaceOfUnloadingTextBox";
			this.PlaceOfUnloadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.PlaceOfUnloadingTextBox.TabIndex = 2;
			// 
			// PlaceOfUnloadingFindBox
			// 
			this.PlaceOfUnloadingFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfUnloadingFindBox, "MovementHeader.BM_PlaceOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PlaceOfUnloading)));
			this.PlaceOfUnloadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 70, true);
			this.PlaceOfUnloadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PlaceOfUnloadingFindBox.Name = "PlaceOfUnloadingFindBox";
			this.PlaceOfUnloadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.PlaceOfUnloadingFindBox.TabIndex = 2;
			// 
			// TransportChargesMoPDropEdit
			// 
			this.TransportChargesMoPDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportChargesMoPDropEdit, "MovementHeader.BM_MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_MethodOfPayment)));
			this.TransportChargesMoPDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 96, true);
			this.TransportChargesMoPDropEdit.Name = "TransportChargesMoPDropEdit";
			this.TransportChargesMoPDropEdit.ShouldResizeByMaxLength = true;
			this.TransportChargesMoPDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.TransportChargesMoPDropEdit.TabIndex = 3;
			// 
			// ConveyanceReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConveyanceReferenceNumberTextBox, "MovementHeader.BM_ConveyanceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_ConveyanceNumber)));
			this.ConveyanceReferenceNumberTextBox.CaptionResourceString = null;
			this.ConveyanceReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConveyanceReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 44, true);
			this.ConveyanceReferenceNumberTextBox.Name = "ConveyanceReferenceNumberTextBox";
			this.ConveyanceReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.ConveyanceReferenceNumberTextBox.TabIndex = 1;
			// 
			// CommercialReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommercialReferenceNumberTextBox, "MovementHeader.BM_AdditionalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_AdditionalText)));
			this.CommercialReferenceNumberTextBox.CaptionResourceString = null;
			this.CommercialReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommercialReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 122, true);
			this.CommercialReferenceNumberTextBox.Name = "CommercialReferenceNumberTextBox";
			this.CommercialReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.CommercialReferenceNumberTextBox.TabIndex = 4;
			// 
			// SpecificCircumstanceIndicatorDropEdit
			// 
			this.SpecificCircumstanceIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificCircumstanceIndicatorDropEdit, "MovementHeader.BM_BTAIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_BTAIndicator)));
			this.SpecificCircumstanceIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 18, true);
			this.SpecificCircumstanceIndicatorDropEdit.Name = "SpecificCircumstanceIndicatorDropEdit";
			this.SpecificCircumstanceIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.SpecificCircumstanceIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.SpecificCircumstanceIndicatorDropEdit.TabIndex = 0;
			// 
			// ItineraryGroupBox
			// 
			this.ItineraryGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("713021C7-6C62-9162-B5D4-FD9F85D80E41", "Itinerary Countries of Routing");
			this.ItineraryGroupBox.Controls.Add(this.itineraryUserControl1);
			this.ItineraryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(707, 0, true);
			this.ItineraryGroupBox.Name = "ItineraryGroupBox";
			this.ItineraryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 179, true);
			this.ItineraryGroupBox.TabIndex = 2;
			this.ItineraryGroupBox.TabStop = false;
			// 
			// itineraryUserControl1
			// 
			this.itineraryUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.itineraryUserControl1, ".");
			this.itineraryUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.itineraryUserControl1.Name = "itineraryUserControl1";
			this.itineraryUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 151, true);
			this.itineraryUserControl1.TabIndex = 0;
			// 
			// SecurityTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItineraryGroupBox);
			this.Controls.Add(this.SecurityTradersGroupBox);
			this.Controls.Add(this.SecurityDetailsGroupBox);
			this.Name = "SecurityTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 660, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SecurityTradersGroupBox.ResumeLayout(false);
			this.SecurityTradersGroupBox.PerformLayout();
			this.CarrierAddressControl.ResumeLayout(true);
			this.CarrierAddressControl.PerformLayout();
			this.SecurityConsigneeDocAddressControl.ResumeLayout(true);
			this.SecurityConsigneeDocAddressControl.PerformLayout();
			this.SecurityConsignorDocAddressControl.ResumeLayout(true);
			this.SecurityConsignorDocAddressControl.PerformLayout();
			this.SecurityDetailsGroupBox.ResumeLayout(false);
			this.SecurityDetailsGroupBox.PerformLayout();
			this.PlaceOfUnloadingFindBox.ResumeLayout(true);
			this.PlaceOfUnloadingFindBox.PerformLayout();
			this.TransportChargesMoPDropEdit.ResumeLayout(true);
			this.TransportChargesMoPDropEdit.PerformLayout();
			this.SpecificCircumstanceIndicatorDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceIndicatorDropEdit.PerformLayout();
			this.ItineraryGroupBox.ResumeLayout(false);
			this.ItineraryGroupBox.PerformLayout();
			this.itineraryUserControl1.ResumeLayout(true);
			this.itineraryUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SecurityTradersGroupBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox CarrierAddressControl;
		private MasterFiles.GUI.ZDocAddressControl SecurityConsigneeDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl SecurityConsignorDocAddressControl;
		protected ZArchitecture.GUI.ZGroupBox SecurityDetailsGroupBox;
		protected ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingFindBox;
		protected ZArchitecture.GUI.ZDropEdit TransportChargesMoPDropEdit;
		private ZArchitecture.ZTextBox ConveyanceReferenceNumberTextBox;
		protected ZArchitecture.ZTextBox CommercialReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit SpecificCircumstanceIndicatorDropEdit;
		private ZArchitecture.GUI.ZGroupBox ItineraryGroupBox;
		private ItineraryUserControl itineraryUserControl1;
		protected ZArchitecture.ZTextBox PlaceOfUnloadingTextBox;
	}
}
