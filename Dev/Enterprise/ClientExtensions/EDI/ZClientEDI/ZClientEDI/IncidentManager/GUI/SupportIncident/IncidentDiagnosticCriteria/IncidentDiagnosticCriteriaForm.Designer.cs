using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentDiagnosticCriteriaForm
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

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 644, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 617, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 617, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 617, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 644, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(593, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_Type)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_Question)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_InternalSupportNote)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_Keywords)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IncidentTriagePivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).Lookups.IncidentTriageNotLinked)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_IsActive)));
			// 
			// 
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria)(null)).IMD_FocusRelatedTriageNodesOnly)));
			// 
			// IncidentDiagnosticCriteriaForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(899, 700, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.IncidentDiagnosticCriteria";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 458, true);
			this.Name = "IncidentDiagnosticCriteriaForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Diagnostic Criterion";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.descriptionZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.questionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.internalSupportNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.keywordsZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1 = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentDiagnosticCriteriaTriageModuleButtonGrid();
			this.incidentDiagnosticCriteriaTriageModuleGridLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FocusRelatedTriageNodesOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabPage.SuspendLayout();
			this.typeDropEdit.SuspendLayout();
			this.tableLayoutPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid)).BeginInit();
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.SuspendLayout();
			this.MainTabPage.Controls.Add(this.zPanel1);
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.typeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.typeDropEdit, "IMD_Type");
			this.typeDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("5a806673-6e12-4816-84ce-b0d462334100", "Type");
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 35, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 20, true);
			this.typeDropEdit.TabIndex = 2;
			this.typeDropEdit.TabStop = false;
			// 
			// isActiveCheckBox
			// 
			this.isActiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "IMD_IsActive");
			this.isActiveCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("99127e4c-0628-4579-89e5-90d9680b2753", "Is Active");
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 35, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 24, true);
			this.isActiveCheckBox.TabIndex = 3;
			this.isActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.zGroupBox4, 0, 3);
			this.tableLayoutPanel.Controls.Add(this.zGroupBox1, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.zGroupBox2, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.zGroupBox3, 0, 2);
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 4;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50)));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 491, true);
			this.tableLayoutPanel.TabIndex = 4;
			// 
			// descriptionZTextBox
			// 
			this.descriptionZTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.descriptionZTextBox, "IMD_Description");
			this.descriptionZTextBox.CaptionResourceString = ZClientEDI.Res.GetData("9ebbe1ac-81f2-4eb9-ba33-81e5b2d0c260", "Description");
			this.descriptionZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 9, true);
			this.descriptionZTextBox.Name = "descriptionZTextBox";
			this.descriptionZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 20, true);
			this.descriptionZTextBox.TabIndex = 1;
			// 
			// questionTextBox
			// 
			this.questionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.questionTextBox, "IMD_Question");
			this.questionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.questionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 9, true);
			this.questionTextBox.Multiline = true;
			this.questionTextBox.Name = "questionTextBox";
			this.questionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.questionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 126, true);
			this.questionTextBox.TabIndex = 5;
			// 
			// internalSupportNoteTextBox
			// 
			this.internalSupportNoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.internalSupportNoteTextBox, "IMD_InternalSupportNote");
			this.internalSupportNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.internalSupportNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 9, true);
			this.internalSupportNoteTextBox.Multiline = true;
			this.internalSupportNoteTextBox.Name = "internalSupportNoteTextBox";
			this.internalSupportNoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.internalSupportNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 126, true);
			this.internalSupportNoteTextBox.TabIndex = 7;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.FocusRelatedTriageNodesOnlyCheckBox);
			this.zPanel1.Controls.Add(this.descriptionZTextBox);
			this.zPanel1.Controls.Add(this.tableLayoutPanel);
			this.zPanel1.Controls.Add(this.typeDropEdit);
			this.zPanel1.Controls.Add(this.isActiveCheckBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 575, true);
			this.zPanel1.TabIndex = 1;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.questionTextBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 141, true);
			this.zGroupBox1.TabIndex = 5;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.internalSupportNoteTextBox);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox2, false);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 150, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 141, true);
			this.zGroupBox2.TabIndex = 6;
			this.zGroupBox2.TabStop = false;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Controls.Add(this.keywordsZTextBox);
			this.zGroupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox3, false);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 297, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 44, true);
			this.zGroupBox3.TabIndex = 13;
			this.zGroupBox3.TabStop = false;
			// 
			// keywordsZTextBox
			// 
			this.keywordsZTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.keywordsZTextBox, "IMD_Keywords");
			this.keywordsZTextBox.CaptionResourceString = ZClientEDI.Res.GetData("325a3564-b033-4564-b5f7-f8dd0b516c24", "Keywords");
			this.keywordsZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.keywordsZTextBox.HideSelection = false;
			this.keywordsZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 9, true);
			this.keywordsZTextBox.Multiline = true;
			this.keywordsZTextBox.Name = "keywordsZTextBox";
			this.keywordsZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 29, true);
			this.keywordsZTextBox.TabIndex = 10;
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.Controls.Add(this.incidentDiagnosticCriteriaTriageModuleGridLabel);
			this.zGroupBox4.Controls.Add(this.incidentDiagnosticCriteriaTriageModuleButtonGrid1);
			this.zGroupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox4, false);
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 347, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 141, true);
			this.zGroupBox4.TabIndex = 14;
			this.zGroupBox4.TabStop = false;
			// 
			// incidentDiagnosticCriteriaTriageModuleButtonGrid1
			// 
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.AllowDrop = true;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.incidentDiagnosticCriteriaTriageModuleButtonGrid1, "IncidentTriagePivots");
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.BindToFindBoxList = "Lookups+IncidentTriageNotLinked";
			zTextBoxColumnStyleInfo9.Caption = "";
			zTextBoxColumnStyleInfo9.CaptionResourceString = ZClientEDI.Res.GetData("6beefb59-8c89-456c-8123-6cfc90748a9e", "Number");
			zTextBoxColumnStyleInfo9.ColumnName = "Triage+IMT_TriageNumber";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "";
			zTextBoxColumnStyleInfo10.CaptionResourceString = ZClientEDI.Res.GetData("c15c9b95-3bc4-426c-80e3-9881cfc5b144", "Type");
			zTextBoxColumnStyleInfo10.ColumnName = "Triage+TypeDescription";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "";
			zTextBoxColumnStyleInfo11.CaptionResourceString = ZClientEDI.Res.GetData("40077c32-5c81-4a4f-9dda-5b00effa0db5", "Description");
			zTextBoxColumnStyleInfo11.ColumnName = "Triage+IMT_SupportDescription";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo12.ColumnName = "Triage+IMT_Product";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "Triage+IMT_ProductArea";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "Triage+IMT_Module";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.Caption = "";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("3ce3e0d4-c64b-40cb-8a60-dcae6925804e", "Is Active");
			zCheckBoxColumnStyleInfo2.ColumnName = "Triage+IMT_IsActive";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("f1de2b93-af25-41d6-9fbc-3ead0d573175", "Created Time (UTC)");
			zDateEditColumnStyleInfo3.ColumnName = "Triage+IMT_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.GroupName = ZClientEDI.Res.GetData("a40dfef1-2e65-4223-adce-14fcd7561a4d", "Audit Details");
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo15.CaptionResourceString = ZClientEDI.Res.GetData("81a3ed35-9f92-4860-b8b3-0927e4ec8794", "Created By");
			zTextBoxColumnStyleInfo15.ColumnName = "Triage+IMT_SystemCreateUser";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.GroupName = ZClientEDI.Res.GetData("a40dfef1-2e65-4223-adce-14fcd7561a4d", "Audit Details");
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("519bcf0b-2cde-4660-9f88-91e1107d7cce", "Last Edited Time (UTC)");
			zDateEditColumnStyleInfo4.ColumnName = "Triage+IMT_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.GroupName = ZClientEDI.Res.GetData("a40dfef1-2e65-4223-adce-14fcd7561a4d", "Audit Details");
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo16.CaptionResourceString = ZClientEDI.Res.GetData("f3efd291-5dbc-4435-90b5-dbc56cb86834", "Last Edit");
			zTextBoxColumnStyleInfo16.ColumnName = "Triage+IMT_SystemLastEditUser";
			zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo16.GroupName = ZClientEDI.Res.GetData("a40dfef1-2e65-4223-adce-14fcd7561a4d", "Audit Details");
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.Caption = "";
			zTextBoxColumnStyleInfo17.CaptionResourceString = ZClientEDI.Res.GetData("462be34f-cd94-487d-9863-63c728a5dad5", "Level");
			zTextBoxColumnStyleInfo17.ColumnName = "Triage+LevelDescription";
			zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.GridId = null;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.AllowNavigation = false;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.CaptionVisible = false;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.GridId = null;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.LayoutKey = "Grid";
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.Name = "Grid";
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 88, true);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid.TabIndex = 0;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 9, true);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.Name = "incidentDiagnosticCriteriaTriageModuleButtonGrid1";
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ReadOnly = false;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ShowNewButton = false;
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 126, true);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.TabIndex = 0;
			// 
			// incidentDiagnosticCriteriaTriageModuleGridLabel
			// 
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.CaptionResourceString = ZClientEDI.Res.GetData("8ff72462-b1d7-4c83-8606-8f0e2738c1a6", "Related Triage Nodes");
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 16, true);
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.Name = "incidentDiagnosticCriteriaTriageModuleGridLabel";
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 86, true);
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.TabIndex = 1;
			this.incidentDiagnosticCriteriaTriageModuleGridLabel.UseMnemonic = false;
			// 
			// FocusRelatedTriageNodesOnlyCheckBox
			// 
			this.FocusRelatedTriageNodesOnlyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FocusRelatedTriageNodesOnlyCheckBox, "IMD_FocusRelatedTriageNodesOnly");
			this.FocusRelatedTriageNodesOnlyCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("038bca3f-457b-4b78-94b5-b4e5c33d5c16", "When confirmed, de-focus unrelated triage nodes");
			this.FocusRelatedTriageNodesOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 60, true);
			this.FocusRelatedTriageNodesOnlyCheckBox.Name = "FocusRelatedTriageNodesOnlyCheckBox";
			this.FocusRelatedTriageNodesOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.FocusRelatedTriageNodesOnlyCheckBox.TabIndex = 4;
			this.FocusRelatedTriageNodesOnlyCheckBox.UseVisualStyleBackColor = true;
			this.MainTabPage.PerformLayout();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.zGroupBox4.ResumeLayout(false);
			this.zGroupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.InnerGrid)).EndInit();
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.ResumeLayout(true);
			this.incidentDiagnosticCriteriaTriageModuleButtonGrid1.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}
		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		#endregion
		private ZDropEdit typeDropEdit;
		private ZCheckBox isActiveCheckBox;
		private ZArchitecture.ZTextBox descriptionZTextBox;
		private ZArchitecture.ZTextBox questionTextBox;
		private ZArchitecture.ZTextBox internalSupportNoteTextBox;
		private ZPanel zPanel1;
		private ZGroupBox zGroupBox1;
		private ZGroupBox zGroupBox2;
		private ZGroupBox zGroupBox3;
		private ZTextBox keywordsZTextBox;
		private ZGroupBox zGroupBox4;
		private IncidentDiagnosticCriteriaTriageModuleButtonGrid incidentDiagnosticCriteriaTriageModuleButtonGrid1;
		private ZLabel incidentDiagnosticCriteriaTriageModuleGridLabel;
		private ZCheckBox FocusRelatedTriageNodesOnlyCheckBox;
		protected KTableLayoutPanel tableLayoutPanel;
	}
}
