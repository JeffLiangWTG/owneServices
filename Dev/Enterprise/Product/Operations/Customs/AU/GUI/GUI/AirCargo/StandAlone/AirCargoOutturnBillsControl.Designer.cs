using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoOutturnBillsControl
	{
		private void InitializeComponent()
		{
			this.outturnMessagesControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.outturnMessagesSplitter = new CargoWise.Windows.UI.KSplitter();
			this.mAWBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.mAWBLabel = new Enterprise.ZArchitecture.ZLabel();
			this.underbondFlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.flightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.underbondArrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.underbondArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.cargoMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cargoMessagesControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.outturnMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.outturnMessagesUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.DetailsTabControl.SuspendLayout();
			this.UnderbondDetailsPanel.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.OutturnTabPage.SuspendLayout();
			this.cargoMessagesTabPage.SuspendLayout();
			this.outturnMessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessageStatusLabel
			// 
			this.MessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 98, true);
			this.MessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.MessageStatusLabel.Text = "Underbond Status:";
			// 
			// CustomsStatusLabel
			// 
			this.CustomsStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 121, true);
			this.CustomsStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.CustomsStatusLabel.Text = "Cargo Status:";
			// 
			// UnderbondDetailsPanel
			// 
			this.UnderbondDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 488, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.cargoMessagesTabPage);
			this.MainTabControl.Controls.Add(this.outturnMessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 488, true);
			this.MainTabControl.Controls.SetChildIndex(this.outturnMessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.cargoMessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.OutturnTabPage, 0);
			// 
			// RequestReasonLabel
			// 
			this.RequestReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 29, true);
			// 
			// UnderbondForLabel
			// 
			this.UnderbondForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 6, true);
			// 
			// ModeOfMovementLabel
			// 
			this.ModeOfMovementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 52, true);
			// 
			// ReferenceLabel
			// 
			this.ReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 75, true);
			// 
			// OutturnTabPage
			// 
			this.OutturnTabPage.Controls.Add(this.underbondArrivalDateEdit);
			this.OutturnTabPage.Controls.Add(this.underbondArrivalDateLabel);
			this.OutturnTabPage.Controls.Add(this.underbondFlightNoTextBox);
			this.OutturnTabPage.Controls.Add(this.flightLabel);
			this.OutturnTabPage.Controls.Add(this.mAWBLabel);
			this.OutturnTabPage.Controls.Add(this.mAWBNumberTextBox);
			this.OutturnTabPage.Controls.Add(this.outturnMessagesSplitter);
			this.OutturnTabPage.Controls.Add(this.outturnMessagesControl);
			this.OutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 461, true);
			this.OutturnTabPage.Resize += new System.EventHandler(this.OutturnTabPage_Resize);
			this.OutturnTabPage.Controls.SetChildIndex(this.outturnMessagesControl, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.outturnMessagesSplitter, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.OutturnUserControl, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.mAWBNumberTextBox, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.mAWBLabel, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.flightLabel, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.underbondFlightNoTextBox, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.underbondArrivalDateLabel, 0);
			this.OutturnTabPage.Controls.SetChildIndex(this.underbondArrivalDateEdit, 0);
			// 
			// OutturnUserControl
			// 
			this.OutturnUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 309, true);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 461, true);
			// 
			// OutturnMessagesControl
			// 
			this.outturnMessagesControl.AllowDrop = true;
			this.outturnMessagesControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.outturnMessagesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 317, true);
			this.outturnMessagesControl.Name = "OutturnMessagesControl";
			this.outturnMessagesControl.ShowChangingBlueMessageHeading = false;
			this.outturnMessagesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 144, true);
			this.outturnMessagesControl.TabIndex = 12;
			// 
			// OutturnMessagesSplitter
			// 
			this.outturnMessagesSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.outturnMessagesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 309, true);
			this.outturnMessagesSplitter.Name = "OutturnMessagesSplitter";
			this.outturnMessagesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 8, true);
			this.outturnMessagesSplitter.TabIndex = 13;
			this.outturnMessagesSplitter.TabStop = false;
			// 
			// MAWBNumberTextBox
			// 
			this.mAWBNumberTextBox.BindTo = "C4_MAWB";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).C4_MAWBInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).C4_MAWB)));
			this.mAWBNumberTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.mAWBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 5, true);
			this.mAWBNumberTextBox.Name = "MAWBNumberTextBox";
			this.mAWBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.mAWBNumberTextBox.TabIndex = 14;
			// 
			// MAWBLabel
			// 
			this.mAWBLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 8, true);
			this.mAWBLabel.Name = "MAWBLabel";
			this.mAWBLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.mAWBLabel.TabIndex = 15;
			this.mAWBLabel.Text = "MAWB:";
			// 
			// UnderbondFlightNoTextBox
			// 
			this.underbondFlightNoTextBox.BindTo = "C4_FlightNo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).C4_FlightNoInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).C4_FlightNo)));
			this.underbondFlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 5, true);
			this.underbondFlightNoTextBox.Name = "UnderbondFlightNoTextBox";
			this.underbondFlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.underbondFlightNoTextBox.TabIndex = 16;
			// 
			// FlightLabel
			// 
			this.flightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 8, true);
			this.flightLabel.Name = "FlightLabel";
			this.flightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 16, true);
			this.flightLabel.TabIndex = 17;
			this.flightLabel.Text = "Flight:";
			// 
			// UnderbondArrivalDateLabel
			// 
			this.underbondArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(832, 8, true);
			this.underbondArrivalDateLabel.Name = "UnderbondArrivalDateLabel";
			this.underbondArrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
			this.underbondArrivalDateLabel.TabIndex = 18;
			this.underbondArrivalDateLabel.Text = "Arrival:";
			// 
			// UnderbondArrivalDateEdit
			// 
			this.underbondArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.underbondArrivalDateEdit.AutoCompleteYear = true;
			this.underbondArrivalDateEdit.BindTo = "C4_ArrivalDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).C4_ArrivalDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CusUnderbond)(null)).C4_ArrivalDateInfo)));
			this.underbondArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.underbondArrivalDateEdit.IsFixedReadOnly = false;
			this.underbondArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(872, 5, true);
			this.underbondArrivalDateEdit.Name = "UnderbondArrivalDateEdit";
			this.underbondArrivalDateEdit.TabIndex = 19;
			// 
			// CargoMessagesTabPage
			// 
			this.cargoMessagesTabPage.AllowDrop = true;
			this.cargoMessagesTabPage.CheckForNotifications = true;
			this.cargoMessagesTabPage.Controls.Add(this.cargoMessagesControl);
			this.cargoMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.cargoMessagesTabPage.Name = "CargoMessagesTabPage";
			this.cargoMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.cargoMessagesTabPage.TabIndex = 3;
			this.cargoMessagesTabPage.Text = "Cargo Messages";
			// 
			// CargoMessagesControl
			// 
			this.cargoMessagesControl.AllowDrop = true;
			this.cargoMessagesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cargoMessagesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cargoMessagesControl.Name = "CargoMessagesControl";
			this.cargoMessagesControl.ShowChangingBlueMessageHeading = false;
			this.cargoMessagesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.cargoMessagesControl.TabIndex = 0;
			// 
			// OutturnMessagesTabPage
			// 
			this.outturnMessagesTabPage.AllowDrop = true;
			this.outturnMessagesTabPage.CheckForNotifications = true;
			this.outturnMessagesTabPage.Controls.Add(this.outturnMessagesUserControl);
			this.outturnMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.outturnMessagesTabPage.Name = "OutturnMessagesTabPage";
			this.outturnMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.outturnMessagesTabPage.TabIndex = 5;
			this.outturnMessagesTabPage.Text = "Outturn Messages";
			// 
			// OutturnMessagesUserControl
			// 
			this.outturnMessagesUserControl.AllowDrop = true;
			this.outturnMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outturnMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.outturnMessagesUserControl.Name = "OutturnMessagesUserControl";
			this.outturnMessagesUserControl.ShowChangingBlueMessageHeading = false;
			this.outturnMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 144, true);
			this.outturnMessagesUserControl.TabIndex = 1;
			// 
			// AirCargoOutturnBillsControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusUnderbond";
			this.Name = "AirCargoOutturnBillsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 488, true);
			this.DetailsTabControl.ResumeLayout(false);
			this.UnderbondDetailsPanel.ResumeLayout(false);
			this.MainTabControl.ResumeLayout(false);
			this.OutturnTabPage.ResumeLayout(false);
			this.OutturnTabPage.PerformLayout();
			this.cargoMessagesTabPage.ResumeLayout(false);
			this.outturnMessagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private Messaging.GUI.EDIMessageUserControl outturnMessagesControl;
		private CargoWise.Windows.UI.KSplitter outturnMessagesSplitter;
		private ZArchitecture.ZTextBox mAWBNumberTextBox;
		private ZArchitecture.ZLabel mAWBLabel;
		private ZArchitecture.ZTextBox underbondFlightNoTextBox;
		private ZArchitecture.ZLabel underbondArrivalDateLabel;
		private ZDateEdit underbondArrivalDateEdit;
		private ZArchitecture.ZLabel flightLabel;
		private ZTabPage cargoMessagesTabPage;
		internal Messaging.GUI.EDIMessageUserControl cargoMessagesControl;
		private ZTabPage outturnMessagesTabPage;
		internal Messaging.GUI.EDIMessageUserControl outturnMessagesUserControl;
	}
}
