namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5UnloadingDifferencesTabUserControl
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
			this.components = new System.ComponentModel.Container();
			this.UnloadingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnloadingDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.GuaranteeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GuaranteeGroupBoxDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.UnloadingDifferencesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SubSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ArrivalTransportInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnloadingDifferencesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.UnloadingDifferencesContainersEquipmentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ArrivalContainersAndSealsUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5ArrivalContainersAndSealsUserControl();
			this.UnloadingDifferencesDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentsMainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.UnloadingDifferencesSupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnloadingDifferencesSupportingDocumentsGridUserControl = new Enterprise.Customs.EU.NCTS.GUI.UnloadingDifferencesSupportingDocumentsGridUserControl();
			this.DocumentsSubSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.UnloadingDifferencesAdditionalDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl = new Enterprise.Customs.EU.NCTS.GUI.UnloadingDifferencesAdditionalDocumentsGridUserControl();
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5UnloadingDifferencesPreviousDocumentsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadingDetailsGroupBox.SuspendLayout();
			this.GuaranteeGroupBox.SuspendLayout();
			this.UnloadingDifferencesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubSplitContainer)).BeginInit();
			this.SubSplitContainer.Panel1.SuspendLayout();
			this.SubSplitContainer.Panel2.SuspendLayout();
			this.SubSplitContainer.SuspendLayout();
			this.UnloadingDifferencesTabControl.SuspendLayout();
			this.UnloadingDifferencesContainersEquipmentTabPage.SuspendLayout();
			this.ArrivalContainersAndSealsUserControl.SuspendLayout();
			this.UnloadingDifferencesDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsMainSplitContainer)).BeginInit();
			this.DocumentsMainSplitContainer.Panel1.SuspendLayout();
			this.DocumentsMainSplitContainer.Panel2.SuspendLayout();
			this.DocumentsMainSplitContainer.SuspendLayout();
			this.UnloadingDifferencesSupportingDocumentsGroupBox.SuspendLayout();
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsSubSplitContainer)).BeginInit();
			this.DocumentsSubSplitContainer.Panel1.SuspendLayout();
			this.DocumentsSubSplitContainer.Panel2.SuspendLayout();
			this.DocumentsSubSplitContainer.SuspendLayout();
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.SuspendLayout();
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.SuspendLayout();
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// UnloadingDetailsGroupBox
			// 
			this.UnloadingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("807183e2-2515-4d31-a9d6-751b5a1601e7", "Unloading Details");
			this.UnloadingDetailsGroupBox.Controls.Add(this.UnloadingDetailsDynamicLayoutPanel);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnloadingDetailsGroupBox, false);
			this.UnloadingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDetailsGroupBox.Name = "UnloadingDetailsGroupBox";
			this.UnloadingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 450, true);
			this.UnloadingDetailsGroupBox.TabIndex = 1;
			this.UnloadingDetailsGroupBox.TabStop = false;
			// 
			// UnloadingDetailsDynamicLayoutPanel
			// 
			this.UnloadingDetailsDynamicLayoutPanel.AllowDrop = true;
			this.UnloadingDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.UnloadingDetailsDynamicLayoutPanel.Name = "UnloadingDetailsDynamicLayoutPanel";
			this.UnloadingDetailsDynamicLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.UnloadingDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 431, true);
			this.UnloadingDetailsDynamicLayoutPanel.TabIndex = 0;
			// 
			// GuaranteeGroupBox
			// 
			this.GuaranteeGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ED5F79D8-B80C-407D-AE8B-050387E40629", "Guarantee");
			this.GuaranteeGroupBox.Controls.Add(this.GuaranteeGroupBoxDynamicLayoutPanel);
			this.GuaranteeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 460, true);
			this.GuaranteeGroupBox.Name = "GuaranteeGroupBox";
			this.GuaranteeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 130, true);
			this.GuaranteeGroupBox.TabIndex = 2;
			this.GuaranteeGroupBox.TabStop = false;
			this.GuaranteeGroupBox.Visible = false;
			// 
			// GuaranteeGroupBoxDynamicLayoutPanel
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeGroupBoxDynamicLayoutPanel, "ArrivalMovementHeader.SingleGuaranteeForArrival");
			this.GuaranteeGroupBoxDynamicLayoutPanel.AllowDrop = true;
			this.GuaranteeGroupBoxDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteeGroupBoxDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GuaranteeGroupBoxDynamicLayoutPanel.Name = "GuaranteeGroupBoxDynamicLayoutPanel";
			this.GuaranteeGroupBoxDynamicLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.GuaranteeGroupBoxDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 128, true);
			this.GuaranteeGroupBoxDynamicLayoutPanel.TabIndex = 0;
			// 
			// UnloadingDifferencesGroupBox
			// 
			this.UnloadingDifferencesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.UnloadingDifferencesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("bea32d5d-eabe-4a5a-8fd1-7690a2679e29", "Unloading Differences");
			this.UnloadingDifferencesGroupBox.Controls.Add(this.MainSplitContainer);
			this.UnloadingDifferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 0, true);
			this.UnloadingDifferencesGroupBox.Name = "UnloadingDifferencesGroupBox";
			this.UnloadingDifferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 746, true);
			this.UnloadingDifferencesGroupBox.TabIndex = 2;
			this.UnloadingDifferencesGroupBox.TabStop = false;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel);
			this.MainSplitContainer.Panel1.Controls.Add(this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.SubSplitContainer);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 727, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(181);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// UnloadingDifferencesDeclaredValueDynamicLayoutPanel
			// 
			this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel.AllowDrop = true;
			this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel.Name = "UnloadingDifferencesDeclaredValueDynamicLayoutPanel";
			this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 100, true);
			this.UnloadingDifferencesDeclaredValueDynamicLayoutPanel.TabIndex = 0;
			// 
			// UnloadingDifferencesUnloadedValueDynamicLayoutPanel
			// 
			this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel.AllowDrop = true;
			this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 16, true);
			this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel.Name = "UnloadingDifferencesUnloadedValueDynamicLayoutPanel";
			this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 100, true);
			this.UnloadingDifferencesUnloadedValueDynamicLayoutPanel.TabIndex = 1;
			// 
			// SubSplitContainer
			// 
			this.SubSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubSplitContainer.Name = "SubSplitContainer";
			this.SubSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SubSplitContainer.Panel1
			// 
			this.SubSplitContainer.Panel1.Controls.Add(this.ArrivalTransportInfoGroupBox);
			// 
			// SubSplitContainer.Panel2
			// 
			this.SubSplitContainer.Panel2.Controls.Add(this.UnloadingDifferencesTabControl);
			this.SubSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 542, true);
			this.SubSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(135);
			this.SubSplitContainer.TabIndex = 0;
			// 
			// ArrivalTransportInfoGroupBox
			// 
			this.ArrivalTransportInfoGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("418B6EBD-F334-463D-A12E-615021C36BFF", "Transport Info");
			this.ArrivalTransportInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArrivalTransportInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ArrivalTransportInfoGroupBox.Name = "ArrivalTransportInfoGroupBox";
			this.ArrivalTransportInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 135, true);
			this.ArrivalTransportInfoGroupBox.TabIndex = 1;
			this.ArrivalTransportInfoGroupBox.TabStop = false;
			// 
			// UnloadingDifferencesTabControl
			// 
			this.UnloadingDifferencesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.UnloadingDifferencesTabControl.Controls.Add(this.UnloadingDifferencesContainersEquipmentTabPage);
			this.UnloadingDifferencesTabControl.Controls.Add(this.UnloadingDifferencesDocumentsTabPage);
			this.UnloadingDifferencesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDifferencesTabControl.Name = "UnloadingDifferencesTabControl";
			this.UnloadingDifferencesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 403, true);
			this.UnloadingDifferencesTabControl.TabIndex = 4;
			// 
			// UnloadingDifferencesContainersEquipmentTabPage
			// 
			this.UnloadingDifferencesContainersEquipmentTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UnloadingDifferencesContainersEquipmentTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b1bf75cf-4c65-4a2c-9879-f8713f576b1e", "Containers/Equipment");
			this.UnloadingDifferencesContainersEquipmentTabPage.Controls.Add(this.ArrivalContainersAndSealsUserControl);
			this.UnloadingDifferencesContainersEquipmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnloadingDifferencesContainersEquipmentTabPage.Name = "UnloadingDifferencesContainersEquipmentTabPage";
			this.UnloadingDifferencesContainersEquipmentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnloadingDifferencesContainersEquipmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 376, true);
			this.UnloadingDifferencesContainersEquipmentTabPage.TabIndex = 0;
			// 
			// ArrivalContainersAndSealsUserControl
			// 
			this.ArrivalContainersAndSealsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalContainersAndSealsUserControl, ".");
			this.ArrivalContainersAndSealsUserControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("a35eda09-4fdc-4cf2-be53-bf1e04de46de", "Containers/Equipment and Seals");
			this.ArrivalContainersAndSealsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArrivalContainersAndSealsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ArrivalContainersAndSealsUserControl.Name = "ArrivalContainersAndSealsUserControl";
			this.ArrivalContainersAndSealsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 370, true);
			this.ArrivalContainersAndSealsUserControl.TabIndex = 3;
			// 
			// UnloadingDifferencesDocumentsTabPage
			// 
			this.UnloadingDifferencesDocumentsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UnloadingDifferencesDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b3143e38-89be-4cc8-ab17-f2893f15c21b", "Documents");
			this.UnloadingDifferencesDocumentsTabPage.Controls.Add(this.DocumentsMainSplitContainer);
			this.UnloadingDifferencesDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnloadingDifferencesDocumentsTabPage.Name = "UnloadingDifferencesDocumentsTabPage";
			this.UnloadingDifferencesDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnloadingDifferencesDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 376, true);
			this.UnloadingDifferencesDocumentsTabPage.TabIndex = 1;
			// 
			// DocumentsMainSplitContainer
			// 
			this.DocumentsMainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsMainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DocumentsMainSplitContainer.Name = "DocumentsMainSplitContainer";
			this.DocumentsMainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DocumentsMainSplitContainer.Panel1
			// 
			this.DocumentsMainSplitContainer.Panel1.Controls.Add(this.UnloadingDifferencesSupportingDocumentsGroupBox);
			// 
			// DocumentsMainSplitContainer.Panel2
			// 
			this.DocumentsMainSplitContainer.Panel2.Controls.Add(this.DocumentsSubSplitContainer);
			this.DocumentsMainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 370, true);
			this.DocumentsMainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(262);
			this.DocumentsMainSplitContainer.TabIndex = 0;
			// 
			// UnloadingDifferencesSupportingDocumentsGroupBox
			// 
			this.UnloadingDifferencesSupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("facc0cd6-d160-4c25-adec-42a4909fbde6", "Supporting Documents");
			this.UnloadingDifferencesSupportingDocumentsGroupBox.Controls.Add(this.UnloadingDifferencesSupportingDocumentsGridUserControl);
			this.UnloadingDifferencesSupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesSupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDifferencesSupportingDocumentsGroupBox.Name = "UnloadingDifferencesSupportingDocumentsGroupBox";
			this.UnloadingDifferencesSupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 262, true);
			this.UnloadingDifferencesSupportingDocumentsGroupBox.TabIndex = 2;
			this.UnloadingDifferencesSupportingDocumentsGroupBox.TabStop = false;
			// 
			// UnloadingDifferencesSupportingDocumentsGridUserControl
			// 
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingDifferencesSupportingDocumentsGridUserControl, "ArrivalMovementHeader.SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupportingDocuments)));
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.Name = "UnloadingDifferencesSupportingDocumentsGridUserControl";
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 5, 0, 0, true);
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 243, true);
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.TabIndex = 0;
			// 
			// DocumentsSubSplitContainer
			// 
			this.DocumentsSubSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsSubSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentsSubSplitContainer.Name = "DocumentsSubSplitContainer";
			this.DocumentsSubSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DocumentsSubSplitContainer.Panel1
			// 
			this.DocumentsSubSplitContainer.Panel1.Controls.Add(this.UnloadingDifferencesAdditionalDocumentsGroupBox);
			// 
			// DocumentsSubSplitContainer.Panel2
			// 
			this.DocumentsSubSplitContainer.Panel2.Controls.Add(this.Phase5UnloadingDifferencesPreviousDocumentsUserControl);
			this.DocumentsSubSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 104, true);
			this.DocumentsSubSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(73);
			this.DocumentsSubSplitContainer.TabIndex = 0;
			// 
			// UnloadingDifferencesAdditionalDocumentsGroupBox
			// 
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("3a2bd92c-e07e-400e-81ff-52745d09764e", "Additional Documents");
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.Controls.Add(this.UnloadingDifferencesAdditionalDocumentsGridUserControl);
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.Name = "UnloadingDifferencesAdditionalDocumentsGroupBox";
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 73, true);
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.TabIndex = 3;
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.TabStop = false;
			// 
			// UnloadingDifferencesAdditionalDocumentsGridUserControl
			// 
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingDifferencesAdditionalDocumentsGridUserControl, "ArrivalMovementHeader.AdditionalDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.INctsAdditionalInfoCollection<Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalDocuments)));
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.Name = "UnloadingDifferencesAdditionalDocumentsGridUserControl";
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 5, 0, 0, true);
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 54, true);
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.TabIndex = 0;
			// 
			// Phase5UnloadingDifferencesPreviousDocumentsUserControl
			// 
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Phase5UnloadingDifferencesPreviousDocumentsUserControl, ".");
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("aaad27e8-a263-4781-ab76-7cc0b764f549", "Previous Documents");
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.Name = "Phase5UnloadingDifferencesPreviousDocumentsUserControl";
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 27, true);
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.TabIndex = 2;
			// 
			// Phase5UnloadingDifferencesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadingDetailsGroupBox);
			this.Controls.Add(this.GuaranteeGroupBox);
			this.Controls.Add(this.UnloadingDifferencesGroupBox);
			this.Name = "Phase5UnloadingDifferencesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1446, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadingDetailsGroupBox.ResumeLayout(false);
			this.UnloadingDetailsGroupBox.PerformLayout();
			this.GuaranteeGroupBox.ResumeLayout(false);
			this.GuaranteeGroupBox.PerformLayout();
			this.UnloadingDifferencesGroupBox.ResumeLayout(false);
			this.UnloadingDifferencesGroupBox.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.SubSplitContainer.Panel1.ResumeLayout(false);
			this.SubSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SubSplitContainer)).EndInit();
			this.SubSplitContainer.ResumeLayout(false);
			this.SubSplitContainer.PerformLayout();
			this.UnloadingDifferencesTabControl.ResumeLayout(false);
			this.UnloadingDifferencesTabControl.PerformLayout();
			this.UnloadingDifferencesContainersEquipmentTabPage.ResumeLayout(false);
			this.UnloadingDifferencesContainersEquipmentTabPage.PerformLayout();
			this.ArrivalContainersAndSealsUserControl.ResumeLayout(true);
			this.ArrivalContainersAndSealsUserControl.PerformLayout();
			this.UnloadingDifferencesDocumentsTabPage.ResumeLayout(false);
			this.UnloadingDifferencesDocumentsTabPage.PerformLayout();
			this.DocumentsMainSplitContainer.Panel1.ResumeLayout(false);
			this.DocumentsMainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DocumentsMainSplitContainer)).EndInit();
			this.DocumentsMainSplitContainer.ResumeLayout(false);
			this.DocumentsMainSplitContainer.PerformLayout();
			this.UnloadingDifferencesSupportingDocumentsGroupBox.ResumeLayout(false);
			this.UnloadingDifferencesSupportingDocumentsGroupBox.PerformLayout();
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.ResumeLayout(true);
			this.UnloadingDifferencesSupportingDocumentsGridUserControl.PerformLayout();
			this.DocumentsSubSplitContainer.Panel1.ResumeLayout(false);
			this.DocumentsSubSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DocumentsSubSplitContainer)).EndInit();
			this.DocumentsSubSplitContainer.ResumeLayout(false);
			this.DocumentsSubSplitContainer.PerformLayout();
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.ResumeLayout(false);
			this.UnloadingDifferencesAdditionalDocumentsGroupBox.PerformLayout();
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.ResumeLayout(true);
			this.UnloadingDifferencesAdditionalDocumentsGridUserControl.PerformLayout();
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.ResumeLayout(true);
			this.Phase5UnloadingDifferencesPreviousDocumentsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox UnloadingDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel UnloadingDetailsDynamicLayoutPanel;
		protected internal ZArchitecture.GUI.ZGroupBox GuaranteeGroupBox;
		public ZArchitecture.GUI.DynamicLayoutPanel GuaranteeGroupBoxDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox UnloadingDifferencesGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel UnloadingDifferencesDeclaredValueDynamicLayoutPanel;
		internal ZArchitecture.GUI.DynamicLayoutPanel UnloadingDifferencesUnloadedValueDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox UnloadingDifferencesSupportingDocumentsGroupBox;
		internal Enterprise.Customs.EU.NCTS.GUI.UnloadingDifferencesSupportingDocumentsGridUserControl UnloadingDifferencesSupportingDocumentsGridUserControl;
		internal ZArchitecture.GUI.ZGroupBox UnloadingDifferencesAdditionalDocumentsGroupBox;
		internal Enterprise.Customs.EU.NCTS.GUI.UnloadingDifferencesAdditionalDocumentsGridUserControl UnloadingDifferencesAdditionalDocumentsGridUserControl;
		internal Phase5UnloadingDifferencesPreviousDocumentsUserControl Phase5UnloadingDifferencesPreviousDocumentsUserControl;
		internal Phase5ArrivalContainersAndSealsUserControl ArrivalContainersAndSealsUserControl;
		internal ZArchitecture.GUI.ZTabControl UnloadingDifferencesTabControl;
		internal ZArchitecture.GUI.ZTabPage UnloadingDifferencesContainersEquipmentTabPage;
		internal ZArchitecture.GUI.ZTabPage UnloadingDifferencesDocumentsTabPage;
		internal ZArchitecture.GUI.ZGroupBox ArrivalTransportInfoGroupBox;
		internal CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer SubSplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer DocumentsMainSplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer DocumentsSubSplitContainer;
	}
}
