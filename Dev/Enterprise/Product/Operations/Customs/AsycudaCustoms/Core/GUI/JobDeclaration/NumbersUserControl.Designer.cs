using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class NumbersUserControl
	{
		private void InitializeComponent()
		{
			this.ExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumbersGroupBox.SuspendLayout();
			this.DateEditIssued.SuspendLayout();
			this.DropEditType.SuspendLayout();
			this.ExpiryDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).BeginInit();
			this.NumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.JobDeclaration);
			// 
			// ReferenceNumbersGroupBox
			// 
			this.ReferenceNumbersGroupBox.Controls.Add(this.ExpiryDateEdit);
			this.ReferenceNumbersGroupBox.Controls.Add(this.DateEditIssued);
			this.ReferenceNumbersGroupBox.Controls.Add(this.TextBoxInfo);
			this.ReferenceNumbersGroupBox.Controls.Add(this.TextBoxNumber);
			this.ReferenceNumbersGroupBox.Controls.Add(this.DropEditType);
			this.ReferenceNumbersGroupBox.Controls.Add(this.NumbersGrid);
			this.ReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			// 
			// NumbersGrid
			//
			this.NumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 200, true);
			// 
			// DropEditType
			//
			this.DropEditType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 219, true);
			// 
			// TextBoxNumber
			//
			this.TextBoxNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 240, true);
			// 
			// TextBoxInfo
			//
			this.TextBoxInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 262, true);
			// 
			// DateEditIssued
			//
			this.DateEditIssued.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 284, true);
			// 
			// ExpiryDateEdit
			// 
			this.ExpiryDateEdit.AllowDrop = true;
			this.ExpiryDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExpiryDateEdit.AutoCompleteYear = true;
			this.ExpiryDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
			this.ExpiryDateEdit.CaptionResourceString =
				Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("287B049E-A579-4F94-8ED8-452733F21201", "Expiry Date");
			this.BindingSource.SetBindingMember(this.ExpiryDateEdit, "AdditionalReferenceNumbers.CE_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.JobDeclaration)(null)).AdditionalReferenceNumbers)).SyncRoot)).CE_ExpiryDate)));
			this.ExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 284, true);
			this.ExpiryDateEdit.Name = "ExpiryDateEdit";
			this.ExpiryDateEdit.TabIndex = 5;
			// 
			// NumbersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumbersGroupBox);
			this.Name = "NumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumbersGroupBox.ResumeLayout(false);
			this.ReferenceNumbersGroupBox.PerformLayout();
			this.DateEditIssued.ResumeLayout(true);
			this.DateEditIssued.PerformLayout();
			this.DropEditType.ResumeLayout(true);
			this.DropEditType.PerformLayout();
			this.ExpiryDateEdit.ResumeLayout(true);
			this.ExpiryDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).EndInit();
			this.NumbersGrid.ResumeLayout(false);
			this.NumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZDateEdit ExpiryDateEdit;
	}
}
