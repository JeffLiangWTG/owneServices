namespace Enterprise.Customs.EU.NCTS.GUI
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
			this.MrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PhaseStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AcceptanceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ActivationDeadlineDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DepartureStatusDropEdit.SuspendLayout();
			this.PhaseStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.ReleaseDateEdit.SuspendLayout();
			this.AcceptanceDateEdit.SuspendLayout();
			this.ActivationDeadlineDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// MrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.MrnTextBox, "Header.MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.MovementReferenceNumber)));
			this.MrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.MrnTextBox.Name = "MrnTextBox";
			this.MrnTextBox.ReadOnly = true;
			this.MrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.MrnTextBox.TabIndex = 0;
			this.MrnTextBox.TabStop = false;
			// 
			// DepartureStatusDropEdit
			// 
			this.DepartureStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureStatusDropEdit, "BM_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_CustomsStatus)));
			this.DepartureStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 50, true);
			this.DepartureStatusDropEdit.Name = "DepartureStatusDropEdit";
			this.DepartureStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DepartureStatusDropEdit.TabIndex = 1;
			// 
			// PhaseStatusDropEdit
			// 
			this.PhaseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhaseStatusDropEdit, "BM_Phase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_Phase)));
			this.PhaseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 110, true);
			this.PhaseStatusDropEdit.Name = "PhaseStatusDropEdit";
			this.PhaseStatusDropEdit.PreBoundMaxLength = 3;
			this.PhaseStatusDropEdit.ShouldResizeByMaxLength = false;
			this.PhaseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.PhaseStatusDropEdit.TabIndex = 3;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "BM_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 80, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.MessageStatusDropEdit.TabIndex = 2;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "Header.MovementReferenceIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.MovementReferenceIssueDate)));
			this.ReleaseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 140, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 4;
			// 
			// AcceptanceDateEdit
			// 
			this.AcceptanceDateEdit.AllowDrop = true;
			this.AcceptanceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptanceDateEdit, "BM_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_EntryDate)));
			this.AcceptanceDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AcceptanceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 170, true);
			this.AcceptanceDateEdit.Name = "AcceptanceDateEdit";
			this.AcceptanceDateEdit.TabIndex = 5;
			// 
			// ActivationDeadlineDateEdit
			// 
			this.ActivationDeadlineDateEdit.AllowDrop = true;
			this.ActivationDeadlineDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ActivationDeadlineDateEdit, "Header.MovementReferenceExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.MovementReferenceExpiryDate)));
			this.ActivationDeadlineDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ActivationDeadlineDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 207, true);
			this.ActivationDeadlineDateEdit.Name = "ActivationDeadlineDateEdit";
			this.ActivationDeadlineDateEdit.ReadOnly = true;
			this.ActivationDeadlineDateEdit.TabIndex = 6;
			// 
			// DeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MrnTextBox);
			this.Controls.Add(this.DepartureStatusDropEdit);
			this.Controls.Add(this.PhaseStatusDropEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.ReleaseDateEdit);
			this.Controls.Add(this.AcceptanceDateEdit);
			this.Controls.Add(this.ActivationDeadlineDateEdit);
			this.Name = "DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DepartureStatusDropEdit.ResumeLayout(true);
			this.DepartureStatusDropEdit.PerformLayout();
			this.PhaseStatusDropEdit.ResumeLayout(true);
			this.PhaseStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.AcceptanceDateEdit.ResumeLayout(true);
			this.AcceptanceDateEdit.PerformLayout();
			this.ActivationDeadlineDateEdit.ResumeLayout(true);
			this.ActivationDeadlineDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox MrnTextBox;
		internal ZArchitecture.GUI.ZDropEdit DepartureStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PhaseStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		internal ZArchitecture.GUI.ZDateEdit AcceptanceDateEdit;
		internal ZArchitecture.GUI.ZDateEdit ActivationDeadlineDateEdit;
	}
}

