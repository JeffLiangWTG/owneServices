using System.Windows.Forms;

namespace Enterprise.Customs.EU.GUI
{
	partial class GuidedDecisionMakingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.GuidedDecisionMakingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.NavigationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BasicTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BasicGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GDMBasicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.NextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdditionalCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalCodesContainerPanel = new AdditionalCodesContainerPanel();
			this.MeursingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MeursingUserControl = new MeursingUserControl(GDMBasic.MeursingManager, closeFormAfterCalculation: false);
			this.MeursingResultUserControl = new MeursingResultUserControl();
			this.MeursingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConditionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConditionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentConditionsContainerPanel = new DocumentConditionsContainerPanel();
			this.VATTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.VATContainerPanel = new VATContainerPanel();
			this.VATGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SummaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CancelGDMButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DoneButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllWaiversLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.SummaryControl = GetSummaryControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GuidedDecisionMakingTabControl.SuspendLayout();
			this.NavigationPanel.SuspendLayout();
			this.BasicTabPage.SuspendLayout();
			this.BasicGroupBox.SuspendLayout();
			this.AdditionalCodesTabPage.SuspendLayout();
			this.AdditionalCodesGroupBox.SuspendLayout();
			this.MeursingTabPage.SuspendLayout();
			this.MeursingGroupBox.SuspendLayout();
			this.ConditionsTabPage.SuspendLayout();
			this.ConditionsGroupBox.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.SummaryGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 628, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = this.ExpectedDataSourceType;
			// 
			// NavigationPanel
			// 
			this.NavigationPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.NavigationPanel.Controls.Add(this.PreviousButton);
			this.NavigationPanel.Controls.Add(this.CancelGDMButton);
			this.NavigationPanel.Controls.Add(this.NextButton);
			this.NavigationPanel.Controls.Add(this.DoneButton);
			this.NavigationPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NavigationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 610, true);
			this.NavigationPanel.Name = "NavigationPanel";
			this.NavigationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 33, true);
			this.NavigationPanel.TabIndex = 1;
			// 
			// GuidedDecisionMakingTabControl
			// 
			this.GuidedDecisionMakingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
			this.GuidedDecisionMakingTabControl.Controls.Add(this.BasicTabPage);
			this.GuidedDecisionMakingTabControl.Controls.Add(this.VATTabPage);
			this.GuidedDecisionMakingTabControl.Controls.Add(this.AdditionalCodesTabPage);
			this.GuidedDecisionMakingTabControl.Controls.Add(this.MeursingTabPage);
			this.GuidedDecisionMakingTabControl.Controls.Add(this.ConditionsTabPage);
			this.GuidedDecisionMakingTabControl.Controls.Add(this.SummaryTabPage);
			this.GuidedDecisionMakingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuidedDecisionMakingTabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
			this.GuidedDecisionMakingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GuidedDecisionMakingTabControl.Name = "GuidedDecisionMakingTabControl";
			this.GuidedDecisionMakingTabControl.SelectedIndex = 0;
			this.GuidedDecisionMakingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 660, true);
			this.GuidedDecisionMakingTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 38);
			this.GuidedDecisionMakingTabControl.SizeMode = TabSizeMode.Fixed;
			this.GuidedDecisionMakingTabControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4);
			this.GuidedDecisionMakingTabControl.TabIndex = 0;
			this.GuidedDecisionMakingTabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.GuidedDecisionMakingTabControl_DrawItem);
			this.GuidedDecisionMakingTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.GuidedDecisionMakingTabControl_Selecting);
			this.GuidedDecisionMakingTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.GuidedDecisionMakingTabControl_Selected);
			this.GuidedDecisionMakingTabControl.Alignment = System.Windows.Forms.TabAlignment.Top;
			// 
			// BasicTabPage
			// 
			this.BasicTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                 | System.Windows.Forms.AnchorStyles.Right)));
			this.BasicTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("76F1850C-677E-4307-A513-B8E2A5922667", "1. Basic");
			this.BasicTabPage.Controls.Add(this.BasicGroupBox);
			this.BasicTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicTabPage.Name = "BasicTabPage";
			this.BasicTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 660, true);
			this.BasicTabPage.TabIndex = 0;
			// 
			// BasicGroupBox
			// 
			this.BasicGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                  | System.Windows.Forms.AnchorStyles.Right)));
			this.BasicGroupBox.Controls.Add(this.GDMBasicLayoutPanel);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BasicGroupBox, false);
			this.BasicGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicGroupBox.Name = "BasicGroupBox";
			this.BasicGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 580, true);
			this.BasicGroupBox.TabIndex = 0;
			this.BasicGroupBox.TabStop = false;
			// 
			// GDMBasicLayoutPanel
			// 
			this.GDMBasicLayoutPanel.AllowDrop = true;
			this.GDMBasicLayoutPanel.AutoScroll = true;
			this.GDMBasicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GDMBasicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GDMBasicLayoutPanel.Name = "GDMBasicLayoutPanel";
			this.GDMBasicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 580, true);
			this.GDMBasicLayoutPanel.TabIndex = 1;
			// 
			// VATTabPage
			// 
			this.VATTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																				  | System.Windows.Forms.AnchorStyles.Right)));
			this.VATTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("854A59CE-5329-4384-A6DC-C2595A188FA4", "2. VAT");
			this.VATTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.VATTabPage.Name = "VATTabPage";
			this.VATTabPage.Controls.Add(this.VATGroupBox);
			this.VATTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 660, true);
			this.VATTabPage.TabIndex = 1;
			// 
			// VATGroupBox
			// 
			this.VATGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																				   | System.Windows.Forms.AnchorStyles.Right)));
			this.VATGroupBox.Controls.Add(this.VATContainerPanel);
			this.VATGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VATGroupBox.Name = "VATGroupBox";
			this.VATGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 580, true);
			this.VATGroupBox.TabIndex = 0;
			this.VATGroupBox.TabStop = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VATGroupBox, false);
			// 
			// VATContainerPanel
			//
			this.VATContainerPanel.AllowDrop = true;
			this.VATContainerPanel.AutoScroll = true;
			this.VATContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VATContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VATContainerPanel.Name = "VATContainerPanel";
			this.VATContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 547, true);
			this.VATContainerPanel.TabIndex = 1;
			// 
			// AdditionalCodesTabPage
			// 
			this.AdditionalCodesTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                           | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalCodesTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9FF96924-2027-45E1-AF83-2B6278D426F9", "3. Additional Codes");
			this.AdditionalCodesTabPage.Controls.Add(this.AdditionalCodesGroupBox);
			this.AdditionalCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.AdditionalCodesTabPage.Name = "AdditionalCodesTabPage";
			this.AdditionalCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 660, true);
			this.AdditionalCodesTabPage.TabIndex = 2;
			// 
			// AdditionalCodesGroupBox
			// 
			this.AdditionalCodesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalCodesGroupBox.Controls.Add(this.AdditionalCodesContainerPanel);
			this.AdditionalCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalCodesGroupBox.Name = "AdditionalCodesGroupBox";
			this.AdditionalCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 580, true);
			this.AdditionalCodesGroupBox.TabIndex = 0;
			this.AdditionalCodesGroupBox.TabStop = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalCodesGroupBox, false);
			// 
			// AdditionalCodesContainerPanel
			// 
			this.AdditionalCodesContainerPanel.AllowDrop = true;
			this.AdditionalCodesContainerPanel.AutoScroll = true;
			this.AdditionalCodesContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalCodesContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalCodesContainerPanel.Name = "AdditionalCodesContainerPanel";
			this.AdditionalCodesContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 580, true);
			this.AdditionalCodesContainerPanel.TabIndex = 1;
			// 
			// MeursingTabPage
			// 
			this.MeursingTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                    | System.Windows.Forms.AnchorStyles.Right)));
			this.MeursingTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("08A08F7B-076C-452F-89E5-24E77785EB5E", "4. Meursing");
			this.MeursingTabPage.Controls.Add(this.MeursingGroupBox);
			this.MeursingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.MeursingTabPage.Name = "MeursingTabPage";
            this.MeursingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 660, true);
            this.MeursingTabPage.TabIndex = 3;
			// 
			// MeursingGroupBox
			// 
			this.MeursingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                     | System.Windows.Forms.AnchorStyles.Right)));
			this.MeursingGroupBox.Controls.Add(this.MeursingUserControl);
			this.MeursingGroupBox.Controls.Add(this.MeursingResultUserControl);
			this.MeursingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MeursingGroupBox.Name = "MeursingGroupBox";
            this.MeursingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 580, true);
            this.MeursingGroupBox.TabIndex = 0;
            this.MeursingGroupBox.TabStop = false;
			// 
			// MeursingUserControl
			//
			this.MeursingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 0, true);
			this.MeursingUserControl.Name = "MeursingUserControl";
			this.MeursingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 150, true);
			this.MeursingUserControl.TabIndex = 0;
			this.MeursingUserControl.CaptionRenderingEnabled = true;
			//
			//MeursingResultUserControl
			//
			this.MeursingResultUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
			this.MeursingResultUserControl.Name = "MeursingResultUserControl";
			this.MeursingResultUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 67, true);
			this.MeursingResultUserControl.TabIndex = 0;
			// 
			// ConditionsTabPage
			// 
			this.ConditionsTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                      | System.Windows.Forms.AnchorStyles.Right)));
			this.ConditionsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4CF20BCF-F2FA-4CFF-8F1F-071DFE620DD1", "5. Conditions");
			this.ConditionsTabPage.Controls.Add(this.ConditionsGroupBox);
			this.ConditionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.ConditionsTabPage.Name = "ConditionsTabPage";
			this.ConditionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 660, true);
			this.ConditionsTabPage.TabIndex = 4;
			// 
			// ConditionsGroupBox
			// 
			this.ConditionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                       | System.Windows.Forms.AnchorStyles.Right)));
			this.ConditionsGroupBox.Controls.Add(this.DocumentConditionsContainerPanel);
			this.ConditionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConditionsGroupBox.Name = "ConditionsGroupBox";
			this.ConditionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 580, true);
			this.ConditionsGroupBox.TabIndex = 0;
			this.ConditionsGroupBox.TabStop = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConditionsGroupBox, false);
			// 
			// SelectAllWaiversLinkLabel
			//
			this.SelectAllWaiversLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAllWaiversLinkLabel.AutoSize = true;
			this.SelectAllWaiversLinkLabel.Text = "Select All Waivers";
			this.SelectAllWaiversLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(810, 0, true);
			this.SelectAllWaiversLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectAllWaiversLinkLabelClicked);
			this.SelectAllWaiversLinkLabel.TabIndex = 1;
			// 
			// DocumentConditionsContainerPanel
			// 
			this.DocumentConditionsContainerPanel.AllowDrop = true;
			this.DocumentConditionsContainerPanel.AutoScroll = true;
			this.DocumentConditionsContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentConditionsContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentConditionsContainerPanel.Name = "DocumentConditionsContainerPanel";
			this.DocumentConditionsContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 547, true);
			this.DocumentConditionsContainerPanel.TabIndex = 1;
			this.DocumentConditionsContainerPanel.Controls.Add(this.SelectAllWaiversLinkLabel);
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                   | System.Windows.Forms.AnchorStyles.Right)));
			this.SummaryTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9A4CD187-2852-420F-8F04-0D15B3A1B1BA", "6. Summary");
			this.SummaryTabPage.Controls.Add(this.SummaryGroupBox);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 660, true);
			this.SummaryTabPage.TabIndex = 5;
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			                                                                    | System.Windows.Forms.AnchorStyles.Right)));
			this.SummaryGroupBox.Controls.Add(this.SummaryControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SummaryGroupBox, false);
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 580, true);
			this.SummaryGroupBox.TabIndex = 0;
			this.SummaryGroupBox.TabStop = false;
			// 
			// SummaryControl
			// 
			this.SummaryControl.AutoScroll = true;
			this.SummaryControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryControl.Name = "SummaryControl";
			this.SummaryControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 580, true);
			this.SummaryControl.TabIndex = 0;
			// 
			// CancelGDMButton
			// 
			this.CancelGDMButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelGDMButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("115368CE-C0B4-4088-ABEE-9F66805A0CD5", "Cancel");
			this.CancelGDMButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 4, true);
			this.CancelGDMButton.Name = "CancelGDMButton";
			this.CancelGDMButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 28, true);
			this.CancelGDMButton.TabIndex = 6;
			this.CancelGDMButton.ToolTipCaption = null;
			this.CancelGDMButton.UseVisualStyleBackColor = true;
			this.CancelGDMButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// PreviousButton
			// 
			this.PreviousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviousButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("12EA963C-B10E-47A8-BFE0-15B7F6C90E61", "Previous");
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 4, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 28, true);
			this.PreviousButton.TabIndex = 5;
			this.PreviousButton.ToolTipCaption = null;
			this.PreviousButton.UseVisualStyleBackColor = true;
			this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NextButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9E3BB557-E390-4EA4-B3D8-C0778F66A998", "Next");
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(810, 4, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 28, true);
			this.NextButton.TabIndex = 7;
			this.NextButton.ToolTipCaption = null;
			this.NextButton.UseVisualStyleBackColor = true;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			//
			// DoneButton
			// 
			this.DoneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DoneButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("32C6E198-A76C-48D2-87F9-6B70F740682E", "Done");
			this.DoneButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(810, 4, true);
			this.DoneButton.Name = "DoneButton";
			this.DoneButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 28, true);
			this.DoneButton.TabIndex = 8;
			this.DoneButton.ToolTipCaption = null;
			this.DoneButton.UseVisualStyleBackColor = true;
			this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
			// 
			// GuidedDecisionMakingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("723EDD1A-D134-4E23-969C-9F3DCD284C9E", "Guided Decision Making");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 680, true);
			this.Controls.Add(this.NavigationPanel);
			this.Controls.Add(this.GuidedDecisionMakingTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 715, true);
			this.MinimizeBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "GuidedDecisionMakingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.GuidedDecisionMakingTabControl, 0);
			this.Controls.SetChildIndex(this.NavigationPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GuidedDecisionMakingTabControl.ResumeLayout(false);
			this.GuidedDecisionMakingTabControl.PerformLayout();
			this.NavigationPanel.ResumeLayout(false);
			this.NavigationPanel.PerformLayout();
			this.BasicTabPage.ResumeLayout(false);
			this.BasicTabPage.PerformLayout();
			this.BasicGroupBox.ResumeLayout(false);
			this.BasicGroupBox.PerformLayout();
			this.AdditionalCodesTabPage.ResumeLayout(false);
			this.AdditionalCodesTabPage.PerformLayout();
			this.AdditionalCodesGroupBox.ResumeLayout(false);
			this.AdditionalCodesGroupBox.PerformLayout();
			this.MeursingTabPage.ResumeLayout(false);
			this.MeursingTabPage.PerformLayout();
			this.MeursingGroupBox.ResumeLayout(false);
			this.MeursingGroupBox.PerformLayout();
			this.ConditionsTabPage.ResumeLayout(false);
			this.ConditionsTabPage.PerformLayout();
			this.ConditionsGroupBox.ResumeLayout(false);
			this.ConditionsGroupBox.PerformLayout();
			this.SummaryTabPage.ResumeLayout(false);
			this.SummaryTabPage.PerformLayout();
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZTabControl GuidedDecisionMakingTabControl;
		private ZArchitecture.GUI.ZPanel NavigationPanel;
		private ZArchitecture.GUI.ZTabPage BasicTabPage;
		private ZArchitecture.GUI.ZTabPage AdditionalCodesTabPage;
		private ZArchitecture.GUI.ZTabPage MeursingTabPage;
		private ZArchitecture.GUI.ZTabPage ConditionsTabPage;
		private ZArchitecture.GUI.ZTabPage VATTabPage;
		private ZArchitecture.GUI.ZTabPage SummaryTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox BasicGroupBox;
		private AdditionalCodesContainerPanel AdditionalCodesContainerPanel;
		private DocumentConditionsContainerPanel DocumentConditionsContainerPanel;
		private VATContainerPanel VATContainerPanel;
		private Enterprise.ZArchitecture.GUI.ZButton PreviousButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelGDMButton;
		private Enterprise.ZArchitecture.GUI.ZButton NextButton;
		private Enterprise.ZArchitecture.GUI.ZButton DoneButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalCodesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MeursingGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ConditionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox VATGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SummaryGroupBox;
		private Enterprise.ZArchitecture.GUI.DynamicLayoutPanel GDMBasicLayoutPanel;
		private MeursingUserControl MeursingUserControl;
		internal MeursingResultUserControl MeursingResultUserControl;
		private SummaryControl SummaryControl;
		internal Enterprise.ZArchitecture.GUI.ZLinkLabel SelectAllWaiversLinkLabel;
	}
}
