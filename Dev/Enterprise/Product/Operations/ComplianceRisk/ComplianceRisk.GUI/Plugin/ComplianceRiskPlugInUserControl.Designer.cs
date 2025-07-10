namespace Enterprise.ComplianceRisk.GUI
{
	partial class ComplianceRiskPlugInUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ComplianceRiskGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComplianceRiskTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OverallRiskPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OverallRiskPanelLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverallRiskLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverallRiskRegistryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SubSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PartyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartyHideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PartyTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.PartyRiskPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PartyRiskPanelLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PartyRiskLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LocationHideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LocationTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.LocationRiskPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LocationRiskPanelLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LocationRiskLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommodityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityHideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommodityTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.CommodityRiskPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommodityRiskLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommodityRiskPanelLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssessmentInitializeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CommoditySpinnerIndicator = new Enterprise.ComplianceRisk.GUI.ComplianceRiskSpinnerIndicator();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ComplianceRiskGroupBox.SuspendLayout();
			this.ComplianceRiskTableLayoutPanel.SuspendLayout();
			this.OverallRiskPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubSplitContainer)).BeginInit();
			this.SubSplitContainer.Panel1.SuspendLayout();
			this.SubSplitContainer.Panel2.SuspendLayout();
			this.SubSplitContainer.SuspendLayout();
			this.PartyGroupBox.SuspendLayout();
			this.PartyTableLayoutPanel.SuspendLayout();
			this.PartyRiskPanel.SuspendLayout();
			this.LocationGroupBox.SuspendLayout();
			this.LocationTableLayoutPanel.SuspendLayout();
			this.LocationRiskPanel.SuspendLayout();
			this.CommodityGroupBox.SuspendLayout();
			this.CommodityTableLayoutPanel.SuspendLayout();
			this.CommodityRiskPanel.SuspendLayout();
			this.CommoditySpinnerIndicator.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject);
			// 
			// ComplianceRiskGroupBox
			// 
			this.ComplianceRiskGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("cd6e47bf-7414-42ab-8f10-f2e3ecc9d39a", "Compliance Summary");
			this.ComplianceRiskGroupBox.Controls.Add(this.ComplianceRiskTableLayoutPanel);
			this.ComplianceRiskGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceRiskGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceRiskGroupBox.Name = "ComplianceRiskGroupBox";
			this.ComplianceRiskGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 640, true);
			this.ComplianceRiskGroupBox.TabIndex = 0;
			this.ComplianceRiskGroupBox.TabStop = false;
			this.ComplianceRiskGroupBox.ClientSizeChanged += new System.EventHandler(this.HideCheckBox_CheckedChanged);
			// 
			// ComplianceRiskTableLayoutPanel
			// 
			this.ComplianceRiskTableLayoutPanel.ColumnCount = 1;
			this.ComplianceRiskTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ComplianceRiskTableLayoutPanel.Controls.Add(this.OverallRiskPanel, 0, 0);
			this.ComplianceRiskTableLayoutPanel.Controls.Add(this.SplitContainer, 0, 1);
			this.ComplianceRiskTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceRiskTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ComplianceRiskTableLayoutPanel.Name = "ComplianceRiskTableLayoutPanel";
			this.ComplianceRiskTableLayoutPanel.RowCount = 2;
			this.ComplianceRiskTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.ComplianceRiskTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ComplianceRiskTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 623, true);
			this.ComplianceRiskTableLayoutPanel.TabIndex = 0;
			// 
			// OverallRiskPanel
			// 
			this.OverallRiskPanel.Controls.Add(this.OverallRiskPanelLabel);
			this.OverallRiskPanel.Controls.Add(this.OverallRiskLabel);
			this.OverallRiskPanel.Controls.Add(this.OverallRiskRegistryLabel);
			this.OverallRiskPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OverallRiskPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OverallRiskPanel.Name = "OverallRiskPanel";
			this.OverallRiskPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 26, true);
			this.OverallRiskPanel.TabIndex = 0;
			// 
			// OverallRiskPanelLabel
			// 
			this.OverallRiskPanelLabel.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("db1e2efa-aff5-4814-95cf-5b1a174ae068", "Job Compliance Status");
			this.OverallRiskPanelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OverallRiskPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.OverallRiskPanelLabel.Name = "OverallRiskPanelLabel";
			this.OverallRiskPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 16, true);
			this.OverallRiskPanelLabel.TabIndex = 0;
			this.OverallRiskPanelLabel.UseMnemonic = false;
			// 
			// OverallRiskLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallRiskLabel, "OverallRiskDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).OverallRiskDescription)));
			this.OverallRiskLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OverallRiskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 1, true);
			this.OverallRiskLabel.Name = "OverallRiskLabel";
			this.OverallRiskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 18, true);
			this.OverallRiskLabel.TabIndex = 1;
			this.OverallRiskLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.OverallRiskLabel.UseMnemonic = false;
			this.OverallRiskLabel.TextChanged += new System.EventHandler(this.RiskStatusValueChanged);
			// 
			// OverallRiskRegistryLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallRiskRegistryLabel, "OverallRiskRegistryInfo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).OverallRiskRegistryInfo)));
			this.OverallRiskRegistryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OverallRiskRegistryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 2, true);
			this.OverallRiskRegistryLabel.Name = "OverallRiskRegistryLabel";
			this.OverallRiskRegistryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 16, true);
			this.OverallRiskRegistryLabel.TabIndex = 2;
			this.OverallRiskRegistryLabel.UseMnemonic = false;
			this.OverallRiskRegistryLabel.Visible = false;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 32, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SubSplitContainer);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 589, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(190);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.CommodityGroupBox);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(230);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(349);
			this.SplitContainer.SplitterWidth = 7;
			this.SplitContainer.TabIndex = 0;
			this.SplitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitterMoved);
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
			this.SubSplitContainer.Panel1.Controls.Add(this.PartyGroupBox);
			this.SubSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 349, true);
			this.SubSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(90);
			// 
			// SubSplitContainer.Panel2
			// 
			this.SubSplitContainer.Panel2.Controls.Add(this.LocationGroupBox);
			this.SubSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(90);
			this.SubSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(173);
			this.SubSplitContainer.SplitterWidth = 7;
			this.SubSplitContainer.TabIndex = 0;
			// 
			// PartyGroupBox
			// 
			this.PartyGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("b2736c48-2fff-4f13-ac84-0efc5feddfe8", "Parties");
			this.PartyGroupBox.Controls.Add(this.PartyHideCheckBox);
			this.PartyGroupBox.Controls.Add(this.PartyTableLayoutPanel);
			this.PartyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PartyGroupBox.Name = "PartyGroupBox";
			this.PartyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 173, true);
			this.PartyGroupBox.TabIndex = 0;
			this.PartyGroupBox.TabStop = false;
			// 
			// PartyHideCheckBox
			// 
			this.PartyHideCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PartyHideCheckBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("7120eaa1-1fc9-427b-9d73-2b501dcaa64b", "Hide");
			this.PartyHideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(934, 0, true);
			this.PartyHideCheckBox.Name = "PartyHideCheckBox";
			this.PartyHideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.PartyHideCheckBox.TabIndex = 3;
			this.PartyHideCheckBox.CheckedChanged += new System.EventHandler(this.HideCheckBox_CheckedChanged);
			// 
			// PartyTableLayoutPanel
			// 
			this.PartyTableLayoutPanel.ColumnCount = 1;
			this.PartyTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PartyTableLayoutPanel.Controls.Add(this.PartyRiskPanel, 0, 0);
			this.PartyTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartyTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.PartyTableLayoutPanel.Name = "PartyTableLayoutPanel";
			this.PartyTableLayoutPanel.RowCount = 2;
			this.PartyTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(27)));
			this.PartyTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PartyTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 156, true);
			this.PartyTableLayoutPanel.TabIndex = 0;
			// 
			// PartyRiskPanel
			// 
			this.PartyRiskPanel.Controls.Add(this.PartyRiskPanelLabel);
			this.PartyRiskPanel.Controls.Add(this.PartyRiskLabel);
			this.PartyRiskPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartyRiskPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PartyRiskPanel.Name = "PartyRiskPanel";
			this.PartyRiskPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
			this.PartyRiskPanel.TabIndex = 0;
			// 
			// PartyRiskPanelLabel
			// 
			this.PartyRiskPanelLabel.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("7b8bda2b-f2fc-4538-9702-5cfaabcfd0dd", "Compliance Risk");
			this.PartyRiskPanelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PartyRiskPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.PartyRiskPanelLabel.Name = "PartyRiskPanelLabel";
			this.PartyRiskPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 16, true);
			this.PartyRiskPanelLabel.TabIndex = 0;
			this.PartyRiskPanelLabel.UseMnemonic = false;
			// 
			// PartyRiskLabel
			// 
			this.BindingSource.SetBindingMember(this.PartyRiskLabel, "PartyRiskDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).PartyRiskDescription)));
			this.PartyRiskLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PartyRiskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, -1, true);
			this.PartyRiskLabel.Name = "PartyRiskLabel";
			this.PartyRiskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 18, true);
			this.PartyRiskLabel.TabIndex = 1;
			this.PartyRiskLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.PartyRiskLabel.UseMnemonic = false;
			this.PartyRiskLabel.TextChanged += new System.EventHandler(this.RiskStatusValueChanged);
			// 
			// LocationGroupBox
			// 
			this.LocationGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("b14c97cc-42f9-4d19-8001-b287bcdc2537", "Locations");
			this.LocationGroupBox.Controls.Add(this.LocationHideCheckBox);
			this.LocationGroupBox.Controls.Add(this.LocationTableLayoutPanel);
			this.LocationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocationGroupBox.Name = "LocationGroupBox";
			this.LocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 171, true);
			this.LocationGroupBox.TabIndex = 0;
			this.LocationGroupBox.TabStop = false;
			// 
			// LocationHideCheckBox
			// 
			this.LocationHideCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocationHideCheckBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("7120eaa1-1fc9-427b-9d73-2b501dcaa64b", "Hide");
			this.LocationHideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(934, 0, true);
			this.LocationHideCheckBox.Name = "LocationHideCheckBox";
			this.LocationHideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.LocationHideCheckBox.TabIndex = 3;
			this.LocationHideCheckBox.CheckedChanged += new System.EventHandler(this.HideCheckBox_CheckedChanged);
			// 
			// LocationTableLayoutPanel
			// 
			this.LocationTableLayoutPanel.ColumnCount = 1;
			this.LocationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.LocationTableLayoutPanel.Controls.Add(this.LocationRiskPanel, 0, 0);
			this.LocationTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocationTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.LocationTableLayoutPanel.Name = "LocationTableLayoutPanel";
			this.LocationTableLayoutPanel.RowCount = 2;
			this.LocationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(27)));
			this.LocationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.LocationTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 155, true);
			this.LocationTableLayoutPanel.TabIndex = 1;
			// 
			// LocationRiskPanel
			// 
			this.LocationRiskPanel.Controls.Add(this.LocationRiskPanelLabel);
			this.LocationRiskPanel.Controls.Add(this.LocationRiskLabel);
			this.LocationRiskPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocationRiskPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.LocationRiskPanel.Name = "LocationRiskPanel";
			this.LocationRiskPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
			this.LocationRiskPanel.TabIndex = 0;
			// 
			// LocationRiskPanelLabel
			// 
			this.LocationRiskPanelLabel.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("23f207a1-7a2b-4300-8b9d-596022f97265", "Compliance Risk");
			this.LocationRiskPanelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LocationRiskPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.LocationRiskPanelLabel.Name = "LocationRiskPanelLabel";
			this.LocationRiskPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 16, true);
			this.LocationRiskPanelLabel.TabIndex = 0;
			this.LocationRiskPanelLabel.UseMnemonic = false;
			// 
			// LocationRiskLabel
			// 
			this.BindingSource.SetBindingMember(this.LocationRiskLabel, "LocationRiskDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).LocationRiskDescription)));
			this.LocationRiskLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LocationRiskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, -1, true);
			this.LocationRiskLabel.Name = "LocationRiskLabel";
			this.LocationRiskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 18, true);
			this.LocationRiskLabel.TabIndex = 1;
			this.LocationRiskLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.LocationRiskLabel.UseMnemonic = false;
			this.LocationRiskLabel.TextChanged += new System.EventHandler(this.RiskStatusValueChanged);
			// 
			// CommodityGroupBox
			// 
			this.CommodityGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("7120eaa1-1fc9-427b-9d73-2b501dcaa64a", "Commodities");
			this.CommodityGroupBox.Controls.Add(this.CommodityHideCheckBox);
			this.CommodityGroupBox.Controls.Add(this.CommodityTableLayoutPanel);
			this.CommodityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityGroupBox.Name = "CommodityGroupBox";
			this.CommodityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 236, true);
			this.CommodityGroupBox.TabIndex = 0;
			this.CommodityGroupBox.TabStop = false;
			// 
			// CommodityHideCheckBox
			// 
			this.CommodityHideCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CommodityHideCheckBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("7120eaa1-1fc9-427b-9d73-2b501dcaa64b", "Hide");
			this.CommodityHideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(934, 0, true);
			this.CommodityHideCheckBox.Name = "CommodityHideCheckBox";
			this.CommodityHideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.CommodityHideCheckBox.TabIndex = 3;
			this.CommodityHideCheckBox.CheckedChanged += new System.EventHandler(this.HideCheckBox_CheckedChanged);
			// 
			// CommodityTableLayoutPanel
			// 
			this.CommodityTableLayoutPanel.ColumnCount = 1;
			this.CommodityTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.CommodityTableLayoutPanel.Controls.Add(this.CommodityRiskPanel, 0, 0);
			this.CommodityTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CommodityTableLayoutPanel.Name = "CommodityTableLayoutPanel";
			this.CommodityTableLayoutPanel.RowCount = 2;
			this.CommodityTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(27)));
			this.CommodityTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.CommodityTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 219, true);
			this.CommodityTableLayoutPanel.TabIndex = 0;
			// 
			// CommodityRiskPanel
			// 
			this.CommodityRiskPanel.Controls.Add(this.CommodityRiskLabel);
			this.CommodityRiskPanel.Controls.Add(this.CommodityRiskPanelLabel);
			this.CommodityRiskPanel.Controls.Add(this.AssessmentInitializeButton);
			this.CommodityRiskPanel.Controls.Add(this.CommoditySpinnerIndicator);
			this.CommodityRiskPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityRiskPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.CommodityRiskPanel.Name = "CommodityRiskPanel";
			this.CommodityRiskPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 23, true);
			this.CommodityRiskPanel.TabIndex = 0;
			// 
			// CommodityRiskLabel
			// 
			this.BindingSource.SetBindingMember(this.CommodityRiskLabel, "CommodityRiskDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRiskPlugInBusinessObject)(null)).CommodityRiskDescription)));
			this.CommodityRiskLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CommodityRiskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 1, true);
			this.CommodityRiskLabel.Name = "CommodityRiskLabel";
			this.CommodityRiskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 18, true);
			this.CommodityRiskLabel.TabIndex = 1;
			this.CommodityRiskLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CommodityRiskLabel.UseMnemonic = false;
			this.CommodityRiskLabel.TextChanged += new System.EventHandler(this.RiskStatusValueChanged);
			// 
			// CommodityRiskPanelLabel
			// 
			this.CommodityRiskPanelLabel.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("9C827351-07D5-4758-A334-CE87F79C1833", "Compliance Risk");
			this.CommodityRiskPanelLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CommodityRiskPanelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.CommodityRiskPanelLabel.Name = "CommodityRiskPanelLabel";
			this.CommodityRiskPanelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 16, true);
			this.CommodityRiskPanelLabel.TabIndex = 0;
			this.CommodityRiskPanelLabel.UseMnemonic = false;
			// 
			// AssessmentInitializeButton
			// 
			this.AssessmentInitializeButton.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("111DCB47-D659-40C6-8AAC-3F1110118573", "Initiate Assessment");
			this.AssessmentInitializeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.AssessmentInitializeButton.Name = "AssessmentInitializeButton";
			this.AssessmentInitializeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.AssessmentInitializeButton.TabIndex = 2;
			this.AssessmentInitializeButton.ToolTipCaption = null;
			this.AssessmentInitializeButton.UseVisualStyleBackColor = true;
			// 
			// CommoditySpinnerIndicator
			// 
			this.CommoditySpinnerIndicator.AllowDrop = true;
			this.CommoditySpinnerIndicator.AutoSize = true;
			this.CommoditySpinnerIndicator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 0, true);
			this.CommoditySpinnerIndicator.Name = "CommoditySpinnerIndicator";
			this.CommoditySpinnerIndicator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.CommoditySpinnerIndicator.TabIndex = 3;
			this.CommoditySpinnerIndicator.Visible = false;
			// 
			// ComplianceRiskPlugInUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComplianceRiskGroupBox);
			this.Name = "ComplianceRiskPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 640, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ComplianceRiskGroupBox.ResumeLayout(false);
			this.ComplianceRiskGroupBox.PerformLayout();
			this.ComplianceRiskTableLayoutPanel.ResumeLayout(false);
			this.ComplianceRiskTableLayoutPanel.PerformLayout();
			this.OverallRiskPanel.ResumeLayout(false);
			this.OverallRiskPanel.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.SubSplitContainer.Panel1.ResumeLayout(false);
			this.SubSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SubSplitContainer)).EndInit();
			this.SubSplitContainer.ResumeLayout(false);
			this.SubSplitContainer.PerformLayout();
			this.PartyGroupBox.ResumeLayout(false);
			this.PartyGroupBox.PerformLayout();
			this.PartyTableLayoutPanel.ResumeLayout(false);
			this.PartyTableLayoutPanel.PerformLayout();
			this.PartyRiskPanel.ResumeLayout(false);
			this.PartyRiskPanel.PerformLayout();
			this.LocationGroupBox.ResumeLayout(false);
			this.LocationGroupBox.PerformLayout();
			this.LocationTableLayoutPanel.ResumeLayout(false);
			this.LocationTableLayoutPanel.PerformLayout();
			this.LocationRiskPanel.ResumeLayout(false);
			this.LocationRiskPanel.PerformLayout();
			this.CommodityGroupBox.ResumeLayout(false);
			this.CommodityGroupBox.PerformLayout();
			this.CommodityTableLayoutPanel.ResumeLayout(false);
			this.CommodityTableLayoutPanel.PerformLayout();
			this.CommodityRiskPanel.ResumeLayout(false);
			this.CommodityRiskPanel.PerformLayout();
			this.CommoditySpinnerIndicator.ResumeLayout(true);
			this.CommoditySpinnerIndicator.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.GUI.ZGroupBox ComplianceRiskGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox PartyGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox LocationGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox CommodityGroupBox;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal CargoWise.Windows.UI.KSplitContainer SubSplitContainer;
		protected CargoWise.Windows.UI.KTableLayoutPanel ComplianceRiskTableLayoutPanel;
		internal CargoWise.Windows.UI.KTableLayoutPanel PartyTableLayoutPanel;
		internal CargoWise.Windows.UI.KTableLayoutPanel LocationTableLayoutPanel;
		internal CargoWise.Windows.UI.KTableLayoutPanel CommodityTableLayoutPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel OverallRiskPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel PartyRiskPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel LocationRiskPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel CommodityRiskPanel;
		internal Enterprise.ZArchitecture.ZLabel OverallRiskLabel;
		internal Enterprise.ZArchitecture.ZLabel PartyRiskLabel;
		internal Enterprise.ZArchitecture.ZLabel LocationRiskLabel;
		protected Enterprise.ZArchitecture.ZLabel OverallRiskPanelLabel;
		protected Enterprise.ZArchitecture.ZLabel PartyRiskPanelLabel;
		protected Enterprise.ZArchitecture.ZLabel LocationRiskPanelLabel;
		protected Enterprise.ZArchitecture.ZLabel OverallRiskRegistryLabel;
		internal Enterprise.ZArchitecture.ZLabel CommodityRiskLabel;
		protected Enterprise.ZArchitecture.ZLabel CommodityRiskPanelLabel;
		internal ZArchitecture.GUI.ZButton AssessmentInitializeButton;
		protected Enterprise.ComplianceRisk.GUI.ComplianceRiskSpinnerIndicator CommoditySpinnerIndicator;
		internal ZArchitecture.GUI.ZCheckBox PartyHideCheckBox;
		internal ZArchitecture.GUI.ZCheckBox LocationHideCheckBox;
		internal ZArchitecture.GUI.ZCheckBox CommodityHideCheckBox;

		#endregion
	}
}
