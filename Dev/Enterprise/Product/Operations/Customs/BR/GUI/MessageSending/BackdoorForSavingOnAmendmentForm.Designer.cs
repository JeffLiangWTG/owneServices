
namespace Enterprise.Customs.BR.GUI
{
	partial class BackdoorForSavingOnAmendmentForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SaveWithoutSendingToDeferAmendmentSendingRadioButton
			// 
			this.SaveWithoutSendingToDeferAmendmentSendingRadioButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("02483FE9-649D-41F1-BE0C-0760C7B5547F",
				" Save WITHOUT sending amendment. Message Status will show \"Not Sent\" until you send Rectification Message.");
			// 
			// BackdoorForSavingOnAmendmentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 208, true);
			this.CaptionRenderingEnabled = true;
			this.DataSourceAssemblyName = "Enterprise.Customs.BR.Business";
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.DeclarationDeferredAmendmentSavingOptions);
			this.DataSourceTypeName = "Enterprise.Customs.BR.Business.DeferredAmendmentSavingOptions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "BackdoorForSavingOnAmendmentForm";
			this.Text = "CustomisedMessageBoxForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
