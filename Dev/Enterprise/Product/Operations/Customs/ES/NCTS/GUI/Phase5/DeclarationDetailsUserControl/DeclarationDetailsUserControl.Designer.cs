namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class DeclarationDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AcceptanceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CircuitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearanceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearanceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// AcceptanceDateDateEdit
			//
			this.AcceptanceDateDateEdit.AllowDrop = true;
			this.AcceptanceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AcceptanceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AcceptanceDateDateEdit, "Header.AcceptanceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.AcceptanceDate)));
			this.AcceptanceDateDateEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("E3CFA30B-5395-4419-9541-45AB9C6B8368", "Acceptance Date");
			this.AcceptanceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.AcceptanceDateDateEdit.Name = "AcceptanceDateDateEdit";
			this.AcceptanceDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.AcceptanceDateDateEdit.ReadOnly = true;
			this.AcceptanceDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.AcceptanceDateDateEdit.TabIndex = 0;
			this.AcceptanceDateDateEdit.TabStop = false;
			// 
			// CircuitTextBox
			// 
			this.BindingSource.SetBindingMember(this.CircuitTextBox, "Header.Circuit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.Circuit)));
			this.CircuitTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("970DD0C9-C141-4990-B152-6BFC8F2E4906", "Circuit");
			this.CircuitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 48, true);
			this.CircuitTextBox.Name = "CircuitTextBox";
			this.CircuitTextBox.ReadOnly = true;
			this.CircuitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.CircuitTextBox.TabIndex = 1;
			this.CircuitTextBox.TabStop = false;
			// 
			// ClearanceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClearanceNumberTextBox, "Header.ClearanceReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.ClearanceReferenceNumber)));
			this.ClearanceNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("99cf2f8b-a81d-45ae-b3d0-1d43548b0c96", "Clearance Number");
			this.ClearanceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 81, true);
			this.ClearanceNumberTextBox.Name = "ClearanceNumberTextBox";
			this.ClearanceNumberTextBox.ReadOnly = true;
			this.ClearanceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.ClearanceNumberTextBox.TabIndex = 2;
			this.ClearanceNumberTextBox.TabStop = false;
			// 
			// ClearanceDateDateEdit
			//
			this.ClearanceDateDateEdit.AllowDrop = true;
			this.ClearanceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ClearanceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ClearanceDateDateEdit, "Header.ClearanceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.ClearanceDate)));
			this.ClearanceDateDateEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("ed197766-7f37-4445-9601-c74f8ab9442d", "Clearance Date");
			this.ClearanceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 113, true);
			this.ClearanceDateDateEdit.Name = "ClearanceDateDateEdit";
			this.ClearanceDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.ClearanceDateDateEdit.ReadOnly = true;
			this.ClearanceDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.ClearanceDateDateEdit.TabIndex = 3;
			this.ClearanceDateDateEdit.TabStop = false;
			// 
			// ArrivalLimitDateEdit
			//
			this.ArrivalLimitDateEdit.AllowDrop = true;
			this.ArrivalLimitDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalLimitDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalLimitDateEdit, "Header.ArrivalLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.ArrivalLimit)));
			this.ArrivalLimitDateEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("6f167fe0-7dc7-4ca8-acfc-5bdcbdfa74d0", "Arrival Limit");
			this.ArrivalLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 144, true);
			this.ArrivalLimitDateEdit.Name = "ArrivalLimitDateEdit";
			this.ArrivalLimitDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.ArrivalLimitDateEdit.ReadOnly = true;
			this.ArrivalLimitDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.ArrivalLimitDateEdit.TabIndex = 4;
			this.ArrivalLimitDateEdit.TabStop = false;
			// 
			// DeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalLimitDateEdit);
			this.Controls.Add(this.ClearanceDateDateEdit);
			this.Controls.Add(this.ClearanceNumberTextBox);
			this.Controls.Add(this.CircuitTextBox);
			this.Controls.Add(this.AcceptanceDateDateEdit);
			this.Name = "DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 186, true);
			this.Tag = "";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDateEdit AcceptanceDateDateEdit;
		internal ZArchitecture.ZTextBox CircuitTextBox;
		internal ZArchitecture.ZTextBox ClearanceNumberTextBox;
		internal ZArchitecture.GUI.ZDateEdit ClearanceDateDateEdit;
		internal ZArchitecture.GUI.ZDateEdit ArrivalLimitDateEdit;
	}
}

