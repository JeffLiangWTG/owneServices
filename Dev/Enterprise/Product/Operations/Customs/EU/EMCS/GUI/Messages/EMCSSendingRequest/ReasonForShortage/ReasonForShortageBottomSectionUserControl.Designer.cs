namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class ReasonForShortageBottomSectionUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReasonForShortageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GeneralExplanationTextBox = new Enterprise.Customs.GUI.WordWrappingTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReasonForShortageGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.ReasonForShortageSendingActionParent);
			// 
			// ReasonForShortageGroupBox
			// 
			this.ReasonForShortageGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("3FD5AD3A-4A7C-41D3-ABA0-37AF5583663D", "Reason for Shortage");
			this.ReasonForShortageGroupBox.Controls.Add(this.GeneralExplanationTextBox);
			this.ReasonForShortageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReasonForShortageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReasonForShortageGroupBox.Name = "ReasonForShortageGroupBox";
			this.ReasonForShortageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 130, true);
			this.ReasonForShortageGroupBox.TabIndex = 0;
			this.ReasonForShortageGroupBox.TabStop = false;
			// 
			// GeneralExplanationTextBox
			// 
			this.BindingSource.SetBindingMember(this.GeneralExplanationTextBox, "SendingObjectsCollection.GeneralExplanation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.EMCS.Business.ReasonForShortageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.EU.EMCS.Business.ReasonForShortageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).GeneralExplanation)));
			this.GeneralExplanationTextBox.CaptionResourceString = null;
			this.GeneralExplanationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.GeneralExplanationTextBox, 3);
			this.GeneralExplanationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 25, true);
			this.GeneralExplanationTextBox.Multiline = true;
			this.GeneralExplanationTextBox.Name = "GeneralExplanationTextBox";
			this.GeneralExplanationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.GeneralExplanationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 108, true);
			this.GeneralExplanationTextBox.TabIndex = 0;
			// 
			// ReasonForShortageBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReasonForShortageGroupBox);
			this.Name = "ReasonForShortageBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReasonForShortageGroupBox.ResumeLayout(false);
			this.ReasonForShortageGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox ReasonForShortageGroupBox;
		internal Customs.GUI.WordWrappingTextBox GeneralExplanationTextBox;
	}
}
