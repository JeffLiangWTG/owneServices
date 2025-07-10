namespace Enterprise.Customs.KR.GUI
{
	partial class MethodTwoToSixUserControl
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
      this.ExpectedCustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
      this.SupportingDocument1TextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.SupportingDocument2TextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.SampleItemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.GiftOrFreeDonationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.AdvertisingUseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.ForProductionAndManufactureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.UseOfDefectiveRepairCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.ReplacementItemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.PerformancePriceOfPaidTransactionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.InvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.PriceListCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.ManufacturingCostCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.ItemUseCodeOtherReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.GoodsPricingBasisOtherReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
      // 
      // ExpectedCustomsValueCalcEdit
      // 
      this.BindingSource.SetBindingMember(this.ExpectedCustomsValueCalcEdit, "Invoices.ExpectedCustomsValue");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ExpectedCustomsValue)));
      this.ExpectedCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 14, true);
      this.ExpectedCustomsValueCalcEdit.Name = "ExpectedCustomsValueCalcEdit";
      this.ExpectedCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 17, true);
      this.ExpectedCustomsValueCalcEdit.TabIndex = 0;
      this.ExpectedCustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.ExpectedCustomsValueCalcEdit.TrackDisposedAccess = true;
      // 
      // SupportingDocument1TextBox
      // 
      this.BindingSource.SetBindingMember(this.SupportingDocument1TextBox, "Invoices.ValuationSupportingDocument1");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationSupportingDocument1)));
      this.SupportingDocument1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
      this.SupportingDocument1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 40, true);
      this.SupportingDocument1TextBox.Name = "SupportingDocument1TextBox";
      this.SupportingDocument1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 17, true);
      this.SupportingDocument1TextBox.TabIndex = 1;
      // 
      // SupportingDocument2TextBox
      // 
      this.BindingSource.SetBindingMember(this.SupportingDocument2TextBox, "Invoices.ValuationSupportingDocument2");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationSupportingDocument2)));
      this.SupportingDocument2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
      this.SupportingDocument2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 66, true);
      this.SupportingDocument2TextBox.Name = "SupportingDocument2TextBox";
      this.SupportingDocument2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 17, true);
      this.SupportingDocument2TextBox.TabIndex = 2;
      // 
      // SampleItemCheckBox
      // 
      this.BindingSource.SetBindingMember(this.SampleItemCheckBox, "Invoices.ValuationDeclarationCode301");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode301)));
      this.SampleItemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.SampleItemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 103, true);
      this.SampleItemCheckBox.Name = "SampleItemCheckBox";
      this.SampleItemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 19, true);
      this.SampleItemCheckBox.TabIndex = 3;
      this.SampleItemCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.SampleItemCheckBox.UseVisualStyleBackColor = true;
      // 
      // GiftOrFreeDonationCheckBox
      // 
      this.BindingSource.SetBindingMember(this.GiftOrFreeDonationCheckBox, "Invoices.ValuationDeclarationCode305");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode305)));
      this.GiftOrFreeDonationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.GiftOrFreeDonationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 103, true);
      this.GiftOrFreeDonationCheckBox.Name = "GiftOrFreeDonationCheckBox";
      this.GiftOrFreeDonationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 19, true);
      this.GiftOrFreeDonationCheckBox.TabIndex = 7;
      this.GiftOrFreeDonationCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.GiftOrFreeDonationCheckBox.UseVisualStyleBackColor = true;
      // 
      // AdvertisingUseCheckBox
      // 
      this.BindingSource.SetBindingMember(this.AdvertisingUseCheckBox, "Invoices.ValuationDeclarationCode302");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode302)));
      this.AdvertisingUseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.AdvertisingUseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 128, true);
      this.AdvertisingUseCheckBox.Name = "AdvertisingUseCheckBox";
      this.AdvertisingUseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 19, true);
      this.AdvertisingUseCheckBox.TabIndex = 4;
      this.AdvertisingUseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.AdvertisingUseCheckBox.UseVisualStyleBackColor = true;
      // 
      // ForProductionAndManufactureCheckBox
      // 
      this.BindingSource.SetBindingMember(this.ForProductionAndManufactureCheckBox, "Invoices.ValuationDeclarationCode306");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode306)));
      this.ForProductionAndManufactureCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.ForProductionAndManufactureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 130, true);
      this.ForProductionAndManufactureCheckBox.Name = "ForProductionAndManufactureCheckBox";
      this.ForProductionAndManufactureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 19, true);
      this.ForProductionAndManufactureCheckBox.TabIndex = 8;
      this.ForProductionAndManufactureCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.ForProductionAndManufactureCheckBox.UseVisualStyleBackColor = true;
      // 
      // UseOfDefectiveRepairCheckBox
      // 
      this.BindingSource.SetBindingMember(this.UseOfDefectiveRepairCheckBox, "Invoices.ValuationDeclarationCode303");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode303)));
      this.UseOfDefectiveRepairCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.UseOfDefectiveRepairCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 153, true);
      this.UseOfDefectiveRepairCheckBox.Name = "UseOfDefectiveRepairCheckBox";
      this.UseOfDefectiveRepairCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 19, true);
      this.UseOfDefectiveRepairCheckBox.TabIndex = 5;
      this.UseOfDefectiveRepairCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.UseOfDefectiveRepairCheckBox.UseVisualStyleBackColor = true;
      // 
      // ReplacementItemCheckBox
      // 
      this.BindingSource.SetBindingMember(this.ReplacementItemCheckBox, "Invoices.ValuationDeclarationCode304");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode304)));
      this.ReplacementItemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.ReplacementItemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 178, true);
      this.ReplacementItemCheckBox.Name = "ReplacementItemCheckBox";
      this.ReplacementItemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 19, true);
      this.ReplacementItemCheckBox.TabIndex = 6;
      this.ReplacementItemCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.ReplacementItemCheckBox.UseVisualStyleBackColor = true;
      // 
      // PerformancePriceOfPaidTransactionCheckBox
      // 
      this.BindingSource.SetBindingMember(this.PerformancePriceOfPaidTransactionCheckBox, "Invoices.ValuationDeclarationCode401");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode401)));
      this.PerformancePriceOfPaidTransactionCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.PerformancePriceOfPaidTransactionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 227, true);
      this.PerformancePriceOfPaidTransactionCheckBox.Name = "PerformancePriceOfPaidTransactionCheckBox";
      this.PerformancePriceOfPaidTransactionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 19, true);
      this.PerformancePriceOfPaidTransactionCheckBox.TabIndex = 10;
      this.PerformancePriceOfPaidTransactionCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.PerformancePriceOfPaidTransactionCheckBox.UseVisualStyleBackColor = true;
      // 
      // InvoiceCheckBox
      // 
      this.BindingSource.SetBindingMember(this.InvoiceCheckBox, "Invoices.ValuationDeclarationCode404");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode404)));
      this.InvoiceCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.InvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 227, true);
      this.InvoiceCheckBox.Name = "InvoiceCheckBox";
      this.InvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 19, true);
      this.InvoiceCheckBox.TabIndex = 13;
      this.InvoiceCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.InvoiceCheckBox.UseVisualStyleBackColor = true;
      // 
      // PriceListCheckBox
      // 
      this.BindingSource.SetBindingMember(this.PriceListCheckBox, "Invoices.ValuationDeclarationCode402");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode402)));
      this.PriceListCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.PriceListCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 252, true);
      this.PriceListCheckBox.Name = "PriceListCheckBox";
      this.PriceListCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 19, true);
      this.PriceListCheckBox.TabIndex = 11;
      this.PriceListCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.PriceListCheckBox.UseVisualStyleBackColor = true;
      // 
      // ManufacturingCostCheckBox
      // 
      this.BindingSource.SetBindingMember(this.ManufacturingCostCheckBox, "Invoices.ValuationDeclarationCode403");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode403)));
      this.ManufacturingCostCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.ManufacturingCostCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 277, true);
      this.ManufacturingCostCheckBox.Name = "ManufacturingCostCheckBox";
      this.ManufacturingCostCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 19, true);
      this.ManufacturingCostCheckBox.TabIndex = 12;
      this.ManufacturingCostCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.ManufacturingCostCheckBox.UseVisualStyleBackColor = true;
      // 
      // ItemUseCodeOtherReasonTextBox
      // 
      this.BindingSource.SetBindingMember(this.ItemUseCodeOtherReasonTextBox, "Invoices.ValuationDeclarationCode307");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode307)));
      this.ItemUseCodeOtherReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
      this.ItemUseCodeOtherReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 154, true);
      this.ItemUseCodeOtherReasonTextBox.Name = "ItemUseCodeOtherReasonTextBox";
      this.ItemUseCodeOtherReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
      this.ItemUseCodeOtherReasonTextBox.TabIndex = 9;
      // 
      // GoodsPricingBasisOtherReasonTextBox
      // 
      this.BindingSource.SetBindingMember(this.GoodsPricingBasisOtherReasonTextBox, "Invoices.ValuationDeclarationCode405");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ValuationDeclarationCode405)));
      this.GoodsPricingBasisOtherReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
      this.GoodsPricingBasisOtherReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 252, true);
      this.GoodsPricingBasisOtherReasonTextBox.Name = "GoodsPricingBasisOtherReasonTextBox";
      this.GoodsPricingBasisOtherReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
      this.GoodsPricingBasisOtherReasonTextBox.TabIndex = 14;
      // 
      // MethodTwoToSixUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.GoodsPricingBasisOtherReasonTextBox);
      this.Controls.Add(this.ItemUseCodeOtherReasonTextBox);
      this.Controls.Add(this.ManufacturingCostCheckBox);
      this.Controls.Add(this.PriceListCheckBox);
      this.Controls.Add(this.InvoiceCheckBox);
      this.Controls.Add(this.PerformancePriceOfPaidTransactionCheckBox);
      this.Controls.Add(this.ReplacementItemCheckBox);
      this.Controls.Add(this.UseOfDefectiveRepairCheckBox);
      this.Controls.Add(this.ForProductionAndManufactureCheckBox);
      this.Controls.Add(this.AdvertisingUseCheckBox);
      this.Controls.Add(this.GiftOrFreeDonationCheckBox);
      this.Controls.Add(this.SampleItemCheckBox);
      this.Controls.Add(this.SupportingDocument2TextBox);
      this.Controls.Add(this.SupportingDocument1TextBox);
      this.Controls.Add(this.ExpectedCustomsValueCalcEdit);
      this.Name = "MethodTwoToSixUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 389, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZCalcEdit ExpectedCustomsValueCalcEdit;
		public ZArchitecture.ZTextBox SupportingDocument1TextBox;
		public ZArchitecture.ZTextBox SupportingDocument2TextBox;
		public ZArchitecture.GUI.ZCheckBox SampleItemCheckBox;
		public ZArchitecture.GUI.ZCheckBox GiftOrFreeDonationCheckBox;
		public ZArchitecture.GUI.ZCheckBox AdvertisingUseCheckBox;
		public ZArchitecture.GUI.ZCheckBox ForProductionAndManufactureCheckBox;
		public ZArchitecture.GUI.ZCheckBox UseOfDefectiveRepairCheckBox;
		public ZArchitecture.GUI.ZCheckBox ReplacementItemCheckBox;
		public ZArchitecture.GUI.ZCheckBox PerformancePriceOfPaidTransactionCheckBox;
		public ZArchitecture.GUI.ZCheckBox InvoiceCheckBox;
		public ZArchitecture.GUI.ZCheckBox PriceListCheckBox;
		public ZArchitecture.GUI.ZCheckBox ManufacturingCostCheckBox;
		public ZArchitecture.ZTextBox ItemUseCodeOtherReasonTextBox;
		public ZArchitecture.ZTextBox GoodsPricingBasisOtherReasonTextBox;
	}
}
