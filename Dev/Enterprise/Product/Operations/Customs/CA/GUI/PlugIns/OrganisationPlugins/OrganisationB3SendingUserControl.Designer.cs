namespace Enterprise.Customs.CA.GUI
{
	partial class OrganisationB3SendingUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeferredB3SendingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeferredLowValueB3SendActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeferredNormalB3SendActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.B3AutoSaveGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AutoSaveCONDelayIntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutoSaveCONDelayIntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AutoSaveHVSDelayIntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutoSaveHVSDelayIntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FailSafeWarningGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FailSafeCONDelayIntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FailSafeCONDelayIntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FailSafeHVSDelayIntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FailSafeHVSDelayIntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsImporterDirectPaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsImporterDirectForLVSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsGSTDirectPaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsGSTDirectAutoRatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PreventWarningOnSendingB3CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FreightPercentageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreightPercentagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BondDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BondDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.B3DeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CADDeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CADIsBrokerToPayCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.DeferredB3SendingGroupBox.SuspendLayout();
			this.DeferredLowValueB3SendActionDropEdit.SuspendLayout();
			this.DeferredNormalB3SendActionDropEdit.SuspendLayout();
			this.B3AutoSaveGroupBox.SuspendLayout();
			this.AutoSaveCONDelayIntervalTypeDropEdit.SuspendLayout();
			this.AutoSaveHVSDelayIntervalTypeDropEdit.SuspendLayout();
			this.FailSafeWarningGroupBox.SuspendLayout();
			this.FailSafeCONDelayIntervalTypeDropEdit.SuspendLayout();
			this.FailSafeHVSDelayIntervalTypeDropEdit.SuspendLayout();
			this.FreightPercentageGroupBox.SuspendLayout();
			this.BondDetailsGroupBox.SuspendLayout();
			this.BondDetailsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FreightPercentagesGrid)).BeginInit();
			this.B3DeclarationsGroupBox.SuspendLayout();
			this.CADDeclarationsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			// 
			// DetailsGroupBox
			//
			this.DetailsGroupBox.Controls.Add(this.CADDeclarationsGroupBox);
			this.DetailsGroupBox.Controls.Add(this.B3DeclarationsGroupBox);
			this.DetailsGroupBox.Controls.Add(this.DeferredB3SendingGroupBox);
			this.DetailsGroupBox.Controls.Add(this.B3AutoSaveGroupBox);
			this.DetailsGroupBox.Controls.Add(this.FailSafeWarningGroupBox);
			this.DetailsGroupBox.Controls.Add(this.FreightPercentageGroupBox);
			this.DetailsGroupBox.Controls.Add(this.BondDetailsGroupBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 435, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// PreventWarningOnSendingB3CheckBox
			// 
			this.PreventWarningOnSendingB3CheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PreventWarningOnSendingB3CheckBox, "ZO_PreventWarningOnSendingB3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_PreventWarningOnSendingB3)));
			this.PreventWarningOnSendingB3CheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D9F6264A-0DC9-4E8F-B16A-748C2622F3BD", "Do not warn when sending Accounting Declaration messages if Importer or GST direct is set");
			this.PreventWarningOnSendingB3CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PreventWarningOnSendingB3CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 78, true);
			this.PreventWarningOnSendingB3CheckBox.Name = "PreventWarningOnSendingB3CheckBox";
			this.PreventWarningOnSendingB3CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PreventWarningOnSendingB3CheckBox.TabIndex = 6;
			this.PreventWarningOnSendingB3CheckBox.UseVisualStyleBackColor = true;
			// 
			// IsGSTDirectAutoRatedCheckBox
			// 
			this.IsGSTDirectAutoRatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsGSTDirectAutoRatedCheckBox, "ZO_IsGSTDirectAutoRated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsGSTDirectAutoRated)));
			this.IsGSTDirectAutoRatedCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4b836f46-9ace-455e-ba9c-abf0bfb214fb", "Auto-Rate GST Direct Amounts");
			this.IsGSTDirectAutoRatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsGSTDirectAutoRatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 57, true);
			this.IsGSTDirectAutoRatedCheckBox.Name = "IsGSTDirectAutoRatedCheckBox";
			this.IsGSTDirectAutoRatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsGSTDirectAutoRatedCheckBox.TabIndex = 5;
			this.IsGSTDirectAutoRatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsGSTDirectPaymentCheckBox
			// 
			this.IsGSTDirectPaymentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsGSTDirectPaymentCheckBox, "ZO_IsGSTDirectPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsGSTDirectPayment)));
			this.IsGSTDirectPaymentCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("34f45da7-b868-413b-9762-ed2b08f4fb3c", "GST Direct (Importer pays GST)");
			this.IsGSTDirectPaymentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsGSTDirectPaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 57, true);
			this.IsGSTDirectPaymentCheckBox.Name = "IsGSTDirectPaymentCheckBox";
			this.IsGSTDirectPaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsGSTDirectPaymentCheckBox.TabIndex = 4;
			this.IsGSTDirectPaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsImporterDirectForLVSAutoDutyAmtsCheckBox
			// 
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.AutoSize = true;
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.IsImporterDirectForLVSAutoDutyAmtsCheckBox, "ZO_IsLVSImporterAutoDutyDirectAmts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsLVSImporterAutoDutyDirectAmts)));
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 36, true);
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.Name = "IsImporterDirectForLVSAutoDutyAmtsCheckBox";
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.TabIndex = 3;
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.UseCompatibleTextRendering = true;
			this.IsImporterDirectForLVSAutoDutyAmtsCheckBox.UseVisualStyleBackColor = true;

			// 
			// IsImporterDirectForLVSCheckBox
			// 
			this.IsImporterDirectForLVSCheckBox.AutoSize = true;
			this.IsImporterDirectForLVSCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.IsImporterDirectForLVSCheckBox, "ZO_IsLVSImporterDirectPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsLVSImporterDirectPayment)));
			this.IsImporterDirectForLVSCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cd997eaf-f29d-4f30-8487-092e02a6ed47", "Importer Direct for Low Value Shipments");
			this.IsImporterDirectForLVSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsImporterDirectForLVSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 36, true);
			this.IsImporterDirectForLVSCheckBox.Name = "IsImporterDirectForLVSCheckBox";
			this.IsImporterDirectForLVSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsImporterDirectForLVSCheckBox.TabIndex = 2;
			this.IsImporterDirectForLVSCheckBox.UseCompatibleTextRendering = true;
			this.IsImporterDirectForLVSCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsImporterDirectPaymentCheckBox
			// 
			this.IsImporterDirectPaymentCheckBox.AutoSize = true;
			this.IsImporterDirectPaymentCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.IsImporterDirectPaymentCheckBox, "ZO_IsImporterDirectPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsImporterDirectPayment)));
			this.IsImporterDirectPaymentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsImporterDirectPaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.IsImporterDirectPaymentCheckBox.Name = "IsImporterDirectPaymentCheckBox";
			this.IsImporterDirectPaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsImporterDirectPaymentCheckBox.TabIndex = 0;
			this.IsImporterDirectPaymentCheckBox.UseCompatibleTextRendering = true;
			this.IsImporterDirectPaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsImporterDirectPaymentAutoDutyAmtsCheckBox
			// 
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.AutoSize = true;
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.IsImporterDirectPaymentAutoDutyAmtsCheckBox, "ZO_IsHighImporterAutoDutyDirectAmts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsHighImporterAutoDutyDirectAmts)));
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 15, true);
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.Name = "IsImporterDirectPaymentAutoDutyAmtsCheckBox";
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.TabIndex = 1;
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.UseCompatibleTextRendering = true;
			this.IsImporterDirectPaymentAutoDutyAmtsCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeferredB3SendingGroupBox
			// 
			this.DeferredB3SendingGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("afa8d582-5032-43c7-85d7-64a33d078474", "Deferred Accounting Declaration Sending Overrides");
			this.DeferredB3SendingGroupBox.Controls.Add(this.DeferredLowValueB3SendActionDropEdit);
			this.DeferredB3SendingGroupBox.Controls.Add(this.DeferredNormalB3SendActionDropEdit);
			this.DeferredB3SendingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 207, true);
			this.DeferredB3SendingGroupBox.Name = "DeferredB3SendingGroupBox";
			this.DeferredB3SendingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 74, true);
			this.DeferredB3SendingGroupBox.TabIndex = 4;
			this.DeferredB3SendingGroupBox.TabStop = false;
			// 
			// DeferredLowValueB3SendActionDropEdit
			// 
			this.DeferredLowValueB3SendActionDropEdit.AllowDrop = true;
			this.DeferredLowValueB3SendActionDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeferredLowValueB3SendActionDropEdit, "ZO_DeferredLowValueB3SendAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_DeferredLowValueB3SendAction)));
			this.DeferredLowValueB3SendActionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a85de759-8db8-4ad4-a716-40f99ddae60c", "Low Value Ship.", "Low Value Shipment");
			this.DeferredLowValueB3SendActionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 44, true);
			this.DeferredLowValueB3SendActionDropEdit.Name = "DeferredLowValueB3SendActionDropEdit";
			this.DeferredLowValueB3SendActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DeferredLowValueB3SendActionDropEdit.TabIndex = 1;
			// 
			// DeferredNormalB3SendActionDropEdit
			// 
			this.DeferredNormalB3SendActionDropEdit.AllowDrop = true;
			this.DeferredNormalB3SendActionDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeferredNormalB3SendActionDropEdit, "ZO_DeferredNormalB3SendAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_DeferredNormalB3SendAction)));
			this.DeferredNormalB3SendActionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3ddc36a7-8649-451c-aabb-669122b7acae", "Normal Ship.", "Normal Shipment");
			this.DeferredNormalB3SendActionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 18, true);
			this.DeferredNormalB3SendActionDropEdit.Name = "DeferredNormalB3SendActionDropEdit";
			this.DeferredNormalB3SendActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DeferredNormalB3SendActionDropEdit.TabIndex = 0;
			// 
			// B3AutoSaveGroupBox
			// 
			this.B3AutoSaveGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c6c69d89-2215-419c-902b-5bc90bb701c6", "Auto Accounting Declaration Sending Overrides");
			this.B3AutoSaveGroupBox.Controls.Add(this.AutoSaveCONDelayIntervalTypeDropEdit);
			this.B3AutoSaveGroupBox.Controls.Add(this.AutoSaveCONDelayIntervalCalcEdit);
			this.B3AutoSaveGroupBox.Controls.Add(this.AutoSaveHVSDelayIntervalTypeDropEdit);
			this.B3AutoSaveGroupBox.Controls.Add(this.AutoSaveHVSDelayIntervalCalcEdit);
			this.B3AutoSaveGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 128, true);
			this.B3AutoSaveGroupBox.Name = "B3AutoSaveGroupBox";
			this.B3AutoSaveGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 74, true);
			this.B3AutoSaveGroupBox.TabIndex = 2;
			this.B3AutoSaveGroupBox.TabStop = false;
			// 
			// AutoSaveCONDelayIntervalTypeDropEdit
			// 
			this.AutoSaveCONDelayIntervalTypeDropEdit.AllowDrop = true;
			this.AutoSaveCONDelayIntervalTypeDropEdit.AutoSize = false;
			this.BindingSource.SetBindingMember(this.AutoSaveCONDelayIntervalTypeDropEdit, "ZO_CONDelayIntervalTypeAutoSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_CONDelayIntervalTypeAutoSend)));
			this.AutoSaveCONDelayIntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 44, true);
			this.AutoSaveCONDelayIntervalTypeDropEdit.Name = "AutoSaveCONDelayIntervalTypeDropEdit";
			this.AutoSaveCONDelayIntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.AutoSaveCONDelayIntervalTypeDropEdit.TabIndex = 3;
			// 
			// AutoSaveCONDelayIntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AutoSaveCONDelayIntervalCalcEdit, "ZO_CONDelayIntervalAutoSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_CONDelayIntervalAutoSend)));
			this.AutoSaveCONDelayIntervalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5ff216ed-81a7-49bf-85de-25ec287e8640", "CourierLVS-F", "Type F Consolidation");
			this.AutoSaveCONDelayIntervalCalcEdit.DecimalPlaces = 0;
			this.AutoSaveCONDelayIntervalCalcEdit.Decimals = 0;
			this.AutoSaveCONDelayIntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 44, true);
			this.AutoSaveCONDelayIntervalCalcEdit.Name = "AutoSaveCONDelayIntervalCalcEdit";
			this.AutoSaveCONDelayIntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.AutoSaveCONDelayIntervalCalcEdit.TabIndex = 2;
			this.AutoSaveCONDelayIntervalCalcEdit.Text = "0";
			this.AutoSaveCONDelayIntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AutoSaveHVSDelayIntervalTypeDropEdit
			// 
			this.AutoSaveHVSDelayIntervalTypeDropEdit.AllowDrop = true;
			this.AutoSaveHVSDelayIntervalTypeDropEdit.AutoSize = false;
			this.BindingSource.SetBindingMember(this.AutoSaveHVSDelayIntervalTypeDropEdit, "ZO_HVSDelayIntervalTypeAutoSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_HVSDelayIntervalTypeAutoSend)));
			this.AutoSaveHVSDelayIntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 18, true);
			this.AutoSaveHVSDelayIntervalTypeDropEdit.Name = "AutoSaveHVSDelayIntervalTypeDropEdit";
			this.AutoSaveHVSDelayIntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.AutoSaveHVSDelayIntervalTypeDropEdit.TabIndex = 1;
			// 
			// AutoSaveHVSDelayIntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AutoSaveHVSDelayIntervalCalcEdit, "ZO_HVSDelayIntervalAutoSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_HVSDelayIntervalAutoSend)));
			this.AutoSaveHVSDelayIntervalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("323cffc0-75a4-4f35-a5a3-1280b475177d", "HVS", "High Value Release Declaration");
			this.AutoSaveHVSDelayIntervalCalcEdit.DecimalPlaces = 0;
			this.AutoSaveHVSDelayIntervalCalcEdit.Decimals = 0;
			this.AutoSaveHVSDelayIntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 18, true);
			this.AutoSaveHVSDelayIntervalCalcEdit.Name = "AutoSaveHVSDelayIntervalCalcEdit";
			this.AutoSaveHVSDelayIntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.AutoSaveHVSDelayIntervalCalcEdit.TabIndex = 0;
			this.AutoSaveHVSDelayIntervalCalcEdit.Text = "0";
			this.AutoSaveHVSDelayIntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FailSafeWarningGroupBox
			// 
			this.FailSafeWarningGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d339ed74-344f-4a0f-a68b-4d5ac8a0064d", "Fail-Safe Warning Overrides");
			this.FailSafeWarningGroupBox.Controls.Add(this.FailSafeCONDelayIntervalTypeDropEdit);
			this.FailSafeWarningGroupBox.Controls.Add(this.FailSafeCONDelayIntervalCalcEdit);
			this.FailSafeWarningGroupBox.Controls.Add(this.FailSafeHVSDelayIntervalTypeDropEdit);
			this.FailSafeWarningGroupBox.Controls.Add(this.FailSafeHVSDelayIntervalCalcEdit);
			this.FailSafeWarningGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 128, true);
			this.FailSafeWarningGroupBox.Name = "FailSafeWarningGroupBox";
			this.FailSafeWarningGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 74, true);
			this.FailSafeWarningGroupBox.TabIndex = 3;
			this.FailSafeWarningGroupBox.TabStop = false;
			// 
			// FailSafeCONDelayIntervalTypeDropEdit
			// 
			this.FailSafeCONDelayIntervalTypeDropEdit.AllowDrop = true;
			this.FailSafeCONDelayIntervalTypeDropEdit.AutoSize = false;
			this.BindingSource.SetBindingMember(this.FailSafeCONDelayIntervalTypeDropEdit, "ZO_CONDelayIntervalTypeFailSafe");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_CONDelayIntervalTypeFailSafe)));
			this.FailSafeCONDelayIntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 44, true);
			this.FailSafeCONDelayIntervalTypeDropEdit.Name = "FailSafeCONDelayIntervalTypeDropEdit";
			this.FailSafeCONDelayIntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.FailSafeCONDelayIntervalTypeDropEdit.TabIndex = 3;
			// 
			// FailSafeCONDelayIntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FailSafeCONDelayIntervalCalcEdit, "ZO_CONDelayIntervalFailSafe");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_CONDelayIntervalFailSafe)));
			this.FailSafeCONDelayIntervalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("368a9baa-1584-4961-b78a-b48679e873ee", "CourierLVS-F", "Type F Consolidation");
			this.FailSafeCONDelayIntervalCalcEdit.DecimalPlaces = 0;
			this.FailSafeCONDelayIntervalCalcEdit.Decimals = 0;
			this.FailSafeCONDelayIntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 44, true);
			this.FailSafeCONDelayIntervalCalcEdit.Name = "FailSafeCONDelayIntervalCalcEdit";
			this.FailSafeCONDelayIntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.FailSafeCONDelayIntervalCalcEdit.TabIndex = 2;
			this.FailSafeCONDelayIntervalCalcEdit.Text = "0";
			this.FailSafeCONDelayIntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FailSafeHVSDelayIntervalTypeDropEdit
			// 
			this.FailSafeHVSDelayIntervalTypeDropEdit.AllowDrop = true;
			this.FailSafeHVSDelayIntervalTypeDropEdit.AutoSize = false;
			this.BindingSource.SetBindingMember(this.FailSafeHVSDelayIntervalTypeDropEdit, "ZO_HVSDelayIntervalTypeFailSafe");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_HVSDelayIntervalTypeFailSafe)));
			this.FailSafeHVSDelayIntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 18, true);
			this.FailSafeHVSDelayIntervalTypeDropEdit.Name = "FailSafeHVSDelayIntervalTypeDropEdit";
			this.FailSafeHVSDelayIntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.FailSafeHVSDelayIntervalTypeDropEdit.TabIndex = 1;
			// 
			// FailSafeHVSDelayIntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FailSafeHVSDelayIntervalCalcEdit, "ZO_HVSDelayIntervalFailSafe");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_HVSDelayIntervalFailSafe)));
			this.FailSafeHVSDelayIntervalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("df3c7cc9-fc99-44bb-af89-3f5cafb0c388", "HVS", "High Value Release Declaration");
			this.FailSafeHVSDelayIntervalCalcEdit.DecimalPlaces = 0;
			this.FailSafeHVSDelayIntervalCalcEdit.Decimals = 0;
			this.FailSafeHVSDelayIntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 18, true);
			this.FailSafeHVSDelayIntervalCalcEdit.Name = "FailSafeHVSDelayIntervalCalcEdit";
			this.FailSafeHVSDelayIntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.FailSafeHVSDelayIntervalCalcEdit.TabIndex = 0;
			this.FailSafeHVSDelayIntervalCalcEdit.Text = "0";
			this.FailSafeHVSDelayIntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FreightPercentageGroupBox
			// 
			this.FreightPercentageGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a9e92030-350d-4ffa-b4f7-91a817ec7e5f", "Freight Percentage Overrides");
			this.FreightPercentageGroupBox.Controls.Add(this.FreightPercentagesGrid);
			this.FreightPercentageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 207, true);
			this.FreightPercentageGroupBox.Name = "FreightPercentageGroupBox";
			this.FreightPercentageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 150, true);
			this.FreightPercentageGroupBox.TabIndex = 5;
			this.FreightPercentageGroupBox.TabStop = false;
			// 
			// FreightPercentagesGrid
			// 
			this.FreightPercentagesGrid.AllowNavigation = false;
			this.FreightPercentagesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FreightPercentagesGrid, "FreightPercentages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.FreightPercentage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.FreightPercentage)(null)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.FreightPercentage)(null)).Lookups.TransportTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.FreightPercentage)(null)).DefaultFreightPercentage)));
			this.FreightPercentagesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.TransportTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2fb6102e-5919-425c-8a52-652aa7904371", "Mode of Transport");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9f7f478a-7f51-470f-8393-8dc39864669e", "Freight Percentage");
			zCalcEditColumnStyleInfo1.ColumnName = "DefaultFreightPercentage";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.FreightPercentagesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FreightPercentagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FreightPercentagesGrid.GridId = "f0d0fba5-9c3c-4eac-a38d-66983a958d61";
			this.FreightPercentagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FreightPercentagesGrid.LayoutKey = "FreightPercentagesGrid";
			this.FreightPercentagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FreightPercentagesGrid.Name = "FreightPercentagesGrid";
			this.FreightPercentagesGrid.TabIndex = 0;
			// 
			// BondDetailsGroupBox
			// 
			this.BondDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("14D316D1-C515-4BED-8B7E-ABB6492D5429", "Bond Details");
			this.BondDetailsGroupBox.Controls.Add(this.BondDetailsGrid);
			this.BondDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 360, true);
			this.BondDetailsGroupBox.Name = "BondDetailsGroupBox";
			this.BondDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 152, true);
			this.BondDetailsGroupBox.TabIndex = 6;
			this.BondDetailsGroupBox.TabStop = false;
			// 
			// BondDetailsGrid
			//
			this.BondDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BondDetailsGrid, "BondDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_ActivityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_BondType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).Lookups.BondTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_BondNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_SuretyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_BondAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_BondEffectiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).BondDetails)).SyncRoot)).PW_BondExpiryDate)));
			this.BondDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("62419D13-D45F-46D9-A193-8AF465DA9BDD", "Bond Number");
			zTextBoxColumnStyleInfo1.ColumnName = "PW_BondNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+BondTypeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6B9C952C-EA81-4B51-9586-7DB0593EF639", "Bond Type");
			zDropEditColumnStyleInfo2.ColumnName = "PW_BondType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("11E23D20-CB02-4309-B523-19F1E4745859", "Surety Code");
			zTextBoxColumnStyleInfo2.ColumnName = "PW_SuretyCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DC7DC5FD-E6AB-426F-954C-E0DFFD32094E", "Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "PW_BondAmount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8595BA57-3A3A-45BA-8D56-1F453579B3A4", "Bond Effective Date");
			zDateEditColumnStyleInfo1.ColumnName = "PW_BondEffectiveDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BAAF1CAD-D23E-4B43-93F0-85DA8D20D4D5", "Bond Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "PW_BondExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.BondDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BondDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BondDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BondDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BondDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BondDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BondDetailsGrid.CopySelectedRowsAllowed = true;
			this.BondDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BondDetailsGrid.GridId = "9AFE36B9-358C-4EA6-83C3-77B337A20857";
			this.BondDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BondDetailsGrid.LayoutKey = "BondDetailsGrid";
			this.BondDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BondDetailsGrid.Name = "BondDetailsGrid";
			this.BondDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 94, true);
			this.BondDetailsGrid.TabIndex = 0;
			// 
			// B3DeclarationsGroupBox
			// 
			this.B3DeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("05c33646-d9ba-4946-963a-8b0a440f6352", "Entry Declarations");
			this.B3DeclarationsGroupBox.Controls.Add(this.IsImporterDirectPaymentCheckBox);
			this.B3DeclarationsGroupBox.Controls.Add(this.IsImporterDirectPaymentAutoDutyAmtsCheckBox);
			this.B3DeclarationsGroupBox.Controls.Add(this.IsImporterDirectForLVSCheckBox);
			this.B3DeclarationsGroupBox.Controls.Add(this.IsImporterDirectForLVSAutoDutyAmtsCheckBox);
			this.B3DeclarationsGroupBox.Controls.Add(this.IsGSTDirectPaymentCheckBox);
			this.B3DeclarationsGroupBox.Controls.Add(this.IsGSTDirectAutoRatedCheckBox);
			this.B3DeclarationsGroupBox.Controls.Add(this.PreventWarningOnSendingB3CheckBox);
			this.B3DeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.B3DeclarationsGroupBox.Name = "B3DeclarationsGroupBox";
			this.B3DeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 109, true);
			this.B3DeclarationsGroupBox.TabIndex = 0;
			this.B3DeclarationsGroupBox.TabStop = false;
			// 
			// CADDeclarationsGroupBox
			// 
			this.CADDeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("44f52add-7c67-4a49-bd03-13bbee04dbec", "CAD Declarations");
			this.CADDeclarationsGroupBox.Controls.Add(this.CADIsBrokerToPayCheckBox);
			this.CADDeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 14, true);
			this.CADDeclarationsGroupBox.Name = "CADDeclarationsGroupBox";
			this.CADDeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 109, true);
			this.CADDeclarationsGroupBox.TabIndex = 1;
			this.CADDeclarationsGroupBox.TabStop = false;
			// 
			// CADIsBrokerToPayCheckBox
			// 
			this.CADIsBrokerToPayCheckBox.AutoSize = true;
			this.CADIsBrokerToPayCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.CADIsBrokerToPayCheckBox, "ZO_CADIsBrokerToPay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_CADIsBrokerToPay)));
			this.CADIsBrokerToPayCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6c72b7f0-47b5-480f-b010-326fa19c9d53", "Broker to Pay");
			this.CADIsBrokerToPayCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 17, true);
			this.CADIsBrokerToPayCheckBox.Name = "CADIsBrokerToPayCheckBox";
			this.CADIsBrokerToPayCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 17, true);
			this.CADIsBrokerToPayCheckBox.TabIndex = 0;
			this.CADIsBrokerToPayCheckBox.UseCompatibleTextRendering = true;
			this.CADIsBrokerToPayCheckBox.UseVisualStyleBackColor = false;
			// 
			// OrganisationB3SendingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "OrganisationB3SendingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 435, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DeferredB3SendingGroupBox.ResumeLayout(false);
			this.DeferredB3SendingGroupBox.PerformLayout();
			this.DeferredLowValueB3SendActionDropEdit.ResumeLayout(true);
			this.DeferredLowValueB3SendActionDropEdit.PerformLayout();
			this.DeferredNormalB3SendActionDropEdit.ResumeLayout(true);
			this.DeferredNormalB3SendActionDropEdit.PerformLayout();
			this.B3AutoSaveGroupBox.ResumeLayout(false);
			this.B3AutoSaveGroupBox.PerformLayout();
			this.AutoSaveCONDelayIntervalTypeDropEdit.ResumeLayout(true);
			this.AutoSaveCONDelayIntervalTypeDropEdit.PerformLayout();
			this.AutoSaveHVSDelayIntervalTypeDropEdit.ResumeLayout(true);
			this.AutoSaveHVSDelayIntervalTypeDropEdit.PerformLayout();
			this.FailSafeWarningGroupBox.ResumeLayout(false);
			this.FailSafeWarningGroupBox.PerformLayout();
			this.FailSafeCONDelayIntervalTypeDropEdit.ResumeLayout(true);
			this.FailSafeCONDelayIntervalTypeDropEdit.PerformLayout();
			this.FailSafeHVSDelayIntervalTypeDropEdit.ResumeLayout(true);
			this.FailSafeHVSDelayIntervalTypeDropEdit.PerformLayout();
			this.FreightPercentageGroupBox.ResumeLayout(false);
			this.FreightPercentageGroupBox.PerformLayout();
			this.BondDetailsGroupBox.ResumeLayout(false);
			this.BondDetailsGroupBox.PerformLayout();
			this.BondDetailsGrid.ResumeLayout(false);
			this.BondDetailsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FreightPercentagesGrid)).EndInit();
			this.B3DeclarationsGroupBox.ResumeLayout(false);
			this.B3DeclarationsGroupBox.PerformLayout();
			this.CADDeclarationsGroupBox.ResumeLayout(false);
			this.CADDeclarationsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox B3AutoSaveGroupBox;
		private ZArchitecture.GUI.ZGroupBox FailSafeWarningGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsImporterDirectPaymentCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsImporterDirectPaymentAutoDutyAmtsCheckBox;
		private ZArchitecture.GUI.ZDropEdit AutoSaveCONDelayIntervalTypeDropEdit;
		internal ZArchitecture.ZCalcEdit AutoSaveCONDelayIntervalCalcEdit;
		private ZArchitecture.GUI.ZDropEdit AutoSaveHVSDelayIntervalTypeDropEdit;
		internal ZArchitecture.ZCalcEdit AutoSaveHVSDelayIntervalCalcEdit;
		private ZArchitecture.GUI.ZDropEdit FailSafeHVSDelayIntervalTypeDropEdit;
		internal ZArchitecture.ZCalcEdit FailSafeHVSDelayIntervalCalcEdit;
		private ZArchitecture.GUI.ZDropEdit FailSafeCONDelayIntervalTypeDropEdit;
		internal ZArchitecture.ZCalcEdit FailSafeCONDelayIntervalCalcEdit;
		internal ZArchitecture.GUI.ZGroupBox DeferredB3SendingGroupBox;
		internal ZArchitecture.GUI.ZDropEdit DeferredNormalB3SendActionDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DeferredLowValueB3SendActionDropEdit;
		private ZArchitecture.GUI.ZCheckBox IsImporterDirectForLVSCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsImporterDirectForLVSAutoDutyAmtsCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsGSTDirectPaymentCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsGSTDirectAutoRatedCheckBox;
		private ZArchitecture.GUI.ZCheckBox PreventWarningOnSendingB3CheckBox;
		private ZArchitecture.GUI.ZGroupBox FreightPercentageGroupBox;
		private ZArchitecture.ZGrid FreightPercentagesGrid;
		private ZArchitecture.GUI.ZGroupBox BondDetailsGroupBox;
		private ZArchitecture.ZGrid BondDetailsGrid;
		private ZArchitecture.GUI.ZGroupBox B3DeclarationsGroupBox;
		private ZArchitecture.GUI.ZGroupBox CADDeclarationsGroupBox;
		private ZArchitecture.GUI.ZCheckBox CADIsBrokerToPayCheckBox;
	}
}
