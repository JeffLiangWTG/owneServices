
namespace Enterprise.Client.DHL.GUI
{
	partial class FlightBulkUpdateForm
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
		protected override void InitializeComponent()
		{
			this.ClosezButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdatezButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GroupBoxNewFlightDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterBillControlMAWB = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.DateEditEDI = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateEditATA = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateEditATD = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TextBoxFlightNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.LabelEDI = new Enterprise.ZArchitecture.ZLabel();
			this.LabelATA = new Enterprise.ZArchitecture.ZLabel();
			this.LabelATD = new Enterprise.ZArchitecture.ZLabel();
			this.LabelFlightNumber = new Enterprise.ZArchitecture.ZLabel();
			this.LabelMAWB = new Enterprise.ZArchitecture.ZLabel();
			this.LabelExistingMAWB = new Enterprise.ZArchitecture.ZLabel();
			this.MasterBillControlExistingMAWB = new Enterprise.ZArchitecture.ZMasterBillControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBoxNewFlightDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 243, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject);
			// 
			// ClosezButton
			// 
			this.ClosezButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ClosezButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ClosezButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 214, true);
			this.ClosezButton.Name = "ClosezButton";
			this.ClosezButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClosezButton.TabIndex = 4;
			this.ClosezButton.Text = "Close";
			this.ClosezButton.UseVisualStyleBackColor = true;
			// 
			// UpdatezButton
			// 
			this.UpdatezButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.UpdatezButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 214, true);
			this.UpdatezButton.Name = "UpdatezButton";
			this.UpdatezButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdatezButton.TabIndex = 3;
			this.UpdatezButton.Text = "Update";
			this.UpdatezButton.UseVisualStyleBackColor = true;
			this.UpdatezButton.Click += new System.EventHandler(this.Update_Click);
			// 
			// GroupBoxNewFlightDetails
			// 
			this.GroupBoxNewFlightDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.GroupBoxNewFlightDetails.Controls.Add(this.MasterBillControlMAWB);
			this.GroupBoxNewFlightDetails.Controls.Add(this.DateEditEDI);
			this.GroupBoxNewFlightDetails.Controls.Add(this.DateEditATA);
			this.GroupBoxNewFlightDetails.Controls.Add(this.DateEditATD);
			this.GroupBoxNewFlightDetails.Controls.Add(this.TextBoxFlightNumber);
			this.GroupBoxNewFlightDetails.Controls.Add(this.LabelEDI);
			this.GroupBoxNewFlightDetails.Controls.Add(this.LabelATA);
			this.GroupBoxNewFlightDetails.Controls.Add(this.LabelATD);
			this.GroupBoxNewFlightDetails.Controls.Add(this.LabelFlightNumber);
			this.GroupBoxNewFlightDetails.Controls.Add(this.LabelMAWB);
			this.GroupBoxNewFlightDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 49, true);
			this.GroupBoxNewFlightDetails.Name = "GroupBoxNewFlightDetails";
			this.GroupBoxNewFlightDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 159, true);
			this.GroupBoxNewFlightDetails.TabIndex = 2;
			this.GroupBoxNewFlightDetails.TabStop = false;
			this.GroupBoxNewFlightDetails.Text = "New Flight Details";
			// 
			// MasterBillControlMAWB
			// 
			this.MasterBillControlMAWB.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MasterBillControlMAWB, "MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject)(null)).MAWB)));
			this.MasterBillControlMAWB.FormattedMasterBill = "";
			this.MasterBillControlMAWB.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 23, true);
			this.MasterBillControlMAWB.Name = "MasterBillControlMAWB";
			this.MasterBillControlMAWB.ReadOnly = false;
			this.MasterBillControlMAWB.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.MasterBillControlMAWB.TabIndex = 1;
			// 
			// DateEditEDI
			// 
			this.DateEditEDI.AutoCompleteMonthThreshold = 1;
			this.DateEditEDI.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditEDI, "EDITransmitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject)(null)).EDITransmitDate)));
			this.DateEditEDI.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 131, true);
			this.DateEditEDI.Name = "DateEditEDI";
			this.DateEditEDI.TabIndex = 9;
			// 
			// DateEditATA
			// 
			this.DateEditATA.AutoCompleteMonthThreshold = 1;
			this.DateEditATA.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditATA, "ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject)(null)).ArrivalDate)));
			this.DateEditATA.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 103, true);
			this.DateEditATA.Name = "DateEditATA";
			this.DateEditATA.TabIndex = 7;
			// 
			// DateEditATD
			// 
			this.DateEditATD.AutoCompleteMonthThreshold = 1;
			this.DateEditATD.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditATD, "DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject)(null)).DepartureDate)));
			this.DateEditATD.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 77, true);
			this.DateEditATD.Name = "DateEditATD";
			this.DateEditATD.TabIndex = 5;
			// 
			// TextBoxFlightNumber
			// 
			this.BindingSource.SetBindingMember(this.TextBoxFlightNumber, "FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject)(null)).FlightNo)));
			this.TextBoxFlightNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 50, true);
			this.TextBoxFlightNumber.Name = "TextBoxFlightNumber";
			this.TextBoxFlightNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.TextBoxFlightNumber.TabIndex = 3;
			// 
			// LabelEDI
			// 
			this.LabelEDI.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 134, true);
			this.LabelEDI.Name = "LabelEDI";
			this.LabelEDI.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.LabelEDI.TabIndex = 8;
			this.LabelEDI.Text = "EDI Transmit Date:";
			// 
			// LabelATA
			// 
			this.LabelATA.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 107, true);
			this.LabelATA.Name = "LabelATA";
			this.LabelATA.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 13, true);
			this.LabelATA.TabIndex = 6;
			this.LabelATA.Text = "Arrival Date:";
			// 
			// LabelATD
			// 
			this.LabelATD.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 80, true);
			this.LabelATD.Name = "LabelATD";
			this.LabelATD.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.LabelATD.TabIndex = 4;
			this.LabelATD.Text = "Delivery Date:";
			// 
			// LabelFlightNumber
			// 
			this.LabelFlightNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 53, true);
			this.LabelFlightNumber.Name = "LabelFlightNumber";
			this.LabelFlightNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 13, true);
			this.LabelFlightNumber.TabIndex = 2;
			this.LabelFlightNumber.Text = "Flight Number:";
			// 
			// LabelMAWB
			// 
			this.LabelMAWB.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 26, true);
			this.LabelMAWB.Name = "LabelMAWB";
			this.LabelMAWB.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 13, true);
			this.LabelMAWB.TabIndex = 0;
			this.LabelMAWB.Text = "MAWB:";
			// 
			// LabelExistingMAWB
			// 
			this.LabelExistingMAWB.IsFontBold = true;
			this.LabelExistingMAWB.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 18, true);
			this.LabelExistingMAWB.Name = "LabelExistingMAWB";
			this.LabelExistingMAWB.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.LabelExistingMAWB.TabIndex = 0;
			this.LabelExistingMAWB.Text = "Existing MAWB:";
			// 
			// MasterBillControlExistingMAWB
			// 
			this.MasterBillControlExistingMAWB.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MasterBillControlExistingMAWB, "ExistingMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject)(null)).ExistingMAWB)));
			this.MasterBillControlExistingMAWB.FormattedMasterBill = "";
			this.MasterBillControlExistingMAWB.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 15, true);
			this.MasterBillControlExistingMAWB.Name = "MasterBillControlExistingMAWB";
			this.MasterBillControlExistingMAWB.ReadOnly = false;
			this.MasterBillControlExistingMAWB.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 20, true);
			this.MasterBillControlExistingMAWB.TabIndex = 1;
			// 
			// FlightBulkUpdateForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 267, true);
			this.Controls.Add(this.GroupBoxNewFlightDetails);
			this.Controls.Add(this.LabelExistingMAWB);
			this.Controls.Add(this.ClosezButton);
			this.Controls.Add(this.UpdatezButton);
			this.Controls.Add(this.MasterBillControlExistingMAWB);
			this.DataSourceAssemblyName = "ZClientDHL";
			this.DataSourceType = typeof(Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject);
			this.DataSourceTypeName = "Enterprise.Client.DHL.Business.FlightBulkUpdateBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FlightBulkUpdateForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Flight Bulk Update";
			this.Controls.SetChildIndex(this.MasterBillControlExistingMAWB, 0);
			this.Controls.SetChildIndex(this.UpdatezButton, 0);
			this.Controls.SetChildIndex(this.ClosezButton, 0);
			this.Controls.SetChildIndex(this.LabelExistingMAWB, 0);
			this.Controls.SetChildIndex(this.GroupBoxNewFlightDetails, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBoxNewFlightDetails.ResumeLayout(false);
			this.GroupBoxNewFlightDetails.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton ClosezButton;
		private Enterprise.ZArchitecture.GUI.ZButton UpdatezButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox GroupBoxNewFlightDetails;
		private Enterprise.ZArchitecture.ZLabel LabelExistingMAWB;
		private Enterprise.ZArchitecture.ZLabel LabelEDI;
		private Enterprise.ZArchitecture.ZLabel LabelATA;
		private Enterprise.ZArchitecture.ZLabel LabelATD;
		private Enterprise.ZArchitecture.ZLabel LabelFlightNumber;
		private Enterprise.ZArchitecture.ZLabel LabelMAWB;
		private Enterprise.ZArchitecture.ZTextBox TextBoxFlightNumber;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateEditEDI;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateEditATA;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateEditATD;
		private Enterprise.ZArchitecture.ZMasterBillControl MasterBillControlMAWB;
		private Enterprise.ZArchitecture.ZMasterBillControl MasterBillControlExistingMAWB;
	}
}
