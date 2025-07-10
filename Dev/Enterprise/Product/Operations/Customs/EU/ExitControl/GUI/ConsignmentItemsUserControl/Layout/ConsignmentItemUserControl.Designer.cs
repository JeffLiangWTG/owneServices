namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ConsignmentItemUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ConsignmentItemPackingDetailsUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ConsignmentItemPackingDetailsUserControl();
			this.AdditionalDocumentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReportAdditionalDocumentsGridUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ReportAdditionalDocumentsGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReportAdditionalDocumentsGridUserControl.SuspendLayout();
			this.ConsignmentItemPackingDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentItemCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem>);
			// 
			// ConsignmentItemPackingDetailsUserControl
			// 
			this.ConsignmentItemPackingDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignmentItemPackingDetailsUserControl, "CusExitConsignmentPackagePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentPivotCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot>)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(null)).CusExitConsignmentPackagePivots)));
			this.ConsignmentItemPackingDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.ConsignmentItemPackingDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentItemPackingDetailsUserControl.Name = "ConsignmentItemPackingDetailsUserControl";
			this.ConsignmentItemPackingDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 362, true);
			this.ConsignmentItemPackingDetailsUserControl.TabIndex = 1;
			// 
			// ReportAdditionalDocumentsGridUserControl
			// 
			this.ReportAdditionalDocumentsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportAdditionalDocumentsGridUserControl, "AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.ExitControl.Business.IAdditionalInfoCollection<Enterprise.Customs.EU.ExitControl.Business.AdditionalInfo>)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem)(null)).AdditionalInfos)));
			this.ReportAdditionalDocumentsGridUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ReportAdditionalDocumentsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportAdditionalDocumentsGridUserControl.Name = "ReportAdditionalDocumentsGridUserControl";
			this.ReportAdditionalDocumentsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 362, true);
			this.ReportAdditionalDocumentsGridUserControl.TabIndex = 2;
			// 
			// AdditionalDocumentsLabel
			// 
			this.AdditionalDocumentsLabel.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("44902571-CF5A-4462-B59A-C3CC974B27CD", "Additional Document");
			this.AdditionalDocumentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Medium | Enterprise.ZArchitecture.Core.OFontTypes.Bolded)));
			this.AdditionalDocumentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsLabel.Name = "AdditionalDocumentsLabel";
			this.AdditionalDocumentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AdditionalDocumentsLabel.TabIndex = 5;
			// 
			// ConsignmentItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsignmentItemPackingDetailsUserControl);
			this.Controls.Add(this.ReportAdditionalDocumentsGridUserControl);
			this.Controls.Add(this.AdditionalDocumentsLabel);
			this.Name = "ConsignmentItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 362, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsignmentItemPackingDetailsUserControl.ResumeLayout(true);
			this.ConsignmentItemPackingDetailsUserControl.PerformLayout();
			this.ReportAdditionalDocumentsGridUserControl.ResumeLayout(true);
			this.ReportAdditionalDocumentsGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ConsignmentItemPackingDetailsUserControl ConsignmentItemPackingDetailsUserControl;
		internal ReportAdditionalDocumentsGridUserControl ReportAdditionalDocumentsGridUserControl;
		internal ZArchitecture.ZLabel AdditionalDocumentsLabel;
	}
}
