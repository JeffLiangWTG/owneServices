using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CheckInAllChildPiecesUserControl
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
			this.ButtonOk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContainerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SealTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsDamagedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsBeingReleasedNowCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReceivedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ShedStorageLocationZGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PackagesUnitsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReceivedDateDateEdit.SuspendLayout();
			this.ShedStorageLocationZGuidFindBox.SuspendLayout();
			this.PackagesUnitsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces);
			// 
			// ButtonOk
			// 
			this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOk.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("D04EF691-9952-461C-B598-138D8009C4C6", "OK");
			this.ButtonOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 293, true);
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ButtonOk.TabIndex = 9;
			this.ButtonOk.ToolTipCaption = null;
			this.ButtonOk.UseVisualStyleBackColor = true;
			this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("39CDF5E4-8562-43ED-B26D-1F3D799F9F8B", "Cancel");
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 293, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ButtonCancel.TabIndex = 10;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// ContainerTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContainerTextBox, "ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).ContainerNumber)));
			this.ContainerTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("61E90EF1-B644-4947-981B-57C5FCFF661A", "Container Number");
			this.ContainerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 174, true);
			this.ContainerTextBox.Name = "ContainerTextBox";
			this.ContainerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.ContainerTextBox.TabIndex = 6;
			// 
			// SealTextBox
			// 
			this.BindingSource.SetBindingMember(this.SealTextBox, "ContainerSeal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).ContainerSeal)));
			this.SealTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7CD352E4-7594-4C02-8C30-F02CBAF7B3D6", "Container Seal");
			this.SealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 200, true);
			this.SealTextBox.Name = "SealTextBox";
			this.SealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.SealTextBox.TabIndex = 7;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("EC9BDB93-5665-49DC-B737-FEF4375B4D15", "Goods\' Description");
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 148, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 5;
			// 
			// IsDamagedCheckBox
			// 
			this.IsDamagedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDamagedCheckBox, "IsDamaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).IsDamaged)));
			this.IsDamagedCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("209DD33C-9D85-436D-BAA4-F7743ABEF8B6", "Is Damaged?");
			this.IsDamagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 226, true);
			this.IsDamagedCheckBox.Name = "IsDamagedCheckBox";
			this.IsDamagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
			this.IsDamagedCheckBox.TabIndex = 8;
			this.IsDamagedCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsBeingReleasedNowCheckBox
			// 
			this.IsBeingReleasedNowCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsBeingReleasedNowCheckBox, "IsBeingReleasedNow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).IsBeingReleasedNow)));
			this.IsBeingReleasedNowCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("70F18504-308E-4511-A2B3-1B11265DBFD8", "Release Now?");
			this.IsBeingReleasedNowCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 125, true);
			this.IsBeingReleasedNowCheckBox.Name = "IsBeingReleasedNowCheckBox";
			this.IsBeingReleasedNowCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.IsBeingReleasedNowCheckBox.TabIndex = 4;
			this.IsBeingReleasedNowCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReceivedDateDateEdit
			// 
			this.ReceivedDateDateEdit.AllowDrop = true;
			this.ReceivedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReceivedDateDateEdit, "ReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).ReceivedDate)));
			this.ReceivedDateDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("14E18E31-3CBD-414B-8781-59BA2CD25CD2", "Received Date");
			this.ReceivedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 47, true);
			this.ReceivedDateDateEdit.Name = "ReceivedDateDateEdit";
			this.ReceivedDateDateEdit.TabIndex = 1;
			// 
			// ShedStorageLocationZGuidFindBox
			// 
			this.ShedStorageLocationZGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShedStorageLocationZGuidFindBox, "ShedStorageLocationId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).ShedStorageLocationId)));
			this.ShedStorageLocationZGuidFindBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("F0AC60C4-50A6-4C1E-8310-6C0EE63F7260", "Shed Storage Location");
			this.ShedStorageLocationZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 99, true);
			this.ShedStorageLocationZGuidFindBox.Name = "ShedStorageLocationZGuidFindBox";
			this.ShedStorageLocationZGuidFindBox.ParentType = null;
			this.ShedStorageLocationZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 20, true);
			this.ShedStorageLocationZGuidFindBox.TabIndex = 3;
			// 
			// PackagesUnitsDropEdit
			// 
			this.PackagesUnitsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesUnitsDropEdit, "PackagesUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).PackagesUnits)));
			this.PackagesUnitsDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8B653660-61F2-45C2-9E98-EA18A48DEE43", "Packages");
			this.PackagesUnitsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 21, true);
			this.PackagesUnitsDropEdit.Name = "PackagesUnitsDropEdit";
			this.PackagesUnitsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 20, true);
			this.PackagesUnitsDropEdit.TabIndex = 0;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentCheckInAllChildPieces)(null)).MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("86AB9CA5-5176-431D-A195-60A737594B0E", "Marks and Numbers");
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 73, true);
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 20, true);
			this.MarksAndNumbersTextBox.TabIndex = 2;
			// 
			// CheckInAllChildPiecesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainerTextBox);
			this.Controls.Add(this.SealTextBox);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.IsDamagedCheckBox);
			this.Controls.Add(this.IsBeingReleasedNowCheckBox);
			this.Controls.Add(this.ReceivedDateDateEdit);
			this.Controls.Add(this.ShedStorageLocationZGuidFindBox);
			this.Controls.Add(this.PackagesUnitsDropEdit);
			this.Controls.Add(this.MarksAndNumbersTextBox);
			this.Controls.Add(this.ButtonOk);
			this.Controls.Add(this.ButtonCancel);
			this.Name = "CheckInAllChildPiecesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 325, true);
			this.Load += new System.EventHandler(this.CheckInAllChildPiecesUserControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReceivedDateDateEdit.ResumeLayout(true);
			this.ReceivedDateDateEdit.PerformLayout();
			this.ShedStorageLocationZGuidFindBox.ResumeLayout(true);
			this.ShedStorageLocationZGuidFindBox.PerformLayout();
			this.PackagesUnitsDropEdit.ResumeLayout(true);
			this.PackagesUnitsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton ButtonOk;
		private ZArchitecture.GUI.ZButton ButtonCancel;
		private ZArchitecture.ZTextBox ContainerTextBox;
		private ZArchitecture.ZTextBox SealTextBox;
		private ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		private ZCheckBox IsDamagedCheckBox;
		private ZCheckBox IsBeingReleasedNowCheckBox;
		private ZDateEdit ReceivedDateDateEdit;
		private ZGuidFindBox ShedStorageLocationZGuidFindBox;
		private ZDropEdit PackagesUnitsDropEdit;
		private ZArchitecture.ZTextBox MarksAndNumbersTextBox;
	}
}
