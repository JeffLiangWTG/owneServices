namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class DepartureDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CountryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDispatchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TirCarnetNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommunicationLanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AdditionalDeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationOfGoodsUserControl = new LocationOfGoodsUserControl();
			this.DateLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SimplifiedProcedureAndReducedDataSetUserControl = new Enterprise.Customs.EU.NCTS.GUI.SimplifiedProcedureAndReducedDataSetUserControl();
			this.PresentationDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.TimeLimitForTransitCalcEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.OverrideFreightDetailsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommercialReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.SecurityDropEdit.SuspendLayout();
			this.CommunicationLanguageDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.AdditionalDeclarationTypeDropEdit.SuspendLayout();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.DateLimitDateEdit.SuspendLayout();
			this.SimplifiedProcedureAndReducedDataSetUserControl.SuspendLayout();
			this.PresentationDateTimeOffsetEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// CountryOfDestinationDropEdit
			// 
			this.CountryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationDropEdit, "MovementHeader.BM_RL_NKDestinationPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_RL_NKDestinationPort)));
			this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 180, true);
			this.CountryOfDestinationDropEdit.Name = "CountryOfDestinationDropEdit";
			this.CountryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.CountryOfDestinationDropEdit.TabIndex = 6;
			// 
			// CountryOfDispatchDropEdit
			// 
			this.CountryOfDispatchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDispatchDropEdit, "MovementHeader.BM_RN_NKCountryOfDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_RN_NKCountryOfDispatch)));
			this.CountryOfDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 150, true);
			this.CountryOfDispatchDropEdit.Name = "CountryOfDispatchDropEdit";
			this.CountryOfDispatchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.CountryOfDispatchDropEdit.TabIndex = 5;
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "MovementHeader.BM_InBondEntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_InBondEntryType)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 50, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 20, true);
			this.DeclarationTypeDropEdit.TabIndex = 1;
			// 
			// CustomerReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomerReferenceNumberTextBox, "MovementHeader.BM_PaperlessInbondNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PaperlessInbondNum)));
			this.CustomerReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 20, true);
			this.CustomerReferenceNumberTextBox.Name = "CustomerReferenceNumberTextBox";
			this.CustomerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.CustomerReferenceNumberTextBox.TabIndex = 0;
			this.CustomerReferenceNumberTextBox.TabStop = false;
			// 
			// TirCarnetNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TirCarnetNumberTextBox, "MovementHeader.TirCarnetNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.TirCarnetNumber)));
			this.TirCarnetNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 80, true);
			this.TirCarnetNumberTextBox.Name = "TirCarnetNumberTextBox";
			this.TirCarnetNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TirCarnetNumberTextBox.TabIndex = 2;
			// 
			// SecurityDropEdit
			// 
			this.SecurityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDropEdit, "MovementHeader.BM_TypeOfSecurity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_TypeOfSecurity)));
			this.SecurityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 210, true);
			this.SecurityDropEdit.Name = "SecurityDropEdit";
			this.SecurityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.SecurityDropEdit.TabIndex = 7;
			// 
			// CommunicationLanguageDropEdit
			// 
			this.CommunicationLanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommunicationLanguageDropEdit, "BH_CommunicationLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_CommunicationLanguage)));
			this.CommunicationLanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 406, true);
			this.CommunicationLanguageDropEdit.Name = "CommunicationLanguageDropEdit";
			this.CommunicationLanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CommunicationLanguageDropEdit.TabIndex = 13;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_GrossWeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "MovementHeader.BM_GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "MovementHeader.BM_GrossWeightUQ";
			this.GrossWeightCalcDropEdit.Decimals = 6;
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 258, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 8;
			// 
			// AdditionalDeclarationTypeDropEdit
			// 
			this.AdditionalDeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalDeclarationTypeDropEdit, "MovementHeader.BM_AdditionalDeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_AdditionalDeclarationType)));
			this.AdditionalDeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 294, true);
			this.AdditionalDeclarationTypeDropEdit.Name = "AdditionalDeclarationTypeDropEdit";
			this.AdditionalDeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AdditionalDeclarationTypeDropEdit.TabIndex = 9;
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, "MovementHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader)));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 323, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
			this.LocationOfGoodsUserControl.TabIndex = 10;
			// 
			// DateLimitDateEdit
			// 
			this.DateLimitDateEdit.AllowDrop = true;
			this.DateLimitDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateLimitDateEdit, "MovementHeader.BM_ExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_ExportDate)));
			this.DateLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 350, true);
			this.DateLimitDateEdit.Name = "DateLimitDateEdit";
			this.DateLimitDateEdit.TabIndex = 11;
			// 
			// SimplifiedProcedureAndReducedDataSetUserControl
			// 
			this.SimplifiedProcedureAndReducedDataSetUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SimplifiedProcedureAndReducedDataSetUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)))));
			this.SimplifiedProcedureAndReducedDataSetUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 376, true);
			this.SimplifiedProcedureAndReducedDataSetUserControl.Name = "SimplifiedProcedureAndReducedDataSetUserControl";
			this.SimplifiedProcedureAndReducedDataSetUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 26, true);
			this.SimplifiedProcedureAndReducedDataSetUserControl.TabIndex = 12;
			// 
			// PresentationDateTimeOffsetEdit
			// 
			this.PresentationDateTimeOffsetEdit.AllowDrop = true;
			this.PresentationDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationDateTimeOffsetEdit, "MovementHeader.BM_PresentationDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_PresentationDateTime)));
			this.PresentationDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 232, true);
			this.PresentationDateTimeOffsetEdit.Name = "PresentationDateTimeOffsetEdit";
			this.PresentationDateTimeOffsetEdit.TabIndex = 12;
			// 
			// TimeLimitForTransitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TimeLimitForTransitCalcEdit, "MovementHeader.BM_ExportTimeLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_ExportTimeLimit)));
			this.TimeLimitForTransitCalcEdit.DecimalPlaces = 0;
			this.TimeLimitForTransitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 432, true);
			this.TimeLimitForTransitCalcEdit.Name = "TimeLimitForTransitCalcEdit";
			this.TimeLimitForTransitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.TimeLimitForTransitCalcEdit.TabIndex = 11;
			this.TimeLimitForTransitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverrideFreightDetailsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideFreightDetailsCheckBox, "BH_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_OverrideFreightDefaults)));
			this.OverrideFreightDetailsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 109, true);
			this.OverrideFreightDetailsCheckBox.Name = "OverrideFreightDetailsCheckBox";
			this.OverrideFreightDetailsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 24, true);
			this.OverrideFreightDetailsCheckBox.TabIndex = 14;
			this.OverrideFreightDetailsCheckBox.UseVisualStyleBackColor = true;
			// 
			// CommercialReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommercialReferenceNumberTextBox, "MovementHeader.BM_UniqueConsignmentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_UniqueConsignmentReference)));
			this.CommercialReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommercialReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 123, true);
			this.CommercialReferenceNumberTextBox.Name = "CommercialReferenceNumberTextBox";
			this.CommercialReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 20, true);
			this.CommercialReferenceNumberTextBox.TabIndex = 4;
			// 
			// DepartureDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OverrideFreightDetailsCheckBox);
			this.Controls.Add(this.PresentationDateTimeOffsetEdit);
			this.Controls.Add(this.SimplifiedProcedureAndReducedDataSetUserControl);
			this.Controls.Add(this.DateLimitDateEdit);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.AdditionalDeclarationTypeDropEdit);
			this.Controls.Add(this.CustomerReferenceNumberTextBox);
			this.Controls.Add(this.DeclarationTypeDropEdit);
			this.Controls.Add(this.TirCarnetNumberTextBox);
			this.Controls.Add(this.CountryOfDispatchDropEdit);
			this.Controls.Add(this.CountryOfDestinationDropEdit);
			this.Controls.Add(this.SecurityDropEdit);
			this.Controls.Add(this.CommunicationLanguageDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.TimeLimitForTransitCalcEdit);
			this.Controls.Add(this.CommercialReferenceNumberTextBox);
			this.Name = "DepartureDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 459, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.SecurityDropEdit.ResumeLayout(true);
			this.SecurityDropEdit.PerformLayout();
			this.CommunicationLanguageDropEdit.ResumeLayout(true);
			this.CommunicationLanguageDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.AdditionalDeclarationTypeDropEdit.ResumeLayout(true);
			this.AdditionalDeclarationTypeDropEdit.PerformLayout();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.DateLimitDateEdit.ResumeLayout(true);
			this.DateLimitDateEdit.PerformLayout();
			this.SimplifiedProcedureAndReducedDataSetUserControl.ResumeLayout(true);
			this.SimplifiedProcedureAndReducedDataSetUserControl.PerformLayout();
			this.PresentationDateTimeOffsetEdit.ResumeLayout(true);
			this.PresentationDateTimeOffsetEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox CustomerReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit DeclarationTypeDropEdit;
		internal ZArchitecture.ZTextBox TirCarnetNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDispatchDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDestinationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SecurityDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZDropEdit AdditionalDeclarationTypeDropEdit;
		internal LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal ZArchitecture.GUI.ZDateEdit DateLimitDateEdit;
		internal SimplifiedProcedureAndReducedDataSetUserControl SimplifiedProcedureAndReducedDataSetUserControl;
		internal ZArchitecture.GUI.ZDateTimeOffsetEdit PresentationDateTimeOffsetEdit;
		internal ZArchitecture.GUI.ZDropEdit CommunicationLanguageDropEdit;
		internal ZArchitecture.GUI.ZIntEdit TimeLimitForTransitCalcEdit;
		internal ZArchitecture.GUI.ZCheckBox OverrideFreightDetailsCheckBox;
		internal ZArchitecture.ZTextBox CommercialReferenceNumberTextBox;

	}
}

