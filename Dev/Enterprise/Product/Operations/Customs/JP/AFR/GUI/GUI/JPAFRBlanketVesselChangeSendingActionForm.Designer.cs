namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRBlanketVesselChangeSendingActionForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VesselInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BlanketChangeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NewOperatorVoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewCarrierCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewVoyageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewPortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NewPortOfLoadingSuffixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewIsDepartureFromRelaxedAredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NewETDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.HasCheckedAllBillsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JPH_VesselNameFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_RN_NKCountryOfRegFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JPH_RadioCallSignTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			this.BillsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.MainControlPanel.SuspendLayout();
			this.VesselInfoGroupBox.SuspendLayout();
			this.NewPortOfLoadingCodeFindBox.SuspendLayout();
			this.NewETDDateEdit.SuspendLayout();
			this.JPH_VesselNameFindBox.SuspendLayout();
			this.JPH_RN_NKCountryOfRegFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.BlanketVesselChange);
			// 
			// BillsGroupBox
			// 
			this.BillsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BillsGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("43b7029c-00dc-4e3a-b10c-dd5510d53339", "Bills");
			this.BillsGroupBox.Controls.Add(this.BillsGrid);
			this.BillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 271, true);
			this.BillsGroupBox.Name = "BillsGroupBox";
			this.BillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 173, true);
			this.BillsGroupBox.TabIndex = 12;
			this.BillsGroupBox.TabStop = false;
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, "BlanketVesselChangeBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChangeBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)).SyncRoot)).JPM_BillOfLadingNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChangeBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)).SyncRoot)).JPM_Send)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChangeBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)).SyncRoot)).JPM_ReleaseStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChangeBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)).SyncRoot)).JPM_MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChangeBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)).SyncRoot)).JPM_ReleaseStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChangeBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).BlanketVesselChangeBills)).SyncRoot)).JPM_MessageStatus)));
			this.BillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JPM_BillOfLadingNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCheckBoxColumnStyleInfo1.ColumnName = "JPM_Send";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JPM_ReleaseStatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "JPM_MessageStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "JPM_ReleaseStatus";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "JPM_MessageStatus";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "7f9ffd73-2f0b-43ac-a6a3-b5790d2b08bb";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.LayoutKey = "BillsGrid";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 154, true);
			this.BillsGrid.TabIndex = 15;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.cancelButton);
			this.BottomPanel.Controls.Add(this.SendButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 37, true);
			this.BottomPanel.TabIndex = 6;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 6, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 16;
			this.cancelButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c601e4ec-538d-48b4-a40d-ef3a950fec4b", "&Cancel");
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 6, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.SendButton.TabIndex = 15;
			this.SendButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4fd9aa20-3410-4299-ad69-e64f271fc7fa", "&Send");
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MainControlPanel
			// 
			this.MainControlPanel.Controls.Add(this.VesselInfoGroupBox);
			this.MainControlPanel.Controls.Add(this.HasCheckedAllBillsCheckBox);
			this.MainControlPanel.Controls.Add(this.BillsGroupBox);
			this.MainControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainControlPanel.Name = "MainControlPanel";
			this.MainControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 450, true);
			this.MainControlPanel.TabIndex = 1;
			// 
			// VesselInfoGroupBox
			// 
			this.VesselInfoGroupBox.Controls.Add(this.JPH_VesselNameFindBox);
			this.VesselInfoGroupBox.Controls.Add(this.JPH_RN_NKCountryOfRegFindBox);
			this.VesselInfoGroupBox.Controls.Add(this.JPH_RadioCallSignTextBox);
			this.VesselInfoGroupBox.Controls.Add(this.BlanketChangeCheckBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewOperatorVoyageTextBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewCarrierCodeTextBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewVoyageNumberTextBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewPortOfLoadingCodeFindBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewPortOfLoadingSuffixTextBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewIsDepartureFromRelaxedAredCheckBox);
			this.VesselInfoGroupBox.Controls.Add(this.NewETDDateEdit);
			this.VesselInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.VesselInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselInfoGroupBox.Name = "VesselInfoGroupBox";
			this.VesselInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 235, true);
			this.VesselInfoGroupBox.TabIndex = 5;
			this.VesselInfoGroupBox.TabStop = false;
			this.VesselInfoGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("b6c1b542-5fe5-46f3-a90e-3590164c08f5", "New Voyage Details");
			// 
			// BlanketChangeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BlanketChangeCheckBox, "JPM_BlanketChange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_BlanketChange)));
			this.BlanketChangeCheckBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c31ef5f5-b3af-47fa-8c56-8c7ddb11c122", "Is Blanket Change");
			this.BlanketChangeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BlanketChangeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BlanketChangeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 209, true);
			this.BlanketChangeCheckBox.Name = "BlanketChangeCheckBox";
			this.BlanketChangeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 24, true);
			this.BlanketChangeCheckBox.TabIndex = 10;
			this.BlanketChangeCheckBox.UseVisualStyleBackColor = true;
			this.BlanketChangeCheckBox.CheckedChanged += new System.EventHandler(this.BlanketChangeCheckBox_CheckedChanged);
			// 
			// NewOperatorVoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewOperatorVoyageTextBox, "JPM_OperatorVoyageNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_OperatorVoyageNew)));
			this.NewOperatorVoyageTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("507976f1-99f1-42f6-b050-2b22770e98d9", "Operator Voyage");
			this.NewOperatorVoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 131, true);
			this.NewOperatorVoyageTextBox.Name = "NewOperatorVoyageTextBox";
			this.NewOperatorVoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NewOperatorVoyageTextBox.TabIndex = 5;
			// 
			// NewCarrierCodeTextBox
			// 
			this.NewCarrierCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewCarrierCodeTextBox, "JPM_CarrierCodeNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_CarrierCodeNew)));
			this.NewCarrierCodeTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("1a791143-d962-4c30-ab90-c5b5103a8f83", "Carrier Code");
			this.NewCarrierCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 25, true);
			this.NewCarrierCodeTextBox.Name = "NewCarrierCodeTextBox";
			this.NewCarrierCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NewCarrierCodeTextBox.TabIndex = 1;
			// 
			// NewVoyageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewVoyageNumberTextBox, "JPM_VoyageNumberNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_VoyageNumberNew)));
			this.NewVoyageNumberTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4a5781ac-6d6d-4d54-850a-89a77465b176", "Voyage Number");
			this.NewVoyageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 105, true);
			this.NewVoyageNumberTextBox.Name = "NewVoyageNumberTextBox";
			this.NewVoyageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NewVoyageNumberTextBox.TabIndex = 4;
			// 
			// NewPortOfLoadingCodeFindBox
			// 
			this.NewPortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewPortOfLoadingCodeFindBox, "JPM_PortOfLoadingCodeNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_PortOfLoadingCodeNew)));
			this.NewPortOfLoadingCodeFindBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c116b4e6-b88b-4d3b-847f-66fd8c54f506", "Port Of Loading");
			this.NewPortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 157, true);
			this.NewPortOfLoadingCodeFindBox.Name = "NewPortOfLoadingCodeFindBox";
			this.NewPortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.NewPortOfLoadingCodeFindBox.ShouldResize = true;
			this.NewPortOfLoadingCodeFindBox.ShowDescriptionBox = false;
			this.NewPortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.NewPortOfLoadingCodeFindBox.TabIndex = 6;
			// 
			// NewPortOfLoadingSuffixTextBox
			// 
			this.NewPortOfLoadingSuffixTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewPortOfLoadingSuffixTextBox, "JPM_PortOfLoadingSuffixNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_PortOfLoadingSuffixNew)));
			this.NewPortOfLoadingSuffixTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("2efdb429-6f3a-4080-a58b-20926199765f", "Suffix", "Port Of Loading Suffix", "");
			this.NewPortOfLoadingSuffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 154, true);
			this.NewPortOfLoadingSuffixTextBox.Name = "NewPortOfLoadingSuffixTextBox";
			this.NewPortOfLoadingSuffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.NewPortOfLoadingSuffixTextBox.TabIndex = 7;
			// 
			// NewIsDepartureFromRelaxedAredCheckBox
			// 
			this.NewIsDepartureFromRelaxedAredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NewIsDepartureFromRelaxedAredCheckBox, "JPM_IsDepartureFromRelaxedAreaNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_IsDepartureFromRelaxedAreaNew)));
			this.NewIsDepartureFromRelaxedAredCheckBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("34b02e83-685f-4555-9ddd-55701a307985", "Relaxed Area?");
			this.NewIsDepartureFromRelaxedAredCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NewIsDepartureFromRelaxedAredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NewIsDepartureFromRelaxedAredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 157, true);
			this.NewIsDepartureFromRelaxedAredCheckBox.Name = "NewIsDepartureFromRelaxedAredCheckBox";
			this.NewIsDepartureFromRelaxedAredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NewIsDepartureFromRelaxedAredCheckBox.TabIndex = 8;
			this.NewIsDepartureFromRelaxedAredCheckBox.UseVisualStyleBackColor = true;
			// 
			// NewETDDateEdit
			// 
			this.NewETDDateEdit.AllowDrop = true;
			this.NewETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.NewETDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NewETDDateEdit, "JPM_ETDNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_ETDNew)));
			this.NewETDDateEdit.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("410c2142-c552-4fc9-870d-1f378cf7721d", "ETD");
			this.NewETDDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.NewETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 183, true);
			this.NewETDDateEdit.Name = "NewETDDateEdit";
			this.NewETDDateEdit.TabIndex = 9;
			// 
			// HasCheckedAllBillsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HasCheckedAllBillsCheckBox, "JPM_AreAllBillsChecked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_AreAllBillsChecked)));
			this.HasCheckedAllBillsCheckBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("e0f2259a-77f9-4ea1-8c50-848594a97843", "Check/Uncheck All Bills");
			this.HasCheckedAllBillsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasCheckedAllBillsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 241, true);
			this.HasCheckedAllBillsCheckBox.Name = "HasCheckedAllBillsCheckBox";
			this.HasCheckedAllBillsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 24, true);
			this.HasCheckedAllBillsCheckBox.TabIndex = 11;
			this.HasCheckedAllBillsCheckBox.UseVisualStyleBackColor = true;
			// 
			// JPH_VesselNameFindBox
			// 
			this.JPH_VesselNameFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_VesselNameFindBox, "JPM_VesselNameNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_VesselNameNew)));
			this.JPH_VesselNameFindBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("d64809a3-26e2-4030-8186-e35a9bdae7e2", "Vessel Name");
			this.JPH_VesselNameFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 51, true);
			this.JPH_VesselNameFindBox.Name = "JPH_VesselNameFindBox";
			this.JPH_VesselNameFindBox.PreBoundMaxLength = 23;
			this.JPH_VesselNameFindBox.ShouldResize = true;
			this.JPH_VesselNameFindBox.ShowDescriptionBox = false;
			this.JPH_VesselNameFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.JPH_VesselNameFindBox.TabIndex = 11;
			// 
			// JPH_RN_NKCountryOfRegFindBox
			// 
			this.JPH_RN_NKCountryOfRegFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JPH_RN_NKCountryOfRegFindBox, "JPM_RN_NKCountryOfRegNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_RN_NKCountryOfRegNew)));
			this.JPH_RN_NKCountryOfRegFindBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("bd0bb662-3a05-4bb9-bdf2-df0fc3caf075", "Nationality");
			this.JPH_RN_NKCountryOfRegFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 77, true);
			this.JPH_RN_NKCountryOfRegFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.JPH_RN_NKCountryOfRegFindBox.Name = "JPH_RN_NKCountryOfRegFindBox";
			this.JPH_RN_NKCountryOfRegFindBox.PreBoundMaxLength = 3;
			this.JPH_RN_NKCountryOfRegFindBox.ShouldResize = true;
			this.JPH_RN_NKCountryOfRegFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.JPH_RN_NKCountryOfRegFindBox.TabIndex = 13;
			// 
			// JPH_RadioCallSignTextBox
			// 
			this.BindingSource.SetBindingMember(this.JPH_RadioCallSignTextBox, "JPM_RadioCallSignNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BlanketVesselChange)(null)).JPM_RadioCallSignNew)));
			this.JPH_RadioCallSignTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("6dd5851f-2b75-4f73-95b7-f8ebe27763b4", "Call Sign");
			this.JPH_RadioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 77, true);
			this.JPH_RadioCallSignTextBox.Name = "JPH_RadioCallSignTextBox";
			this.JPH_RadioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JPH_RadioCallSignTextBox.TabIndex = 12;
			// 
			// JPAFRBlanketVesselChangeSendingActionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 511, true);
			this.Controls.Add(this.MainControlPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.BlanketVesselChange);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 550, true);
			this.Name = "JPAFRBlanketVesselChangeSendingActionForm";
			this.Text = "JPAFRMessageSendingActionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillsGroupBox.ResumeLayout(false);
			this.BillsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.BillsGrid.ResumeLayout(false);
			this.BillsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainControlPanel.ResumeLayout(false);
			this.MainControlPanel.PerformLayout();
			this.VesselInfoGroupBox.ResumeLayout(false);
			this.VesselInfoGroupBox.PerformLayout();
			this.NewPortOfLoadingCodeFindBox.ResumeLayout(true);
			this.NewPortOfLoadingCodeFindBox.PerformLayout();
			this.NewETDDateEdit.ResumeLayout(true);
			this.NewETDDateEdit.PerformLayout();
			this.JPH_VesselNameFindBox.ResumeLayout(true);
			this.JPH_VesselNameFindBox.PerformLayout();
			this.JPH_RN_NKCountryOfRegFindBox.ResumeLayout(true);
			this.JPH_RN_NKCountryOfRegFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid BillsGrid;
		private ZArchitecture.GUI.ZGroupBox BillsGroupBox;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZPanel MainControlPanel;
		private ZArchitecture.GUI.ZCheckBox HasCheckedAllBillsCheckBox;
		private ZArchitecture.GUI.ZGroupBox VesselInfoGroupBox;
		private ZArchitecture.ZTextBox NewOperatorVoyageTextBox;
		private ZArchitecture.ZTextBox NewCarrierCodeTextBox;
		private ZArchitecture.ZTextBox NewVoyageNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox NewPortOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox NewPortOfLoadingSuffixTextBox;
		private ZArchitecture.GUI.ZCheckBox NewIsDepartureFromRelaxedAredCheckBox;
		private ZArchitecture.GUI.ZDateEdit NewETDDateEdit;
		private ZArchitecture.GUI.ZCheckBox BlanketChangeCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_VesselNameFindBox;
		private ZArchitecture.GUI.ZCodeFindBox JPH_RN_NKCountryOfRegFindBox;
		private ZArchitecture.ZTextBox JPH_RadioCallSignTextBox;
	}
}
