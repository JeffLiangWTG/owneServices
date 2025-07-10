namespace CargoWise.Bi.Product.Manager.GUI
{
	partial class InformationControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.informationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.powerBiServerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.powerBiReportsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.powerBiReportsVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.powerBiReportsUrlTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.powerBiServerVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.analysisServerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.analysisServerVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.analysisServerModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.analysisServerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.edwDatabaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.edwDbLogSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.edwDbSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.edwDbVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.edwDbServerVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.edwDbNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.edwDbServerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.auditDatabaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.auditDbLogSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.auditDbSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.auditDbVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.auditDbServerVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.auditDbNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.auditDbServerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mainServerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainDbLogSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mainDbSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mainDbVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mainServerVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mainDbNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mainServerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.copyInfoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.informationPanel.SuspendLayout();
			this.powerBiServerGroupBox.SuspendLayout();
			this.analysisServerGroupBox.SuspendLayout();
			this.edwDatabaseGroupBox.SuspendLayout();
			this.auditDatabaseGroupBox.SuspendLayout();
			this.mainServerGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject);
			// 
			// informationPanel
			// 
			this.informationPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.informationPanel.AutoScroll = true;
			this.informationPanel.Controls.Add(this.powerBiServerGroupBox);
			this.informationPanel.Controls.Add(this.analysisServerGroupBox);
			this.informationPanel.Controls.Add(this.edwDatabaseGroupBox);
			this.informationPanel.Controls.Add(this.auditDatabaseGroupBox);
			this.informationPanel.Controls.Add(this.mainServerGroupBox);
			this.informationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.informationPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.informationPanel.Name = "informationPanel";
			this.informationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 740, true);
			this.informationPanel.TabIndex = 20;
			// 
			// powerBiServerGroupBox
			// 
			this.powerBiServerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.powerBiServerGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.powerBiServerGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("feed59b0-3586-4341-9858-dc66fe060C68", "Power BI Server Information");
			this.powerBiServerGroupBox.Controls.Add(this.powerBiReportsStatusTextBox);
			this.powerBiServerGroupBox.Controls.Add(this.powerBiReportsVersionTextBox);
			this.powerBiServerGroupBox.Controls.Add(this.powerBiReportsUrlTextBox);
			this.powerBiServerGroupBox.Controls.Add(this.powerBiServerVersionTextBox);
			this.powerBiServerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 514, true);
			this.powerBiServerGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.powerBiServerGroupBox.Name = "powerBiServerGroupBox";
			this.powerBiServerGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.powerBiServerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 90, true);
			this.powerBiServerGroupBox.TabIndex = 14;
			this.powerBiServerGroupBox.TabStop = false;
			// 
			// powerBiReportsStatusTextBox
			// 
			this.powerBiReportsStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.powerBiReportsStatusTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.powerBiReportsStatusTextBox, "PowerBiReportsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).PowerBiReportsStatus)));
			this.powerBiReportsStatusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.powerBiReportsStatusTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("fe03a977-36be-40d0-bb3f-bc352283c134", "Power BI Reports Status");
			this.powerBiReportsStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.powerBiReportsStatusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.powerBiReportsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 71, true);
			this.powerBiReportsStatusTextBox.Name = "powerBiReportsStatusTextBox";
			this.powerBiReportsStatusTextBox.ReadOnly = true;
			this.powerBiReportsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.powerBiReportsStatusTextBox.TabIndex = 18;
			// 
			// powerBiReportsVersionTextBox
			// 
			this.powerBiReportsVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.powerBiReportsVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.powerBiReportsVersionTextBox, "PowerBiReportsVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).PowerBiReportsVersion)));
			this.powerBiReportsVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.powerBiReportsVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("0e4ca6ef-6462-46a2-afa9-0013c25ae5c8", "Power BI Reports Version");
			this.powerBiReportsVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.powerBiReportsVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.powerBiReportsVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 54, true);
			this.powerBiReportsVersionTextBox.Name = "powerBiReportsVersionTextBox";
			this.powerBiReportsVersionTextBox.ReadOnly = true;
			this.powerBiReportsVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.powerBiReportsVersionTextBox.TabIndex = 17;
			// 
			// powerBiReportsUrlTextBox
			// 
			this.powerBiReportsUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.powerBiReportsUrlTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.powerBiReportsUrlTextBox, "PowerBiWebPortalUrl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).PowerBiWebPortalUrl)));
			this.powerBiReportsUrlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.powerBiReportsUrlTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("c0fab6c6-265c-48ec-96c9-47b8e0843f12", "Power BI Web Portal URL");
			this.powerBiReportsUrlTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.powerBiReportsUrlTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.powerBiReportsUrlTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.powerBiReportsUrlTextBox.Name = "powerBiReportsUrlTextBox";
			this.powerBiReportsUrlTextBox.ReadOnly = true;
			this.powerBiReportsUrlTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.powerBiReportsUrlTextBox.TabIndex = 15;
			// 
			// powerBiServerVersionTextBox
			// 
			this.powerBiServerVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.powerBiServerVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.powerBiServerVersionTextBox, "PowerBiServerVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).PowerBiServerVersion)));
			this.powerBiServerVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.powerBiServerVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("d353815d-5d82-4237-9354-3dae66cb7F17", "Power BI Server Version");
			this.powerBiServerVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.powerBiServerVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.powerBiServerVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 36, true);
			this.powerBiServerVersionTextBox.Name = "powerBiServerVersionTextBox";
			this.powerBiServerVersionTextBox.ReadOnly = true;
			this.powerBiServerVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.powerBiServerVersionTextBox.TabIndex = 16;
			// 
			// analysisServerGroupBox
			// 
			this.analysisServerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.analysisServerGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.analysisServerGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("7f68f50a-0266-4df7-9602-abebb836bb29", "Analysis Server Information");
			this.analysisServerGroupBox.Controls.Add(this.analysisServerVersionTextBox);
			this.analysisServerGroupBox.Controls.Add(this.analysisServerModeTextBox);
			this.analysisServerGroupBox.Controls.Add(this.analysisServerNameTextBox);
			this.analysisServerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 422, true);
			this.analysisServerGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.analysisServerGroupBox.Name = "analysisServerGroupBox";
			this.analysisServerGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.analysisServerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 73, true);
			this.analysisServerGroupBox.TabIndex = 5;
			this.analysisServerGroupBox.TabStop = false;
			// 
			// analysisServerVersionTextBox
			// 
			this.analysisServerVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.analysisServerVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.analysisServerVersionTextBox, "AnalysisServerVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).AnalysisServerVersion)));
			this.analysisServerVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.analysisServerVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("5a8e3d0f-9871-4594-9cca-214b68048bff", "Analysis Server Version");
			this.analysisServerVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.analysisServerVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.analysisServerVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 35, true);
			this.analysisServerVersionTextBox.Name = "analysisServerVersionTextBox";
			this.analysisServerVersionTextBox.ReadOnly = true;
			this.analysisServerVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.analysisServerVersionTextBox.TabIndex = 1;
			// 
			// analysisServerModeTextBox
			// 
			this.analysisServerModeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.analysisServerModeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.analysisServerModeTextBox, "AnalysisServerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).AnalysisServerMode)));
			this.analysisServerModeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.analysisServerModeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("b7ed9dfb-387f-4d5c-a6d6-48551a52dd20", "Server Mode");
			this.analysisServerModeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.analysisServerModeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.analysisServerModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 53, true);
			this.analysisServerModeTextBox.Name = "analysisServerModeTextBox";
			this.analysisServerModeTextBox.ReadOnly = true;
			this.analysisServerModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.analysisServerModeTextBox.TabIndex = 2;
			// 
			// analysisServerNameTextBox
			// 
			this.analysisServerNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.analysisServerNameTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.analysisServerNameTextBox, "AnalysisServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).AnalysisServerName)));
			this.analysisServerNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.analysisServerNameTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("a4aadbca-9ea0-484d-bebc-b99e465a20ea", "Analysis Server Name");
			this.analysisServerNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.analysisServerNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.analysisServerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.analysisServerNameTextBox.Name = "analysisServerNameTextBox";
			this.analysisServerNameTextBox.ReadOnly = true;
			this.analysisServerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.analysisServerNameTextBox.TabIndex = 0;
			// 
			// edwDatabaseGroupBox
			// 
			this.edwDatabaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDatabaseGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.edwDatabaseGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("3bf46b63-3bef-4076-b82c-069cd52e52f6", "EDW Database Information");
			this.edwDatabaseGroupBox.Controls.Add(this.edwDbLogSizeTextBox);
			this.edwDatabaseGroupBox.Controls.Add(this.edwDbSizeTextBox);
			this.edwDatabaseGroupBox.Controls.Add(this.edwDbVersionTextBox);
			this.edwDatabaseGroupBox.Controls.Add(this.edwDbServerVersionTextBox);
			this.edwDatabaseGroupBox.Controls.Add(this.edwDbNameTextBox);
			this.edwDatabaseGroupBox.Controls.Add(this.edwDbServerTextBox);
			this.edwDatabaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 279, true);
			this.edwDatabaseGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.edwDatabaseGroupBox.Name = "edwDatabaseGroupBox";
			this.edwDatabaseGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.edwDatabaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 125, true);
			this.edwDatabaseGroupBox.TabIndex = 4;
			this.edwDatabaseGroupBox.TabStop = false;
			// 
			// edwDbLogSizeTextBox
			// 
			this.edwDbLogSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDbLogSizeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.edwDbLogSizeTextBox, "BiInformation.EdwDbInfo.DatabaseLogSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.EdwDbInfo.DatabaseLogSize)));
			this.edwDbLogSizeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.edwDbLogSizeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("ce40ad37-5722-4f4f-844e-0e5485d693ec", "Database Log Size");
			this.edwDbLogSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.edwDbLogSizeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.edwDbLogSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 106, true);
			this.edwDbLogSizeTextBox.Name = "edwDbLogSizeTextBox";
			this.edwDbLogSizeTextBox.ReadOnly = true;
			this.edwDbLogSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.edwDbLogSizeTextBox.TabIndex = 5;
			// 
			// edwDbSizeTextBox
			// 
			this.edwDbSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDbSizeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.edwDbSizeTextBox, "BiInformation.EdwDbInfo.DatabaseSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.EdwDbInfo.DatabaseSize)));
			this.edwDbSizeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.edwDbSizeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("1286747d-9d98-4fb4-adca-61f7685bfead", "Database Size");
			this.edwDbSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.edwDbSizeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.edwDbSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 88, true);
			this.edwDbSizeTextBox.Name = "edwDbSizeTextBox";
			this.edwDbSizeTextBox.ReadOnly = true;
			this.edwDbSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.edwDbSizeTextBox.TabIndex = 4;
			// 
			// edwDbVersionTextBox
			// 
			this.edwDbVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDbVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.edwDbVersionTextBox, "BiInformation.EdwDbInfo.DatabaseVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.EdwDbInfo.DatabaseVersion)));
			this.edwDbVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.edwDbVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("20d02bbb-e400-4afa-b9d4-28f154272352", "Database Version");
			this.edwDbVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.edwDbVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.edwDbVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 70, true);
			this.edwDbVersionTextBox.Name = "edwDbVersionTextBox";
			this.edwDbVersionTextBox.ReadOnly = true;
			this.edwDbVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.edwDbVersionTextBox.TabIndex = 3;
			// 
			// edwDbServerVersionTextBox
			// 
			this.edwDbServerVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDbServerVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.edwDbServerVersionTextBox, "BiInformation.EdwDbInfo.ServerVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.EdwDbInfo.ServerVersion)));
			this.edwDbServerVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.edwDbServerVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("c806df9b-1030-4647-829e-fc32adc93ed5", "SQL Server Version");
			this.edwDbServerVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.edwDbServerVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.edwDbServerVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 35, true);
			this.edwDbServerVersionTextBox.Name = "edwDbServerVersionTextBox";
			this.edwDbServerVersionTextBox.ReadOnly = true;
			this.edwDbServerVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.edwDbServerVersionTextBox.TabIndex = 1;
			// 
			// edwDbNameTextBox
			// 
			this.edwDbNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDbNameTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.edwDbNameTextBox, "BiInformation.EdwDbInfo.DatabaseName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.EdwDbInfo.DatabaseName)));
			this.edwDbNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.edwDbNameTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("24720146-7e2e-45c7-962d-d6f33674afed", "Database Name");
			this.edwDbNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.edwDbNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.edwDbNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 53, true);
			this.edwDbNameTextBox.Name = "edwDbNameTextBox";
			this.edwDbNameTextBox.ReadOnly = true;
			this.edwDbNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.edwDbNameTextBox.TabIndex = 2;
			// 
			// edwDbServerTextBox
			// 
			this.edwDbServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.edwDbServerTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.edwDbServerTextBox, "BiInformation.EdwDbInfo.ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.EdwDbInfo.ServerName)));
			this.edwDbServerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.edwDbServerTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("4593b444-8316-4078-98b9-b10d126a7520", "Database Server");
			this.edwDbServerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.edwDbServerTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.edwDbServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.edwDbServerTextBox.Name = "edwDbServerTextBox";
			this.edwDbServerTextBox.ReadOnly = true;
			this.edwDbServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.edwDbServerTextBox.TabIndex = 0;
			// 
			// auditDatabaseGroupBox
			// 
			this.auditDatabaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDatabaseGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.auditDatabaseGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("f1cbfa38-3b52-446e-a542-45c41a47c0d3", "Audit Database Information");
			this.auditDatabaseGroupBox.Controls.Add(this.auditDbLogSizeTextBox);
			this.auditDatabaseGroupBox.Controls.Add(this.auditDbSizeTextBox);
			this.auditDatabaseGroupBox.Controls.Add(this.auditDbVersionTextBox);
			this.auditDatabaseGroupBox.Controls.Add(this.auditDbServerVersionTextBox);
			this.auditDatabaseGroupBox.Controls.Add(this.auditDbNameTextBox);
			this.auditDatabaseGroupBox.Controls.Add(this.auditDbServerTextBox);
			this.auditDatabaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.auditDatabaseGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.auditDatabaseGroupBox.Name = "auditDatabaseGroupBox";
			this.auditDatabaseGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.auditDatabaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 126, true);
			this.auditDatabaseGroupBox.TabIndex = 3;
			this.auditDatabaseGroupBox.TabStop = false;
			// 
			// auditDbLogSizeTextBox
			// 
			this.auditDbLogSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDbLogSizeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.auditDbLogSizeTextBox, "BiInformation.AuditDbInfo.DatabaseLogSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.AuditDbInfo.DatabaseLogSize)));
			this.auditDbLogSizeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.auditDbLogSizeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("086c4fbe-e871-4646-a231-6a062be6fd81", "Database Log Size");
			this.auditDbLogSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.auditDbLogSizeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.auditDbLogSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 106, true);
			this.auditDbLogSizeTextBox.Name = "auditDbLogSizeTextBox";
			this.auditDbLogSizeTextBox.ReadOnly = true;
			this.auditDbLogSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.auditDbLogSizeTextBox.TabIndex = 5;
			// 
			// auditDbSizeTextBox
			// 
			this.auditDbSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDbSizeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.auditDbSizeTextBox, "BiInformation.AuditDbInfo.DatabaseSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.AuditDbInfo.DatabaseSize)));
			this.auditDbSizeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.auditDbSizeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("7314a6b7-250e-41f2-97ed-7ce2678ed93b", "Database Size");
			this.auditDbSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.auditDbSizeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.auditDbSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 88, true);
			this.auditDbSizeTextBox.Name = "auditDbSizeTextBox";
			this.auditDbSizeTextBox.ReadOnly = true;
			this.auditDbSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.auditDbSizeTextBox.TabIndex = 4;
			// 
			// auditDbVersionTextBox
			// 
			this.auditDbVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDbVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.auditDbVersionTextBox, "BiInformation.AuditDbInfo.DatabaseVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.AuditDbInfo.DatabaseVersion)));
			this.auditDbVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.auditDbVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("50220f2c-ba6f-48c7-ae2a-2c1170ee19d9", "Database Version");
			this.auditDbVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.auditDbVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.auditDbVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 70, true);
			this.auditDbVersionTextBox.Name = "auditDbVersionTextBox";
			this.auditDbVersionTextBox.ReadOnly = true;
			this.auditDbVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.auditDbVersionTextBox.TabIndex = 3;
			// 
			// auditDbServerVersionTextBox
			// 
			this.auditDbServerVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDbServerVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.auditDbServerVersionTextBox, "BiInformation.AuditDbInfo.ServerVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.AuditDbInfo.ServerVersion)));
			this.auditDbServerVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.auditDbServerVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("fe28cac0-935c-41de-ba4c-5810cca4e665", "SQL Server Version");
			this.auditDbServerVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.auditDbServerVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.auditDbServerVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 35, true);
			this.auditDbServerVersionTextBox.Name = "auditDbServerVersionTextBox";
			this.auditDbServerVersionTextBox.ReadOnly = true;
			this.auditDbServerVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.auditDbServerVersionTextBox.TabIndex = 1;
			// 
			// auditDbNameTextBox
			// 
			this.auditDbNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDbNameTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.auditDbNameTextBox, "BiInformation.AuditDbInfo.DatabaseName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.AuditDbInfo.DatabaseName)));
			this.auditDbNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.auditDbNameTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("a631e415-0bd7-43c8-991c-8132ce5d681b", "Database Name");
			this.auditDbNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.auditDbNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.auditDbNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 53, true);
			this.auditDbNameTextBox.Name = "auditDbNameTextBox";
			this.auditDbNameTextBox.ReadOnly = true;
			this.auditDbNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.auditDbNameTextBox.TabIndex = 2;
			// 
			// auditDbServerTextBox
			// 
			this.auditDbServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.auditDbServerTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.auditDbServerTextBox, "BiInformation.AuditDbInfo.ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.AuditDbInfo.ServerName)));
			this.auditDbServerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.auditDbServerTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("d6751bae-dfb8-4e68-ae3b-cfe86ca1a4af", "Database Server");
			this.auditDbServerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.auditDbServerTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.auditDbServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.auditDbServerTextBox.Name = "auditDbServerTextBox";
			this.auditDbServerTextBox.ReadOnly = true;
			this.auditDbServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.auditDbServerTextBox.TabIndex = 0;
			// 
			// mainServerGroupBox
			// 
			this.mainServerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainServerGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.mainServerGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("beef8153-7f57-447d-9a08-2dc5e7894530", "Main Server Information");
			this.mainServerGroupBox.Controls.Add(this.mainDbLogSizeTextBox);
			this.mainServerGroupBox.Controls.Add(this.mainDbSizeTextBox);
			this.mainServerGroupBox.Controls.Add(this.mainDbVersionTextBox);
			this.mainServerGroupBox.Controls.Add(this.mainServerVersionTextBox);
			this.mainServerGroupBox.Controls.Add(this.mainDbNameTextBox);
			this.mainServerGroupBox.Controls.Add(this.mainServerTextBox);
			this.mainServerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainServerGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.mainServerGroupBox.Name = "mainServerGroupBox";
			this.mainServerGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.mainServerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 126, true);
			this.mainServerGroupBox.TabIndex = 2;
			this.mainServerGroupBox.TabStop = false;
			// 
			// mainDbLogSizeTextBox
			// 
			this.mainDbLogSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainDbLogSizeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.mainDbLogSizeTextBox, "BiInformation.MainDbInfo.DatabaseLogSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.MainDbInfo.DatabaseLogSize)));
			this.mainDbLogSizeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainDbLogSizeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("6bb927c0-77e6-4f09-af62-f877f5a057f9", "Database Log Size");
			this.mainDbLogSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.mainDbLogSizeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.mainDbLogSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 106, true);
			this.mainDbLogSizeTextBox.Name = "mainDbLogSizeTextBox";
			this.mainDbLogSizeTextBox.ReadOnly = true;
			this.mainDbLogSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.mainDbLogSizeTextBox.TabIndex = 5;
			// 
			// mainDbSizeTextBox
			// 
			this.mainDbSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainDbSizeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.mainDbSizeTextBox, "BiInformation.MainDbInfo.DatabaseSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.MainDbInfo.DatabaseSize)));
			this.mainDbSizeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainDbSizeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("7616d46e-0e64-4a0c-a53b-490bac6591da", "Database Size");
			this.mainDbSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.mainDbSizeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.mainDbSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 88, true);
			this.mainDbSizeTextBox.Name = "mainDbSizeTextBox";
			this.mainDbSizeTextBox.ReadOnly = true;
			this.mainDbSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.mainDbSizeTextBox.TabIndex = 4;
			// 
			// mainDbVersionTextBox
			// 
			this.mainDbVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainDbVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.mainDbVersionTextBox, "BiInformation.MainDbInfo.DatabaseVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.MainDbInfo.DatabaseVersion)));
			this.mainDbVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainDbVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("5f5c1294-7314-4868-990b-bc66d9f8a24f", "Database Version");
			this.mainDbVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.mainDbVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.mainDbVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 70, true);
			this.mainDbVersionTextBox.Name = "mainDbVersionTextBox";
			this.mainDbVersionTextBox.ReadOnly = true;
			this.mainDbVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.mainDbVersionTextBox.TabIndex = 3;
			// 
			// mainServerVersionTextBox
			// 
			this.mainServerVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainServerVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.mainServerVersionTextBox, "BiInformation.MainDbInfo.ServerVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.MainDbInfo.ServerVersion)));
			this.mainServerVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainServerVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("b4e05048-6834-4897-bfee-7436db922d38", "SQL Server Version");
			this.mainServerVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.mainServerVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.mainServerVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 35, true);
			this.mainServerVersionTextBox.Name = "mainServerVersionTextBox";
			this.mainServerVersionTextBox.ReadOnly = true;
			this.mainServerVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.mainServerVersionTextBox.TabIndex = 1;
			// 
			// mainDbNameTextBox
			// 
			this.mainDbNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainDbNameTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.mainDbNameTextBox, "BiInformation.MainDbInfo.DatabaseName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.MainDbInfo.DatabaseName)));
			this.mainDbNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainDbNameTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("ddd2c121-1b7b-4d99-ade0-fa109f23cbcf", "Database Name");
			this.mainDbNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.mainDbNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.mainDbNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 52, true);
			this.mainDbNameTextBox.Name = "mainDbNameTextBox";
			this.mainDbNameTextBox.ReadOnly = true;
			this.mainDbNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.mainDbNameTextBox.TabIndex = 2;
			// 
			// mainServerTextBox
			// 
			this.mainServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.mainServerTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.mainServerTextBox, "BiInformation.MainDbInfo.ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.BiMonitorBusinessObject)(null)).BiInformation.MainDbInfo.ServerName)));
			this.mainServerTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mainServerTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("609d2814-b656-413a-a0e0-fcfede9984bc", "Database Server");
			this.mainServerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.mainServerTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.mainServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.mainServerTextBox.Name = "mainServerTextBox";
			this.mainServerTextBox.ReadOnly = true;
			this.mainServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 13, true);
			this.mainServerTextBox.TabIndex = 0;
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("bb8f93ac-1056-4ca9-b385-21742638ffb8", "Refresh");
			this.refreshButton.IsCaptionOverridden = false;
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 752, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.refreshButton.TabIndex = 12;
			this.refreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.refreshButton.ToolTipCaption = null;
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// copyInfoButton
			// 
			this.copyInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyInfoButton.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("3b2bddaf-b4d4-4c31-a78e-7486dd67338b", "Copy Info");
			this.copyInfoButton.IsCaptionOverridden = false;
			this.copyInfoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(561, 752, true);
			this.copyInfoButton.Name = "copyInfoButton";
			this.copyInfoButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.copyInfoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.copyInfoButton.TabIndex = 13;
			this.copyInfoButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.copyInfoButton.ToolTipCaption = null;
			this.copyInfoButton.UseVisualStyleBackColor = true;
			this.copyInfoButton.Click += new System.EventHandler(this.copyInfoButton_Click);
			// 
			// InformationControl
			// 
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.copyInfoButton);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.informationPanel);
			this.Name = "InformationControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 786, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.informationPanel.ResumeLayout(false);
			this.informationPanel.PerformLayout();
			this.powerBiServerGroupBox.ResumeLayout(false);
			this.powerBiServerGroupBox.PerformLayout();
			this.analysisServerGroupBox.ResumeLayout(false);
			this.analysisServerGroupBox.PerformLayout();
			this.edwDatabaseGroupBox.ResumeLayout(false);
			this.edwDatabaseGroupBox.PerformLayout();
			this.auditDatabaseGroupBox.ResumeLayout(false);
			this.auditDatabaseGroupBox.PerformLayout();
			this.mainServerGroupBox.ResumeLayout(false);
			this.mainServerGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel informationPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox mainServerGroupBox;
		private Enterprise.ZArchitecture.ZTextBox mainDbNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox mainServerTextBox;
		private Enterprise.ZArchitecture.ZTextBox mainServerVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox mainDbVersionTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox auditDatabaseGroupBox;
		private Enterprise.ZArchitecture.ZTextBox auditDbVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox auditDbServerVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox auditDbNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox auditDbServerTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox edwDatabaseGroupBox;
		private Enterprise.ZArchitecture.ZTextBox edwDbVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox edwDbServerVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox edwDbNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox edwDbServerTextBox;
		private Enterprise.ZArchitecture.ZTextBox mainDbSizeTextBox;
		private Enterprise.ZArchitecture.ZTextBox auditDbSizeTextBox;
		private Enterprise.ZArchitecture.ZTextBox edwDbSizeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox analysisServerGroupBox;
		private Enterprise.ZArchitecture.ZTextBox analysisServerVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox analysisServerModeTextBox;
		private Enterprise.ZArchitecture.ZTextBox analysisServerNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton refreshButton;
		private Enterprise.ZArchitecture.GUI.ZButton copyInfoButton;
		private Enterprise.ZArchitecture.ZTextBox mainDbLogSizeTextBox;
		private Enterprise.ZArchitecture.ZTextBox auditDbLogSizeTextBox;
		private Enterprise.ZArchitecture.ZTextBox edwDbLogSizeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox powerBiServerGroupBox;
		private Enterprise.ZArchitecture.ZTextBox powerBiServerVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox powerBiReportsUrlTextBox;
		private Enterprise.ZArchitecture.ZTextBox powerBiReportsVersionTextBox;
		private Enterprise.ZArchitecture.ZTextBox powerBiReportsStatusTextBox;
	}
}
