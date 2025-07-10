
namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class NctsSupportingDocumentsUserControl
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
			this.CSI_StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CSI_UnitOfQuantityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_YearOfIssueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_RN_NKCountryCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BottomPanel.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.SupDocTypeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSI_StatusDropEdit.SuspendLayout();
			this.CSI_RN_NKCountryCodeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SupportingDocumentsSplitter
			// 
			this.SupportingDocumentsSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SupportingDocumentsSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 137, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 137, true);
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_StatusDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_QuantityCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_YearOfIssueTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_RN_NKCountryCodeCodeFindBox);
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 137, true);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_RN_NKCountryCodeCodeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_YearOfIssueTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_UnitOfQuantityTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_QuantityCalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocReferenceTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocTypeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocReasonTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_StatusDropEdit, 0);
			// 
			// SupDocReasonTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SupDocReasonTextBox, false);
			this.SupDocReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupDocReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.SupDocReasonTextBox.Visible = false;
			// 
			// SupDocTypeFindBox
			// 
			this.SupDocTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19, true);
			this.SupDocTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 20, true);
			this.SupDocTypeFindBox.TabIndex = 0;
			// 
			// SupDocReferenceTextBox
			// 
			this.SupDocReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 42, true);
			this.SupDocReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 20, true);
			this.SupDocReferenceTextBox.TabIndex = 1;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 182, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// CSI_StatusDropEdit
			// 
			this.CSI_StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_StatusDropEdit, "SupportingDocuments.CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_Status)));
			this.CSI_StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64, true);
			this.CSI_StatusDropEdit.Name = "CSI_StatusDropEdit";
			this.CSI_StatusDropEdit.ShouldResizeByMaxLength = true;
			this.CSI_StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 20, true);
			this.CSI_StatusDropEdit.TabIndex = 2;
			// 
			// CSI_QuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CSI_QuantityCalcEdit, "SupportingDocuments.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
			this.CSI_QuantityCalcEdit.CaptionResourceString = null;
			this.CSI_QuantityCalcEdit.DecimalPlaces = 5;
			this.CSI_QuantityCalcEdit.Decimals = 5;
			this.CSI_QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 86, true);
			this.CSI_QuantityCalcEdit.Name = "CSI_QuantityCalcEdit";
			this.CSI_QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_QuantityCalcEdit.TabIndex = 3;
			this.CSI_QuantityCalcEdit.Text = "0.00000";
			this.CSI_QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CSI_UnitOfQuantityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityTextBox, "SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.CSI_UnitOfQuantityTextBox.CaptionResourceString = null;
			this.CSI_UnitOfQuantityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 86, true);
			this.CSI_UnitOfQuantityTextBox.Name = "CSI_UnitOfQuantityTextBox";
			this.CSI_UnitOfQuantityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_UnitOfQuantityTextBox.TabIndex = 4;
			// 
			// CSI_YearOfIssueTextBox
			// 
			this.BindingSource.SetBindingMember(this.CSI_YearOfIssueTextBox, "SupportingDocuments.CSI_YearOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_YearOfIssue)));
			this.CSI_YearOfIssueTextBox.CaptionResourceString = null;
			this.CSI_YearOfIssueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 108, true);
			this.CSI_YearOfIssueTextBox.Name = "CSI_YearOfIssueTextBox";
			this.CSI_YearOfIssueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CSI_YearOfIssueTextBox.TabIndex = 5;
			// 
			// CSI_RN_NKCountryCodeCodeFindBox
			// 
			this.CSI_RN_NKCountryCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_RN_NKCountryCodeCodeFindBox, "SupportingDocuments.CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsSupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).SupportingDocuments)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.CSI_RN_NKCountryCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 108, true);
			this.CSI_RN_NKCountryCodeCodeFindBox.Name = "CSI_RN_NKCountryCodeCodeFindBox";
			this.CSI_RN_NKCountryCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CSI_RN_NKCountryCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.CSI_RN_NKCountryCodeCodeFindBox.TabIndex = 6;
			// 
			// NctsSupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "NctsSupportingDocumentsUserControl";
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.SupDocTypeFindBox.ResumeLayout(true);
			this.SupDocTypeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSI_StatusDropEdit.ResumeLayout(true);
			this.CSI_StatusDropEdit.PerformLayout();
			this.CSI_RN_NKCountryCodeCodeFindBox.ResumeLayout(true);
			this.CSI_RN_NKCountryCodeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZDropEdit CSI_StatusDropEdit;
		ZArchitecture.ZCalcEdit CSI_QuantityCalcEdit;
		ZArchitecture.ZTextBox CSI_UnitOfQuantityTextBox;
		ZArchitecture.ZTextBox CSI_YearOfIssueTextBox;
		ZArchitecture.GUI.ZCodeFindBox CSI_RN_NKCountryCodeCodeFindBox;
	}
}
