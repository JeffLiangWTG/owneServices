namespace Enterprise.Customs.CO.Manifest.GUI
{
	partial class COBillCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
            this.TravelDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CargoDispositionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MultimodalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CarriersLiabilityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GoodsValueConvertToLocalCurrencyControl = new Customs.GUI.ConvertToLocalCurrencyControl();
			this.BillIssueDateEdit = new ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.TravelDocumentTypeDropEdit.SuspendLayout();
            this.CargoDispositionDropEdit.SuspendLayout();
			this.MultimodalCheckBox.SuspendLayout();
			this.CarriersLiabilityCheckBox.SuspendLayout();
			this.GoodsValueConvertToLocalCurrencyControl.SuspendLayout();
			this.BillIssueDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CO.Manifest.Business.AsycudaBill);
            // 
            // TravelDocumentTypeDropEdit
            // 
            this.TravelDocumentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TravelDocumentTypeDropEdit, "TravelDocumentType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CO.Manifest.Business.AsycudaBill)(null)).TravelDocumentType)));
            this.TravelDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 10, true);
            this.TravelDocumentTypeDropEdit.Name = "TravelDocumentTypeDropEdit";
            this.TravelDocumentTypeDropEdit.PreBoundMaxLength = 1;
            this.TravelDocumentTypeDropEdit.ShouldResizeByMaxLength = true;
            this.TravelDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.TravelDocumentTypeDropEdit.TabIndex = 1;
            // 
            // CargoDispositionDropEdit
            // 
            this.CargoDispositionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CargoDispositionDropEdit, "CargoDisposition");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CO.Manifest.Business.AsycudaBill)(null)).CargoDisposition)));
            this.CargoDispositionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 36, true);
            this.CargoDispositionDropEdit.Name = "CargoDispositionDropEdit";
            this.CargoDispositionDropEdit.PreBoundMaxLength = 2;
            this.CargoDispositionDropEdit.ShouldResizeByMaxLength = true;
            this.CargoDispositionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.CargoDispositionDropEdit.TabIndex = 2;
			// 
			// MultimodalCheckBox
			//
			this.BindingSource.SetBindingMember(this.MultimodalCheckBox, "Multimodal");
			this.MultimodalCheckBox.AutoSize = true;
			this.MultimodalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.MultimodalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MultimodalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.MultimodalCheckBox.Name = "MultimodalCheckBox";
			this.MultimodalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.MultimodalCheckBox.TabIndex = 3;
			this.MultimodalCheckBox.UseVisualStyleBackColor = true;
			// 
			// CarriersLiabilityCheckBox
			//
			this.BindingSource.SetBindingMember(this.CarriersLiabilityCheckBox, "CarriersLiability");
			this.CarriersLiabilityCheckBox.AutoSize = true;
			this.CarriersLiabilityCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.CarriersLiabilityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CarriersLiabilityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 88, true);
			this.CarriersLiabilityCheckBox.Name = "CarriersLiabilityCheckBox";
			this.CarriersLiabilityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CarriersLiabilityCheckBox.TabIndex = 4;
			this.CarriersLiabilityCheckBox.UseVisualStyleBackColor = true;
			// 
			// GoodsValueConvertToLocalCurrencyControl
			// 
			this.GoodsValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.GoodsValueConvertToLocalCurrencyControl.BindToAmount = "ABL_GoodsValue";
			this.GoodsValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKGoodsValueCurrency";
			this.GoodsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(832, 88, true);
			this.GoodsValueConvertToLocalCurrencyControl.Name = "GoodsValueConvertToLocalCurrencyControl";
			this.GoodsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GoodsValueConvertToLocalCurrencyControl.TabIndex = 5;
			// 
			// BillIssueDateEdit
			// 
			this.BillIssueDateEdit.AllowDrop = true;
			this.BillIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BillIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BillIssueDateEdit, "ABL_BillIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CO.Manifest.Business.AsycudaBill)(null)).ABL_BillIssueDate)));
			this.BillIssueDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.BillIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 334, true);
			this.BillIssueDateEdit.Name = "BillIssueDateEdit";
			this.BillIssueDateEdit.TabIndex = 6;
			// 
			// COBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.TravelDocumentTypeDropEdit);
            this.Controls.Add(this.CargoDispositionDropEdit);
			this.Controls.Add(this.MultimodalCheckBox);
			this.Controls.Add(this.CarriersLiabilityCheckBox);
			this.Controls.Add(this.GoodsValueConvertToLocalCurrencyControl);
			this.Controls.Add(this.BillIssueDateEdit);
            this.Name = "COBillCountrySpecificUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 68, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.TravelDocumentTypeDropEdit.ResumeLayout(true);
            this.TravelDocumentTypeDropEdit.PerformLayout();
            this.CargoDispositionDropEdit.ResumeLayout(true);
            this.CargoDispositionDropEdit.PerformLayout();
			this.MultimodalCheckBox.ResumeLayout(true);
			this.MultimodalCheckBox.PerformLayout();
			this.CarriersLiabilityCheckBox.ResumeLayout();
			this.CarriersLiabilityCheckBox.PerformLayout();
			this.GoodsValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.GoodsValueConvertToLocalCurrencyControl.PerformLayout();
			this.BillIssueDateEdit.ResumeLayout(true);
			this.BillIssueDateEdit.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDropEdit TravelDocumentTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CargoDispositionDropEdit;
		internal ZArchitecture.GUI.ZCheckBox MultimodalCheckBox;
		internal ZArchitecture.GUI.ZCheckBox CarriersLiabilityCheckBox;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl GoodsValueConvertToLocalCurrencyControl;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit BillIssueDateEdit;
	}
}
