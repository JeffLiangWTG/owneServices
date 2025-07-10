namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoOutturnDetailUserControl
	{
		private void InitializeComponent()
		{
			this.containerNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.sealNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.outternResultLabel = new Enterprise.ZArchitecture.ZLabel();
			this.houseBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.oceanBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.sealIntactCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.damagedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.pillagedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.receiptLabel = new Enterprise.ZArchitecture.ZLabel();
			this.unpackLabel = new Enterprise.ZArchitecture.ZLabel();
			this.packagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cargoTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.goodsDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.marksNumsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.receiptDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.unpackDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.cargoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.containerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.packagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.oceanBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.houseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.goodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.marksNumsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.sealNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.outturnResultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.outerPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.outerPacksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.customsStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.customsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.commercialStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.detailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.underbondStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.seaCargoInfoButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader);
			// 
			// ContainerNumberLabel
			// 
			this.containerNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 9, true);
			this.containerNumberLabel.Name = "ContainerNumberLabel";
			this.containerNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.containerNumberLabel.TabIndex = 10;
			this.containerNumberLabel.Text = "Container No.:";
			// 
			// SealNumberLabel
			// 
			this.sealNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 34, true);
			this.sealNumberLabel.Name = "SealNumberLabel";
			this.sealNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.sealNumberLabel.TabIndex = 12;
			this.sealNumberLabel.Text = "Seal Number:";
			// 
			// OutternResultLabel
			// 
			this.outternResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 109, true);
			this.outternResultLabel.Name = "OutternResultLabel";
			this.outternResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
			this.outternResultLabel.TabIndex = 18;
			this.outternResultLabel.Text = "Outturn Result:";
			// 
			// HouseBillLabel
			// 
			this.houseBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 109, true);
			this.houseBillLabel.Name = "HouseBillLabel";
			this.houseBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.houseBillLabel.TabIndex = 8;
			this.houseBillLabel.Text = "House Bill:";
			// 
			// OceanBillLabel
			// 
			this.oceanBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 84, true);
			this.oceanBillLabel.Name = "OceanBillLabel";
			this.oceanBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.oceanBillLabel.TabIndex = 6;
			this.oceanBillLabel.Text = "Ocean Bill:";
			// 
			// SealIntactCheckBox
			// 
			this.BindingSource.SetBindingMember(this.sealIntactCheckBox, "Outturns.C5_SealIntactIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_SealIntactIndicator)));
			this.sealIntactCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.sealIntactCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(905, 65, true);
			this.sealIntactCheckBox.Name = "SealIntactCheckBox";
			this.sealIntactCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
			this.sealIntactCheckBox.TabIndex = 33;
			this.sealIntactCheckBox.Text = "Seal Intact";
			// 
			// DamagedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.damagedCheckBox, "Outturns.C5_DamageIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_DamageIndicator)));
			this.damagedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.damagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(905, 15, true);
			this.damagedCheckBox.Name = "DamagedCheckBox";
			this.damagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
			this.damagedCheckBox.TabIndex = 31;
			this.damagedCheckBox.Text = "Damaged";
			// 
			// PillagedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.pillagedCheckBox, "Outturns.C5_PillageIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_PillageIndicator)));
			this.pillagedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.pillagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(905, 40, true);
			this.pillagedCheckBox.Name = "PillagedCheckBox";
			this.pillagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 24, true);
			this.pillagedCheckBox.TabIndex = 32;
			this.pillagedCheckBox.Text = "Pillaged";
			// 
			// ReceiptLabel
			// 
			this.receiptLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.receiptLabel.Name = "ReceiptLabel";
			this.receiptLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.receiptLabel.TabIndex = 0;
			this.receiptLabel.Text = "Receipt Date:";
			// 
			// UnpackLabel
			// 
			this.unpackLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 34, true);
			this.unpackLabel.Name = "UnpackLabel";
			this.unpackLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 13, true);
			this.unpackLabel.TabIndex = 2;
			this.unpackLabel.Text = "Unpack Date:";
			// 
			// PackagesLabel
			// 
			this.packagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 84, true);
			this.packagesLabel.Name = "PackagesLabel";
			this.packagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.packagesLabel.TabIndex = 16;
			this.packagesLabel.Text = "Packages:";
			// 
			// CargoTypeLabel
			// 
			this.cargoTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 59, true);
			this.cargoTypeLabel.Name = "CargoTypeLabel";
			this.cargoTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 13, true);
			this.cargoTypeLabel.TabIndex = 4;
			this.cargoTypeLabel.Text = "Cargo Type:";
			// 
			// GoodsDescriptionLabel
			// 
			this.goodsDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 2, true);
			this.goodsDescriptionLabel.Name = "GoodsDescriptionLabel";
			this.goodsDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.goodsDescriptionLabel.TabIndex = 20;
			this.goodsDescriptionLabel.Text = "Goods Description:";
			// 
			// MarksNumsLabel
			// 
			this.marksNumsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 2, true);
			this.marksNumsLabel.Name = "MarksNumsLabel";
			this.marksNumsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 13, true);
			this.marksNumsLabel.TabIndex = 22;
			this.marksNumsLabel.Text = "Marks and Numbers:";
			// 
			// ReceiptDateEdit
			// 
			this.receiptDateEdit.AutoCompleteMonthThreshold = 1;
			this.receiptDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.receiptDateEdit, "Outturns.C5_CargoReceiptDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CargoReceiptDate)));
			this.receiptDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.receiptDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 5, true);
			this.receiptDateEdit.Name = "ReceiptDateEdit";
			this.receiptDateEdit.TabIndex = 1;
			// 
			// UnpackDateEdit
			// 
			this.unpackDateEdit.AutoCompleteMonthThreshold = 1;
			this.unpackDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.unpackDateEdit, "Outturns.C5_CargoUnpackDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CargoUnpackDate)));
			this.unpackDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.unpackDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 30, true);
			this.unpackDateEdit.Name = "UnpackDateEdit";
			this.unpackDateEdit.TabIndex = 3;
			// 
			// CargoTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.cargoTypeDropEdit, "Outturns.C5_CargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CargoType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.CargoTypes)));
			this.cargoTypeDropEdit.BindToList = "Outturns.Lookups+CargoTypes";
			this.cargoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 55, true);
			this.cargoTypeDropEdit.Name = "CargoTypeDropEdit";
			this.cargoTypeDropEdit.ShowDescriptionBox = false;
			this.cargoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.cargoTypeDropEdit.TabIndex = 5;
			// 
			// ContainerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.containerNumberTextBox, "Outturns.C5_ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_ContainerNumber)));
			this.containerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 5, true);
			this.containerNumberTextBox.Name = "ContainerNumberTextBox";
			this.containerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.containerNumberTextBox.TabIndex = 11;
			// 
			// PackagesCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.packagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_PackagesOutturned)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_PackagesUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.PackageTypes)));
			this.packagesCalcDropEdit.BindToAmount = "Outturns.C5_PackagesOutturned";
			this.packagesCalcDropEdit.BindToList = "Outturns.Lookups+PackageTypes";
			this.packagesCalcDropEdit.BindToUnit = "Outturns.C5_PackagesUnits";
			this.packagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 80, true);
			this.packagesCalcDropEdit.Name = "PackagesCalcDropEdit";
			this.packagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.packagesCalcDropEdit.TabIndex = 17;
			this.packagesCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// OceanBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.oceanBillTextBox, "Outturns.C5_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_MasterBill)));
			this.oceanBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 80, true);
			this.oceanBillTextBox.Name = "OceanBillTextBox";
			this.oceanBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.oceanBillTextBox.TabIndex = 7;
			// 
			// HouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.houseBillTextBox, "Outturns.C5_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_HouseBill)));
			this.houseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 105, true);
			this.houseBillTextBox.Name = "HouseBillTextBox";
			this.houseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.houseBillTextBox.TabIndex = 9;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.goodsDescriptionTextBox, "Outturns.C5_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_GoodsDescription)));
			this.goodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.goodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 20, true);
			this.goodsDescriptionTextBox.Multiline = true;
			this.goodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.goodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 55, true);
			this.goodsDescriptionTextBox.TabIndex = 21;
			// 
			// MarksNumsTextBox
			// 
			this.BindingSource.SetBindingMember(this.marksNumsTextBox, "Outturns.C5_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_MarksAndNumbers)));
			this.marksNumsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.marksNumsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 20, true);
			this.marksNumsTextBox.Multiline = true;
			this.marksNumsTextBox.Name = "MarksNumsTextBox";
			this.marksNumsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 55, true);
			this.marksNumsTextBox.TabIndex = 23;
			// 
			// SealNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.sealNumberTextBox, "Outturns.C5_ContainerSeal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_ContainerSeal)));
			this.sealNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 30, true);
			this.sealNumberTextBox.Name = "SealNumberTextBox";
			this.sealNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.sealNumberTextBox.TabIndex = 13;
			// 
			// OutturnResultDropEdit
			// 
			this.BindingSource.SetBindingMember(this.outturnResultDropEdit, "Outturns.C5_OutturnResultType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_OutturnResultType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.OutturnResultTypeList)));
			this.outturnResultDropEdit.BindToList = "Outturns.Lookups+OutturnResultTypeList";
			this.outturnResultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 105, true);
			this.outturnResultDropEdit.Name = "OutturnResultDropEdit";
			this.outturnResultDropEdit.ShowDescriptionBox = false;
			this.outturnResultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.outturnResultDropEdit.TabIndex = 19;
			// 
			// OuterPacksCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.outerPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_OuterPackUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.PackageTypes)));
			this.outerPacksCalcDropEdit.BindToAmount = "Outturns.C5_OuterPacks";
			this.outerPacksCalcDropEdit.BindToList = "Outturns.Lookups+PackageTypes";
			this.outerPacksCalcDropEdit.BindToUnit = "Outturns.C5_OuterPackUnits";
			this.outerPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 55, true);
			this.outerPacksCalcDropEdit.Name = "OuterPacksCalcDropEdit";
			this.outerPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.outerPacksCalcDropEdit.TabIndex = 15;
			this.outerPacksCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// OuterPacksLabel
			// 
			this.outerPacksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 59, true);
			this.outerPacksLabel.Name = "OuterPacksLabel";
			this.outerPacksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.outerPacksLabel.TabIndex = 14;
			this.outerPacksLabel.Text = "Manifested:";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 84, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 13, true);
			this.zLabel1.TabIndex = 24;
			this.zLabel1.Text = "Commercial Status:";
			// 
			// CustomsStatusLabel
			// 
			this.customsStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 109, true);
			this.customsStatusLabel.Name = "CustomsStatusLabel";
			this.customsStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.customsStatusLabel.TabIndex = 26;
			this.customsStatusLabel.Text = "Customs Status:";
			// 
			// CustomsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.customsStatusTextBox, "Outturns.CustomsStatus+Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).CustomsStatus.Description)));
			this.customsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 105, true);
			this.customsStatusTextBox.Name = "CustomsStatusTextBox";
			this.customsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.customsStatusTextBox.TabIndex = 27;
			// 
			// CommercialStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.commercialStatusDropEdit, "Outturns.C5_CommercialStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CommercialStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.CommercialStatusList)));
			this.commercialStatusDropEdit.BindToList = "Outturns.Lookups+CommercialStatusList";
			this.commercialStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 80, true);
			this.commercialStatusDropEdit.Name = "CommercialStatusDropEdit";
			this.commercialStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.commercialStatusDropEdit.TabIndex = 25;
			// 
			// DetailsButton
			// 
			this.detailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 105, true);
			this.detailsButton.Name = "DetailsButton";
			this.detailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.detailsButton.TabIndex = 28;
			this.detailsButton.Text = "Details";
			this.detailsButton.Click += new System.EventHandler(this.DetailsButton_Click);
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 133, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.zLabel2.TabIndex = 29;
			this.zLabel2.Text = "Underbond Status:";
			// 
			// UnderbondStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.underbondStatusTextBox, "Outturns.MessageStatus+Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).MessageStatus.Description)));
			this.underbondStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 129, true);
			this.underbondStatusTextBox.Name = "UnderbondStatusTextBox";
			this.underbondStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.underbondStatusTextBox.TabIndex = 30;
			// 
			// SeaCargoInfoButton
			// 
			this.seaCargoInfoButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 129, true);
			this.seaCargoInfoButton1.Name = "SeaCargoInfoButton";
			this.seaCargoInfoButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.seaCargoInfoButton1.TabIndex = 34;
			this.seaCargoInfoButton1.Text = "Sea Cargo Info";
			this.seaCargoInfoButton1.Click += new System.EventHandler(this.SeaCargoInfoButton_Click);
			// 
			// SeaCargoOutturnDetailUserControl
			// 
			this.Controls.Add(this.seaCargoInfoButton1);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.underbondStatusTextBox);
			this.Controls.Add(this.detailsButton);
			this.Controls.Add(this.commercialStatusDropEdit);
			this.Controls.Add(this.customsStatusLabel);
			this.Controls.Add(this.customsStatusTextBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.outerPacksCalcDropEdit);
			this.Controls.Add(this.outerPacksLabel);
			this.Controls.Add(this.outturnResultDropEdit);
			this.Controls.Add(this.sealNumberTextBox);
			this.Controls.Add(this.marksNumsTextBox);
			this.Controls.Add(this.goodsDescriptionTextBox);
			this.Controls.Add(this.houseBillTextBox);
			this.Controls.Add(this.oceanBillTextBox);
			this.Controls.Add(this.packagesCalcDropEdit);
			this.Controls.Add(this.containerNumberTextBox);
			this.Controls.Add(this.cargoTypeDropEdit);
			this.Controls.Add(this.unpackDateEdit);
			this.Controls.Add(this.receiptDateEdit);
			this.Controls.Add(this.marksNumsLabel);
			this.Controls.Add(this.goodsDescriptionLabel);
			this.Controls.Add(this.cargoTypeLabel);
			this.Controls.Add(this.packagesLabel);
			this.Controls.Add(this.unpackLabel);
			this.Controls.Add(this.receiptLabel);
			this.Controls.Add(this.pillagedCheckBox);
			this.Controls.Add(this.damagedCheckBox);
			this.Controls.Add(this.sealIntactCheckBox);
			this.Controls.Add(this.oceanBillLabel);
			this.Controls.Add(this.houseBillLabel);
			this.Controls.Add(this.outternResultLabel);
			this.Controls.Add(this.sealNumberLabel);
			this.Controls.Add(this.containerNumberLabel);
			this.Name = "SeaCargoOutturnDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
