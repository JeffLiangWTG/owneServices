using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Sailing.GUI
{
	public partial class SailingPluginUserControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.impendingArrivalStatusTextBox = new ZArchitecture.ZTextBox();
			this.impendingArrivalLabel = new ZArchitecture.ZLabel();
			this.arrivalTabControl = new ZTemplateTabControl();
			this.arrivalPortsTabPage = new ZTabPage();
			this.arrivalsGrid = new ZArchitecture.ZGrid();
			this.actualArrivalsTabControl = new ZTemplateTabControl();
			this.actualArrivalMessagesTabPage = new ZTabPage();
			this.actualArrivalMessageUserControl = new Messaging.GUI.EDIMessageUserControl();
			this.impendingMessagesTabPage = new ZTabPage();
			this.ediMessageUserControl1 = new Messaging.GUI.EDIMessageUserControl();
			this.upperPanel = new ZPanel();
			this.flightNumberTextBox = new ZArchitecture.ZTextBox();
			this.flightNumberLabel = new ZArchitecture.ZLabel();
			this.aTDLabel = new ZArchitecture.ZLabel();
			this.aTDDateEdit = new ZDateEdit();
			this.firstPortOfArrivalLabel = new ZArchitecture.ZLabel();
			this.portOfFirstArrivalTextBox = new ZArchitecture.ZTextBox();
			this.lastOverseasPortOfDepartureLabel = new ZArchitecture.ZLabel();
			this.lastOverseasPortOfDepartureTextBox = new ZArchitecture.ZTextBox();
			this.lowerPanel = new ZPanel();
			this.arrivalTabControl.SuspendLayout();
			this.arrivalPortsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.arrivalsGrid)).BeginInit();
			this.actualArrivalsTabControl.SuspendLayout();
			this.actualArrivalMessagesTabPage.SuspendLayout();
			this.impendingMessagesTabPage.SuspendLayout();
			this.upperPanel.SuspendLayout();
			this.lowerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ImpendingArrivalStatusTextBox
			// 
			this.impendingArrivalStatusTextBox.BindTo = "ImpendingArrivalStatus+Description";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).ImpendingArrivalStatus.DescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).ImpendingArrivalStatus.Description)));
			this.impendingArrivalStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|bb98b423-0d56-4b3d-a43c-153f893282cb", "IAR Status");
			this.impendingArrivalStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.impendingArrivalStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.impendingArrivalStatusTextBox.Name = "ImpendingArrivalStatusTextBox";
			this.impendingArrivalStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.impendingArrivalStatusTextBox.TabIndex = 0;
			// 
			// ImpendingArrivalLabel
			// 
			this.impendingArrivalLabel.AutoSize = true;
			this.impendingArrivalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.impendingArrivalLabel.Name = "ImpendingArrivalLabel";
			this.impendingArrivalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.impendingArrivalLabel.TabIndex = 1;
			this.impendingArrivalLabel.Text = "IAR Status:";
			// 
			// ArrivalTabControl
			// 
			this.arrivalTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.arrivalTabControl.Controls.Add(this.arrivalPortsTabPage);
			this.arrivalTabControl.Controls.Add(this.impendingMessagesTabPage);
			this.arrivalTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.arrivalTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.arrivalTabControl.Name = "ArrivalTabControl";
			this.arrivalTabControl.SelectedIndex = 0;
			this.arrivalTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 464, true);
			this.arrivalTabControl.TabIndex = 0;
			// 
			// ArrivalPortsTabPage
			// 
			this.arrivalPortsTabPage.CheckForNotifications = true;
			this.arrivalPortsTabPage.Controls.Add(this.arrivalsGrid);
			this.arrivalPortsTabPage.Controls.Add(this.actualArrivalsTabControl);
			this.arrivalPortsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.arrivalPortsTabPage.Name = "ArrivalPortsTabPage";
			this.arrivalPortsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 437, true);
			this.arrivalPortsTabPage.TabIndex = 0;
			this.arrivalPortsTabPage.Text = "Arrivals";
			// 
			// ArrivalsGrid
			// 
			this.arrivalsGrid.AllowNavigation = false;
			this.arrivalsGrid.BindTo = "Destinations";
			this.arrivalsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "Port Of Arrival";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|4ea890a3-f669-40cc-9930-8b7b6e75061c", "Port Of Arrival");
			zTextBoxColumnStyleInfo3.ColumnName = "PortOfArrival";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zDateEditColumnStyleInfo3.Caption = "Actual Arrival Date Time (UTC)";
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|fe01ae7c-ee3d-40b9-971b-ea482031c492", "Actual Arrival Date Time(UTC)");
			zDateEditColumnStyleInfo3.ColumnName = "ActualArrivalDateTimeUTC";
			zDateEditColumnStyleInfo4.Caption = "Estimated Date Time Of Arrival (UTC)";
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|9e2a076f-10e2-491e-9ffe-f97f6e53c683", "Estimated Date Of Arrival(UTC)");
			zDateEditColumnStyleInfo4.ColumnName = "EstimatedDateTimeOfArrivalUTC";
			zTextBoxColumnStyleInfo4.Caption = "Actual ArrivalStatus";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|03e06c8f-eef9-4d73-967b-bb292090b92b", "Actual Arrival Status");
			zTextBoxColumnStyleInfo4.ColumnName = "ActualArrivalStatus+Description";
			this.arrivalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.arrivalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.arrivalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.arrivalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.arrivalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.arrivalsGrid.EnableToolTips = false;
			this.arrivalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.arrivalsGrid.LayoutKey = "zGrid1";
			this.arrivalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.arrivalsGrid.Name = "ArrivalsGrid";
			this.arrivalsGrid.ReadOnly = true;
			this.arrivalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 109, true);
			this.arrivalsGrid.TabIndex = 0;
			// 
			// ActualArrivalsTabControl
			// 
			this.actualArrivalsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.actualArrivalsTabControl.Controls.Add(this.actualArrivalMessagesTabPage);
			this.actualArrivalsTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.actualArrivalsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.actualArrivalsTabControl.Name = "ActualArrivalsTabControl";
			this.actualArrivalsTabControl.SelectedIndex = 0;
			this.actualArrivalsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 328, true);
			this.actualArrivalsTabControl.TabIndex = 1;
			// 
			// ActualArrivalMessagesTabPage
			// 
			this.actualArrivalMessagesTabPage.CheckForNotifications = true;
			this.actualArrivalMessagesTabPage.Controls.Add(this.actualArrivalMessageUserControl);
			this.actualArrivalMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.actualArrivalMessagesTabPage.Name = "ActualArrivalMessagesTabPage";
			this.actualArrivalMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 301, true);
			this.actualArrivalMessagesTabPage.TabIndex = 0;
			this.actualArrivalMessagesTabPage.Text = "Actual Arrival Messages";
			// 
			// ActualArrivalMessageUserControl
			// 
			this.actualArrivalMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.actualArrivalMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.actualArrivalMessageUserControl.Name = "ActualArrivalMessageUserControl";
			this.actualArrivalMessageUserControl.ShowChangingBlueMessageHeading = false;
			this.actualArrivalMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 301, true);
			this.actualArrivalMessageUserControl.TabIndex = 0;
			// 
			// ImpendingMessagesTabPage
			// 
			this.impendingMessagesTabPage.CheckForNotifications = true;
			this.impendingMessagesTabPage.Controls.Add(this.ediMessageUserControl1);
			this.impendingMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.impendingMessagesTabPage.Name = "ImpendingMessagesTabPage";
			this.impendingMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 437, true);
			this.impendingMessagesTabPage.TabIndex = 1;
			this.impendingMessagesTabPage.Text = "Impending Arrival Messages";
			// 
			// ediMessageUserControl1
			// 
			this.ediMessageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ediMessageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ediMessageUserControl1.Name = "ediMessageUserControl1";
			this.ediMessageUserControl1.ShowChangingBlueMessageHeading = false;
			this.ediMessageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 437, true);
			this.ediMessageUserControl1.TabIndex = 0;
			// 
			// UpperPanel
			// 
			this.upperPanel.Controls.Add(this.flightNumberTextBox);
			this.upperPanel.Controls.Add(this.flightNumberLabel);
			this.upperPanel.Controls.Add(this.aTDLabel);
			this.upperPanel.Controls.Add(this.aTDDateEdit);
			this.upperPanel.Controls.Add(this.firstPortOfArrivalLabel);
			this.upperPanel.Controls.Add(this.portOfFirstArrivalTextBox);
			this.upperPanel.Controls.Add(this.lastOverseasPortOfDepartureLabel);
			this.upperPanel.Controls.Add(this.lastOverseasPortOfDepartureTextBox);
			this.upperPanel.Controls.Add(this.impendingArrivalLabel);
			this.upperPanel.Controls.Add(this.impendingArrivalStatusTextBox);
			this.upperPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.upperPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.upperPanel.Name = "UpperPanel";
			this.upperPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 80, true);
			this.upperPanel.TabIndex = 3;
			// 
			// FlightNumberTextBox
			// 
			this.flightNumberTextBox.BindTo = "FlightNo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).FlightNoInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).FlightNo)));
			this.flightNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|7591b04b-a3d4-47c7-bff6-5951afa4b00b", "Flight No.");
			this.flightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 32, true);
			this.flightNumberTextBox.Name = "FlightNumberTextBox";
			this.flightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.flightNumberTextBox.TabIndex = 1;
			// 
			// FlightNumberLabel
			// 
			this.flightNumberLabel.AutoSize = true;
			this.flightNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.flightNumberLabel.Name = "FlightNumberLabel";
			this.flightNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.flightNumberLabel.TabIndex = 12;
			this.flightNumberLabel.Text = "Flight No.:";
			// 
			// ATDLabel
			// 
			this.aTDLabel.AutoSize = true;
			this.aTDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 32, true);
			this.aTDLabel.Name = "ATDLabel";
			this.aTDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 13, true);
			this.aTDLabel.TabIndex = 7;
			this.aTDLabel.Text = "ATD (UTC Time):";
			// 
			// ATDDateEdit
			// 
			this.aTDDateEdit.AutoCompleteMonthThreshold = 1;
			this.aTDDateEdit.AutoCompleteYear = true;
			this.aTDDateEdit.BindTo = "DateTimeOfDepartureUTC";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).DateTimeOfDepartureUTC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).DateTimeOfDepartureUTCInfo)));
			this.aTDDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|d4a09009-5fd7-465e-abd4-6a959d2bef96", "ATD(UTC Time)");
			this.aTDDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.aTDDateEdit.IsFixedReadOnly = false;
			this.aTDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 32, true);
			this.aTDDateEdit.Name = "ATDDateEdit";
			this.aTDDateEdit.TabIndex = 3;
			// 
			// FirstPortOfArrivalLabel
			// 
			this.firstPortOfArrivalLabel.AutoSize = true;
			this.firstPortOfArrivalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 8, true);
			this.firstPortOfArrivalLabel.Name = "FirstPortOfArrivalLabel";
			this.firstPortOfArrivalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
			this.firstPortOfArrivalLabel.TabIndex = 5;
			this.firstPortOfArrivalLabel.Text = "First Local Port:";
			// 
			// PortOfFirstArrivalTextBox
			// 
			this.portOfFirstArrivalTextBox.BindTo = "PortOfFirstArrival";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).PortOfFirstArrivalInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).PortOfFirstArrival)));
			this.portOfFirstArrivalTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|78cfd781-ccc2-44a7-bbb3-9feb62cc441e", "First Local Port");
			this.portOfFirstArrivalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 8, true);
			this.portOfFirstArrivalTextBox.Name = "PortOfFirstArrivalTextBox";
			this.portOfFirstArrivalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.portOfFirstArrivalTextBox.TabIndex = 4;
			// 
			// LastOverseasPortOfDepartureLabel
			// 
			this.lastOverseasPortOfDepartureLabel.AutoSize = true;
			this.lastOverseasPortOfDepartureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 8, true);
			this.lastOverseasPortOfDepartureLabel.Name = "LastOverseasPortOfDepartureLabel";
			this.lastOverseasPortOfDepartureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.lastOverseasPortOfDepartureLabel.TabIndex = 3;
			this.lastOverseasPortOfDepartureLabel.Text = "Last Overseas Port:";
			// 
			// LastOverseasPortOfDepartureTextBox
			// 
			this.lastOverseasPortOfDepartureTextBox.BindTo = "LastOverseasPortOfDeparture";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).LastOverseasPortOfDepartureInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Declaration.Business.CustomsJobVoyageWrapper)(null)).LastOverseasPortOfDeparture)));
			this.lastOverseasPortOfDepartureTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SailingPluginUserControl|428d6b33-6d7e-4eb4-b86e-180c005da832", "Last Overseas Port");
			this.lastOverseasPortOfDepartureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 8, true);
			this.lastOverseasPortOfDepartureTextBox.Name = "LastOverseasPortOfDepartureTextBox";
			this.lastOverseasPortOfDepartureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.lastOverseasPortOfDepartureTextBox.TabIndex = 2;
			// 
			// LowerPanel
			// 
			this.lowerPanel.Controls.Add(this.arrivalTabControl);
			this.lowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lowerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.lowerPanel.Name = "LowerPanel";
			this.lowerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 464, true);
			this.lowerPanel.TabIndex = 4;
			// 
			// SailingPluginUserControl
			// 
			this.Controls.Add(this.lowerPanel);
			this.Controls.Add(this.upperPanel);
			this.CaptionRenderingEnabled = false;
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CustomsJobVoyageWrapper";
			this.Name = "SailingPluginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 544, true);
			this.arrivalTabControl.ResumeLayout(false);
			this.arrivalPortsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.arrivalsGrid)).EndInit();
			this.actualArrivalsTabControl.ResumeLayout(false);
			this.actualArrivalMessagesTabPage.ResumeLayout(false);
			this.impendingMessagesTabPage.ResumeLayout(false);
			this.upperPanel.ResumeLayout(false);
			this.upperPanel.PerformLayout();
			this.lowerPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		ZArchitecture.ZGrid arrivalsGrid;
		ZArchitecture.ZTextBox impendingArrivalStatusTextBox;
		ZArchitecture.ZLabel impendingArrivalLabel;
		ZTemplateTabControl arrivalTabControl;
		ZTabPage arrivalPortsTabPage;
		ZTabPage impendingMessagesTabPage;
		Messaging.GUI.EDIMessageUserControl ediMessageUserControl1;
		ZTemplateTabControl actualArrivalsTabControl;
		ZTabPage actualArrivalMessagesTabPage;
		ZPanel upperPanel;
		ZPanel lowerPanel;
		ZArchitecture.ZTextBox lastOverseasPortOfDepartureTextBox;
		ZArchitecture.ZLabel lastOverseasPortOfDepartureLabel;
		ZArchitecture.ZTextBox portOfFirstArrivalTextBox;
		ZArchitecture.ZLabel firstPortOfArrivalLabel;
		ZArchitecture.ZLabel aTDLabel;
		ZDateEdit aTDDateEdit;
		ZArchitecture.ZLabel flightNumberLabel;
		ZArchitecture.ZTextBox flightNumberTextBox;
		internal Messaging.GUI.EDIMessageUserControl actualArrivalMessageUserControl;
	}
}
