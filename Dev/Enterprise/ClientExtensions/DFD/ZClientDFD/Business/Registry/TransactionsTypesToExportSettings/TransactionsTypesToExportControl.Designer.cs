using Enterprise.Registry.GUI;

namespace Enterprise.Client.DFD.Registry
{
	internal partial class TransactionsTypesToExportControl : RegistryZUserControl
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox groupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARAdjustmentNote;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARNonJobRelatedCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARJobRelatedCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARCreditNoteCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ARInvoiceCheckBox;

		void InitializeComponent()
		{
			this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ARAdjustmentNote = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARNonJobRelatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARJobRelatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARCreditNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.DFD.Registry.TransactionsTypesToExportBusinessObject);
			// 
			// groupBox
			// 
			this.groupBox.Controls.Add(this.ARAdjustmentNote);
			this.groupBox.Controls.Add(this.ARNonJobRelatedCheckBox);
			this.groupBox.Controls.Add(this.ARJobRelatedCheckBox);
			this.groupBox.Controls.Add(this.ARCreditNoteCheckBox);
			this.groupBox.Controls.Add(this.ARInvoiceCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.groupBox, false);
			this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 151, true);
			this.groupBox.TabIndex = 0;
			this.groupBox.TabStop = false;
			// 
			// ARAdjustmentNote
			// 
			this.BindingSource.SetBindingMember(this.ARAdjustmentNote, "ARAdjustmentNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.DFD.Registry.TransactionsTypesToExportBusinessObject)(null)).ARAdjustmentNote)));
			this.ARAdjustmentNote.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARAdjustmentNote.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 67, true);
			this.ARAdjustmentNote.Name = "ARAdjustmentNote";
			this.ARAdjustmentNote.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
			this.ARAdjustmentNote.TabIndex = 2;
			this.ARAdjustmentNote.Text = "AR Adjustment Note";
			this.ARAdjustmentNote.UseVisualStyleBackColor = true;
			// 
			// ARNonJobRelatedCheckBox
			// 
			this.ARNonJobRelatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ARNonJobRelatedCheckBox, "ARNonJobRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.DFD.Registry.TransactionsTypesToExportBusinessObject)(null)).ARNonJobRelated)));
			this.ARNonJobRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARNonJobRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 113, true);
			this.ARNonJobRelatedCheckBox.Name = "ARNonJobRelatedCheckBox";
			this.ARNonJobRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 17, true);
			this.ARNonJobRelatedCheckBox.TabIndex = 4;
			this.ARNonJobRelatedCheckBox.Text = "AR Non Job Related";
			this.ARNonJobRelatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ARJobRelatedCheckBox
			// 
			this.ARJobRelatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ARJobRelatedCheckBox, "ARJobRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.DFD.Registry.TransactionsTypesToExportBusinessObject)(null)).ARJobRelated)));
			this.ARJobRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARJobRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 90, true);
			this.ARJobRelatedCheckBox.Name = "ARJobRelatedCheckBox";
			this.ARJobRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 17, true);
			this.ARJobRelatedCheckBox.TabIndex = 3;
			this.ARJobRelatedCheckBox.Text = "AR Job Related";
			this.ARJobRelatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ARCreditNoteCheckBox
			// 
			this.ARCreditNoteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ARCreditNoteCheckBox, "ARCreditNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.DFD.Registry.TransactionsTypesToExportBusinessObject)(null)).ARCreditNote)));
			this.ARCreditNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARCreditNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			this.ARCreditNoteCheckBox.Name = "ARCreditNoteCheckBox";
			this.ARCreditNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.ARCreditNoteCheckBox.TabIndex = 1;
			this.ARCreditNoteCheckBox.Text = "AR Credit Note";
			this.ARCreditNoteCheckBox.UseVisualStyleBackColor = true;
			// 
			// ARInvoiceCheckBox
			// 
			this.ARInvoiceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ARInvoiceCheckBox, "ARInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.DFD.Registry.TransactionsTypesToExportBusinessObject)(null)).ARInvoice)));
			this.ARInvoiceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21, true);
			this.ARInvoiceCheckBox.Name = "ARInvoiceCheckBox";
			this.ARInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.ARInvoiceCheckBox.TabIndex = 0;
			this.ARInvoiceCheckBox.Text = "AR Invoice";
			this.ARInvoiceCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransactionsTypesToExportControl
			// 
			this.Controls.Add(this.groupBox);
			this.Name = "TransactionsTypesToExportControl";
			this.ReadOnly = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 161, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
