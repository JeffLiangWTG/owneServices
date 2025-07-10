namespace Enterprise.Registry.GUI
{
	partial class HVLVPreScreeningRuleControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ValidationRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IsEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ScreeningValuesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HSCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SpecialCharactersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidationFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.SplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.ScreeningValuePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HSCodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeminimusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeminimusCheckedTextBox = new Enterprise.ZArchitecture.GUI.ZCheckedTextBox();
			this.SameConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeminimusCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MacrosScriptPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MacrosScriptText = new Enterprise.ZArchitecture.ZTextBox();
			this.InsertMacroButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SpecialCharactersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesGrid)).BeginInit();
			this.ValidationRulesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ScreeningValuesGrid)).BeginInit();
			this.ScreeningValuesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HSCodeGrid)).BeginInit();
			this.HSCodeGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecialCharactersGrid)).BeginInit();
			this.SpecialCharactersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationFieldsGrid)).BeginInit();
			this.ValidationFieldsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
			this.SplitContainer1.Panel1.SuspendLayout();
			this.SplitContainer1.Panel2.SuspendLayout();
			this.SplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).BeginInit();
			this.SplitContainer2.Panel1.SuspendLayout();
			this.SplitContainer2.Panel2.SuspendLayout();
			this.SplitContainer2.SuspendLayout();
			this.ScreeningValuePanel.SuspendLayout();
			this.HSCodePanel.SuspendLayout();
			this.DeminimusPanel.SuspendLayout();
			this.DeminimusCheckedTextBox.SuspendLayout();
			this.SameConsigneeCheckBox.SuspendLayout();
			this.MacrosScriptPanel.SuspendLayout();
			this.SpecialCharactersPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration);
			// 
			// ValidationRulesGrid
			// 
			this.ValidationRulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValidationRulesGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).ETailer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).OriginCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).DestinationCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).ModuleType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).EmailNotificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).EmailNotificationGroup)));
			this.ValidationRulesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("95cda532-6a11-4b31-bece-1b06365bbed6", "Transport Mode");
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3ecbeed7-24b8-4a23-8cf5-486c45f7cc83", "eTailer");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ETailer";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d0c67700-e723-40f0-8755-e51684ab65fc", "Origin Country/Region");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "OriginCountryCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("16b1dd93-e53c-4f8c-80bd-b48d393c2739", "Destination Country/Region");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "DestinationCountryCode";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3c20d732-cde2-4266-a7cc-91b37bcbad3a", "Module Type");
			zDropEditColumnStyleInfo2.ColumnName = "ModuleType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("25F69E34-21E5-4B1A-AB6D-65E56432F823", "Notification Type");
			zDropEditColumnStyleInfo7.ColumnName = "EmailNotificationType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2CAC1955-8DF4-483A-98AC-AF4B4BE9C83D", "Notification Group");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "EmailNotificationGroup";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ValidationRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ValidationRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.ValidationRulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationRulesGrid.GridId = "33e2bda3-7759-49b0-998f-36aae13a7e1e";
			this.ValidationRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValidationRulesGrid.LayoutKey = "ValidationRulesGrid";
			this.ValidationRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationRulesGrid.Name = "ValidationRulesGrid";
			this.ValidationRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 136, true);
			this.ValidationRulesGrid.TabIndex = 0;
			// 
			// IsEnabledCheckBox
			// 
			this.IsEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsEnabledCheckBox, "IsEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).IsEnabled)));
			this.IsEnabledCheckBox.CaptionResourceString = Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration.IsEnabledUICaption;
			this.IsEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IsEnabledCheckBox.Name = "IsEnabledCheckBox";
			this.IsEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.IsEnabledCheckBox.TabIndex = 0;
			this.IsEnabledCheckBox.UseMnemonic = false;
			this.IsEnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// ScreeningValuesGrid
			// 
			this.ScreeningValuesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ScreeningValuesGrid, "Rules.Fields.ScreeningValues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)).SyncRoot)).ScreeningValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)).SyncRoot)).ScreeningComparisonOperatorDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)).SyncRoot)).MessageTextPerValue)));
			this.ScreeningValuesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("eb4e6583-fb99-4dbc-86e3-cfbda03c42e1", "Screening Value");
			zTextBoxColumnStyleInfo1.ColumnName = "ScreeningValue";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0c2498d6-06f1-4a8c-a859-eb85f9c1ecf9", "Screening Value Options");
			zDropEditColumnStyleInfo3.ColumnName = "ScreeningComparisonOperatorDescription";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ab14fd5a-1f3f-4343-923f-1c7ef6ffceb9", "Message Text Per Value");
			zTextBoxColumnStyleInfo2.ColumnName = "MessageTextPerValue";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ScreeningValuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ScreeningValuesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ScreeningValuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ScreeningValuesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScreeningValuesGrid.GridId = "d38cd717-76dc-4c23-bfbc-ab7238c79403";
			this.ScreeningValuesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ScreeningValuesGrid.LayoutKey = "ScreeningValuesGrid";
			this.ScreeningValuesGrid.LinkColor = System.Drawing.SystemColors.Highlight;
			this.ScreeningValuesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScreeningValuesGrid.Name = "ScreeningValuesGrid";
			this.ScreeningValuesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.ScreeningValuesGrid.TabIndex = 1;
			//
			// HSCodeGrid
			// 
			this.HSCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HSCodeGrid, "Rules.Fields.ScreeningValues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)).SyncRoot)).FromHSCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)).SyncRoot)).ToHSCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ScreeningValues)).SyncRoot)).MessageTextPerValue)));
			this.HSCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("19835d5b-7632-404f-acfb-9d6c909b6ded", "From HS Code");
			zTextBoxColumnStyleInfo4.ColumnName = "FromHSCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2c9f65c5-659d-4105-b5e7-bed8c8c1ae6f", "To HS Code");
			zTextBoxColumnStyleInfo6.ColumnName = "ToHSCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ab14fd5a-1f3f-4343-923f-1c7ef6ffceb9", "Message Text Per Value");
			zTextBoxColumnStyleInfo5.ColumnName = "MessageTextPerValue";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.HSCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.HSCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.HSCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.HSCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HSCodeGrid.GridId = "34a9e3bb-1920-439c-8115-cfd6d3c23c71";
			this.HSCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HSCodeGrid.LayoutKey = "HSCodeGrid";
			this.HSCodeGrid.LinkColor = System.Drawing.SystemColors.Highlight;
			this.HSCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HSCodeGrid.Name = "HSCodeGrid";
			this.HSCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.HSCodeGrid.TabIndex = 2;
			// 
			// SpecialCharactersGrid
			// 
			this.SpecialCharactersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SpecialCharactersGrid, "Rules.Fields.SpecialCharacters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).SpecialCharacters)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningSpecialCharacterValue)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).SpecialCharacters)).SyncRoot)).CharacterValue)));
			this.SpecialCharactersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b0b6f34b-ba36-43ff-8a83-516f16901caa", "Special Character");
			zDropEditColumnStyleInfo4.ColumnName = "CharacterValue";
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.SpecialCharactersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.SpecialCharactersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialCharactersGrid.GridId = "2f1f0ab8-7d04-4d5a-98e8-3a32a1373259";
			this.SpecialCharactersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SpecialCharactersGrid.LayoutKey = "SpecialCharactersGrid";
			this.SpecialCharactersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SpecialCharactersGrid.Name = "SpecialCharactersGrid";
			this.SpecialCharactersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.SpecialCharactersGrid.TabIndex = 0;
			// 
			// ValidationFieldsGrid
			// 
			this.ValidationFieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValidationFieldsGrid, "Rules.Fields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).FieldDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).ValidationRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).MessageText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).IsMandatory)));
			this.ValidationFieldsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("641b92c9-c22f-4f87-bd44-e83d28388831", "Field Name");
			zDropEditColumnStyleInfo5.ColumnName = "FieldDescription";
			zDropEditColumnStyleInfo5.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e674ef2a-43f9-4976-96c4-20f781b4c3c1", "Validation Rule");
			zDropEditColumnStyleInfo6.ColumnName = "ValidationRule";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5ac51592-3674-4d99-97a6-b3486c8792f6", "Message Text");
			zTextBoxColumnStyleInfo3.ColumnName = "MessageText";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("fa7166d8-f988-4e11-8e4f-8ef067309f45", "Is Mandatory");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsMandatory";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ValidationFieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ValidationFieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ValidationFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ValidationFieldsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ValidationFieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationFieldsGrid.GridId = "d38cd717-76dc-4c23-bfbc-ab7238c79403";
			this.ValidationFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValidationFieldsGrid.LayoutKey = "ScreeningValuesGrid";
			this.ValidationFieldsGrid.LinkColor = System.Drawing.SystemColors.Highlight;
			this.ValidationFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationFieldsGrid.Name = "ValidationFieldsGrid";
			this.ValidationFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 124, true);
			this.ValidationFieldsGrid.TabIndex = 0;
			// 
			// SplitContainer1
			// 
			this.SplitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.SplitContainer1.Name = "SplitContainer1";
			this.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer1.Panel1
			// 
			this.SplitContainer1.Panel1.Controls.Add(this.ValidationRulesGrid);
			// 
			// SplitContainer1.Panel2
			// 
			this.SplitContainer1.Panel2.Controls.Add(this.SplitContainer2);
			this.SplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 394, true);
			this.SplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(136);
			this.SplitContainer1.TabIndex = 1;
			// 
			// SplitContainer2
			// 
			this.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer2.Name = "SplitContainer2";
			this.SplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer2.Panel1
			// 
			this.SplitContainer2.Panel1.Controls.Add(this.ValidationFieldsGrid);
			// 
			// SplitContainer2.Panel2
			// 
			this.SplitContainer2.Panel2.Controls.Add(this.ScreeningValuePanel);
			this.SplitContainer2.Panel2.Controls.Add(this.HSCodePanel);
			this.SplitContainer2.Panel2.Controls.Add(this.DeminimusPanel);
			this.SplitContainer2.Panel2.Controls.Add(this.MacrosScriptPanel);
			this.SplitContainer2.Panel2.Controls.Add(this.SpecialCharactersPanel);
			this.SplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 254, true);
			this.SplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(124);
			this.SplitContainer2.TabIndex = 2;
			// 
			// ScreeningValuePanel
			// 
			this.ScreeningValuePanel.Controls.Add(this.ScreeningValuesGrid);
			this.ScreeningValuePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ScreeningValuePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScreeningValuePanel.Name = "ScreeningValuePanel";
			this.ScreeningValuePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.ScreeningValuePanel.TabIndex = 0;
			//
			// HSCodePanel
			//
			this.HSCodePanel.Controls.Add(this.HSCodeGrid);
			this.HSCodePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HSCodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HSCodePanel.Name = "HSCodePanel";
			this.HSCodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.HSCodePanel.TabIndex = 0;
			//
			// DeminimusPanel
			// 
			this.DeminimusPanel.Controls.Add(this.DeminimusCheckedTextBox);
			this.DeminimusPanel.Controls.Add(this.DeminimusCurrencyCodeFindBox);
			this.DeminimusPanel.Controls.Add(this.SameConsigneeCheckBox);
			this.DeminimusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeminimusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeminimusPanel.Name = "DeminimusPanel";
			this.DeminimusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 156, true);
			this.DeminimusPanel.TabIndex = 1;
			// 
			// DeminimusCheckedTextBox
			// 
			this.DeminimusCheckedTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeminimusCheckedTextBox, "Rules.Fields.DeminimusValue");
			this.BindingSource.SetBindingMember(this.DeminimusCheckedTextBox.CheckBox, "Rules.Fields.IsDeminimusValueOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).DeminimusValue)));
			this.DeminimusCheckedTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("767e8cb7-4468-4c53-aca2-fb8d61df8596", "Override De Minimis Value");
			this.DeminimusCheckedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.DeminimusCheckedTextBox.Checked = false;
			this.DeminimusCheckedTextBox.Gap = 5;
			this.DeminimusCheckedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 20, true);
			this.DeminimusCheckedTextBox.MaxLength = 32767;
			this.DeminimusCheckedTextBox.Name = "DeminimusCheckedTextBox";
			this.DeminimusCheckedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 21, true);
			this.DeminimusCheckedTextBox.TabIndex = 0;
			// 
			// Check Same Consignee
			//
			this.SameConsigneeCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SameConsigneeCheckBox, "Rules.Fields.CheckSameConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).CheckSameConsignee)));
			this.SameConsigneeCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2a58e370-2884-48e2-a745-898688cb115f", "Check Same Consignee");
			this.SameConsigneeCheckBox.CheckAlign = ZArchitecture.GUI.ZContentAlignment.Right;
			this.SameConsigneeCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SameConsigneeCheckBox.Checked = false;
			this.SameConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.SameConsigneeCheckBox.Name = "SameConsigneeCheckedTextBox";
			this.SameConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 21, true);
			this.SameConsigneeCheckBox.TabIndex = 1;
			// 
			// DeminimusCurrencyCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.DeminimusCurrencyCodeFindBox, "Rules.Fields.DeminimusCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).DeminimusCurrency)));
			this.DeminimusCurrencyCodeFindBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("F287E254-6072-4578-AE76-239CAA1EE600", "Deminimus Currency");
			this.DeminimusCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 20, true);
			this.DeminimusCurrencyCodeFindBox.Name = "DeminimusCurrencyCodeFindBox";
			this.DeminimusCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 21, true);
			this.DeminimusCurrencyCodeFindBox.TabIndex = 1;
			// 
			// MacrosScriptPanel
			// 
			this.MacrosScriptPanel.Controls.Add(this.MacrosScriptText);
			this.MacrosScriptPanel.Controls.Add(this.InsertMacroButton);
			this.MacrosScriptPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MacrosScriptPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MacrosScriptPanel.Name = "MacrosScriptPanel";
			this.MacrosScriptPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.MacrosScriptPanel.TabIndex = 2;
			// 
			// MacrosScriptText
			// 
			this.MacrosScriptText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MacrosScriptText, "Rules.Fields.MacrosScript");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HVLVPreScreeningField)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVPreScreeningRule)(((System.Collections.IList)(((Enterprise.Registry.Business.HVLVDetailsPreScreeningConfiguration)(null)).Rules)).SyncRoot)).Fields)).SyncRoot)).MacrosScript)));
			this.MacrosScriptText.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f54d7ed6-3e89-4def-8a50-ab909fe587a8", "Macros");
			this.MacrosScriptText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MacrosScriptText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 3, true);
			this.MacrosScriptText.Multiline = true;
			this.MacrosScriptText.Name = "MacrosScriptText";
			this.MacrosScriptText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 77, true);
			this.MacrosScriptText.TabIndex = 0;
			// 
			// InsertMacroButton
			// 
			this.InsertMacroButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c987c05f-d407-4ca6-a7a0-6cdeb08ce9ee", "Insert Macro");
			this.InsertMacroButton.IsCaptionOverridden = false;
			this.InsertMacroButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 86, true);
			this.InsertMacroButton.Name = "InsertMacroButton";
			this.InsertMacroButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.InsertMacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.InsertMacroButton.TabIndex = 1;
			this.InsertMacroButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.InsertMacroButton.ToolTipCaption = null;
			this.InsertMacroButton.Click += new System.EventHandler(this.InsertMacroButton_Click);
			// 
			// SpecialCharactersPanel
			// 
			this.SpecialCharactersPanel.Controls.Add(this.SpecialCharactersGrid);
			this.SpecialCharactersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialCharactersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SpecialCharactersPanel.Name = "SpecialCharactersPanel";
			this.SpecialCharactersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 126, true);
			this.SpecialCharactersPanel.TabIndex = 0;
			// 
			// HVLVPreScreeningRuleControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer1);
			this.Controls.Add(this.IsEnabledCheckBox);
			this.Name = "HVLVPreScreeningRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 420, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesGrid)).EndInit();
			this.ValidationRulesGrid.ResumeLayout(false);
			this.ValidationRulesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ScreeningValuesGrid)).EndInit();
			this.ScreeningValuesGrid.ResumeLayout(false);
			this.ScreeningValuesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HSCodeGrid)).EndInit();
			this.HSCodeGrid.ResumeLayout(false);
			this.HSCodeGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecialCharactersGrid)).EndInit();
			this.SpecialCharactersGrid.ResumeLayout(false);
			this.SpecialCharactersGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationFieldsGrid)).EndInit();
			this.ValidationFieldsGrid.ResumeLayout(false);
			this.ValidationFieldsGrid.PerformLayout();
			this.SplitContainer1.Panel1.ResumeLayout(false);
			this.SplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
			this.SplitContainer1.ResumeLayout(false);
			this.SplitContainer1.PerformLayout();
			this.SplitContainer2.Panel1.ResumeLayout(false);
			this.SplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).EndInit();
			this.SplitContainer2.ResumeLayout(false);
			this.SplitContainer2.PerformLayout();
			this.ScreeningValuePanel.ResumeLayout(false);
			this.ScreeningValuePanel.PerformLayout();
			this.HSCodePanel.ResumeLayout(false);
			this.HSCodePanel.PerformLayout();
			this.DeminimusPanel.ResumeLayout(false);
			this.DeminimusPanel.PerformLayout();
			this.DeminimusCheckedTextBox.ResumeLayout(true);
			this.DeminimusCheckedTextBox.PerformLayout();
			this.SameConsigneeCheckBox.ResumeLayout(true);
			this.SameConsigneeCheckBox.PerformLayout();
			this.MacrosScriptPanel.ResumeLayout(false);
			this.MacrosScriptPanel.PerformLayout();
			this.SpecialCharactersPanel.ResumeLayout(false);
			this.SpecialCharactersPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ValidationRulesGrid;
		internal ZArchitecture.GUI.ZCheckBox IsEnabledCheckBox;
		private Enterprise.ZArchitecture.ZGrid ValidationFieldsGrid;
		private Enterprise.ZArchitecture.ZGrid ScreeningValuesGrid;
		private Enterprise.ZArchitecture.ZGrid HSCodeGrid;
		private Enterprise.ZArchitecture.ZGrid SpecialCharactersGrid;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer1;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer2;
		private ZArchitecture.GUI.ZCheckedTextBox DeminimusCheckedTextBox;
		private ZArchitecture.GUI.ZCheckBox SameConsigneeCheckBox;
		private Enterprise.ZArchitecture.ZTextBox MacrosScriptText;
		private ZArchitecture.GUI.ZPanel ScreeningValuePanel;
		private ZArchitecture.GUI.ZPanel HSCodePanel;
		private ZArchitecture.GUI.ZPanel DeminimusPanel;
		private ZArchitecture.GUI.ZPanel MacrosScriptPanel;
		private ZArchitecture.GUI.ZPanel SpecialCharactersPanel;
		private ZArchitecture.GUI.ZCodeFindBox DeminimusCurrencyCodeFindBox;
		private ZArchitecture.GUI.ZButton InsertMacroButton;
	}
}
