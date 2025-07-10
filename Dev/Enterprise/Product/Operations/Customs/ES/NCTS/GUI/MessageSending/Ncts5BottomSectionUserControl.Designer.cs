namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class Ncts5BottomSectionUserControl
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
			this.CancellationReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestDispatchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CancellationGroupBox.SuspendLayout();
			this.RequestDispatchDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObjectParent);
			// 
			// CancellationGroupBox
			// 
			this.CancellationGroupBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("AC122E48-2808-4D67-9163-F4A1A7391B3B", "Reason for Cancellation");
			this.CancellationGroupBox.Controls.Add(this.CancellationReasonTextBox);
			this.CancellationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CancellationGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 145, true);
			this.CancellationGroupBox.Name = "CancellationGroupBox";
			this.CancellationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 149, true);
			this.CancellationGroupBox.TabIndex = 9;
			this.CancellationGroupBox.TabStop = false;
			// 
			// CancellationReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.CancellationReasonTextBox, "SendingObjectsCollection.ReasonForCancellation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ReasonForCancellation)));
			this.CancellationReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CancellationReasonTextBox, false);
			this.CancellationReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CancellationReasonTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 80, true);
			this.CancellationReasonTextBox.Multiline = true;
			this.CancellationReasonTextBox.Name = "CancellationReasonTextBox";
			this.CancellationReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 98, true);
			this.CancellationReasonTextBox.TabIndex = 10;
			// 
			// RequestDispatchDropEdit
			// 
			this.RequestDispatchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestDispatchDropEdit, "SendingObjectsCollection.RequestDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RequestDispatch)));
			this.RequestDispatchDropEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("5B2845BD-D10D-4A37-AE3E-54CF63E6E87C", "Request Dispatch");
			this.RequestDispatchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 7, true);
			this.RequestDispatchDropEdit.Name = "RequestDispatchDropEdit";
			this.RequestDispatchDropEdit.PreBoundMaxLength = 1;
			this.RequestDispatchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.RequestDispatchDropEdit.TabIndex = 9;
			// 
			// NctsBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CancellationGroupBox);
			this.Controls.Add(this.RequestDispatchDropEdit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 155, true);
			this.Name = "NctsBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 155, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CancellationGroupBox.ResumeLayout(false);
			this.CancellationGroupBox.PerformLayout();
			this.RequestDispatchDropEdit.ResumeLayout(true);
			this.RequestDispatchDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CancellationGroupBox;
		internal ZArchitecture.ZTextBox CancellationReasonTextBox;
		internal ZArchitecture.GUI.ZDropEdit RequestDispatchDropEdit;
	}
}
