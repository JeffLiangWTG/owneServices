namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ArrivalDeclarationDetailsUserControl
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
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PhaseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AcceptanceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SeparatorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageStatusDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.PhaseDropEdit.SuspendLayout();
			this.ReleaseDateEdit.SuspendLayout();
			this.AcceptanceDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "EffectiveMessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).EffectiveMessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 24, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.MessageStatusDropEdit.TabIndex = 1;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "ArrivalMovementHeader.BM_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_CustomsStatus)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 3, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.StatusDropEdit.TabIndex = 0;
			// 
			// PhaseDropEdit
			// 
			this.PhaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhaseDropEdit, "ArrivalMovementHeader.BM_Phase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_Phase)));
			this.PhaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 50, true);
			this.PhaseDropEdit.Name = "PhaseDropEdit";
			this.PhaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.PhaseDropEdit.TabIndex = 2;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "MovementReferenceIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementReferenceIssueDate)));
			this.ReleaseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 96, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 6;
			// 
			// AcceptanceDateEdit
			// 
			this.AcceptanceDateEdit.AllowDrop = true;
			this.AcceptanceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptanceDateEdit, "ArrivalMovementHeader.BM_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_EntryDate)));
			this.AcceptanceDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AcceptanceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 122, true);
			this.AcceptanceDateEdit.Name = "AcceptanceDateEdit";
			this.AcceptanceDateEdit.TabIndex = 7;
			// 
			// SeparatorLabel
			// 
			this.SeparatorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SeparatorLabel, false);
			this.SeparatorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 73, true);
			this.SeparatorLabel.Name = "SeparatorLabel";
			this.SeparatorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SeparatorLabel.TabIndex = 9;
			this.SeparatorLabel.UseMnemonic = false;
			// 
			// ArrivalDeclarationDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SeparatorLabel);
			this.Controls.Add(this.ReleaseDateEdit);
			this.Controls.Add(this.AcceptanceDateEdit);
			this.Controls.Add(this.PhaseDropEdit);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Name = "ArrivalDeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.PhaseDropEdit.ResumeLayout(true);
			this.PhaseDropEdit.PerformLayout();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.AcceptanceDateEdit.ResumeLayout(true);
			this.AcceptanceDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PhaseDropEdit;
		internal ZArchitecture.ZLabel SeparatorLabel;
		internal ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		internal ZArchitecture.GUI.ZDateEdit AcceptanceDateEdit;

		#endregion
	}
}
