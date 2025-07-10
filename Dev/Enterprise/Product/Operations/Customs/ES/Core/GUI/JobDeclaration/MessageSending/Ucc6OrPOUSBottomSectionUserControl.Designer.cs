namespace Enterprise.Customs.ES.GUI
{
	partial class Ucc6OrPOUSBottomSectionUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CancellationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActivateByOperatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SecurityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestDispatchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CancellationGroupBox.SuspendLayout();
			this.CodeDropEdit.SuspendLayout();
			this.ActivateByOperatorDropEdit.SuspendLayout();
			this.SecurityDropEdit.SuspendLayout();
			this.RequestDispatchDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObjectParent);
			// 
			// CancellationGroupBox
			// 
			this.CancellationGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("a44eb361-96fe-4a47-a13f-c9fc8055ec7b", "Reason for Cancellation");
			this.CancellationGroupBox.Controls.Add(this.ReasonTextBox);
			this.CancellationGroupBox.Controls.Add(this.CodeDropEdit);
			this.CancellationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CancellationGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 145, true);
			this.CancellationGroupBox.Name = "CancellationGroupBox";
			this.CancellationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 149, true);
			this.CancellationGroupBox.TabIndex = 9;
			this.CancellationGroupBox.TabStop = false;
			// 
			// ReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "SendingObjectsCollection.ReasonForCancellation.Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ReasonForCancellation.Reason)));
			this.ReasonTextBox.CaptionResourceString = null;
			this.ReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReasonTextBox, false);
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.ReasonTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 80, true);
			this.ReasonTextBox.Multiline = true;
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 98, true);
			this.ReasonTextBox.TabIndex = 10;
			// 
			// CodeDropEdit
			// 
			this.CodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CodeDropEdit, "SendingObjectsCollection.ReasonForCancellation.Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ReasonForCancellation.Code)));
			this.CodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CodeDropEdit.Name = "CodeDropEdit";
			this.CodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.CodeDropEdit.TabIndex = 11;
			// 
			// ActivateByOperatorDropEdit
			// 
			this.ActivateByOperatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActivateByOperatorDropEdit, "SendingObjectsCollection.ActivateByOperatorFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ActivateByOperatorFlag)));
			this.ActivateByOperatorDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C54B9481-C9E6-402C-B271-F6025FCF12FE", "Activate");
			this.ActivateByOperatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 7, true);
			this.ActivateByOperatorDropEdit.Name = "ActivateByOperatorDropEdit";
			this.ActivateByOperatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.ActivateByOperatorDropEdit.PreBoundMaxLength = 2;
			this.ActivateByOperatorDropEdit.TabIndex = 12;
			// 
			// SecurityDropEdit
			// 
			this.SecurityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDropEdit, "SendingObjectsCollection.SecurityFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SecurityFlag)));
			this.SecurityDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("6D815956-CFC9-4396-8EC4-DDA4115C24FD", "Security");
			this.SecurityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 7, true);
			this.SecurityDropEdit.Name = "SecurityDropEdit";
			this.SecurityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.SecurityDropEdit.PreBoundMaxLength = 2;
			this.SecurityDropEdit.TabIndex = 13;
			// 
			// RequestDispatchDropEdit
			// 
			this.RequestDispatchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestDispatchDropEdit, "SendingObjectsCollection.RequestDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RequestDispatch)));
			this.RequestDispatchDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A52C6619-E314-49CF-A11B-C16955ACD276", "Request Dispatch");
			this.RequestDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 7, true);
			this.RequestDispatchDropEdit.Name = "RequestDispatchDropEdit";
			this.RequestDispatchDropEdit.PreBoundMaxLength = 1;
			this.RequestDispatchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.RequestDispatchDropEdit.TabIndex = 14;
			// 
			// BottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CancellationGroupBox);
			this.Controls.Add(this.ActivateByOperatorDropEdit);
			this.Controls.Add(this.SecurityDropEdit);
			this.Controls.Add(this.RequestDispatchDropEdit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 155, true);
			this.Name = "BottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 155, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CancellationGroupBox.ResumeLayout(false);
			this.CancellationGroupBox.PerformLayout();
			this.CodeDropEdit.ResumeLayout(true);
			this.CodeDropEdit.PerformLayout();
			this.ActivateByOperatorDropEdit.ResumeLayout(true);
			this.ActivateByOperatorDropEdit.PerformLayout();
			this.SecurityDropEdit.ResumeLayout(true);
			this.SecurityDropEdit.PerformLayout();
			this.RequestDispatchDropEdit.ResumeLayout(true);
			this.RequestDispatchDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CancellationGroupBox;
		internal ZArchitecture.ZTextBox ReasonTextBox;
		internal ZArchitecture.GUI.ZDropEdit CodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ActivateByOperatorDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SecurityDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RequestDispatchDropEdit;
	}
}
