namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUEdificeImportInvoiceLinesUserControl
	{
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zA_PRFBoundDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.zA_LinePrefix_HiddenBoundDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.InstrumentTypeBoundDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.InstrumentCodeBoundDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.ValuationBasisBoundDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.AdjustmentCurrencyLabel = new ZArchitecture.ZLabel();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// JI_AddInfoBoundAddInfoControl
			// 
			this.JI_AddInfoBoundAddInfoControl.TabIndex = 22;
			// 
			// AdjustmentDollarPercentageBoundDropEdit
			// 
			this.AdjustmentDollarPercentageBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("e49bd7c9-e349-4410-8b4a-56c569a197a6", "Adjust");
			this.AdjustmentDollarPercentageBoundDropEdit.TabIndex = 18;
			// 
			// AdjustmentCurrencyBoundFindBox
			// 
			this.AdjustmentCurrencyBoundFindBox.TabIndex = 20;
			// 
			// TreatmentCodeBoundDropEdit
			// 
			this.TreatmentCodeBoundDropEdit.TabIndex = 7;
			// 
			// AdjustmentAmountBoundCalcEdit
			// 
			this.AdjustmentAmountBoundCalcEdit.TabIndex = 19;
			// 
			// DutyRateCalcEdit
			// 
			this.dutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 165, true);
			this.dutyRateCalcEdit.TabIndex = 14;
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 0, true);
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 320, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 240, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 320, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 320, true);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 116, true);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 40, true);
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.AdjustmentCurrencyLabel);
			this.ClassificationDetailsGroupBox.Controls.Add(this.ValuationBasisBoundDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.InstrumentTypeBoundDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.InstrumentCodeBoundDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zA_LinePrefix_HiddenBoundDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zA_PRFBoundDropEdit);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 168, true);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AdjustmentAmountBoundCalcEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TreatmentCodeBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AdjustmentCurrencyBoundFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AdjustmentDollarPercentageBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AddInfoBoundAddInfoControl, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.tariffFindBoxAUCClass, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.tariffFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zA_PRFBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zA_LinePrefix_HiddenBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.InstrumentCodeBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.InstrumentTypeBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ValuationBasisBoundDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AdjustmentCurrencyLabel, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 293, true);
			// 
			// CurrentInvoicePanel
			// 
			this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 84, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 217, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 293, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 293, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 274, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 293, true);
			// 
			// ClassificationPanel
			// 
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 168, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			zDropEditColumnStyleInfo7.BindToList = "AddInfo+ZA_PRFList";
			zDropEditColumnStyleInfo7.Caption = "Preference";
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "AddInfo+ZA_PRF";
			zDropEditColumnStyleInfo8.BindToList = "JI_LinePrefix_List";
			zDropEditColumnStyleInfo8.Caption = "Line Prefix";
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "JI_LinePrefix";
			zDropEditColumnStyleInfo8.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|d9bb8964-990f-4cc5-bdb4-63b71d6856b8", "Line Prefix");
			zDropEditColumnStyleInfo8.IsVisible = false;
			zDropEditColumnStyleInfo8.ToolTip = "Parent or trailer line";
			zTextBoxColumnStyleInfo4.Caption = "Parent Line";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "JI_ParentLineCode";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|d9bb8964-990f-4cc5-bdb4-63b71d6856b8", "Line Prefix");
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.ToolTip = "Parent Line No / Invoice No";
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 8;
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|9c49754a-69df-439b-b746-2caabc961e88", "Insurance", "Insurance In Invoice Currency", "Insurance for current line item");
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.TabIndex = 6;
			// 
			// JI_Calc_FreightConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FreightConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|df6cbd06-1b7d-4fba-a848-a9045e84277f", "Freight", "Freight In Invoice Currency", "Freight Charges for current line item");
			this.JI_Calc_FreightConvertToLocalCurrencyControl.TabIndex = 4;
			// 
			// JI_Calc_FOBConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FOBConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|cbff40a7-668f-406e-888c-56abbdfbbb6b", "FOB", "FOB value for current line item");
			this.JI_Calc_FOBConvertToLocalCurrencyControl.TabIndex = 2;
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 141, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 12;
			// 
			// JI_Calc_DutyConvertToLocalCurrencyControl
			// 
			this.JI_Calc_DutyConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|cbe9d0e2-2952-498b-8317-0bf8824d157a", "Duty Amount", "Duty value for current line item");
			this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 10;
			// 
			// JI_Calc_LinesTotalConvertToLocalCurrencyControl
			// 
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUEdificeImportInvoiceLinesUserControl|f273fae5-11fa-40bc-80e1-c93b0fa9cdbf", "Lines Total", "The line total");
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 64, true);
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 23, true);
			// 
			// ZA_PRFBoundDropEdit
			// 
			this.zA_PRFBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_PRFBoundDropEdit, "FilteredInvoiceLines.AddInfo+ZA_PRF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_PRF)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_PRFList)));
			this.zA_PRFBoundDropEdit.BindToList = "FilteredInvoiceLines.AddInfo+ZA_PRFList";
			this.zA_PRFBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("98f1966f-156e-4071-9a4e-800adf7c4277", "Preference");
			this.zA_PRFBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 40, true);
			this.zA_PRFBoundDropEdit.Name = "ZA_PRFBoundDropEdit";
			this.zA_PRFBoundDropEdit.PreBoundMaxLength = 1;
			this.zA_PRFBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.zA_PRFBoundDropEdit.TabIndex = 5;
			// 
			// ZA_LinePrefix_HiddenBoundDropEdit
			// 
			this.zA_LinePrefix_HiddenBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zA_LinePrefix_HiddenBoundDropEdit, "FilteredInvoiceLines.JI_LinePrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LinePrefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LinePrefix_List)));
			this.zA_LinePrefix_HiddenBoundDropEdit.BindToList = "FilteredInvoiceLines.JI_LinePrefix_List";
			this.zA_LinePrefix_HiddenBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("b640f3d4-35b0-4c49-af7b-d7b3bbe988da", "Line Prefix");
			this.zA_LinePrefix_HiddenBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 64, true);
			this.zA_LinePrefix_HiddenBoundDropEdit.Name = "ZA_LinePrefix_HiddenBoundDropEdit";
			this.zA_LinePrefix_HiddenBoundDropEdit.PreBoundMaxLength = 1;
			this.zA_LinePrefix_HiddenBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.zA_LinePrefix_HiddenBoundDropEdit.TabIndex = 9;
			// 
			// InstrumentTypeBoundDropEdit
			// 
			this.InstrumentTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InstrumentTypeBoundDropEdit, "FilteredInvoiceLines.InstrumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InstrumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InstrumentTypeList)));
			this.InstrumentTypeBoundDropEdit.BindToList = "FilteredInvoiceLines.InstrumentTypeList";
			this.InstrumentTypeBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("fb1a31da-4d04-4acd-a031-2fd71f834f59", "Instrument Type");
			this.InstrumentTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 88, true);
			this.InstrumentTypeBoundDropEdit.Name = "InstrumentTypeBoundDropEdit";
			this.InstrumentTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.InstrumentTypeBoundDropEdit.ShowDescriptionBox = false;
			this.InstrumentTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.InstrumentTypeBoundDropEdit.TabIndex = 11;
			// 
			// InstrumentCodeBoundDropEdit
			// 
			this.InstrumentCodeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InstrumentCodeBoundDropEdit, "FilteredInvoiceLines.InstrumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InstrumentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InstrumentCodeList)));
			this.InstrumentCodeBoundDropEdit.BindToList = "FilteredInvoiceLines.InstrumentCodeList";
			this.InstrumentCodeBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("29b2336f-5e73-407b-a245-1ae7c9a14694", "Code", "Instrument Code", "");
			this.InstrumentCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 88, true);
			this.InstrumentCodeBoundDropEdit.Name = "InstrumentCodeBoundDropEdit";
			this.InstrumentCodeBoundDropEdit.PreBoundMaxLength = 7;
			this.InstrumentCodeBoundDropEdit.ShowDescriptionBox = false;
			this.InstrumentCodeBoundDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.InstrumentCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.InstrumentCodeBoundDropEdit.TabIndex = 12;
			// 
			// ValuationBasisBoundDropEdit
			// 
			this.ValuationBasisBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationBasisBoundDropEdit, "FilteredInvoiceLines.AddInfo+ZA_ValuationBasis_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_ValuationBasis_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobComInvoiceLine)(((System.Collections.IList)(((Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfo.ZA_ValuationBasis_Hidden_List)));
			this.ValuationBasisBoundDropEdit.BindToList = "FilteredInvoiceLines.AddInfo+ZA_ValuationBasis_Hidden_List";
			this.ValuationBasisBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8fa9b5c3-d02e-4d92-b38c-6704708ef25f", "Valuation Basis");
			this.ValuationBasisBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 112, true);
			this.ValuationBasisBoundDropEdit.Name = "ValuationBasisBoundDropEdit";
			this.ValuationBasisBoundDropEdit.PreBoundMaxLength = 3;
			this.ValuationBasisBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.ValuationBasisBoundDropEdit.TabIndex = 17;
			// 
			// AdjustmentCurrencyLabel
			// 
			this.AdjustmentCurrencyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 88, true);
			this.AdjustmentCurrencyLabel.Name = "AdjustmentCurrencyLabel";
			this.AdjustmentCurrencyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.AdjustmentCurrencyLabel.TabIndex = 15;
			this.AdjustmentCurrencyLabel.Text = "Currency:";
			// 
			// AUEdificeImportInvoiceLinesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "AUEdificeImportInvoiceLinesUserControl";
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		ZArchitecture.GUI.ZDropEdit zA_LinePrefix_HiddenBoundDropEdit;
		public ZArchitecture.GUI.ZDropEdit InstrumentTypeBoundDropEdit;
		public ZArchitecture.GUI.ZDropEdit InstrumentCodeBoundDropEdit;
		public ZArchitecture.GUI.ZDropEdit ValuationBasisBoundDropEdit;
		public ZArchitecture.ZLabel AdjustmentCurrencyLabel;
		ZArchitecture.GUI.ZDropEdit zA_PRFBoundDropEdit;
	}
}
