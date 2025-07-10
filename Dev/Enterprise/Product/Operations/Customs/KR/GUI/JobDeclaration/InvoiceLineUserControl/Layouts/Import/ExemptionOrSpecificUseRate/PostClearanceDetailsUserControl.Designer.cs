using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class PostClearanceDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.PostClearanceYNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ProductTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UseCodeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.SerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PostClearanceYNDropEdit.SuspendLayout();
            this.ProductTypeDropEdit.SuspendLayout();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.GoodsLocationAddressControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
            // 
            // PostClearanceYNDropEdit
            // 
            this.PostClearanceYNDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PostClearanceYNDropEdit, "JI_PCProcedure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PCProcedure)));
            this.PostClearanceYNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 9, true);
            this.PostClearanceYNDropEdit.Name = "PostClearanceYNDropEdit";
            this.PostClearanceYNDropEdit.PreBoundMaxLength = 1;
            this.PostClearanceYNDropEdit.ShowDescriptionBox = false;
            this.PostClearanceYNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.PostClearanceYNDropEdit.TabIndex = 3;
            // 
            // ProductTypeDropEdit
            // 
            this.ProductTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ProductTypeDropEdit, "JI_SpecificUseProductType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SpecificUseProductType)));
            this.ProductTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 49, true);
            this.ProductTypeDropEdit.Name = "ProductTypeDropEdit";
            this.ProductTypeDropEdit.PreBoundMaxLength = 2;
            this.ProductTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 17, true);
            this.ProductTypeDropEdit.TabIndex = 5;
            // 
            // UseCodeDescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.UseCodeDescriptionTextBox, "JI_SpecificUseCodeDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SpecificUseCodeDescription)));
            this.UseCodeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 9, true);
            this.UseCodeDescriptionTextBox.Name = "UseCodeDescriptionTextBox";
            this.UseCodeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 17, true);
            this.UseCodeDescriptionTextBox.TabIndex = 6;
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JI_JurisdictionalCusOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_JurisdictionalCusOffice)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.CustomsOfficeCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 85, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.ShowDescriptionBox = false;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 7;
            // 
            // SerialNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.SerialNumberTextBox, "JI_SerialNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SerialNumber)));
            this.SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 49, true);
            this.SerialNumberTextBox.Name = "SerialNumberTextBox";
            this.SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 17, true);
            this.SerialNumberTextBox.TabIndex = 8;
            // 
            // GoodsLocationAddressControl
            // 
            this.GoodsLocationAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsLocationAddressControl, "JI_OA_ConsigneeAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_OA_ConsigneeAddress)));
            this.GoodsLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 85, true);
            this.GoodsLocationAddressControl.Name = "GoodsLocationAddressControl";
            this.GoodsLocationAddressControl.PopupCaption = "";
            this.GoodsLocationAddressControl.ShowAddress = false;
            this.GoodsLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 17, true);
            this.GoodsLocationAddressControl.TabIndex = 0;
            // 
            // PostClearanceDetailsUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GoodsLocationAddressControl);
            this.Controls.Add(this.SerialNumberTextBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Controls.Add(this.UseCodeDescriptionTextBox);
            this.Controls.Add(this.ProductTypeDropEdit);
            this.Controls.Add(this.PostClearanceYNDropEdit);
            this.Name = "PostClearanceDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 133, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PostClearanceYNDropEdit.ResumeLayout(true);
            this.PostClearanceYNDropEdit.PerformLayout();
            this.ProductTypeDropEdit.ResumeLayout(true);
            this.ProductTypeDropEdit.PerformLayout();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.GoodsLocationAddressControl.ResumeLayout(true);
            this.GoodsLocationAddressControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZDropEdit PostClearanceYNDropEdit;
		internal ZDropEdit ProductTypeDropEdit;
		internal ZArchitecture.ZTextBox UseCodeDescriptionTextBox;
		internal ZCodeFindBox CustomsOfficeCodeFindBox;
		internal ZArchitecture.ZTextBox SerialNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl GoodsLocationAddressControl;
	}
}
