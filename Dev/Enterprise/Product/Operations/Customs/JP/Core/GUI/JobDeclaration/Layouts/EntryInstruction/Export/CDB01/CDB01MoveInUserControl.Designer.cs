namespace Enterprise.Customs.JP.GUI
{
	partial class CDB01MoveInUserControl
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
			this.MoveInDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MoveInGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MoveInWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MoveInQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MoveInNoticeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MoveInDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MoveInDateDateEdit.SuspendLayout();
			this.MoveInGroupBox.SuspendLayout();
			this.MoveInDestinationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// MoveInDateDateEdit
			// 
			this.MoveInDateDateEdit.AllowDrop = true;
			this.MoveInDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.MoveInDateDateEdit, "MoveInDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).MoveInDate)));
			this.MoveInDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 19, true);
			this.MoveInDateDateEdit.Name = "MoveInDateDateEdit";
			this.MoveInDateDateEdit.TabIndex = 0;
			// 
			// MoveInGroupBox
			// 
			this.MoveInGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("0fa37f4e-f0cb-4217-a0db-24a099e4fa37", "Move In");
			this.MoveInGroupBox.Controls.Add(this.MoveInWeightCalcEdit);
			this.MoveInGroupBox.Controls.Add(this.MoveInQuantityCalcEdit);
			this.MoveInGroupBox.Controls.Add(this.MoveInNoticeTextBox);
			this.MoveInGroupBox.Controls.Add(this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox);
			this.MoveInGroupBox.Controls.Add(this.MoveInDestinationCodeFindBox);
			this.MoveInGroupBox.Controls.Add(this.MoveInDateDateEdit);
			this.MoveInGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MoveInGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MoveInGroupBox.Name = "MoveInGroupBox";
			this.MoveInGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 188, true);
			this.MoveInGroupBox.TabIndex = 1;
			this.MoveInGroupBox.TabStop = false;
			// 
			// MoveInWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MoveInWeightCalcEdit, "MoveInWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).MoveInWeight)));
			this.MoveInWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 153, true);
			this.MoveInWeightCalcEdit.Name = "MoveInWeightCalcEdit";
			this.MoveInWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.MoveInWeightCalcEdit.TabIndex = 10;
			// 
			// MoveInQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MoveInQuantityCalcEdit, "MoveInQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).MoveInQuantity)));
			this.MoveInQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 127, true);
			this.MoveInQuantityCalcEdit.Name = "MoveInQuantityCalcEdit";
			this.MoveInQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.MoveInQuantityCalcEdit.TabIndex = 8;
			// 
			// MoveInNoticeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MoveInNoticeTextBox, "MoveInNotice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).MoveInNotice)));
			this.MoveInNoticeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 101, true);
			this.MoveInNoticeTextBox.Name = "MoveInNoticeTextBox";
			this.MoveInNoticeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.MoveInNoticeTextBox.TabIndex = 6;
			// 
			// IsMoveInNoticeEntryNumSystemGeneratedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox, "RequestMoveInNotice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).RequestMoveInNotice)));
			this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 71, true);
			this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox.Name = "IsMoveInNoticeEntryNumSystemGeneratedCheckBox";
			this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 24, true);
			this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox.TabIndex = 4;
			this.IsMoveInNoticeEntryNumSystemGeneratedCheckBox.UseVisualStyleBackColor = true;
			// 
			// MoveInDestinationCodeFindBox
			// 
			this.MoveInDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MoveInDestinationCodeFindBox, "MoveInDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).MoveInDestination)));
			this.MoveInDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 45, true);
			this.MoveInDestinationCodeFindBox.Name = "MoveInDestinationCodeFindBox";
			this.MoveInDestinationCodeFindBox.ParentType = null;
			this.MoveInDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.MoveInDestinationCodeFindBox.TabIndex = 2;
			// 
			// CDB01MoveInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MoveInGroupBox);
			this.Name = "CDB01MoveInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 188, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MoveInDateDateEdit.ResumeLayout(true);
			this.MoveInDateDateEdit.PerformLayout();
			this.MoveInGroupBox.ResumeLayout(false);
			this.MoveInGroupBox.PerformLayout();
			this.MoveInDestinationCodeFindBox.ResumeLayout(true);
			this.MoveInDestinationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit MoveInDateDateEdit;
		private ZArchitecture.GUI.ZGroupBox MoveInGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsMoveInNoticeEntryNumSystemGeneratedCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox MoveInDestinationCodeFindBox;
		private ZArchitecture.ZTextBox MoveInNoticeTextBox;
		private ZArchitecture.ZCalcEdit MoveInQuantityCalcEdit;
		private ZArchitecture.ZCalcEdit MoveInWeightCalcEdit;
	}
}
