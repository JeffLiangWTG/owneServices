namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CountryOfDispatchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ReferenceNumberUCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportMoPDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.LinePriceCurrencyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfDispatchDropEdit.SuspendLayout();
			this.CountryOfDestinationDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.TransportMoPDropEdit.SuspendLayout();
			this.ConsignorDocAddressControl.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.LinePriceCurrencyDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsBill);
			// 
			// CountryOfDispatchDropEdit
			// 
			this.CountryOfDispatchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDispatchDropEdit, "B0_RN_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_RN_NKCountryOfExport)));
			this.CountryOfDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 26, true);
			this.CountryOfDispatchDropEdit.Name = "CountryOfDispatchDropEdit";
			this.CountryOfDispatchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 22, true);
			this.CountryOfDispatchDropEdit.TabIndex = 0;
			// 
			// CountryOfDestinationDropEdit
			// 
			this.CountryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfDestinationDropEdit, "B0_RN_NKCountryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_RN_NKCountryOfDestination)));
			this.CountryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 59, true);
			this.CountryOfDestinationDropEdit.Name = "CountryOfDestinationDropEdit";
			this.CountryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 22, true);
			this.CountryOfDestinationDropEdit.TabIndex = 1;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_WeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "B0_Weight";
			this.GrossWeightCalcDropEdit.BindToUnit = "B0_WeightUQ";
			this.GrossWeightCalcDropEdit.Decimals = 6;
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 91, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 22, true);
			this.GrossWeightCalcDropEdit.TabIndex = 2;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ReferenceNumberUCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberUCRTextBox, "B0_ReferenceID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_ReferenceID)));
			this.ReferenceNumberUCRTextBox.CaptionResourceString = null;
			this.ReferenceNumberUCRTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberUCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 124, true);
			this.ReferenceNumberUCRTextBox.Name = "ReferenceNumberUCRTextBox";
			this.ReferenceNumberUCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 22, true);
			this.ReferenceNumberUCRTextBox.TabIndex = 3;
			// 
			// TransportMoPDropEdit
			// 
			this.TransportMoPDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportMoPDropEdit, "B0_TransportPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_TransportPaymentMethod)));
			this.TransportMoPDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 162, true);
			this.TransportMoPDropEdit.Name = "TransportMoPDropEdit";
			this.TransportMoPDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 22, true);
			this.TransportMoPDropEdit.TabIndex = 4;
			// 
			// ConsignorDocAddressControl
			// 
			this.ConsignorDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsignorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocAddressControl, "Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).Consignor)));
			this.ConsignorDocAddressControl.BindToOrganisations = "Lookups.Consignors";
			this.ConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B554800D-1146-4789-B2CE-CE0C734EA436", "Consignor");
			this.ConsignorDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsignorDocAddressControl, false);
			this.ConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 201, true);
			this.ConsignorDocAddressControl.Name = "ConsignorDocAddressControl";
			this.ConsignorDocAddressControl.ReadOnly = false;
			this.ConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsignorDocAddressControl.TabIndex = 5;
			this.ConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).Consignee)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("FA23510C-0F76-4837-B87D-261CEEFA7316", "Consignee");
			this.ConsigneeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocAddressControl, false);
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 201, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 6;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// LinePriceCurrencyDropEdit
			// 
			this.LinePriceCurrencyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LinePriceCurrencyDropEdit, "B0_RX_NKLinePriceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).B0_RX_NKLinePriceCurrency)));
			this.LinePriceCurrencyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 337, true);
			this.LinePriceCurrencyDropEdit.Name = "LinePriceCurrencyDropEdit";
			this.LinePriceCurrencyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.LinePriceCurrencyDropEdit.TabIndex = 7;
			// 
			// HouseConsignmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LinePriceCurrencyDropEdit);
			this.Controls.Add(this.CountryOfDispatchDropEdit);
			this.Controls.Add(this.CountryOfDestinationDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.ReferenceNumberUCRTextBox);
			this.Controls.Add(this.TransportMoPDropEdit);
			this.Controls.Add(this.ConsignorDocAddressControl);
			this.Controls.Add(this.ConsigneeDocAddressControl);
			this.Name = "HouseConsignmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 433, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfDispatchDropEdit.ResumeLayout(true);
			this.CountryOfDispatchDropEdit.PerformLayout();
			this.CountryOfDestinationDropEdit.ResumeLayout(true);
			this.CountryOfDestinationDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.TransportMoPDropEdit.ResumeLayout(true);
			this.TransportMoPDropEdit.PerformLayout();
			this.ConsignorDocAddressControl.ResumeLayout(true);
			this.ConsignorDocAddressControl.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.LinePriceCurrencyDropEdit.ResumeLayout(true);
			this.LinePriceCurrencyDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CountryOfDispatchDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfDestinationDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZArchitecture.ZTextBox ReferenceNumberUCRTextBox;
		internal ZArchitecture.GUI.ZDropEdit TransportMoPDropEdit;
		internal MasterFiles.GUI.ZDocAddressControl ConsignorDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		internal ZArchitecture.GUI.ZDropEdit LinePriceCurrencyDropEdit;
	}
}
