namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class CancellationBottomSectionUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CancellationOfEADGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReasonDropEdit.SuspendLayout();
			this.CancellationOfEADGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.CancellationSendingActionParent);
			// 
			// ReasonDropEdit
			// 
			this.ReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReasonDropEdit, "SendingObjectsCollection.Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.EMCS.Business.CancellationSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.CancellationSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Reason)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ReasonDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 18, true);
			this.ReasonDropEdit.Name = "ReasonDropEdit";
			this.ReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.ReasonDropEdit.TabIndex = 1;
			// 
			// InformationTextBox
			// 
			this.InformationTextBox.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.InformationTextBox, "SendingObjectsCollection.Information");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.CancellationSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.CancellationSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Information)));
			this.InformationTextBox.CaptionResourceString = null;
			this.InformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.InformationTextBox, 0);
			this.InformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 50, true);
			this.InformationTextBox.Multiline = true;
			this.InformationTextBox.Name = "InformationTextBox";
			this.InformationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.InformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 87, true);
			this.InformationTextBox.TabIndex = 2;
			// 
			// CancellationOfEADGroupBox
			// 
			this.CancellationOfEADGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("FB69745B-00F1-4718-AD8F-B0B4C9333DCB", "Cancellation of EAD");
			this.CancellationOfEADGroupBox.Controls.Add(this.ReasonDropEdit);
			this.CancellationOfEADGroupBox.Controls.Add(this.InformationTextBox);
			this.CancellationOfEADGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancellationOfEADGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CancellationOfEADGroupBox.Name = "CancellationOfEADGroupBox";
			this.CancellationOfEADGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 148, true);
			this.CancellationOfEADGroupBox.TabIndex = 0;
			this.CancellationOfEADGroupBox.TabStop = false;
			// 
			// CancellationBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CancellationOfEADGroupBox);
			this.Name = "CancellationBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 148, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReasonDropEdit.ResumeLayout(true);
			this.ReasonDropEdit.PerformLayout();
			this.CancellationOfEADGroupBox.ResumeLayout(false);
			this.CancellationOfEADGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit ReasonDropEdit;
		internal ZArchitecture.ZTextBox InformationTextBox;
		ZArchitecture.GUI.ZGroupBox CancellationOfEADGroupBox;
	}
}
