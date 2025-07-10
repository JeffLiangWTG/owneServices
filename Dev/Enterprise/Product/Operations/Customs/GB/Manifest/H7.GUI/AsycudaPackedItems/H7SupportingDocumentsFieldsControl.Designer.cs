using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.GUI
{
	public partial class H7SupportingDocumentsFieldsControl
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
			this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CSI_AvailabilityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CSI_ActionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CSI_ReferenceNumber2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CSI_SubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSI_CodeCodeFindBox.SuspendLayout();
			this.CSI_AvailabilityDropEdit.SuspendLayout();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.CSI_ActionsDropEdit.SuspendLayout();
			this.CSI_DescriptionTextBox.SuspendLayout();
			this.SuspendLayout();
			//
			// Binding Source
			//
			this.BindingSource.DataSourceType = typeof(AsycudaPackedItem);
			// 
			// CSI_CodeCodeFindBox
			// 
			this.CSI_CodeCodeFindBox.AllowDrop = true;
			this.CSI_CodeCodeFindBox.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_CodeCodeFindBox, "SupportingDocuments.CSI_Code");
			this.CSI_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 19);
			this.CSI_CodeCodeFindBox.Name = "CSI_CodeCodeFindBox";
			this.CSI_CodeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CSI_CodeCodeFindBox.ParentType = null;
			this.CSI_CodeCodeFindBox.PreBoundMaxLength = 4;
			this.CSI_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20);
			this.CSI_CodeCodeFindBox.TabIndex = 0;
			// 
			// CSI_ReferenceNumberTextBox
			// 
			this.CSI_ReferenceNumberTextBox.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "SupportingDocuments.CSI_ReferenceNumber");
			this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 41);
			this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
			this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20	);
			this.CSI_ReferenceNumberTextBox.TabIndex = 1;
			// 
			// CSI_ActionsDropEdit
			// 
			this.CSI_ActionsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSI_ActionsDropEdit, "SupportingDocuments.CSI_Actions");
			this.CSI_ActionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64);
			this.CSI_ActionsDropEdit.Name = "CSI_ActionsDropEdit";
			this.CSI_ActionsDropEdit.PreBoundMaxLength = 3;
			this.CSI_ActionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20);
			this.CSI_ActionsDropEdit.TabIndex = 2;
			// 
			// CSI_AvailabilityDropEdit
			// 
			this.CSI_AvailabilityDropEdit.AllowDrop = true;
			this.CSI_AvailabilityDropEdit.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_AvailabilityDropEdit, "SupportingDocuments.CSI_Availability");
			this.CSI_AvailabilityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 64);
			this.CSI_AvailabilityDropEdit.Name = "CSI_AvailabilityDropEdit";
			this.CSI_AvailabilityDropEdit.PreBoundMaxLength = 3;
			this.CSI_AvailabilityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20);
			this.CSI_AvailabilityDropEdit.TabIndex = 3;
			// 
			// CSI_SubTypeTextBox
			// 
			this.CSI_SubTypeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_SubTypeTextBox, "SupportingDocuments.CSI_SubType");
			this.CSI_SubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 87);
			this.CSI_SubTypeTextBox.Name = "CSI_SubTypeTextBox";
			this.CSI_SubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20);
			this.CSI_SubTypeTextBox.TabIndex = 4;
			// 
			// CSI_DateOfIssueDateEdit
			// 
			this.CSI_DateOfIssueDateEdit.AllowDrop = true;
			this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "SupportingDocuments.CSI_DateOfIssue");
			this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 110);
			this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
			this.CSI_DateOfIssueDateEdit.TabIndex = 5;
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
			this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "SupportingDocuments.CSI_DateOfExpiry");
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 110);
			this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
			this.CSI_DateOfExpiryDateEdit.TabIndex = 6;
			// 
			// CSI_DescriptionTextBox
			// 
			this.CSI_DescriptionTextBox.AllowDrop = true;
			this.CSI_DescriptionTextBox.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CSI_DescriptionTextBox, "SupportingDocuments.CSI_Description");
			this.CSI_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 133);
			this.CSI_DescriptionTextBox.Name = "CSI_DescriptionTextBox";
			this.CSI_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 20);
			this.CSI_DescriptionTextBox.TabIndex = 7;
			// 
			// CSI_ReferenceNumber2TextBox
			// 
			this.CSI_ReferenceNumber2TextBox.Anchor = (System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CSI_ReferenceNumber2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CSI_ReferenceNumber2TextBox, "SupportingDocuments.CSI_ReferenceNumber2");
			this.CSI_ReferenceNumber2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 156);
			this.CSI_ReferenceNumber2TextBox.Name = "CSI_ReferenceNumber2TextBox";
			this.CSI_ReferenceNumber2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 20);
			this.CSI_ReferenceNumber2TextBox.TabIndex = 8;
			// 
			// SupportingDocumentsGroupBox
			//
			this.SupportingDocumentsGroupBox.CaptionResourceString = Res.GetData("2d3704aa-51e5-416e-a6ce-6d9552f423c3", "Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_CodeCodeFindBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ActionsDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_AvailabilityDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_SubTypeTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DescriptionTextBox);
			this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumber2TextBox);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 230);
			this.SupportingDocumentsGroupBox.TabIndex = 0;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// H7SupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "H7SupportingDocumentsFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 230);
			this.Controls.SetChildIndex(this.SupportingDocumentsGroupBox, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSI_CodeCodeFindBox.ResumeLayout(true);
			this.CSI_CodeCodeFindBox.PerformLayout();
			this.CSI_ReferenceNumberTextBox.ResumeLayout(true);
			this.CSI_ReferenceNumberTextBox.PerformLayout();
			this.CSI_ActionsDropEdit.ResumeLayout(true);
			this.CSI_ActionsDropEdit.PerformLayout();
			this.CSI_AvailabilityDropEdit.ResumeLayout(true);
			this.CSI_AvailabilityDropEdit.PerformLayout();
			this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
			this.CSI_DateOfIssueDateEdit.PerformLayout();
			this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
			this.CSI_DateOfExpiryDateEdit.PerformLayout();
			this.CSI_DescriptionTextBox.ResumeLayout(true);
			this.CSI_DescriptionTextBox.PerformLayout();
			this.CSI_ReferenceNumber2TextBox.ResumeLayout(true);
			this.CSI_ReferenceNumber2TextBox.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_CodeCodeFindBox;
		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit CSI_ActionsDropEdit;
		private ZArchitecture.GUI.ZDropEdit CSI_AvailabilityDropEdit;
		private ZArchitecture.ZTextBox CSI_SubTypeTextBox;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		private ZArchitecture.ZTextBox CSI_DescriptionTextBox;
		private ZArchitecture.ZTextBox CSI_ReferenceNumber2TextBox;
	}
}
