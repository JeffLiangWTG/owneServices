namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class PreviousProcedureExportATZLPanel
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LocalReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizationNumberDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorizationNumberDropDown.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader);
			// 
			// LocalReferenceTextBox
			// 
			this.LocalReferenceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalReferenceTextBox, "PreviousDocumentMaster.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(null)).PreviousDocumentMaster.CSI_ReferenceNumber2)));
			this.LocalReferenceTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("89221c83-385b-41ae-9fff-b97f7d6fc547", "Local Reference");
			this.LocalReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LocalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 3, true);
			this.LocalReferenceTextBox.Name = "LocalReferenceTextBox";
			this.LocalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.LocalReferenceTextBox.TabIndex = 1;
			// 
			// AuthorizationNumberDropDown
			// 
			this.AuthorizationNumberDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberDropDown, "PreviousDocumentMaster.AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(null)).PreviousDocumentMaster.AuthorizationNumber)));
			this.AuthorizationNumberDropDown.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("55da905e-a40a-4f8e-b8dc-4903113f7117", "Auth. No.");
			this.AuthorizationNumberDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 3, true);
			this.AuthorizationNumberDropDown.Name = "AuthorizationNumberDropDown";
			this.AuthorizationNumberDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 20, true);
			this.AuthorizationNumberDropDown.TabIndex = 2;
			// 
			// PreviousProcedureExportATZLPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LocalReferenceTextBox);
			this.Controls.Add(this.AuthorizationNumberDropDown);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 43, true);
			this.Name = "PreviousProcedureExportATZLPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorizationNumberDropDown.ResumeLayout(true);
			this.AuthorizationNumberDropDown.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox LocalReferenceTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit AuthorizationNumberDropDown;

	}
}
