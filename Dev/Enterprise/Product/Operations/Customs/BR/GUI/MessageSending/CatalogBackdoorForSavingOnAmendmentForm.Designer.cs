namespace Enterprise.Customs.BR.GUI
{
	partial class CatalogBackdoorForSavingOnAmendmentForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CatalogBackdoorForSavingOnAmendmentForm));
            this.OptionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // TextLabel
            // 
            this.TextLabel.Text = resources.GetString("TextLabel.Text");
            // 
            // SendWithoutSendingAmendmentAtAllRadioButton
            // 
            this.SendWithoutSendingAmendmentAtAllRadioButton.Visible = false;
            // 
            // SaveWithoutSendingToDeferAmendmentSendingRadioButton
            // 
            this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.BackColor = System.Drawing.SystemColors.Control;
            this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2C30AABD-1FAE-449C-B121-88B27A8395C2", " Save without sending the message. The message status will be set to \'NOT - Not S" +
        "ent\' until the message is Sent.");
            this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 14, true);
            this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.ReadOnly = true;
            this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.UseVisualStyleBackColor = false;
            // 
            // SendAmendmentRadioButton
            // 
            this.SendAmendmentRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 48, true);
            this.SendAmendmentRadioButton.Visible = false;
            // 
            // SendAmendmentExplanationButton
            // 
            this.SendAmendmentExplanationButton.Visible = false;
            // 
            // SaveWithoutEntryChangesExplanationButton
            // 
            this.SaveWithoutEntryChangesExplanationButton.Visible = false;
            // 
            // SaveWithEntryChangesExplanationButton
            // 
            this.SaveWithEntryChangesExplanationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
            this.SaveWithEntryChangesExplanationButton.Visible = false;
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
            // 
            // CatalogBackdoorForSavingOnAmendmentForm
            // 
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 206, true);
            this.Name = "CatalogBackdoorForSavingOnAmendmentForm";
            this.OptionsGroupBox.ResumeLayout(false);
            this.OptionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
	}
}
