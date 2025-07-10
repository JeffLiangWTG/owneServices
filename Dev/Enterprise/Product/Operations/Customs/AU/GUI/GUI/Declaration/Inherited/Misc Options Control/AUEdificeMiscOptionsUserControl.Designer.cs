using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUEdificeMiscOptionsUserControl
	{
		private void InitializeComponent()
		{
			this.CompilePrintersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryPrinterNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.clearanceAdvicePrinterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eFTReceiptPrinterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NumberOfEntryPrintsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ForcePrimeEnclosureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ManifestClientIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CCANTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExcisableGoodsHiddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrescribedGoodsHiddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.landedCostingDefaultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByCostBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByUnitsBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByVolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByWeightBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.eXDOCOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.certificateRequestCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.jE_UseOwnerRefAsQuarantineRefCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MiscOptionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompilePrintersGroupBox.SuspendLayout();
			this.landedCostingDefaultsGroupBox.SuspendLayout();
			this.eXDOCOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 73, true);
			this.PaymentPartyDropEdit.TabIndex = 5;
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.PrescribedGoodsHiddenCheckBox);
			this.MiscOptionsGroupBox.Controls.Add(this.CCANTextBox);
			this.MiscOptionsGroupBox.Controls.Add(this.ManifestClientIDTextBox);
			this.MiscOptionsGroupBox.Controls.Add(this.ForcePrimeEnclosureCheckBox);
			this.MiscOptionsGroupBox.Controls.Add(this.ExcisableGoodsHiddenCheckBox);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 232, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.ExcisableGoodsHiddenCheckBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.ForcePrimeEnclosureCheckBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.ManifestClientIDTextBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.CCANTextBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PrescribedGoodsHiddenCheckBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 24, true);
			// 
			// MergeByDropEdit
			// 
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 97, true);
			this.MergeByDropEdit.TabIndex = 7;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 49, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// CompilePrintersGroupBox
			// 
			this.CompilePrintersGroupBox.Controls.Add(this.EntryPrinterNumberTextBox);
			this.CompilePrintersGroupBox.Controls.Add(this.clearanceAdvicePrinterTextBox);
			this.CompilePrintersGroupBox.Controls.Add(this.eFTReceiptPrinterTextBox);
			this.CompilePrintersGroupBox.Controls.Add(this.NumberOfEntryPrintsCalcEdit);
			this.CompilePrintersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 246, true);
			this.CompilePrintersGroupBox.Name = "CompilePrintersGroupBox";
			this.CompilePrintersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 120, true);
			this.CompilePrintersGroupBox.TabIndex = 1;
			this.CompilePrintersGroupBox.TabStop = false;
			this.CompilePrintersGroupBox.Text = "Compile Printers";
			// 
			// EntryPrinterNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryPrinterNumberTextBox, "AddInfo+ZA_PrinterNumber_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_PrinterNumber_Hidden)));
			this.EntryPrinterNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9c41e35b-a543-4f01-b521-728aca6f01c8", "Entry Printer Number");
			this.EntryPrinterNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 16, true);
			this.EntryPrinterNumberTextBox.Name = "EntryPrinterNumberTextBox";
			this.EntryPrinterNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.EntryPrinterNumberTextBox.TabIndex = 1;
			// 
			// ClearanceAdvicePrinterTextBox
			// 
			this.BindingSource.SetBindingMember(this.clearanceAdvicePrinterTextBox, "AddInfo+ZA_ClearanceAdvicePrinter_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_ClearanceAdvicePrinter_Hidden)));
			this.clearanceAdvicePrinterTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("b26598aa-9e2b-48aa-b810-a2f71d47db24", "Clearance Advice Printer");
			this.clearanceAdvicePrinterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 88, true);
			this.clearanceAdvicePrinterTextBox.Name = "ClearanceAdvicePrinterTextBox";
			this.clearanceAdvicePrinterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.clearanceAdvicePrinterTextBox.TabIndex = 9;
			// 
			// EFTReceiptPrinterTextBox
			// 
			this.BindingSource.SetBindingMember(this.eFTReceiptPrinterTextBox, "AddInfo+ZA_EFTReceiptPrinter_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_EFTReceiptPrinter_Hidden)));
			this.eFTReceiptPrinterTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("b9bb4323-d36b-4ef4-9596-1f3c2fcca51a", "EFT Receipt Printer");
			this.eFTReceiptPrinterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 64, true);
			this.eFTReceiptPrinterTextBox.Name = "EFTReceiptPrinterTextBox";
			this.eFTReceiptPrinterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.eFTReceiptPrinterTextBox.TabIndex = 6;
			// 
			// NumberOfEntryPrintsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfEntryPrintsCalcEdit, "AddInfo+ZA_NumberOfEntryPrints_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_NumberOfEntryPrints_Hidden)));
			this.NumberOfEntryPrintsCalcEdit.DecimalPlaces = 2;
			this.NumberOfEntryPrintsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 40, true);
			this.NumberOfEntryPrintsCalcEdit.Name = "NumberOfEntryPrintsCalcEdit";
			this.NumberOfEntryPrintsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 20, true);
			this.NumberOfEntryPrintsCalcEdit.TabIndex = 4;
			this.NumberOfEntryPrintsCalcEdit.Text = "0";
			this.NumberOfEntryPrintsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ForcePrimeEnclosureCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ForcePrimeEnclosureCheckBox, "JE_ForcePrimeEnclosure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_ForcePrimeEnclosure)));
			this.ForcePrimeEnclosureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForcePrimeEnclosureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 196, true);
			this.ForcePrimeEnclosureCheckBox.Name = "ForcePrimeEnclosureCheckBox";
			this.ForcePrimeEnclosureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 24, true);
			this.ForcePrimeEnclosureCheckBox.TabIndex = 14;
			this.ForcePrimeEnclosureCheckBox.Text = "Always Send Declaration Prime-Enclosure If Multiple Entries";
			// 
			// ManifestClientIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManifestClientIDTextBox, "AddInfo+ZA_ManifestClientIDOverride_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_ManifestClientIDOverride_Hidden)));
			this.ManifestClientIDTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8407f0c0-20a5-49d9-9668-cb8ad7be0ad9", "Manifest Client ID");
			this.ManifestClientIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 172, true);
			this.ManifestClientIDTextBox.Name = "ManifestClientIDTextBox";
			this.ManifestClientIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ManifestClientIDTextBox.TabIndex = 13;
			// 
			// CCANTextBox
			// 
			this.BindingSource.SetBindingMember(this.CCANTextBox, "ContingencyCAN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).ContingencyCAN)));
			this.CCANTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeMiscOptionsUserControl|a653bf90-0549-4d76-9b4b-0123c16be372", "C-CAN", "Contingency CAN", "");
			this.CCANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 124, true);
			this.CCANTextBox.Name = "CCANTextBox";
			this.CCANTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CCANTextBox.TabIndex = 9;
			// 
			// ExcisableGoodsHiddenCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExcisableGoodsHiddenCheckBox, "AddInfo.ZA_ExcisableGoods_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_ExcisableGoods_Hidden)));
			this.ExcisableGoodsHiddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcisableGoodsHiddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 148, true);
			this.ExcisableGoodsHiddenCheckBox.Name = "ExcisableGoodsHiddenCheckBox";
			this.ExcisableGoodsHiddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.ExcisableGoodsHiddenCheckBox.TabIndex = 10;
			this.ExcisableGoodsHiddenCheckBox.Text = "Excisable Goods";
			// 
			// PrescribedGoodsHiddenCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PrescribedGoodsHiddenCheckBox, "AddInfo.ZA_PrescribedGoods_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_PrescribedGoods_Hidden)));
			this.PrescribedGoodsHiddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrescribedGoodsHiddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 148, true);
			this.PrescribedGoodsHiddenCheckBox.Name = "PrescribedGoodsHiddenCheckBox";
			this.PrescribedGoodsHiddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
			this.PrescribedGoodsHiddenCheckBox.TabIndex = 11;
			this.PrescribedGoodsHiddenCheckBox.Text = "Prescribed Goods";
			// 
			// LandedCostingDefaultsGroupBox
			// 
			this.landedCostingDefaultsGroupBox.Controls.Add(this.zLabel10);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByCostBoundCalcEdit);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.zLabel8);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByUnitsBoundCalcEdit);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.zLabel6);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByVolumeBoundCalcEdit);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.zLabel5);
			this.landedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByWeightBoundCalcEdit);
			this.landedCostingDefaultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 8, true);
			this.landedCostingDefaultsGroupBox.Name = "LandedCostingDefaultsGroupBox";
			this.landedCostingDefaultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 128, true);
			this.landedCostingDefaultsGroupBox.TabIndex = 2;
			this.landedCostingDefaultsGroupBox.TabStop = false;
			this.landedCostingDefaultsGroupBox.Text = "Landed Costing Defaults";
			// 
			// zLabel10
			// 
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 92, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel10.TabIndex = 11;
			this.zLabel10.Text = "%";
			// 
			// JE_LandedCostByCostBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByCostBoundCalcEdit, "JE_LandedCostByCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByCost)));
			this.JE_LandedCostByCostBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByCostBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 94, true);
			this.JE_LandedCostByCostBoundCalcEdit.Name = "JE_LandedCostByCostBoundCalcEdit";
			this.JE_LandedCostByCostBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JE_LandedCostByCostBoundCalcEdit.TabIndex = 10;
			this.JE_LandedCostByCostBoundCalcEdit.Text = "0";
			this.JE_LandedCostByCostBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 68, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel8.TabIndex = 8;
			this.zLabel8.Text = "%";
			// 
			// JE_LandedCostByUnitsBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByUnitsBoundCalcEdit, "JE_LandedCostByUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByUnits)));
			this.JE_LandedCostByUnitsBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByUnitsBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 70, true);
			this.JE_LandedCostByUnitsBoundCalcEdit.Name = "JE_LandedCostByUnitsBoundCalcEdit";
			this.JE_LandedCostByUnitsBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JE_LandedCostByUnitsBoundCalcEdit.TabIndex = 7;
			this.JE_LandedCostByUnitsBoundCalcEdit.Text = "0";
			this.JE_LandedCostByUnitsBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 44, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel6.TabIndex = 5;
			this.zLabel6.Text = "%";
			// 
			// JE_LandedCostByVolumeBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByVolumeBoundCalcEdit, "JE_LandedCostByVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByVolume)));
			this.JE_LandedCostByVolumeBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByVolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 46, true);
			this.JE_LandedCostByVolumeBoundCalcEdit.Name = "JE_LandedCostByVolumeBoundCalcEdit";
			this.JE_LandedCostByVolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JE_LandedCostByVolumeBoundCalcEdit.TabIndex = 4;
			this.JE_LandedCostByVolumeBoundCalcEdit.Text = "0";
			this.JE_LandedCostByVolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 20, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel5.TabIndex = 2;
			this.zLabel5.Text = "%";
			// 
			// JE_LandedCostByWeightBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByWeightBoundCalcEdit, "JE_LandedCostByWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByWeight)));
			this.JE_LandedCostByWeightBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByWeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 22, true);
			this.JE_LandedCostByWeightBoundCalcEdit.Name = "JE_LandedCostByWeightBoundCalcEdit";
			this.JE_LandedCostByWeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.JE_LandedCostByWeightBoundCalcEdit.TabIndex = 1;
			this.JE_LandedCostByWeightBoundCalcEdit.Text = "0";
			this.JE_LandedCostByWeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EXDOCOptionsGroupBox
			// 
			this.eXDOCOptionsGroupBox.Controls.Add(this.certificateRequestCheckBox);
			this.eXDOCOptionsGroupBox.Controls.Add(this.jE_UseOwnerRefAsQuarantineRefCheckBox);
			this.eXDOCOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 142, true);
			this.eXDOCOptionsGroupBox.Name = "EXDOCOptionsGroupBox";
			this.eXDOCOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 65, true);
			this.eXDOCOptionsGroupBox.TabIndex = 3;
			this.eXDOCOptionsGroupBox.TabStop = false;
			this.eXDOCOptionsGroupBox.Text = "EXDOC Options";
			// 
			// CertificateRequestCheckBox
			// 
			this.certificateRequestCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.certificateRequestCheckBox, "IsAQISCertificateRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).IsAQISCertificateRequest)));
			this.certificateRequestCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeMiscOptionsUserControl|f39eb212-3f1c-4d23-90fd-482150fe140c", "Certificate Request", "Indicates the job is a Certificate Request.");
			this.certificateRequestCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.certificateRequestCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 41, true);
			this.certificateRequestCheckBox.Name = "CertificateRequestCheckBox";
			this.certificateRequestCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.certificateRequestCheckBox.TabIndex = 1;
			this.certificateRequestCheckBox.UseVisualStyleBackColor = true;
			// 
			// JE_UseOwnerRefAsQuarantineRefCheckBox
			// 
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.jE_UseOwnerRefAsQuarantineRefCheckBox, "JE_UseOwnerRefAsQuarantineRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_UseOwnerRefAsQuarantineRef)));
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21, true);
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.Name = "JE_UseOwnerRefAsQuarantineRefCheckBox";
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 17, true);
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.TabIndex = 0;
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.Text = "Use Owners Reference as Exporters Reference";
			this.jE_UseOwnerRefAsQuarantineRefCheckBox.UseVisualStyleBackColor = true;
			// 
			// AUEdificeMiscOptionsUserControl
			// 
			this.Controls.Add(this.eXDOCOptionsGroupBox);
			this.Controls.Add(this.landedCostingDefaultsGroupBox);
			this.Controls.Add(this.CompilePrintersGroupBox);
			this.Name = "AUEdificeMiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 384, true);
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.CompilePrintersGroupBox, 0);
			this.Controls.SetChildIndex(this.landedCostingDefaultsGroupBox, 0);
			this.Controls.SetChildIndex(this.eXDOCOptionsGroupBox, 0);
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompilePrintersGroupBox.ResumeLayout(false);
			this.CompilePrintersGroupBox.PerformLayout();
			this.landedCostingDefaultsGroupBox.ResumeLayout(false);
			this.landedCostingDefaultsGroupBox.PerformLayout();
			this.eXDOCOptionsGroupBox.ResumeLayout(false);
			this.eXDOCOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		protected internal ZGroupBox CompilePrintersGroupBox;
		protected internal ZArchitecture.ZCalcEdit NumberOfEntryPrintsCalcEdit;
		protected internal ZCheckBox ForcePrimeEnclosureCheckBox;
		private ZArchitecture.ZTextBox clearanceAdvicePrinterTextBox;
		private ZArchitecture.ZTextBox eFTReceiptPrinterTextBox;
		//		private ZArchitecture.ZTextBox PrinterNumberTextBox;
		protected internal ZArchitecture.ZTextBox ManifestClientIDTextBox;
		protected internal ZArchitecture.ZTextBox EntryPrinterNumberTextBox;
		protected internal ZArchitecture.ZTextBox CCANTextBox;
		protected internal ZCheckBox ExcisableGoodsHiddenCheckBox;
		protected internal ZCheckBox PrescribedGoodsHiddenCheckBox;
		private ZGroupBox landedCostingDefaultsGroupBox;
		protected internal ZArchitecture.ZLabel zLabel10;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByCostBoundCalcEdit;
		protected internal ZArchitecture.ZLabel zLabel8;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByUnitsBoundCalcEdit;
		protected internal ZArchitecture.ZLabel zLabel6;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByVolumeBoundCalcEdit;
		protected internal ZArchitecture.ZLabel zLabel5;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByWeightBoundCalcEdit;
		protected ZGroupBox eXDOCOptionsGroupBox;
		protected ZCheckBox jE_UseOwnerRefAsQuarantineRefCheckBox;
		protected ZCheckBox certificateRequestCheckBox;
	}
}
