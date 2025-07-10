using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukMasterControl
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
			this.MasterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterBillControl = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.DateOfArrival = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Flight = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MasterGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB);
			// 
			// MasterGroupBox
			//
			this.MasterGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a164bfee-196f-44c5-9a9c-3c9f17ca7a24", "Master");
			this.MasterGroupBox.Controls.Add(this.MasterBillControl);
			this.MasterGroupBox.Controls.Add(this.DateOfArrival);
			this.MasterGroupBox.Controls.Add(this.Flight);
			this.LabelCaptionRenderProvider.SetLabelTop(this.MasterGroupBox, 0);
			this.MasterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MasterGroupBox.Name = "MasterGroupBox";
			this.MasterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 91, true);
			this.MasterGroupBox.TabIndex = 3;
			this.MasterGroupBox.TabStop = false;
			// 
			// MasterBillControl
			// 
			this.MasterBillControl.AllowDrop = true;
			this.MasterBillControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MasterBillControl, "MAWB.CM_MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).MAWB.CM_MAWB)));
			this.MasterBillControl.FormattedMasterBill = "";
			this.MasterBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 16, true);
			this.MasterBillControl.Name = "MasterBillControl";
			this.MasterBillControl.ReadOnly = false;
			this.MasterBillControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.MasterBillControl.TabIndex = 0;
			this.MasterBillControl.AllowAlphaInMAWP = true;
			// 
			// DateOfArrival
			// 
			this.DateOfArrival.AllowDrop = true;
			this.DateOfArrival.AutoCompleteMonthThreshold = 1;
			this.DateOfArrival.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfArrival, "MAWB.CM_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).MAWB.CM_ArrivalDate)));
			this.DateOfArrival.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 68, true);
			this.DateOfArrival.Name = "DateOfArrival";
			this.DateOfArrival.TabIndex = 2;
			// 
			// Flight
			// 
			this.BindingSource.SetBindingMember(this.Flight, "MAWB.CM_FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).MAWB.CM_FlightNo)));
			this.Flight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 42, true);
			this.Flight.Name = "Flight";
			this.Flight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.Flight.TabIndex = 1;
			// 
			// CcsukMasterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MasterGroupBox);
			this.Name = "CcsukMasterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 96, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MasterGroupBox.ResumeLayout(false);
			this.MasterGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MasterGroupBox;
		private ZArchitecture.ZMasterBillControl MasterBillControl;
		private ZArchitecture.GUI.ZDateEdit DateOfArrival;
		private ZArchitecture.ZTextBox Flight;
	}
}
