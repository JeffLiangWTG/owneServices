namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AsycudaRelatedDeclarationsUserControl = new Enterprise.Customs.AsycudaCustoms.GUI.RelatedDeclarationsUserControl();
			this.PaymentAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AsycudaRelatedDeclarationsUserControl.SuspendLayout();
			this.PaymentAccountNumberTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.JobDeclaration);
			// 
			// AsycudaRelatedDeclarationsUserControl
			// 
			this.AsycudaRelatedDeclarationsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AsycudaRelatedDeclarationsUserControl, ".");
			this.AsycudaRelatedDeclarationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 98, true);
			this.AsycudaRelatedDeclarationsUserControl.Name = "AsycudaRelatedDeclarationsUserControl";
			this.AsycudaRelatedDeclarationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 271, true);
			this.AsycudaRelatedDeclarationsUserControl.TabIndex = 0;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AsycudaRelatedDeclarationsUserControl);
			this.Controls.Add(this.PaymentAccountNumberTextBox);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 510, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AsycudaRelatedDeclarationsUserControl.ResumeLayout(true);
			this.AsycudaRelatedDeclarationsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			//
			// PaymentPartyDropEdit
			// 
			this.PaymentAccountNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentAccountNumberTextBox, "JE_DefermentAccountNumber");
			this.PaymentAccountNumberTextBox.BackColor = System.Drawing.Color.White;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DefermentAccountNumber)));
			this.PaymentAccountNumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PaymentAccountNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PaymentAccountNumberTextBox.Name = "PaymentAccountNumberTextBox";
			this.PaymentAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
		}

		#endregion

		internal RelatedDeclarationsUserControl AsycudaRelatedDeclarationsUserControl;
		internal Enterprise.ZArchitecture.ZTextBox PaymentAccountNumberTextBox;
	}
}
