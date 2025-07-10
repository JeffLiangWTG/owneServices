namespace Enterprise.Customs.DE.GUI
{
	partial class TemporaryStorageForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.TemporaryStorageUserControl = new Enterprise.Customs.DE.GUI.TemporaryStorageUserControl();
			this.CUSPCSDeclarationUserControl = new Enterprise.Customs.DE.GUI.CUSPCSDeclarationUserControl();
			this.chgspoDeclarationUserControl = new Enterprise.Customs.DE.GUI.CHGSPODeclarationUserControl();
			this.rexdisDeclarationUserControl = new Enterprise.Customs.DE.GUI.REXDISDeclarationUserControl();
			this.PRLCONDeclarationUserControl = new Enterprise.Customs.DE.GUI.PRLCONDeclarationUserControl();
			this.AmendmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AmendmentsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ChangeCustodyTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.chgtstDeclarationUserControl = new Enterprise.Customs.DE.GUI.CHGTSTDeclarationUserControl();
			this.ChangeDisposalEntitledTraderTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.chgoffDeclarationUserControl = new Enterprise.Customs.DE.GUI.CHGOFFDeclarationUserControl();
			this.ChangeOwnerReferenceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsolidationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SplitTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CUSPRLTabPageUserControl = new Enterprise.Customs.DE.GUI.CUSPRLTabPageUserControl();
			this.SumADeclarationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DeclarationMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SumAMessagesUserControl = new Enterprise.Customs.DE.GUI.SumAMessagesUserControl();
			this.SumADeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RexDeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.REXDISMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.REXDISMessagesUserControl = new Enterprise.Customs.DE.GUI.REXDISMessagesUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WorkflowTabPage.SuspendLayout();
			this.TemporaryStorageUserControl.SuspendLayout();
			this.CUSPCSDeclarationUserControl.SuspendLayout();
			this.chgspoDeclarationUserControl.SuspendLayout();
			this.rexdisDeclarationUserControl.SuspendLayout();
			this.PRLCONDeclarationUserControl.SuspendLayout();
			this.AmendmentsTabPage.SuspendLayout();
			this.AmendmentsTabControl.SuspendLayout();
			this.ChangeCustodyTabPage.SuspendLayout();
			this.chgtstDeclarationUserControl.SuspendLayout();
			this.ChangeDisposalEntitledTraderTabPage.SuspendLayout();
			this.chgoffDeclarationUserControl.SuspendLayout();
			this.ChangeOwnerReferenceTabPage.SuspendLayout();
			this.ConsolidationTabPage.SuspendLayout();
			this.SplitTabPage.SuspendLayout();
			this.DeclarationTabPage.SuspendLayout();
			this.CUSPRLTabPageUserControl.SuspendLayout();
			this.SumADeclarationTabControl.SuspendLayout();
			this.DeclarationMessagesTabPage.SuspendLayout();
			this.SumAMessagesUserControl.SuspendLayout();
			this.SumADeclarationTabPage.SuspendLayout();
			this.RexDeclarationTabPage.SuspendLayout();
			this.REXDISMessagesTabPage.SuspendLayout();
			this.REXDISMessagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.SumADeclarationTabPage);
			this.MainTabControl.Controls.Add(this.AmendmentsTabPage);
			this.MainTabControl.Controls.Add(this.RexDeclarationTabPage);
			this.MainTabControl.Controls.Add(this.REXDISMessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 600, true);
			this.MainTabControl.TabIndex = 1;
			this.MainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.REXDISMessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RexDeclarationTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AmendmentsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.SumADeclarationTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.MainTabPage.Controls.Add(this.TemporaryStorageUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			this.MainTabPage.Text = "SumA";
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 600, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.UseVisualStyleBackColor = true;
			// 
			// TemporaryStorageUserControl
			// 
			this.TemporaryStorageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemporaryStorageUserControl, ".");
			this.TemporaryStorageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemporaryStorageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemporaryStorageUserControl.Name = "TemporaryStorageUserControl";
			this.TemporaryStorageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			this.TemporaryStorageUserControl.TabIndex = 0;
			// 
			// CUSPCSDeclarationUserControl
			// 
			this.CUSPCSDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CUSPCSDeclarationUserControl, ".");
			this.CUSPCSDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CUSPCSDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CUSPCSDeclarationUserControl.Name = "CUSPCSDeclarationUserControl";
			this.CUSPCSDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.CUSPCSDeclarationUserControl.TabIndex = 0;
			// 
			// chgspoDeclarationUserControl
			// 
			this.chgspoDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chgspoDeclarationUserControl, ".");
			this.chgspoDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chgspoDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.chgspoDeclarationUserControl.Name = "chgspoDeclarationUserControl";
			this.chgspoDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1073, 534, true);
			this.chgspoDeclarationUserControl.TabIndex = 0;
			// 
			// rexdisDeclarationUserControl
			// 
			this.rexdisDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rexdisDeclarationUserControl, "REXDISCusTempStorageDec");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).REXDISCusTempStorageDec)));
			this.rexdisDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rexdisDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.rexdisDeclarationUserControl.Name = "rexdisDeclarationUserControl";
			this.rexdisDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1087, 567, true);
			this.rexdisDeclarationUserControl.TabIndex = 0;
			// 
			// PRLCONDeclarationUserControl
			// 
			this.PRLCONDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PRLCONDeclarationUserControl, ".");
			this.PRLCONDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PRLCONDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PRLCONDeclarationUserControl.Name = "PRLCONDeclarationUserControl";
			this.PRLCONDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1073, 534, true);
			this.PRLCONDeclarationUserControl.TabIndex = 0;
			// 
			// AmendmentsTabPage
			// 
			this.AmendmentsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2e511095-6cb7-4805-8637-a8a92dbeb22f", "Amendments");
			this.AmendmentsTabPage.Controls.Add(this.AmendmentsTabControl);
			this.AmendmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AmendmentsTabPage.Name = "AmendmentsTabPage";
			this.AmendmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AmendmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			this.AmendmentsTabPage.TabIndex = 4;
			// 
			// AmendmentsTabControl
			// 
			this.AmendmentsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AmendmentsTabControl.Controls.Add(this.ChangeCustodyTabPage);
			this.AmendmentsTabControl.Controls.Add(this.ChangeDisposalEntitledTraderTabPage);
			this.AmendmentsTabControl.Controls.Add(this.ChangeOwnerReferenceTabPage);
			this.AmendmentsTabControl.Controls.Add(this.ConsolidationTabPage);
			this.AmendmentsTabControl.Controls.Add(this.SplitTabPage);
			this.AmendmentsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmendmentsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AmendmentsTabControl.Name = "AmendmentsTabControl";
			this.AmendmentsTabControl.SelectedIndex = 0;
			this.AmendmentsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1087, 567, true);
			this.AmendmentsTabControl.TabIndex = 0;
			// 
			// ChangeCustodyTabPage
			// 
			this.ChangeCustodyTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4557fce5-20bf-4ade-9b72-0b8a0463ff4a", "Change Custody Information");
			this.ChangeCustodyTabPage.Controls.Add(this.chgtstDeclarationUserControl);
			this.ChangeCustodyTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChangeCustodyTabPage.Name = "ChangeCustodyTabPage";
			this.ChangeCustodyTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChangeCustodyTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.ChangeCustodyTabPage.TabIndex = 1;
			this.ChangeCustodyTabPage.UseVisualStyleBackColor = true;
			// 
			// chgtstDeclarationUserControl
			// 
			this.chgtstDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chgtstDeclarationUserControl, ".");
			this.chgtstDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chgtstDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.chgtstDeclarationUserControl.Name = "chgtstDeclarationUserControl";
			this.chgtstDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1073, 534, true);
			this.chgtstDeclarationUserControl.TabIndex = 0;
			// 
			// ChangeDisposalEntitledTraderTabPage
			// 
			this.ChangeDisposalEntitledTraderTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("42aa8a76-258f-48ad-ae57-c55e9abad7d5", "Change Disposal Entitled Trader");
			this.ChangeDisposalEntitledTraderTabPage.Controls.Add(this.chgoffDeclarationUserControl);
			this.ChangeDisposalEntitledTraderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChangeDisposalEntitledTraderTabPage.Name = "ChangeDisposalEntitledTraderTabPage";
			this.ChangeDisposalEntitledTraderTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChangeDisposalEntitledTraderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.ChangeDisposalEntitledTraderTabPage.TabIndex = 2;
			// 
			// chgoffDeclarationUserControl
			// 
			this.chgoffDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chgoffDeclarationUserControl, ".");
			this.chgoffDeclarationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chgoffDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.chgoffDeclarationUserControl.Name = "chgoffDeclarationUserControl";
			this.chgoffDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1073, 534, true);
			this.chgoffDeclarationUserControl.TabIndex = 0;
			// 
			// ChangeOwnerReferenceTabPage
			// 
			this.ChangeOwnerReferenceTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("a8835815-bc05-4b9e-be7b-0038b9322a29", "Change Owner Reference");
			this.ChangeOwnerReferenceTabPage.Controls.Add(this.chgspoDeclarationUserControl);
			this.ChangeOwnerReferenceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChangeOwnerReferenceTabPage.Name = "ChangeOwnerReferenceTabPage";
			this.ChangeOwnerReferenceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChangeOwnerReferenceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.ChangeOwnerReferenceTabPage.TabIndex = 2;
			this.ChangeOwnerReferenceTabPage.UseVisualStyleBackColor = true;
			// 
			// ConsolidationTabPage
			// 
			this.ConsolidationTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8f527005-1bfe-4d58-8d81-a820ef4c8096", "Consolidation");
			this.ConsolidationTabPage.Controls.Add(this.PRLCONDeclarationUserControl);
			this.ConsolidationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsolidationTabPage.Name = "ConsolidationTabPage";
			this.ConsolidationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConsolidationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.ConsolidationTabPage.TabIndex = 0;
			this.ConsolidationTabPage.UseVisualStyleBackColor = true;
			// 
			// SplitTabPage
			// 
			this.SplitTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("fd652146-e37e-4ae8-8f41-7966618de78e", "Split");
			this.SplitTabPage.Controls.Add(this.CUSPCSDeclarationUserControl);
			this.SplitTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SplitTabPage.Name = "SplitTabPage";
			this.SplitTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.SplitTabPage.TabIndex = 1;
			// 
			// DeclarationTabPage
			// 
			this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c47a8ab8-2076-4db6-93bf-f1e7d9edccb4", "Declaration");
			this.DeclarationTabPage.Controls.Add(this.CUSPRLTabPageUserControl);
			this.DeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationTabPage.Name = "DeclarationTabPage";
			this.DeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 546, true);
			this.DeclarationTabPage.TabIndex = 0;
			this.DeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// CUSPRLTabPageUserControl
			// 
			this.CUSPRLTabPageUserControl.AllowDrop = true;
			this.CUSPRLTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CUSPRLTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CUSPRLTabPageUserControl.Name = "CUSPRLTabPageUserControl";
			this.CUSPRLTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 540, true);
			this.CUSPRLTabPageUserControl.TabIndex = 0;
			// 
			// SumADeclarationTabControl
			// 
			this.SumADeclarationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SumADeclarationTabControl.Controls.Add(this.DeclarationTabPage);
			this.SumADeclarationTabControl.Controls.Add(this.DeclarationMessagesTabPage);
			this.SumADeclarationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SumADeclarationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SumADeclarationTabControl.Name = "SumADeclarationTabControl";
			this.SumADeclarationTabControl.SelectedIndex = 0;
			this.SumADeclarationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			this.SumADeclarationTabControl.TabIndex = 0;
			this.SumADeclarationTabControl.SelectedIndexChanged += new System.EventHandler(this.DeclarationTabControl_SelectedIndexChanged);
			// 
			// DeclarationMessagesTabPage
			// 
			this.DeclarationMessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("13b50ff4-0a20-48a3-85c7-cab1fa4a36d0", "Messages");
			this.DeclarationMessagesTabPage.Controls.Add(this.SumAMessagesUserControl);
			this.DeclarationMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationMessagesTabPage.Name = "DeclarationMessagesTabPage";
			this.DeclarationMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 546, true);
			this.DeclarationMessagesTabPage.TabIndex = 1;
			// 
			// SumAMessagesUserControl
			// 
			this.SumAMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SumAMessagesUserControl, "CUSPRLCusTempStorageDec+Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPRLCusTempStorageDec.Messages)));
			this.SumAMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SumAMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SumAMessagesUserControl.Name = "SumAMessagesUserControl";
			this.SumAMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 546, true);
			this.SumAMessagesUserControl.TabIndex = 0;
			// 
			// SumADeclarationTabPage
			// 
			this.SumADeclarationTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2e89ee95-e083-4300-af9b-ff87b14d136d", "SumA Declaration");
			this.SumADeclarationTabPage.Controls.Add(this.SumADeclarationTabControl);
			this.SumADeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SumADeclarationTabPage.Name = "SumADeclarationTabPage";
			this.SumADeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 573, true);
			this.SumADeclarationTabPage.TabIndex = 5;
			// 
			// RexDeclarationTabPage
			// 
			this.RexDeclarationTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("08ceea74-50aa-4f94-a613-9ff6f44e40c4", "Declaration");
			this.RexDeclarationTabPage.Controls.Add(this.rexdisDeclarationUserControl);
			this.RexDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RexDeclarationTabPage.Name = "RexDeclarationTabPage";
			this.RexDeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RexDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.RexDeclarationTabPage.TabIndex = 0;
			this.RexDeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// REXDISMessagesTabPage
			// 
			this.REXDISMessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("149e1f04-5536-4659-be6f-659229f2e81a", "Messages");
			this.REXDISMessagesTabPage.Controls.Add(this.REXDISMessagesUserControl);
			this.REXDISMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.REXDISMessagesTabPage.Name = "REXDISMessagesTabPage";
			this.REXDISMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.REXDISMessagesTabPage.TabIndex = 6;
			// 
			// REXDISMessagesUserControl
			// 
			this.REXDISMessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXDISMessagesUserControl, "REXDISCusTempStorageDec.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).REXDISCusTempStorageDec.Messages)));
			this.REXDISMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REXDISMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.REXDISMessagesUserControl.Name = "REXDISMessagesUserControl";
			this.REXDISMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.REXDISMessagesUserControl.TabIndex = 1;
			// 
			// TemporaryStorageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 656, true);
			this.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1115, 695, true);
			this.Name = "TemporaryStorageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "SumAForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.TemporaryStorageUserControl.ResumeLayout(true);
			this.TemporaryStorageUserControl.PerformLayout();
			this.CUSPCSDeclarationUserControl.ResumeLayout(true);
			this.CUSPCSDeclarationUserControl.PerformLayout();
			this.chgspoDeclarationUserControl.ResumeLayout(true);
			this.chgspoDeclarationUserControl.PerformLayout();
			this.rexdisDeclarationUserControl.ResumeLayout(true);
			this.rexdisDeclarationUserControl.PerformLayout();
			this.PRLCONDeclarationUserControl.ResumeLayout(true);
			this.PRLCONDeclarationUserControl.PerformLayout();
			this.AmendmentsTabPage.ResumeLayout(false);
			this.AmendmentsTabPage.PerformLayout();
			this.AmendmentsTabControl.ResumeLayout(false);
			this.AmendmentsTabControl.PerformLayout();
			this.ChangeCustodyTabPage.ResumeLayout(false);
			this.ChangeCustodyTabPage.PerformLayout();
			this.chgtstDeclarationUserControl.ResumeLayout(true);
			this.chgtstDeclarationUserControl.PerformLayout();
			this.ChangeDisposalEntitledTraderTabPage.ResumeLayout(false);
			this.ChangeDisposalEntitledTraderTabPage.PerformLayout();
			this.chgoffDeclarationUserControl.ResumeLayout(true);
			this.chgoffDeclarationUserControl.PerformLayout();
			this.ChangeOwnerReferenceTabPage.ResumeLayout(false);
			this.ChangeOwnerReferenceTabPage.PerformLayout();
			this.ConsolidationTabPage.ResumeLayout(false);
			this.ConsolidationTabPage.PerformLayout();
			this.SplitTabPage.ResumeLayout(false);
			this.SplitTabPage.PerformLayout();
			this.DeclarationTabPage.ResumeLayout(false);
			this.DeclarationTabPage.PerformLayout();
			this.CUSPRLTabPageUserControl.ResumeLayout(true);
			this.CUSPRLTabPageUserControl.PerformLayout();
			this.SumADeclarationTabControl.ResumeLayout(false);
			this.SumADeclarationTabControl.PerformLayout();
			this.DeclarationMessagesTabPage.ResumeLayout(false);
			this.DeclarationMessagesTabPage.PerformLayout();
			this.SumAMessagesUserControl.ResumeLayout(true);
			this.SumAMessagesUserControl.PerformLayout();
			this.SumADeclarationTabPage.ResumeLayout(false);
			this.SumADeclarationTabPage.PerformLayout();
			this.RexDeclarationTabPage.ResumeLayout(false);
			this.RexDeclarationTabPage.PerformLayout();
			this.REXDISMessagesTabPage.ResumeLayout(false);
			this.REXDISMessagesTabPage.PerformLayout();
			this.REXDISMessagesUserControl.ResumeLayout(true);
			this.REXDISMessagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private TemporaryStorageUserControl TemporaryStorageUserControl;
		private ZArchitecture.GUI.ZTabPage AmendmentsTabPage;
		private ZArchitecture.GUI.ZTabPage SumADeclarationTabPage;
		private ZArchitecture.GUI.ZTabControl AmendmentsTabControl;
		private ZArchitecture.GUI.ZTabControl SumADeclarationTabControl;
		private ZArchitecture.GUI.ZTabPage DeclarationTabPage;
		private CUSPRLTabPageUserControl CUSPRLTabPageUserControl;
		private CHGTSTDeclarationUserControl chgtstDeclarationUserControl;
		private ZArchitecture.GUI.ZTabPage ChangeCustodyTabPage;
		private ZArchitecture.GUI.ZTabPage SplitTabPage;
		private CUSPCSDeclarationUserControl CUSPCSDeclarationUserControl;
		private ZArchitecture.GUI.ZTabPage ChangeOwnerReferenceTabPage;
		private CHGSPODeclarationUserControl chgspoDeclarationUserControl;
		private ZArchitecture.GUI.ZTabPage ChangeDisposalEntitledTraderTabPage;
		private CHGOFFDeclarationUserControl chgoffDeclarationUserControl;
		private ZArchitecture.GUI.ZTabPage RexDeclarationTabPage;
		private REXDISDeclarationUserControl rexdisDeclarationUserControl;
		private ZArchitecture.GUI.ZTabPage ConsolidationTabPage;
		private PRLCONDeclarationUserControl PRLCONDeclarationUserControl;
		private ZArchitecture.GUI.ZTabPage DeclarationMessagesTabPage;
		private SumAMessagesUserControl SumAMessagesUserControl;
		private ZArchitecture.GUI.ZTabPage REXDISMessagesTabPage;
		private REXDISMessagesUserControl REXDISMessagesUserControl;
	}
}
