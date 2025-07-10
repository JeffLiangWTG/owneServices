namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class ExplanationOnDelayBottomSectionUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExplanationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InformationWordWrapTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			this.MessageRoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExplanationOnDelayGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExplanationCodeDropEdit.SuspendLayout();
			this.MessageRoleDropEdit.SuspendLayout();
			this.ExplanationOnDelayGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingActionParent);
			// 
			// ExplanationCodeDropEdit
			// 
			this.ExplanationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExplanationCodeDropEdit, "SendingObjectsCollection.ExplanationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ExplanationCode)));
			this.ExplanationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 25, true);
			this.ExplanationCodeDropEdit.Name = "ExplanationCodeDropEdit";
			this.ExplanationCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ExplanationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 20, true);
			this.ExplanationCodeDropEdit.TabIndex = 1;
			// 
			// InformationWordWrapTextBox
			// 
			this.BindingSource.SetBindingMember(this.InformationWordWrapTextBox, "SendingObjectsCollection.Information");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Information)));
			this.InformationWordWrapTextBox.CaptionResourceString = null;
			this.InformationWordWrapTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.InformationWordWrapTextBox, 6);
			this.InformationWordWrapTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 51, true);
			this.InformationWordWrapTextBox.Multiline = true;
			this.InformationWordWrapTextBox.Name = "InformationWordWrapTextBox";
			this.InformationWordWrapTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.InformationWordWrapTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 108, true);
			this.InformationWordWrapTextBox.TabIndex = 2;
			// 
			// MessageRoleDropEdit
			// 
			this.MessageRoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageRoleDropEdit, "SendingObjectsCollection.MessageRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.ExplanationOnDelaySendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageRole)));
			this.MessageRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 165, true);
			this.MessageRoleDropEdit.Name = "MessageRoleDropEdit";
			this.MessageRoleDropEdit.ShouldResizeByMaxLength = true;
			this.MessageRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 20, true);
			this.MessageRoleDropEdit.TabIndex = 3;
			// 
			// ExplanationOnDelayGroupBox
			//
			this.ExplanationOnDelayGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("64839C5E-3D0E-4A59-A1C6-982FCBED5336", "Explanation on Delay");
			this.ExplanationOnDelayGroupBox.Controls.Add(this.ExplanationCodeDropEdit);
			this.ExplanationOnDelayGroupBox.Controls.Add(this.MessageRoleDropEdit);
			this.ExplanationOnDelayGroupBox.Controls.Add(this.InformationWordWrapTextBox);
			this.ExplanationOnDelayGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExplanationOnDelayGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExplanationOnDelayGroupBox.Name = "ExplanationOnDelayGroupBox";
			this.ExplanationOnDelayGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 190, true);
			this.ExplanationOnDelayGroupBox.TabIndex = 0;
			this.ExplanationOnDelayGroupBox.TabStop = false;
			// 
			// ExplanationOnDelayBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExplanationOnDelayGroupBox);
			this.Name = "ExplanationOnDelayBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 190, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExplanationCodeDropEdit.ResumeLayout(true);
			this.ExplanationCodeDropEdit.PerformLayout();
			this.MessageRoleDropEdit.ResumeLayout(true);
			this.MessageRoleDropEdit.PerformLayout();
			this.ExplanationOnDelayGroupBox.ResumeLayout(false);
			this.ExplanationOnDelayGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit ExplanationCodeDropEdit;
		internal Customs.GUI.WordWrappingTextBox InformationWordWrapTextBox;
		internal ZArchitecture.GUI.ZDropEdit MessageRoleDropEdit;
		ZArchitecture.GUI.ZGroupBox ExplanationOnDelayGroupBox;
	}
}
