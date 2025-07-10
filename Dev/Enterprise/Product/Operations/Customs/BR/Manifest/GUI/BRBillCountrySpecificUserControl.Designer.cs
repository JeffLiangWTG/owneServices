
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Manifest.GUI
{
	partial class BRBillCountrySpecificUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToOrderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BLServiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FRTModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SellerCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CEMercanteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentTypeDropEdit.SuspendLayout();
			this.ToOrderCheckBox.SuspendLayout();
			this.BLServiceCheckBox.SuspendLayout();
			this.FRTModeDropEdit.SuspendLayout();
			this.SellerCountryCodeFindBox.SuspendLayout();
			this.CEMercanteTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Manifest.Business.AsycudaBill);
			// 
			// DocumentTypeDropEdit
			// 
			this.DocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentTypeDropEdit, "DocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Manifest.Business.AsycudaBill)(null)).DocumentType)));
			this.DocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 23, true);
			this.DocumentTypeDropEdit.Name = "DocumentTypeDropEdit";
			this.DocumentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.DocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DocumentTypeDropEdit.TabIndex = 0;
			// 
			// ToOrderCheckBox
			//
			this.BindingSource.SetBindingMember(this.ToOrderCheckBox, "To_Order");
			this.ToOrderCheckBox.AutoSize = true;
			this.ToOrderCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ToOrderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ToOrderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.ToOrderCheckBox.Name = "ToOrderCheckBox";
			this.ToOrderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ToOrderCheckBox.TabIndex = 1;
			this.ToOrderCheckBox.UseVisualStyleBackColor = true;
			// 
			// BLServiceCheckBox
			//
			this.BindingSource.SetBindingMember(this.BLServiceCheckBox, "BL_Service");
			this.BLServiceCheckBox.AutoSize = true;
			this.BLServiceCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BLServiceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BLServiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.BLServiceCheckBox.Name = "BLServiceCheckBox";
			this.BLServiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.BLServiceCheckBox.TabIndex = 2;
			this.BLServiceCheckBox.UseVisualStyleBackColor = true;
			// 
			// FRTModeDropEdit
			// 
			this.FRTModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FRTModeDropEdit, "FRTMode");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Manifest.Business.AsycudaBill)(null)).FRTMode)));
			this.FRTModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.FRTModeDropEdit.Name = "FRTModeDropEdit";
			this.FRTModeDropEdit.PreBoundMaxLength = 3;
			this.FRTModeDropEdit.ShouldResizeByMaxLength = true;
			this.FRTModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.FRTModeDropEdit.TabIndex = 3;
			// 
			// SellerCountryCodeFindBox
			// 
			this.SellerCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerCountryCodeFindBox, "ABL_RN_NKSellerCountry");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Manifest.Business.AsycudaBill)(null)).ABL_RN_NKSellerCountry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SellerCountryCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.SellerCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 40, true);
			this.SellerCountryCodeFindBox.Name = "SellerCountryCodeFindBox";
			this.SellerCountryCodeFindBox.PreBoundMaxLength = 5;
			this.SellerCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.SellerCountryCodeFindBox.TabIndex = 4;
			// 
			// CEMercanteTextBox
			// 
			this.BindingSource.SetBindingMember(this.CEMercanteTextBox, "CustomsOwnNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Manifest.Business.AsycudaBill)(null)).CustomsOwnNumber)));
			this.CEMercanteTextBox.CaptionResourceString = null;
			this.CEMercanteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 0, true);
			this.CEMercanteTextBox.Name = "CEMercanteTextBox";
			this.CEMercanteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.CEMercanteTextBox.TabIndex = 5;
			// 
			// BRBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DocumentTypeDropEdit);
			this.Controls.Add(this.ToOrderCheckBox);
			this.Controls.Add(this.BLServiceCheckBox);
			this.Controls.Add(this.FRTModeDropEdit);
			this.Controls.Add(this.SellerCountryCodeFindBox);
			this.Controls.Add(this.CEMercanteTextBox);
			this.Name = "BRBillCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 68, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentTypeDropEdit.ResumeLayout(true);
			this.DocumentTypeDropEdit.PerformLayout();
			this.ToOrderCheckBox.ResumeLayout(true);
			this.ToOrderCheckBox.PerformLayout();
			this.BLServiceCheckBox.ResumeLayout(true);
			this.BLServiceCheckBox.PerformLayout();
			this.FRTModeDropEdit.ResumeLayout(true);
			this.FRTModeDropEdit.PerformLayout();
			this.SellerCountryCodeFindBox.ResumeLayout(true);
			this.SellerCountryCodeFindBox.PerformLayout();
			this.CEMercanteTextBox.ResumeLayout(true);
			this.CEMercanteTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit DocumentTypeDropEdit;
		internal ZArchitecture.GUI.ZCheckBox ToOrderCheckBox;
		internal ZArchitecture.GUI.ZCheckBox BLServiceCheckBox;
		internal ZArchitecture.GUI.ZDropEdit FRTModeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox SellerCountryCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox CEMercanteTextBox;
	}
}
