namespace Enterprise.Customs.KR.GUI
{
	partial class CarnetDetailsUserControl
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
            this.HouseBillSplitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CarnetUseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CarnetCertificateNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RepresentativeProductNameLongTextControl = new Customs.GUI.LongTextControl();
            this.CargoManagementNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.HouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TotalWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.EffectiveToDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.TotalQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.NoPackagesCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.TotalAmountConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.HouseBillSplitDropEdit.SuspendLayout();
            this.CarnetUseDropEdit.SuspendLayout();
            this.TotalWeightCalcDropEdit.SuspendLayout();
            this.EffectiveToDateDateEdit.SuspendLayout();
            this.NoPackagesCalcDropEdit.SuspendLayout();
            this.TotalAmountConvertToLocalCurrencyControl.SuspendLayout();
            this.SuspendLayout();
            //
            // BindingSource
            //
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            //
            // HouseBillSplitDropEdit
            //
            this.HouseBillSplitDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.HouseBillSplitDropEdit, "D87HBSplitDecInd");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).D87HBSplitDecInd)));
            this.HouseBillSplitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 109, true);
            this.HouseBillSplitDropEdit.Name = "HouseBillSplitDropEdit";
            this.HouseBillSplitDropEdit.ShowDescriptionBox = false;
            this.HouseBillSplitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.HouseBillSplitDropEdit.TabIndex = 6;
            //
            // CarnetUseDropEdit
            //
            this.CarnetUseDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CarnetUseDropEdit, "JE_ExportGoodsType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ExportGoodsType)));
            this.CarnetUseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 32, true);
            this.CarnetUseDropEdit.Name = "CarnetUseDropEdit";
            this.CarnetUseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
            this.CarnetUseDropEdit.TabIndex = 2;
            //
            // CarnetCertificateNoTextBox
            //
            this.BindingSource.SetBindingMember(this.CarnetCertificateNoTextBox, "JE_AgentsReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AgentsReference)));
            this.CarnetCertificateNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 5, true);
            this.CarnetCertificateNoTextBox.Name = "CarnetCertificateNoTextBox";
            this.CarnetCertificateNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
            this.CarnetCertificateNoTextBox.TabIndex = 0;
						//
						// RepresentativeProductNameLongTextControl
						//
						this.BindingSource.SetBindingMember(this.RepresentativeProductNameLongTextControl, "DetailedGoodsDescriptionRemarks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).DetailedGoodsDescriptionRemarks)));
            this.RepresentativeProductNameLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 57, true);
            this.RepresentativeProductNameLongTextControl.Name = "RepresentativeProductNameLongTextControl";
            this.RepresentativeProductNameLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 17, true);
            this.RepresentativeProductNameLongTextControl.TabIndex = 3;
            //
            // CargoManagementNoTextBox
            //
            this.BindingSource.SetBindingMember(this.CargoManagementNoTextBox, "Invoices.JZ_ImportCargoManagementNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ImportCargoManagementNumber)));
            this.CargoManagementNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 83, true);
            this.CargoManagementNoTextBox.Name = "CargoManagementNoTextBox";
            this.CargoManagementNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
            this.CargoManagementNoTextBox.TabIndex = 4;
            //
            // HouseBillTextBox
            //
            this.BindingSource.SetBindingMember(this.HouseBillTextBox, "JE_HouseBill");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_HouseBill)));
            this.HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 109, true);
            this.HouseBillTextBox.Name = "HouseBillTextBox";
            this.HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
            this.HouseBillTextBox.TabIndex = 5;
            //
            // TotalWeightCalcDropEdit
            //
            this.TotalWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TotalWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalWeightUnit)));
            this.TotalWeightCalcDropEdit.BindToAmount = "JE_TotalWeight";
            this.TotalWeightCalcDropEdit.BindToUnit = "JE_TotalWeightUnit";
            this.TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 133, true);
            this.TotalWeightCalcDropEdit.Name = "TotalWeightCalcDropEdit";
            this.TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
            this.TotalWeightCalcDropEdit.TabIndex = 7;
            //
            // EffectiveToDateDateEdit
            //
            this.EffectiveToDateDateEdit.AllowDrop = true;
            this.EffectiveToDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EffectiveToDateDateEdit, "JE_EntryDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_EntryDate)));
            this.EffectiveToDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 5, true);
            this.EffectiveToDateDateEdit.Name = "EffectiveToDateDateEdit";
            this.EffectiveToDateDateEdit.TabIndex = 1;
            //
            // TotalQtyCalcEdit
            //
            this.BindingSource.SetBindingMember(this.TotalQtyCalcEdit, "JE_TotalNoOfPieces");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalNoOfPieces)));
            this.TotalQtyCalcEdit.DecimalPlaces = 2;
            this.TotalQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 133, true);
            this.TotalQtyCalcEdit.Name = "TotalQtyCalcEdit";
            this.TotalQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
            this.TotalQtyCalcEdit.TabIndex = 8;
            this.TotalQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalQtyCalcEdit.TrackDisposedAccess = true;
            //
            // NoPackagesCalcDropEdit
            //
            this.NoPackagesCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.NoPackagesCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalNoOfPacks)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalNoOfPacksPackType)));
            this.NoPackagesCalcDropEdit.BindToAmount = "JE_TotalNoOfPacks";
            this.NoPackagesCalcDropEdit.BindToUnit = "JE_TotalNoOfPacksPackType";
            this.NoPackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 159, true);
            this.NoPackagesCalcDropEdit.Name = "NoPackagesCalcDropEdit";
            this.NoPackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 17, true);
            this.NoPackagesCalcDropEdit.TabIndex = 9;
            //
            // TotalAmountConvertToLocalCurrencyControl
            //
            this.TotalAmountConvertToLocalCurrencyControl.AllowDrop = true;
            this.TotalAmountConvertToLocalCurrencyControl.BindToAmount = "D87InvoiceAmount";
            this.TotalAmountConvertToLocalCurrencyControl.BindToUnit = "D87InvoiceCurrency";
            this.TotalAmountConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.TotalAmountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 159, true);
            this.TotalAmountConvertToLocalCurrencyControl.Name = "TotalAmountConvertToLocalCurrencyControl";
            this.TotalAmountConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
            this.TotalAmountConvertToLocalCurrencyControl.TabIndex = 10;
            //
            // CarnetDetailsUserControl
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.TotalAmountConvertToLocalCurrencyControl);
            this.Controls.Add(this.NoPackagesCalcDropEdit);
            this.Controls.Add(this.TotalQtyCalcEdit);
            this.Controls.Add(this.EffectiveToDateDateEdit);
            this.Controls.Add(this.TotalWeightCalcDropEdit);
            this.Controls.Add(this.HouseBillTextBox);
            this.Controls.Add(this.CargoManagementNoTextBox);
            this.Controls.Add(this.RepresentativeProductNameLongTextControl);
            this.Controls.Add(this.CarnetCertificateNoTextBox);
            this.Controls.Add(this.CarnetUseDropEdit);
            this.Controls.Add(this.HouseBillSplitDropEdit);
            this.Name = "CarnetDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 190, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.HouseBillSplitDropEdit.ResumeLayout(true);
            this.HouseBillSplitDropEdit.PerformLayout();
            this.CarnetUseDropEdit.ResumeLayout(true);
            this.CarnetUseDropEdit.PerformLayout();
            this.TotalWeightCalcDropEdit.ResumeLayout(true);
            this.TotalWeightCalcDropEdit.PerformLayout();
            this.EffectiveToDateDateEdit.ResumeLayout(true);
            this.EffectiveToDateDateEdit.PerformLayout();
            this.NoPackagesCalcDropEdit.ResumeLayout(true);
            this.NoPackagesCalcDropEdit.PerformLayout();
            this.TotalAmountConvertToLocalCurrencyControl.ResumeLayout(true);
            this.TotalAmountConvertToLocalCurrencyControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit HouseBillSplitDropEdit;
		private ZArchitecture.GUI.ZDropEdit CarnetUseDropEdit;
		private ZArchitecture.ZTextBox CarnetCertificateNoTextBox;
		private Customs.GUI.LongTextControl RepresentativeProductNameLongTextControl;
		private ZArchitecture.ZTextBox CargoManagementNoTextBox;
		private ZArchitecture.ZTextBox HouseBillTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit TotalWeightCalcDropEdit;
		private ZArchitecture.GUI.ZDateEdit EffectiveToDateDateEdit;
		private ZArchitecture.ZCalcEdit TotalQtyCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit NoPackagesCalcDropEdit;
		private Customs.GUI.ConvertToLocalCurrencyControl TotalAmountConvertToLocalCurrencyControl;
	}
}
