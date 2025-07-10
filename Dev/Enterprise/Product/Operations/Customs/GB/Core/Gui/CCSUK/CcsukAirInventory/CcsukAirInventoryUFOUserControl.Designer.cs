using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukAirInventoryUFOUserControl
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
		void InitializeComponent()
		{ 
			Enterprise.ZArchitecture.GUI.ZGuidFindBox SSL;
			this.ButtonGenerate = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MawbTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PimaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.numberOfPiecesReceivedControl = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.arrivalDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			SSL = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numberOfPiecesReceivedControl)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB);
			// 
			// ButtonGenerate
			//
			this.ButtonGenerate.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("34e2e34b-a23c-4dc1-aec1-784f62c712e7", "Send UFO FRI");
			this.ButtonGenerate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 86, true);
			this.ButtonGenerate.Name = "ButtonGenerate";
			this.ButtonGenerate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
			this.ButtonGenerate.TabIndex = 6;
			this.ButtonGenerate.UseVisualStyleBackColor = true;
			this.ButtonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
			// 
			// MawbTextBox
			// 
			this.BindingSource.SetBindingMember(this.MawbTextBox, "CM_MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).CM_MAWB)));
			this.MawbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 33, true);
			this.MawbTextBox.Name = "MawbTextBox";
			this.MawbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MawbTextBox.TabIndex = 1;
			// 
			// FlightNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNoTextBox, "CM_FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).CM_FlightNo)));
			this.FlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 33, true);
			this.FlightNoTextBox.Name = "FlightNoTextBox";
			this.FlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.FlightNoTextBox.TabIndex = 2;
			// 
			// PimaDropEdit
			// 
			this.PimaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PimaDropEdit, "Profile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).Profile)));
			this.PimaDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirInventoryUFOUserControl|951dcdfe-b2df-41f6-8c59-8e6ceead5dc0", "PIMA");
			this.PimaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 7, true);
			this.PimaDropEdit.Name = "PimaDropEdit";
			this.PimaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.PimaDropEdit.TabIndex = 0;
			// 
			// numberOfPiecesReceivedControl
			// 
			this.BindingSource.SetBindingMember(this.numberOfPiecesReceivedControl, "UfoPiecesReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).UfoPiecesReceived)));
			this.numberOfPiecesReceivedControl.BindTo = "UfoPiecesReceived";
			this.numberOfPiecesReceivedControl.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("1456f04c-07c8-4846-bc19-93fae20cf197", "NPR");
			this.numberOfPiecesReceivedControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 63, true);
			this.numberOfPiecesReceivedControl.Name = "numberOfPiecesReceivedControl";
			this.numberOfPiecesReceivedControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.numberOfPiecesReceivedControl.TabIndex = 3;
			// 
			// arrivalDate
			// 
			this.arrivalDate.AllowDrop = true;
			this.arrivalDate.AutoCompleteMonthThreshold = 1;
			this.arrivalDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.arrivalDate, "CM_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).CM_ArrivalDate)));
			this.arrivalDate.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("dcd7a23f-82ac-4d32-8b5d-37574978f481", "Arr. Date", "Arrival Date", "");
			this.arrivalDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 88, true);
			this.arrivalDate.Name = "arrivalDate";
			this.arrivalDate.TabIndex = 5;
			// 
			// SSL
			// 
			SSL.AllowDrop = true;
			this.BindingSource.SetBindingMember(SSL, "UfoShedStorageLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(null)).UfoShedStorageLocation)));
			SSL.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukAirInventoryUFOUserControl|12345678-b2df-41f6-8c59-8e6ceead5dc0", "", "Stored In", "SSL", "Shed storage location.");
			SSL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 62, true);
			SSL.Name = "SSL";
			SSL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			SSL.TabIndex = 4;
			SSL.ShowDescriptionBox = false;
			// 
			// CcsukAirInventoryUFOUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(SSL);
			this.Controls.Add(this.arrivalDate);
			this.Controls.Add(this.numberOfPiecesReceivedControl);
			this.Controls.Add(this.PimaDropEdit);
			this.Controls.Add(this.FlightNoTextBox);
			this.Controls.Add(this.MawbTextBox);
			this.Controls.Add(this.ButtonGenerate);
			this.Name = "CcsukAirInventoryUFOUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 118, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numberOfPiecesReceivedControl)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton ButtonGenerate;
		private ZArchitecture.ZTextBox MawbTextBox;
		private ZArchitecture.ZTextBox FlightNoTextBox;
		private ZArchitecture.GUI.ZDropEdit PimaDropEdit;
		private ZNumericUpDown numberOfPiecesReceivedControl;
		private ZDateEdit arrivalDate; 
	}
}
